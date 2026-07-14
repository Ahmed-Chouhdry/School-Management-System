import { describe, it, expect } from 'vitest';
import { mapApiUserToUser } from './mappers';
import type { ApiUser } from './types';

describe('mapApiUserToUser Mapper', () => {
  it('should correctly map a standard ApiUser to a User object', () => {
    const mockApiUser: ApiUser = {
      id: 'usr_abc123',
      firstName: 'John',
      lastName: 'Doe',
      email: 'john.doe@example.com',
      status: 'Active',
      walletBalance: 125.50,
      role: 'Student',
    };

    const result = mapApiUserToUser(mockApiUser);

    expect(result).toEqual({
      id: 'usr_abc123',
      name: 'John Doe',
      email: 'john.doe@example.com',
      status: 'Active',
      walletBalance: 125.50,
      avatarUrl: 'https://ui-avatars.com/api/?name=John%20Doe&background=random',
    });
  });

  it('should correctly encode special characters in the name for the avatarUrl', () => {
    const mockApiUser: ApiUser = {
      id: 'usr_special',
      firstName: 'René',
      lastName: 'd\'Anjou',
      email: 'rene@example.com',
      status: 'Active',
      walletBalance: 0.00,
      role: 'Teacher',
    };

    const result = mapApiUserToUser(mockApiUser);

    expect(result.name).toBe("René d'Anjou");
    
  });

  it('should handle empty or whitespace names gracefully', () => {
    const mockApiUser: ApiUser = {
      id: 'usr_empty',
      firstName: '',
      lastName: '',
      email: 'anonymous@example.com',
      status: 'Suspended',
      walletBalance: -10.00,
      role: 'Teacher',
    };

    const result = mapApiUserToUser(mockApiUser);

    expect(result.name).toBe(' ');
    expect(result.avatarUrl).toBe('https://ui-avatars.com/api/?name=%20&background=random');
  });
});