import React from 'react';
import { Link } from 'react-router-dom';
import { Star, MessageCircle } from 'lucide-react';
import './BookCard.css';

/**
 * BookCard
 * --------
 * Displays a preview card for a single book.
 * Expects a normalised book object from apiFacade:
 *   { id, title, author, genre, description, coverUrl, rating, reviews }
 */
const BookCard = ({ book }) => {
  const formattedReviews = book.reviews >= 1000
    ? `${(book.reviews / 1000).toFixed(1)}k`
    : String(book.reviews);

  return (
    <div className="book-card glass-panel">
      <div className="book-cover-wrapper">
        <img
          src={book.coverUrl}
          alt={book.title}
          className="book-cover"
          onError={(e) => {
            e.currentTarget.src = 'https://images.unsplash.com/photo-1544947950-fa07a98d237f?auto=format&fit=crop&q=80&w=600';
          }}
        />
        <div className="book-overlay">
          <Link to={`/book/${book.id}`} className="view-btn">View Details</Link>
        </div>
        <div className="book-genres">
          {book.genres && book.genres.slice(0, 2).map((g, idx) => (
            <div key={idx} className="book-genre-badge">{g}</div>
          ))}
        </div>
      </div>

      <div className="book-info">
        <h3 className="book-title">{book.title}</h3>
        <p className="book-author">{book.author}</p>
        <p className="book-desc">{book.description}</p>

        <div className="book-metrics">
          <div className="metric">
            <Star className="metric-icon star" size={16} fill="#fbbf24" stroke="#fbbf24" />
            <span className="metric-val">{book.rating.toFixed(1)}</span>
          </div>
          <div className="metric">
            <MessageCircle className="metric-icon" size={16} />
            <span className="metric-val">{formattedReviews}</span>
          </div>
        </div>
      </div>
    </div>
  );
};

export default BookCard;
