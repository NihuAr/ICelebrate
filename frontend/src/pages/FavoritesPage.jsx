import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { QrCode } from 'lucide-react';
import api from '../api';

export default function FavoritesPage() {
  const [favorites, setFavorites] = useState([]);

  useEffect(() => {
    api.get('/favorites/').then(r => setFavorites(r.data.results || r.data));
  }, []);

  return (
    <div>
      <h1 className="text-2xl font-bold text-black mb-2">Избранное</h1>

      {favorites.length === 0 && (
        <p className="text-sm text-gray-400 mt-2">
          Вы пока ничего не добавили в избранное. <Link to="/" className="text-black font-semibold underline">Перейти к афише</Link>
        </p>
      )}

      <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-5 mt-6">
        {favorites.map(fav => {
          const event = fav.event;
          return (
            <Link to={`/events/${event.id}`} key={fav.id} className="no-underline">
              <div className="bg-white rounded-lg overflow-hidden hover:shadow-md transition-shadow">
                <div className="h-[140px] overflow-hidden" style={{ backgroundColor: event.color || '#6366F1' }}>
                  {event.image && <img src={event.image} alt="" className="w-full h-full object-cover" />}
                </div>
                <div className="p-3">
                  <h3 className="text-sm font-semibold text-black mb-1 truncate">{event.title}</h3>
                  <p className="text-[11px] mb-0.5">
                    <span className="text-gray-400">Дата: </span>
                    <span className="text-red-500 font-medium">
                      {new Date(event.date_time).toLocaleDateString('ru-RU', { day: '2-digit', month: '2-digit', year: 'numeric' })} {new Date(event.date_time).toLocaleTimeString('ru-RU', { hour: '2-digit', minute: '2-digit' })}
                    </span>
                  </p>
                  <p className="text-[11px] text-gray-400 mb-3"><span>Место: </span>{event.location}</p>
                  <div className="flex items-center gap-2">
                    <button className="w-9 h-9 border border-gray-200 rounded flex items-center justify-center bg-white cursor-pointer hover:bg-gray-50">
                      <QrCode size={16} className="text-gray-600" />
                    </button>
                    <button className="flex-1 py-2 bg-[#333340] text-white rounded text-xs font-medium hover:bg-[#444455] cursor-pointer border-0">
                      Зарегистрироваться
                    </button>
                  </div>
                </div>
              </div>
            </Link>
          );
        })}
      </div>
    </div>
  );
}
