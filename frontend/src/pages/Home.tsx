const Home = () => {
  const accessToken = localStorage.getItem('accessToken');
  
  const handleLogout = () => {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    window.location.reload();
  };

  return (
    <div>
      <h1>Главная страница</h1>
      {accessToken ? (
        <div>
          <p>Вы успешно авторизованы!</p>
          <p>Токен доступа сохранен в localStorage</p>
          <button className="btn btn-danger" onClick={handleLogout}>
            Выйти
          </button>
        </div>
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
