import { IBoard } from "../Interfaces/Kanban";
import { boardsApi, BoardResponse } from "../api/boards.api";

function convertBoardResponseToIBoard(boardResponse: BoardResponse): IBoard {
  return {
    id: boardResponse.id,
    title: boardResponse.title,
    cards: [],
    userId: boardResponse.userId,
    createdAtUtc: boardResponse.createdAtUtc,
    updatedAtUtc: boardResponse.updatedAtUtc || null,
  };
}

export class BoardAPI {
  async fetchBoardList(userId: string): Promise<IBoard[]> {
    try {
      const boardResponses = await boardsApi.getBoardsByUser(userId);
      return boardResponses.map(convertBoardResponseToIBoard);
    } catch (error) {
      throw error;
    }
  }

  async createBoard(title: string, userId: string): Promise<IBoard> {
    try {
      const boardResponse = await boardsApi.createBoard({ title, userId });
      return convertBoardResponseToIBoard(boardResponse);
    } catch (error) {
      throw error;
    }
  }

  async updateBoard(boardId: string, title: string): Promise<IBoard> {
    try {
      const boardResponse = await boardsApi.updateBoard(boardId, { title });
      return convertBoardResponseToIBoard(boardResponse);
    } catch (error) {
      throw error;
    }
  }

  async deleteBoard(boardId: string): Promise<void> {
    try {
      await boardsApi.deleteBoard(boardId);
    } catch (error) {
      throw error;
    }
  }
}


export async function fetchBoardList(userId: string): Promise<IBoard[]> {
  const api = new BoardAPI();
  return api.fetchBoardList(userId);
}

export async function createBoard(title: string, userId: string): Promise<IBoard> {
  const api = new BoardAPI();
  return api.createBoard(title, userId);
}

export async function updateBoard(boardId: string, title: string): Promise<IBoard> {
  const api = new BoardAPI();
  return api.updateBoard(boardId, title);
}

export async function deleteBoard(boardId: string): Promise<void> {
  const api = new BoardAPI();
  return api.deleteBoard(boardId);
}
