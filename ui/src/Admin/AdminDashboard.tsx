import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Users, Settings, BarChart2 } from 'react-feather';
import { useAppDispatch, useAppSelector } from '../store/hooks';
import { logout as logoutAction } from '../store/slices/authSlice';
import UserManagement from './UserManagement/UserManagement';
import RoleManagement from './RoleManagement/RoleManagement';
import Analytics from './Analytics/Analytics';
import './AdminDashboard.css';

const ICON_LOGO_URL = "https://www.icon.com.mt/wp-content/themes/icon-2024update/img/logo.svg?v25";

type MenuItem = 'users' | 'roles' | 'analytics';

function AdminDashboard() {
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const { user } = useAppSelector((state) => state.auth);
  const [activeMenu, setActiveMenu] = useState<MenuItem>('users');

  const handleLogout = () => {
    dispatch(logoutAction());
    navigate('/login');
  };

  const renderContent = () => {
    switch (activeMenu) {
      case 'users':
        return <UserManagement />;
      case 'roles':
        return <RoleManagement />;
      case 'analytics':
        return <Analytics />;
      default:
        return null;
    }
  };

  return (
    <div className="admin-container">
      <div className="admin-nav">
        <div className="admin-nav-left">
          <div className="admin-logo">
            <div className="admin-logo-icon">
              <img src={ICON_LOGO_URL} alt="ICON Logo" />
            </div>
            <span>Admin Panel</span>
          </div>
        </div>
        <div className="admin-nav-right">
          <span className="user-email">
            {user?.email} {user?.role && <span className="user-role">({user.role})</span>}
          </span>
          <button
            onClick={() => navigate('/dashboard')}
            className="logout-button"
            style={{ background: 'rgba(255, 255, 255, 0.1)', marginRight: '10px' }}
          >
            Dashboard
          </button>
          <button onClick={handleLogout} className="logout-button">
            Logout
          </button>
        </div>
      </div>

      <div className="admin-content">
        <div className="admin-sidebar">
          <ul className="sidebar-menu">
            <li
              className={`sidebar-menu-item ${activeMenu === 'users' ? 'active' : ''}`}
              onClick={() => setActiveMenu('users')}
            >
              <Users size={20} />
              <span>User Management</span>
            </li>
            <li
              className={`sidebar-menu-item ${activeMenu === 'roles' ? 'active' : ''}`}
              onClick={() => setActiveMenu('roles')}
            >
              <Settings size={20} />
              <span>Role Management</span>
            </li>
            <li
              className={`sidebar-menu-item ${activeMenu === 'analytics' ? 'active' : ''}`}
              onClick={() => setActiveMenu('analytics')}
            >
              <BarChart2 size={20} />
              <span>Analytics</span>
            </li>
          </ul>
        </div>

        <div className="admin-main">
          {renderContent()}
        </div>
      </div>
    </div>
  );
}

export default AdminDashboard;

