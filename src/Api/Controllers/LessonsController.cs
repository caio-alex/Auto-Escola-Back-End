using AutoEscola.Api.Application.DTOs;
using AutoEscola.Api.Application.Services;
using AutoEscola.Api.Domain.Enums;
using AutoEscola.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoEscola.Api.Api.Controllers;

[ApiController]
[Route("api/lessons")]
[Authorize]
public class LessonsController : ControllerBase
{
    private readonly LessonBookingService _bookingService;
    private readonly AppDbContext _context;

    public LessonsController(LessonBookingService bookingService, AppDbContext context)
    {
        _bookingService = bookingService;
        _context = context;
    }

    /// <summary>
    /// Consulta os horários disponíveis no dia para o Wizard de Agendamento em 3 Passos.
    /// </summary>
    [HttpGet("available-slots")]
    public async Task<ActionResult<List<AvailableSlotDto>>> GetAvailableSlots(
        [FromQuery] DateTime date,
        [FromQuery] CnhCategory category = CnhCategory.B)
    {
        var slots = await _bookingService.GetAvailableSlotsForDateAsync(date, category);
        return Ok(slots);
    }

    /// <summary>
    /// Lista aulas filtradas por aluno, instrutor ou data.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<LessonResponseDto>>> GetLessons(
        [FromQuery] Guid? studentId,
        [FromQuery] Guid? instructorId,
        [FromQuery] DateTime? date)
    {
        var query = _context.Lessons
            .Include(l => l.Student).ThenInclude(s => s.User)
            .Include(l => l.Instructor).ThenInclude(i => i.User)
            .Include(l => l.Vehicle)
            .AsQueryable();

        if (studentId.HasValue)
            query = query.Where(l => l.StudentId == studentId.Value);

        if (instructorId.HasValue)
            query = query.Where(l => l.InstructorId == instructorId.Value);

        if (date.HasValue)
            query = query.Where(l => l.StartTime.Date == date.Value.Date);

        var lessons = await query
            .OrderBy(l => l.StartTime)
            .Select(l => LessonBookingService.MapToResponseDto(
                l,
                l.Student.User.Name,
                l.Instructor.User.Name,
                l.Vehicle.Model,
                l.Vehicle.Plate))
            .ToListAsync();

        return Ok(lessons);
    }

    /// <summary>
    /// Agenda uma nova aula prática validando conflitos, frota e regras do DETRAN.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "STUDENT,ADMIN")]
    public async Task<ActionResult<LessonResponseDto>> CreateLesson([FromBody] CreateLessonRequestDto request)
    {
        var result = await _bookingService.ScheduleLessonAsync(request);
        return CreatedAtAction(nameof(GetLessons), new { id = result.Id }, result);
    }

    /// <summary>
    /// Marca uma aula como concluída, registra a nota/feedback do instrutor e incrementa o progresso do aluno.
    /// </summary>
    [HttpPatch("{id:guid}/complete")]
    [Authorize(Roles = "INSTRUCTOR,ADMIN")]
    public async Task<ActionResult<LessonResponseDto>> CompleteLesson(Guid id, [FromBody] CompleteLessonRequestDto request)
    {
        var result = await _bookingService.CompleteLessonAsync(id, request);
        return Ok(result);
    }

    /// <summary>
    /// Cancela uma aula agendada (valida antecedência mínima de 24h caso acionado pelo aluno).
    /// </summary>
    [HttpPatch("{id:guid}/cancel")]
    public async Task<ActionResult<LessonResponseDto>> CancelLesson(Guid id, [FromBody] CancelLessonRequestDto request)
    {
        bool isAdminOrInstructor = User.IsInRole("ADMIN") || User.IsInRole("INSTRUCTOR");
        var result = await _bookingService.CancelLessonAsync(id, request, isAdminOrInstructor);
        return Ok(result);
    }
}
