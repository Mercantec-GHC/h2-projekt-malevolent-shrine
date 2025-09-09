import { getUserInfo, isAuthenticated, logout } from '../utils/auth';
import { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';

const Profile = () => {
  const user = getUserInfo();
  const navigate = useNavigate();

  useEffect(() => {
    if (!isAuthenticated()) {
      navigate('/login');
    }
  }, [navigate]);

  if (!user) return null;

  return (
    <div className="card">
      <h2>Профиль</h2>
      <div><strong>Email:</strong> {user.email || user.sub}</div>
      {user.username && <div><strong>Username:</strong> {user.username}</div>}
      {user.firstName && <div><strong>Имя:</strong> {user.firstName}</div>}
      {user.lastName && <div><strong>Фамилия:</strong> {user.lastName}</div>}
      <button className="btn btn-danger" style={{marginTop:16}} onClick={logout}>Выйти</button>
    </div>
  );
};

export default Profile;

