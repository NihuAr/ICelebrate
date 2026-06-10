import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import api from '../api';

export default function LoginPage() {
  const [step, setStep] = useState(1);
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [otpCode, setOtpCode] = useState('');
  const [otpId, setOtpId] = useState(null);
  const [rememberDevice, setRememberDevice] = useState(false);
  const [deviceToken, setDeviceToken] = useState(localStorage.getItem('device_token') || '');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const [resendTimer, setResendTimer] = useState(0);
  const { loginStep2 } = useAuth();
  const navigate = useNavigate();

  // Step 1: Validate credentials
  const handleStep1 = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    
    try {
      const { data } = await api.post('/auth/login/', {
        email,
        password,
        device_token: deviceToken || undefined,
      });

      if (data.requires_otp) {
        // 2FA required - go to step 2
        setOtpId(data.otp_id);
        setStep(2);
        startResendTimer();
      } else {
        // No 2FA - login successful
        localStorage.setItem('access_token', data.access);
        localStorage.setItem('refresh_token', data.refresh);
        if (data.device_token) {
          localStorage.setItem('device_token', data.device_token);
        }
        navigate('/');
        window.location.reload();
      }
    } catch (err) {
      const msg = err.response?.data?.error;
      if (msg && msg.includes('Подтвердите email')) {
        setError('Требуется подтверждение email. Проверьте почту или запросите код повторно.');
      } else {
        setError(msg || 'Неверный email или пароль');
      }
    } finally {
      setLoading(false);
    }
  };

  // Step 2: Verify OTP
  const handleStep2 = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      await loginStep2(otpId, otpCode, rememberDevice);
      navigate('/');
    } catch (err) {
      setError(err.response?.data?.error || 'Неверный код');
    } finally {
      setLoading(false);
    }
  };

  const startResendTimer = () => {
    setResendTimer(60);
    const interval = setInterval(() => {
      setResendTimer((prev) => {
        if (prev <= 1) {
          clearInterval(interval);
          return 0;
        }
        return prev - 1;
      });
    }, 1000);
  };

  const handleResend = async () => {
    if (resendTimer > 0) return;
    
    setError('');
    setLoading(true);
    
    try {
      const { data } = await api.post('/auth/login/', {
        email,
        password,
      });
      setOtpId(data.otp_id);
      startResendTimer();
    } catch (err) {
      setError(err.response?.data?.error || 'Не удалось отправить код');
    } finally {
      setLoading(false);
    }
  };

  const goBack = () => {
    setStep(1);
    setOtpCode('');
    setError('');
  };

  return (
    <div className="min-h-screen bg-[#F8F7FC] flex items-center justify-center">
      <div className="bg-white rounded-2xl shadow-sm border border-gray-100 p-12 w-full max-w-[480px]">
        {step === 1 ? (
          <>
            <h1 className="text-xl font-bold text-black text-center mb-2">Войти в аккаунт</h1>
            <p className="text-sm text-gray-400 text-center mb-8">Войдите, чтобы продолжить</p>

            {error && (
              <div className="bg-red-50 text-red-500 text-sm rounded-lg p-3 mb-4">{error}</div>
            )}
            
            <form onSubmit={handleStep1} className="space-y-3">
              <input
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                placeholder="Введите email"
                className="w-full px-4 py-3 border border-gray-200 rounded-md text-sm text-black placeholder-gray-400 focus:border-[#4338CA] outline-none"
                required
              />
              <input
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                placeholder="Введите пароль"
                className="w-full px-4 py-3 border border-gray-200 rounded-md text-sm text-black placeholder-gray-400 focus:border-[#4338CA] outline-none"
                required
              />
              
              <div className="flex items-center">
                <input
                  type="checkbox"
                  id="rememberDevice"
                  checked={rememberDevice}
                  onChange={(e) => setRememberDevice(e.target.checked)}
                  className="mr-2"
                />
                <label htmlFor="rememberDevice" className="text-sm text-gray-600">
                  Запомнить устройство на 14 дней
                </label>
              </div>
              
              <button
                type="submit"
                disabled={loading}
                className="w-full py-3 bg-[#333340] text-white rounded-md text-sm font-medium hover:bg-[#444455] transition-colors cursor-pointer disabled:opacity-50"
              >
                {loading ? 'Вход...' : 'Войти'}
              </button>
            </form>
            
            <div className="flex justify-between items-center mt-4">
              <Link to="/password-reset" className="text-sm text-[#4338CA] hover:underline">
                Забыли пароль?
              </Link>
            </div>
            
            <p className="text-center text-[13px] text-gray-400 mt-5">
              Нет аккаунта?{' '}
              <Link to="/register" className="text-black font-semibold no-underline hover:underline">Зарегистрируйтесь</Link>
            </p>
          </>
        ) : (
          <>
            <h1 className="text-xl font-bold text-black text-center mb-2">Подтверждение входа</h1>
            <p className="text-sm text-gray-400 text-center mb-8">
              Введите 6-значный код, отправленный на {email}
            </p>

            {error && (
              <div className="bg-red-50 text-red-500 text-sm rounded-lg p-3 mb-4">{error}</div>
            )}
            
            <form onSubmit={handleStep2} className="space-y-3">
              <input
                type="text"
                value={otpCode}
                onChange={(e) => setOtpCode(e.target.value.replace(/\D/g, '').slice(0, 6))}
                placeholder="Введите код"
                className="w-full px-4 py-3 border border-gray-200 rounded-md text-sm text-black placeholder-gray-400 focus:border-[#4338CA] outline-none text-center text-lg tracking-widest"
                required
                maxLength={6}
              />
              
              <div className="flex items-center">
                <input
                  type="checkbox"
                  id="rememberDeviceStep2"
                  checked={rememberDevice}
                  onChange={(e) => setRememberDevice(e.target.checked)}
                  className="mr-2"
                />
                <label htmlFor="rememberDeviceStep2" className="text-sm text-gray-600">
                  Запомнить устройство на 14 дней
                </label>
              </div>
              
              <button
                type="submit"
                disabled={loading || otpCode.length !== 6}
                className="w-full py-3 bg-[#333340] text-white rounded-md text-sm font-medium hover:bg-[#444455] transition-colors cursor-pointer disabled:opacity-50"
              >
                {loading ? 'Проверка...' : 'Подтвердить'}
              </button>
            </form>
            
            <div className="flex justify-between items-center mt-4">
              <button
                onClick={handleResend}
                disabled={resendTimer > 0 || loading}
                className="text-sm text-[#4338CA] hover:underline disabled:text-gray-400"
              >
                {resendTimer > 0 ? `Отправить повторно (${resendTimer})` : 'Отправить код повторно'}
              </button>
              <button
                onClick={goBack}
                className="text-sm text-gray-500 hover:text-black"
              >
                ← Назад
              </button>
            </div>
          </>
        )}
      </div>
    </div>
  );
}
