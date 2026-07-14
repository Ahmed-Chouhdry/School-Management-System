import { describe, it, expect, vi, beforeEach } from 'vitest';
import { api } from './client';
import type { ApiUser, WalletAdjustmentResponse } from './types';

describe('API Client Service', () => {
  const fetchMock = vi.fn();
  
  beforeEach(() => {
    vi.stubGlobal('fetch', fetchMock);
    vi.clearAllMocks();
  });

  describe('getUsers', () => {
    it('returns user data on a successful response', async () => {
      const mockUsers: ApiUser[] = [
        { id: '1', firstName: 'Alice', lastName: 'Vance', email: 'alice@test.com', status: 'Active', walletBalance: 100, role: 'Student' },
      ];

      // Simulate a successful JSON response
      fetchMock.mockResolvedValueOnce({
        ok: true,
        status: 200,
        json: async () => mockUsers,
      });

      const result = await api.getUsers();

      expect(fetchMock).toHaveBeenCalledWith('https://localhost:7100/users');
      expect(result).toEqual(mockUsers);
    });
  });

  describe('getUserById', () => {
    it('returns a single user on a successful response', async () => {
      const mockUser: ApiUser = { 
        id: '1', firstName: 'Alice', lastName: 'Vance', email: 'alice@test.com', status: 'Active', walletBalance: 100, role: 'Student'
      };

      fetchMock.mockResolvedValueOnce({
        ok: true,
        status: 200,
        json: async () => mockUser,
      });

      const result = await api.getUserById('1');

      expect(fetchMock).toHaveBeenCalledWith('https://localhost:7100/users/1');
      expect(result).toEqual(mockUser);
    });
  });

  describe('adjustWallet', () => {
    it('sends a POST request with correct headers and payload', async () => {

     const mockResponse: WalletAdjustmentResponse = {
        id: 'tx_999',
        resultingBalance: 150.00,
        userId: 'usr_123',
        amount: 50.00,
        reason: 'add via portal',
        createdAtUtc: new Date().toISOString()
      };

      fetchMock.mockResolvedValueOnce({
        ok: true,
        status: 200,
        json: async () => mockResponse,
      });

      const result = await api.adjustWallet('usr_123', 50, 'add via portal');

      expect(fetchMock).toHaveBeenCalledWith(
        'https://localhost:7100/users/usr_123/wallet-adjustments',
        {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ amount: 50, reason: 'add via portal' }),
        }
      );
      expect(result).toEqual(mockResponse);
    });
  });

  describe('handleResponse (Error Handling)', () => {
    it('throws custom error message parsed from JSON body when available', async () => {
      // Simulate an API error structure with a JSON payload
      fetchMock.mockResolvedValueOnce({
        ok: false,
        status: 400,
        json: async () => ({ error: 'Insufficient wallet balance' }),
      });

      await expect(api.getUserById('1')).rejects.toThrow('Insufficient wallet balance');
    });

    it('falls back to status code message when error payload is not JSON', async () => {
      // Simulate an HTML or crash page error response from the server
      fetchMock.mockResolvedValueOnce({
        ok: false,
        status: 500,
        json: async () => {
          throw new Error('Not JSON content'); // Triggering catch block in handleResponse
        },
      });

      await expect(api.getUserById('1')).rejects.toThrow('Request failed with status 500');
    });
  });
});