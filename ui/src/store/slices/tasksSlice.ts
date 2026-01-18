import { createSlice, createAsyncThunk, PayloadAction } from '@reduxjs/toolkit';
import {
  tasksApi,
  TaskResponse,
  CreateTaskRequest,
  UpdateTaskRequest,
  TaskStatus,
} from '../../api/tasks.api';

interface TasksState {
  tasks: TaskResponse[];
  isLoading: boolean;
  error: string | null;
  selectedTaskId: string | null;
}

const initialState: TasksState = {
  tasks: [],
  isLoading: false,
  error: null,
  selectedTaskId: null,
};

export const fetchTasksByUser = createAsyncThunk(
  'tasks/fetchByUser',
  async (userId: string, { rejectWithValue }) => {
    try {
      return await tasksApi.getTasksByUser(userId);
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to fetch tasks');
    }
  }
);

export const fetchTasksByStatus = createAsyncThunk(
  'tasks/fetchByStatus',
  async ({ userId, status }: { userId: string; status: TaskStatus }, { rejectWithValue }) => {
    try {
      return await tasksApi.getTasksByStatus(userId, status);
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to fetch tasks');
    }
  }
);

export const createTask = createAsyncThunk(
  'tasks/create',
  async (data: CreateTaskRequest, { rejectWithValue }) => {
    try {
      return await tasksApi.createTask(data);
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to create task');
    }
  }
);

export const updateTask = createAsyncThunk(
  'tasks/update',
  async ({ taskId, data }: { taskId: string; data: UpdateTaskRequest }, { rejectWithValue }) => {
    try {
      return await tasksApi.updateTask(taskId, data);
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to update task');
    }
  }
);

export const deleteTask = createAsyncThunk(
  'tasks/delete',
  async (taskId: string, { rejectWithValue }) => {
    try {
      await tasksApi.deleteTask(taskId);
      return taskId;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to delete task');
    }
  }
);

export const markTaskComplete = createAsyncThunk(
  'tasks/markComplete',
  async (taskId: string, { rejectWithValue }) => {
    try {
      return await tasksApi.markTaskComplete(taskId);
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to mark task as complete');
    }
  }
);

export const markTaskIncomplete = createAsyncThunk(
  'tasks/markIncomplete',
  async (taskId: string, { rejectWithValue }) => {
    try {
      return await tasksApi.markTaskIncomplete(taskId);
    } catch (error: any) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to mark task as incomplete'
      );
    }
  }
);

const tasksSlice = createSlice({
  name: 'tasks',
  initialState,
  reducers: {
    clearError: (state) => {
      state.error = null;
    },
    setSelectedTask: (state, action: PayloadAction<string | null>) => {
      state.selectedTaskId = action.payload;
    },
    clearTasks: (state) => {
      state.tasks = [];
    },
  },
  extraReducers: (builder) => {
    // Fetch tasks by user
    builder
      .addCase(fetchTasksByUser.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(fetchTasksByUser.fulfilled, (state, action) => {
        state.isLoading = false;
        state.tasks = action.payload;
      })
      .addCase(fetchTasksByUser.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      });

    // Fetch tasks by status
    builder
      .addCase(fetchTasksByStatus.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(fetchTasksByStatus.fulfilled, (state, action) => {
        state.isLoading = false;
        state.tasks = action.payload;
      })
      .addCase(fetchTasksByStatus.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      });

    // Create task
    builder
      .addCase(createTask.pending, (state) => {
        state.error = null;
      })
      .addCase(createTask.fulfilled, (state, action) => {
        state.tasks.push(action.payload);
      })
      .addCase(createTask.rejected, (state, action) => {
        state.error = action.payload as string;
      });

    // Update task
    builder
      .addCase(updateTask.fulfilled, (state, action) => {
        const index = state.tasks.findIndex((t) => t.id === action.payload.id);
        if (index !== -1) {
          state.tasks[index] = action.payload;
        }
      })
      .addCase(updateTask.rejected, (state, action) => {
        state.error = action.payload as string;
      });

    // Delete task
    builder
      .addCase(deleteTask.fulfilled, (state, action) => {
        state.tasks = state.tasks.filter((t) => t.id !== action.payload);
      })
      .addCase(deleteTask.rejected, (state, action) => {
        state.error = action.payload as string;
      });

    // Mark task complete
    builder
      .addCase(markTaskComplete.fulfilled, (state, action) => {
        const index = state.tasks.findIndex((t) => t.id === action.payload.id);
        if (index !== -1) {
          state.tasks[index] = action.payload;
        }
      })
      .addCase(markTaskComplete.rejected, (state, action) => {
        state.error = action.payload as string;
      });

    // Mark task incomplete
    builder
      .addCase(markTaskIncomplete.fulfilled, (state, action) => {
        const index = state.tasks.findIndex((t) => t.id === action.payload.id);
        if (index !== -1) {
          state.tasks[index] = action.payload;
        }
      })
      .addCase(markTaskIncomplete.rejected, (state, action) => {
        state.error = action.payload as string;
      });
  },
});

export const { clearError, setSelectedTask, clearTasks } = tasksSlice.actions;
export default tasksSlice.reducer;

