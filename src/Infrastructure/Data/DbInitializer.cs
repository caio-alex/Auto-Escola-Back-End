using AutoEscola.Api.Domain.Entities;
using AutoEscola.Api.Domain.Enums;

namespace AutoEscola.Api.Infrastructure.Data;

public static class DbInitializer
{
    public static void Seed(AppDbContext context)
    {
        if (context.Users.Any()) return;

        // 1. Admin User
        var adminUser = new User
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Name = "Admin Secretaria",
            Email = "admin@autoescola.com",
            Cpf = "00000000001",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            Role = UserRole.ADMIN,
            MustChangePassword = false
        };

        // 2. Instructors
        var instUser1 = new User
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222221"),
            Name = "Roberto Carlos (Instrutor)",
            Email = "roberto@autoescola.com",
            Cpf = "11122233344",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Instrutor@123"),
            Role = UserRole.INSTRUCTOR
        };
        var instructor1 = new Instructor
        {
            Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1"),
            UserId = instUser1.Id,
            DetranCredential = "SP-99881-B",
            TeachingCategory = CnhCategory.AB,
            Phone = "11988887777",
            Rating = 4.9m
        };

        var instUser2 = new User
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Name = "Fernanda Lima (Instrutora)",
            Email = "fernanda@autoescola.com",
            Cpf = "55566677788",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Instrutor@123"),
            Role = UserRole.INSTRUCTOR
        };
        var instructor2 = new Instructor
        {
            Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2"),
            UserId = instUser2.Id,
            DetranCredential = "SP-77442-B",
            TeachingCategory = CnhCategory.B,
            Phone = "11977776666",
            Rating = 4.8m
        };

        // 3. Student
        var studentUser = new User
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333331"),
            Name = "Caio Alexandre",
            Email = "aluno@autoescola.com",
            Cpf = "12345678909",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Aluno@123"),
            Role = UserRole.STUDENT
        };
        var student = new Student
        {
            Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb1"),
            UserId = studentUser.Id,
            DesiredCategory = CnhCategory.B,
            Phone = "11999990000",
            BirthDate = new DateTime(2004, 9, 9),
            MedicalExamApproved = true,
            TheoreticalCourseCompleted = true,
            RequiredPracticalLessons = 20,
            CompletedPracticalLessons = 12
        };

        // 4. Fleet (Vehicles)
        var vehicle1 = new Vehicle
        {
            Id = Guid.Parse("cccccccc-cccc-cccc-cccc-ccccccccccc1"),
            Brand = "Hyundai",
            Model = "HB20 Hatch 1.0",
            Plate = "BRA2E19",
            Year = 2025,
            Transmission = "Manual",
            Category = CnhCategory.B,
            Status = VehicleStatus.ACTIVE
        };

        var vehicle2 = new Vehicle
        {
            Id = Guid.Parse("cccccccc-cccc-cccc-cccc-ccccccccccc2"),
            Brand = "Volkswagen",
            Model = "Polo Track",
            Plate = "SPO4A88",
            Year = 2025,
            Transmission = "Manual",
            Category = CnhCategory.B,
            Status = VehicleStatus.ACTIVE
        };

        var vehicleMoto = new Vehicle
        {
            Id = Guid.Parse("cccccccc-cccc-cccc-cccc-ccccccccccc3"),
            Brand = "Honda",
            Model = "CG 160 Start",
            Plate = "MOT9X11",
            Year = 2024,
            Transmission = "Manual",
            Category = CnhCategory.A,
            Status = VehicleStatus.ACTIVE
        };

        // 5. Initial Lessons
        var tomorrow = DateTime.Today.AddDays(1).AddHours(9); // Tomorrow 09:00
        if (tomorrow.DayOfWeek == DayOfWeek.Sunday) tomorrow = tomorrow.AddDays(1);

        var sampleLesson = new Lesson
        {
            Id = Guid.NewGuid(),
            StudentId = student.Id,
            InstructorId = instructor1.Id,
            VehicleId = vehicle1.Id,
            StartTime = tomorrow,
            EndTime = tomorrow.AddMinutes(50),
            Status = LessonStatus.SCHEDULED
        };

        context.Users.AddRange(adminUser, instUser1, instUser2, studentUser);
        context.Instructors.AddRange(instructor1, instructor2);
        context.Students.Add(student);
        context.Vehicles.AddRange(vehicle1, vehicle2, vehicleMoto);
        context.Lessons.Add(sampleLesson);

        context.SaveChanges();
    }
}
