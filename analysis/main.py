from flask import Flask, jsonify, request
from flask_cors import CORS
from db import get_db
import bcrypt

app = Flask(__name__)
CORS(app)


# -------------------------------------------------
# Home
# -------------------------------------------------
@app.route("/")
def home():
    return "BiblioRate Backend Running 🚀"


# -------------------------------------------------
# GET ALL BOOKS
# DB kolonları (EF Migration'dan): BookId, Title, Author, Genre, Year
# -------------------------------------------------
@app.route("/api/books")
def get_books():
    db = get_db()
    cur = db.cursor()
    cur.execute("SELECT BookId, Title, Author, Genre, Year FROM Books")
    rows = cur.fetchall()
    cur.close()
    db.close()

    return jsonify([
        {"id": r[0], "title": r[1], "author": r[2], "genre": r[3], "year": r[4]}
        for r in rows
    ])


# -------------------------------------------------
# GET SINGLE BOOK
# -------------------------------------------------
@app.route("/api/book/<int:book_id>")
def get_book(book_id):
    db = get_db()
    cur = db.cursor()
    cur.execute(
        "SELECT BookId, Title, Author, Genre, Year, Description FROM Books WHERE BookId = %s",
        (book_id,)
    )
    r = cur.fetchone()
    cur.close()
    db.close()

    if not r:
        return jsonify({"error": "Book not found"}), 404

    return jsonify({"id": r[0], "title": r[1], "author": r[2],
                    "genre": r[3], "year": r[4], "description": r[5]})


# -------------------------------------------------
# RATE A BOOK
# DB kolonları: RatingId, UserId, BookId, Score, CreatedAt
# ON DUPLICATE KEY → (UserId, BookId) UNIQUE index var (EF migration'dan)
# -------------------------------------------------
@app.route("/api/rate", methods=["POST"])
def rate_book():
    data    = request.get_json(silent=True) or {}
    user_id = data.get("user_id")
    book_id = data.get("book_id")
    score   = data.get("score")

    if not all([user_id, book_id, score]):
        return jsonify({"error": "user_id, book_id ve score zorunludur"}), 400
    if not (1 <= int(score) <= 10):
        return jsonify({"error": "Puan 1-10 arasında olmalıdır"}), 400

    db = get_db()
    cur = db.cursor()
    cur.execute("""
        INSERT INTO Ratings (UserId, BookId, Score)
        VALUES (%s, %s, %s)
        ON DUPLICATE KEY UPDATE Score = %s
    """, (user_id, book_id, score, score))
    db.commit()
    cur.close()
    db.close()

    return jsonify({"status": "rating saved"})


# -------------------------------------------------
# LOG BOOK VIEW
# DB kolonları: ViewId, UserId, BookId, ViewedAt
# -------------------------------------------------
@app.route("/api/view/<int:book_id>", methods=["POST"])
def log_view(book_id):
    data    = request.get_json(silent=True) or {}
    user_id = data.get("user_id")

    db = get_db()
    cur = db.cursor()
    cur.execute(
        "INSERT INTO BookViews (UserId, BookId) VALUES (%s, %s)",
        (user_id, book_id)
    )
    db.commit()
    cur.close()
    db.close()

    return jsonify({"status": "view logged"})


# -------------------------------------------------
# ANALYTICS — MOST VIEWED BOOKS
# Dönüş: [{title, views}]  → C# BookAnalyticsDto.Views
# -------------------------------------------------
@app.route("/api/analytics/most-viewed")
def most_viewed():
    db = get_db()
    cur = db.cursor()
    cur.execute("""
        SELECT b.Title, COUNT(v.ViewId) AS views
        FROM BookViews v
        JOIN Books b ON v.BookId = b.BookId
        GROUP BY v.BookId, b.Title
        ORDER BY views DESC
        LIMIT 10
    """)
    rows = cur.fetchall()
    cur.close()
    db.close()

    return jsonify([{"title": r[0], "views": r[1]} for r in rows])


# -------------------------------------------------
# ANALYTICS — TOP RATED BOOKS
# Dönüş: [{title, rating, votes}]  → C# BookAnalyticsDto.Rating + .Votes
# -------------------------------------------------
@app.route("/api/analytics/top-rated")
def top_rated():
    db = get_db()
    cur = db.cursor()
    cur.execute("""
        SELECT b.Title, AVG(r.Score) AS avg_rating, COUNT(r.Score) AS votes
        FROM Ratings r
        JOIN Books b ON r.BookId = b.BookId
        GROUP BY r.BookId, b.Title
        HAVING votes >= 1
        ORDER BY avg_rating DESC
        LIMIT 10
    """)
    rows = cur.fetchall()
    cur.close()
    db.close()

    return jsonify([
        {"title": r[0], "rating": round(float(r[1]), 1), "votes": r[2]}
        for r in rows
    ])


