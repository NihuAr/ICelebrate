import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { Upload, X } from 'lucide-react';
import api from '../api';

export default function EditProfilePage() {
  const { user, refreshUser } = useAuth();
  const navigate = useNavigate();
  const [form, setForm] = useState({
    first_name: '', last_name: '', email: '', phone_number: '', date_of_birth: '',
  });
  const [categories, setCategories] = useState([]);
  const [selectedPrefs, setSelectedPrefs] = useState([]);
  const [profileImage, setProfileImage] = useState(null);
  const [profileImagePreview, setProfileImagePreview] = useState(null);
  const [error, setError] = useState('');

  useEffect(() => {
    if (user) {
      setForm({
        first_name: user.first_name || '',
        last_name: user.last_name || '',
        email: user.email || '',
        phone_number: user.phone_number || '',
        date_of_birth: user.date_of_birth || '',
      });
      setSelectedPrefs(
        Array.isArray(user.preferences) ? user.preferences
          : (user.preferences ? user.preferences.split(',').map(p => p.trim()) : [])
      );
    }
    api.get('/categories/').then(r => setCategories(r.data)).catch(() => {});
  }, [user]);

  const handleChange = (e) => setForm({ ...form, [e.target.name]: e.target.value });

  const handleImageChange = (e) => {
    const file = e.target.files?.[0];
    if (file) {
      setProfileImage(file);
      const reader = new FileReader();
      reader.onload = (event) => setProfileImagePreview(event.target?.result);
      reader.readAsDataURL(file);
    }
  };

  const removeImage = () => {
    setProfileImage(null);
    setProfileImagePreview(null);
  };

  const togglePref = (cat) => {
    setSelectedPrefs(prev => prev.includes(cat) ? prev.filter(c => c !== cat) : [...prev, cat]);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    try {
      // Обновляем профиль
      await api.patch('/auth/profile/', { ...form, preferences: selectedPrefs.join(',') });
      
      // Загружаем изображение если выбрано
      if (profileImage) {
        const formData = new FormData();
        formData.append('profile_image', profileImage);
        await api.patch('/auth/profile/', formData, {
          headers: { 'Content-Type': 'multipart/form-data' }
        });
      }
      
      if (refreshUser) await refreshUser();
      navigate('/profile');
    } catch (err) {
      setError('Ошибка сохранения');
    }
  };

  const inputCls = "w-full px-4 py-3 border border-gray-200 rounded-md text-sm text-black placeholder-gray-400 focus:border-[#4338CA] outline-none";

  return (
    <div className="max-w-[600px]">
      <h1 className="text-2xl font-bold text-black mb-6">Редактирование профиля</h1>

      {error && <div className="bg-red-50 text-red-500 text-sm rounded-lg p-3 mb-4">{error}</div>}

      <form onSubmit={handleSubmit}>
        {/* Profile Image Upload */}
        <div className="mb-6">
          <label className="block text-sm font-semibold text-black mb-3">Фото профиля</label>
          <div className="flex items-center gap-4">
            {/* Preview */}
            <div className="w-20 h-20 rounded-full flex items-center justify-center flex-shrink-0 overflow-hidden"
              style={{ backgroundColor: profileImagePreview ? 'transparent' : '#6366F1' }}>
              {profileImagePreview ? (
                <img src={profileImagePreview} alt="Preview" className="w-full h-full object-cover" />
              ) : user?.profile_image ? (
                <img src={user.profile_image} alt="Profile" className="w-full h-full object-cover" />
              ) : (
                <span className="text-white text-2xl font-bold">{user?.first_name?.[0]?.toUpperCase() || '?'}</span>
              )}
            </div>

            {/* Upload */}
            <div className="flex-1">
              <label className="flex items-center justify-center gap-2 px-4 py-3 border-2 border-dashed border-gray-200 rounded-lg cursor-pointer hover:border-[#4338CA] transition-colors">
                <Upload size={18} className="text-gray-400" />
                <span className="text-sm text-gray-600">{profileImage ? profileImage.name : 'Выберите изображение'}</span>
                <input type="file" accept="image/*" onChange={handleImageChange} className="hidden" />
              </label>
              {profileImage && (
                <button type="button" onClick={removeImage} className="mt-2 text-sm text-red-500 hover:text-red-700">
                  Удалить
                </button>
              )}
            </div>
          </div>
        </div>

        <div className="grid grid-cols-2 gap-4 mb-4">
          <div>
            <label className="block text-xs text-gray-500 mb-1">Имя</label>
            <input name="first_name" value={form.first_name} onChange={handleChange} className={inputCls} />
          </div>
          <div>
            <label className="block text-xs text-gray-500 mb-1">Фамилия</label>
            <input name="last_name" value={form.last_name} onChange={handleChange} className={inputCls} />
          </div>
          <div>
            <label className="block text-xs text-gray-500 mb-1">Email</label>
            <input name="email" type="email" value={form.email} onChange={handleChange} className={inputCls} />
          </div>
          <div>
            <label className="block text-xs text-gray-500 mb-1">Телефон</label>
            <input name="phone_number" value={form.phone_number} onChange={handleChange} className={inputCls} />
          </div>
          <div>
            <label className="block text-xs text-gray-500 mb-1">Дата рождения</label>
            <input name="date_of_birth" type="date" value={form.date_of_birth} onChange={handleChange} className={inputCls} />
          </div>
        </div>

        <div className="mb-6">
          <label className="block text-sm font-semibold text-black mb-2">Предпочтения</label>
          <div className="flex flex-wrap gap-2">
            {categories.map(c => (
              <button key={c.id} type="button" onClick={() => togglePref(c.name)}
                className={`px-3 py-1.5 rounded text-xs cursor-pointer transition-colors ${
                  selectedPrefs.includes(c.name)
                    ? 'bg-[#333340] text-white border-0'
                    : 'bg-transparent text-gray-600 border border-gray-200 hover:border-[#4338CA]'
                }`}>
                {c.name} {selectedPrefs.includes(c.name) ? '✕' : '+'}
              </button>
            ))}
          </div>
        </div>

        <div className="flex gap-3">
          <button type="submit"
            className="flex-1 py-3 bg-[#333340] text-white rounded-md text-sm font-medium hover:bg-[#444455] cursor-pointer border-0">
            Сохранить изменения
          </button>
          <button type="button" onClick={() => navigate('/profile')}
            className="flex-1 py-3 bg-white text-black rounded-md text-sm font-medium border border-gray-200 hover:bg-gray-50 cursor-pointer">
            Отмена
          </button>
        </div>
      </form>
    </div>
  );
}
