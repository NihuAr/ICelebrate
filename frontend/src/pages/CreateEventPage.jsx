import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Upload, X, Plus } from 'lucide-react';
import api from '../api';

export default function CreateEventPage() {
  const navigate = useNavigate();
  const [categories, setCategories] = useState([]);
  const [form, setForm] = useState({
    title: '', description: '', date_time: '', duration_hours: '1',
    location: '', price: '0', payment_method: 'free', category_id: '', color: '#6366F1',
    visible_only_for_creator: false,
  });
  const [image, setImage] = useState(null);
  const [documents, setDocuments] = useState([]);
  const [contractors, setContractors] = useState([]);
  const [newContractor, setNewContractor] = useState({ name: '', role: '' });
  const [error, setError] = useState('');

  useEffect(() => {
    api.get('/categories/').then(r => setCategories(r.data));
  }, []);

  const handleChange = (e) => setForm({ ...form, [e.target.name]: e.target.value });

  const handleImageChange = (e) => {
    const file = e.target.files?.[0];
    if (file) setImage(file);
  };

  const handleDocumentAdd = (e) => {
    const files = e.target.files;
    if (files) {
      setDocuments([...documents, ...Array.from(files)]);
    }
  };

  const removeDocument = (idx) => {
    setDocuments(documents.filter((_, i) => i !== idx));
  };

  const addContractor = () => {
    if (newContractor.name.trim()) {
      setContractors([...contractors, { ...newContractor, id: Date.now() }]);
      setNewContractor({ name: '', role: '' });
    }
  };

  const removeContractor = (id) => {
    setContractors(contractors.filter(c => c.id !== id));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    try {
      const payload = {
        ...form,
        category_id: form.category_id ? Number(form.category_id) : null,
        duration_hours: Number(form.duration_hours),
        price: Number(form.price),
        visible_only_for_creator: Boolean(form.visible_only_for_creator),
      };
      const res = await api.post('/events/', payload);
      const eventId = res.data.id;

      // Upload image
      if (image) {
        const formData = new FormData();
        formData.append('image', image);
        await api.patch(`/events/${eventId}/`, formData, {
          headers: { 'Content-Type': 'multipart/form-data' }
        });
      }

      // Upload documents (if API supports)
      for (const doc of documents) {
        const formData = new FormData();
        formData.append('file', doc);
        try {
          await api.post(`/events/${eventId}/documents/`, formData, {
            headers: { 'Content-Type': 'multipart/form-data' }
          });
        } catch (e) {
          console.log('Document upload not supported');
        }
      }

      navigate('/my-events');
    } catch (err) {
      setError(err.response?.data ? JSON.stringify(err.response.data) : 'Ошибка');
    }
  };

  return (
    <div className="max-w-2xl mx-auto">
      <h1 className="text-2xl font-bold text-black mb-6">Создать мероприятие</h1>

      {error && <div className="bg-red-50 text-[#EF4444] text-sm rounded-lg p-3 mb-4">{error}</div>}

      <form onSubmit={handleSubmit} className="bg-white rounded-[10px] shadow-[0_0_10px_rgba(0,0,0,0.08)] p-8 space-y-4">
        <Field label="Название *">
          <input name="title" value={form.title} onChange={handleChange} required
            className="w-full px-4 py-3.5 border border-[#E0E0E6] rounded-lg text-sm text-[#1A1A2E] focus:border-[#4338CA] outline-none" />
        </Field>

        <Field label="Описание">
          <textarea name="description" value={form.description} onChange={handleChange} rows={3}
            className="w-full px-4 py-3.5 border border-[#E0E0E6] rounded-lg text-sm text-[#1A1A2E] focus:border-[#4338CA] outline-none resize-none" />
        </Field>

        <div className="grid grid-cols-2 gap-4">
          <Field label="Дата и время *">
            <input name="date_time" type="datetime-local" value={form.date_time} onChange={handleChange} required
              className="w-full px-4 py-3.5 border border-[#E0E0E6] rounded-lg text-sm text-[#1A1A2E] focus:border-[#4338CA] outline-none" />
          </Field>
          <Field label="Длительность (часы) *">
            <input name="duration_hours" type="number" step="0.5" min="0.5" value={form.duration_hours} onChange={handleChange} required
              className="w-full px-4 py-3.5 border border-[#E0E0E6] rounded-lg text-sm text-[#1A1A2E] focus:border-[#4338CA] outline-none" />
          </Field>
        </div>

        <Field label="Место *">
          <input name="location" value={form.location} onChange={handleChange} required
            className="w-full px-4 py-3.5 border border-[#E0E0E6] rounded-lg text-sm text-[#1A1A2E] focus:border-[#4338CA] outline-none" />
        </Field>

        <div className="grid grid-cols-2 gap-4">
          <Field label="Стоимость (BYN)">
            <input name="price" type="number" step="0.01" min="0" value={form.price} onChange={handleChange}
              className="w-full px-4 py-3.5 border border-[#E0E0E6] rounded-lg text-sm text-[#1A1A2E] focus:border-[#4338CA] outline-none" />
          </Field>
          <Field label="Способ оплаты">
            <select name="payment_method" value={form.payment_method} onChange={handleChange}
              className="w-full px-4 py-3.5 border border-[#E0E0E6] rounded-lg text-sm text-[#1A1A2E] focus:border-[#4338CA] outline-none">
              <option value="free">Бесплатно</option>
              <option value="cash">Наличные</option>
              <option value="card">Карта (терминал)</option>
              <option value="both">Наличные и карта</option>
            </select>
          </Field>
        </div>

        <Field label="Категория">
          <select name="category_id" value={form.category_id} onChange={handleChange}
            className="w-full px-4 py-3.5 border border-[#E0E0E6] rounded-lg text-sm text-[#1A1A2E] focus:border-[#4338CA] outline-none">
            <option value="">Без категории</option>
            {categories.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
          </select>
        </Field>

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

        {/* Image upload */}
        <Field label="Изображение мероприятия">
          <div className="flex items-center gap-4">
            <label className="flex-1 flex items-center justify-center gap-2 px-4 py-3.5 border-2 border-dashed border-[#E0E0E6] rounded-lg cursor-pointer hover:border-[#4338CA] transition-colors">
              <Upload size={18} className="text-gray-400" />
              <span className="text-sm text-gray-600">{image ? image.name : 'Выберите изображение'}</span>
              <input type="file" accept="image/*" onChange={handleImageChange} className="hidden" />
            </label>
            {image && (
              <button type="button" onClick={() => setImage(null)} className="p-2 text-red-500 hover:bg-red-50 rounded">
                <X size={18} />
              </button>
            )}
          </div>
        </Field>

        {/* Documents upload */}
        <Field label="Документы">
          <div className="space-y-2">
            <label className="flex items-center justify-center gap-2 px-4 py-3.5 border-2 border-dashed border-[#E0E0E6] rounded-lg cursor-pointer hover:border-[#4338CA] transition-colors">
              <Upload size={18} className="text-gray-400" />
              <span className="text-sm text-gray-600">Добавить документы</span>
              <input type="file" multiple onChange={handleDocumentAdd} className="hidden" />
            </label>
            {documents.length > 0 && (
              <div className="space-y-1">
                {documents.map((doc, idx) => (
                  <div key={idx} className="flex items-center justify-between p-2 bg-gray-50 rounded">
                    <span className="text-sm text-gray-700">{doc.name}</span>
                    <button type="button" onClick={() => removeDocument(idx)} className="p-1 text-red-500 hover:bg-red-50 rounded">
                      <X size={16} />
                    </button>
                  </div>
                ))}
              </div>
            )}
          </div>
        </Field>

        {/* Contractors */}
        <Field label="Подрядчики">
          <div className="space-y-2">
            <div className="flex gap-2">
              <input
                type="text"
                placeholder="Имя подрядчика"
                value={newContractor.name}
                onChange={(e) => setNewContractor({ ...newContractor, name: e.target.value })}
                className="flex-1 px-4 py-3.5 border border-[#E0E0E6] rounded-lg text-sm text-[#1A1A2E] focus:border-[#4338CA] outline-none"
              />
              <input
                type="text"
                placeholder="Роль/должность"
                value={newContractor.role}
                onChange={(e) => setNewContractor({ ...newContractor, role: e.target.value })}
                className="flex-1 px-4 py-3.5 border border-[#E0E0E6] rounded-lg text-sm text-[#1A1A2E] focus:border-[#4338CA] outline-none"
              />
              <button type="button" onClick={addContractor} className="px-4 py-3.5 bg-[#333340] text-white rounded-lg hover:bg-[#444455] transition-colors flex items-center gap-2">
                <Plus size={16} />
              </button>
            </div>
            {contractors.length > 0 && (
              <div className="space-y-1">
                {contractors.map((c) => (
                  <div key={c.id} className="flex items-center justify-between p-2 bg-gray-50 rounded">
                    <div className="text-sm">
                      <span className="font-medium text-gray-700">{c.name}</span>
                      {c.role && <span className="text-gray-500"> — {c.role}</span>}
                    </div>
                    <button type="button" onClick={() => removeContractor(c.id)} className="p-1 text-red-500 hover:bg-red-50 rounded">
                      <X size={16} />
                    </button>
                  </div>
                ))}
              </div>
            )}
          </div>
        </Field>

        <div className="flex gap-3 pt-4">
          <button type="submit" className="flex-1 py-3 bg-[#333340] text-white rounded-md text-sm font-medium hover:bg-[#444455] transition-colors cursor-pointer border-0">
            Создать
          </button>
          <button type="button" onClick={() => navigate(-1)} className="px-6 py-3 border border-[#E0E0E6] rounded-md text-sm text-[#1A1A2E] hover:bg-[#F0F0F4] transition-colors cursor-pointer bg-transparent">
            Отмена
          </button>
        </div>
      </form>
    </div>
  );
}

function Field({ label, children }) {
  return (
    <div>
      <label className="block text-[13px] font-medium text-[#1A1A2E] mb-1">{label}</label>
      {children}
    </div>
  );
}
