import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import api from '../api';

export default function BecomeOrganizerPage() {
  const { refreshUser } = useAuth();
  const navigate = useNavigate();
  const [companyName, setCompanyName] = useState('');
  const [categories, setCategories] = useState([]);
  const [selectedCats, setSelectedCats] = useState([]);
  const [error, setError] = useState('');

  useEffect(() => {
    api.get('/categories/').then(r => setCategories(r.data)).catch(() => {});
  }, []);

  const toggleCat = (name) => {
    setSelectedCats(prev => prev.includes(name) ? prev.filter(c => c !== name) : [...prev, name]);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (selectedCats.length === 0) { setError('Выберите хотя бы одну категорию'); return; }
    try {
      await api.post('/auth/become-organizer/', {
        company_name: companyName,
        organizer_categories: selectedCats.join(','),
      });
      if (refreshUser) await refreshUser();
      navigate('/');
    } catch {
      setError('Ошибка при переключении на бизнес-аккаунт');
    }
  };

  return (
    <div className="max-w-[500px]">
      <h1 className="text-2xl font-bold text-black mb-2">Стать организатором</h1>
      <p className="text-sm text-gray-500 mb-6">Заполните информацию чтобы получить доступ к функциям организатора</p>

      {error && <div className="bg-red-50 text-red-500 text-sm rounded-lg p-3 mb-4">{error}</div>}

      <form onSubmit={handleSubmit}>
        <div className="mb-4">
          <label className="block text-xs text-gray-500 mb-1">Название компании</label>
          <input value={companyName} onChange={(e) => setCompanyName(e.target.value)}
            placeholder="Введите название"
            className="w-full px-4 py-3 border border-gray-200 rounded-md text-sm text-black placeholder-gray-400 focus:border-[#4338CA] outline-none" />
        </div>

        <div className="mb-6">
          <label className="block text-sm font-semibold text-black mb-2">Категории мероприятий</label>
          <div className="flex flex-wrap gap-2">
            {categories.map(c => (
              <button key={c.id} type="button" onClick={() => toggleCat(c.name)}
                className={`px-3 py-1.5 rounded text-xs cursor-pointer transition-colors ${
                  selectedCats.includes(c.name)
                    ? 'bg-[#333340] text-white border-0'
                    : 'bg-transparent text-gray-600 border border-gray-200 hover:border-[#4338CA]'
                }`}>
                {c.name} {selectedCats.includes(c.name) ? '✕' : '+'}
              </button>
            ))}
          </div>
        </div>

        <button type="submit"
          className="w-full py-3 bg-[#333340] text-white rounded-md text-sm font-medium hover:bg-[#444455] cursor-pointer border-0">
          Стать организатором
        </button>
      </form>
    </div>
  );
}
