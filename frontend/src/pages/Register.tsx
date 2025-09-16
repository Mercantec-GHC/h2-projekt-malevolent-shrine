import { useState } from 'react';
import { register } from '../services/authService';
import { useNavigate } from 'react-router-dom';

interface RegisterForm {
  username: string;
  password: string;
  email: string;
  firstName: string;
  lastName: string;
}

const Register = () => {
  const [form, setForm] = useState<RegisterForm>({
    username: '',
    password: '',
    email: '',
    firstName: '',
    lastName: '',
  });
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const navigate = useNavigate();

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setForm({ ...form, [e.target.name]: e.target.value });
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setSuccess(null);
    try {
      const res = await register(form);
      setSuccess(res);
      setTimeout(() => navigate('/login'), 1500);
    } catch (err: any) {
      setError(err.response?.data || 'Ошибка регистрации');
    }
  };

  return (
    <div className="auth-form">
      <h2>Регистрация</h2>
      <form onSubmit={handleSubmit}>
        <div className="form-group">
          <input 
            name="username" 
            placeholder="Имя пользователя" 
            value={form.username} 
            onChange={handleChange} 
            required 
          />
        </div>
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
        <div className="form-group">
          <input 
            name="firstName" 
            placeholder="Имя" 
            value={form.firstName} 
            onChange={handleChange} 
          />
        </div>
        <div className="form-group">
          <input 
            name="lastName" 
            placeholder="Фамилия" 
            value={form.lastName} 
            onChange={handleChange} 
          />
        </div>
        <button type="submit" className="btn">Зарегистрироваться</button>
      </form>
      {error && <div className="error">{error}</div>}
      {success && <div className="success">{success}</div>}
    </div>
  );
};

export default Register;
