import { createContext, useContext, useState, useEffect } from 'react';
import api from '../api';

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);

  const fetchProfile = async () => {
    try {
      const { data } = await api.get('/auth/profile/');
      setUser(data);
    } catch {
      setUser(null);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (localStorage.getItem('access_token')) {
      fetchProfile();
    } else {
      setLoading(false);
    }
  }, []);

  const login = async (username, password) => {
    const { data } = await api.post('/auth/token/', { username, password });
    localStorage.setItem('access_token', data.access);
    localStorage.setItem('refresh_token', data.refresh);
    await fetchProfile();
  };

  const verifyEmailRequest = async (email) => {
    const { data } = await api.post('/auth/verify-email/request/', { email });
    return data; // { message, otp_id }
  };

  const verifyEmailConfirm = async (otpId, code) => {
    const { data } = await api.post('/auth/verify-email/confirm/', { otp_id: otpId, code });
    await fetchProfile();
    return data;
  };

  const loginStep2 = async (otpId, code, rememberDevice) => {
    const { data } = await api.post('/auth/login/otp/', {
      otp_id: otpId,
      code,
      remember_device: rememberDevice,
    });
    localStorage.setItem('access_token', data.access);
    localStorage.setItem('refresh_token', data.refresh);
    if (data.device_token) {
      localStorage.setItem('device_token', data.device_token);
    }
    await fetchProfile();
  };

  const register = async (formData) => {
    await api.post('/auth/register/', formData);
    // Не логиним сразу, требуется подтверждение email
  };

  const logout = () => {
    localStorage.removeItem('access_token');
    localStorage.removeItem('refresh_token');
    setUser(null);
  };

  const refreshUser = () => fetchProfile();

  return (
    <AuthContext.Provider value={{ user, loading, login, loginStep2, register, verifyEmailRequest, verifyEmailConfirm, logout, refreshUser }}>
      {children}
    </AuthContext.Provider>
  );
}

export const useAuth = () => useContext(AuthContext);
