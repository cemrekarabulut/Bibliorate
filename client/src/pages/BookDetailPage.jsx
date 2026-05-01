import React, { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import { Star, MessageCircle, ArrowLeft, BookmarkPlus, BookmarkCheck } from 'lucide-react';
import { apiFacade } from '../services/apiFacade';
import { useAuth } from '../context/AuthContext';
import ReviewModal from '../components/modals/ReviewModal';
import './BookDetailPage.css';

/**
 * BookDetailPage
 * --------------
 * Displays full details for a single book fetched from the .NET API.
 * Authenticated users can add / remove the book from their favourites list.
 */
const BookDetailPage = () => {
  const { id }                              = useParams();
  const { isLoggedIn, userId, token }       = useAuth();

  const [book, setBook]                     = useState(null);
  const [loading, setLoading]               = useState(true);
  const [isFavourite, setIsFavourite]       = useState(false);
  const [favouriteLoading, setFavouriteLoading] = useState(false);
  const [actionError, setActionError]       = useState('');
  const [isReviewOpen, setIsReviewOpen]     = useState(false);

  // ── Fetch book details ──────────────────────────────────────────────────
  useEffect(() => {
    const fetchBook = async () => {
      setLoading(true);
      try {
        const data = await apiFacade.getBookById(Number(id), userId);
        setBook(data);
      } catch (error) {
        console.error('Failed to fetch book:', error);
      } finally {
        setLoading(false);
      }
    };
    fetchBook();
  }, [id, userId]);

  // ── Favourite toggle ────────────────────────────────────────────────────

  const showAuthError = (msg) => {
    setActionError(msg);
    setTimeout(() => setActionError(''), 3500);
  };

  const handleFavouriteToggle = async () => {
    if (!isLoggedIn) {
      showAuthError('You must be logged in to manage your favourites.');
      return;
    }
    setFavouriteLoading(true);
    try {
      if (isFavourite) {
        await apiFacade.removeFavorite(userId, book.id, token);
        setIsFavourite(false);
      } else {
        await apiFacade.addFavorite(userId, book.id, token);
        setIsFavourite(true);
      }
    } catch (err) {
      showAuthError(err.message);
    } finally {
      setFavouriteLoading(false);
    }
  };

  const handleRateClick = () => {
    if (!isLoggedIn) {
      showAuthError('You must be logged in to rate and review books.');
    } else {
      setIsReviewOpen(true);
    }
  };

  // ── Render ──────────────────────────────────────────────────────────────

  if (loading) {
    return (
      <div className="detail-loading-state">
        <div className="spinner-large" />
      </div>
    );
  }

  if (!book) {
    return (
      <div className="detail-empty-state">
        <h2>Book not found</h2>
        <Link to="/" className="back-link">
          <ArrowLeft size={16} /> Back to Discovery
        </Link>
      </div>
    );
  }

  const formattedReviews = book.reviews >= 1000
    ? `${(book.reviews / 1000).toFixed(1)}k`
    : String(book.reviews);

  return (
    <div className="book-detail-page">
      <Link to="/" className="back-link">
        <ArrowLeft size={16} /> Back to Discovery
      </Link>

      <div className="detail-hero glass-panel">
        <div className="detail-cover-container">
          <img
            src={book.coverUrl}
            alt={book.title}
            className="detail-cover"
            onError={(e) => {
              e.currentTarget.src = 'https://images.unsplash.com/photo-1544947950-fa07a98d237f?auto=format&fit=crop&q=80&w=600';
            }}
          />
        </div>

        <div className="detail-info">
          {actionError && (
            <div className="auth-error" style={{ marginBottom: '1rem', display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
              <span>{actionError}</span>
              <Link to="/login" className="primary-btn" style={{ padding: '0.4rem 1rem', fontSize: '0.8rem' }}>
                Log In
              </Link>
            </div>
          )}

          <div className="detail-badges">
            <span className="genre-badge">{book.genre}</span>
          </div>

          <h1 className="detail-title">{book.title}</h1>
          <p className="detail-author">
            by <span className="text-gradient">{book.author}</span>
          </p>

          <div className="detail-metrics">
            <div className="metric-box">
              <Star className="metric-icon star" size={24} fill="#fbbf24" />
              <div className="metric-data">
                <span className="metric-val">{book.rating.toFixed(1)}</span>
                <span className="metric-label">Rating</span>
              </div>
            </div>
            <div className="metric-box">
              <MessageCircle className="metric-icon" size={24} />
              <div className="metric-data">
                <span className="metric-val">{formattedReviews}</span>
                <span className="metric-label">Reviews</span>
              </div>
            </div>
          </div>

          <h3 className="section-title">Synopsis</h3>
          <p className="detail-desc">{book.description || 'No description available.'}</p>

          <div className="detail-actions">
            <button className="primary-btn" onClick={handleRateClick}>
              <Star size={18} /> Rate & Review
            </button>
            <button
              className="secondary-btn"
              onClick={handleFavouriteToggle}
              disabled={favouriteLoading}
            >
              {isFavourite
                ? <><BookmarkCheck size={18} /> Remove from List</>
                : <><BookmarkPlus  size={18} /> Add to List</>
              }
            </button>
          </div>
        </div>
      </div>

      <ReviewModal 
        isOpen={isReviewOpen} 
        onClose={() => setIsReviewOpen(false)} 
        bookId={book.id} 
        bookTitle={book.title} 
      />
    </div>
  );
};

export default BookDetailPage;
