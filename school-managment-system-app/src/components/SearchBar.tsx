import React from 'react';

interface SearchBarProps {
  value: string;
  onChange: (newValue: string) => void;
}

export const SearchBar: React.FC<SearchBarProps> = ({ value, onChange }) => {
  return (
    <div className="search-container">
      <label htmlFor="user-search" className="detail-label" style={{ marginBottom: '8px' }}>
        Filter System Directory
      </label>
      <input
        id="user-search"
        type="text"
        placeholder="Search users by name or email..."
        value={value}
        onChange={(e) => onChange(e.target.value)}
        className="search-input"
        aria-label="Search users by name or email"
      />
    </div>
  );
};