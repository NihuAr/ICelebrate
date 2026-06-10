import { useState, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import { Link, useNavigate } from 'react-router-dom';
import api from '../api';

export default function ProfilePage() {
  const { user, logout, refreshUser } = useAuth();
  const navigate = useNavigate();
  const [categories, setCategories] = useState([]);
  const [show2FAModal, setShow2FAModal] = useState(false);
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  useEffect(() => {
    api.get('/categories/').then(r => setCategories(r.data)).catch(() => {});
  }, []);

  if (!user) return null;

  const handleLogout = () => { logout(); navigate('/login'); };

  const handleToggle2FA = async () => {
    setError('');
    setSuccess('');
    
    if (user.is_2fa_enabled) {
      // Disable 2FA - no password needed
      setLoading(true);
      try {
        await api.post('/auth/2fa/toggle/', { action: 'disable' });
        setSuccess('2FA отключена');
        await refreshUser();
      } catch (err) {
        setError(err.response?.data?.error || 'Не удалось отключить 2FA');
      } finally {
        setLoading(false);
      }
    } else {
      // Show modal to enable 2FA
      setShow2FAModal(true);
    }
  };

  const handleEnable2FA = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    
    try {
      await api.post('/auth/2fa/toggle/', { 
        action: 'enable',
        password 
      });
      setSuccess('2FA включена! Теперь при каждом входе будет требоваться код из email.');
      setShow2FAModal(false);
      setPassword('');
      await refreshUser();
    } catch (err) {
      setError(err.response?.data?.error || 'Не удалось включить 2FA');
    } finally {
      setLoading(false);
    }
  };

  // preferences may be array or comma-separated string
  const userPrefs = Array.isArray(user.preferences)
    ? user.preferences
    : (user.preferences ? user.preferences.split(',').map(p => p.trim()) : []);

  return (
    <div>
      {/* Header */}
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-black">Аккаунт</h1>
        <div className="flex items-center gap-2">
          <select className="px-3 py-2 border border-gray-200 rounded text-sm bg-white cursor-pointer outline-none">
            <option>{user.is_organizer ? 'Бизнес-аккаунт' : 'Обычный аккаунт'}</option>
          </select>
        </div>
      </div>

      {/* Profile card */}
      <div className="max-w-[700px]">
        {/* Avatar + Personal info */}
        <div className="flex items-start gap-6 mb-6">
          <div className="w-16 h-16 bg-[#6366F1] rounded-full flex items-center justify-center text-white text-2xl font-bold flex-shrink-0 overflow-hidden">
            {user.profile_image ? (
              <img src={user.profile_image} alt="Profile" className="w-full h-full object-cover" />
            ) : (
              user.first_name?.[0]?.toUpperCase() || '?'
            )}
          </div>
          <div>
            <h3 className="text-sm font-semibold text-black mb-1">Личная информация</h3>
            <p className="text-sm text-gray-600">{user.first_name} {user.last_name}</p>
            <p className="text-xs text-gray-400 mt-0.5">
              Дата рождения: {user.date_of_birth ? new Date(user.date_of_birth).toLocaleDateString('ru-RU') : '—'}
            </p>
          </div>
        </div>

        {/* Contact info */}
        <div className="mb-6">
          <h3 className="text-sm font-semibold text-black mb-2">Контактная информация</h3>
          <div className="flex gap-8">
            <div>
              <p className="text-[11px] text-gray-400">Телефон</p>
              <p className="text-sm text-black">{user.phone_number || '—'}</p>
            </div>
            <div>
              <p className="text-[11px] text-gray-400">Эл. почта</p>
              <p className="text-sm text-black">{user.email}</p>
            </div>
          </div>
        </div>

        {/* 2FA Security */}
        <div className="mb-6">
          <h3 className="text-sm font-semibold text-black mb-2">Безопасность</h3>
          
          {error && (
            <div className="bg-red-50 text-red-500 text-sm rounded-lg p-3 mb-3">{error}</div>
          )}
          {success && (
            <div className="bg-green-50 text-green-600 text-sm rounded-lg p-3 mb-3">{success}</div>
          )}
          
          <div className="flex items-center justify-between p-4 border border-gray-200 rounded-lg">
            <div>
              <p className="text-sm font-medium text-black">Двухфакторная аутентификация (2FA)</p>
              <p className="text-xs text-gray-500">
                {user.is_2fa_enabled 
                  ? 'Включена — при входе требуется код из email' 
                  : 'Отключена — рекомендуется для безопасности'}
              </p>
            </div>
            <button
              onClick={handleToggle2FA}
              disabled={loading}
              className={`px-4 py-2 rounded text-sm font-medium transition-colors ${
                user.is_2fa_enabled 
                  ? 'bg-red-50 text-red-600 hover:bg-red-100 border border-red-200' 
                  : 'bg-green-50 text-green-600 hover:bg-green-100 border border-green-200'
              } disabled:opacity-50`}
            >
              {loading ? 'Обработка...' : (user.is_2fa_enabled ? 'Отключить' : 'Включить')}
            </button>
          </div>
        </div>

        {/* 2FA Enable Modal */}
        {show2FAModal && (
          <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
            <div className="bg-white rounded-2xl p-6 w-full max-w-[400px] mx-4">
              <h3 className="text-lg font-bold text-black mb-2">Включить 2FA</h3>
              <p className="text-sm text-gray-500 mb-4">
                Для включения двухфакторной аутентификации введите ваш текущий пароль.
                После включения при каждом входе вам будет отправляться код подтверждения на email.
              </p>
              
              {error && (
                <div className="bg-red-50 text-red-500 text-sm rounded-lg p-3 mb-3">{error}</div>
              )}
              
              <form onSubmit={handleEnable2FA} className="space-y-3">
                <input
                  type="password"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  placeholder="Введите пароль"
                  className="w-full px-4 py-3 border border-gray-200 rounded-md text-sm text-black placeholder-gray-400 focus:border-[#4338CA] outline-none"
                  required
                />
                
                <div className="flex gap-3">
                  <button
                    type="submit"
                    disabled={loading || !password}
                    className="flex-1 py-3 bg-[#333340] text-white rounded text-sm font-medium hover:bg-[#444455] transition-colors disabled:opacity-50"
                  >
                    {loading ? 'Включение...' : 'Включить 2FA'}
                  </button>
                  <button
                    type="button"
                    onClick={() => { setShow2FAModal(false); setPassword(''); setError(''); }}
                    className="flex-1 py-3 bg-white text-black border border-gray-200 rounded text-sm font-medium hover:bg-gray-50"
                  >
                    Отмена
                  </button>
                </div>
              </form>
            </div>
          </div>
        )}

        {/* Preferences */}
        <div className="mb-8">
          <h3 className="text-sm font-semibold text-black mb-2">Предпочтения</h3>
          <div className="flex flex-wrap gap-2">
            {categories.map(cat => {
              const isActive = userPrefs.includes(cat.name);
              return (
                <span key={cat.id} className={`px-3 py-1.5 rounded text-xs font-medium ${isActive ? 'bg-[#333340] text-white' : 'border border-gray-200 text-gray-500'}`}>
                  {cat.name} {isActive ? '✕' : '+'}
                </span>
              );
            })}
            {categories.length === 0 && userPrefs.length > 0 && userPrefs.map(p => (
              <span key={p} className="px-3 py-1.5 rounded text-xs font-medium bg-[#333340] text-white">{p}</span>
            ))}
          </div>
        </div>

        {/* Action buttons */}
        <div className="flex gap-3">
          <Link to="/edit-profile"
            className="flex-1 py-3 bg-[#333340] text-white rounded text-sm font-medium no-underline text-center hover:bg-[#444455]">
            Редактировать
          </Link>
          <button onClick={handleLogout}
            className="flex-1 py-3 bg-white text-black rounded text-sm font-medium border border-gray-200 hover:bg-gray-50 cursor-pointer">
            Выйти
          </button>
        </div>
      </div>
    </div>
  );
}
