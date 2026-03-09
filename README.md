# 💡 Fikir Havuzu - Üniversite İçi İnovasyon Platformu
Bu proje, bir kurum veya üniversite içerisindeki personelin/öğrencilerin fikirlerini paylaşabildiği, bu fikirlerin değerlendirildiği ve aktif katılımın ödüllendirildiği sıfırdan tasarlanmış bir web platformudur.
---
## 🚀 Öne Çıkan Özellikler
Sıfırdan Tasarlanmış Kullanıcı Yönetimi: IdentityServer veya hazır Microsoft Identity kütüphaneleri kullanılmadan, tamamen Cookie Authentication ve Claims tabanlı manuel bir altyapıyla geliştirilmiştir.

Liderlik Tablosu (Leaderboard): En aktif 5 fikirci, puanlarına ve rütbelerine (Üstad, Kıdemli, Kaşif vb.) göre dinamik olarak listelenir.

Fikir Değerlendirme Sistemi: Fikirler; konu, öncelik ve içerik kriterlerine göre puanlanabilir ve onay durumuna göre performans analizi yapılır.

Puan Market Sistemi: Kazanılan puanlarla ürün satın alınabilen, AJAX tabanlı çalışan bir envanter sistemi.

Gelişmiş Filtreleme: Başlık, tarih, personel ve konu bazlı akıllı arama motoru.

## 🛠️ Kullanılan Teknolojiler
Backend: ASP.NET Core 8.0 MVC

Database: MS SQL Server & Entity Framework Core (Code First)

Frontend: Bootstrap 5, jQuery, AJAX, SweetAlert2, Animate.css

Mimari: N-Tier Architecture (Entities, DataAccess, Business, WebUI)

## 🔑 Test Bilgileri
Projeyi test edebilmeniz için hazır kullanıcı bilgileri aşağıdadır. (Veritabanı otomatik olarak Seed Data ile bu kullanıcıları oluşturacaktır):

### Admin Hesabı
   
E-posta: admin@fikirhavuzu.com

Şifre: 123

Yetki: Fikir silme, tüm süreçleri yönetme.

### Kullanıcı Hesabı
E-posta: asliabyk@gmail.com (Kayıt ol sayfasından yeni bir hesap oluşturabilir veya veritabanındaki mevcut bir kullanıcıyı kullanabilirsiniz.)

Şifre: 123456 (Kayıt sırasında belirlenen şifre.)

## ⚙️ Kurulum Notları
appsettings.json veya AppDbContext içindeki ConnectionString bilgisini kendi SQL Server adresinize göre güncelleyin.

Package Manager Console üzerinden update-database komutunu çalıştırarak tabloları oluşturun.
