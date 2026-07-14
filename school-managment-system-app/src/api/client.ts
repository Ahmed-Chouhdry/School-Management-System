const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'https://localhost:7100';

async function handleResponse<T>(res: Response): Promise<T> {
  if (!res.ok) {
    let message = `Request failed with status ${res.status}`;
    try {
      const body = await res.json();
      if (body?.error) message = body.error;
    } catch {
    }
    throw new Error(message);
  }
  return res.json() as Promise<T>;
}

export const api = {
  getUsers: () =>
    fetch(`${API_BASE_URL}/users`).then((res) => handleResponse<ApiUser[]>(res)),

  getUserById: (id: string) =>
    fetch(`${API_BASE_URL}/users/${id}`).then((res) => handleResponse<ApiUser>(res)),

  adjustWallet: (userId: string, amount: number, reason?: string) =>
    fetch(`${API_BASE_URL}/users/${userId}/wallet-adjustments`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ amount, reason }),
    }).then((res) => handleResponse<WalletAdjustmentResponse>(res)),
};

import type { ApiUser, WalletAdjustmentResponse } from './types';