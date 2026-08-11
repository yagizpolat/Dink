# DINK Oyun Projesi Durum Raporu

## Proje Genel Bakışı

- **Proje Yolu:** C:\Users\engin\OneDrive\Belgeler\Kisisel Dosyalarim\ozel\Oyun Gelistirme\Denemelik projelerim\Dink
- **Unity Sürümü:** Unity 6 (6000.0.14f1) LTS, URP (Universal Render Pipeline)
- **Hedef:** Birinci şahıs (FPS), sabit kamera açılı, atmosferik psikolojik korku ve kapı seçimi oyunu.
- **Dil Desteği:** 7 Dilli canlı seslendirme ve altyazı altyapısı (Varsayılan: İngilizce; Desteklenen: İngilizce, Türkçe, Almanca, Fransızca, İspanyolca, Portekizce, Rusça).

---

## 0. Resmî Oyun Hikayesi & Karakter Mimarisi (Lore)

- **Karakter:** **Hakan Kaya** (16 Yaşında, Lise Terk)
  - **Geçmiş & Yaşam:** Annesi ve babası vefat etmiş. Annesinden ve babasından kalan tek evde küçük kız kardeşiyle tek başına yaşıyor. Büyüğü olmadığı için erken yaşta ağır sorumluluklar üstlenmiş, düşük maaşla ağır şartlarda markette çalışarak faturaları ödemeye çalışan, hayattan şimdiden ümidini kesmiş bir genç.
- **Kapı Cehennemine Giriş:**
  - Vardiya sonunda iş arkadaşıyla kavga eden ve dayak yiyen Hakan, mesaisi bitmeden kendini sokağa atar. İş arkadaşı öldürme niyetiyle peşine düşer. Dar bir sokakta kaçarken, sokağın ortasında geçişi tamamen kapatan devasa bir kapı görür. Ölüm korkusu ve bu dünyadan kurtulma içgüdüsüyle kapıdan içeri girer.
- **Mektup Sistemi:**
  - Bu kapı döngüsüne daha önce düşmüş eski kurbanların notlarıdır. Hem derin bir dram ve duygu sunar hem de doğru kapıyı bulduracak şifre ve ipuçlarını içerir.
- **Akıl Sağlığı Sistemi (Sanity):**
  - Akıl sağlığı düştükçe görüş bulanıklaşır, kapı seçerken imleç kendi kendine titremeye başlar ve bulmaca/minioyun zorluğu artar.
- **Çift Sonlu Hikaye Mimarisi:**
  - **İyi Son (Kaçış):** Yeterli mektup toplayıp hikayenin tüm parçalarını birleştiren oyuncular kapı cehenneminden kaçar.
  - **Kötü Son (Sonsuz Döngü):** Eksik mektup ve yarım bilgiyle bitiren oyuncular sonsuz bir kapı açma döngüsüne hapsolur.

---

## 1. Ana Mimari & Mekanik Özet

- **Ana Şablon Oda (Master Template Room):**
  - İki kapılı (Sol Mavi / Sağ Kırmızı) temel sahne yapısı. Bu şablon mükemmelleştirilerek gelecek bölümler için kopyalanacaktır.
  - Sol duvarda organik küf lekesi (`Isolated_Mold_TrueAlpha.mat`), sağ duvarda sıva çatlağı (`Wall_Crack_TrueAlpha.mat`) ve havalanan mikro toz zerreleri (`Atmospheric_Dust`) bulunur.
- **Gelişmiş Kapı Işıkları & Süzülme:**
  - Sol kapı üzerinde Mavi, Sağ kapı üzerinde Kırmızı 3D armatür lambaları bulunur.
  - Lambalar doğrudan kapılara ebeveynlenmiştir; kapılar süzüldüğünde ışıklar senkronize hareket eder.
- **Aydınlatma Felsefesi:**
  - Ortam ışığı koyu gri/mavi tonuna (RGB 45, 50, 60) ayarlanmıştır. Ortam hem loş hem de oynanabilir seviyededir.

---

## 2. Son Yapılan Değişiklikler ve Eklenen Sistemler (11.08.2026)

