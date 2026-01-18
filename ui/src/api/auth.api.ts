import { apiClient } from './client';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  refreshToken: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  role?: string;
}

export interface UserResponse {
  id: string;
  email: string | null;
  role?: string;
}

export interface RefreshTokenRequest {
  token: string;
  refreshToken: string;
}

export interface RefreshTokenResponse {
  token: string;
  refreshToken: string;
}

export const authApi = {
  login: async (credentials: LoginRequest): Promise<LoginResponse> => {
    const response = await apiClient.post<LoginResponse>('/users/login', credentials);
    return response.data;
  },

  register: async (data: RegisterRequest): Promise<UserResponse> => {
    const response = await apiClient.post<UserResponse>('/users/register', data);
    return response.data;
  },

  refreshToken: async (data: RefreshTokenRequest): Promise<RefreshTokenResponse> => {
    const response = await apiClient.post<RefreshTokenResponse>('/users/refresh', data);
    return response.data;
  },

  getCurrentUser: async (userId: string): Promise<UserResponse> => {
    const response = await apiClient.get<UserResponse>(`/users/${userId}`);
    return response.data;
  },
};

