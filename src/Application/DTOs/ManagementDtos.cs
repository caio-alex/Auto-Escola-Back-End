using AutoEscola.Api.Domain.Enums;

namespace AutoEscola.Api.Application.DTOs;

public record CreateStudentRequestDto(
    string Name,
    string Email,
    string Cpf,
    string Phone,
    DateTime BirthDate,
    CnhCategory DesiredCategory,
    bool MedicalExamApproved = true,
    bool TheoreticalCourseCompleted = true
);

public record CreateInstructorRequestDto(
    string Name,
    string Email,
    string Cpf,
    string Phone,
    string DetranCredential,
    CnhCategory TeachingCategory
);

public record UpdateVehicleStatusRequestDto(
    VehicleStatus NewStatus
);
