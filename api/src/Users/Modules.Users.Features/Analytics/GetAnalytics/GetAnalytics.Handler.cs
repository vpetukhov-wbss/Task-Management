using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Modules.Common.Domain.Results;
using Modules.Users.Domain.Users;
using Modules.Users.Infrastructure.Database;

namespace Modules.Users.Features.Analytics.GetAnalytics;

public sealed record GetAnalyticsQuery;

public sealed class GetAnalyticsHandler
{
    private readonly UsersDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;

    public GetAnalyticsHandler(
        UsersDbContext context,
        UserManager<User> userManager,
        RoleManager<Role> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<Result<AnalyticsResponse>> Handle(CancellationToken cancellationToken)
    {
        // Get total users
        var totalUsers = await _userManager.Users.CountAsync(cancellationToken);

        // Get users created in last 7 days
        var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
        var recentUsers = await _userManager.Users
            .Where(u => u.CreatedAtUtc >= sevenDaysAgo)
            .CountAsync(cancellationToken);

        // Get users created in last 30 days
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        var monthlyUsers = await _userManager.Users
            .Where(u => u.CreatedAtUtc >= thirtyDaysAgo)
            .CountAsync(cancellationToken);

        // Get role distribution
        var roles = await _roleManager.Roles.ToListAsync(cancellationToken);
        var roleDistribution = new List<RoleDistribution>();

        foreach (var role in roles)
        {
            var userCount = await _context.UserRoles
                .Where(ur => ur.RoleId == role.Id)
                .CountAsync(cancellationToken);

            roleDistribution.Add(new RoleDistribution(role.Name!, userCount));
        }

        // Get user growth data (last 7 days)
        var userGrowth = new List<UserGrowthData>();
        for (var i = 6; i >= 0; i--)
        {
            var date = DateTime.UtcNow.Date.AddDays(-i);
            var nextDate = date.AddDays(1);

            var count = await _userManager.Users
                .Where(u => u.CreatedAtUtc >= date && u.CreatedAtUtc < nextDate)
                .CountAsync(cancellationToken);

            userGrowth.Add(new UserGrowthData(date.ToString("yyyy-MM-dd"), count));
        }

        var response = new AnalyticsResponse(
            new UserStatistics(
                totalUsers,
                recentUsers,
                monthlyUsers,
                roleDistribution,
                userGrowth
            )
        );

        return response;
    }
}

