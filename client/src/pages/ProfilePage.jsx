import React, { useState, useEffect } from 'react';
import { User, Library, Star, Settings, LogOut } from 'lucide-react';
import { Link } from 'react-router-dom';
import { apiFacade } from '../services/apiFacade';
import { useAuth } from '../context/AuthContext';
import BookCard from '../components/discovery/BookCard';
import SettingsModal from '../components/modals/SettingsModal';
import './ProfilePage.css';

/**
 * ProfilePage
 * -----------
 * Shows the authenticated user's profile, their favourites list,
 * and their review history.
 * Redirects unauthenticated visitors to the login prompt.
 */
const ProfilePage = () => {
  const { isLoggedIn, user, userId, token, logout } = useAuth();

  const [favourites, setFavourites] = useState([]);
  const [userReviews, setUserReviews] = useState([]);
  const [reviewedBooks, setReviewedBooks] = useState([]);
  const [loading, setLoading]       = useState(true);
  const [isSettingsOpen, setIsSettingsOpen] = useState(false);

  useEffect(() => {
    if (!isLoggedIn) {
      setLoading(false);
      return;
    }

    const fetchProfileData = async () => {
      setLoading(true);
      try {
        const [favData, ratingsData] = await Promise.all([
          apiFacade.getFavorites(userId, token),
          apiFacade.getUserRatings(userId, token),
        ]);
        setFavourites(favData);
        
        const ratings = Array.isArray(ratingsData) ? ratingsData : [];
        setUserReviews(ratings);

        // For each rating, try to fetch the book details
        if (ratings.length > 0) {
          const bookPromises = ratings.map(async (r) => {
            try {
              const book = await apiFacade.getBookById(r.bookId, userId);
              return {
                ...book,
                // kitabın gerçek ortalamasını koru, kullanıcı puanını ayrı sakla
                rating: book.rating,
                userScore: r.score,
                userComment: r.comment,
                reviewDate: r.createdAt,
              };
            } catch {
              return {
                id: r.bookId,
                title: `Book #${r.bookId}`,
                author: 'Unknown',
                genre: 'General',
                coverUrl: 'https://images.unsplash.com/photo-1544947950-fa07a98d237f?auto=format&fit=crop&q=80&w=600',
                rating: 0,
                reviews: 0,
                userScore: r.score,
                userComment: r.comment,
                reviewDate: r.createdAt,
              };
            }
          });
          const books = await Promise.all(bookPromises);
          setReviewedBooks(books);
        }
      } catch (err) {
        console.error('Failed to load profile data:', err);
      } finally {
        setLoading(false);
      }
    };

    fetchProfileData();
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
          <button className="settings-btn" onClick={() => setIsSettingsOpen(true)}>
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

      {/* ── My Reviews Section ─────────────────────────────────────── */}
      <section className="profile-content" style={{ marginTop: '2rem' }}>
        <div className="section-header">
          <Star className="text-gradient" size={24} />
          <h2>My Reviews ({reviewedBooks.length})</h2>
        </div>

        {loading ? (
          <div className="spinner-large" style={{ margin: '4rem auto' }} />
        ) : reviewedBooks.length > 0 ? (
          <div className="reviews-list-profile">
            {reviewedBooks.map((book, index) => (
              <Link to={`/book/${book.id}`} key={book.id || index} className="reviewed-book-card glass-panel">
                <img
                  src={book.coverUrl}
                  alt={book.title}
                  className="reviewed-book-cover"
                  onError={(e) => {
                    e.currentTarget.src = 'https://images.unsplash.com/photo-1544947950-fa07a98d237f?auto=format&fit=crop&q=80&w=600';
                  }}
                />
                <div className="reviewed-book-info">
                  <h3>{book.title}</h3>
                  <p className="reviewed-book-author">by {book.author}</p>
                  <div className="reviewed-book-score">
                    <Star size={16} fill="#fbbf24" stroke="#fbbf24" />
                    <span>Your rating: {book.userScore}/10</span>
                  </div>
                  {book.userComment && (
                    <p className="reviewed-book-comment">"{book.userComment}"</p>
                  )}
                  {book.reviewDate && (
                    <span className="reviewed-book-date">
                      {new Date(book.reviewDate).toLocaleDateString('en-US', {
                        year: 'numeric', month: 'short', day: 'numeric'
                      })}
                    </span>
                  )}
                </div>
              </Link>
            ))}
          </div>
        ) : (
          <div className="empty-favorites">
            <p>You haven't reviewed any books yet.</p>
            <Link to="/" className="primary-btn" style={{ marginTop: '1rem', display: 'inline-flex' }}>
              Discover & Review Books
            </Link>
          </div>
        )}
      </section>

      <SettingsModal 
        isOpen={isSettingsOpen} 
        onClose={() => setIsSettingsOpen(false)} 
      />
    </div>
  );
};

export default ProfilePage;
