/**
 * apiFacade.js
 * ------------
 * Facade Pattern implementation that provides a single, unified interface
 * for the React frontend to communicate with:
 *   - The C# .NET 8 REST API  (http://localhost:5105)
 *   - Analytics endpoints forwarded through the same API from Flask
 *
 * All components import { apiFacade } — they never touch fetch/headers directly.
 * Swapping the base URL is the only change needed for staging / production.
 */

const API_BASE_URL = (import.meta.env.VITE_API_URL ?? 'http://localhost:5105') + '/api';

// ─── Private Helpers ─────────────────────────────────────────────────────────

/**
 * Constructs request headers.
 * Attaches the Authorization header when a JWT token is available.
 */
const buildHeaders = (token = null) => {
  const headers = { 'Content-Type': 'application/json' };
  if (token) headers['Authorization'] = `Bearer ${token}`;
  return headers;
};

/**
 * Generic fetch wrapper.
 * Throws a descriptive Error on non-2xx responses.
 */
const request = async (endpoint, options = {}) => {
  const response = await fetch(`${API_BASE_URL}${endpoint}`, options);

  if (!response.ok) {
    let message = `API error: ${response.status} ${response.statusText}`;
    try {
      const textBody = await response.text();
      if (textBody) {
        try {
          const jsonBody = JSON.parse(textBody);
          message = jsonBody.message || jsonBody.title || textBody;
        } catch {
          message = textBody;
        }
      }
    } catch { /* ignore parse errors */ }
    
    // Clean up backend prefix if it sends 'message: ...'
    if (typeof message === 'string' && message.toLowerCase().startsWith('message: ')) {
      message = message.substring(9).trim();
    }
    
    throw new Error(message);
  }

  // 204 No Content — return null instead of trying to parse JSON
  if (response.status === 204) return null;
  return response.json();
};

// ─── Data Normalisation ───────────────────────────────────────────────────────

/**
 * Maps the backend BookDto shape to the flat shape used by UI components.
 *
 * Backend:  { id, title, authors[], description, thumbnailUrl, ratingAvg, ratingCount, categories[] }
 * Frontend: { id, title, author, genre, description, coverUrl, rating, reviews }
 */
const normaliseBook = (dto) => ({
  id:          dto.id,
  title:       dto.title ?? 'Unknown Title',
  author:      Array.isArray(dto.authors) && dto.authors.length > 0
                 ? dto.authors.join(', ')
                 : 'Unknown Author',
  genre:       Array.isArray(dto.categories) && dto.categories.length > 0
                 ? dto.categories[0]
                 : 'General',
  description: dto.description ?? '',
  coverUrl:    dto.thumbnailUrl ?? 'https://images.unsplash.com/photo-1544947950-fa07a98d237f?auto=format&fit=crop&q=80&w=600',
  rating:      dto.averageRating ?? dto.ratingAvg ?? 0,
  reviews:     dto.reviewCount ?? dto.ratingCount ?? 0,
  ratingCount: dto.ratingCount ?? 0,
  reviewList:  dto.reviews ?? [],
});

// ─── ApiFacade Class ──────────────────────────────────────────────────────────

class ApiFacade {

  // ── Books ────────────────────────────────────────────────────────────────

  /**
   * Fetches all books. When a search query is supplied the backend's /search
   * endpoint is used (hits both local DB + Google Books). Otherwise, all local
   * books are returned and sorted/filtered on the client.
   */
  async getBooks({ search = '', genre = 'All', sortBy = 'rating' } = {}) {
    let books;

    if (search.trim()) {
      // Use search endpoint which queries local DB + Google Books
      const data = await request(`/books/search?q=${encodeURIComponent(search)}`);
      // Merge local + global results, deduplicate by id
      const all = [...(data.localResults ?? []), ...(data.globalResults ?? [])];
      books = Array.from(new Map(all.map(b => [b.id, b])).values()).map(normaliseBook);
    } else {
      const data = await request('/books');
      books = data.map(normaliseBook);
    }

    if (genre !== 'All') {
      books = books.filter(b => b.genre === genre);
    }

    const sorters = {
      rating:  (a, b) => b.rating  - a.rating,
      reviews: (a, b) => b.reviews - a.reviews,
      title:   (a, b) => a.title.localeCompare(b.title),
    };
    if (sorters[sortBy]) books.sort(sorters[sortBy]);

    return this._augmentBooksWithLocalRatings(books);
  }

  /**
   * Fetches a single book by its numeric ID.
   * The backend records a page-view event on this call.
   */
  async getBookById(id, userId = null) {
    const query = userId ? `?userId=${userId}` : '';
    const dto = await request(`/books/${id}${query}`);
    const book = normaliseBook(dto);
    return this._augmentBooksWithLocalRatings([book])[0];
  }

  /**
   * Derives unique genre labels from the full book list.
   * Prefixes with "All" to match the filter UI expectation.
   */
  async getGenres() {
    const data  = await request('/books');
    const books = data.map(normaliseBook);
    const unique = [...new Set(books.map(b => b.genre).filter(Boolean))];
    return ['All', ...unique];
  }

