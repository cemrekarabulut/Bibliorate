import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { Sparkles, Star } from 'lucide-react';
import { apiFacade } from '../../services/apiFacade';
import { useAuth } from '../../context/AuthContext';
import './RecommendedBook.css';

const RecommendedBook = () => {
  const { isLoggedIn, userId, token } = useAuth();
  const [recommended, setRecommended] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!isLoggedIn) {
      setLoading(false);
      return;
    }

    const computeRecommendation = async () => {
      try {
        const [favorites, allBooks] = await Promise.all([
          apiFacade.getFavorites(userId, token),
          apiFacade.getBooks()
        ]);
        
        if (!allBooks || allBooks.length === 0) {
          setLoading(false);
          return;
        }
        
        if (favorites.length === 0) {
          // If no favorites, recommend the highest rated book overall
          const topBook = [...allBooks].sort((a, b) => b.rating - a.rating)[0];
          setRecommended(topBook);
          setLoading(false);
          return;
        }

        // Count genres in favorites
        const genreCounts = {};
        favorites.forEach(b => {
          const g = b.genre;
          genreCounts[g] = (genreCounts[g] || 0) + 1;
        });

        // Find the most frequent genre
        let topGenre = null;
        let maxCount = 0;
        for (const g in genreCounts) {
          if (genreCounts[g] > maxCount) {
            maxCount = genreCounts[g];
            topGenre = g;
          }
        }

        // Find the highest rated book in that genre that is NOT in favorites
        const favIds = new Set(favorites.map(f => f.id));
        const candidateBooks = allBooks.filter(b => b.genre === topGenre && !favIds.has(b.id));
        
        if (candidateBooks.length > 0) {
          // Sort by rating
          candidateBooks.sort((a, b) => b.rating - a.rating);
          setRecommended(candidateBooks[0]);
        } else {
          // Fallback: just recommend top rated book that isn't favorited
          const fallbackBooks = allBooks.filter(b => !favIds.has(b.id)).sort((a, b) => b.rating - a.rating);
          setRecommended(fallbackBooks[0] || allBooks[0]);
        }
      } catch (err) {
        console.error("Failed to compute recommendation", err);
      } finally {
        setLoading(false);
      }
    };

    computeRecommendation();
  }, [isLoggedIn, userId, token]);

  if (loading || !recommended || !isLoggedIn) {
    return null; // Don't show anything while loading or if not logged in
  }

  return (
    <div className="recommended-section">
      <div className="recommended-header">
        <Sparkles className="text-gradient" size={24} />
        <h2>Recommended For You</h2>
      </div>
      <p className="recommended-subtitle">Based on your favorites and review history</p>
      
      <div className="recommended-card glass-panel">
        <img 
          src={recommended.coverUrl} 
          alt={recommended.title} 
          className="recommended-cover"
          onError={(e) => {
            e.currentTarget.src = 'https://images.unsplash.com/photo-1544947950-fa07a98d237f?auto=format&fit=crop&q=80&w=600';
          }}
        />
        <div className="recommended-info">
          <span className="genre-badge">{recommended.genre}</span>
          <h3>{recommended.title}</h3>
          <p className="recommended-author">by {recommended.author}</p>
          <div className="recommended-metric">
            <Star size={16} fill="#fbbf24" stroke="#fbbf24" />
            <span>{recommended.rating.toFixed(1)}</span>
          </div>
          <p className="recommended-desc">{recommended.description}</p>
          <Link to={`/book/${recommended.id}`} className="primary-btn view-recommended-btn">
            View Book
          </Link>
        </div>
      </div>
    </div>
  );
};

export default RecommendedBook;
