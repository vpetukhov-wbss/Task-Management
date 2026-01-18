import { createSlice, createAsyncThunk, PayloadAction } from '@reduxjs/toolkit';
import { IBoard, ICard } from '../../Interfaces/Kanban';
import { boardsApi, BoardResponse } from '../../api/boards.api';
import { tasksApi, TaskPriority, TaskStatus } from '../../api/tasks.api';

interface BoardsState {
  boards: IBoard[];
  isLoading: boolean;
  error: string | null;
  selectedBoardId: string | null;
}

const initialState: BoardsState = {
  boards: [],
  isLoading: false,
  error: null,
  selectedBoardId: null,
};

const convertBoardResponseToIBoard = (boardResponse: BoardResponse): IBoard => ({
  id: boardResponse.id,
  title: boardResponse.title,
  cards: [],
  userId: boardResponse.userId,
  createdAtUtc: boardResponse.createdAtUtc,
  updatedAtUtc: boardResponse.updatedAtUtc,
});

const convertTaskResponseToICard = (task: any): ICard => ({
  id: task.id, // Backend task ID (GUID)
  title: task.title,
  description: task.description,
  status: task.status,
  statusName: task.statusName,
  priority: task.priority,
  priorityName: task.priorityName,
  userId: task.userId,
  boardId: task.boardId,
  createdAtUtc: task.createdAtUtc,
  updatedAtUtc: task.updatedAtUtc,
});

export const fetchBoards = createAsyncThunk(
  'boards/fetchBoards',
  async (_, { rejectWithValue }) => {
    try {
      const boardResponses = await boardsApi.getAllBoards();
      console.log('Board responses from API:', boardResponses);

      const boards = boardResponses.map(convertBoardResponseToIBoard);
      console.log('Converted boards:', boards);

      const boardsWithTasks = await Promise.all(
        boards.map(async (board) => {
          if (!board.id) {
            console.error('Board has no ID:', board);
            return board;
          }

          try {
            const tasks = await tasksApi.getTasksByBoard(board.id);
            return {
              ...board,
              cards: tasks.map(convertTaskResponseToICard),
            };
          } catch (error) {
            console.error(`Failed to fetch tasks for board ${board.id}:`, error);
            return board;
          }
        })
      );

      return boardsWithTasks;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to fetch boards');
    }
  }
);

export const createBoard = createAsyncThunk(
  'boards/createBoard',
  async ({ title, userId }: { title: string; userId: string }, { rejectWithValue }) => {
    try {
      const boardResponse = await boardsApi.createBoard({ title, userId });
      return convertBoardResponseToIBoard(boardResponse);
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to create board');
    }
  }
);

export const updateBoardTitle = createAsyncThunk(
  'boards/updateBoard',
  async ({ boardId, title }: { boardId: string; title: string }, { rejectWithValue }) => {
    try {
      const boardResponse = await boardsApi.updateBoard(boardId, { title });
      return convertBoardResponseToIBoard(boardResponse);
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to update board');
    }
  }
);

export const deleteBoard = createAsyncThunk(
  'boards/deleteBoard',
  async (boardId: string, { rejectWithValue }) => {
    try {
      await boardsApi.deleteBoard(boardId);
      return boardId;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to delete board');
    }
  }
);

export const addCard = createAsyncThunk(
  'boards/addCard',
  async (
    { boardId, title, userId }: { boardId: string; title: string; userId: string },
    { rejectWithValue }
  ) => {
    try {
      console.log('Creating task with:', { boardId, title, userId });

      const taskResponse = await tasksApi.createTask({
        title,
        description: '',
        priority: TaskPriority.Medium,
        userId,
        boardId,
      });

      console.log('Task created successfully:', taskResponse);
      return { boardId, task: taskResponse };
    } catch (error: any) {
      console.error('Failed to create task:', error.response?.data || error.message);
      return rejectWithValue(error.response?.data?.message || 'Failed to create card');
    }
  }
);

export const patchCard = createAsyncThunk(
  'boards/patchCard',
  async (
    {
      boardId,
      cardId,
      taskId,
      updates,
    }: {
      boardId: string;
      cardId: string;
      taskId: string;
      updates: { title?: string; description?: string; priority?: TaskPriority; status?: TaskStatus };
    },
    { rejectWithValue }
  ) => {
    try {
      console.log('📝 Patching task:', taskId, 'with updates:', updates);

      const taskResponse = await tasksApi.patchTask(taskId, {
        taskId,
        ...updates,
      });

      console.log('✅ Task patched successfully:', taskResponse);
      return { boardId, cardId, task: taskResponse };
    } catch (error: any) {
      console.error('❌ Failed to patch task:', error.response?.data || error.message);
      return rejectWithValue(error.response?.data?.message || 'Failed to update card');
    }
  }
);

