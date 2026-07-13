import React from 'react';
import type { User } from '../types';


interface UserDetailsProps {
  selectedUser: User | null;
  onOpenWalletModal: (user: User) => void;
}

export const UserDetails: React.FC<UserDetailsProps> = ({ selectedUser, onOpenWalletModal }) => {
  return (
    <section className="card details-panel">
      <h2>User Specifications</h2>
      {selectedUser ? (
        <div>
          <div className="user-cell" style={{ marginBottom: '20px' }}>
            <img 
              className="avatar" 
              src={selectedUser.avatarUrl} 
              alt={selectedUser.name} 
              style={{ width: '48px', height: '48px' }} 
            />
            <div>
              <h3 style={{ margin: 0 }}>{selectedUser.name}</h3>
              <p style={{ margin: 0, fontSize: '0.75rem', color: 'var(--text-muted)' }}>
                ID: {selectedUser.id}
              </p>
            </div>
          </div>

          <div className="detail-group">
            <span className="detail-label">Email Address</span>
            <span style={{ fontSize: '0.9rem' }}>{selectedUser.email}</span>
          </div>

          <div className="detail-group">
            <span className="detail-label">Profile Status</span>
            <span className={`badge ${selectedUser.status.toLowerCase()}`}>
              {selectedUser.status}
            </span>
          </div>

          <div className="detail-group" style={{ marginTop: '16px' }}>
            <span className="detail-label">Current Account Balance</span>
            <span style={{ fontSize: '1.2rem', fontFamily: 'monospace', fontWeight: 'bold', color: 'var(--primary)' }}>
              ${selectedUser.walletBalance.toFixed(2)}
            </span>
          </div>

          <button 
            onClick={() => onOpenWalletModal(selectedUser)} 
            className="btn-primary" 
            style={{ marginTop: '12px' }}
          >
            Modify User Funds
          </button>
        </div>
      ) : (
        <p style={{ textAlign: 'center', color: 'var(--text-muted)', fontSize: '0.9rem', paddingTop: '24px' }}>
          Select a user to view metrics.
        </p>
      )}
    </section>
  );
};