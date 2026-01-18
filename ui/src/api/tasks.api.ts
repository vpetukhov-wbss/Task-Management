import { apiClient } from './client';

export enum TaskStatus {
  Incomplete = 0,
  Complete = 1,
  InProgress = 2,
  OnHold = 3,
}

export enum TaskPriority {
  Low = 0,
  Medium = 1,
  High = 2,
}

export interface TaskResponse {
  id: string;
  title: string;
  description: string | null;
  status: TaskStatus;
  statusName: string;
  priority: TaskPriority;
  priorityName: string;
  userId: string;
  boardId: string; // Added missing boardId field
  createdAtUtc: string;
  updatedAtUtc: string | null;
}

export interface CreateTaskRequest {
  title: string;
  description?: string | null;
  priority: TaskPriority;
  userId: string;
  boardId: string;
}

export interface UpdateTaskRequest {
  taskId: string;
  title: string;
  description?: string | null;
  priority: TaskPriority;
}

export interface PatchTaskRequest {
  taskId: string;
  title?: string | null;
  description?: string | null;
  priority?: TaskPriority | null;
  status?: TaskStatus | null;
}

export const tasksApi = {
  getTasksByUser: async (userId: string): Promise<TaskResponse[]> => {
    const response = await apiClient.get<TaskResponse[]>(`/tasks/user/${userId}`);
    return response.data;
  },

  getTasksByStatus: async (userId: string, status: TaskStatus): Promise<TaskResponse[]> => {
    const response = await apiClient.get<TaskResponse[]>(`/tasks/status/${status}?userId=${userId}`);
    return response.data;
  },

  getTasksByBoard: async (boardId: string): Promise<TaskResponse[]> => {
    const response = await apiClient.get<TaskResponse[]>(`/tasks/board/${boardId}`);
    return response.data;
  },

  getTaskById: async (taskId: string): Promise<TaskResponse> => {
    const response = await apiClient.get<TaskResponse>(`/tasks/${taskId}`);
    return response.data;
  },

  createTask: async (data: CreateTaskRequest): Promise<TaskResponse> => {
    const response = await apiClient.post<TaskResponse>('/tasks', data);
    return response.data;
  },

  updateTask: async (taskId: string, data: UpdateTaskRequest): Promise<TaskResponse> => {
    const response = await apiClient.put<TaskResponse>(`/tasks/${taskId}`, data);
    return response.data;
  },

  patchTask: async (taskId: string, data: PatchTaskRequest): Promise<TaskResponse> => {
    console.log('Patching task:', taskId, 'with data:', data);
    const response = await apiClient.patch<TaskResponse>(`/tasks/${taskId}`, data);
    return response.data;
  },

  deleteTask: async (taskId: string): Promise<void> => {
    await apiClient.delete(`/tasks/${taskId}`);
  },

  markTaskComplete: async (taskId: string): Promise<TaskResponse> => {
    const response = await apiClient.post<TaskResponse>(`/tasks/${taskId}/complete`);
    return response.data;
  },

  markTaskIncomplete: async (taskId: string): Promise<TaskResponse> => {
    const response = await apiClient.post<TaskResponse>(`/tasks/${taskId}/incomplete`);
    return response.data;
  },
};

