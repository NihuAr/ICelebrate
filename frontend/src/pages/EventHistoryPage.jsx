import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import api from '../api';

export default function EventHistoryPage() {
  const [events, setEvents] = useState([]);

  useEffect(() => {
    api.get('/applications/my/').then(r => {
      const apps = r.data.results || r.data;
      const past = apps.filter(a => a.status === 'approved');
      setEvents(past);
    }).catch(() => {});
  }, []);

  return (
    <div>
      <h1 className="text-2xl font-bold text-black mb-2">История</h1>

      {events.length === 0 && (
        <p className="text-sm text-gray-400 mt-2">
          Вы пока не посещали мероприятия. <Link to="/" className="text-black font-semibold underline">Перейти к афише</Link>
        </p>
      )}

      <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-5 mt-6">
        {events.map(app => (
          <div key={app.id} className="bg-white rounded-lg overflow-hidden border border-gray-100">
            <div className="h-[140px] overflow-hidden bg-gray-200">
              {app.event?.image && <img src={app.event.image} alt="" className="w-full h-full object-cover" />}
            </div>
            <div className="p-3">
              <p className="text-[11px] text-gray-400 mb-0.5">{app.event?.category?.name || ''}</p>
              <h3 className="text-sm font-semibold text-black mb-1">{app.event?.title || 'Мероприятие'}</h3>
              <p className="text-[11px] mb-0.5">
                <span className="text-gray-400">Дата: </span>
                <span className="text-red-500 font-medium">
                  {app.event?.date_time ? new Date(app.event.date_time).toLocaleDateString('ru-RU') : ''}{' '}
                  {app.event?.date_time ? new Date(app.event.date_time).toLocaleTimeString('ru-RU', { hour: '2-digit', minute: '2-digit' }) : ''}
                </span>
              </p>
              <p className="text-[11px] text-gray-400 mb-3">Место: {app.event?.location}</p>
              <Link to={`/events/${app.event?.id}/review`}
                className="block w-full py-2 bg-[#333340] text-white rounded text-xs font-medium text-center no-underline hover:bg-[#444455]">
                Оставить отзыв
              </Link>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
