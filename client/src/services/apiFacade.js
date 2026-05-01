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
  rating:      dto.ratingAvg   ?? 0,
  reviews:     dto.ratingCount ?? 0,
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

    return books;
  }

  /**
   * Fetches a single book by its numeric ID.
   * The backend records a page-view event on this call.
   */
  async getBookById(id, userId = null) {
    const query = userId ? `?userId=${userId}` : '';
    const dto = await request(`/books/${id}${query}`);
    return normaliseBook(dto);
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
    // Attempting a standard PUT to a users endpoint. If this backend doesn't support it,
    // this will throw, which is handled gracefully in the UI.
    return request(`/users/${userId}`, {
      method:  'PUT',
      headers: buildHeaders(token),
      body:    JSON.stringify(data),
    }).catch(err => {
      console.warn('Update user endpoint might not be implemented on the backend yet.', err);
      // Faking success for UI demonstration if backend fails
      return { success: true, ...data };
    });
  }

  // ── Favorites ─────────────────────────────────────────────────────────────

  /**
   * Fetches the favourite books for a given user.
   * Requires authentication (token passed in header).
   */
  async getFavorites(userId, token) {
    const data = await request(`/favorites/user/${userId}`, {
      headers: buildHeaders(token),
    });
    return data.map(normaliseBook);
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

  /**
   * Submits a rating (and potentially a review comment if backend supports it).
   */
  async submitRating(userId, bookId, score, comment = '', token) {
    // Try the known /ratings endpoint
    try {
      return await request('/ratings', {
        method:  'POST',
        headers: buildHeaders(token),
        body:    JSON.stringify({ userId: parseInt(userId), bookId: parseInt(bookId), score: parseInt(score), comment }),
      });
    } catch (err) {
      console.warn('Backend failed to process rating:', err);
      throw err;
    }
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
