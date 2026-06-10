import { useState, useEffect } from 'react';
import api from '../api';

export default function NotificationsPage() {
  const [notifications, setNotifications] = useState([]);

  const load = () => api.get('/notifications/').then(r => setNotifications(r.data.results || r.data));

  useEffect(() => { load(); }, []);

  const clearAll = async () => {
    await api.post('/notifications/clear_all/');
    load();
  };

  // Group notifications by month
  const grouped = notifications.reduce((acc, n) => {
    const date = new Date(n.created_at);
    const key = date.toLocaleDateString('ru-RU', { month: 'long', year: 'numeric' });
    if (!acc[key]) acc[key] = [];
    acc[key].push(n);
    return acc;
  }, {});

  return (
    <div className="max-w-2xl">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-black">Уведомления</h1>
        {notifications.length > 0 && (
          <button onClick={clearAll} className="text-sm text-blue-600 hover:underline cursor-pointer bg-transparent border-0">
            Очистить все
          </button>
        )}
      </div>

      {notifications.length === 0 && <p className="text-sm text-gray-400">Уведомлений нет</p>}

      {Object.entries(grouped).map(([month, items]) => (
        <div key={month} className="mb-6">
          <p className="text-xs text-gray-400 font-medium mb-3">{month}</p>
          <div className="space-y-4">
            {items.map(n => (
              <div key={n.id} className="border-b border-gray-100 pb-4">
                <p className="text-[11px] text-gray-400 mb-1">
                  {new Date(n.created_at).toLocaleDateString('ru-RU')}
                </p>
                <p className="text-sm font-semibold text-black mb-1">{n.title}</p>
                <p className="text-[13px] text-gray-500 leading-relaxed">{n.message}</p>

                {!n.is_read && (
                  <div className="flex gap-3 mt-3">
                    <button
                      onClick={async () => { await api.patch(`/notifications/${n.id}/`, { is_read: true }); load(); }}
                      className="flex-1 py-2 bg-[#333340] text-white rounded text-xs font-medium hover:bg-[#444455] cursor-pointer border-0"
                    >
                      Подтверждаю
                    </button>
                    <button
                      onClick={async () => { await api.patch(`/notifications/${n.id}/`, { is_read: true }); load(); }}
                      className="flex-1 py-2 bg-white text-black rounded text-xs font-medium border border-gray-200 hover:bg-gray-50 cursor-pointer"
                    >
                      Отклоняю
                    </button>
                  </div>
                )}
              </div>
            ))}
          </div>
        </div>
      ))}
    </div>
  );
}
