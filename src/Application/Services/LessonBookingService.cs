using AutoEscola.Api.Application.DTOs;
using AutoEscola.Api.Domain.Entities;
using AutoEscola.Api.Domain.Enums;
using AutoEscola.Api.Domain.Exceptions;
using AutoEscola.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoEscola.Api.Application.Services;

public class LessonBookingService
{
    private readonly AppDbContext _context;
    private const int LessonDurationMinutes = 50;
    private const int MaxDailyLessonsPerStudent = 3;

    public LessonBookingService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<LessonResponseDto> ScheduleLessonAsync(CreateLessonRequestDto dto)
    {
        var startTime = dto.StartTime;
        var endTime = startTime.AddMinutes(LessonDurationMinutes);

        // 1. Operating Hours & Calendar Rules
        if (startTime.DayOfWeek == DayOfWeek.Sunday)
            throw new DomainException("A Auto Escola não realiza aulas práticas aos domingos.");

        if (startTime.Hour < 7 || startTime.Hour >= 21)
            throw new DomainException("Horário fora de expediente. As aulas ocorrem das 07:00 às 21:00.");

        if (startTime < DateTime.Now.AddHours(1))
            throw new DomainException("O agendamento deve ser feito com no mínimo 1 hora de antecedência.");

        // 2. Load & Validate Entities
        var student = await _context.Students
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == dto.StudentId)
            ?? throw new DomainException("Aluno não encontrado.", 404);

        if (!student.MedicalExamApproved || !student.TheoreticalCourseCompleted)
            throw new DomainException("O aluno precisa concluir o Exame Médico e o Curso Teórico antes de agendar aulas práticas.");

        var instructor = await _context.Instructors
            .Include(i => i.User)
            .FirstOrDefaultAsync(i => i.Id == dto.InstructorId)
            ?? throw new DomainException("Instrutor não encontrado.", 404);

        var vehicle = await _context.Vehicles
            .FirstOrDefaultAsync(v => v.Id == dto.VehicleId)
            ?? throw new DomainException("Veículo não encontrado.", 404);

        if (vehicle.Status != VehicleStatus.ACTIVE)
            throw new DomainException($"O veículo {vehicle.Model} ({vehicle.Plate}) está {vehicle.Status} e indisponível para aulas.");

        // 3. Category Compatibility Validation
        bool categoryCompatible = student.DesiredCategory == vehicle.Category ||
                                  (student.DesiredCategory == CnhCategory.AB && (vehicle.Category == CnhCategory.A || vehicle.Category == CnhCategory.B));

        if (!categoryCompatible)
            throw new DomainException($"Incompatibilidade de categoria: Aluno matriculado na categoria {student.DesiredCategory}, mas veículo é categoria {vehicle.Category}.");

        bool instructorCompatible = instructor.TeachingCategory == vehicle.Category ||
                                    instructor.TeachingCategory == CnhCategory.AB;

        if (!instructorCompatible)
            throw new DomainException($"O instrutor {instructor.User.Name} não leciona na categoria {vehicle.Category}.");

        // 4. DETRAN Daily Limit per Student
        var studentLessonsOnDay = await _context.Lessons.CountAsync(l =>
            l.StudentId == dto.StudentId &&
            l.StartTime.Date == startTime.Date &&
            l.Status == LessonStatus.SCHEDULED);

        if (studentLessonsOnDay >= MaxDailyLessonsPerStudent)
            throw new DomainException($"Limite diário atingido: O DETRAN permite no máximo {MaxDailyLessonsPerStudent} aulas práticas por dia.");

        // 5. Anti-Double-Booking (Time Overlap Check for Student, Instructor, and Vehicle)
        var conflictingLesson = await _context.Lessons
            .Where(l => l.Status == LessonStatus.SCHEDULED &&
                        l.StartTime < endTime &&
                        l.EndTime > startTime &&
                        (l.StudentId == dto.StudentId || l.InstructorId == dto.InstructorId || l.VehicleId == dto.VehicleId))
            .FirstOrDefaultAsync();

        if (conflictingLesson != null)
        {
            if (conflictingLesson.StudentId == dto.StudentId)
                throw new DomainException("Você já possui uma aula agendada neste mesmo horário.", 409);
            if (conflictingLesson.InstructorId == dto.InstructorId)
                throw new DomainException("Este instrutor já está ocupado neste horário.", 409);
            if (conflictingLesson.VehicleId == dto.VehicleId)
                throw new DomainException("Este veículo já está reservado para outra aula neste horário.", 409);
        }

        var lesson = new Lesson
        {
            StudentId = student.Id,
            InstructorId = instructor.Id,
            VehicleId = vehicle.Id,
            StartTime = startTime,
            EndTime = endTime,
            Status = LessonStatus.SCHEDULED
        };

        _context.Lessons.Add(lesson);
        await _context.SaveChangesAsync();