# -------------------------------------------------
# ANALYTICS — GENRE POPULARITY
# Dönüş: [{genre, count}]  → C# GenrePopularityDto
# -------------------------------------------------
@app.route("/api/analytics/genre-popularity")
def genre_popularity():
    db = get_db()
    cur = db.cursor()
    cur.execute("""
        SELECT Genre, COUNT(*) AS total
        FROM Books
        GROUP BY Genre
        ORDER BY total DESC
    """)
    rows = cur.fetchall()
    cur.close()
    db.close()

    return jsonify([{"genre": r[0] or "Unknown", "count": r[1]} for r in rows])


# -------------------------------------------------
# ANALYTICS — VIEWS OVER TIME
# Dönüş: [{date, views}]  → C# ViewsOverTimeDto
# -------------------------------------------------
@app.route("/api/analytics/views-over-time")
def views_over_time():
    db = get_db()
    cur = db.cursor()
    cur.execute("""
        SELECT DATE(ViewedAt) AS day, COUNT(*) AS views
        FROM BookViews
        GROUP BY day
        ORDER BY day
    """)
    rows = cur.fetchall()
    cur.close()
    db.close()

    return jsonify([{"date": str(r[0]), "views": r[1]} for r in rows])


# -------------------------------------------------
# ANALYTICS — SEARCH TREND
# Dönüş: [{date, searches}]  → C# SearchTrendDto
# -------------------------------------------------
@app.route("/api/analytics/search-trend")
def search_trend():
    db = get_db()
    cur = db.cursor()
    cur.execute("""
        SELECT DATE(SearchedAt) AS day, COUNT(*) AS searches
        FROM SearchLogs
        GROUP BY day
        ORDER BY day
    """)
    rows = cur.fetchall()
    cur.close()
    db.close()

    return jsonify([{"date": str(r[0]), "searches": r[1]} for r in rows])


# -------------------------------------------------
# ANALYTICS — MOST ACTIVE USERS
# Dönüş: [{username, views}]  → C# ActiveUserDto
# -------------------------------------------------
@app.route("/api/analytics/most-active-users")
def most_active_users():
    db = get_db()
    cur = db.cursor()
    cur.execute("""
        SELECT u.Username, COUNT(v.ViewId) AS total_views
        FROM BookViews v
        JOIN Users u ON v.UserId = u.UserId
        GROUP BY v.UserId, u.Username
        ORDER BY total_views DESC
        LIMIT 5
    """)
    rows = cur.fetchall()
    cur.close()
    db.close()

    return jsonify([{"username": r[0], "views": r[1]} for r in rows])


# -------------------------------------------------
# RECOMMENDATION — Sadece Views'a göre
# Dönüş: [{title, rating, votes}]  → C# RecommendationDto
# C# karşılığı: GET api/recommendation/{userId}
# -------------------------------------------------
@app.route("/api/recommend/<int:user_id>")
def recommend(user_id):
    db = get_db()
    cur = db.cursor()

    cur.execute("""
        SELECT b.Genre, COUNT(*) AS total
        FROM BookViews v
        JOIN Books b ON v.BookId = b.BookId
        WHERE v.UserId = %s
        GROUP BY b.Genre
        ORDER BY total DESC
        LIMIT 1
    """, (user_id,))
    fav_genre = cur.fetchone()

    if not fav_genre:
        cur.close()
        db.close()
        return jsonify([])

    cur.execute("""
        SELECT b.Title, AVG(r.Score) AS rating, COUNT(r.Score) AS votes
        FROM Books b
        LEFT JOIN Ratings r ON b.BookId = r.BookId
        WHERE b.Genre = %s
        GROUP BY b.BookId, b.Title
        ORDER BY rating DESC
        LIMIT 5
    """, (fav_genre[0],))
    rows = cur.fetchall()
    cur.close()
    db.close()

    return jsonify([
        {"title": r[0], "rating": round(float(r[1]), 1) if r[1] else 0.0, "votes": r[2]}
        for r in rows
    ])


# -------------------------------------------------
# SMART RECOMMENDATION — Views + Favorites'a göre
# Dönüş: [{title, rating, votes}]  → C# RecommendationDto
# C# karşılığı: GET api/recommendation/smart/{userId}
# -------------------------------------------------
@app.route("/api/recommend-smart/<int:user_id>")
def recommend_smart(user_id):
    db = get_db()
    cur = db.cursor()

    cur.execute("""
        SELECT b.Genre, COUNT(*) AS total
        FROM (
            SELECT BookId FROM BookViews WHERE UserId = %s
            UNION ALL
            SELECT BookId FROM Favorites  WHERE UserId = %s
        ) AS user_books
        JOIN Books b ON user_books.BookId = b.BookId
        GROUP BY b.Genre
        ORDER BY total DESC
        LIMIT 1
    """, (user_id, user_id))
    fav_genre = cur.fetchone()

    if not fav_genre:
        cur.close()
        db.close()
        return jsonify([])

    cur.execute("""
        SELECT b.Title, AVG(r.Score) AS rating, COUNT(r.Score) AS votes
        FROM Books b
        LEFT JOIN Ratings r ON b.BookId = r.BookId
        WHERE b.Genre = %s
        GROUP BY b.BookId, b.Title
        ORDER BY rating DESC
        LIMIT 5
    """, (fav_genre[0],))
    rows = cur.fetchall()
    cur.close()
    db.close()

    return jsonify([
        {"title": r[0], "rating": round(float(r[1]), 1) if r[1] else 0.0, "votes": r[2]}
        for r in rows
    ])


