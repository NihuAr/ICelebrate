import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { Heart, QrCode, Filter, X } from 'lucide-react';
import api from '../api';
import { useAuth } from '../context/AuthContext';

export default function CatalogPage() {
  const [events, setEvents] = useState([]);
  const [categories, setCategories] = useState([]);
  const [activeCategory, setActiveCategory] = useState('Все');
  const [search, setSearch] = useState('');
  const [showFilter, setShowFilter] = useState(false);
  const [fromDate, setFromDate] = useState('');
  const [toDate, setToDate] = useState('');
  const [favorites, setFavorites] = useState({});
  const { user } = useAuth();

  useEffect(() => {
    api.get('/categories/').then(r => setCategories(r.data));
    loadEvents('Все', '', '', '');
    if (user) loadFavorites();
  }, [user]);

  const loadFavorites = async () => {
    try {
      const res = await api.get('/favorites/');
      const favMap = {};
      (res.data.results || res.data).forEach(fav => {
        favMap[fav.event] = true;
      });
      setFavorites(favMap);
    } catch (e) {}
  };

  const loadEvents = (category, searchQ, from, to) => {
    const params = { upcoming: 'true' };
    if (category && category !== 'Все') params.category = category;
    if (searchQ) params.search = searchQ;
    if (from) params.date_from = from;
    if (to) params.date_to = to;
    api.get('/events/', { params }).then(r => setEvents(r.data.results || r.data));
  };

  const handleCategory = (cat) => {
    setActiveCategory(cat);
    loadEvents(cat, search, fromDate, toDate);
  };

  const handleSearch = (val) => {
    setSearch(val);
    loadEvents(activeCategory, val, fromDate, toDate);
  };

  const applyFilters = () => {
    loadEvents(activeCategory, search, fromDate, toDate);
    setShowFilter(false);
  };

  const resetFilters = () => {
    setFromDate('');
    setToDate('');
    loadEvents(activeCategory, search, '', '');
  };

  const toggleFavorite = async (eventId, e) => {
    e.preventDefault();
    e.stopPropagation();
    if (!user) return;
    await api.post(`/events/${eventId}/toggle_favorite/`);
    setFavorites(prev => ({
      ...prev,
      [eventId]: !prev[eventId]
    }));
  };

  return (
    <div>
      {/* Title + Filter */}
      <div className="flex items-center justify-between mb-6 relative">
        <h1 className="text-2xl font-bold text-black">Афиша</h1>
        <button onClick={() => setShowFilter(!showFilter)}
          className="flex items-center gap-1.5 px-3 py-1.5 border border-gray-200 rounded text-[13px] text-gray-700 font-medium hover:bg-gray-50 cursor-pointer bg-white">
          <Filter size={14} /> Фильтр
        </button>

        {/* Filter Popup */}
        {showFilter && (
          <div className="absolute top-full right-0 mt-2 p-4 bg-white rounded-lg border border-gray-200 shadow-lg z-50 w-80">
            <div className="space-y-3">
              <div>
                <label className="block text-xs font-medium text-gray-700 mb-1">С даты</label>
                <input
                  type="date"
                  value={fromDate}
                  onChange={(e) => setFromDate(e.target.value)}
                  className="w-full px-3 py-2 border border-gray-200 rounded text-sm outline-none focus:border-[#4338CA]"
                />
              </div>
              <div>
                <label className="block text-xs font-medium text-gray-700 mb-1">По дату</label>
                <input
                  type="date"
                  value={toDate}
                  onChange={(e) => setToDate(e.target.value)}
                  className="w-full px-3 py-2 border border-gray-200 rounded text-sm outline-none focus:border-[#4338CA]"
                />
              </div>
              <div className="flex gap-2 pt-2">
                <button
                  onClick={applyFilters}
                  className="flex-1 px-3 py-2 bg-black text-white rounded text-sm font-medium hover:bg-gray-800"
                >
                  Применить
                </button>
                <button
                  onClick={resetFilters}
                  className="flex-1 px-3 py-2 bg-white border border-gray-200 text-gray-700 rounded text-sm font-medium hover:bg-gray-50"
                >
                  Сбросить
                </button>
              </div>
            </div>
          </div>
        )}
      </div>

      {/* Search */}
      <div className="mb-6">
        <input
          type="text"
          placeholder="Поиск по названию, описанию, месту..."
          value={search}
          onChange={(e) => handleSearch(e.target.value)}
          className="w-full px-4 py-2.5 border border-gray-200 rounded-lg text-sm outline-none focus:border-[#4338CA] placeholder-gray-400"
        />
      </div>

      {/* Categories */}
      <div className="flex gap-2 mb-6 overflow-x-auto pb-2">
        {['Все', ...categories.map(c => c.name)].map(cat => (
          <button
            key={cat}
            onClick={() => handleCategory(cat)}
            className={`px-4 py-2 rounded-lg font-medium text-sm whitespace-nowrap transition-colors ${
              activeCategory === cat
                ? 'bg-black text-white'
                : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
            }`}
          >
            {cat}
          </button>
        ))}
      </div>

      {/* Event cards grid */}
      <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-5">
        {events.map(event => (
          <Link to={`/events/${event.id}`} key={event.id} className="no-underline">
            <div className="bg-white rounded-lg overflow-hidden hover:shadow-md transition-shadow cursor-pointer">
              {/* Image */}
              <div className="h-[140px] relative overflow-hidden"
                style={{ backgroundColor: event.color || '#6366F1' }}>
                {event.image && <img src={event.image} alt="" className="w-full h-full object-cover" />}
                {user && (
                  <button onClick={(e) => toggleFavorite(event.id, e)}
                    className="absolute top-2 right-2 w-7 h-7 bg-white/90 rounded-full flex items-center justify-center text-sm cursor-pointer border-0 hover:bg-white transition-colors">
                    {favorites[event.id] ? <span className="text-red-500">♥</span> : <span className="text-gray-400">♡</span>}
                  </button>
                )}
              </div>

              {/* Content */}
              <div className="p-3">
                <h3 className="text-sm font-semibold text-black mb-1 truncate">{event.title}</h3>

                {/* Date in red */}
                <p className="text-[11px] mb-0.5">
                  <span className="text-gray-400">Дата: </span>
                  <span className="text-red-500 font-medium">
                    {new Date(event.date_time).toLocaleDateString('ru-RU', { day: '2-digit', month: '2-digit', year: 'numeric' })} {new Date(event.date_time).toLocaleTimeString('ru-RU', { hour: '2-digit', minute: '2-digit' })}
                  </span>
                </p>

                {/* Location */}
                <p className="text-[11px] text-gray-400 mb-3">
                  <span>Место: </span>{event.location}
                </p>

                {/* Bottom buttons: QR + Register */}
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
        ))}
      </div>

      {events.length === 0 && (
        <p className="text-center text-gray-400 mt-12">Мероприятия не найдены</p>
      )}
    </div>
  );
}
