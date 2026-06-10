import { useState, useEffect } from 'react';
import { TrendingUp, TrendingDown } from 'lucide-react';
import api from '../api';
import { useAuth } from '../context/AuthContext';

export default function BonusHistoryPage() {
  const { user } = useAuth();
  const [transactions, setTransactions] = useState([]);

  useEffect(() => {
    api.get('/bonus/history/').then(r => setTransactions(r.data.results || r.data));
  }, []);

  return (
    <div className="max-w-2xl">
      <h1 className="text-2xl font-bold text-black mb-2">История бонусов</h1>
      <p className="text-sm text-gray-500 mb-6">Текущий баланс: <span className="font-bold text-[#4338CA]">{user?.bonus_balance || 0}</span> баллов</p>

      {transactions.length === 0 && <p className="text-sm text-gray-400">Операций пока нет</p>}

      <div className="space-y-3">
        {transactions.map(t => (
          <div key={t.id} className="border border-gray-100 rounded-lg p-4 flex items-center justify-between">
            <div className="flex items-center gap-3">
              {t.amount > 0 ? (
                <div className="w-10 h-10 bg-green-50 rounded-full flex items-center justify-center">
                  <TrendingUp size={18} className="text-green-500" />
                </div>
              ) : (
                <div className="w-10 h-10 bg-red-50 rounded-full flex items-center justify-center">
                  <TrendingDown size={18} className="text-red-500" />
                </div>
              )}
              <div>
                <p className="text-sm font-medium text-black">{t.description}</p>
                <p className="text-xs text-gray-400">{new Date(t.created_at).toLocaleString('ru-RU')}</p>
              </div>
            </div>
            <span className={`font-bold ${t.amount > 0 ? 'text-green-500' : 'text-red-500'}`}>
              {t.amount > 0 ? '+' : ''}{t.amount}
            </span>
          </div>
        ))}
      </div>
    </div>
  );
}