# -------------------------------------------------
# ADD / REMOVE FAVORITE (toggle)
# DB kolonları: FavId, UserId, BookId, CreatedAt
# C# karşılığı: POST/DELETE api/favorites
# -------------------------------------------------
@app.route("/api/favorite", methods=["POST"])
def toggle_favorite():
    data    = request.get_json(silent=True) or {}
    user_id = data.get("user_id")
    book_id = data.get("book_id")

    if not all([user_id, book_id]):
        return jsonify({"error": "user_id ve book_id zorunludur"}), 400

    db = get_db()
    cur = db.cursor()
    cur.execute(
        "SELECT FavId FROM Favorites WHERE UserId=%s AND BookId=%s",
        (user_id, book_id)
    )
    exists = cur.fetchone()

    if exists:
        cur.execute(
            "DELETE FROM Favorites WHERE UserId=%s AND BookId=%s",
            (user_id, book_id)
        )
        action = "removed"
    else:
        cur.execute(
            "INSERT INTO Favorites (UserId, BookId) VALUES (%s, %s)",
            (user_id, book_id)
        )
        action = "added"

    db.commit()
    cur.close()
    db.close()

    return jsonify({"status": action})


# -------------------------------------------------
# LOG SEARCH
# DB kolonları: SearchId, UserId, Query, SearchedAt
# C# karşılığı: GET api/books/search (log otomatik eklenir C# tarafında)
# -------------------------------------------------
@app.route("/api/search", methods=["POST"])
def log_search():
    data    = request.get_json(silent=True) or {}
    user_id = data.get("user_id")
    query   = data.get("query")

    if not query:
        return jsonify({"error": "query zorunludur"}), 400

    db = get_db()
    cur = db.cursor()
    cur.execute(
        "INSERT INTO SearchLogs (UserId, Query) VALUES (%s, %s)",
        (user_id, query)
    )
    db.commit()
    cur.close()
    db.close()

    return jsonify({"status": "logged"})


# -------------------------------------------------
# REGISTER USER
# DB kolonları: UserId, Username, Email, PasswordHash, CreatedAt
# bcrypt kullanıyoruz — C# BCrypt.Net ile aynı algoritma ($2b$)
# C# karşılığı: POST api/auth/register
# -------------------------------------------------
@app.route("/api/register", methods=["POST"])
def register():
    data     = request.get_json(silent=True) or {}
    username = data.get("username", "").strip()
    email    = data.get("email", "").strip()
    password = data.get("password", "")

    if not all([username, email, password]):
        return jsonify({"error": "username, email ve password zorunludur"}), 400

    password_hash = bcrypt.hashpw(password.encode("utf-8"), bcrypt.gensalt()).decode("utf-8")

    db = get_db()
    cur = db.cursor()
    try:
        cur.execute(
            "INSERT INTO Users (Username, Email, PasswordHash) VALUES (%s, %s, %s)",
            (username, email, password_hash)
        )
        db.commit()
    except Exception:
        return jsonify({"error": "Bu kullanıcı adı veya e-posta zaten kullanımda"}), 409
    finally:
        cur.close()
        db.close()

    return jsonify({"status": "registered"}), 201


# -------------------------------------------------
# LOGIN USER
# bcrypt ile doğrulama — C# BCrypt.Net.BCrypt.Verify ile uyumlu
# C# karşılığı: POST api/auth/login
# -------------------------------------------------
@app.route("/api/login", methods=["POST"])
def login():
    data     = request.get_json(silent=True) or {}
    username = data.get("username", "").strip()
    password = data.get("password", "")

    if not all([username, password]):
        return jsonify({"error": "username ve password zorunludur"}), 400

    db = get_db()
    cur = db.cursor()
    cur.execute(
        "SELECT UserId, PasswordHash FROM Users WHERE Username=%s",
        (username,)
    )
    user = cur.fetchone()
    cur.close()
    db.close()

    if not user:
        return jsonify({"error": "Kullanıcı bulunamadı"}), 404

    user_id, password_hash = user

    if not bcrypt.checkpw(password.encode("utf-8"), password_hash.encode("utf-8")):
        return jsonify({"error": "Şifre hatalı"}), 401

    return jsonify({"status": "login success", "user_id": user_id})


# -------------------------------------------------
# RUN APP
# port=5000 → C# appsettings.json FlaskApi:BaseUrl ile uyumlu
# -------------------------------------------------
if __name__ == "__main__":
    app.run(host="0.0.0.0", debug=False, port=5000)
