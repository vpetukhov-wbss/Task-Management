// Role constants matching backend
export const ROLES = {
  ADMIN: 'Admin',
  MANAGER: 'Manager',
  USER: 'User',
} as const;

export type Role = typeof ROLES[keyof typeof ROLES];

export const permissions = {
  canCreateBoard: (role?: string): boolean => {
    return role === ROLES.ADMIN;
  },

  canManageBoard: (role?: string): boolean => {
    return role === ROLES.ADMIN || role === ROLES.MANAGER;
  },

  canCreateTask: (role?: string): boolean => {
    return role === ROLES.ADMIN || role === ROLES.MANAGER;
  },

  canManageTask: (role?: string): boolean => {
    return role === ROLES.ADMIN || role === ROLES.MANAGER;
  },

  canViewContent: (role?: string): boolean => {
    return !!role;
  },

  isReadOnly: (role?: string): boolean => {
    return role === ROLES.USER;
  },

  isAdmin: (role?: string): boolean => {
    return role === ROLES.ADMIN;
  },

  isManager: (role?: string): boolean => {
    return role === ROLES.MANAGER;
  },
};

export const usePermissions = (userRole?: string) => {
  return {
    canCreateBoard: permissions.canCreateBoard(userRole),
    canManageBoard: permissions.canManageBoard(userRole),
    canCreateTask: permissions.canCreateTask(userRole),
    canManageTask: permissions.canManageTask(userRole),
    canViewContent: permissions.canViewContent(userRole),
    isReadOnly: permissions.isReadOnly(userRole),
    isAdmin: permissions.isAdmin(userRole),
    isManager: permissions.isManager(userRole),
  };
};

