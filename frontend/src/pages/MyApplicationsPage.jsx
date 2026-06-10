import { useState, useEffect } from 'react';
import api from '../api';
import { Link } from 'react-router-dom';

const statusColors = { pending: 'text-[#F5A623]', approved: 'text-[#10B981]', rejected: 'text-[#EF4444]', cancelled: 'text-[#8B8B8B]' };
const statusLabels = { pending: 'В обработке', approved: 'Подтверждено', rejected: 'Отклонено', cancelled: 'Отменено' };
const attLabels = { attended: 'Посетил', missed: 'Не пришёл', unknown: '' };
const attColors = { attended: 'text-[#10B981]', missed: 'text-[#EF4444]', unknown: '' };

export default function MyApplicationsPage() {
  const [apps, setApps] = useState([]);

  useEffect(() => {
    api.get('/applications/my/').then(r => setApps(r.data.results || r.data));
  }, []);

  const handleCancel = async (appId) => {
    await api.patch(`/applications/${appId}/action/`, { action: 'cancel' });
    api.get('/applications/my/').then(r => setApps(r.data.results || r.data));
  };

  return (
    <div className="max-w-3xl">
      <h1 className="text-2xl font-bold text-black mb-6">Мои заявки</h1>
      {apps.length === 0 && <p className="text-center text-[#8B8B8B] py-12">Заявок пока нет</p>}
      <div className="space-y-3">
        {apps.map(app => (
          <div key={app.id} className="bg-white rounded-[10px] shadow-[0_0_10px_rgba(0,0,0,0.08)] p-4 flex items-center justify-between">
            <div>
              <Link to={`/events/${app.event}`} className="text-sm font-medium text-[#1A1A2E] hover:text-[#4338CA] no-underline">
                {app.event_title}
              </Link>
              <div className="flex items-center gap-3 mt-1.5">
                <span className={`text-sm font-semibold ${statusColors[app.status]}`}>{statusLabels[app.status]}</span>
                {app.attendance !== 'unknown' && (
                  <span className={`text-sm font-semibold ${attColors[app.attendance]}`}>{attLabels[app.attendance]}</span>
                )}
              </div>
              <p className="text-xs text-[#ACACAC] mt-1">Подана: {new Date(app.created_at).toLocaleDateString('ru-RU')}</p>
            </div>
            {(app.status === 'pending' || app.status === 'approved') && (
              <button onClick={() => handleCancel(app.id)}
                className="px-3 py-1.5 text-[#EF4444] border border-[#EF4444]/30 rounded-md text-sm hover:bg-red-50 cursor-pointer bg-transparent">
                Отменить
              </button>
            )}
          </div>
        ))}
      </div>
    </div>
  );
}
