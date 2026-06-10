import { useState, useEffect } from 'react';
import { Gift, QrCode, PlusCircle } from 'lucide-react';
import { Link } from 'react-router-dom';
import api from '../api';
import { useAuth } from '../context/AuthContext';

export default function PromosPage() {
  const { user, refreshUser } = useAuth();
  const [promos, setPromos] = useState([]);
  const [myPromos, setMyPromos] = useState([]);
  const [tab, setTab] = useState('catalog');
  const [qrData, setQrData] = useState(null);

  useEffect(() => {
    api.get('/promos/').then(r => setPromos(r.data.results || r.data));
    if (user) api.get('/my-promos/').then(r => setMyPromos(r.data.results || r.data));
  }, [user]);

  const handlePurchase = async (promoId) => {
    try {
      await api.post(`/promos/${promoId}/purchase/`);
      refreshUser();
      api.get('/my-promos/').then(r => setMyPromos(r.data.results || r.data));
      setTab('my');
    } catch (err) {
      alert(err.response?.data?.error || 'Ошибка');
    }
  };

  const showQR = async (userPromoId) => {
    const { data } = await api.get(`/my-promos/${userPromoId}/qr/`);
    setQrData(data);
  };

  return (
    <div className="max-w-3xl">
      <div className="mb-6">
        <h1 className="text-2xl font-bold text-black">Промокоды</h1>
      </div>

      {user && (
        <div className="flex gap-2 mb-6">
          <TabBtn active={tab === 'catalog'} onClick={() => setTab('catalog')}>Каталог</TabBtn>
          <TabBtn active={tab === 'my'} onClick={() => setTab('my')}>Мои промокоды</TabBtn>
        </div>
      )}

      {tab === 'catalog' && (
        <div className="space-y-3">
          {promos.length === 0 && <p className="text-center text-[#8B8B8B] py-12">Промокодов пока нет</p>}
          {promos.map(p => (
            <div key={p.id} className="bg-white rounded-[10px] shadow-[0_0_10px_rgba(0,0,0,0.08)] p-5 flex items-center justify-between">
              <div>
                <div className="flex items-center gap-2">
                  <Gift size={18} className="text-[#4338CA]" />
                  <h3 className="text-base font-semibold text-[#1A1A2E]">{p.title}</h3>
                </div>
                {p.description && <p className="text-sm text-[#8B8B8B] mt-1">{p.description}</p>}
                <div className="flex items-center gap-3 mt-2 text-xs text-[#ACACAC]">
                  <span>От: {p.organizer.company_name || `${p.organizer.first_name} ${p.organizer.last_name}`}</span>
                  {p.valid_until && <span>До: {new Date(p.valid_until).toLocaleDateString('ru-RU')}</span>}
                </div>
              </div>
              <div className="text-right">
                <p className="text-lg font-bold text-[#4338CA]">{p.bonus_price} <span className="text-sm font-normal text-[#8B8B8B]">баллов</span></p>
                {user && p.is_available && (
                  <button onClick={() => handlePurchase(p.id)}
                    className="mt-2 px-4 py-1.5 bg-[#333340] text-white rounded-md text-sm hover:bg-[#444455] cursor-pointer border-0">
                    Купить
                  </button>
                )}
                {!p.is_available && <span className="text-xs text-[#8B8B8B]">Недоступен</span>}
              </div>
            </div>
          ))}
        </div>
      )}

      {tab === 'my' && (
        <div className="space-y-3">
          {myPromos.length === 0 && <p className="text-center text-[#8B8B8B] py-12">Вы ещё не купили промокодов</p>}
          {myPromos.map(up => (
            <div key={up.id} className="bg-white rounded-[10px] shadow-[0_0_10px_rgba(0,0,0,0.08)] p-5 flex items-center justify-between">
              <div>
                <h3 className="text-base font-semibold text-[#1A1A2E]">{up.promo.title}</h3>
                <p className="text-sm text-[#8B8B8B] mt-1">Код: <code className="bg-[#F5F5F9] px-2 py-0.5 rounded text-sm text-[#1A1A2E]">{up.unique_code}</code></p>
                <p className="text-xs text-[#ACACAC] mt-1">{new Date(up.purchased_at).toLocaleDateString('ru-RU')}</p>
              </div>
              <button onClick={() => showQR(up.id)}
                className="flex items-center gap-2 px-4 py-2 border border-[#E0E0E6] rounded-md text-sm text-[#1A1A2E] hover:bg-[#F0F0F4] cursor-pointer bg-transparent">
                <QrCode size={18} /> QR-код
              </button>
            </div>
          ))}
        </div>
      )}

      {/* QR Modal */}
      {qrData && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50" onClick={() => setQrData(null)}>
          <div className="bg-white rounded-[10px] shadow-[0_0_10px_rgba(0,0,0,0.08)] p-8 text-center" onClick={e => e.stopPropagation()}>
            <h3 className="text-lg font-semibold text-[#1A1A2E] mb-4">QR-код промокода</h3>
            <img src={qrData.qr_image} alt="QR" className="w-64 h-64 mx-auto mb-4" />
            <p className="text-sm text-[#8B8B8B]">Код: <span className="font-mono font-bold text-[#1A1A2E]">{qrData.unique_code}</span></p>
            <button onClick={() => setQrData(null)}
              className="mt-4 px-6 py-2 border border-[#E0E0E6] rounded-md text-sm text-[#1A1A2E] hover:bg-[#F0F0F4] cursor-pointer bg-white">
              Закрыть
            </button>
          </div>
        </div>
      )}
    </div>
  );
}

function TabBtn({ active, onClick, children }) {
  return (
    <button onClick={onClick}
      className={`px-3 py-1.5 rounded text-[13px] cursor-pointer transition-colors ${
        active ? 'bg-[#333340] text-white border-0' : 'bg-transparent text-[#1A1A2E] border border-[#E0E0E6] hover:border-[#4338CA]'
      }`}>
      {children}
    </button>
  );
}
