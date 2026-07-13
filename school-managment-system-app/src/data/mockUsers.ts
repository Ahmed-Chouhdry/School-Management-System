import type { User } from "../types";


export const mockUsers: User[] = [
  {
    id: '1',
    name: 'Alex Rivera',
    email: 'alex.rivera@example.com',
    status: 'Active',
    walletBalance: 125.50,
    joinedDate: '2024-03-15',
    avatarUrl: 'https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=100&h=100&fit=crop&crop=faces'
  },
  {
    id: '2',
    name: 'Marcus Chen',
    email: 'marcus.chen@example.com',
    status: 'Active',
    walletBalance: 42.00,
    joinedDate: '2023-11-02',
    avatarUrl: 'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=100&h=100&fit=crop&crop=faces'
  },
  {
    id: '3',
    name: 'Sarah Jenkins',
    email: 'sarah.j@example.com',
    status: 'Inactive',
    walletBalance: 0.00,
    joinedDate: '2025-01-20',
    avatarUrl: 'https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=100&h=100&fit=crop&crop=faces'
  }
];