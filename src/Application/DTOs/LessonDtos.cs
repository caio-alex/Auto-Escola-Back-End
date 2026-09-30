using AutoEscola.Api.Domain.Enums;

namespace AutoEscola.Api.Application.DTOs;

public record CreateLessonRequestDto(
    Guid StudentId,
    Guid InstructorId,
    Guid VehicleId,
    DateTime StartTime
);

public record CompleteLessonRequestDto(
    int PerformanceScore,
    string? InstructorNotes
);

public record CancelLessonRequestDto(
    string Reason
);

public record LessonResponseDto(
    Guid Id,
    DateTime StartTime,
    DateTime EndTime,
    string Status,
    Guid StudentId,
    string StudentName,
    Guid InstructorId,
    string InstructorName,
    Guid VehicleId,
    string VehicleModel,
    string VehiclePlate,
    int? PerformanceScore,
    string? InstructorNotes
);

public record AvailableSlotDto(
    string TimeSlot,
    DateTime StartTime,
    bool IsAvailable,
    List<AvailableInstructorVehicleDto> AvailableOptions
);

public record AvailableInstructorVehicleDto(
    Guid InstructorId,
    string InstructorName,
    decimal InstructorRating,
    Guid VehicleId,
    string VehicleModel,
    string Transmission
);
