namespace AutoEscola.Api.Application.DTOs;

public record StudentDashboardDto(
    Guid StudentId,
    string Name,
    string DesiredCategory,
    bool MedicalExamApproved,
    bool TheoreticalCourseCompleted,
    int CompletedPracticalLessons,
    int RequiredPracticalLessons,
    double ProgressPercentage,
    bool ReadyForPracticalExam,
    LessonResponseDto? NextUpcomingLesson
);

public record AdminDashboardKpiDto(
    int TotalStudents,
    int ActiveInstructors,
    int ActiveVehicles,
    int VehiclesInMaintenance,
    int LessonsScheduledToday,
    int CompletedLessonsToday,
    int StudentsReadyForExam
);
