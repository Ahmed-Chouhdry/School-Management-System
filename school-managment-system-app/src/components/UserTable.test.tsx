import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { UserTable } from './UserTable';
import type { User } from '../types';

const mockUsers: User[] = [
    {
        id: 'usr_1',
        name: 'Alice Vance',
        email: 'alice@example.com',
        avatarUrl: 'https://example.com/alice.jpg',
        status: 'Active',
        walletBalance: 2500.00,
    },
    {
        id: 'usr_2',
        name: 'Charlie Smith',
        email: 'charlie@example.com',
        avatarUrl: 'https://example.com/charlie.jpg',
        status: 'Suspended',
        walletBalance: 0.50,
    }
];

describe('UserTable Component', () => {
    it('renders fallback empty state when no users are provided', () => {
        render(
            <table>
                <UserTable users={[]} onSelectUser={() => { }} onOpenWalletModal={() => { }} />
            </table>
        );

        expect(screen.getByText('No matching user records found.')).toBeInTheDocument();
    });

    it('renders a list of users with proper values and structure', () => {
        render(
            <table>
                <UserTable
                    users={mockUsers}
                    onSelectUser={() => { }}
                    onOpenWalletModal={() => { }}
                />
            </table>
        );

        expect(screen.getByRole('columnheader', { name: 'User' })).toBeInTheDocument();
        expect(screen.getByRole('columnheader', { name: 'Status' })).toBeInTheDocument();
        expect(screen.getByRole('columnheader', { name: 'Wallet Balance' })).toBeInTheDocument();

        expect(screen.getByText('Alice Vance')).toBeInTheDocument();
        expect(screen.getByText('alice@example.com')).toBeInTheDocument();
        expect(screen.getByText('Active')).toBeInTheDocument();
        expect(screen.getByText('$2500.00')).toBeInTheDocument();

        expect(screen.getByText('Charlie Smith')).toBeInTheDocument();
        expect(screen.getByText('charlie@example.com')).toBeInTheDocument();
        expect(screen.getByText('Suspended')).toBeInTheDocument();
        expect(screen.getByText('$0.50')).toBeInTheDocument();
    });

    it('calls onSelectUser when "View Details" is clicked', async () => {
        const handleSelectUser = vi.fn();
        const user = userEvent.setup();

        render(
            <table>
                <UserTable
                    users={mockUsers}
                    onSelectUser={handleSelectUser}
                    onOpenWalletModal={() => { }}
                />
            </table>
        );

        const viewButtons = screen.getAllByRole('button', { name: 'View Details' });
        await user.click(viewButtons[1]);

        expect(handleSelectUser).toHaveBeenCalledTimes(1);
        expect(handleSelectUser).toHaveBeenCalledWith(mockUsers[1]);
    });

    it('calls onOpenWalletModal when "Adjust" is clicked', async () => {
        const handleOpenWalletModal = vi.fn();
        const user = userEvent.setup();

        render(
            <table>
                <UserTable
                    users={mockUsers}
                    onSelectUser={() => { }}
                    onOpenWalletModal={handleOpenWalletModal}
                />
            </table>
        );

        const adjustButtons = screen.getAllByRole('button', { name: 'Adjust' });
        await user.click(adjustButtons[0]);

        expect(handleOpenWalletModal).toHaveBeenCalledTimes(1);
        expect(handleOpenWalletModal).toHaveBeenCalledWith(mockUsers[0]);
    });
});