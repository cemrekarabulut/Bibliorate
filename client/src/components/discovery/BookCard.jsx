import React from 'react';
import { Link } from 'react-router-dom';
import { Star, MessageCircle } from 'lucide-react';
import './BookCard.css';

const BookCard = ({ book }) => {
  return (
    <div className="book-card glass-panel">
      <div className="book-cover-wrapper">
        <img src={book.coverUrl} alt={book.title} className="book-cover" />
        <div className="book-overlay">
          <Link to={`/book/${book.id}`} className="view-btn">View Details</Link>
        </div>
        <div className="book-genre-badge">{book.genre}</div>
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
            <span className="metric-val">{(book.reviews / 1000).toFixed(1)}k</span>
          </div>
        </div>
      </div>
    </div>
  );
};

export default BookCard;
