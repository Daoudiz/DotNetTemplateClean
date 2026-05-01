export interface RoleOption {
  id: string;
  name: string;
}

export interface ApplicationUser {
  id: string;
  userName: string;
  email: string;
  firstName: string;
  lastName: string;
  phoneNumber?: string | null;
  roles?: string;
  roleId?: string;
  isLocked: boolean;
  createdBy?: string | null;
  createdDate?: string;
  updatedBy?: string | null;
  updatedDate?: string | null;
}

export interface CreateUserViewModel {
  firstName: string;
  lastName: string;
  userRole: string;
  email: string;
  userName: string;
  password: string;
  confirmPassword: string;
  twoFactorEnabled: boolean;
}

export interface UpdateUserViewModel {
  userId: string;
  firstName: string;
  lastName: string;
  userRole: string;
  email: string;
  userName: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  isFullResult: boolean;
}

export interface UserSearchCriteria {
  nom?: string | null;
  prenom?: string | null;
  pageNumber: number;
  pageSize: number;
}

export interface UserSearchResultDto {
  id: string;
  nom: string;
  prenom: string;
  email: string;
  userName: string;
  isLocked: boolean;
  roles: string;
  roleId: string;
}

export interface AdminResetPasswordRequest {
  newPassword: string;
  confirmPassword: string;
}

export interface AdminResetPasswordResponse {
  userId: string;
  temporaryPassword: string;
  mustChangePassword: boolean;
}
