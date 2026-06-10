import { Globe, Download } from 'lucide-react';
import { useNavigate } from 'react-router-dom';

export default function LandingPage() {
  const navigate = useNavigate();

  return (
    <div className="min-h-screen bg-white flex flex-col items-center justify-center px-8">
      <div className="text-center max-w-2xl">
        {/* Logo */}
        <div className="w-16 h-16 bg-black rounded-full flex items-center justify-center text-white font-bold text-3xl mx-auto mb-8">
          i
        </div>

        {/* Main text */}
        <h1 className="text-5xl font-bold text-black mb-4">iCelebrate</h1>
        
        <p className="text-xl text-gray-700 mb-8">
          Умный способ организовать групповые мероприятия
        </p>

        {/* CTA Buttons */}
        <div className="flex flex-col sm:flex-row items-center justify-center gap-4 mb-12">
          <button
            onClick={() => navigate('/login')}
            className="flex items-center gap-2 px-8 py-3 bg-black text-white rounded-lg font-medium hover:bg-gray-800 transition-colors w-full sm:w-auto justify-center"
          >
            <Globe size={18} />
            Продолжить в веб-версии
          </button>
          <button
            onClick={() => {
              const link = document.createElement('a');
              link.href = '/iCelebrate-Setup.exe';
              link.download = 'iCelebrate-Setup.exe';
              document.body.appendChild(link);
              link.click();
              document.body.removeChild(link);
            }}
            className="flex items-center gap-2 px-8 py-3 border-2 border-black text-black rounded-lg font-medium hover:bg-gray-50 transition-colors w-full sm:w-auto justify-center"
          >
            <Download size={18} />
            Скачать десктопное приложение
          </button>
        </div>

        <p className="text-gray-500 text-sm">
          Доступно для Windows, macOS и Linux
        </p>
      </div>
    </div>
  );
}
