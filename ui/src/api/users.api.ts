import { apiClient } from './client';

export interface User {
  id: string;
  email: string | null;
  role: string;
  createdAtUtc?: string;
  updatedAtUtc?: string | null;
}

export interface CreateUserRequest {
  email: string;
  password: string;
  role: string;
}

export interface UpdateUserRequest {
  email?: string;
  role?: string;
  password?: string;
}

export interface PagedRequest {
  pageNumber?: number;
  pageSize?: number;
  searchTerm?: string;
  sortBy?: string;
  sortAscending?: boolean;
}

export interface PagedResult<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export const usersApi = {
  getAllUsers: async (request?: PagedRequest): Promise<PagedResult<User>> => {
    const params = new URLSearchParams();
    if (request?.pageNumber) params.append('pageNumber', request.pageNumber.toString());
    if (request?.pageSize) params.append('pageSize', request.pageSize.toString());
    if (request?.searchTerm) params.append('searchTerm', request.searchTerm);
    if (request?.sortBy) params.append('sortBy', request.sortBy);
    if (request?.sortAscending !== undefined) params.append('sortAscending', request.sortAscending.toString());

    const response = await apiClient.get<PagedResult<User>>(`/users?${params.toString()}`);
    return response.data;
  },

  getUserById: async (userId: string): Promise<User> => {
    const response = await apiClient.get<User>(`/users/${userId}`);
    return response.data;
  },

  createUser: async (data: CreateUserRequest): Promise<User> => {
    const response = await apiClient.post<User>('/users/register', data);
    return response.data;
  },

  updateUser: async (userId: string, data: UpdateUserRequest): Promise<User> => {
    const response = await apiClient.put<User>(`/users/${userId}`, data);
    return response.data;
  },

  deleteUser: async (userId: string): Promise<void> => {
    await apiClient.delete(`/users/${userId}`);
  },
};

