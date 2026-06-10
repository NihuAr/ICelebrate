import { useState, useEffect } from 'react';
import api from '../api';
import { useAuth } from '../context/AuthContext';

export default function MyReviewsPage() {
  const { user } = useAuth();
  const [reviews, setReviews] = useState([]);

  useEffect(() => {
    api.get('/reviews/', { params: { user: user?.id } }).then(r => {
      setReviews(r.data.results || r.data);
    }).catch(() => {});
  }, [user]);

  return (
    <div className="max-w-2xl">
      <h1 className="text-2xl font-bold text-black mb-6">Мои отзывы</h1>

      {reviews.length === 0 && <p className="text-sm text-gray-400">Вы ещё не оставляли отзывов</p>}

      <div className="space-y-4">
        {reviews.map(r => (
          <div key={r.id} className="border border-gray-100 rounded-lg p-4">
            <div className="flex items-center justify-between mb-2">
              <h3 className="text-sm font-semibold text-black">{r.event_title || 'Мероприятие'}</h3>
              <div className="text-yellow-500 text-sm">
                {'★'.repeat(r.rating)}{'☆'.repeat(5 - r.rating)}
              </div>
            </div>
            <p className="text-[13px] text-gray-600">{r.text}</p>
            <p className="text-[11px] text-gray-400 mt-2">{new Date(r.created_at).toLocaleDateString('ru-RU')}</p>
          </div>
        ))}
      </div>
    </div>
  );
}
