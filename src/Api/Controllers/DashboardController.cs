using AutoEscola.Api.Application.DTOs;
using AutoEscola.Api.Application.Services;
using AutoEscola.Api.Domain.Enums;
using AutoEscola.Api.Domain.Exceptions;
using AutoEscola.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoEscola.Api.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _context;

    public DashboardController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retorna a Jornada do Aluno (Tracker da CNH, progresso de aulas práticas e próxima aula).
    /// </summary>
    [HttpGet("student/{studentId:guid}")]
    public async Task<ActionResult<StudentDashboardDto>> GetStudentDashboard(Guid studentId)
    {
        var student = await _context.Students
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == studentId)
            ?? throw new DomainException("Aluno não encontrado.", 404);

        var nextLesson = await _context.Lessons
            .Include(l => l.Student).ThenInclude(s => s.User)
            .Include(l => l.Instructor).ThenInclude(i => i.User)
            .Include(l => l.Vehicle)
            .Where(l => l.StudentId == studentId && l.Status == LessonStatus.SCHEDULED && l.StartTime >= DateTime.Now)
            .OrderBy(l => l.StartTime)
            .FirstOrDefaultAsync();

        double progress = student.RequiredPracticalLessons > 0
            ? Math.Min(100.0, Math.Round((double)student.CompletedPracticalLessons / student.RequiredPracticalLessons * 100, 1))
            : 0;

        bool readyForExam = student.MedicalExamApproved &&
                            student.TheoreticalCourseCompleted &&
                            student.CompletedPracticalLessons >= student.RequiredPracticalLessons;

        return Ok(new StudentDashboardDto(
            StudentId: student.Id,
            Name: student.User.Name,
            DesiredCategory: student.DesiredCategory.ToString(),
            MedicalExamApproved: student.MedicalExamApproved,
            TheoreticalCourseCompleted: student.TheoreticalCourseCompleted,
            CompletedPracticalLessons: student.CompletedPracticalLessons,
            RequiredPracticalLessons: student.RequiredPracticalLessons,
            ProgressPercentage: progress,
            ReadyForPracticalExam: readyForExam,
            NextUpcomingLesson: nextLesson == null ? null : LessonBookingService.MapToResponseDto(
                nextLesson,
                nextLesson.Student.User.Name,
                nextLesson.Instructor.User.Name,
                nextLesson.Vehicle.Model,
                nextLesson.Vehicle.Plate)
        ));
    }

    /// <summary>
    /// Retorna os KPIs operacionais para a Secretaria / Administrador.
    /// </summary>
    [HttpGet("admin")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<AdminDashboardKpiDto>> GetAdminKpis()
    {
        var today = DateTime.Today;

        var totalStudents = await _context.Students.CountAsync();
        var activeInstructors = await _context.Instructors.CountAsync(i => i.User.IsActive);
        var activeVehicles = await _context.Vehicles.CountAsync(v => v.Status == VehicleStatus.ACTIVE);
        var maintenanceVehicles = await _context.Vehicles.CountAsync(v => v.Status == VehicleStatus.MAINTENANCE);

        var scheduledToday = await _context.Lessons.CountAsync(l => l.StartTime.Date == today && l.Status == LessonStatus.SCHEDULED);
        var completedToday = await _context.Lessons.CountAsync(l => l.StartTime.Date == today && l.Status == LessonStatus.COMPLETED);
        var readyForExam = await _context.Students.CountAsync(s =>
            s.MedicalExamApproved &&
            s.TheoreticalCourseCompleted &&
            s.CompletedPracticalLessons >= s.RequiredPracticalLessons);

        return Ok(new AdminDashboardKpiDto(
            totalStudents,
            activeInstructors,
            activeVehicles,
            maintenanceVehicles,
            scheduledToday,
            completedToday,
            readyForExam
        ));
    }
}
