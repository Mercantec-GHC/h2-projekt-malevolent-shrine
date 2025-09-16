import { useEffect, useState } from 'react';
import { fetchHotels } from '../services/hotelService';
import type { Hotel } from '../services/hotelService';

const Hotels = () => {
  const [hotels, setHotels] = useState<Hotel[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    fetchHotels()
      .then(setHotels)
      .catch(() => setError('Ошибка загрузки отелей'))
      .finally(() => setLoading(false));
  }, []);

  if (loading) return <div className="card">Загрузка...</div>;
  if (error) return <div className="card error">{error}</div>;

  return (
    <div className="card">
      <h2>Список отелей</h2>
      {hotels.length === 0 && <div>Нет отелей для отображения.</div>}
      <ul style={{padding:0, listStyle:'none'}}>
        {hotels.map(hotel => (
          <li key={hotel.id} style={{marginBottom:16, borderBottom:'1px solid #eee', paddingBottom:8}}>
            <div><strong>{hotel.name}</strong></div>
            <div>{hotel.address}</div>
            {hotel.description && <div style={{color:'#666'}}>{hotel.description}</div>}
          </li>
        ))}
      </ul>
    </div>
  );
};

export default Hotels;
