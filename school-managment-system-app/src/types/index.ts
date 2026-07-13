export interface User {
  id: string;
  name: string;
  email: string;
  status: 'Active' | 'Inactive' | 'Suspended';
  walletBalance: number;
  joinedDate?: string;
  avatarUrl: string;
}