import React, { useState, useEffect } from 'react';
import { User, Library, Settings } from 'lucide-react';
import { apiFacade } from '../services/apiFacade';
import BookCard from '../components/discovery/BookCard';
import './ProfilePage.css';

const ProfilePage = () => {
  const [favorites, setFavorites] = useState([]);
  const [loading, setLoading] = useState(true);

  // Mock checking if user is logged in
  const isLoggedIn = true;

  useEffect(() => {
    const fetchFavs = async () => {
      setLoading(true);
      const data = await apiFacade.getFavorites();
      setFavorites(data);
      setLoading(false);
    };
    if (isLoggedIn) fetchFavs();
  }, [isLoggedIn]);

  if (!isLoggedIn) {
    return (
      <div className="profile-page">
        <div className="profile-locked glass-panel">
          <h2>Please Log In</h2>
          <p>You need to be logged in to view your profile and lists.</p>
        </div>
      </div>
    );
  }

  return (
    <div className="profile-page">
      <header className="profile-header glass-panel">
        <div className="profile-avatar">
          <User size={48} className="text-gradient" />
        </div>
        <div className="profile-info">
          <h1>Welcome, <span className="text-gradient">Bookworm</span></h1>
          <p>Member since 2026</p>
        </div>
        <div className="profile-actions">
          <button className="settings-btn"><Settings size={20} /> Settings</button>
        </div>
      </header>

      <section className="profile-content">
        <div className="section-header">
          <Library className="text-gradient" size={24} />
          <h2>My Favorites List</h2>
        </div>
        
        {loading ? (
          <div className="spinner-large" style={{ margin: '4rem auto' }}></div>
        ) : favorites.length > 0 ? (
          <div className="favorites-grid">
            {favorites.map(book => (
              <BookCard key={book.id} book={book} />
            ))}
          </div>
        ) : (
          <div className="empty-favorites">
            <p>You haven't added any books to your favorites yet.</p>
          </div>
        )}
      </section>
    </div>
  );
};

export default ProfilePage;
