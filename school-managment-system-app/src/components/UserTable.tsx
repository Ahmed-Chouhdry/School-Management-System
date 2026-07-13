import React from 'react';
import type { User } from '../types';


interface UserTableProps {
  users: User[];
  onSelectUser: (user: User) => void;
  onOpenWalletModal: (user: User) => void;
}

export const UserTable: React.FC<UserTableProps> = ({ users, onSelectUser, onOpenWalletModal }) => {
  return (
    <div className="table-wrapper">
      <table className="user-table">
        <thead>
          <tr>
            <th>User</th>
            <th>Status</th>
            <th>Wallet Balance</th>
            <th style={{ textAlign: 'right' }}>Actions</th>
          </tr>
        </thead>
        <tbody>
          {users.length > 0 ? (
            users.map((user) => (
              <tr key={user.id}>
                <td>
                  <div className="user-cell">
                    <img className="avatar" src={user.avatarUrl} alt="" />
                    <div>
                      <div className="username">{user.name}</div>
                      <div className="useremail">{user.email}</div>
                    </div>
                  </div>
                </td>
                <td>
                  <span className={`badge ${user.status.toLowerCase()}`}>
                    {user.status}
                  </span>
                </td>
                <td style={{ fontFamily: 'monospace', fontWeight: 'bold' }}>
                  ${user.walletBalance.toFixed(2)}
                </td>
                <td style={{ textAlign: 'right' }}>
                  <button 
                    onClick={() => onSelectUser(user)} 
                    className="btn-text"
                  >
                    View Details
                  </button>
                  <button 
                    onClick={() => onOpenWalletModal(user)} 
                    className="btn-secondary"
                  >
                    Adjust
                  </button>
                </td>
              </tr>
            ))
          ) : (
            <tr>
              <td colSpan={4} style={{ textAlign: 'center', color: 'var(--text-muted)', padding: '32px' }}>
                No matching user records found.
              </td>
            </tr>
          )}
        </tbody>
      </table>
    </div>
  );
};