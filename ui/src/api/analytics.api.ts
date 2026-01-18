import { apiClient } from './client';

// User Analytics Types
export interface RoleDistribution {
  roleName: string;
  userCount: number;
}

export interface UserGrowthData {
  date: string;
  count: number;
}

export interface UserStatistics {
  totalUsers: number;
  recentUsers: number;
  monthlyUsers: number;
  roleDistribution: RoleDistribution[];
  userGrowth: UserGrowthData[];
}

export interface AnalyticsResponse {
  userStatistics: UserStatistics;
}

// Task Analytics Types
export interface TaskStatusDistribution {
  status: string;
  count: number;
}

export interface TaskPriorityDistribution {
  priority: string;
  count: number;
}

export interface TaskGrowthData {
  date: string;
  count: number;
}

export interface TaskAnalyticsResponse {
  totalTasks: number;
  totalBoards: number;
  recentTasks: number;
  completionRate: number;
  tasksByStatus: TaskStatusDistribution[];
  tasksByPriority: TaskPriorityDistribution[];
  taskGrowth: TaskGrowthData[];
}

// API Functions
export const analyticsApi = {
  getUserAnalytics: async (): Promise<AnalyticsResponse> => {
    const response = await apiClient.get<AnalyticsResponse>('/analytics');
    return response.data;
  },

  getTaskAnalytics: async (): Promise<TaskAnalyticsResponse> => {
    const response = await apiClient.get<TaskAnalyticsResponse>('/analytics/tasks');
    return response.data;
  },
};

