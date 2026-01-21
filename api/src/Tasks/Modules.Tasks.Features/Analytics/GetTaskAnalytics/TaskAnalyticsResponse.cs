namespace Modules.Tasks.Features.Analytics.GetTaskAnalytics;

public sealed record TaskAnalyticsResponse(
    int TotalTasks,
    int TotalBoards,
    int RecentTasks,
    double CompletionRate,
    IReadOnlyList<TaskStatusDistribution> TasksByStatus,
    IReadOnlyList<TaskPriorityDistribution> TasksByPriority,
    IReadOnlyList<TaskGrowthData> TaskGrowth);

public sealed record TaskStatusDistribution(
    string Status,
    int Count);

public sealed record TaskPriorityDistribution(
    string Priority,
    int Count);

public sealed record TaskGrowthData(
    string Date,
    int Count);

