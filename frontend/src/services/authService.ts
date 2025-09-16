import axios from 'axios';

// Используем относительный URL, так как настроен proxy в vite.config.ts
const API_URL = '/api/Auth';

// Интерфейсы для запросов и ответов
interface LoginForm {
  email: string;
  password: string;
}

interface RegisterForm {
  username: string;
  password: string;
  email: string;
  firstName: string;
  lastName: string;
}

interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  accessTokenExpiry: string;
  refreshTokenExpiry: string;
  message: string;
}

// Интерсептор для добавления токена к запросам
axios.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('accessToken');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

export const register = async (data: RegisterForm): Promise<string> => {
  const response = await axios.post(`${API_URL}/register`, data);
  return response.data;
};

export const login = async (data: LoginForm): Promise<AuthResponse> => {
  const response = await axios.post(`${API_URL}/login`, data);
  return response.data;
};
