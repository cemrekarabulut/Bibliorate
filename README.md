📚 Bibliorate: Smart Book Discovery Platform

Bibliorate is a book discovery and rating platform. It bridges the gap between readers and data-driven insights through a robust .NET API and AI-powered Flask analytics.

👥 The Development Team & RolesMemberRoleCore ResponsibilitiesCemre KarabulutBackend & API LeadDeveloped the core C# .NET API and implemented the Onion Architecture .Berra Senem MıynatData ScientistBuilt the Flask microservice for data analysis and book trend processing .Çağlar MesciFrontend (UI/UX)Designed the visual interface, layout, and overall user experience components.İsmail Buğra AkgünFrontend (Integration)Managed API consumption, state management, and frontend-to-backend data flow.

📂 Repository Structure
The project is organized into a modular multi-stack architecture to ensure a clean separation of concerns:

🔹 Backend (C# .NET 8)
Located in the src/ directory, following Onion Architecture:

src/BiblioRate.Domain: Core layer housing entities (Books, Users, Reviews) and repository interfaces.

src/BiblioRate.Application: Business logic, DTOs, and service definitions.

src/BiblioRate.Infrastructure: Implementation of database persistence and SQL Server access.

src/BiblioRate.API: Presentation layer with RESTful endpoints developed by Cemre.

🔹 Data Analysis (Python Flask)
analysis/: Dedicated microservice managed by Berra for advanced book data analytics and trend processing.

🔹 Frontend (Client Application)
client/: The frontend codebase managed by Çağlar and İsmail, focusing on user interface and API integration.

🛠 Tech Stack
Backend: .NET 8, C#, Entity Framework Core.

Data Analysis: Python, Flask.

Frontend: React.

Database: MySQL.

Management: Jira (Scrum Board) & GitHub.

🚀 Key Features
Our platform offers a data-driven book exploration experience powered by AI and real-time analytics:

📚 Smart Book Discovery: Seamlessly browse a comprehensive library with detailed book metadata including genre, author, and descriptions.

🧠 Behavioral Recommendations: A smart recommendation engine that suggests books based on individual user interaction trends and favorite genres.

📊 Real-time Analytics Dashboard:

Trend Tracking: Visualizing book views over time to identify trending titles.

Popularity Metrics: Tracking genre popularity and search trends to understand community interests.

Top-Rated Analysis: Ranking books based on community scores and voting frequency.

👤 User Engagement:

Secure Authentication: Professional user registration and login system using password hashing for security.

Personalized Interaction: Users can rate books, log views, and manage their favorite collections.

🔍 Search Insight: Advanced logging of search queries to provide insights into user search behavior and trends.

🧩 Selected Design Pattern: Facade
To manage the complexity of interacting with multiple subsystems (C# API + Flask Analysis), we utilize the Facade Pattern . This provides a simplified, unified interface for the frontend to access both the core book database and the advanced analytical insights seamlessly .

<<<<<<< HEAD
```text
BiblioRate/
├── BiblioRate.API/            # Presentation Layer: API endpoints and controllers
├── BiblioRate.Application/    # Application Layer: Business logic and services
├── BiblioRate.Domain/         # Core Layer: Entities, interfaces and domain logic
└── BiblioRate.Infrastructure/ # Infrastructure Layer: Database and external tools
berra updated this file!!
=======
>>>>>>> ccc4987 (add professional README only)