const boardsSlice = createSlice({
  name: 'boards',
  initialState,
  reducers: {
    removeCard: (state, action: PayloadAction<{ boardId: string; cardId: string }>) => {
      const board = state.boards.find((b) => b.id === action.payload.boardId);
      if (board) {
        board.cards = board.cards.filter((card) => card.id !== action.payload.cardId);
      }
    },
    updateCard: (
      state,
      action: PayloadAction<{ boardId: string; cardId: string; card: ICard }>
    ) => {
      const board = state.boards.find((b) => b.id === action.payload.boardId);
      if (board) {
        const cardIndex = board.cards.findIndex((c) => c.id === action.payload.cardId);
        if (cardIndex !== -1) {
          board.cards[cardIndex] = action.payload.card;
        }
      }
    },
    moveCard: (
      state,
      action: PayloadAction<{
        sourceBoardId: string;
        targetBoardId: string;
        sourceCardId: string;
        targetCardId: string;
      }>
    ) => {
      const { sourceBoardId, targetBoardId, sourceCardId, targetCardId } = action.payload;

      const sourceBoard = state.boards.find((b) => b.id === sourceBoardId);
      const targetBoard = state.boards.find((b) => b.id === targetBoardId);

      if (!sourceBoard || !targetBoard) return;

      const sourceCardIndex = sourceBoard.cards.findIndex((c) => c.id === sourceCardId);
      const targetCardIndex = targetBoard.cards.findIndex((c) => c.id === targetCardId);

      if (sourceCardIndex === -1 || targetCardIndex === -1) return;

      const [movedCard] = sourceBoard.cards.splice(sourceCardIndex, 1);
      targetBoard.cards.splice(targetCardIndex, 0, movedCard);
    },
    clearError: (state) => {
      state.error = null;
    },
    setSelectedBoard: (state, action: PayloadAction<string | null>) => {
      state.selectedBoardId = action.payload;
    },
    moveBoard: (
      state,
      action: PayloadAction<{ sourceBoardId: string; targetBoardId: string }>
    ) => {
      const { sourceBoardId, targetBoardId } = action.payload;

      const sourceBoardIndex = state.boards.findIndex((b) => b.id === sourceBoardId);
      const targetBoardIndex = state.boards.findIndex((b) => b.id === targetBoardId);

      if (sourceBoardIndex === -1 || targetBoardIndex === -1) return;
      const [movedBoard] = state.boards.splice(sourceBoardIndex, 1);
      state.boards.splice(targetBoardIndex, 0, movedBoard);
    },
  },
  extraReducers: (builder) => {
    // Fetch boards
    builder
      .addCase(fetchBoards.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(fetchBoards.fulfilled, (state, action) => {
        state.isLoading = false;
        state.boards = action.payload;
      })
      .addCase(fetchBoards.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      });

    // Create board
    builder
      .addCase(createBoard.pending, (state) => {
        state.error = null;
      })
      .addCase(createBoard.fulfilled, (state, action) => {
        state.boards.push(action.payload);
      })
      .addCase(createBoard.rejected, (state, action) => {
        state.error = action.payload as string;
      });

    // Update board
    builder
      .addCase(updateBoardTitle.fulfilled, (state, action) => {
        const index = state.boards.findIndex((b) => b.id === action.payload.id);
        if (index !== -1) {
          state.boards[index] = { ...state.boards[index], ...action.payload };
        }
      })
      .addCase(updateBoardTitle.rejected, (state, action) => {
        state.error = action.payload as string;
      });

    // Delete board
    builder
      .addCase(deleteBoard.fulfilled, (state, action) => {
        state.boards = state.boards.filter((b) => b.id !== action.payload);
      })
      .addCase(deleteBoard.rejected, (state, action) => {
        state.error = action.payload as string;
      });

    // Add card (create task)
    builder
      .addCase(addCard.pending, (state) => {
        state.error = null;
      })
      .addCase(addCard.fulfilled, (state, action) => {
        const board = state.boards.find((b) => b.id === action.payload.boardId);
        if (board) {
          // Convert task response to card format
          const task = action.payload.task;
          board.cards.push({
            id: task.id,
            title: task.title,
            description: task.description || null,
            status: task.status,
            statusName: task.statusName,
            priority: task.priority,
            priorityName: task.priorityName,
            userId: task.userId,
            boardId: task.boardId,
            createdAtUtc: task.createdAtUtc,
            updatedAtUtc: task.updatedAtUtc,
          });
        }
      })
      .addCase(addCard.rejected, (state, action) => {
        state.error = action.payload as string;
      });

    // Patch card (partial update)
    builder
      .addCase(patchCard.pending, (state) => {
        state.error = null;
      })
      .addCase(patchCard.fulfilled, (state, action) => {
        const board = state.boards.find((b) => b.id === action.payload.boardId);
        if (board) {
          const cardIndex = board.cards.findIndex((c) => c.id === action.payload.cardId);
          if (cardIndex !== -1) {
            const task = action.payload.task;
            // Update only the fields that came from the backend
            board.cards[cardIndex] = {
              ...board.cards[cardIndex],
              id: task.id,
              title: task.title,
              description: task.description || null,
              priority: task.priority,
              priorityName: task.priorityName,
              status: task.status,
              statusName: task.statusName,
              updatedAtUtc: task.updatedAtUtc,
            };
          }
        }
      })
      .addCase(patchCard.rejected, (state, action) => {
        state.error = action.payload as string;
      });
  },
});

export const { removeCard, updateCard, moveCard, clearError, setSelectedBoard, moveBoard } =
  boardsSlice.actions;
export default boardsSlice.reducer;

