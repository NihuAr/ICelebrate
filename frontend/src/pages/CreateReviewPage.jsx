import { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import api from '../api';

export default function CreateReviewPage() {
  const { eventId } = useParams();
  const navigate = useNavigate();
  const [event, setEvent] = useState(null);
  const [rating, setRating] = useState(0);
  const [text, setText] = useState('');
  const [error, setError] = useState('');

  useEffect(() => {
    api.get(`/events/${eventId}/`).then(r => setEvent(r.data)).catch(() => {});
  }, [eventId]);

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (rating === 0) { setError('Выберите оценку'); return; }
    if (!text.trim()) { setError('Напишите отзыв'); return; }
    try {
      await api.post('/reviews/', { event: eventId, rating, text });
      navigate('/history');
    } catch {
      setError('Ошибка отправки отзыва');
    }
  };

  return (
    <div className="max-w-[500px]">
      <h1 className="text-2xl font-bold text-black mb-2">Оставить отзыв</h1>
      {event && <p className="text-sm text-gray-500 mb-6">Мероприятие: <strong>{event.title}</strong></p>}

      {error && <div className="bg-red-50 text-red-500 text-sm rounded-lg p-3 mb-4">{error}</div>}

      <form onSubmit={handleSubmit}>
        {/* Star rating */}
        <div className="mb-4">
          <label className="block text-sm font-semibold text-black mb-2">Оценка</label>
          <div className="flex gap-1">
            {[1, 2, 3, 4, 5].map(star => (
              <button key={star} type="button" onClick={() => setRating(star)}
                className={`text-2xl cursor-pointer bg-transparent border-0 ${star <= rating ? 'text-yellow-500' : 'text-gray-300'}`}>
                ★
              </button>
            ))}
          </div>
        </div>

        {/* Comment */}
        <div className="mb-6">
          <label className="block text-sm font-semibold text-black mb-2">Комментарий</label>
          <textarea value={text} onChange={(e) => setText(e.target.value)}
            rows={4} placeholder="Напишите ваш отзыв..."
            className="w-full px-4 py-3 border border-gray-200 rounded-md text-sm text-black placeholder-gray-400 focus:border-[#4338CA] outline-none resize-none" />
        </div>

        <div className="flex gap-3">
          <button type="submit"
            className="flex-1 py-3 bg-[#333340] text-white rounded-md text-sm font-medium hover:bg-[#444455] cursor-pointer border-0">
            Отправить
          </button>
          <button type="button" onClick={() => navigate(-1)}
            className="flex-1 py-3 bg-white text-black rounded-md text-sm font-medium border border-gray-200 hover:bg-gray-50 cursor-pointer">
            Отмена
          </button>
        </div>
      </form>
    </div>
  );
}
