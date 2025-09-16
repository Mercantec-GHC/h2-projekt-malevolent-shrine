// Типы для авторизации, соответствующие твоему API

export interface AuthDto {
  username: string;
  password: string;
  email: string;
  firstName?: string;
  lastName?: string;
}

export interface UserLoginDto {
  email: string;
  password: string;
}

export interface RefreshTokenDto {
  refreshToken: string;
}

export interface AuthResponseDto {
  accessToken: string;
  refreshToken: string;
  accessTokenExpiry: string;
  refreshTokenExpiry: string;
  message: string;
}

