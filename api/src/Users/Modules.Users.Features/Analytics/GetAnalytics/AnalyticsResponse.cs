namespace Modules.Users.Features.Analytics.GetAnalytics;

public sealed record AnalyticsResponse(
    UserStatistics UserStatistics);

public sealed record UserStatistics(
    int TotalUsers,
    int RecentUsers,
    int MonthlyUsers,
    IReadOnlyList<RoleDistribution> RoleDistribution,
    IReadOnlyList<UserGrowthData> UserGrowth);

public sealed record RoleDistribution(
    string RoleName,
    int UserCount);

public sealed record UserGrowthData(
    string Date,
    int Count);

