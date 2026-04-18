import React, { useState, useEffect } from 'react';
import { Search, SlidersHorizontal, Loader2 } from 'lucide-react';
import { apiFacade } from '../services/apiFacade';
import BookCard from '../components/discovery/BookCard';
import './DiscoveryPage.css';

const DiscoveryPage = () => {
  const [books, setBooks] = useState([]);
  const [genres, setGenres] = useState([]);
  const [loading, setLoading] = useState(true);
  
  // Filters
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedGenre, setSelectedGenre] = useState('All');
  const [sortBy, setSortBy] = useState('rating');
  
  // Load Genres once
  useEffect(() => {
    const fetchGenres = async () => {
      const data = await apiFacade.getGenres();
      setGenres(data);
    };
    fetchGenres();
  }, []);

  // Load Books when filters change
  useEffect(() => {
    const fetchBooks = async () => {
      setLoading(true);
      try {
        const data = await apiFacade.getBooks({
          search: searchTerm,
          genre: selectedGenre,
          sortBy: sortBy
        });
        setBooks(data);
      } catch (error) {
        console.error("Failed to fetch books:", error);
      } finally {
        setLoading(false);
      }
    };

    // Debounce search
    const delayDebounceFn = setTimeout(() => {
      fetchBooks();
    }, 300);

    return () => clearTimeout(delayDebounceFn);
  }, [searchTerm, selectedGenre, sortBy]);

  return (
    <div className="discovery-page">
      <header className="discovery-header">
        <h1 className="page-title">Discover your next <span className="text-gradient">favorite book</span></h1>
        <p className="page-subtitle">Explore thousands of titles, curated by our intelligent recommendation engine.</p>
        
        <div className="search-container">
          <div className="search-bar glass-panel">
            <Search className="search-icon" size={20} />
            <input 
              type="text" 
              placeholder="Search by title, author, or keyword..." 
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="search-input"
            />
          </div>
        </div>
      </header>

      <div className="discovery-content">
        <aside className="filter-sidebar glass-panel">
          <div className="sidebar-header">
            <SlidersHorizontal size={18} className="text-gradient" />
            <h2>Filters</h2>
          </div>
          
          <div className="filter-group">
            <h3>Genres</h3>
            <div className="genre-list">
              {genres.map(genre => (
                <button 
                  key={genre}
                  className={`genre-btn ${selectedGenre === genre ? 'active' : ''}`}
                  onClick={() => setSelectedGenre(genre)}
                >
                  {genre}
                </button>
              ))}
            </div>
          </div>

          <div className="filter-group">
            <h3>Sort By</h3>
            <select 
              className="sort-select"
              value={sortBy}
              onChange={(e) => setSortBy(e.target.value)}
            >
              <option value="rating">Highest Rated</option>
              <option value="reviews">Most Reviewed</option>
              <option value="title">Alphabetical</option>
            </select>
          </div>
        </aside>

        <main className="books-showcase">
          <div className="showcase-header">
            <h2>{selectedGenre === 'All' ? 'Trending Now' : `${selectedGenre} Books`}</h2>
            <span className="results-count">{books.length} results</span>
          </div>

          {loading ? (
            <div className="loading-state">
              <Loader2 className="spinner" size={40} />
              <p>Curating your catalog...</p>
            </div>
          ) : books.length > 0 ? (
            <div className="book-grid">
              {books.map(book => (
                <BookCard key={book.id} book={book} />
              ))}
            </div>
          ) : (
            <div className="empty-state glass-panel">
              <Search size={48} className="empty-icon" />
              <h3>No books found</h3>
              <p>We couldn't find any books matching your criteria. Try adjusting your filters.</p>
              <button 
                className="clear-btn"
                onClick={() => {
                  setSearchTerm('');
                  setSelectedGenre('All');
                }}
              >
                Clear all filters
              </button>
            </div>
          )}
        </main>
      </div>
    </div>
  );
};

export default DiscoveryPage;
