import { apiClient } from './client';
import { PagedRequest, PagedResult } from './users.api';

export interface Role {
  id: string;
  name: string;
  userCount: number;
}

export interface CreateRoleRequest {
  name: string;
}

export interface UpdateRoleRequest {
  name: string;
}

export const rolesApi = {
  getAllRoles: async (request?: PagedRequest): Promise<PagedResult<Role>> => {
    const params = new URLSearchParams();
    if (request?.pageNumber) params.append('pageNumber', request.pageNumber.toString());
    if (request?.pageSize) params.append('pageSize', request.pageSize.toString());
    if (request?.searchTerm) params.append('searchTerm', request.searchTerm);
    if (request?.sortBy) params.append('sortBy', request.sortBy);
    if (request?.sortAscending !== undefined) params.append('sortAscending', request.sortAscending.toString());

    const response = await apiClient.get<PagedResult<Role>>(`/roles?${params.toString()}`);
    return response.data;
  },

  getRoleById: async (roleId: string): Promise<Role> => {
    const response = await apiClient.get<Role>(`/roles/${roleId}`);
    return response.data;
  },

  createRole: async (data: CreateRoleRequest): Promise<Role> => {
    const response = await apiClient.post<Role>('/roles', data);
    return response.data;
  },

  updateRole: async (roleId: string, data: UpdateRoleRequest): Promise<Role> => {
    const response = await apiClient.put<Role>(`/roles/${roleId}`, data);
    return response.data;
  },

  deleteRole: async (roleId: string): Promise<void> => {
    await apiClient.delete(`/roles/${roleId}`);
  },
};

