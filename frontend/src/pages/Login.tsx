import { useState } from 'react';
import { login } from '../services/authService';
import { useNavigate } from 'react-router-dom';

interface LoginForm {
  email: string;
  password: string;
}

const Login = () => {
  const [form, setForm] = useState<LoginForm>({ email: '', password: '' });
  const [error, setError] = useState<string | null>(null);
  const navigate = useNavigate();

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setForm({ ...form, [e.target.name]: e.target.value });
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    try {
      const res = await login(form);
      localStorage.setItem('accessToken', res.accessToken);
      localStorage.setItem('refreshToken', res.refreshToken);
      navigate('/');
    } catch (err: any) {
      setError(err.response?.data || 'Ошибка входа');
    }
  };

  return (
    <div className="auth-form">
      <h2>Вход</h2>
      <form onSubmit={handleSubmit}>
        <div className="form-group">
          <input 
            name="email" 
            type="email" 
            placeholder="Email" 
            value={form.email} 
            onChange={handleChange} 
            required 
          />
        </div>
        <div className="form-group">
          <input 
            name="password" 
            type="password" 
            placeholder="Пароль" 
            value={form.password} 
            onChange={handleChange} 
            required 
          />
        </div>
        <button type="submit" className="btn">Войти</button>
      </form>
      {error && <div className="error">{error}</div>}
    </div>
  );
};

export default Login;
