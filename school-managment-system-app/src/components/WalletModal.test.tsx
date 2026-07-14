import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { WalletModal } from './WalletModal';
import { adjustWallet } from '../store/userSlice';
import type { User } from '../types';

const mockDispatch = vi.fn();

vi.mock('../store', () => ({
  useAppDispatch: () => mockDispatch,
  useAppSelector: (selectorFn: any) => selectorFn({ users: { walletError: null } }),
}));

vi.mock('../store/userSlice', () => ({
  adjustWallet: vi.fn(),
}));

const mockUser: User = {
  id: 'usr_abc',
  name: 'Alex Rivera',
  email: 'alex@example.com',
  avatarUrl: 'https://example.com/alex.jpg',
  status: 'Active',
  walletBalance: 100.00,
};

describe('WalletModal Component', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders correctly with default UI values', () => {
    render(<WalletModal user={mockUser} onClose={() => {}} />);

    expect(screen.getByRole('dialog')).toBeInTheDocument();
    expect(screen.getByRole('heading', { name: 'Adjust Wallet Balance' })).toBeInTheDocument();
    expect(screen.getByText('Alex Rivera')).toBeInTheDocument();
    expect(screen.getByLabelText('Amount ($)')).toHaveValue(null);
  });

  it('validates and shows error for invalid, empty, or negative inputs', async () => {
    const user = userEvent.setup();
    render(<WalletModal user={mockUser} onClose={() => {}} />);

    const submitBtn = screen.getByRole('button', { name: 'Confirm' });

    await user.click(submitBtn);
    expect(screen.getByText('Please enter a valid amount greater than 0.')).toBeInTheDocument();

    const input = screen.getByLabelText('Amount ($)');
    await user.type(input, '-10');
    await user.click(submitBtn);
    expect(screen.getByText('Please enter a valid amount greater than 0.')).toBeInTheDocument();
  });

  it('validates and shows error if deduction exceeds current balance', async () => {
    const user = userEvent.setup();
    render(<WalletModal user={mockUser} onClose={() => {}} />);

    const deductBtn = screen.getByRole('button', { name: 'Deduct Funds' });
    await user.click(deductBtn);

    const input = screen.getByLabelText('Amount ($)');
    await user.type(input, '150');

    const submitBtn = screen.getByRole('button', { name: 'Confirm' });
    await user.click(submitBtn);

    expect(screen.getByText('This will exceed the current balance of $100.00.')).toBeInTheDocument();
  });

  it('dispatches adjustWallet action on successful Add validation', async () => {
    const user = userEvent.setup();
    
    const mockFulfilledThunk = { type: 'users/adjustWallet/fulfilled' };
    const mockResult = { payload: {} };

    vi.mocked(adjustWallet).mockReturnValue(mockFulfilledThunk as any);

    mockDispatch.mockResolvedValue(mockResult);
    adjustWallet.fulfilled = {
      match: (action: any) => action === mockResult
    } as any;

    render(<WalletModal user={mockUser} onClose={() => {}} />);

    const input = screen.getByLabelText('Amount ($)');
    await user.type(input, '50.25');

    const submitBtn = screen.getByRole('button', { name: 'Confirm' });
    await user.click(submitBtn);

    expect(adjustWallet).toHaveBeenCalledWith({
      userId: 'usr_abc',
      amount: 50.25,
      reason: 'add via portal',
    });
    
    expect(mockDispatch).toHaveBeenCalledWith(mockFulfilledThunk);
    expect(screen.getByText('Successfully added $50.25!')).toBeInTheDocument();
  });

  it('calls onClose when close buttons are clicked', async () => {
    const handleClose = vi.fn();
    const user = userEvent.setup();

    render(<WalletModal user={mockUser} onClose={handleClose} />);

    const closeBtn = screen.getByRole('button', { name: 'Close modal' });
    await user.click(closeBtn);

    expect(handleClose).toHaveBeenCalledTimes(1);
  });
});