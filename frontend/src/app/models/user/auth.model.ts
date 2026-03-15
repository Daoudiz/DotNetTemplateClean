export interface LoginRequest {
    userName: string;
    password: string;
    rememberMe?: boolean;
}

export interface AuthResponse {
    token: string;
    expires: string;
    username: string;
    roles: string[];
}

// Model user to display user profile information
export interface UserProfile {
    id: string;
    matricule: string;
    lastName: string;
    firstName: string;
    userName: string;
    email: string;
    userRole: string;
    dateRecrutement?: Date;
    entite: string;
    passwordHash: string;
}

export interface ChangePasswordDto {
    OldPassword: string;      // Correspond au [Required] du Back
    NewPassword: string;
    ConfirmPassword: string;  // Correspond au [Compare] du Back
}