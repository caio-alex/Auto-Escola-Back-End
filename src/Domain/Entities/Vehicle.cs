using AutoEscola.Api.Domain.Enums;

namespace AutoEscola.Api.Domain.Entities;

public class Vehicle
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Plate { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Transmission { get; set; } = "Manual"; // Manual ou Automatico
    public CnhCategory Category { get; set; }
    public VehicleStatus Status { get; set; } = VehicleStatus.ACTIVE;

    public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
}
