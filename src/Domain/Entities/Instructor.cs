using AutoEscola.Api.Domain.Enums;

namespace AutoEscola.Api.Domain.Entities;

public class Instructor
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string DetranCredential { get; set; } = string.Empty;
    public CnhCategory TeachingCategory { get; set; }
    public string Phone { get; set; } = string.Empty;
    public decimal Rating { get; set; } = 5.0m;

    public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
}
