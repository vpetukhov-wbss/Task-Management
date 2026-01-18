import React, { useState, useEffect } from 'react';
import { X } from 'react-feather';
import { useAppDispatch, useAppSelector } from '../../store/hooks';
import { createUser, updateUser } from '../../store/slices/usersSlice';
import { fetchRoles } from '../../store/slices/rolesSlice';
import { User } from '../../api/users.api';
import { ROLES } from '../../utils/permissions';
import './UserModal.css';

interface UserModalProps {
  user: User | null;
  onClose: () => void;
  onSuccess: () => void;
}

function UserModal({ user, onClose, onSuccess }: UserModalProps) {
  const dispatch = useAppDispatch();
  const { error } = useAppSelector((state) => state.users);
  const { roles } = useAppSelector((state) => state.roles);
  const [formData, setFormData] = useState({
    email: user?.email || '',
    password: '',
    role: user?.role || ROLES.USER,
  });
  const [isSubmitting, setIsSubmitting] = useState(false);

  // Fetch all roles when modal opens
  useEffect(() => {
    dispatch(fetchRoles({ pageNumber: 1, pageSize: 100, searchTerm: '', sortBy: 'name', sortAscending: true }));
  }, [dispatch]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value,
    });
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSubmitting(true);

    try {
      if (user) {
        // Update existing user
        const updateData: any = {
          email: formData.email,
          role: formData.role,
        };
        if (formData.password) {
          updateData.password = formData.password;
        }
        await dispatch(updateUser({ userId: user.id, data: updateData })).unwrap();
      } else {
        // Create new user
        if (!formData.password) {
          alert('Password is required for new users');
          setIsSubmitting(false);
          return;
        }
        await dispatch(createUser({
          email: formData.email,
          password: formData.password,
          role: formData.role,
        })).unwrap();
      }
      onSuccess();
    } catch (err) {
      console.error('Failed to save user:', err);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="user-modal-overlay" onClick={onClose}>
      <div className="user-modal" onClick={(e) => e.stopPropagation()}>
        <div className="user-modal-header">
          <h3>{user ? 'Edit User' : 'Add New User'}</h3>
          <button className="close-button" onClick={onClose}>
            <X size={24} />
          </button>
        </div>

        {error && <div className="modal-error">{error}</div>}

        <form className="user-modal-form" onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="email">Email</label>
            <input
              type="email"
              id="email"
              name="email"
              value={formData.email}
              onChange={handleChange}
              required
              placeholder="user@example.com"
            />
          </div>

          <div className="form-group">
            <label htmlFor="password">
              Password {user && '(leave blank to keep current)'}
            </label>
            <input
              type="password"
              id="password"
              name="password"
              value={formData.password}
              onChange={handleChange}
              required={!user}
              placeholder={user ? 'Enter new password' : 'Enter password'}
            />
            {!user && (
              <span className="password-hint">
                Password must be at least 8 characters long
              </span>
            )}
          </div>

          <div className="form-group">
            <label htmlFor="role">Role</label>
            <select
              id="role"
              name="role"
              value={formData.role}
              onChange={handleChange}
              required
            >
              {roles.length > 0 ? (
                roles.map((role) => (
                  <option key={role.id} value={role.name}>
                    {role.name}
                  </option>
                ))
              ) : (
                <>
                  <option value={ROLES.USER}>User</option>
                  <option value={ROLES.MANAGER}>Manager</option>
                  <option value={ROLES.ADMIN}>Admin</option>
                </>
              )}
            </select>
          </div>

          <div className="form-actions">
            <button
              type="button"
              className="form-button secondary"
              onClick={onClose}
              disabled={isSubmitting}
            >
              Cancel
            </button>
            <button
              type="submit"
              className="form-button primary"
              disabled={isSubmitting}
            >
              {isSubmitting ? 'Saving...' : user ? 'Update User' : 'Create User'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

export default UserModal;

