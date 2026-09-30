using AutoEscola.Api.Domain.Enums;

namespace AutoEscola.Api.Domain.Entities;

public class Lesson
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid StudentId { get; set; }
    public Student Student { get; set; } = null!;

    public Guid InstructorId { get; set; }
    public Instructor Instructor { get; set; } = null!;

    public Guid VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = null!;

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public LessonStatus Status { get; set; } = LessonStatus.SCHEDULED;

    // Evaluation / Feedback by Instructor
    public string? InstructorNotes { get; set; }
    public int? PerformanceScore { get; set; } // 1 to 5
    public string? CancellationReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
