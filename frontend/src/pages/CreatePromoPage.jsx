import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../api';

export default function CreatePromoPage() {
  const navigate = useNavigate();
  const [form, setForm] = useState({
    title: '', description: '', bonus_price: '', usage_limit: '0', valid_until: '',
  });
  const [error, setError] = useState('');

  const handleChange = (e) => setForm({ ...form, [e.target.name]: e.target.value });

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    try {
      const payload = {
        ...form,
        bonus_price: Number(form.bonus_price),
        usage_limit: Number(form.usage_limit),
        valid_until: form.valid_until || null,
      };
      await api.post('/promos/', payload);
      navigate('/promos');
    } catch (err) {
      setError(err.response?.data ? JSON.stringify(err.response.data) : 'Ошибка');
    }
  };

  const inputCls = "w-full px-4 py-3.5 border border-[#E0E0E6] rounded-lg text-sm text-[#1A1A2E] focus:border-[#4338CA] outline-none";

  return (
    <div className="max-w-lg">
      <h1 className="text-2xl font-bold text-black mb-6">Создать промокод</h1>
      {error && <div className="bg-red-50 text-[#EF4444] text-sm rounded-lg p-3 mb-4">{error}</div>}

      <form onSubmit={handleSubmit} className="bg-white rounded-[10px] shadow-[0_0_10px_rgba(0,0,0,0.08)] p-8 space-y-4">
        <div>
          <label className="block text-[13px] font-medium text-[#1A1A2E] mb-1">Название *</label>
          <input name="title" value={form.title} onChange={handleChange} required className={inputCls} />
        </div>
        <div>
          <label className="block text-[13px] font-medium text-[#1A1A2E] mb-1">Описание</label>
          <textarea name="description" value={form.description} onChange={handleChange} rows={3}
            className={`${inputCls} resize-none`} />
        </div>
        <div className="grid grid-cols-2 gap-4">
          <div>
            <label className="block text-[13px] font-medium text-[#1A1A2E] mb-1">Цена (баллы) *</label>
            <input name="bonus_price" type="number" min="1" value={form.bonus_price} onChange={handleChange} required className={inputCls} />
          </div>
          <div>
            <label className="block text-[13px] font-medium text-[#1A1A2E] mb-1">Лимит (0 = без)</label>
            <input name="usage_limit" type="number" min="0" value={form.usage_limit} onChange={handleChange} className={inputCls} />
          </div>
        </div>
        <div>
          <label className="block text-[13px] font-medium text-[#1A1A2E] mb-1">Действует до</label>
          <input name="valid_until" type="datetime-local" value={form.valid_until} onChange={handleChange} className={inputCls} />
        </div>
        <div className="flex gap-3 pt-4">
          <button type="submit" className="flex-1 py-3 bg-[#333340] text-white rounded-md text-sm font-medium hover:bg-[#444455] cursor-pointer border-0">Создать</button>
          <button type="button" onClick={() => navigate(-1)} className="px-6 py-3 border border-[#E0E0E6] rounded-md text-sm text-[#1A1A2E] hover:bg-[#F0F0F4] cursor-pointer bg-transparent">Отмена</button>
        </div>
      </form>
    </div>
  );
}
