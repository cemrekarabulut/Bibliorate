import React, { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import { Star, MessageCircle, ArrowLeft, BookmarkPlus } from 'lucide-react';
import { apiFacade } from '../services/apiFacade';
import './BookDetailPage.css';

const BookDetailPage = () => {
  const { id } = useParams();
  const [book, setBook] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchBook = async () => {
      setLoading(true);
      try {
        const data = await apiFacade.getBookById(Number(id));
        setBook(data);
      } catch (error) {
        console.error("Failed to fetch book:", error);
      } finally {
        setLoading(false);
      }
    };
    fetchBook();
  }, [id]);

  if (loading) {
    return (
      <div className="detail-loading-state">
        <div className="spinner-large"></div>
      </div>
    );
  }

  if (!book) {
    return (
      <div className="detail-empty-state">
        <h2>Book not found</h2>
        <Link to="/" className="back-link"><ArrowLeft size={16} /> Back to Discovery</Link>
      </div>
    );
  }

  return (
    <div className="book-detail-page">
      <Link to="/" className="back-link">
        <ArrowLeft size={16} /> Back to Discovery
      </Link>
      
      <div className="detail-hero glass-panel">
        <div className="detail-cover-container">
          <img src={book.coverUrl} alt={book.title} className="detail-cover" />
        </div>
        
        <div className="detail-info">
          <div className="detail-badges">
            <span className="genre-badge">{book.genre}</span>
          </div>
          <h1 className="detail-title">{book.title}</h1>
          <p className="detail-author">by <span className="text-gradient">{book.author}</span></p>
          
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
                <span className="metric-val">{(book.reviews / 1000).toFixed(1)}k</span>
                <span className="metric-label">Reviews</span>
              </div>
            </div>
          </div>
          
          <h3 className="section-title">Synopsis</h3>
          <p className="detail-desc">{book.description}</p>
          
          <div className="detail-actions">
            <button className="primary-btn">
              <Star size={18} /> Rate this Book
            </button>
            <button className="secondary-btn">
              <BookmarkPlus size={18} /> Add to List
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default BookDetailPage;
