import { getAccessToken, getRefreshToken, getUserInfo, isAuthenticated, logout } from '../utils/auth';

const Home = () => {
  const accessToken = getAccessToken();
  const refreshToken = getRefreshToken();
  const user = getUserInfo();
  const authenticated = isAuthenticated();

  return (
    <div className="card">
      <h1>Главная страница</h1>
      <p>Статус: {authenticated ? 'Авторизован' : 'Не авторизован'}</p>
      {authenticated && user && (
        <div style={{marginBottom: 16}}>
          <strong>Пользователь:</strong>
          <div>Email: {user.email || user.sub}</div>
          {user.username && <div>Username: {user.username}</div>}
        </div>
      )}
      {authenticated ? (
        <>
          <div style={{marginBottom: 16}}>
            <strong>Access Token:</strong>
            <pre style={{whiteSpace:'pre-wrap',wordBreak:'break-all',background:'#f4f4f4',padding:8,borderRadius:4}}>{accessToken}</pre>
            <strong>Refresh Token:</strong>
            <pre style={{whiteSpace:'pre-wrap',wordBreak:'break-all',background:'#f4f4f4',padding:8,borderRadius:4}}>{refreshToken}</pre>
          </div>
          <button className="btn btn-danger" onClick={logout}>Выйти</button>
        </>
      ) : (
        <div>
          <p>Пожалуйста, войдите в систему.</p>
          <p>Используйте навигацию выше для перехода к форме входа или регистрации.</p>
        </div>
      )}
    </div>
  );
};

export default Home;