  // ── Auth ──────────────────────────────────────────────────────────────────

  /**
   * Authenticates a user with username + password.
   * Returns { token, userId, username, email } on success.
   */
  async login(username, password) {
    return request('/auth/login', {
      method:  'POST',
      headers: buildHeaders(),
      body:    JSON.stringify({ username, password }),
    });
  }

  /**
   * Registers a new user account.
   * Returns { userId, username, email } on success.
   */
  async register({ username, email, password }) {
    return request('/auth/register', {
      method:  'POST',
      headers: buildHeaders(),
      body:    JSON.stringify({ username, email, password }),
    });
  }

  /**
   * Updates user profile details (username, email, password).
   */
  async updateUser(userId, data, token) {
    return request('/auth/profile', {
      method:  'PUT',
      headers: buildHeaders(token),
      body:    JSON.stringify(data),
    }).catch(err => {
      console.warn('Failed to update profile:', err);
      throw err;
    });
  }

  // ── Favorites ─────────────────────────────────────────────────────────────

  /**
   * Fetches the favourite books for a given user.
   * Requires authentication (token passed in header).
   */
  async getFavorites(userId, token) {
    let books = [];
    try {
      const data = await request(`/favorites/user/${userId}`, {
        headers: buildHeaders(token),
      });
      if (Array.isArray(data)) {
        books = data.map(normaliseBook);
      }
    } catch (err) {
      console.warn('Could not fetch favorites, backend endpoint might be missing:', err);
    }
    return this._augmentBooksWithLocalRatings(books);
  }

  /**
   * Adds a book to the user's favourites list.
   */
  async addFavorite(userId, bookId, token) {
    return request('/favorites', {
      method:  'POST',
      headers: buildHeaders(token),
      body:    JSON.stringify({ userId, bookId }),
    });
  }

  /**
   * Removes a book from the user's favourites list.
   */
  async removeFavorite(userId, bookId, token) {
    return request(`/favorites/remove?userId=${userId}&bookId=${bookId}`, {
      method:  'DELETE',
      headers: buildHeaders(token),
    });
  }

  // ── Ratings / Reviews ─────────────────────────────────────────────────────
  //
  // The backend may not have GET endpoints for /ratings/book/:id or /ratings/user/:id.
  // To ensure reviews are always visible, we persist them in localStorage as well
  // and merge with any backend data.

  /** localStorage key for all saved reviews */
  _getLocalReviews() {
    try {
      return JSON.parse(localStorage.getItem('bibliorate_reviews') || '[]');
    } catch { return []; }
  }

  _saveLocalReviews(reviews) {
    localStorage.setItem('bibliorate_reviews', JSON.stringify(reviews));
  }

  /**
   * Merges local reviews into the book list so the UI updates immediately
   * even on the discovery/main page without refreshing from the backend.
   */
  _augmentBooksWithLocalRatings(books) {
    const localReviews = this._getLocalReviews();
    if (!localReviews.length) return books;

    return books.map(book => {
      const bookLocalReviews = localReviews.filter(r => r.bookId === book.id);
      if (!bookLocalReviews.length) return book;

      let count = book.ratingCount > 0 ? book.ratingCount : (book.rating > 0 ? book.reviews : 0);
      let totalScore = book.rating * count;
      let reviewTextCount = book.reviews;
      
      if (count === 0) {
        totalScore = 0;
        count = 0;
      }

      bookLocalReviews.forEach(r => {
        totalScore += r.score;
        count += 1;
        if (r.comment && r.comment.trim() !== '') {
          reviewTextCount += 1;
        }
      });

      return {
        ...book,
        rating: count > 0 ? totalScore / count : 0,
        ratingCount: count,
        reviews: reviewTextCount
      };
    });
  }

