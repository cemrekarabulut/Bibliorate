/**
 * apiFacade.js
 * ------------
 * Facade Pattern implementation that provides a single, unified interface
 * for the React frontend to communicate with:
 *   - The C# .NET 8 REST API  (VITE_API_URL, default: http://localhost:5001)
 *   - The Python Flask analytics microservice (VITE_FLASK_URL, default: http://localhost:5000)
 *
 * All components import { apiFacade } — they never touch fetch/headers directly.
 * Base URLs are read from environment variables (.env / Docker --build-arg) so
 * no code changes are needed between local, staging, and production environments.
 */

const API_BASE = import.meta.env.VITE_API_URL || 'http://localhost:5001';
const FLASK_BASE = import.meta.env.VITE_FLASK_URL || 'http://localhost:5000';
const API_BASE_URL = `${API_BASE}/api`;

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
      const body = await response.text();
      if (body) message = body;
    } catch { /* ignore parse errors */ }
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
const genreTranslation = {
  'kurgu': 'Fiction',
  'edebiyat': 'Literature',
  'roman': 'Novel',
  'genel': 'General',
  'bilimkurgu': 'Sci-Fi',
  'tarih': 'History',
  'biyografi': 'Biography',
  'şiir': 'Poetry',
  'tiyatro': 'Theater',
  'dünya klasikleri': 'Classics',
  'macera': 'Adventure'
};

const translateGenre = (g) => {
  if (!g) return g;
  const lower = g.trim().toLowerCase();
  return genreTranslation[lower] || g;
};

const normaliseBook = (dto) => ({
  id:          dto.id,
  title:       dto.title ?? 'Unknown Title',
  author:      Array.isArray(dto.authors) && dto.authors.length > 0
                 ? dto.authors.join(', ')
                 : 'Unknown Author',
  genres:      Array.isArray(dto.categories) && dto.categories.length > 0
                 ? dto.categories.map(translateGenre)
                 : ['General'],
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
      books = books.filter(b => b.genres && b.genres.includes(genre));
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
   * Submits a star rating (1–10) for a book.
   * Requires authentication (token in header).
   */
  async rateBook(bookId, userId, score, token) {
    return request('/ratings', {
      method:  'POST',
      headers: buildHeaders(token),
      body:    JSON.stringify({ bookId, userId, score }),
    });
  }

  /**
   * Derives unique genre labels from the full book list.
   * Prefixes with "All" to match the filter UI expectation.
   */
  async getGenres() {
    const data  = await request('/books');
    const books = data.map(normaliseBook);
    const unique = [...new Set(books.flatMap(b => b.genres || []).filter(Boolean))];
    unique.sort();
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
   * Updates user profile data (username, email, or password).
   * Requires authentication (token passed in header).
   */
  async updateProfile(userId, { username, email, currentPassword, newPassword }, token) {
    return request('/auth/profile', {
      method:  'PUT',
      headers: buildHeaders(token),
      body:    JSON.stringify({ userId, username, email, currentPassword, newPassword }),
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
