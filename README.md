# RandevuTakip (BookPilot)

Modern, hızlı ve ölçeklenebilir bir randevu takip ve yönetim sistemi. Bu uygulama, hem işletmelerin (Admin/Personel) hem de müşterilerin kullanımına sunulmuş tam kapsamlı bir çözümdür.

## 🚀 Temel Özellikler

- **Multi-Tenant Mimari**: Birden fazla işletmeyi tek bir sistem üzerinden yönetebilme.
- **Dinamik Form Yapıcı**: İşletme bazlı özelleştirilebilir randevu formları.
- **Personel Yönetimi & RBAC**: Rol tabanlı yetkilendirme (Sahip ve Personel rolleri).
- **Akıllı Müsaitlik Hesaplama**: Personel vardiyalarına ve mevcut randevulara göre otomatik slot hesaplama.
- **Concurrency Protection**: Redis tabanlı atomic slot kilitleme ile çakışan randevuların engellenmesi.
- **Distributed Rate Limiting**: Güvenlik için istek sınırlama.
- **Detaylı Audit Log**: Tüm admin faaliyetlerinin takip edilebilirliği.
- **Mobil Uyumlu (Capacitor)**: Android ve web üzerinden erişim.

## 🛠️ Teknoloji Yığını

- **Backend**: .NET 9.0 (ASP.NET Core Web API)
- **Veritabanı**: PostgreSQL
- **Cache & Lock**: Redis
- **Logging**: Serilog (Structural Logging)
- **Frontend**: Next.js 14, TypeScript, Tailwind CSS, shadcn/ui
- **Containerization**: Docker & Docker Compose

---

## 💻 Kurulum ve Çalıştırma

### Yöntem 1: Docker Compose (Önerilen)

En hızlı kurulum şekli Docker kullanmaktır. Hiçbir yerel bağımlılık (Postgres, Redis vb.) kurmanıza gerek kalmaz.

1.  **Repo'yu Clone'layın**:
    ```bash
    git clone https://github.com/acemersoy/RandevuTakip.git
    cd RandevuTakip
    ```

2.  **Sistemi Ayağa Kaldırın**:
    ```bash
    docker-compose up -d --build
    ```

3.  **Erişim**:
    - **Frontend**: `http://localhost:3000`
    - **Backend API**: `http://localhost:5032/swagger`
    - **Varsayılan Admin**: `admin@demo.com` / `admin123`

---

### Yöntem 2: Manuel Kurulum (Geliştiriciler İçin)

#### 1. Backend Hazırlığı
- `.NET 9.0 SDK` yüklü olduğundan emin olun.
- PostgreSQL ve Redis'in yerel olarak (veya Docker'da) çalıştığından emin olun.
- `src/backend/RandevuTakip.Api/appsettings.json` içindeki `ConnectionStrings` ayarlarını kontrol edin.
- Çalıştırın:
  ```bash
  cd src/backend/RandevuTakip.Api
  dotnet run
  ```

#### 2. Frontend Hazırlığı
- `Node.js 18+` yüklü olduğundan emin olun.
- Bağımlılıkları kurun ve çalıştırın:
  ```bash
  cd src/frontend
  npm install
  npm run dev
  ```
- Tarayıcıda `http://localhost:3000` adresine gidin.

---

## 🔒 Güvenlik Uyarıları

- Üretim (Production) ortamına alırken `appsettings.json` içindeki `Jwt:Key` değerini güvenli bir anahtar ile değiştirmeyi unutmayın.
- Varsayılan veritabanı şifrelerini `docker-compose.yml` ve `appsettings.json` içerisinden güncelleyin.
