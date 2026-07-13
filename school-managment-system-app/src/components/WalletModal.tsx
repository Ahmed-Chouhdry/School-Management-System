import React, { useState } from 'react';
import type { User } from '../types';
import { useAppDispatch, useAppSelector } from '../store';
import { adjustWallet } from '../store/userSlice';

interface WalletModalProps {
  user: User;
  onClose: () => void;
}

export const WalletModal: React.FC<WalletModalProps> = ({ user, onClose }) => {
  const dispatch = useAppDispatch();
  const walletError = useAppSelector((state) => state.users.walletError);

  const [amount, setAmount] = useState<string>('');
  const [action, setAction] = useState<'add' | 'deduct'>('add');
  const [localError, setLocalError] = useState<string | null>(null);
  const [feedback, setFeedback] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  const handleValidateAndSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLocalError(null);
    setFeedback(null);

    const parsedAmount = parseFloat(amount);

    if (isNaN(parsedAmount) || parsedAmount <= 0) {
      setLocalError('Please enter a valid amount greater than 0.');
      return;
    }

    // Client-side hint only — the backend is the real source of truth
    // for the negative-balance rule, so we still send the request either way
    // and surface the server's rejection if it disagrees.
    if (action === 'deduct' && parsedAmount > user.walletBalance) {
      setLocalError(`This will exceed the current balance of $${user.walletBalance.toFixed(2)}.`);
      return;
    }

    const signedAmount = action === 'add' ? parsedAmount : -parsedAmount;

    setSubmitting(true);
    const result = await dispatch(
      adjustWallet({ userId: user.id, amount: signedAmount, reason: `${action} via portal` })
    );
    setSubmitting(false);

    if (adjustWallet.fulfilled.match(result)) {
      setFeedback(`Successfully ${action === 'add' ? 'added' : 'deducted'} $${parsedAmount.toFixed(2)}!`);
      setAmount('');
    }
    // on rejection, walletError from Redux state will render below
  };

  return (
    <div className="modal-overlay" role="dialog" aria-modal="true" aria-labelledby="modal-title">
      <div className="modal-content">
        <div className="modal-header">
          <h3 id="modal-title">Adjust Wallet Balance</h3>
          <button onClick={onClose} className="close-btn" aria-label="Close modal">×</button>
        </div>

        <p style={{ fontSize: '0.9rem', margin: '0 0 16px 0' }}>
          User: <strong>{user.name}</strong>
        </p>

        {feedback && <div className="alert success">{feedback}</div>}
        {walletError && <div className="alert error">{walletError}</div>}

        <form onSubmit={handleValidateAndSubmit}>
          <div style={{ marginBottom: '16px' }}>
            <span className="detail-label">Adjustment Type</span>
            <div className="btn-grid">
              <button type="button" onClick={() => setAction('add')} className={`btn-toggle ${action === 'add' ? 'active' : ''}`}>
                Add Funds
              </button>
              <button type="button" onClick={() => setAction('deduct')} className={`btn-toggle ${action === 'deduct' ? 'active' : ''}`}>
                Deduct Funds
              </button>
            </div>
          </div>

          <div style={{ marginBottom: '20px' }}>
            <label htmlFor="amount" className="detail-label">Amount ($)</label>
            <input
              id="amount"
              type="number"
              step="0.01"
              placeholder="0.00"
              value={amount}
              onChange={(e) => setAmount(e.target.value)}
              className="search-input"
              style={{ borderColor: localError ? 'var(--error-text)' : '' }}
            />
            {localError && <div className="alert error" style={{ marginTop: '8px', marginBottom: 0 }}>{localError}</div>}
          </div>

          <div style={{ display: 'flex', gap: '8px', justifyContent: 'flex-end' }}>
            <button type="button" onClick={onClose} className="btn-secondary" style={{ padding: '10px 16px' }}>Close</button>
            <button type="submit" className="btn-primary" style={{ width: 'auto' }} disabled={submitting}>
              {submitting ? 'Submitting...' : 'Confirm'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};