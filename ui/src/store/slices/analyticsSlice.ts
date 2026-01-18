import { createSlice, createAsyncThunk, PayloadAction } from '@reduxjs/toolkit';
import { 
  analyticsApi, 
  AnalyticsResponse, 
  TaskAnalyticsResponse 
} from '../../api/analytics.api';

interface AnalyticsState {
  userAnalytics: AnalyticsResponse | null;
  taskAnalytics: TaskAnalyticsResponse | null;
  loading: boolean;
  error: string | null;
}

const initialState: AnalyticsState = {
  userAnalytics: null,
  taskAnalytics: null,
  loading: false,
  error: null,
};

// Async thunks
export const fetchUserAnalytics = createAsyncThunk(
  'analytics/fetchUserAnalytics',
  async (_, { rejectWithValue }) => {
    try {
      const data = await analyticsApi.getUserAnalytics();
      return data;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to fetch user analytics');
    }
  }
);

export const fetchTaskAnalytics = createAsyncThunk(
  'analytics/fetchTaskAnalytics',
  async (_, { rejectWithValue }) => {
    try {
      const data = await analyticsApi.getTaskAnalytics();
      return data;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to fetch task analytics');
    }
  }
);

export const fetchAllAnalytics = createAsyncThunk(
  'analytics/fetchAllAnalytics',
  async (_, { dispatch }) => {
    await Promise.all([
      dispatch(fetchUserAnalytics()),
      dispatch(fetchTaskAnalytics()),
    ]);
  }
);

const analyticsSlice = createSlice({
  name: 'analytics',
  initialState,
  reducers: {
    clearError: (state) => {
      state.error = null;
    },
  },
  extraReducers: (builder) => {
    // Fetch User Analytics
    builder
      .addCase(fetchUserAnalytics.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchUserAnalytics.fulfilled, (state, action: PayloadAction<AnalyticsResponse>) => {
        state.loading = false;
        state.userAnalytics = action.payload;
      })
      .addCase(fetchUserAnalytics.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string;
      });

    // Fetch Task Analytics
    builder
      .addCase(fetchTaskAnalytics.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchTaskAnalytics.fulfilled, (state, action: PayloadAction<TaskAnalyticsResponse>) => {
        state.loading = false;
        state.taskAnalytics = action.payload;
      })
      .addCase(fetchTaskAnalytics.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string;
      });

    // Fetch All Analytics
    builder
      .addCase(fetchAllAnalytics.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchAllAnalytics.fulfilled, (state) => {
        state.loading = false;
      })
      .addCase(fetchAllAnalytics.rejected, (state, action) => {
        state.loading = false;
        state.error = action.error.message || 'Failed to fetch analytics';
      });
  },
});

export const { clearError } = analyticsSlice.actions;
export default analyticsSlice.reducer;

