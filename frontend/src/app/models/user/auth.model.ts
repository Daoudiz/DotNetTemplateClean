export interface LoginRequest {
  userName: string;
  password: string;
  rememberMe?: boolean;
}

export interface AuthResponse {
  passwordChangeRequired?: boolean;
  token?: string;
  expires?: string;
  username: string;
  roles?: string[];
}

export interface FirstLoginChangePasswordRequest {
  userName: string;
  oldPassword: string;
  newPassword: string;
  confirmPassword: string;
}

// Model user to display user profile information
export interface UserProfile {
  id: string;
  lastName: string;
  firstName: string;
  userName: string;
  email: string;
  userRole: string;
  password?: string | null;
  createdBy?: string | null;
  createdDate?: string;
  updatedBy?: string | null;
  updatedDate?: string | null;
}

export interface ChangePasswordDto {
  OldPassword: string;
  NewPassword: string;
  ConfirmPassword: string;
}
