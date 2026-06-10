import { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import { Check, X, UserCheck, UserX } from 'lucide-react';
import api from '../api';

const statusColors = { pending: 'text-[#F5A623]', approved: 'text-[#10B981]', rejected: 'text-[#EF4444]', cancelled: 'text-[#8B8B8B]' };
const statusLabels = { pending: 'В обработке', approved: 'Подтверждено', rejected: 'Отклонено', cancelled: 'Отменено' };
const attLabels = { attended: 'Посетил', missed: 'Не пришёл', unknown: '' };
const attColors = { attended: 'text-[#10B981]', missed: 'text-[#EF4444]', unknown: '' };

export default function EventApplicationsPage() {
  const { eventId } = useParams();
  const [apps, setApps] = useState([]);
  const [event, setEvent] = useState(null);

  const load = () => {
    api.get(`/events/${eventId}/applications/`).then(r => setApps(r.data.results || r.data));
    api.get(`/events/${eventId}/`).then(r => setEvent(r.data));
  };

  useEffect(() => { load(); }, [eventId]);

  const handleAction = async (appId, action) => {
    await api.patch(`/applications/${appId}/action/`, { action });
    load();
  };

  const btnCls = "p-2 rounded-md cursor-pointer border";

  return (
    <div className="max-w-3xl">
      <h1 className="text-2xl font-bold text-black mb-2">Заявки</h1>
      {event && <p className="text-[#8B8B8B] mb-6">{event.title}</p>}

      {apps.length === 0 && <p className="text-center text-[#8B8B8B] py-12">Заявок пока нет</p>}

      <div className="space-y-3">
        {apps.map(app => (
          <div key={app.id} className="bg-white rounded-[10px] shadow-[0_0_10px_rgba(0,0,0,0.08)] p-4">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-[#1A1A2E]">
                  {app.user.first_name} {app.user.last_name}
                </p>
                <div className="flex items-center gap-3 mt-1">
                  <span className={`text-sm font-semibold ${statusColors[app.status]}`}>{statusLabels[app.status]}</span>
                  {app.attendance !== 'unknown' && (
                    <span className={`text-sm font-semibold ${attColors[app.attendance]}`}>{attLabels[app.attendance]}</span>
                  )}
                </div>
                <p className="text-xs text-[#ACACAC] mt-1">{new Date(app.created_at).toLocaleDateString('ru-RU')}</p>
              </div>

              <div className="flex items-center gap-2">
                {app.status === 'pending' && (
                  <>
                    <button onClick={() => handleAction(app.id, 'approve')}
                      className={`${btnCls} text-[#10B981] border-[#10B981]/30 hover:bg-green-50 bg-transparent`} title="Одобрить">
                      <Check size={18} />
                    </button>
                    <button onClick={() => handleAction(app.id, 'reject')}
                      className={`${btnCls} text-[#EF4444] border-[#EF4444]/30 hover:bg-red-50 bg-transparent`} title="Отклонить">
                      <X size={18} />
                    </button>
                  </>
                )}
                {app.status === 'approved' && app.attendance === 'unknown' && (
                  <>
                    <button onClick={() => handleAction(app.id, 'attended')}
                      className={`${btnCls} text-[#10B981] border-[#10B981]/30 hover:bg-green-50 bg-transparent`} title="Посетил">
                      <UserCheck size={18} />
                    </button>
                    <button onClick={() => handleAction(app.id, 'missed')}
                      className={`${btnCls} text-[#EF4444] border-[#EF4444]/30 hover:bg-red-50 bg-transparent`} title="Не пришёл">
                      <UserX size={18} />
                    </button>
                  </>
                )}
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
