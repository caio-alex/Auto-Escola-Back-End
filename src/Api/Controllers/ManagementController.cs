using AutoEscola.Api.Application.DTOs;
using AutoEscola.Api.Domain.Entities;
using AutoEscola.Api.Domain.Enums;
using AutoEscola.Api.Domain.Exceptions;
using AutoEscola.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoEscola.Api.Api.Controllers;

[ApiController]
[Route("api/management")]
[Authorize]
public class ManagementController : ControllerBase
{
    private readonly AppDbContext _context;

    public ManagementController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Lista todos os veículos da frota.
    /// </summary>
    [HttpGet("vehicles")]
    public async Task<IActionResult> GetVehicles()
    {
        var vehicles = await _context.Vehicles.ToListAsync();
        return Ok(vehicles);
    }

    /// <summary>
    /// Atualiza o status de um veículo (ex: colocar em MANUTENÇÃO) e retorna aulas impactadas.
    /// </summary>
    [HttpPatch("vehicles/{id:guid}/status")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> UpdateVehicleStatus(Guid id, [FromBody] UpdateVehicleStatusRequestDto request)
    {
        var vehicle = await _context.Vehicles.FindAsync(id)
            ?? throw new DomainException("Veículo não encontrado.", 404);

        vehicle.Status = request.NewStatus;
        await _context.SaveChangesAsync();

        var impactedLessons = await _context.Lessons
            .Where(l => l.VehicleId == id && l.Status == LessonStatus.SCHEDULED && l.StartTime >= DateTime.Now)
            .CountAsync();

        return Ok(new
        {
            message = $"Status do veículo {vehicle.Plate} atualizado para {vehicle.Status}.",
            impactedFutureLessonsCount = impactedLessons
        });
    }

    /// <summary>
    /// Cadastra um novo Aluno (Onboarding pela Secretaria com senha inicial temporária = 6 primeiros dígitos do CPF).
    /// </summary>
    [HttpPost("students")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> CreateStudent([FromBody] CreateStudentRequestDto request)
    {
        var cleanCpf = new string(request.Cpf.Where(char.IsDigit).ToArray());
        if (cleanCpf.Length != 11)
            throw new DomainException("CPF inválido. Deve conter 11 dígitos.");

        if (await _context.Users.AnyAsync(u => u.Email == request.Email || u.Cpf == cleanCpf))
            throw new DomainException("Já existe um usuário cadastrado com este E-mail ou CPF.", 409);

        var tempPassword = cleanCpf.Substring(0, 6);

        var user = new User
        {
            Name = request.Name,
            Email = request.Email.Trim().ToLower(),
            Cpf = cleanCpf,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(tempPassword),
            Role = UserRole.STUDENT,
            MustChangePassword = true
        };

        var student = new Student
        {
            UserId = user.Id,
            DesiredCategory = request.DesiredCategory,
            Phone = request.Phone,
            BirthDate = request.BirthDate,
            MedicalExamApproved = request.MedicalExamApproved,
            TheoreticalCourseCompleted = request.TheoreticalCourseCompleted
        };

        _context.Users.Add(user);
        _context.Students.Add(student);
        await _context.SaveChangesAsync();

        return Created("", new
        {
            studentId = student.Id,
            userId = user.Id,
            name = user.Name,
            temporaryPassword = tempPassword,
            note = "O aluno deverá alterar a senha no primeiro login."
        });
    }
}
