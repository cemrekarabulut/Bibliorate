import React, { useState, useEffect } from 'react';
import { User, Library, Settings, LogOut } from 'lucide-react';
import { Link } from 'react-router-dom';
import { apiFacade } from '../services/apiFacade';
import { useAuth } from '../context/AuthContext';
import BookCard from '../components/discovery/BookCard';
import './ProfilePage.css';

/**
 * ProfilePage
 * -----------
 * Shows the authenticated user's profile and their favourites list.
 * Redirects unauthenticated visitors to the login prompt.
 */
const ProfilePage = () => {
  const { isLoggedIn, user, userId, token, logout } = useAuth();

  const [favourites, setFavourites] = useState([]);
  const [loading, setLoading]       = useState(true);

  useEffect(() => {
    if (!isLoggedIn) {
      setLoading(false);
      return;
    }

    const fetchFavourites = async () => {
      setLoading(true);
      try {
        const data = await apiFacade.getFavorites(userId, token);
        setFavourites(data);
      } catch (err) {
        console.error('Failed to load favourites:', err);
      } finally {
        setLoading(false);
      }
    };

    fetchFavourites();
  }, [isLoggedIn, userId, token]);

  // ── Not logged in ───────────────────────────────────────────────────────
  if (!isLoggedIn) {
    return (
      <div className="profile-page">
        <div className="profile-locked glass-panel">
          <h2>Please Log In</h2>
          <p>You need to be logged in to view your profile and lists.</p>
          <Link to="/login" className="primary-btn" style={{ marginTop: '1rem', display: 'inline-flex' }}>
            Go to Login
          </Link>
        </div>
      </div>
    );
  }

  // ── Render ──────────────────────────────────────────────────────────────
  return (
    <div className="profile-page">
      <header className="profile-header glass-panel">
        <div className="profile-avatar">
          <User size={48} className="text-gradient" />
        </div>
        <div className="profile-info">
          <h1>
            Welcome, <span className="text-gradient">{user?.username ?? 'Reader'}</span>
          </h1>
          <p>{user?.email}</p>
        </div>
        <div className="profile-actions">
          <button className="settings-btn">
            <Settings size={20} /> Settings
          </button>
          <button className="settings-btn" onClick={logout} style={{ marginLeft: '0.5rem' }}>
            <LogOut size={20} /> Logout
          </button>
        </div>
      </header>

      <section className="profile-content">
        <div className="section-header">
          <Library className="text-gradient" size={24} />
          <h2>My Favourites List</h2>
        </div>

        {loading ? (
          <div className="spinner-large" style={{ margin: '4rem auto' }} />
        ) : favourites.length > 0 ? (
          <div className="favorites-grid">
            {favourites.map(book => (
              <BookCard key={book.id} book={book} />
            ))}
          </div>
        ) : (
          <div className="empty-favorites">
            <p>You haven't added any books to your favourites yet.</p>
            <Link to="/" className="primary-btn" style={{ marginTop: '1rem', display: 'inline-flex' }}>
              Discover Books
            </Link>
          </div>
        )}
      </section>
    </div>
  );
};

export default ProfilePage;
