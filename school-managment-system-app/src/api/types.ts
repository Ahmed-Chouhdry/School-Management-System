// Shape returned by the backend (matches UserDto)
export interface ApiUser {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  role: 'Student' | 'Teacher' | 'Admin';
  walletBalance: number;
  status: 'Active' | 'Inactive' | 'Suspended';
}

export interface WalletAdjustmentResponse {
  id: string;
  userId: string;
  amount: number;
  resultingBalance: number;
  reason: string | null;
  createdAtUtc: string;
}

export interface ApiError {
  error: string;
}