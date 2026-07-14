import { createSlice, createAsyncThunk, type PayloadAction } from '@reduxjs/toolkit';
import { api } from '../api/client';
import { mapApiUserToUser } from '../api/mappers';
import type { User } from '../types';

interface UserState {
  list: User[];
  searchTerm: string;
  selectedUserId: string | null;
  activeModalUserId: string | null;
  status: 'idle' | 'loading' | 'succeeded' | 'failed';
  error: string | null;
  walletError: string | null;
}

const initialState: UserState = {
  list: [],
  searchTerm: '',
  selectedUserId: null,
  activeModalUserId: null,
  status: 'idle',
  error: null,
  walletError: null,
};

export const fetchUsers = createAsyncThunk('users/fetchUsers', async () => {
  const apiUsers = await api.getUsers();
  return apiUsers.map(mapApiUserToUser);
});

export const adjustWallet = createAsyncThunk(
  'users/adjustWallet',
  async (
    { userId, amount, reason }: { userId: string; amount: number; reason?: string },
    { rejectWithValue }
  ) => {
    try {
      const result = await api.adjustWallet(userId, amount, reason);
      return { userId, newBalance: result.resultingBalance };
    } catch (err) {
      return rejectWithValue(err instanceof Error ? err.message : 'Adjustment failed');
    }
  }
);

export const fetchUserById = createAsyncThunk(
  'users/fetchUserById',
  async (id: string) => {
    const apiUser = await api.getUserById(id);
    return mapApiUserToUser(apiUser);
  }
);

export const userSlice = createSlice({
  name: 'users',
  initialState,
  reducers: {
    setSearchTerm: (state, action: PayloadAction<string>) => {
      state.searchTerm = action.payload;
    },
    selectUser: (state, action: PayloadAction<string | null>) => {
      state.selectedUserId = action.payload;
    },
    setActiveModalUser: (state, action: PayloadAction<string | null>) => {
      state.activeModalUserId = action.payload;
      state.walletError = null; // clear stale errors when opening/closing modal
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchUsers.pending, (state) => {
        state.status = 'loading';
        state.error = null;
      })
      .addCase(fetchUsers.fulfilled, (state, action) => {
        state.status = 'succeeded';
        state.list = action.payload;
      })
      .addCase(fetchUsers.rejected, (state, action) => {
        state.status = 'failed';
        state.error = action.error.message ?? 'Failed to load users';
      })
      .addCase(adjustWallet.fulfilled, (state, action) => {
        const user = state.list.find((u) => u.id === action.payload.userId);
        if (user) user.walletBalance = action.payload.newBalance;
        state.walletError = null;
      })
      .addCase(adjustWallet.rejected, (state, action) => {
        state.walletError = (action.payload as string) ?? 'Adjustment failed';
      })

      .addCase(fetchUserById.fulfilled, (state, action) => {
        const index = state.list.findIndex((u) => u.id === action.payload.id);
        if (index !== -1) {
          state.list[index] = action.payload; // refresh existing entry
        } else {
          state.list.push(action.payload); // in case it wasn't in the list yet
        }
      });
  },
});

export const { setSearchTerm, selectUser, setActiveModalUser } = userSlice.actions;
export default userSlice.reducer;