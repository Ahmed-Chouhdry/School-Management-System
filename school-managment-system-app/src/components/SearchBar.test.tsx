import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { SearchBar } from './SearchBar';
import '@testing-library/jest-dom/vitest';

describe('SearchBar Component', () => {

    it('renders correctly with the initial value and placeholder', () => {
        render(<SearchBar value="Alice" onChange={() => { }} />);

        const inputElement = screen.getByRole('textbox', { name: /search users by name or email/i });
        expect(inputElement).toBeInTheDocument();
        expect(inputElement).toHaveValue('Alice');

        expect(screen.getByPlaceholderText('Search users by name or email...')).toBeInTheDocument();
    });

    it('associates the label correctly with the input for accessibility', () => {
        render(<SearchBar value="" onChange={() => { }} />);

        const labelElement = screen.getByText('Filter System Directory');
        expect(labelElement).toBeInTheDocument();

        const inputElement = screen.getByRole('textbox', { name: /search users by name or email/i });
        expect(inputElement).toHaveAttribute('id', 'user-search');
    });

    it('calls onChange with the correct value when the user types', async () => {
        const handleChange = vi.fn();
        const user = userEvent.setup();

        render(<SearchBar value="" onChange={handleChange} />);

        const inputElement = screen.getByRole('textbox', { name: /search users by name or email/i });

        await user.type(inputElement, 'Bob');

        expect(handleChange).toHaveBeenCalledTimes(3);
        expect(handleChange).toHaveBeenLastCalledWith('b');
    });
});