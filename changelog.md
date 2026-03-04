# RandevuTakip (BookPilot) - Değişiklik Günlüğü (Changelog)

## [UI/UX Redesign] - Humanized & Premium Interface - *TAMAMLANDI*

- **Tipografi Değişimi:** Uygulamanın global fontu standart Inter/Arial yerine, daha premium ve modern duran Google Font `Plus Jakarta Sans` ile değiştirildi.
- **İkonografi:** Standart Lucide ikonları tamamen kaldırılarak, yerine daha karakterli ve belirgin olan `Phosphor Icons` kütüphanesi entegre edildi.
- **Ana Sayfa (Landing Page) Revizyonu:** Doctris tarzı asimetrik tasarıma geçildi. Kullanıcının sağladığı özel görsel Hero bölümüne eklendi. "Uzman Kadromuz" bölümü için gerçekçi (mock) insan yüzleri (pravatar) entegre edildi ve en alta 3 adet sosyal kanıt (Testimonial/Yorum) kartı konuldu.
- **Randevu Sihirbazı (Booking Page):** Siyah, kalın `border` yapıları kaldırılarak yerine çok hafif gölgeli (soft shadow) ve yuvarlak hatlı (`rounded-[2rem]`) ferah kart tasarımları getirildi. Arka plana rahatlatıcı 'Soft Blob' renkleri yerleştirildi.
- **Hizmet Kataloğu:** `/services` sayfası yeniden tasarlanarak her bir hizmet kartının tepesine dinamik (ID'ye göre hashlenmiş) yüksek kaliteli Unsplash fotoğraf kapağı (Cover Image) eklendi.
- **Mikro Etkileşimler:** Ana ekran girişlerine, hizmet kartlarına ve randevu butonlarına `framer-motion` kütüphanesi ile yumuşak geliş (staggered) ve üzerine gelince büyüme (hover/tap) animasyonları kodlandı.

## [Phase 6 / Sprint 10] - Productization & Sales Kit - *TAMAMLANDI*

- **Multi-Industry Demos**: Veritabanı seed işlemine üç farklı sektör (Diş Hekimi, Avukat, Güzellik Salonu) eklendi. Her birine özel renk temaları (`ThemeJson`) ve dinamik randevu formları (`BookingFormSchema`) tanımlandı.
- **Demo Hesapları**: Geliştirme ve tanıtım süreçlerini kolaylaştırmak adına her tenant için (`admin@dentist.com`, `admin@lawyer.com`, vb.) varsayılan admin şifreleri entegre edildi.
- **Dökümantasyon**: `README.md` içerisine sistem gereksinimleri, demo bilgileri, bilinen limitasyonlar ve gelecek yol haritası (Roadmap) eklendi. Ayrıca potansiyel müşterilere sistemi tanıtmak için `demo-script.md` dosyası oluşturuldu.

## [Phase 5 / Sprint 09] - Deploy, CI/CD, Observability - *TAMAMLANDI*

- **Production Deployment Packaging**: Proje için `docker-compose.yml` (API, PostgreSQL, Redis, Frontend ve NGINX dahil) ve production'a özel `Dockerfile`'lar oluşturuldu.
- **NGINX Reverse Proxy**: 80 portu üzerinden HTTP isteklerini frontend ve backend servislerine (proxy_pass) yönlendiren, Gzip sıkıştırmalı `nginx.conf` yapılandırıldı.
- **CI Pipeline (GitHub Actions)**: `.github/workflows/ci.yml` eklenerek projede kod master'a itildiğinde .NET ve Node.js için otomatik derleme (build) süreçleri aktifleştirildi.
- **Observability**: `CorrelationIdMiddleware` ile her HTTP isteğine benzersiz bir `X-Correlation-Id` atanması sağlandı ve hata ayıklama süreçleri için loglara yansıtıldı.

## [2026-03-01] - Redis, Concurrency & Security Hardening (Sprint 08) - *TAMAMLANDI*

- **Redis Altyapısı**: Projeye `StackExchange.Redis` entegre edildi ve `IDistributedCache` olarak yapılandırıldı.
- **Dağıtık Rate Limiting**: Randevu oluşturma uç noktası IP bazlı (saatte 5 istek) Redis üzerinden sınırlandırıldı.
- **Atomic Slot Locking**: `BookingService` içerisinde Redis tabanlı "lock" mekanizması ile aynı slotun eşzamanlı (race condition) alınması tamamen engellendi.
- **Availability Caching**: Müsaitlik sorgu sonuçları performans artışı için 60 saniye boyunca Redis'te önbelleğe alındı ve yeni randevu ile otomatik senkronizasyon sağlandı.
- **Gelişmiş Audit Logging**: Admin panelindeki kritik veri değişiklikleri ve durum güncellemeleri için merkezi audit log mekanizması doğrulandı.

Bu dosya, projede gerçekleştirilen sprintleri, tamamlanan modülleri ve gelecekte yapılması planlanan hedefleri kronolojik olarak özetler.

## [Phase 4] - Android (Capacitor) Entegrasyonu, Mobile Bridge & UI - *TAMAMLANDI*

- **Android Ağ Köprüsü (10.0.2.2 Bridge)**: Emülatörün PC'deki backend'e erişebilmesi için API istemci katmanı ve Android Manifest izinleri (`usesCleartextTraffic`) yapılandırıldı.
- **Gelişmiş Mobil Giriş Ekranı**: Placeholder ekran yerine butonlu, logolu ve responsive bir karşılama ekranı tasarlandı.
- **Build & Sync Optimizasyonu**: Google Fonts bağımlılıkları temizlenerek offline build hataları giderildi ve tüm değişiklikler Android tarafına senkronize edildi.
- **SMTP Mail Bildirim Sistemi (Kritik Fix)**: 
  - `fodasSmtp@gmail.com` üzerinden mail gitmeme sorunu teşhis edildi.
  - Veritabanındaki placeholder verilerinin (`user@test.com`) konfigürasyonu ezmesi engellendi ve e-posta gönderimi başarıyla doğrulandı.

---

## [Phase 3] - İşletme Ayarları ve Form Builder - *TAMAMLANDI*

- **Görsel Form Yapıcı (Form Builder)**: `/admin/settings` arayüzündeki ham JSON `BookingFormSchema` text alanı kaldırılarak görsel bir sürükle-bırak Form oluşturucu ("Soru Ekle") paneline dönüştürüldü.
- **Dinamik Müşteri Randevu Formu**: Halka açık randevu alma sayfasındaki (`/book`) Bilgiler sekmesine bu özel soruların (Dropdown, Textarea, Metin, Onay kutusu) otomatik olarak render edilmesi sağlandı.
- **Form Geri Bildirimi**: Randevular ekranında (`/admin/appointments`) Admin'in detay (Modal) kısmında `ExtraJson` aracılığıyla kullanıcının girdiği özel soruların ("Şikayetiniz", "Evcil Hayvanınızın Türü" vb.) düzgün field etiketleriyle okunması geliştirildi.

---

## [Phase 2] - Personel (Staff) Paneli ve Rol Tabanlı Yetkilendirme - *TAMAMLANDI*

- **Role-Based Access Control (RBAC)**: Backend üzerindeki JWT mimarisi Owner ve Staff şeklinde iki kırılıma ayrıldı.
- **Hizmet-Personel İlişkisi (StaffServices)**: Yeni bir hizmet eklenirken bu hizmeti hangi personellerin sunacağı "Checkbox List" şeklinde eklendi.
- **SideBar Gizleme**: Personel giriş yaptığında Raporlar, Ayarlar, Müşteriler, Personeller gibi sekmeler menüde gizlenerek sadece Randevular modülü yetkisine göre sınırlandırıldı.

---

## [Phase 1] - İletişim & Bildirim Sistemi - *TAMAMLANDI*

- **SMTP E-posta Gönderim Servisi**: Müşterilere bildirim e-postaları (Randevunuz Alındı, Randevunuz Onaylandı) atmak üzere `IEmailService` adında asenkron bir interface eklendi.
- **Mail Şablonları**: E-postaların göze hoş görünmesi için (Yeşil renkli, saat tarih bilgili) mini HTML şablonları tasarlandı.

---

## [Eski Sprintler / Sprint 0] - Temel Admin ve Randevu İşlevleri - *TAMAMLANDI*

- **Zengin Dashboard**: Bugünün randevuları, gelir özetleri, haftalık performans, bekleyen randevular ve bekleyen durumlar mini widget'lar ile (Chart opsiyonlarıyla beraber) analiz ekranına dönüştürüldü.
- **Randevu Detay Modalleri**: Müşteri notlarının gizli kalmasını önlemek için tıklanabilir detay / onayla / iptal et gibi randevu akış kartları entegre edildi.
- **Raporlar ve Müşteri Listeleri**: Hizmet ağırlıklı gelir durum raporları ve en çok randevu alan müşteriler sayfası yapıldı.
- **POST Randevu**: Saat slotlarının 500 Hatası fırlatma bugları çözümlendi.

