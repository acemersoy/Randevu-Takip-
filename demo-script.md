# RandevuTakip (BookPilot) - Demo Sunum Senaryosu

Bu senaryo, RandevuTakip (BookPilot) projesini potansiyel müşterilere (Diş Hekimi, Avukat, Güzellik Salonu vb.) sunarken izlenecek adımları içermektedir.

---

## 1. Giriş ve Karşılama
**Amaç:** Sistemin birden fazla sektörü nasıl tek bir altyapıdan yönettiğini göstermek.

- Tarayıcıyı açın ve ana ekrana gelin.
- **Satıcı:** *"BookPilot, işletmenizin türü ne olursa olsun sizin kurallarınıza ve temanıza anında uyum sağlayan bir altyapıdır. Örneğin, şu üç farklı sektöre bakalım:"*
  - `http://localhost:3000/dentist` adresine gidin. Renklerin mavi tonda ve hizmetlerin (Kanal Tedavisi vb.) dişe özel olduğunu gösterin.
  - `http://localhost:3000/lawyer` adresine gidin. Renklerin lacivert/siyah tonda olduğunu ve ağırbaşlı bir tasarıma büründüğünü gösterin.
  - `http://localhost:3000/salon` adresine gidin. Renklerin pembe tonlarında, daha canlı bir tasarıma geçtiğini gösterin.

---

## 2. Randevu Oluşturma Deneyimi (Müşteri Gözünden)
**Amaç:** Randevu almanın ne kadar kolay ve sektöre özel olduğunu kanıtlamak.

- `http://localhost:3000/dentist` sayfasına geri dönün.
- **Satıcı:** *"Şimdi bir müşteri olarak randevu alalım. 'Genel Muayene' hizmetini seçiyoruz."*
- Tarih ve saati seçin. (Sistemin mevcut dolu saatleri otomatik gizlediğini belirtin).
- **Bilgi Ekranı:**
  - Formda yer alan **"Şikayetiniz"** alanını vurgulayın. *"Gördüğünüz gibi Diş Hekimi için şikayet soruluyor."*
- `http://localhost:3000/lawyer` sekmesine geçip aynı adımları yapın ve Bilgi Ekranında **"Dava Türü"** (Boşanma, Ceza vb.) ve **"Konu Özeti"** sorularının çıktığını gösterin.
  - **Satıcı:** *"Aynı altyapı, tek satır kod yazmadan Avukat için tamamen farklı bir form üretiyor."*
- Randevuyu onaylayın ve müşteri için bitirin.

---

## 3. Yönetim Paneli (İşletme Gözünden)
**Amaç:** İşletme sahibinin sistemi nasıl kontrol ettiğini ve kolaylığını göstermek.

- Yeni sekmede `http://localhost:3000/dentist/admin` adresine gidin.
- `admin@dentist.com` / `admin123` ile giriş yapın.
- **Dashboard:**
  - Günlük randevu sayılarını, gelir durumunu ve grafik ekranını kısaca gösterin.
- **Randevular:**
  - Az önce müşteri gözünden oluşturduğunuz randevunun anında sisteme düştüğünü gösterin.
  - Randevu detayına (Göz ikonuna) tıklayın.
  - Müşterinin formda doldurduğu "Şikayetiniz" bilgisinin admin tarafından kolayca okunabildiğini vurgulayın.
  - Randevu durumunu **Onaylandı** olarak değiştirin. *"Müşteriye şu an onay maili gitti,"* diyerek mail entegrasyonundan bahsedin.

---

## 4. Dinamik Özelleştirme (Ayarlar)
**Amaç:** Sistemin kodlamaya ihtiyaç duymadan arayüzden nasıl özelleştirilebileceğini göstermek.

- **Ayarlar** sekmesine tıklayın.
- **Tema:** Ana rengi (Örn: `#4f46e5` yerine yeşil `#10b981`) olarak değiştirip kaydedin. Public sayfanın anında yeşile döndüğünü gösterin.
- **Form Oluşturucu:**
  - Form alanında sürükle bırak veya "Yeni Soru Ekle" butonunu kullanarak "Kan Grubunuz" adında yeni bir Dropdown soru ekleyin.
  - Kaydedip public `/book` sayfasına dönün ve randevu alırken bu sorunun anında formda çıktığını kanıtlayın.
- **Personeller ve Hizmetler:** Personel bazlı çalışma saatleri ve kime hangi hizmetin atanabildiğini göstererek sunumu tamamlayın.

---

**Kapanış:** *"BookPilot, işletmenizin büyümesine engel olan tüm operasyonel yükleri tek bir çatı altında çözer. Sizin için kurmamızı ister misiniz?"*