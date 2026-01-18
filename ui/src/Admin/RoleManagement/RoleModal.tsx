import React, { useState, useEffect } from 'react';
import { X } from 'react-feather';
import { useAppDispatch } from '../../store/hooks';
import { createRole, updateRole } from '../../store/slices/rolesSlice';
import { Role } from '../../api/roles.api';
import './RoleModal.css';

interface RoleModalProps {
  role: Role | null;
  onClose: () => void;
  onSuccess: () => void;
}

function RoleModal({ role, onClose, onSuccess }: RoleModalProps) {
  const dispatch = useAppDispatch();
  const [name, setName] = useState('');
  const [error, setError] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    if (role) {
      setName(role.name);
    }
  }, [role]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (!name.trim()) {
      setError('Role name is required');
      return;
    }

    setIsSubmitting(true);

    try {
      if (role) {
        await dispatch(updateRole({ roleId: role.id, data: { name } })).unwrap();
      } else {
        await dispatch(createRole({ name })).unwrap();
      }
      onSuccess();
    } catch (err: any) {
      setError(err || 'An error occurred');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h3>{role ? 'Edit Role' : 'Add New Role'}</h3>
          <button className="close-button" onClick={onClose}>
            <X size={20} />
          </button>
        </div>

        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="name">Role Name</label>
            <input
              type="text"
              id="name"
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="Enter role name"
              required
            />
          </div>

          {error && <div className="error-message">{error}</div>}

          <div className="modal-actions">
            <button type="button" className="cancel-button" onClick={onClose}>
              Cancel
            </button>
            <button type="submit" className="submit-button" disabled={isSubmitting}>
              {isSubmitting ? 'Saving...' : role ? 'Update Role' : 'Create Role'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

export default RoleModal;

