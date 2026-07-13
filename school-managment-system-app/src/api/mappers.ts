import type { ApiUser } from './types';
import type { User } from '../types';

export function mapApiUserToUser(apiUser: ApiUser): User {
  const name = `${apiUser.firstName} ${apiUser.lastName}`;
  return {
    id: apiUser.id,
    name,
    email: apiUser.email,
    status: apiUser.status,
    walletBalance: apiUser.walletBalance,
    avatarUrl: `https://ui-avatars.com/api/?name=${encodeURIComponent(name)}&background=random`,
  };
}