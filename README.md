🚀 BiblioRate Backend Engine
This repository contains the core ASP.NET Core 9 RESTful API for the BiblioRate platform. The system is architected using Onion Architecture principles to ensure high scalability, maintainability, and loose coupling between layers.

🏗️ Architectural Layers
The system is organized into four distinct layers where dependencies flow inward toward the Domain core:

BiblioRate.Domain: Contains core entities (Book, User, Rating, Review, Favorite), value objects, and database schema definitions.

BiblioRate.Application: Defines repository interfaces, service abstractions, and business logic contracts.

BiblioRate.Infrastructure: Handles data persistence via Entity Framework Core, MySQL implementations, and external integrations like the Google Books API.

BiblioRate.API: The entry point featuring REST controllers, middleware configurations (CORS, Global Exception Handling), and dependency injection setup.

📡 Core API Endpoints
Designed for seamless integration with the Frontend (React/Vite) and Data Science (Flask/Python) modules:

🔐 Authentication (/api/Auth)
POST /register: Handles new user registration. Returns user_id for frontend state management.

POST /login: Manages user authentication and secure access.

📚 Book & Search Module (/api/Books)
GET /search?q={query}: Implements a Hybrid Search algorithm that queries both the local MySQL database and Google Books API simultaneously.

POST /: Facilitates new book entries with GoogleBookId validation to prevent duplicate records.

📊 Social & Analytics Layer (/api/Ratings, /api/Reviews)
POST /Ratings: Supports 1-10 scale rating. Automatically appends a CreatedAt timestamp for anomaly detection analysis.

GET /Favorites/user/{id}: Returns user-specific favorites optimized via BookDto.

🛠️ Technical Specifications
Data Integrity: Assigned default values for Description and ThumbnailUrl to ensure null-safety for downstream Python-based analytics.

Cross-Origin Resource Sharing (CORS): Configured with an AllowAll policy to support multi-origin requests from frontend and analysis services.

Global Exception Handling: Centralized middleware to intercept and standardize system errors into consistent JSON responses.

Database Management: Powered by MySQL and managed through Entity Framework Core 9.0 migrations.

💻 Setup & Execution
Update Database:

dotnet ef database update --project BiblioRate.Infrastructure --startup-project BiblioRate.API
Run the API:

dotnet run --project BiblioRate.API
The API will be available at: http://localhost:5105