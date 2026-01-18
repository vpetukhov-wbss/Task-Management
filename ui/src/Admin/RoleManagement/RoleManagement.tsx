import React, { useEffect, useState } from 'react';
import { Plus, Edit2, Trash2, ChevronLeft, ChevronRight, Search } from 'react-feather';
import { useAppDispatch, useAppSelector } from '../../store/hooks';
import { fetchRoles, deleteRole, clearError, setPageNumber, setSearchTerm, setSortBy, setSortAscending } from '../../store/slices/rolesSlice';
import RoleModal from './RoleModal';
import { Role } from '../../api/roles.api';
import './RoleManagement.css';

function RoleManagement() {
  const dispatch = useAppDispatch();
  const {
    roles,
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
  } = useAppSelector((state) => state.roles);
  const [showModal, setShowModal] = useState(false);
  const [selectedRole, setSelectedRole] = useState<Role | null>(null);
  const [localSearchTerm, setLocalSearchTerm] = useState(searchTerm);

  useEffect(() => {
    dispatch(fetchRoles({ pageNumber, pageSize, searchTerm, sortBy, sortAscending }));
  }, [dispatch, pageNumber, pageSize, searchTerm, sortBy, sortAscending]);

  const handleAddRole = () => {
    setSelectedRole(null);
    setShowModal(true);
  };

  const handleEditRole = (role: Role) => {
    setSelectedRole(role);
    setShowModal(true);
  };

  const handleDeleteRole = async (roleId: string) => {
    if (window.confirm('Are you sure you want to delete this role?')) {
      await dispatch(deleteRole(roleId));
      dispatch(fetchRoles({ pageNumber, pageSize, searchTerm, sortBy, sortAscending }));
    }
  };

  const handleCloseModal = () => {
    setShowModal(false);
    setSelectedRole(null);
  };

  const handleSuccess = () => {
    handleCloseModal();
    dispatch(fetchRoles({ pageNumber, pageSize, searchTerm, sortBy, sortAscending }));
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
    dispatch(setPageNumber(1));
  };

  const handleSort = (column: string) => {
    if (sortBy === column) {
      dispatch(setSortAscending(!sortAscending));
    } else {
      dispatch(setSortBy(column));
      dispatch(setSortAscending(true));
    }
    dispatch(setPageNumber(1));
  };

  if (isLoading) {
    return <div className="loading-message">Loading roles...</div>;
  }

  return (
    <div className="role-management">
      <div className="role-management-header">
        <h2>Role Management</h2>
        <button className="add-role-button" onClick={handleAddRole}>
          <Plus size={18} />
          Add Role
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
            placeholder="Search by role name..."
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
          Showing {roles.length > 0 ? ((pageNumber - 1) * pageSize + 1) : 0} - {Math.min(pageNumber * pageSize, totalCount)} of {totalCount} roles
        </div>
      </div>

      {roles.length === 0 ? (
        <div className="no-roles-message">No roles found</div>
      ) : (
        <>
          <div className="roles-table-container">
            <table className="roles-table">
              <thead>
                <tr>
                  <th onClick={() => handleSort('name')} style={{ cursor: 'pointer' }}>
                    Role Name {sortBy === 'name' && (sortAscending ? '↑' : '↓')}
                  </th>
                  <th>User Count</th>
                  <th>Actions</th>
                </tr>
              </thead>


            <tbody>
              {roles.map((role) => (
                <tr key={role.id}>
                  <td>
                    <span className="role-name">{role.name}</span>
                  </td>
                  <td>
                    <span className="user-count-badge">{role.userCount} users</span>
                  </td>
                  <td>
                    <div className="action-buttons">
                      <button
                        className="action-button edit"
                        onClick={() => handleEditRole(role)}
                      >
                        <Edit2 size={14} />
                        Edit
                      </button>
                      <button
                        className="action-button delete"
                        onClick={() => handleDeleteRole(role.id)}
                        disabled={role.userCount > 0}
                        title={role.userCount > 0 ? 'Cannot delete role with assigned users' : 'Delete role'}
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
        <RoleModal
          role={selectedRole}
          onClose={handleCloseModal}
          onSuccess={handleSuccess}
        />
      )}
    </div>
  );
}

export default RoleManagement;
