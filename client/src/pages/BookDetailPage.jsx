import React, { useState, useEffect, useCallback } from 'react';
import { useParams, Link } from 'react-router-dom';
import { Star, MessageCircle, ArrowLeft, BookmarkPlus, BookmarkCheck, User } from 'lucide-react';
import { apiFacade } from '../services/apiFacade';
import { useAuth } from '../context/AuthContext';
import ReviewModal from '../components/modals/ReviewModal';
import './BookDetailPage.css';

/**
 * BookDetailPage
 * --------------
 * Displays full details for a single book fetched from the .NET API.
 * Authenticated users can add / remove the book from their favourites list.
 * Shows existing reviews/ratings below the book details.
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
  const [reviews, setReviews]               = useState([]);

  // ── Fetch book details ──────────────────────────────────────────────────
  const fetchBookData = useCallback(async () => {
    setLoading(true);
    try {
      const bookData = await apiFacade.getBookById(Number(id), userId);
      setBook(bookData);

      // /api/books/{id} zaten reviews array'ini döndürüyor — önce onu kullan
      const embeddedReviews = bookData.reviewList ?? [];

      if (embeddedReviews.length > 0) {
        // Backend'in /api/books/{id} içindeki reviews formatını normalize et
        const normalised = embeddedReviews.map(r => ({
          id:        r.reviewId ?? r.id,
          userId:    r.userId,
          username:  r.username ?? r.userName ?? `User #${r.userId}`,
          score:     r.score,
          comment:   r.comment,
          createdAt: r.createdAt,
        }));
        setReviews(normalised);
      } else {
        // Fallback: ayrı ratings endpoint'i dene
        try {
          const reviewData = await apiFacade.getBookRatings(Number(id));
          setReviews(Array.isArray(reviewData) ? reviewData : []);
        } catch {
          setReviews([]);
        }
      }
    } catch (error) {
      console.error('Failed to fetch book:', error);
    } finally {
      setLoading(false);
    }
  }, [id, userId]);

  // ── Favori durumunu kontrol et ──────────────────────────────────────────
  const checkFavouriteStatus = useCallback(async () => {
    if (!isLoggedIn || !userId || !token) {
      setIsFavourite(false);
      return;
    }
    try {
      const favs = await apiFacade.getFavorites(userId, token);
      const bookId = Number(id);
      setIsFavourite(favs.some(f => f.id === bookId));
    } catch {
      setIsFavourite(false);
    }
  }, [id, isLoggedIn, userId, token]);

  useEffect(() => {
    fetchBookData();
  }, [fetchBookData]);

  useEffect(() => {
    checkFavouriteStatus();
  }, [checkFavouriteStatus]);



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

  // Called after a review is submitted — refresh book + reviews
  const handleReviewSubmitted = () => {
    fetchBookData();
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

  // Use the exact number of fetched/merged text reviews to ensure header matches the rendered cards.
  const liveReviewCount = reviews.length;
  // Rating is pre-calculated by the backend and returned in the book object
  const liveRating = book.rating;

  // Check if current user already reviewed this book
  const userExistingReview = isLoggedIn
    ? reviews.find(r => r.userId === userId || r.userId === String(userId))
    : null;

  const formattedReviews = liveReviewCount >= 1000
    ? `${(liveReviewCount / 1000).toFixed(1)}k`
    : String(liveReviewCount);

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
                <span className="metric-val">{liveRating.toFixed(1)}</span>
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
              <Star size={18} /> {userExistingReview ? 'Update Review' : 'Rate & Review'}
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

      {/* ── Reviews Section ──────────────────────────────────────────── */}
      <section className="reviews-section">
        <div className="reviews-header">
          <MessageCircle className="text-gradient" size={24} />
          <h2>Reviews ({liveReviewCount})</h2>
        </div>

        {reviews.length > 0 ? (
          <div className="reviews-list">
            {reviews.map((review, index) => (
              <div key={review.id || index} className="review-card glass-panel">
                <div className="review-card-header">
                  <div className="review-user">
                    <User size={18} />
                    <span className="review-username">{review.username || review.userName || `User #${review.userId}`}</span>
                  </div>
                  {review.score !== undefined && (
                    <div className="review-score">
                      <Star size={16} fill="#fbbf24" stroke="#fbbf24" />
                      <span>{review.score}/10</span>
                    </div>
                  )}
                </div>
                {review.comment && (
                  <p className="review-comment">{review.comment}</p>
                )}
                {review.createdAt && (
                  <span className="review-date">
                    {new Date(review.createdAt).toLocaleDateString('en-US', {
                      year: 'numeric', month: 'short', day: 'numeric'
                    })}
                  </span>
                )}
              </div>
            ))}
          </div>
        ) : (
          <div className="no-reviews glass-panel">
            <p>No reviews yet. Be the first to share your thoughts!</p>
          </div>
        )}
      </section>

      <ReviewModal 
        isOpen={isReviewOpen} 
        onClose={() => setIsReviewOpen(false)} 
        bookId={book.id} 
        bookTitle={book.title} 
        onReviewSubmitted={handleReviewSubmitted}
      />
    </div>
  );
};

export default BookDetailPage;
