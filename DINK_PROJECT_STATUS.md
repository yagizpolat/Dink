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

## 2. Son Yapılan Değişiklikler ve Eklenen Sistemler (12.08.2026 Oturumu)

### A. LootLocker Bulut Destekli Küresel Skor Tablosu & 7 Dilli Arayüz ([LeaderboardManager.cs](file:///C:/Users/engin/OneDrive/Belgeler/Kisisel%20Dosyalarim/ozel/Oyun%20Gelistirme/Denemelik%20projelerim/Dink/Assets/_Project/Scripts/Menu%20Kodlar/LeaderboardManager.cs))
- **Doğrudan REST API Entegrasyonu:** Ek eklenti paketine ihtiyaç duymadan doğrudan `UnityWebRequest` ile LootLocker sunucularına bağlanma.
- **Canlı Bulut Skorları:** Oyuncu oyunu bitirdiğinde tamamlama süresi milisaniye cinsinden LootLocker bulutuna iletilir ve dünyanın her yerinden görülebilecek canlı liderlik tablosunda sıralanır.
- **Oturum Kurtarma & Çevrimdışı Güvence:** İnternet olmaması durumunda yerel `PlayerPrefs` yedek hafızasına kaydeder. Oturum kapalıysa otomatik yeniden açar.
- **7 Dilli Tam Yerelleştirme:** Skor Tablosu UI paneli ve Kullanıcı Adı Paneli 7 dilde (EN, TR, DE, FR, ES, PT, RU) canlı olarak gösterilir.
- **Unity Editor Sıfırlama:** `ContextMenu` üzerinden tek tıkla yerel skorları ve ismi sıfırlama olanağı.
- **Steamworks Gelecek Mimarisi:** İleride Steam'e çıkıldığında arayüz hiç değişmeden doğrudan resmi Steam Leaderboards API'sine bağlanabilecek modüler altyapı.

### B. Oyuncu Adı Giriş Paneli & Akıllı Geçiş ([CreateNicknameUISetup.cs](file:///C:/Users/engin/OneDrive/Belgeler/Kisisel%20Dosyalarim/ozel/Oyun%20Gelistirme/Denemelik%20projelerim/Dink/Assets/_Project/Scripts/Editor/CreateNicknameUISetup.cs), [SceneTransition.cs](file:///C:/Users/engin/OneDrive/Belgeler/Kisisel%20Dosyalarim/ozel/Oyun%20Gelistirme/Denemelik%20projelerim/Dink/Assets/_Project/Scripts/Menu%20Kodlar/SceneTransition.cs))
- **Zamanı Dondurma (`Time.timeScale = 0f`):** 3D kapıdan **BAŞLA** denildiği an zaman anında dondurularak arka plan sesleri ve hareketleri kesilir.
- **Bir Kereye Mahsus İsim Sorma:** İsim 1 kez girildikten sonra oyun tamamlanana kadar tekrar sorulmaz. Taze başlangıçta tekrar aktif olur.
- **7 Dilli Otomatik Kurulum Aracı:** Unity üst menüsünden `Dink Tools -> Oyuncu Adi Giris Paneli Kur` seçeneğiyle paneli 7 dilde otomatik oluşturma.

### C. GitHub Reposu Senkronizasyonu
- Yapılan tüm geliştirmeler, 7 dilli ses ve arayüz dosyaları `origin/main` (`https://github.com/yagizpolat/Dink.git`) deposuna commit edilip pushlandı.

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
3. **✅ ADIM 3: 7 Dilli İsim Giriş Paneli & LootLocker Küresel Skor Tablosu (TAMAMLANDI)**
4. **📌 ADIM 4: Mor Işık (UV) ve Gizli Yazı / İpuçları (Bulmaca Mekaniği - SIRADAKİ ADIM)**
   - `FenerKontrol.cs`: Farenin sağ tıkı (`Input.GetMouseButtonDown(1)`) ile mor ışık modu (`#9D00FF`). Fener pil tüketim hızı değiştirilmeyecektir!
   - `GizliYazi.cs`: Duvarlarda ve mektuplarda sadece mor UV ışığı altında parlayan gizli semboller ve ipuçları.
   - `CreateHiddenTextSetup.cs`: `Dink Tools -> Mor Isik Gizli Yazi Olustur` Editor aracı.
5. **📌 ADIM 5: Hakan'ın Akıl Sağlığı (Sanity) & İmleç Titremesi & Anlık Gerilim Dynamics**
   - Akıl sağlığı düştükçe imlecin sallanması, bulanıklaşma ve anlık korku ögeleri.
6. **📌 ADIM 6: Mektup Sayımına Göre Çift Sonlu Hikaye Finali**
