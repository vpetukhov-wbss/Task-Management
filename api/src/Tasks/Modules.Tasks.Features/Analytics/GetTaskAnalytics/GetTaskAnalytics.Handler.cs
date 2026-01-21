using Microsoft.EntityFrameworkCore;
using Modules.Common.Domain.Results;
using Modules.Tasks.Domain.Tasks;
using Modules.Tasks.Infrastructure.Database;

namespace Modules.Tasks.Features.Analytics.GetTaskAnalytics;

public sealed record GetTaskAnalyticsQuery;

public sealed class GetTaskAnalyticsHandler
{
    private readonly TasksDbContext _context;

    public GetTaskAnalyticsHandler(TasksDbContext context)
    {
        _context = context;
    }

    public async Task<Result<TaskAnalyticsResponse>> Handle(
        CancellationToken cancellationToken)
    {
        var totalTasks = await _context.Tasks.CountAsync(cancellationToken);
        var totalBoards = await _context.Boards.CountAsync(cancellationToken);
        var tasksByStatus = await GetTasksByStatusAsync(cancellationToken);
        var tasksByPriority = await GetTasksByPriorityAsync(cancellationToken);
        var recentTasks = await GetRecentTasksCountAsync(cancellationToken);
        var taskGrowth = await GetTaskGrowthDataAsync(cancellationToken);
        var completionRate = await CalculateCompletionRateAsync(totalTasks, cancellationToken);

        var response = new TaskAnalyticsResponse(
            totalTasks,
            totalBoards,
            recentTasks,
            completionRate,
            tasksByStatus,
            tasksByPriority,
            taskGrowth
        );

        return response;
    }

    private async Task<IReadOnlyList<TaskStatusDistribution>> GetTasksByStatusAsync(CancellationToken cancellationToken)
    {
        return await _context.Tasks
            .GroupBy(t => t.Status)
            .Select(g => new TaskStatusDistribution(g.Key.ToString(), g.Count()))
            .ToListAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<TaskPriorityDistribution>> GetTasksByPriorityAsync(CancellationToken cancellationToken)
    {
        return await _context.Tasks
            .GroupBy(t => t.Priority)
            .Select(g => new TaskPriorityDistribution(g.Key.ToString(), g.Count()))
            .ToListAsync(cancellationToken);
    }

    private async Task<int> GetRecentTasksCountAsync(CancellationToken cancellationToken)
    {
        var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
        return await _context.Tasks
            .Where(t => t.CreatedAtUtc >= sevenDaysAgo)
            .CountAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<TaskGrowthData>> GetTaskGrowthDataAsync(CancellationToken cancellationToken)
    {
        var taskGrowth = new List<TaskGrowthData>();
        for (var i = 6; i >= 0; i--)
        {
            var date = DateTime.UtcNow.Date.AddDays(-i);
            var nextDate = date.AddDays(1);
            var count = await _context.Tasks
                .Where(t => t.CreatedAtUtc >= date && t.CreatedAtUtc < nextDate)
                .CountAsync(cancellationToken);
            taskGrowth.Add(new TaskGrowthData(date.ToString("yyyy-MM-dd"), count));
        }
        return taskGrowth;
    }

    private async Task<double> CalculateCompletionRateAsync(int totalTasks, CancellationToken cancellationToken)
    {
        if (totalTasks == 0)
        {
            return 0;
        }

        var completedTasks = await _context.Tasks
            .Where(t => t.Status == Domain.Tasks.TaskStatus.Complete)
            .CountAsync(cancellationToken);
        return Math.Round((double)completedTasks / totalTasks * 100, 2);
    }
}

