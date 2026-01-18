import React, { useEffect } from 'react';
import { Users, CheckCircle, TrendingUp, Activity, BarChart2, PieChart } from 'react-feather';
import { useAppDispatch, useAppSelector } from '../../store/hooks';
import { fetchAllAnalytics, clearError } from '../../store/slices/analyticsSlice';
import './Analytics.css';

function Analytics() {
  const dispatch = useAppDispatch();
  const { userAnalytics, taskAnalytics, loading, error } = useAppSelector((state) => state.analytics);

  useEffect(() => {
    dispatch(fetchAllAnalytics());
  }, [dispatch]);

  const handleDismissError = () => {
    dispatch(clearError());
  };

  if (loading && !userAnalytics && !taskAnalytics) {
    return (
      <div className="analytics-container">
        <div className="loading-message">Loading analytics...</div>
      </div>
    );
  }

  return (
    <div className="analytics-container">
      <div className="analytics-header">
        <h2>Analytics Dashboard</h2>
        <p className="analytics-subtitle">Overview of system metrics and statistics</p>
      </div>

      {error && (
        <div className="error-banner">
          <span>{error}</span>
          <button onClick={handleDismissError} className="dismiss-button">×</button>
        </div>
      )}

      {/* User Statistics */}
      {userAnalytics && (
        <div className="analytics-section">
          <h3 className="section-title">
            <Users size={20} />
            User Statistics
          </h3>

          <div className="stats-grid">
            <div className="stat-card">
              <div className="stat-icon users">
                <Users size={24} />
              </div>
              <div className="stat-content">
                <div className="stat-value">{userAnalytics.userStatistics.totalUsers}</div>
                <div className="stat-label">Total Users</div>
              </div>
            </div>

            <div className="stat-card">
              <div className="stat-icon recent">
                <TrendingUp size={24} />
              </div>
              <div className="stat-content">
                <div className="stat-value">{userAnalytics.userStatistics.recentUsers}</div>
                <div className="stat-label">New Users (7 days)</div>
              </div>
            </div>

            <div className="stat-card">
              <div className="stat-icon monthly">
                <Activity size={24} />
              </div>
              <div className="stat-content">
                <div className="stat-value">{userAnalytics.userStatistics.monthlyUsers}</div>
                <div className="stat-label">New Users (30 days)</div>
              </div>
            </div>
          </div>

          {/* Role Distribution */}
          <div className="chart-container">
            <h4 className="chart-title">
              <PieChart size={18} />
              Role Distribution
            </h4>
            <div className="role-distribution">
              {userAnalytics.userStatistics.roleDistribution.map((role) => (
                <div key={role.roleName} className="role-item">
                  <div className="role-info">
                    <span className="role-name">{role.roleName}</span>
                    <span className="role-count">{role.userCount} users</span>
                  </div>
                  <div className="role-bar-container">
                    <div
                      className="role-bar"
                      style={{
                        width: `${(role.userCount / userAnalytics.userStatistics.totalUsers) * 100}%`,
                      }}
                    />
                  </div>
                </div>
              ))}
            </div>
          </div>

          {/* User Growth Chart */}
          <div className="chart-container">
            <h4 className="chart-title">
              <BarChart2 size={18} />
              User Growth (Last 7 Days)
            </h4>
            <div className="growth-chart">
              {userAnalytics.userStatistics.userGrowth.map((data) => {
                const maxCount = Math.max(...userAnalytics.userStatistics.userGrowth.map(d => d.count), 1);
                const height = (data.count / maxCount) * 100;

                return (
                  <div key={data.date} className="growth-bar-container">
                    <div className="growth-bar-wrapper">
                      <div
                        className="growth-bar"
                        style={{ height: `${height}%` }}
                        title={`${data.count} users`}
                      />
                    </div>
                    <div className="growth-label">{new Date(data.date).toLocaleDateString('en-US', { month: 'short', day: 'numeric' })}</div>
                    <div className="growth-count">{data.count}</div>
                  </div>
                );
              })}
            </div>
          </div>
        </div>
      )}

      {/* Task Statistics */}
      {taskAnalytics && (
        <div className="analytics-section">
          <h3 className="section-title">
            <CheckCircle size={20} />
            Task Statistics
          </h3>

          <div className="stats-grid">
            <div className="stat-card">
              <div className="stat-icon tasks">
                <CheckCircle size={24} />
              </div>
              <div className="stat-content">
                <div className="stat-value">{taskAnalytics.totalTasks}</div>
                <div className="stat-label">Total Tasks</div>
              </div>
            </div>

            <div className="stat-card">
              <div className="stat-icon boards">
                <BarChart2 size={24} />
              </div>
              <div className="stat-content">
                <div className="stat-value">{taskAnalytics.totalBoards}</div>
                <div className="stat-label">Total Boards</div>
              </div>
            </div>

            <div className="stat-card">
              <div className="stat-icon completion">
                <TrendingUp size={24} />
              </div>
              <div className="stat-content">
                <div className="stat-value">{taskAnalytics.completionRate.toFixed(1)}%</div>
                <div className="stat-label">Completion Rate</div>
              </div>
            </div>

            <div className="stat-card">
              <div className="stat-icon recent">
                <Activity size={24} />
              </div>
              <div className="stat-content">
                <div className="stat-value">{taskAnalytics.recentTasks}</div>
                <div className="stat-label">Recent Tasks (7 days)</div>
              </div>
            </div>
          </div>

          {/* Task Status Distribution */}
          <div className="chart-container">
            <h4 className="chart-title">
              <PieChart size={18} />
              Task Status Distribution
            </h4>
            <div className="status-distribution">
              {taskAnalytics.tasksByStatus.map((status) => (
                <div key={status.status} className="status-item">
                  <div className="status-info">
                    <span className="status-name">{status.status}</span>
                    <span className="status-count">{status.count} tasks</span>
                  </div>
                  <div className="status-bar-container">
                    <div
                      className={`status-bar status-${status.status.toLowerCase()}`}
                      style={{
                        width: `${(status.count / taskAnalytics.totalTasks) * 100}%`,
                      }}
                    />
                  </div>
                </div>
              ))}
            </div>
          </div>

          {/* Task Priority Distribution */}
          <div className="chart-container">
            <h4 className="chart-title">
              <PieChart size={18} />
              Task Priority Distribution
            </h4>
            <div className="priority-distribution">
              {taskAnalytics.tasksByPriority.map((priority) => (
                <div key={priority.priority} className="priority-item">
                  <div className="priority-info">
                    <span className="priority-name">{priority.priority}</span>
                    <span className="priority-count">{priority.count} tasks</span>
                  </div>
                  <div className="priority-bar-container">
                    <div
                      className={`priority-bar priority-${priority.priority.toLowerCase()}`}
                      style={{
                        width: `${(priority.count / taskAnalytics.totalTasks) * 100}%`,
                      }}
                    />
                  </div>
                </div>
              ))}
            </div>
          </div>

          {/* Task Growth Chart */}
          <div className="chart-container">
            <h4 className="chart-title">
              <BarChart2 size={18} />
              Task Creation Trend (Last 7 Days)
            </h4>
            <div className="growth-chart">
              {taskAnalytics.taskGrowth.map((data) => {
                const maxCount = Math.max(...taskAnalytics.taskGrowth.map(d => d.count), 1);
                const height = (data.count / maxCount) * 100;

                return (
                  <div key={data.date} className="growth-bar-container">
                    <div className="growth-bar-wrapper">
                      <div
                        className="growth-bar task-growth"
                        style={{ height: `${height}%` }}
                        title={`${data.count} tasks`}
                      />
                    </div>
                    <div className="growth-label">{new Date(data.date).toLocaleDateString('en-US', { month: 'short', day: 'numeric' })}</div>
                    <div className="growth-count">{data.count}</div>
                  </div>
                );
              })}
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

export default Analytics;

