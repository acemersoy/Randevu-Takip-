# RandevuTakip (BookPilot) - Multi-tenant Appointment Platform

Modern, hızlı ve sektörden bağımsız çok kiracılı (multi-tenant) randevu yönetim sistemi.

## 🚀 Teknolojiler

- **Backend:** .NET 9 Web API, Entity Framework Core, PostgreSQL, Redis
- **Frontend:** Next.js 15 (App Router), TypeScript, Tailwind CSS, Zustand, TanStack Query, Zod
- **DevOps:** Docker, Docker Compose

## 🛠️ Kurulum ve Çalıştırma

### 1. Veritabanı ve Redis'i Başlatın
```bash
docker-compose up -d
```

### 2. Backend'i Çalıştırın
```bash
cd src/backend/RandevuTakip.Api
dotnet run
```

### 3. Frontend'i Çalıştırın
```bash
cd src/frontend
npm install
npm run dev
```

## 🌍 URL Yapısı
Proje çok kiracılı bir yapıya sahiptir. URL üzerinden tenant belirlenir:
- `http://localhost:3000/demo` -> 'demo' kiracısının ana sayfası.
- `http://localhost:3000/demo/admin` -> 'demo' kiracısının admin paneli.

## 📝 Changelog
Tüm değişiklikleri `changelog.md` dosyasından takip edebilirsiniz.
