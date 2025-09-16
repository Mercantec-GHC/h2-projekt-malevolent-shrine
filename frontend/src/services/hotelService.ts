import axios from 'axios';

export interface Hotel {
  id: number;
  name: string;
  address: string;
  description?: string;
  // Добавь другие поля, если нужны
}

export async function fetchHotels(): Promise<Hotel[]> {
  const res = await axios.get('/api/Hotels');
  return res.data;
}

