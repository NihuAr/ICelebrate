import { useState, useEffect } from 'react';
import { Link, Outlet, useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { Star, Heart, Clock, Bell, PlusCircle, CalendarDays, ChevronRight, Search, User, MessageSquare, Gift, FileText, Coins, LogOut } from 'lucide-react';

export default function Layout() {
  const { user, loading, logout } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [collapsed, setCollapsed] = useState(false);

  const isActive = (path) => location.pathname === path;
  const isPublic = ['/login', '/register', '/landing', '/password-reset'].includes(location.pathname);

  // Redirect to landing if not authenticated and not on public page
  useEffect(() => {
    if (!loading && !user && !isPublic) {
      navigate('/landing');
    }
  }, [loading, user, isPublic, navigate]);

  // Don't show sidebar on public pages
  if (isPublic) {
    return <Outlet />;
  }

  const now = new Date();
  const dayName = now.toLocaleDateString('ru-RU', { weekday: 'long' });
  const dayNum = now.getDate();
  const monthName = now.toLocaleDateString('ru-RU', { month: 'long' });

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-[#F8F7FC]">
        <p className="text-gray-400 text-sm">Загрузка...</p>
      </div>
    );
  }

  // Public pages (login, register) — render without sidebar
  if (!user) {
    return <Outlet />;
  }

  // Authenticated — full layout with sidebar
  return (
    <div className="flex h-screen bg-[#F5F5F9]">
      {/* Sidebar */}
      <aside
        className={`${collapsed ? 'w-[100px]' : 'w-[220px]'} bg-[#2D2D3A] flex flex-col transition-all duration-200 flex-shrink-0`}
      >
        {/* Logo */}
        <div className={`flex items-center gap-3 px-4 pt-5 pb-5 ${collapsed ? 'justify-center' : ''}`}>
          <div
            className="w-9 h-9 bg-white rounded-full flex items-center justify-center flex-shrink-0 cursor-pointer"
            onClick={() => setCollapsed(!collapsed)}
          >
            <span className="text-[#2D2D3A] font-bold text-base">i</span>
          </div>
          {!collapsed && <span className="text-white text-base font-semibold">iCelebrate</span>}
        </div>

        {/* Navigation */}
        <nav className="flex-1 px-3 space-y-1 overflow-y-auto">
          <SidebarLink to="/" icon={<Star size={18} />} label="Афиша" active={isActive('/')} collapsed={collapsed} />
          <SidebarLink to="/favorites" icon={<Heart size={18} />} label="Избранное" active={isActive('/favorites')} collapsed={collapsed} />
          <SidebarLink to="/history" icon={<Clock size={18} />} label="История" active={isActive('/history')} collapsed={collapsed} />
          <SidebarLink to="/applications" icon={<FileText size={18} />} label="Мои заявки" active={isActive('/applications')} collapsed={collapsed} />
          <SidebarLink to="/reviews" icon={<MessageSquare size={18} />} label="Отзывы" active={isActive('/reviews')} collapsed={collapsed} />

          {user.is_organizer ? (
            <>
              <div className="border-t border-white/10 my-3 mx-1" />
              <SidebarLink to="/create-event" icon={<PlusCircle size={18} />} label="Новое мероприятие" active={isActive('/create-event')} collapsed={collapsed} />
              <SidebarLink to="/my-events" icon={<CalendarDays size={18} />} label="Мои мероприятия" active={isActive('/my-events')} collapsed={collapsed} />
              <SidebarLink to="/create-promo" icon={<Gift size={18} />} label="Создать промокод" active={isActive('/create-promo')} collapsed={collapsed} />
            </>
          ) : (
            <>
              <div className="border-t border-white/10 my-3 mx-1" />
              <SidebarLink to="/become-organizer" icon={<User size={18} />} label="Стать организатором" active={isActive('/become-organizer')} collapsed={collapsed} />
            </>
          )}

          <div className="border-t border-white/10 my-3 mx-1" />
          <SidebarLink to="/notifications" icon={<Bell size={18} />} label="Уведомления" active={isActive('/notifications')} collapsed={collapsed} />
          <SidebarLink to="/promos" icon={<Gift size={18} />} label="Промокоды" active={isActive('/promos')} collapsed={collapsed} />
          <SidebarLink to="/bonus-history" icon={<Coins size={18} />} label="Бонусы" active={isActive('/bonus-history')} collapsed={collapsed} />
        </nav>

        {/* Profile card at bottom */}
        <div className={`px-3 pb-3 mt-auto ${collapsed ? 'flex justify-center' : ''}`}>
          <Link to="/profile" className="no-underline">
            <div className={`bg-[#3D3D4E] rounded-lg p-3 flex items-center gap-3 hover:bg-[#4A4A5E] transition-colors ${collapsed ? 'justify-center' : ''}`}>
              <div className="w-10 h-10 bg-[#6366F1] rounded-full flex items-center justify-center text-white font-bold text-sm flex-shrink-0">
                {user.first_name?.[0]?.toUpperCase() || '?'}
              </div>
              {!collapsed && (
                <div className="flex-1 min-w-0">
                  <p className="text-[13px] font-medium text-white truncate">{user.first_name} {user.last_name}</p>
                  <p className="text-[11px] text-white/50 truncate">{user.email}</p>
                </div>
              )}
            </div>
          </Link>
          {/* Logout button */}
          <button
            onClick={() => { logout(); navigate('/login'); }}
            className={`mt-2 w-full flex items-center gap-3 px-3 py-2 rounded-md text-white/50 hover:text-white hover:bg-[#3D3D4E] transition-colors cursor-pointer bg-transparent border-0 text-[13px] ${collapsed ? 'justify-center' : ''}`}
          >
            <LogOut size={16} />
            {!collapsed && <span>Выйти</span>}
          </button>
        </div>
      </aside>

      {/* Main content area */}
      <main className="flex-1 flex flex-col overflow-hidden">
        {/* Page content with scroll */}
        <div className="flex-1 overflow-auto p-8">
          <Outlet />
        </div>
      </main>
    </div>
  );
}

function SidebarLink({ to, icon, label, active, collapsed }) {
  return (
    <Link
      to={to}
      className={`flex items-center gap-3 px-3 py-2.5 rounded-lg text-[13px] no-underline transition-colors ${
        collapsed ? 'justify-center' : ''
      } ${
        active
          ? 'bg-[#4A4A5E] text-white font-medium'
          : 'text-white/60 hover:bg-[#3D3D4E] hover:text-white'
      }`}
      title={collapsed ? label : undefined}
    >
      <span className="flex-shrink-0">{icon}</span>
      {!collapsed && <span>{label}</span>}
    </Link>
  );
}
