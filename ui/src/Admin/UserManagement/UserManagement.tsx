import React, { useEffect, useState } from 'react';
import { Plus, Edit2, Trash2, ChevronLeft, ChevronRight, Search } from 'react-feather';
import { useAppDispatch, useAppSelector } from '../../store/hooks';
import { fetchUsers, deleteUser, clearError, setPageNumber, setSearchTerm, setSortBy, setSortAscending } from '../../store/slices/usersSlice';
import UserModal from './UserModal';
import { User } from '../../api/users.api';
import './UserManagement.css';

function UserManagement() {
  const dispatch = useAppDispatch();
  const {
    users,
    isLoading,
    error,
    pageNumber,
    pageSize,
    totalCount,
    totalPages,
    hasPreviousPage,
    hasNextPage,
    searchTerm,
    sortBy,
    sortAscending
  } = useAppSelector((state) => state.users);
  const [showModal, setShowModal] = useState(false);
  const [selectedUser, setSelectedUser] = useState<User | null>(null);
  const [localSearchTerm, setLocalSearchTerm] = useState(searchTerm);

  useEffect(() => {
    dispatch(fetchUsers({ pageNumber, pageSize, searchTerm, sortBy, sortAscending }));
  }, [dispatch, pageNumber, pageSize, searchTerm, sortBy, sortAscending]);

  const handleAddUser = () => {
    setSelectedUser(null);
    setShowModal(true);
  };

  const handleEditUser = (user: User) => {
    setSelectedUser(user);
    setShowModal(true);
  };

  const handleDeleteUser = async (userId: string) => {
    if (window.confirm('Are you sure you want to delete this user?')) {
      await dispatch(deleteUser(userId));
      dispatch(fetchUsers({ pageNumber, pageSize, searchTerm, sortBy, sortAscending }));
    }
  };

  const handleCloseModal = () => {
    setShowModal(false);
    setSelectedUser(null);
    dispatch(clearError());
  };

  const handleSuccess = () => {
    setShowModal(false);
    setSelectedUser(null);
    dispatch(fetchUsers({ pageNumber, pageSize, searchTerm, sortBy, sortAscending }));
  };

  const handlePreviousPage = () => {
    if (hasPreviousPage) {
      dispatch(setPageNumber(pageNumber - 1));
    }
  };

  const handleNextPage = () => {
    if (hasNextPage) {
      dispatch(setPageNumber(pageNumber + 1));
    }
  };

  const handleSearch = () => {
    dispatch(setSearchTerm(localSearchTerm));
    dispatch(setPageNumber(1)); // Reset to first page on search
  };

  const handleSort = (column: string) => {
    if (sortBy === column) {
      dispatch(setSortAscending(!sortAscending));
    } else {
      dispatch(setSortBy(column));
      dispatch(setSortAscending(true));
    }
    dispatch(setPageNumber(1)); // Reset to first page on sort
  };

  const getRoleBadgeClass = (role: string) => {
    switch (role.toLowerCase()) {
      case 'admin':
        return 'admin';
      case 'manager':
        return 'manager';
      default:
        return 'user';
    }
  };

  if (isLoading) {
    return <div className="loading-message">Loading users...</div>;
  }

  return (
    <div className="user-management">
      <div className="user-management-header">
        <h2>User Management</h2>
        <button className="add-user-button" onClick={handleAddUser}>
          <Plus size={18} />
          Add User
        </button>
      </div>

      {error && (
        <div className="error-message">
          {error}
          <button onClick={() => dispatch(clearError())} style={{ marginLeft: '10px', cursor: 'pointer' }}>
            ✕
          </button>
        </div>
      )}

      <div className="search-bar">
        <div className="search-input-container">
          <Search size={18} />
          <input
            type="text"
            placeholder="Search by email..."
            value={localSearchTerm}
            onChange={(e) => setLocalSearchTerm(e.target.value)}
            onKeyPress={(e) => e.key === 'Enter' && handleSearch()}
            className="search-input"
          />
          <button onClick={handleSearch} className="search-button">
            Search
          </button>
        </div>
        <div className="pagination-info">
          Showing {users.length > 0 ? ((pageNumber - 1) * pageSize + 1) : 0} - {Math.min(pageNumber * pageSize, totalCount)} of {totalCount} users
        </div>
      </div>

      {users.length === 0 ? (
        <div className="no-users-message">No users found</div>
      ) : (
        <>
          <div className="users-table-container">
            <table className="users-table">
              <thead>
                <tr>
                  <th onClick={() => handleSort('email')} style={{ cursor: 'pointer' }}>
                    Email {sortBy === 'email' && (sortAscending ? '↑' : '↓')}
                  </th>
                  <th>Role</th>
                  <th onClick={() => handleSort('createdAtUtc')} style={{ cursor: 'pointer' }}>
                    Created At {sortBy === 'createdAtUtc' && (sortAscending ? '↑' : '↓')}
                  </th>
                  <th>Actions</th>
                </tr>
              </thead>
            <tbody>
              {users.map((user) => (
                <tr key={user.id}>
                  <td>{user.email}</td>
                  <td>
                    <span className={`role-badge ${getRoleBadgeClass(user.role)}`}>
                      {user.role}
                    </span>
                  </td>
                  <td>
                    {user.createdAtUtc
                      ? new Date(user.createdAtUtc).toLocaleDateString()
                      : 'N/A'}
                  </td>
                  <td>
                    <div className="action-buttons">
                      <button
                        className="action-button edit"
                        onClick={() => handleEditUser(user)}
                      >
                        <Edit2 size={14} />
                        Edit
                      </button>
                      <button
                        className="action-button delete"
                        onClick={() => handleDeleteUser(user.id)}
                      >
                        <Trash2 size={14} />
                        Delete
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        <div className="pagination-controls">
          <button
            onClick={handlePreviousPage}
            disabled={!hasPreviousPage}
            className="pagination-button"
          >
            <ChevronLeft size={18} />
            Previous
          </button>

          <div className="pagination-pages">
            <span>Page {pageNumber} of {totalPages}</span>
          </div>

          <button
            onClick={handleNextPage}
            disabled={!hasNextPage}
            className="pagination-button"
          >
            Next
            <ChevronRight size={18} />
          </button>
        </div>
        </>
      )}

      {showModal && (
        <UserModal
          user={selectedUser}
          onClose={handleCloseModal}
          onSuccess={handleSuccess}
        />
      )}
    </div>
  );
}

export default UserManagement;

