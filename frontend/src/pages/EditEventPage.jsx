import { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import api from '../api';

export default function EditEventPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [categories, setCategories] = useState([]);
  const [form, setForm] = useState({
    title: '', description: '', date_time: '', location: '', duration_hours: '',
    price: '', payment_method: 'free', category: '', color: '#6366F1', visible_only_for_creator: false,
  });
  const [error, setError] = useState('');

  useEffect(() => {
    api.get('/categories/').then(r => setCategories(r.data));
    api.get(`/events/${id}/`).then(r => {
      const e = r.data;
      setForm({
        title: e.title || '',
        description: e.description || '',
        date_time: e.date_time ? e.date_time.slice(0, 16) : '',
        location: e.location || '',
        duration_hours: e.duration_hours || '',
        price: e.price || '',
        payment_method: e.payment_method || 'free',
        category: e.category?.id || '',
        color: e.color || '#6366F1',
        visible_only_for_creator: e.visible_only_for_creator || false,
      });
    });
  }, [id]);

  const handleChange = (e) => setForm({ ...form, [e.target.name]: e.target.value });

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    try {
      await api.patch(`/events/${id}/`, form);
      navigate('/my-events');
    } catch (err) {
      setError('Ошибка сохранения');
    }
  };

  const inputCls = "w-full px-4 py-3 border border-gray-200 rounded-md text-sm text-black placeholder-gray-400 focus:border-[#4338CA] outline-none";

  return (
    <div className="max-w-[600px]">
      <h1 className="text-2xl font-bold text-black mb-6">Редактирование мероприятия</h1>
      {error && <div className="bg-red-50 text-red-500 text-sm rounded-lg p-3 mb-4">{error}</div>}

      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label className="block text-xs text-gray-500 mb-1">Название</label>
          <input name="title" value={form.title} onChange={handleChange} className={inputCls} required />
        </div>
        <div>
          <label className="block text-xs text-gray-500 mb-1">Описание</label>
          <textarea name="description" value={form.description} onChange={handleChange} rows={4}
            className={inputCls + " resize-none"} />
        </div>
        <div className="grid grid-cols-2 gap-4">
          <div>
            <label className="block text-xs text-gray-500 mb-1">Дата и время</label>
            <input name="date_time" type="datetime-local" value={form.date_time} onChange={handleChange} className={inputCls} required />
          </div>
          <div>
            <label className="block text-xs text-gray-500 mb-1">Длительность (часы)</label>
            <input name="duration_hours" type="number" step="0.5" value={form.duration_hours} onChange={handleChange} className={inputCls} />
          </div>
        </div>
        <div>
          <label className="block text-xs text-gray-500 mb-1">Место</label>
          <input name="location" value={form.location} onChange={handleChange} className={inputCls} required />
        </div>
        <div className="grid grid-cols-2 gap-4">
          <div>
            <label className="block text-xs text-gray-500 mb-1">Цена (BYN)</label>
            <input name="price" type="number" step="0.01" value={form.price} onChange={handleChange} className={inputCls} />
          </div>
          <div>
            <label className="block text-xs text-gray-500 mb-1">Категория</label>
            <select name="category" value={form.category} onChange={handleChange} className={inputCls}>
              <option value="">Выберите</option>
              {categories.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
            </select>
          </div>
        </div>

        <div className="flex items-center gap-3">
          <input
            id="visible_only_for_creator"
            name="visible_only_for_creator"
            type="checkbox"
            checked={form.visible_only_for_creator}
            onChange={(e) => setForm({ ...form, visible_only_for_creator: e.target.checked })}
            className="w-4 h-4 cursor-pointer"
          />
          <label htmlFor="visible_only_for_creator" className="text-sm text-gray-700">Видно только мне (скрыть от других пользователей)</label>
        </div>

        <div className="flex gap-3 pt-4">
          <button type="submit"
            className="flex-1 py-3 bg-[#333340] text-white rounded-md text-sm font-medium hover:bg-[#444455] cursor-pointer border-0">
            Сохранить
          </button>
          <button type="button" onClick={() => navigate('/my-events')}
            className="flex-1 py-3 bg-white text-black rounded-md text-sm font-medium border border-gray-200 hover:bg-gray-50 cursor-pointer">
            Отмена
          </button>
        </div>
      </form>
    </div>
  );
}
