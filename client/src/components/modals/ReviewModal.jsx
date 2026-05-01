import React, { useState } from 'react';
import { X, Star, Send } from 'lucide-react';
import { apiFacade } from '../../services/apiFacade';
import { useAuth } from '../../context/AuthContext';
import './ReviewModal.css';

const ReviewModal = ({ isOpen, onClose, bookId, bookTitle }) => {
  const { userId, token } = useAuth();
  
  const [rating, setRating] = useState(0);
  const [hoverRating, setHoverRating] = useState(0);
  const [comment, setComment] = useState('');
  
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState({ type: '', text: '' });

  if (!isOpen) return null;

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (rating === 0) {
      setMessage({ type: 'error', text: 'Please select a rating.' });
      return;
    }

    setLoading(true);
    setMessage({ type: '', text: '' });
    
    try {
      await apiFacade.submitRating(userId, bookId, rating, comment, token);
      
      setMessage({ type: 'success', text: 'Review submitted successfully!' });
      setTimeout(() => {
        onClose();
        // ideally reload book details here
        window.location.reload(); 
      }, 1500);
    } catch (error) {
      setMessage({ type: 'error', text: error.message || 'Failed to submit review.' });
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="modal-overlay">
      <div className="modal-content glass-panel">
        <button className="modal-close" onClick={onClose}>
          <X size={20} />
        </button>
        
        <h2>Rate & Review</h2>
        <p className="review-subtitle">Share your thoughts on <span className="text-gradient">{bookTitle}</span></p>
        
        {message.text && (
          <div className={`modal-msg ${message.type}`}>
            {message.text}
          </div>
        )}
        
        <form onSubmit={handleSubmit} className="review-form">
          <div className="star-rating">
            {[1, 2, 3, 4, 5, 6, 7, 8, 9, 10].map((star) => (
              <button
                type="button"
                key={star}
                className={`star-btn ${star <= (hoverRating || rating) ? 'active' : ''}`}
                onMouseEnter={() => setHoverRating(star)}
                onMouseLeave={() => setHoverRating(0)}
                onClick={() => setRating(star)}
              >
                <Star size={24} fill={star <= (hoverRating || rating) ? "#fbbf24" : "transparent"} stroke={star <= (hoverRating || rating) ? "#fbbf24" : "var(--text-secondary)"} />
              </button>
            ))}
            <div className="rating-text">
              {rating > 0 ? `${rating} / 10` : 'Select a rating'}
            </div>
          </div>
          
          <div className="form-group">
            <textarea 
              value={comment} 
              onChange={(e) => setComment(e.target.value)} 
              placeholder="Write your review here... (Optional)"
              rows="4"
            ></textarea>
          </div>
          
          <button type="submit" className="primary-btn submit-btn" disabled={loading}>
            {loading ? 'Submitting...' : <><Send size={18} /> Submit Review</>}
          </button>
        </form>
      </div>
    </div>
  );
};

export default ReviewModal;
