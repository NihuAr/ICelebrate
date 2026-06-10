import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import api from '../api';

export default function PasswordResetPage() {
  const [step, setStep] = useState(1);
  const [email, setEmail] = useState('');
  const [otpCode, setOtpCode] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [otpId, setOtpId] = useState(null);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [loading, setLoading] = useState(false);
  const [resendTimer, setResendTimer] = useState(0);
  const navigate = useNavigate();

  // Step 1: Request reset code
  const handleStep1 = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      const { data } = await api.post('/auth/password-reset/request/', { email });
      setOtpId(data.otp_id);
      setSuccess('Код отправлен на ваш email');
      setStep(2);
      startResendTimer();
    } catch (err) {
      setError(err.response?.data?.error || 'Не удалось отправить код');
    } finally {
      setLoading(false);
    }
  };

  // Step 2: Confirm reset
  const handleStep2 = async (e) => {
    e.preventDefault();
    setError('');
    setSuccess('');

    if (newPassword !== confirmPassword) {
      setError('Пароли не совпадают');
      return;
    }

    if (newPassword.length < 6) {
      setError('Пароль должен быть минимум 6 символов');
      return;
    }

    setLoading(true);

    try {
      await api.post('/auth/password-reset/confirm/', {
        otp_id: otpId,
        code: otpCode,
        new_password: newPassword,
      });
      setSuccess('Пароль успешно изменён! Перенаправление...');
      setTimeout(() => navigate('/login'), 2000);
    } catch (err) {
      setError(err.response?.data?.error || 'Неверный код или ошибка сброса');
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
      const { data } = await api.post('/auth/password-reset/request/', { email });
      setOtpId(data.otp_id);
      setSuccess('Новый код отправлен');
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
    setNewPassword('');
    setConfirmPassword('');
    setError('');
    setSuccess('');
  };

  return (
    <div className="min-h-screen bg-[#F8F7FC] flex items-center justify-center">
      <div className="bg-white rounded-2xl shadow-sm border border-gray-100 p-12 w-full max-w-[480px]">
        {step === 1 ? (
          <>
            <h1 className="text-xl font-bold text-black text-center mb-2">Сброс пароля</h1>
            <p className="text-sm text-gray-400 text-center mb-8">
              Введите email для получения кода сброса
            </p>

            {error && (
              <div className="bg-red-50 text-red-500 text-sm rounded-lg p-3 mb-4">{error}</div>
            )}
            {success && (
              <div className="bg-green-50 text-green-600 text-sm rounded-lg p-3 mb-4">{success}</div>
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

              <button
                type="submit"
                disabled={loading}
                className="w-full py-3 bg-[#333340] text-white rounded-md text-sm font-medium hover:bg-[#444455] transition-colors cursor-pointer disabled:opacity-50"
              >
                {loading ? 'Отправка...' : 'Получить код'}
              </button>
            </form>

            <p className="text-center text-[13px] text-gray-400 mt-5">
              <Link to="/login" className="text-black font-semibold no-underline hover:underline">
                ← Вернуться к входу
              </Link>
            </p>
          </>
        ) : (
          <>
            <h1 className="text-xl font-bold text-black text-center mb-2">Новый пароль</h1>
            <p className="text-sm text-gray-400 text-center mb-8">
              Введите код из письма и новый пароль
            </p>

            {error && (
              <div className="bg-red-50 text-red-500 text-sm rounded-lg p-3 mb-4">{error}</div>
            )}
            {success && (
              <div className="bg-green-50 text-green-600 text-sm rounded-lg p-3 mb-4">{success}</div>
            )}

            <form onSubmit={handleStep2} className="space-y-3">
              <input
                type="text"
                value={otpCode}
                onChange={(e) => setOtpCode(e.target.value.replace(/\D/g, '').slice(0, 6))}
                placeholder="Код из email"
                className="w-full px-4 py-3 border border-gray-200 rounded-md text-sm text-black placeholder-gray-400 focus:border-[#4338CA] outline-none text-center text-lg tracking-widest"
                required
                maxLength={6}
              />
              <input
                type="password"
                value={newPassword}
                onChange={(e) => setNewPassword(e.target.value)}
                placeholder="Новый пароль (мин. 6 символов)"
                className="w-full px-4 py-3 border border-gray-200 rounded-md text-sm text-black placeholder-gray-400 focus:border-[#4338CA] outline-none"
                required
                minLength={6}
              />
              <input
                type="password"
                value={confirmPassword}
                onChange={(e) => setConfirmPassword(e.target.value)}
                placeholder="Подтвердите пароль"
                className="w-full px-4 py-3 border border-gray-200 rounded-md text-sm text-black placeholder-gray-400 focus:border-[#4338CA] outline-none"
                required
              />

              <button
                type="submit"
                disabled={loading || otpCode.length !== 6}
                className="w-full py-3 bg-[#333340] text-white rounded-md text-sm font-medium hover:bg-[#444455] transition-colors cursor-pointer disabled:opacity-50"
              >
                {loading ? 'Сохранение...' : 'Изменить пароль'}
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
