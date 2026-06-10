import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import Layout from './components/Layout';
import LandingPage from './pages/LandingPage';
import CatalogPage from './pages/CatalogPage';
import EventDetailsPage from './pages/EventDetailsPage';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import PasswordResetPage from './pages/PasswordResetPage';
import ProfilePage from './pages/ProfilePage';
import EditProfilePage from './pages/EditProfilePage';
import MyApplicationsPage from './pages/MyApplicationsPage';
import MyEventsPage from './pages/MyEventsPage';
import EventApplicationsPage from './pages/EventApplicationsPage';
import CreateEventPage from './pages/CreateEventPage';
import EditEventPage from './pages/EditEventPage';
import NotificationsPage from './pages/NotificationsPage';
import FavoritesPage from './pages/FavoritesPage';
import EventHistoryPage from './pages/EventHistoryPage';
import MyReviewsPage from './pages/MyReviewsPage';
import CreateReviewPage from './pages/CreateReviewPage';
import BecomeOrganizerPage from './pages/BecomeOrganizerPage';
import PromosPage from './pages/PromosPage';
import CreatePromoPage from './pages/CreatePromoPage';
import BonusHistoryPage from './pages/BonusHistoryPage';

function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          <Route path="/landing" element={<LandingPage />} />
          <Route element={<Layout />}>
            <Route path="/" element={<CatalogPage />} />
            <Route path="/events/:id" element={<EventDetailsPage />} />
            <Route path="/login" element={<LoginPage />} />
            <Route path="/register" element={<RegisterPage />} />
            <Route path="/password-reset" element={<PasswordResetPage />} />
            <Route path="/profile" element={<ProfilePage />} />
            <Route path="/edit-profile" element={<EditProfilePage />} />
            <Route path="/applications" element={<MyApplicationsPage />} />
            <Route path="/my-events" element={<MyEventsPage />} />
            <Route path="/events/:eventId/applications" element={<EventApplicationsPage />} />
            <Route path="/create-event" element={<CreateEventPage />} />
            <Route path="/edit-event/:id" element={<EditEventPage />} />
            <Route path="/notifications" element={<NotificationsPage />} />
            <Route path="/favorites" element={<FavoritesPage />} />
            <Route path="/history" element={<EventHistoryPage />} />
            <Route path="/reviews" element={<MyReviewsPage />} />
            <Route path="/events/:eventId/review" element={<CreateReviewPage />} />
            <Route path="/become-organizer" element={<BecomeOrganizerPage />} />
            <Route path="/promos" element={<PromosPage />} />
            <Route path="/create-promo" element={<CreatePromoPage />} />
            <Route path="/bonus-history" element={<BonusHistoryPage />} />
          </Route>
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  );
}

export default App
