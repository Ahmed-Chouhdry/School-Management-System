import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { UserDetails } from './UserDetails';
import type { User } from '../types';

const mockUser: User = {
    id: 'usr_123',
    name: 'Jane Doe',
    email: 'jane@example.com',
    avatarUrl: 'https://example.com/avatar.jpg',
    status: 'Active',
    walletBalance: 150.50,
};

describe('UserDetails Component', () => {
    it('renders a placeholder message when no user is selected', () => {
        render(<UserDetails selectedUser={null} onOpenWalletModal={() => { }} />);

        expect(screen.getByText('Select a user to view metrics.')).toBeInTheDocument();

        expect(screen.queryByText('Email Address')).not.toBeInTheDocument();
    });

    it('renders user specifications correctly when a user is provided', () => {
        render(<UserDetails selectedUser={mockUser} onOpenWalletModal={() => { }} />);

        expect(screen.getByRole('heading', { name: 'User Specifications' })).toBeInTheDocument();

        const avatar = screen.getByRole('img', { name: 'Jane Doe' });
        expect(avatar).toBeInTheDocument();
        expect(avatar).toHaveAttribute('src', 'https://example.com/avatar.jpg');

        expect(screen.getByRole('heading', { name: 'Jane Doe' })).toBeInTheDocument();
        expect(screen.getByText('ID: usr_123')).toBeInTheDocument();
        expect(screen.getByText('jane@example.com')).toBeInTheDocument();
        expect(screen.getByText('Active')).toBeInTheDocument();

        expect(screen.getByText('$150.50')).toBeInTheDocument();
    });

    it('calls onOpenWalletModal with the user object when clicking "Modify User Funds"', async () => {
        const handleOpenModal = vi.fn();
        const user = userEvent.setup();

        render(<UserDetails selectedUser={mockUser} onOpenWalletModal={handleOpenModal} />);

        const modifyButton = screen.getByRole('button', { name: 'Modify User Funds' });
        await user.click(modifyButton);

        expect(handleOpenModal).toHaveBeenCalledTimes(1);
        expect(handleOpenModal).toHaveBeenCalledWith(mockUser);
    });
});