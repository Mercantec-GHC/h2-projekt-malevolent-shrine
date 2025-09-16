import { BrowserRouter, Routes, Route, Link } from 'react-router-dom';
import Home from './pages/Home';
import Login from './pages/Login';
import Register from './pages/Register';
import Profile from './pages/Profile';
import Hotels from './pages/Hotels';
import { isAuthenticated, logout } from './utils/auth';
import './App.css';

function NavBar() {
  const auth = isAuthenticated();
  return (
    <nav style={{ marginBottom: 20 }}>
      <Link to="/">Главная</Link> |{' '}
      <Link to="/hotels">Отели</Link> |{' '}
      {auth && <Link to="/profile">Профиль</Link>} |{' '}
      {!auth && <Link to="/login">Вход</Link>} |{' '}
      {!auth && <Link to="/register">Регистрация</Link>}
      {auth && (
        <button
          className="btn btn-danger"
          style={{ marginLeft: 8 }}
          onClick={logout}
        >
          Выйти
        </button>
      )}
    </nav>
  );
}

function App() {
  return (
    <BrowserRouter>
      <NavBar />
      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/hotels" element={<Hotels />} />
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
        <Route path="/profile" element={<Profile />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;
