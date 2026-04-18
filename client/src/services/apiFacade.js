// Mock Data
export const MOCK_BOOKS = [
  {
    id: 1,
    title: "The Midnight Library",
    author: "Matt Haig",
    genre: "Fiction",
    rating: 4.8,
    reviews: 12450,
    coverUrl: "https://images.unsplash.com/photo-1544947950-fa07a98d237f?auto=format&fit=crop&q=80&w=600",
    description: "Between life and death there is a library, and within that library, the shelves go on forever."
  },
  {
    id: 2,
    title: "Dune",
    author: "Frank Herbert",
    genre: "Science Fiction",
    rating: 4.9,
    reviews: 84320,
    coverUrl: "https://images.unsplash.com/photo-1541963463532-d68292c34b19?auto=format&fit=crop&q=80&w=600",
    description: "Set on the desert planet Arrakis, Dune is the story of the boy Paul Atreides."
  },
  {
    id: 3,
    title: "Sapiens: A Brief History of Humankind",
    author: "Yuval Noah Harari",
    genre: "Non-Fiction",
    rating: 4.7,
    reviews: 53210,
    coverUrl: "https://images.unsplash.com/photo-1589829085413-56de8ae18c73?auto=format&fit=crop&q=80&w=600",
    description: "A hundred thousand years ago, at least six different species of humans inhabited Earth."
  },
  {
    id: 4,
    title: "Project Hail Mary",
    author: "Andy Weir",
    genre: "Science Fiction",
    rating: 4.9,
    reviews: 31200,
    coverUrl: "https://images.unsplash.com/photo-1451187580459-43490279c0fa?auto=format&fit=crop&q=80&w=600",
    description: "Ryland Grace is the sole survivor on a desperate, last-chance mission."
  },
  {
    id: 5,
    title: "The Alchemist",
    author: "Paulo Coelho",
    genre: "Fiction",
    rating: 4.5,
    reviews: 42100,
    coverUrl: "https://images.unsplash.com/photo-1518063004380-4d4eb07ac6fd?auto=format&fit=crop&q=80&w=600",
    description: "The mystical story of Santiago, an Andalusian shepherd boy."
  },
  {
    id: 6,
    title: "Atomic Habits",
    author: "James Clear",
    genre: "Self-Help",
    rating: 4.9,
    reviews: 98110,
    coverUrl: "https://images.unsplash.com/photo-1589998059171-988d887df646?auto=format&fit=crop&q=80&w=600",
    description: "No matter your goals, Atomic Habits offers a proven framework for improving."
  },
  {
    id: 7,
    title: "Neuromancer",
    author: "William Gibson",
    genre: "Science Fiction",
    rating: 4.3,
    reviews: 15400,
    coverUrl: "https://images.unsplash.com/photo-1550751827-4bd374c3f58b?auto=format&fit=crop&q=80&w=600",
    description: "Set in the future, the novel follows Henry Dorsett Case."
  },
  {
    id: 8,
    title: "Thinking, Fast and Slow",
    author: "Daniel Kahneman",
    genre: "Psychology",
    rating: 4.6,
    reviews: 67200,
    coverUrl: "https://images.unsplash.com/photo-1555448248-2571daf6344b?auto=format&fit=crop&q=80&w=600",
    description: "The universally engaging explanation of the two systems that drive the way we think."
  }
];

export const MOCK_GENRES = ["All", "Fiction", "Science Fiction", "Non-Fiction", "Self-Help", "Psychology"];

const MOCK_ANALYTICS = {
  overview: {
    totalBooks: 14502,
    activeUsers: 840,
    dailyReviews: 324
  },
  genrePopularity: [
    { name: 'Sci-Fi', views: 4000 },
    { name: 'Fiction', views: 3000 },
    { name: 'Self-Help', views: 2000 },
    { name: 'Psychology', views: 2780 },
    { name: 'Non-Fiction', views: 1890 },
  ],
  engagementData: [
    { month: 'Jan', activeUsers: 400 },
    { month: 'Feb', activeUsers: 500 },
    { month: 'Mar', activeUsers: 600 },
    { month: 'Apr', activeUsers: 840 },
  ]
};

/**
 * ApiFacade serves as a bridge between the React frontend, the .NET backend API,
 * and the Flask Analytics Microservice.
 * 
 * Currently using Mock Data. Can be swapped for axios/fetch when backend is ready.
 */
class ApiFacade {
  async getBooks({ search = "", genre = "All", sortBy = "rating" } = {}) {
    await new Promise(resolve => setTimeout(resolve, 600));
    let results = [...MOCK_BOOKS];
    if (search) {
      const lowerSearch = search.toLowerCase();
      results = results.filter(book => 
        book.title.toLowerCase().includes(lowerSearch) || 
        book.author.toLowerCase().includes(lowerSearch)
      );
    }
    if (genre !== "All") results = results.filter(book => book.genre === genre);
    if (sortBy === "rating") results.sort((a, b) => b.rating - a.rating);
    else if (sortBy === "reviews") results.sort((a, b) => b.reviews - a.reviews);
    else if (sortBy === "title") results.sort((a, b) => a.title.localeCompare(b.title));
    return results;
  }

  async getGenres() {
    await new Promise(resolve => setTimeout(resolve, 200));
    return MOCK_GENRES;
  }

  // --- NEW PHASE 2 MOCKS --- //

  async getBookById(id) {
    await new Promise(resolve => setTimeout(resolve, 400));
    const book = MOCK_BOOKS.find(b => b.id === id);
    if (!book) throw new Error("Book not found");
    return book;
  }

  async login(email, password) {
    await new Promise(resolve => setTimeout(resolve, 600));
    if (email && password) return { token: 'mock-jwt-token', user: { name: 'Bookworm', email } };
    throw new Error("Invalid credentials");
  }

  async register(data) {
    await new Promise(resolve => setTimeout(resolve, 800));
    return { success: true };
  }

  async getFavorites() {
    await new Promise(resolve => setTimeout(resolve, 400));
    return [MOCK_BOOKS[0], MOCK_BOOKS[3], MOCK_BOOKS[5]];
  }

  async getAnalytics() {
    await new Promise(resolve => setTimeout(resolve, 600));
    // Simulates call to Flask Microservice
    return MOCK_ANALYTICS;
  }
}

export const apiFacade = new ApiFacade();