### A. 7 Dilli Canlı Dil Yöneticisi ([LanguageManager.cs](file:///C:/Users/engin/OneDrive/Belgeler/Kisisel%20Dosyalarim/ozel/Oyun%20Gelistirme/Denemelik%20projelerim/Dink/Assets/_Project/Scripts/Menu%20Kodlar/LanguageManager.cs), [LocalizedText.cs](file:///C:/Users/engin/OneDrive/Belgeler/Kisisel%20Dosyalarim/ozel/Oyun%20Gelistirme/Denemelik%20projelerim/Dink/Assets/_Project/Scripts/LocalizedText.cs), [LocalizationHelper.cs](file:///C:/Users/engin/OneDrive/Belgeler/Kisisel%20Dosyalarim/ozel/Oyun%20Gelistirme/Denemelik%20projelerim/Dink/Assets/_Project/Scripts/Editor/LocalizationHelper.cs))
- **7 Dil Desteği:** İngilizce (Varsayılan / Default), Türkçe, Almanca, Fransızca, İspanyolca, Portekizce ve Rusça dilleri eklendi.
- **Kalıcı Hafıza Kaydı:** Seçilen dil `PlayerPrefs` (`Dink_Language`) ile saklanır; oyuncu ilk girdiğinde varsayılan dil olarak İngilizce yüklenir.
- **Canlı Güncelleme:** Dil değiştirildiğinde `OnLanguageChanged` olayı tetiklenerek sahnedeki 2D Canvas ve 3D kapı metinleri anında güncellenir.

### B. 7 Dilli Seslendirme ve Altyazı Altyapısı ([SubtitleManager.cs](file:///C:/Users/engin/OneDrive/Belgeler/Kisisel%20Dosyalarim/ozel/Oyun%20Gelistirme/Denemelik%20projelerim/Dink/Assets/_Project/Scripts/In%20Game/SubtitleManager.cs))
- Oyun açılışındaki göz açılma sinematiğinde seçilen dile göre doğru ses kaydının çalması ve ilgili dilde altyazının gösterilmesi sağlandı.

### C. 3D Dev Kapılı Ana Menü ve AAA Ayarlar Paneli ([Menu3DDoor.cs](file:///C:/Users/engin/OneDrive/Belgeler/Kisisel%20Dosyalarim/ozel/Oyun%20Gelistirme/Denemelik%20projelerim/Dink/Assets/_Project/Scripts/Menu%20Kodlar/Menu3DDoor.cs), [MainMenu3DController.cs](file:///C:/Users/engin/OneDrive/Belgeler/Kisisel%20Dosyalarim/ozel/Oyun%20Gelistirme/Denemelik%20projelerim/Dink/Assets/_Project/Scripts/Menu%20Kodlar/MainMenu3DController.cs), [SettingsManager.cs](file:///C:/Users/engin/OneDrive/Belgeler/Kisisel%20Dosyalarim/ozel/Oyun%20Gelistirme/Denemelik%20projelerim/Dink/Assets/_Project/Scripts/Menu%20Kodlar/SettingsManager.cs))
- 3D menü kapıları (BAŞLA, AYARLAR, ÇIKIŞ ve GENEL, DİL, GERİ DÖN) ve 3 bağımsız ses kanalı (Genel, Müzik, SFX) yapılandırıldı.

---

## 3. Dikkat Edilmesi Gereken Temel Kurallar

- **DİL KISITI (EN YÜKSEK ÖNCELİK):** Bütün Markdown dokümanlarında (.md) ve konuşmalarda KESİNLİKLE Türkçe dışında bir dil kullanılmayacaktır.
- **Fener Pil Tüketim Katsayısı:** Kullanıcı talimatı gereği Fener pil tüketim hızı kesinlikle değiştirilmeyecektir.
- **Dosya Silme İzni:** Kullanıcıdan izin almadan proje dosyası silinmeyecektir.
- **Master Şablon Korunması:** Sahne 1 (Giriş/Öğretici) tamamlanmış olup yeni sahneler bu Master şablon kopyalanarak türetilecektir.

---

## 4. Oyuncu Geri Bildirimlerine Göre v0.4 Geliştirme Yol Haritası

1. **✅ ADIM 1: Giriş Sinematiği Hızlandırma & Skip Koruması (TAMAMLANDI)**
2. **✅ ADIM 2: Bağımsız Ses Sistemleri ve Müzik Yapılandırması (TAMAMLANDI)**
3. **📌 ADIM 3: Mor Işık (UV) ve Gizli Yazı / İpuçları (Bulmaca Mekaniği - SIRADAKİ ADIM)**
   - Fener için farenin sağ tuşu ile aktifleşen mor ışık modu.
   - Duvarlarda ve mektuplarda Hakan'ın ve eski kurbanların izlerini sadece mor ışık altında gösteren tekinsiz semboller.
4. **📌 ADIM 4: Hakan'ın Akıl Sağlığı (Sanity) & İmleç Titremesi & Anlık Gerilim Dynamics**
   - Akıl sağlığı düştükçe imlecin sallanması, bulanıklaşma ve anlık korku ögeleri.
5. **📌 ADIM 5: Mektup Sayımına Göre Çift Sonlu Hikaye Finali & Skor Tablosu**
