import { useEffect, useMemo } from 'react';
import { WalletModal } from './components/WalletModal';
import { SearchBar } from './components/SearchBar';
import { UserTable } from './components/UserTable';
import { UserDetails } from './components/UserDetails';
import { useAppDispatch, useAppSelector } from './store';
import { fetchUserById, fetchUsers, selectUser, setActiveModalUser, setSearchTerm } from './store/userSlice';
import type { User } from './types';

export default function App() {
  const dispatch = useAppDispatch();
  const { list: users, searchTerm, selectedUserId, activeModalUserId, status, error } =
    useAppSelector((state) => state.users);

  useEffect(() => {
    dispatch(fetchUsers());
  }, [dispatch]);

  const handleSelectUser = (user: User) => {
    dispatch(selectUser(user.id));
    dispatch(fetchUserById(user.id)); // always pull fresh detail
  };

  const selectedUser = users.find((u) => u.id === selectedUserId) || null;
  const activeModalUser = users.find((u) => u.id === activeModalUserId) || null;

  const filteredUsers = useMemo(() => {
    return users.filter(
      (user) =>
        user.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
        user.email.toLowerCase().includes(searchTerm.toLowerCase())
    );
  }, [users, searchTerm]);

  return (
    <div>
      <header className="app-header">
        <h1>School Management System</h1>
      </header>

      <main className="app-main">
        <section className="card">
          <SearchBar value={searchTerm} onChange={(val) => dispatch(setSearchTerm(val))} />

          {status === 'loading' && <p>Loading users...</p>}
          {status === 'failed' && <p className="alert error">{error}</p>}

          {status === 'succeeded' && (
            <UserTable
              users={filteredUsers}
              onSelectUser={handleSelectUser}
              onOpenWalletModal={(user) => dispatch(setActiveModalUser(user.id))}
            />
          )}
        </section>

        <UserDetails
          selectedUser={selectedUser}
          onOpenWalletModal={(user) => dispatch(setActiveModalUser(user.id))}
        />
      </main>

      {activeModalUser && (
        <WalletModal user={activeModalUser} onClose={() => dispatch(setActiveModalUser(null))} />
      )}
    </div>
  );
}