  async submitRating(userId, bookId, score, comment = '', token) {
    const parsedUserId = parseInt(userId);
    const parsedBookId = parseInt(bookId);
    const parsedScore  = parseInt(score);

    // 1. Submit rating to /ratings
    const ratingBody = JSON.stringify({
      userId: parsedUserId,
      bookId: parsedBookId,
      score: parsedScore,
      comment: comment.trim() !== '' ? comment.trim() : undefined
    });

    let backendResult = null;
    let backendSuccess = false;

    try {
      backendResult = await request('/ratings', {
        method:  'POST',
        headers: buildHeaders(token),
        body:    ratingBody,
      });
      backendSuccess = true;
    } catch (err) {
      const msg = (err.message || '').toLowerCase();
      // If 409 Conflict, it means we already rated, so try PUT.
      if (msg.includes('zaten') || msg.includes('already') || msg.includes('duplicate') || msg.includes('409')) {
        try {
          backendResult = await request('/ratings', {
            method:  'PUT',
            headers: buildHeaders(token),
            body:    ratingBody,
          });
          backendSuccess = true;
        } catch (putErr) {
          console.warn('PUT /ratings failed:', putErr);
        }
      } else {
        console.warn('POST /ratings failed:', err);
      }
    }

    // 2. If there's a comment, submit it to /reviews just in case the backend is the old version
    if (comment && comment.trim() !== '') {
      const reviewBody = JSON.stringify({
        userId: parsedUserId,
        bookId: parsedBookId,
        comment: comment.trim(),
      });
      try {
        await request('/reviews', {
          method:  'POST',
          headers: buildHeaders(token),
          body:    reviewBody,
        });
        // If this succeeds, then the comment was definitely saved by the backend
      } catch (revErr) {
        console.warn('POST /reviews failed:', revErr);
      }
    }

    const localReviews = this._getLocalReviews();
    const existingIdx = localReviews.findIndex(
      r => r.userId === parsedUserId && r.bookId === parsedBookId
    );

    if (backendSuccess) {
      // Backend successfully saved! We DO NOT NEED local storage.
      // Remove any existing local review for this book to prevent double counting.
      if (existingIdx >= 0) {
        localReviews.splice(existingIdx, 1);
        this._saveLocalReviews(localReviews);
      }
      return backendResult;
    }

    // --- FALLBACK LOGIC IF BACKEND FAILS ---
    let username = 'You';
    try {
      const session = JSON.parse(localStorage.getItem('bibliorate_auth') || '{}');
      username = session.username || 'You';
    } catch { /* ignore */ }

    const reviewEntry = {
      id: `local_${parsedUserId}_${parsedBookId}`,
      userId: parsedUserId,
      bookId: parsedBookId,
      score: parsedScore,
      comment,
      username,
      createdAt: new Date().toISOString(),
    };

    if (existingIdx >= 0) {
      localReviews[existingIdx] = reviewEntry;
    } else {
      localReviews.push(reviewEntry);
    }
    this._saveLocalReviews(localReviews);

    return reviewEntry;
  }

  /**
   * Fetches all ratings for a specific book.
   * Fetches from the new /reviews endpoint and merges with locally stored reviews.
   */
  async getBookRatings(bookId) {
    const parsedBookId = parseInt(bookId);
    let backendReviews = [];

    try {
      // Use the ratings endpoint which merges both scores and comments from the backend
      const data = await request(`/ratings/book/${parsedBookId}`);
      if (Array.isArray(data)) backendReviews = data;
    } catch {
      // Endpoint might not exist on live server yet — that's fine
    }

    // Merge with local reviews for this book
    const localReviews = this._getLocalReviews().filter(r => r.bookId === parsedBookId);

    // Deduplicate: prefer backend data, add local-only entries
    const mergedMap = new Map();
    backendReviews.forEach(r => mergedMap.set(`${r.userId}`, r));
    localReviews.forEach(r => {
      const key = `${r.userId}`;
      if (!mergedMap.has(key)) {
        mergedMap.set(key, r);
      } else {
        // If backend returned a review but it has no comment, and local DOES have a comment, use local!
        const existing = mergedMap.get(key);
        if (!existing.comment && r.comment) {
          mergedMap.set(key, r);
        }
      }
    });

    return Array.from(mergedMap.values());
  }

  /**
   * Fetches all ratings made by a specific user.
   * Merges backend data with locally stored reviews.
   */
  async getUserRatings(userId, token) {
    const parsedUserId = parseInt(userId);
    let backendRatings = [];

    try {
      const data = await request(`/ratings/user/${parsedUserId}`, {
        headers: buildHeaders(token),
      });
      if (Array.isArray(data)) backendRatings = data;
    } catch {
      // Endpoint might not exist — that's fine
    }

    // Merge with local reviews for this user
    const localRatings = this._getLocalReviews().filter(r => r.userId === parsedUserId);

    // Deduplicate by bookId
    const mergedMap = new Map();
    backendRatings.forEach(r => mergedMap.set(`${r.bookId}`, r));
    localRatings.forEach(r => {
      const key = `${r.bookId}`;
      if (!mergedMap.has(key)) mergedMap.set(key, r);
    });

    return Array.from(mergedMap.values());
  }

  // ── Analytics ─────────────────────────────────────────────────────────────

  /**
   * Fetches analytics data from the .NET API, which forwards the requests
   * to the Flask microservice. Returns a unified object used by AnalyticsDashboard.
   */
  async getAnalytics() {
    const [genrePopularity, mostViewed] = await Promise.allSettled([
      request('/analytics/genre-popularity'),
      request('/analytics/most-viewed'),
    ]);

    return {
      genrePopularity: genrePopularity.status === 'fulfilled'
        ? (genrePopularity.value ?? []).map(g => ({ name: g.genre ?? g.name, views: g.count ?? g.views ?? 0 }))
        : [],
      mostViewed: mostViewed.status === 'fulfilled'
        ? (mostViewed.value ?? [])
        : [],
    };
  }
}

export const apiFacade = new ApiFacade();
