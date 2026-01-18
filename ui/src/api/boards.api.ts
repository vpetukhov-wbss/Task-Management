import { apiClient } from './client';

export interface BoardResponse {
  id: string;
  title: string;
  userId: string;
  createdAtUtc: string;
  updatedAtUtc: string | null;
}

export interface CreateBoardRequest {
  title: string;
  userId: string;
}

export interface UpdateBoardRequest {
  title: string;
}

export const boardsApi = {
  getAllBoards: async (): Promise<BoardResponse[]> => {
    const response = await apiClient.get<BoardResponse[]>('/boards');
    return response.data;
  },

  getBoardsByUser: async (userId: string): Promise<BoardResponse[]> => {
    const response = await apiClient.get<BoardResponse[]>(`/boards/user/${userId}`);
    return response.data;
  },

  getBoardById: async (boardId: string): Promise<BoardResponse> => {
    const response = await apiClient.get<BoardResponse>(`/boards/${boardId}`);
    return response.data;
  },

  createBoard: async (data: CreateBoardRequest): Promise<BoardResponse> => {
    const response = await apiClient.post<BoardResponse>('/boards', data);
    return response.data;
  },

  updateBoard: async (boardId: string, data: UpdateBoardRequest): Promise<BoardResponse> => {
    const response = await apiClient.put<BoardResponse>(`/boards/${boardId}`, data);
    return response.data;
  },

  deleteBoard: async (boardId: string): Promise<void> => {
    await apiClient.delete(`/boards/${boardId}`);
  },
};

