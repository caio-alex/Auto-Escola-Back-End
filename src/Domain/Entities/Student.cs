using AutoEscola.Api.Domain.Enums;

namespace AutoEscola.Api.Domain.Entities;

public class Student
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public CnhCategory DesiredCategory { get; set; }
    public string Phone { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }

    // Progress Tracker (Jornada do Aluno)
    public bool MedicalExamApproved { get; set; } = false;
    public bool TheoreticalCourseCompleted { get; set; } = false;
    public int RequiredPracticalLessons { get; set; } = 20;
    public int CompletedPracticalLessons { get; set; } = 0;

    public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
}
