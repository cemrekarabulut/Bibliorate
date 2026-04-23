import React, { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import { Star, MessageCircle, ArrowLeft, BookmarkPlus, BookmarkCheck } from 'lucide-react';
import { apiFacade } from '../services/apiFacade';
import { useAuth } from '../context/AuthContext';
import './BookDetailPage.css';

/**
 * BookDetailPage
 * --------------
 * Displays full details for a single book fetched from the .NET API.
 * Authenticated users can:
 *   - Add / remove the book from their favourites list
 *   - Rate the book with a 1–10 star picker
 */
const BookDetailPage = () => {
  const { id }                              = useParams();
  const { isLoggedIn, userId, token }       = useAuth();

  const [book, setBook]                     = useState(null);
  const [loading, setLoading]               = useState(true);
  const [isFavourite, setIsFavourite]       = useState(false);
  const [favouriteLoading, setFavouriteLoading] = useState(false);
  const [actionError, setActionError]       = useState('');
  const [actionSuccess, setActionSuccess]   = useState('');

  // Rating state
  const [hoverRating, setHoverRating]       = useState(0);
  const [selectedRating, setSelectedRating] = useState(0);
  const [hasRated, setHasRated]             = useState(false);
  const [ratingLoading, setRatingLoading]   = useState(false);

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

  // ── Helpers ─────────────────────────────────────────────────────────────

  const showError = (msg) => {
    setActionError(msg);
    setActionSuccess('');
    setTimeout(() => setActionError(''), 3500);
  };

  const showSuccess = (msg) => {
    setActionSuccess(msg);
    setActionError('');
    setTimeout(() => setActionSuccess(''), 3500);
  };

  // ── Favourite toggle ────────────────────────────────────────────────────
  const handleFavouriteToggle = async () => {
    if (!isLoggedIn) {
      showError('You must be logged in to manage your favourites.');
      return;
    }
    setFavouriteLoading(true);
    try {
      if (isFavourite) {
        await apiFacade.removeFavorite(userId, book.id, token);
        setIsFavourite(false);
        showSuccess('Removed from your list.');
      } else {
        await apiFacade.addFavorite(userId, book.id, token);
        setIsFavourite(true);
        showSuccess('Added to your list!');
      }
    } catch (err) {
      showError(err.message);
    } finally {
      setFavouriteLoading(false);
    }
  };

  // ── Rating ───────────────────────────────────────────────────────────────
  const handleRateSubmit = async () => {
    if (!isLoggedIn) {
      showError('You must be logged in to rate books.');
      return;
    }
    if (selectedRating === 0) {
      showError('Please select a star rating first.');
      return;
    }
    if (hasRated) {
      showError('You have already rated this book.');
      return;
    }
    setRatingLoading(true);
    try {
      const result = await apiFacade.rateBook(book.id, userId, selectedRating, token);
      setHasRated(true);
      setBook(prev => prev ? { 
        ...prev, 
        rating: result.currentAverage || prev.rating, 
        reviews: prev.reviews + 1 
      } : null);
      showSuccess(`You rated this book ${selectedRating}/10 ⭐`);
    } catch (err) {
      showError(err.message);
    } finally {
      setRatingLoading(false);
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

  const displayGenres = Array.isArray(book.genres) ? book.genres : [];

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

        <div className="detail-info" style={{ position: 'relative' }}>
          {/* Feedback banners (positioned absolute to prevent jumping) */}
          <div style={{ position: 'absolute', top: '-4rem', left: 0, width: '100%', zIndex: 10 }}>
            {actionError && (
              <div className="auth-error" style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', boxShadow: '0 4px 12px rgba(0,0,0,0.5)' }}>
                <span>{actionError}</span>
                {!isLoggedIn && (
                  <Link to="/login" className="primary-btn" style={{ padding: '0.4rem 1rem', fontSize: '0.8rem' }}>
                    Log In
                  </Link>
                )}
              </div>
            )}
            {actionSuccess && (
              <div className="auth-success" style={{ boxShadow: '0 4px 12px rgba(0,0,0,0.5)' }}>
                <span>{actionSuccess}</span>
              </div>
            )}
          </div>

          {/* Genre badges */}
          <div className="detail-badges">
            {displayGenres.length > 0
              ? displayGenres.map((g, i) => (
                  <span key={i} className="genre-badge">{g}</span>
                ))
              : <span className="genre-badge">General</span>
            }
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

          {/* ── Star Rating Picker ─────────────────────────────────────── */}
          <div className="rating-section">
            <h3 className="section-title">Rate This Book</h3>
            {hasRated ? (
              <p className="rating-done">You've rated this book {selectedRating}/10 ⭐</p>
            ) : (
              <>
                <div className="star-picker" aria-label="Rate this book">
                  {[1,2,3,4,5,6,7,8,9,10].map((star) => (
                    <button
                      key={star}
                      className={`star-btn ${star <= (hoverRating || selectedRating) ? 'filled' : ''}`}
                      onClick={() => {
                        if (!isLoggedIn) {
                          showError('You must be logged in to rate books.');
                          return;
                        }
                        setSelectedRating(star);
                      }}
                      onMouseEnter={() => setHoverRating(star)}
                      onMouseLeave={() => setHoverRating(0)}
                      aria-label={`Rate ${star} out of 10`}
                    >
                      <Star
                        size={22}
                        fill={star <= (hoverRating || selectedRating) ? '#fbbf24' : 'none'}
                        stroke={star <= (hoverRating || selectedRating) ? '#fbbf24' : 'currentColor'}
                      />
                    </button>
                  ))}
                </div>
                <p className="selected-rating-label" style={{ minHeight: '1.2rem', marginBottom: '0.75rem' }}>
                  {selectedRating > 0 ? `${selectedRating} / 10 selected` : '\u00A0'}
                </p>
                <button
                  className="primary-btn rate-submit-btn"
                  onClick={handleRateSubmit}
                  disabled={ratingLoading || selectedRating === 0}
                >
                  {ratingLoading ? 'Submitting…' : <><Star size={16} /> Submit Rating</>}
                </button>
              </>
            )}
          </div>

          {/* ── Actions ───────────────────────────────────────────────── */}
          <div className="detail-actions">
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
    </div>
  );
};

export default BookDetailPage;
