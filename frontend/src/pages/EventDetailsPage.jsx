import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { ArrowLeft } from 'lucide-react';
import api from '../api';
import { useAuth } from '../context/AuthContext';

export default function EventDetailsPage() {
  const { id } = useParams();
  const { user } = useAuth();
  const navigate = useNavigate();
  const [event, setEvent] = useState(null);
  const [reviews, setReviews] = useState([]);
  const [applying, setApplying] = useState(false);
  const [myApp, setMyApp] = useState(null);

  useEffect(() => {
    api.get(`/events/${id}/`).then(r => setEvent(r.data));
    api.get('/reviews/', { params: { event: id } }).then(r => setReviews(r.data.results || r.data));
    if (user) {
      api.get('/applications/my/').then(r => {
        const apps = r.data.results || r.data;
        setMyApp(apps.find(a => a.event === Number(id)));
      });
    }
  }, [id, user]);

  const handleApply = async () => {
    setApplying(true);
    try {
      await api.post('/applications/', { event: Number(id) });
      const r = await api.get('/applications/my/');
      const apps = r.data.results || r.data;
      setMyApp(apps.find(a => a.event === Number(id)));
    } catch (err) {
      alert(err.response?.data?.detail || 'Ошибка');
    }
    setApplying(false);
  };

  const handleCancel = async () => {
    if (!myApp) return;
    await api.patch(`/applications/${myApp.id}/action/`, { action: 'cancel' });
    const r = await api.get('/applications/my/');
    const apps = r.data.results || r.data;
    setMyApp(apps.find(a => a.event === Number(id)));
  };

  const toggleFavorite = async () => {
    await api.post(`/events/${id}/toggle_favorite/`);
    api.get(`/events/${id}/`).then(r => setEvent(r.data));
  };

  if (!event) return <div className="text-center py-20 text-[#8B8B8B]">Загрузка...</div>;

  return (
    <div className="max-w-[800px]">
      {/* Back */}
      <button onClick={() => navigate(-1)}
        className="flex items-center gap-2 px-3 py-2.5 border border-[#E0E0E6] rounded-md text-sm text-[#1A1A2E] font-medium mb-6 hover:bg-[#F0F0F4] cursor-pointer bg-transparent">
        <ArrowLeft size={16} /> Назад
      </button>

      {/* Image */}
      <div className="h-[400px] rounded-xl mb-6 relative overflow-hidden"
        style={{ backgroundColor: event.color || '#6366F1' }}>
        {event.image && <img src={event.image} alt="" className="w-full h-full object-cover" />}
        {user && (
          <button onClick={toggleFavorite}
            className="absolute top-4 right-4 w-12 h-12 bg-white rounded-lg flex items-center justify-center text-xl cursor-pointer border-0 hover:bg-[#F0F0F4] shadow-[0_0_10px_rgba(0,0,0,0.08)]">
            {event.is_favorite
              ? <span className="text-[#EF4444]">♥</span>
              : <span className="text-[#EF4444]">♡</span>}
          </button>
        )}
      </div>

      {/* Details card */}
      <div className="bg-white rounded-[10px] shadow-[0_0_10px_rgba(0,0,0,0.08)] p-8">
        {/* Category */}
        {event.category && (
          <p className="text-xs text-[#8B8B8B] mb-2">{event.category.name}</p>
        )}

        {/* Title */}
        <h1 className="text-[28px] font-semibold text-[#1A1A2E] mb-6">{event.title}</h1>

        {/* Info grid */}
        <div className="grid grid-cols-3 gap-6 mb-6">
          <InfoCol icon="📅" label="Дата и время"
            value={`${new Date(event.date_time).toLocaleDateString('ru-RU')} ${new Date(event.date_time).toLocaleTimeString('ru-RU', { hour: '2-digit', minute: '2-digit' })}`} />
          <InfoCol icon="📍" label="Место" value={event.location} />
          <InfoCol icon="🎟" label="Цена"
            value={Number(event.price) === 0 ? 'Бесплатно' : `${event.price} BYN`} />
        </div>

        {/* Organizer */}
        {event.organizer && (
          <div className="bg-white rounded-lg p-5 mb-6 border border-[#E0E0E6]/50">
            <div className="flex items-center gap-1.5 text-xs text-[#8B8B8B] mb-3">
              <span>👤</span> Организатор
            </div>
            <p className="text-base font-semibold text-[#1A1A2E] mb-1">
              {event.organizer.company_name || `${event.organizer.first_name} ${event.organizer.last_name}`}
            </p>
            <div className="flex items-center gap-2">
              <span className="text-sm text-[#4338CA]">★★★★★</span>
              <span className="text-xs text-[#8B8B8B]">{event.average_rating || '5.0'}</span>
            </div>
          </div>
        )}

        {/* Description */}
        <h3 className="text-base font-semibold text-[#1A1A2E] mb-3">Описание</h3>
        <p className="text-sm text-[#1A1A2E] leading-relaxed mb-6">{event.description}</p>

        {/* Bonus info */}
        <p className="text-xs text-[#10B981] mb-6">+{event.bonus_points} баллов за посещение</p>

        {/* Contractors */}
        {event.contractors?.length > 0 && (
          <div className="mb-6">
            <h3 className="text-base font-semibold text-[#1A1A2E] mb-3">Подрядчики</h3>
            {event.contractors.map(c => (
              <div key={c.id} className="bg-[#F5F5F9] rounded-lg p-3 mb-2">
                <p className="text-sm font-medium text-[#1A1A2E]">{c.name}</p>
                <p className="text-xs text-[#8B8B8B]">{c.role}</p>
              </div>
            ))}
          </div>
        )}

        {/* Reviews */}
        {reviews.length > 0 && (
          <div className="mb-6">
            <h3 className="text-base font-semibold text-[#1A1A2E] mb-3">Отзывы</h3>
            {reviews.map(r => (
              <div key={r.id} className="bg-[#F5F5F9] rounded-lg p-4 mb-2">
                <div className="flex items-center justify-between mb-1">
                  <p className="text-sm font-medium text-[#1A1A2E]">{r.user.first_name} {r.user.last_name}</p>
                  <span className="text-sm text-[#4338CA]">{'★'.repeat(r.rating)}{'☆'.repeat(5 - r.rating)}</span>
                </div>
                {r.comment && <p className="text-sm text-[#8B8B8B]">{r.comment}</p>}
              </div>
            ))}
          </div>
        )}

        {/* Action */}
        {user && (
          <div className="text-center">
            {!myApp && (
              <button onClick={handleApply} disabled={applying}
                className="px-8 py-3.5 bg-[#333340] text-white rounded-md text-sm font-medium hover:bg-[#444455] disabled:opacity-50 cursor-pointer border-0">
                {applying ? 'Отправка...' : 'Зарегистрироваться'}
              </button>
            )}
            {myApp && (
              <div className="flex items-center justify-between p-4 bg-[#F5F5F9] rounded-lg">
                <div className="text-left">
                  <p className="text-xs text-[#8B8B8B]">Статус заявки</p>
                  <StatusText status={myApp.status} />
                </div>
                {(myApp.status === 'pending' || myApp.status === 'approved') && (
                  <button onClick={handleCancel}
                    className="px-4 py-2 text-[#EF4444] border border-[#EF4444]/30 rounded-md text-sm hover:bg-red-50 cursor-pointer bg-transparent">
                    Отменить
                  </button>
                )}
              </div>
            )}
          </div>
        )}
      </div>
    </div>
  );
}

function InfoCol({ icon, label, value }) {
  return (
    <div>
      <div className="flex items-center gap-1.5 text-xs text-[#8B8B8B] mb-1">
        <span>{icon}</span> {label}
      </div>
      <p className="text-sm font-semibold text-[#1A1A2E]">{value}</p>
    </div>
  );
}

function StatusText({ status }) {
  const styles = {
    pending: 'text-[#F5A623]', approved: 'text-[#10B981]',
    rejected: 'text-[#EF4444]', cancelled: 'text-[#8B8B8B]',
  };
  const labels = {
    pending: 'В обработке', approved: 'Подтверждено',
    rejected: 'Отклонено', cancelled: 'Отменено',
  };
  return <p className={`text-sm font-semibold ${styles[status]}`}>{labels[status]}</p>;
}
