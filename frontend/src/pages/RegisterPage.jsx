import { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { ArrowLeft } from 'lucide-react';
import { useAuth } from '../context/AuthContext';
import api from '../api';

export default function RegisterPage() {
  const [step, setStep] = useState(1);
  const [categories, setCategories] = useState([]);
  const [selectedPrefs, setSelectedPrefs] = useState([]);
  const [agreedToTerms, setAgreedToTerms] = useState(false);
  const [form, setForm] = useState({
    username: '', email: '', password: '', first_name: '', last_name: '',
    phone_number: '', date_of_birth: '',
  });
  const [error, setError] = useState('');
  const [info, setInfo] = useState('');
  const [otpId, setOtpId] = useState(null);
  const [otpCode, setOtpCode] = useState('');
  const [resendTimer, setResendTimer] = useState(0);
  const [registrationDone, setRegistrationDone] = useState(false);
  const { register, verifyEmailRequest, verifyEmailConfirm } = useAuth();
  const navigate = useNavigate();

  useEffect(() => {
    api.get('/categories/')
      .then(r => {
        const cats = Array.isArray(r.data) ? r.data : r.data.results || [];
        setCategories(cats);
      })
      .catch(err => {
        console.error('Ошибка загрузки категорий:', err);
        setCategories([]);
      });
  }, []);

  const handleChange = (e) => setForm({ ...form, [e.target.name]: e.target.value });

  const validateStep = () => {
    if (step === 1) {
      if (!form.first_name.trim()) { setError('Укажите имя'); return false; }
      if (!form.last_name.trim()) { setError('Укажите фамилию'); return false; }
      if (!agreedToTerms) { setError('Согласитесь на обработку персональных данных'); return false; }
    }
    if (step === 2) {
      if (!form.username.trim()) { setError('Укажите логин'); return false; }
      if (!form.email.trim()) { setError('Укажите эл. почту'); return false; }
      if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email)) { setError('Некорректный email'); return false; }
      if (!form.password.trim()) { setError('Укажите пароль'); return false; }
      if (form.password.length < 6) { setError('Пароль должен быть минимум 6 символов'); return false; }
    }
    if (step === 3) {
      if (!otpCode || otpCode.length < 6) { setError('Введите 6-значный код'); return false; }
    }
    setError('');
    return true;
  };

  const handleNext = async () => {
    if (!validateStep()) return;

    // Step transitions
    if (step === 1) {
      setStep(2);
      return;
    }

    if (step === 2) {
      try {
        const payload = { ...form };
        await register(payload);
        setRegistrationDone(true);
        const { otp_id } = await verifyEmailRequest(form.email);
        setOtpId(otp_id);
        setInfo('Код подтверждения отправлен на email');
        startResendTimer();
        setStep(3);
      } catch (err) {
        const data = err.response?.data;
        setError(data ? Object.values(data).flat().join('. ') : 'Ошибка регистрации');
      }
      return;
    }

    if (step === 3) {
      await handleVerify();
      return;
    }
  };

  const handleBack = () => {
    setError('');
    if (step > 1) setStep(step - 1);
  };

  const handleBackToLogin = () => {
    navigate('/login');
  };

  const handleFinishInterests = () => {
    navigate('/login');
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
    setInfo('');
    try {
      const { otp_id } = await verifyEmailRequest(form.email);
      setOtpId(otp_id);
      setInfo('Код отправлен повторно');
      startResendTimer();
    } catch (err) {
      setError(err.response?.data?.error || 'Не удалось отправить код');
    }
  };

  const handleVerify = async () => {
    setError('');
    setInfo('');
    if (!otpCode || otpCode.length < 6) {
      setError('Введите 6-значный код');
      return;
    }
    try {
      await verifyEmailConfirm(otpId, otpCode);
      setInfo('Email подтвержден');
      setStep(4);
    } catch (err) {
      setError(err.response?.data?.error || 'Неверный код');
    }
  };

  const inputCls = "w-full px-4 py-3 border border-gray-200 rounded-md text-sm text-black placeholder-gray-400 focus:border-[#4338CA] outline-none";

  return (
    <div className="min-h-screen bg-[#F8F7FC] flex items-center justify-center">
      <div className="bg-white rounded-2xl shadow-sm border border-gray-100 p-10 w-full max-w-[420px] relative">
        {/* Back button - for step 1 goes to login, otherwise previous step */}
        {step > 1 ? (
          <button
            onClick={handleBack}
            className="absolute left-4 top-4 p-1.5 text-gray-600 hover:text-black transition-colors"
            title="Назад"
          >
            <ArrowLeft size={18} />
          </button>
        ) : (
          <button
            onClick={handleBackToLogin}
            className="absolute left-4 top-4 p-1.5 text-gray-600 hover:text-black transition-colors"
            title="К входу"
          >
            <ArrowLeft size={18} />
          </button>
        )}

        {/* Progress bar */}
        <div className="flex gap-1 mb-6 mt-6">
          <div className={`flex-1 h-[3px] rounded ${step >= 1 ? 'bg-[#4338CA]' : 'bg-gray-200'}`} />
          <div className={`flex-1 h-[3px] rounded ${step >= 2 ? 'bg-[#4338CA]' : 'bg-gray-200'}`} />
          <div className={`flex-1 h-[3px] rounded ${step >= 3 ? 'bg-[#4338CA]' : 'bg-gray-200'}`} />
          <div className={`flex-1 h-[3px] rounded ${step >= 4 ? 'bg-[#4338CA]' : 'bg-gray-200'}`} />
        </div>

        <h2 className="text-xl font-bold text-black text-center mb-1">Создать аккаунт</h2>
        <p className="text-sm text-gray-400 text-center mb-6">
          {step === 1 && 'Шаг 1 из 4: Личные данные'}
          {step === 2 && 'Шаг 2 из 4: Контактные данные'}
          {step === 3 && 'Шаг 3 из 4: Код из email'}
          {step === 4 && 'Шаг 4 из 4: Выберите увлечения'}
        </p>

        {error && <div className="bg-red-50 text-red-500 text-sm rounded-lg p-3 mb-4">{error}</div>}
        {info && <div className="bg-green-50 text-green-600 text-sm rounded-lg p-3 mb-4">{info}</div>}

        {/* Step 1 */}
        {step === 1 && (
          <div className="space-y-3">
            <input name="first_name" value={form.first_name} onChange={handleChange} placeholder="Имя" className={inputCls} />
            <input name="last_name" value={form.last_name} onChange={handleChange} placeholder="Фамилия" className={inputCls} />
            <input name="date_of_birth" type="date" value={form.date_of_birth} onChange={handleChange} className={inputCls} />
            
            {/* Terms checkbox on Step 1 */}
            <div className="flex items-start gap-3 p-3 bg-gray-50 rounded-lg mt-4">
              <input
                type="checkbox"
                checked={agreedToTerms}
                onChange={(e) => setAgreedToTerms(e.target.checked)}
                className="mt-1 w-4 h-4 cursor-pointer flex-shrink-0"
              />
              <label className="text-xs text-gray-600 cursor-pointer">
                Я согласен(а) на обработку персональных данных и ознакомлен(а) с политикой конфиденциальности
              </label>
            </div>

            <button onClick={handleNext}
              disabled={!agreedToTerms}
              className={`w-full py-3 rounded-md text-sm font-medium transition-colors cursor-pointer mt-4 ${
                agreedToTerms ? 'bg-[#333340] text-white hover:bg-[#444455]' : 'bg-gray-200 text-gray-500 cursor-not-allowed'
              }`}>
              Далее
            </button>
          </div>
        )}

        {/* Step 2 */}
        {step === 2 && (
          <div className="space-y-3">
            <input name="username" value={form.username} onChange={handleChange} placeholder="Логин" className={inputCls} />
            <input name="email" type="email" value={form.email} onChange={handleChange} placeholder="Эл. почта" className={inputCls} />
            <input name="password" type="password" value={form.password} onChange={handleChange} placeholder="Пароль" className={inputCls} />
            <input name="phone_number" value={form.phone_number} onChange={handleChange} placeholder="Номер телефона" className={inputCls} />
            <button onClick={handleNext}
              className="w-full py-3 bg-[#333340] text-white rounded-md text-sm font-medium hover:bg-[#444455] transition-colors cursor-pointer mt-4">
              Далее
            </button>
          </div>
        )}

        {/* Step 3: email code */}
        {step === 3 && (
          <div className="space-y-4">
            <p className="text-sm text-gray-500 text-center">Введите 6-значный код, отправленный на {form.email}</p>
            <input
              type="text"
              value={otpCode}
              onChange={(e) => setOtpCode(e.target.value.replace(/\D/g, '').slice(0, 6))}
              placeholder="Код из email"
              className={inputCls + ' text-center tracking-widest'}
              maxLength={6}
            />
            <div className="flex justify-between items-center text-sm">
              <button
                type="button"
                onClick={handleResend}
                disabled={resendTimer > 0}
                className="text-[#4338CA] hover:underline disabled:text-gray-400"
              >
                {resendTimer > 0 ? `Отправить код повторно (${resendTimer})` : 'Отправить код повторно'}
              </button>
              <span className="text-gray-500">{form.email}</span>
            </div>
            <button
              onClick={handleVerify}
              className="w-full py-3 bg-[#333340] text-white rounded-md text-sm font-medium hover:bg-[#444455] transition-colors cursor-pointer"
            >
              Подтвердить email
            </button>
            <p className="text-center text-[13px] text-gray-400">
              Уже подтвердили? <Link to="/login" className="text-black font-semibold no-underline hover:underline">Войти</Link>
            </p>
          </div>
        )}

        {/* Step 4: interests */}
        {step === 4 && (
          <div>
            <p className="text-sm text-gray-600 mb-4">Выберите интересующие вас категории:</p>
            <div className="flex flex-wrap justify-center gap-2 mb-6">
              {categories.length > 0 ? (
                categories.map(c => (
                  <button key={c.id} onClick={() => setSelectedPrefs(prev => prev.includes(c.name) ? prev.filter(x => x !== c.name) : [...prev, c.name])}
                    className={`px-3 py-1.5 rounded text-[13px] cursor-pointer transition-colors ${
                      selectedPrefs.includes(c.name)
                        ? 'bg-[#333340] text-white border-0'
                        : 'bg-transparent text-gray-600 border border-gray-200 hover:border-[#4338CA]'
                    }`}>
                    {selectedPrefs.includes(c.name) ? `${c.name} ✕` : `${c.name} +`}
                  </button>
                ))
              ) : (
                <p className="text-sm text-gray-400">Категории не загружены</p>
              )}
            </div>

            <button onClick={handleFinishInterests}
              className="w-full py-3 bg-[#333340] text-white rounded-md text-sm font-medium hover:bg-[#444455] transition-colors cursor-pointer">
              Завершить
            </button>
          </div>
        )}
      </div>
    </div>
  );
}