        return MapToResponseDto(lesson, student.User.Name, instructor.User.Name, vehicle.Model, vehicle.Plate);
    }

    public async Task<List<AvailableSlotDto>> GetAvailableSlotsForDateAsync(DateTime date, CnhCategory category)
    {
        var targetDate = date.Date;
        if (targetDate.DayOfWeek == DayOfWeek.Sunday)
            return new List<AvailableSlotDto>();

        var instructors = await _context.Instructors
            .Include(i => i.User)
            .Where(i => i.User.IsActive && (i.TeachingCategory == category || i.TeachingCategory == CnhCategory.AB))
            .ToListAsync();

        var vehicles = await _context.Vehicles
            .Where(v => v.Status == VehicleStatus.ACTIVE && v.Category == category)
            .ToListAsync();

        var lessonsOnDate = await _context.Lessons
            .Where(l => l.StartTime.Date == targetDate && l.Status == LessonStatus.SCHEDULED)
            .ToListAsync();

        // Standard slots: 08:00, 09:00, 10:00, 11:00, 14:00, 15:00, 16:00, 17:00, 18:00
        int[] hours = { 8, 9, 10, 11, 14, 15, 16, 17, 18 };
        var result = new List<AvailableSlotDto>();

        foreach (var hour in hours)
        {
            var slotStart = targetDate.AddHours(hour);
            var slotEnd = slotStart.AddMinutes(LessonDurationMinutes);

            var busyInstructorIds = lessonsOnDate
                .Where(l => l.StartTime < slotEnd && l.EndTime > slotStart)
                .Select(l => l.InstructorId)
                .ToHashSet();

            var busyVehicleIds = lessonsOnDate
                .Where(l => l.StartTime < slotEnd && l.EndTime > slotStart)
                .Select(l => l.VehicleId)
                .ToHashSet();

            var freeInstructors = instructors.Where(i => !busyInstructorIds.Contains(i.Id)).ToList();
            var freeVehicles = vehicles.Where(v => !busyVehicleIds.Contains(v.Id)).ToList();

            var combinations = new List<AvailableInstructorVehicleDto>();
            if (freeInstructors.Any() && freeVehicles.Any())
            {
                var defaultVehicle = freeVehicles.First();
                foreach (var inst in freeInstructors)
                {
                    combinations.Add(new AvailableInstructorVehicleDto(
                        inst.Id,
                        inst.User.Name,
                        inst.Rating,
                        defaultVehicle.Id,
                        defaultVehicle.Model,
                        defaultVehicle.Transmission
                    ));
                }
            }

            result.Add(new AvailableSlotDto(
                TimeSlot: $"{hour:D2}:00",
                StartTime: slotStart,
                IsAvailable: combinations.Any() && slotStart > DateTime.Now.AddHours(1),
                AvailableOptions: combinations
            ));
        }

        return result;
    }

    public async Task<LessonResponseDto> CompleteLessonAsync(Guid lessonId, CompleteLessonRequestDto dto)
    {
        var lesson = await _context.Lessons
            .Include(l => l.Student).ThenInclude(s => s.User)
            .Include(l => l.Instructor).ThenInclude(i => i.User)
            .Include(l => l.Vehicle)
            .FirstOrDefaultAsync(l => l.Id == lessonId)
            ?? throw new DomainException("Aula não encontrada.", 404);

        if (lesson.Status != LessonStatus.SCHEDULED)
            throw new DomainException($"Apenas aulas com status SCHEDULED podem ser concluídas. Status atual: {lesson.Status}.");

        lesson.Status = LessonStatus.COMPLETED;
        lesson.PerformanceScore = Math.Clamp(dto.PerformanceScore, 1, 5);
        lesson.InstructorNotes = dto.InstructorNotes;

        lesson.Student.CompletedPracticalLessons += 1;

        await _context.SaveChangesAsync();

        return MapToResponseDto(lesson, lesson.Student.User.Name, lesson.Instructor.User.Name, lesson.Vehicle.Model, lesson.Vehicle.Plate);
    }

    public async Task<LessonResponseDto> CancelLessonAsync(Guid lessonId, CancelLessonRequestDto dto, bool canceledBySchool)
    {
        var lesson = await _context.Lessons
            .Include(l => l.Student).ThenInclude(s => s.User)
            .Include(l => l.Instructor).ThenInclude(i => i.User)
            .Include(l => l.Vehicle)
            .FirstOrDefaultAsync(l => l.Id == lessonId)
            ?? throw new DomainException("Aula não encontrada.", 404);

        if (lesson.Status != LessonStatus.SCHEDULED)
            throw new DomainException("Esta aula já foi finalizada ou cancelada anteriormente.");

        if (!canceledBySchool && lesson.StartTime < DateTime.Now.AddHours(24))
        {
            throw new DomainException("Cancelamento pelo aluno exige no mínimo 24 horas de antecedência. Entre em contato com a secretaria.");
        }

        lesson.Status = canceledBySchool ? LessonStatus.CANCELED_BY_SCHOOL : LessonStatus.CANCELED_BY_STUDENT;
        lesson.CancellationReason = dto.Reason;

        await _context.SaveChangesAsync();

        return MapToResponseDto(lesson, lesson.Student.User.Name, lesson.Instructor.User.Name, lesson.Vehicle.Model, lesson.Vehicle.Plate);
    }

    public static LessonResponseDto MapToResponseDto(Lesson l, string studentName, string instructorName, string vehicleModel, string vehiclePlate)
    {
        return new LessonResponseDto(
            l.Id,
            l.StartTime,
            l.EndTime,
            l.Status.ToString(),
            l.StudentId,
            studentName,
            l.InstructorId,
            instructorName,
            l.VehicleId,
            vehicleModel,
            vehiclePlate,
            l.PerformanceScore,
            l.InstructorNotes
        );
    }
}
