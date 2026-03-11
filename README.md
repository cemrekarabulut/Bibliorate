# 📚 BiblioRate: Book Discovery & Rating Platform

BiblioRate, kullanıcıların yeni kitaplar keşfetmesini, okudukları eserleri oylamasını ve kişisel kütüphanelerini dijital ortamda yönetmesini sağlayan kapsamlı bir platformdur.

## 🏗️ Technical Architecture

Proje, sürdürülebilir ve test edilebilir bir yapı sunmak adına **Onion Architecture** (Soğan Mimarisi) prensiplerine uygun olarak geliştirilmiştir. Bu sayede iş mantığı (business logic), dış bağımlılıklardan (veritabanı, API servisleri) tamamen izole edilmiştir.

### Core Stack
* **Language:** Python
* **Database:** MySQL (Relational Database Management)
* **Architecture:** Onion Architecture
* **API Framework:** Flask / FastAPI 

## 🌟 Key Features

* **Advanced Search:** Kitap ismi, yazar veya türe göre detaylı arama motoru.
* **Rating System:** Okunan kitaplara puan verme ve kullanıcı yorumları.
* **Personal Library:** "Okunacaklar" ve "Okunanlar" listeleri oluşturma.
* **Modern UI:** Kullanıcı dostu ve estetik arayüz tasarımı.

## 📂 Project Structure

```text
BiblioRate/
├── BiblioRate.API/            # Presentation Layer: API endpoints and controllers
├── BiblioRate.Application/    # Application Layer: Business logic and services
├── BiblioRate.Domain/         # Core Layer: Entities, interfaces and domain logic
└── BiblioRate.Infrastructure/ # Infrastructure Layer: Database and external tools
