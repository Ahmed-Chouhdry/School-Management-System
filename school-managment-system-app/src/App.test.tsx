// @vitest-environment jsdom

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import App from './App';
import { 
  fetchUsers, 
  fetchUserById, 
  selectUser, 
} from './store/userSlice';
import type { User } from './types';

const mockDispatch = vi.fn();


let mockState = {
  users: {
    list: [] as User[],
    searchTerm: '',
    selectedUserId: null as string | null,
    activeModalUserId: null as string | null,
    status: 'idle',
    error: null as string | null,
  }
};

vi.mock('./store', () => ({
  useAppDispatch: () => mockDispatch,
  useAppSelector: (selectorFn: any) => selectorFn(mockState),
}));

vi.mock('./store/userSlice', () => ({
  fetchUsers: vi.fn(() => ({ type: 'users/fetchUsers' })),
  fetchUserById: vi.fn(() => ({ type: 'users/fetchUserById' })),
  selectUser: vi.fn((id) => ({ type: 'users/selectUser', payload: id })),
  setActiveModalUser: vi.fn((id) => ({ type: 'users/setActiveModalUser', payload: id })),
  setSearchTerm: vi.fn((term) => ({ type: 'users/setSearchTerm', payload: term })),
}));

const mockUsersList: User[] = [
  {
    id: 'usr_1',
    name: 'Sarah Connor',
    email: 'sarah@resistance.com',
    avatarUrl: 'https://example.com/sarah.jpg',
    status: 'Active',
    walletBalance: 450.00,
  },
  {
    id: 'usr_2',
    name: 'John Connor',
    email: 'john@resistance.com',
    avatarUrl: 'https://example.com/john.jpg',
    status: 'Active',
    walletBalance: 1000.00,
  }
];

describe('App Component (Integration)', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockState = {
      users: {
        list: [],
        searchTerm: '',
        selectedUserId: null,
        activeModalUserId: null,
        status: 'idle',
        error: null,
      }
    };
  });

  it('dispatches fetchUsers immediately on mount', () => {
    render(<App />);
    expect(mockDispatch).toHaveBeenCalledWith(fetchUsers());
  });

  it('renders the loading state correctly', () => {
    mockState.users.status = 'loading';
    render(<App />);

    expect(screen.getByText('Loading users...')).toBeInTheDocument();
  });

  it('renders the failed state and shows the error message', () => {
    mockState.users.status = 'failed';
    mockState.users.error = 'Failed to sync database records.';
    render(<App />);

    expect(screen.getByText('Failed to sync database records.')).toBeInTheDocument();
  });

  it('renders the user list and default detail panel on successful fetch', () => {
    mockState.users.status = 'succeeded';
    mockState.users.list = mockUsersList;

    render(<App />);

    expect(screen.getByText('Sarah Connor')).toBeInTheDocument();
    expect(screen.getByText('John Connor')).toBeInTheDocument();

    expect(screen.getByText('Select a user to view metrics.')).toBeInTheDocument();
  });

  it('filters the user list based on the search term', () => {
    mockState.users.status = 'succeeded';
    mockState.users.list = mockUsersList;
    mockState.users.searchTerm = 'Sarah';

    render(<App />);

    expect(screen.getByText('Sarah Connor')).toBeInTheDocument();
    expect(screen.queryByText('John Connor')).not.toBeInTheDocument();
  });

  it('dispatches the correct actions when selecting a user', async () => {
    const user = userEvent.setup();
    mockState.users.status = 'succeeded';
    mockState.users.list = mockUsersList;

    render(<App />);

    const viewDetailsButton = screen.getAllByRole('button', { name: 'View Details' })[0]; // Sarah
    await user.click(viewDetailsButton);

    expect(mockDispatch).toHaveBeenCalledWith(selectUser('usr_1'));
    expect(mockDispatch).toHaveBeenCalledWith(fetchUserById('usr_1'));
  });

  it('renders the WalletModal when activeModalUserId is set', () => {
    mockState.users.status = 'succeeded';
    mockState.users.list = mockUsersList;
    mockState.users.activeModalUserId = 'usr_2';

    render(<App />);

    expect(screen.getByRole('dialog')).toBeInTheDocument();
    expect(screen.getByRole('heading', { name: 'Adjust Wallet Balance' })).toBeInTheDocument();
  });
});