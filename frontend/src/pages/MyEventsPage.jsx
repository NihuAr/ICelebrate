import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { PlusCircle, Users, Trash2 } from 'lucide-react';
import api from '../api';
import { useAuth } from '../context/AuthContext';

export default function MyEventsPage() {
  const { user } = useAuth();
  const [events, setEvents] = useState([]);
  const [showFuture, setShowFuture] = useState(true);
  const [search, setSearch] = useState('');

  useEffect(() => {
    loadEvents();
  }, [user, showFuture]);

  const loadEvents = () => {
    if (!user) return;
    const now = new Date();
    api.get('/events/', { params: { organizer: user.id } })
      .then(r => {
        let filtered = r.data.results || r.data;
        
        // Filter by future/past
        filtered = filtered.filter(e => {
          const eventDate = new Date(e.date_time);
          return showFuture ? eventDate >= now : eventDate < now;
        });

        // Filter by search
        if (search) {
          filtered = filtered.filter(e =>
            e.title.toLowerCase().includes(search.toLowerCase()) ||
            e.description.toLowerCase().includes(search.toLowerCase()) ||
            e.location.toLowerCase().includes(search.toLowerCase())
          );
        }

        setEvents(filtered);
      });
  };

  const handleDelete = async (eventId) => {
    if (!window.confirm('Вы уверены, что хотите удалить это мероприятие?')) return;
    await api.delete(`/events/${eventId}/`);
    loadEvents();
  };

  useEffect(() => {
    loadEvents();
  }, [search]);

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-black">Мои мероприятия</h1>
        <Link to="/create-event"
          className="flex items-center gap-2 px-6 py-3 bg-black text-white rounded-lg text-sm font-medium hover:bg-gray-800 no-underline">
          <PlusCircle size={18} /> Новое мероприятие
        </Link>
      </div>

      {/* Tabs */}
      <div className="flex gap-3 mb-6">
        <button
          onClick={() => setShowFuture(true)}
          className={`px-4 py-2 rounded-lg font-medium text-sm transition-colors ${
            showFuture
              ? 'bg-black text-white'
              : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
          }`}
        >
          Предстоящие
        </button>
        <button
          onClick={() => setShowFuture(false)}
          className={`px-4 py-2 rounded-lg font-medium text-sm transition-colors ${
            !showFuture
              ? 'bg-black text-white'
              : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
          }`}
        >
          Прошедшие
        </button>
      </div>

      {/* Search */}
      <div className="mb-6">
        <input
          type="text"
          placeholder="Поиск по названию..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          className="w-full px-4 py-2.5 border border-gray-200 rounded-lg text-sm outline-none focus:border-black placeholder-gray-400"
        />
      </div>

      {events.length === 0 && <p className="text-center text-gray-400 py-12">Мероприятий не найдено</p>}

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-5">
        {events.map(event => (
          <div key={event.id} className="bg-white rounded-lg overflow-hidden border border-gray-200 hover:shadow-md transition-shadow">
            {/* Image */}
            <div className="h-[160px] relative overflow-hidden" style={{ backgroundColor: event.color || '#6366F1' }}>
              {event.image && <img src={event.image} alt="" className="w-full h-full object-cover" />}
              <button
                onClick={() => handleDelete(event.id)}
                className="absolute top-2 right-2 w-8 h-8 bg-red-500/90 rounded-full flex items-center justify-center text-white cursor-pointer border-0 hover:bg-red-600 transition-colors"
              >
                <Trash2 size={16} />
              </button>
            </div>

            {/* Content */}
            <div className="p-4">
              <h3 className="font-semibold text-black mb-2 truncate">{event.title}</h3>
              <p className="text-xs text-gray-600 mb-1">📅 {new Date(event.date_time).toLocaleDateString('ru-RU')}</p>
              <p className="text-xs text-gray-600 mb-4">📍 {event.location}</p>

              {/* Buttons */}
              <div className="flex gap-2">
                <Link to={`/edit-event/${event.id}`}
                  className="flex-1 px-3 py-2 bg-black text-white rounded text-xs font-medium text-center no-underline hover:bg-gray-800">
                  Редактировать
                </Link>
                <Link to={`/events/${event.id}/applications`}
                  className="flex-1 px-3 py-2 border border-gray-200 text-gray-700 rounded text-xs font-medium text-center no-underline hover:bg-gray-50">
                  <Users size={14} className="inline mr-1" /> Заявки
                </Link>
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
