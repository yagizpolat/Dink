# DINK Oyun Projesi Durum Raporu

## Proje Genel Bakisi

- **Proje Yolu:** C:\Users\engin\OneDrive\Belgeler\Kisisel Dosyalarim\ozel\Oyun Gelistirme\Denemelik projelerim\Dink
- **Unity Surumu:** Unity 6 (6000.0.14f1) LTS, URP (Universal Render Pipeline)
- **Hedef:** Birinci sahis (FPS), sabit kamera acili, atmosferik psikolojik korku ve kapi secimi oyunu.
- **Dil Destegi:** Turkce seslendirme ve altyazi altyapisi (Gelecekteki LocalizedText.cs dil sistemiyle %100 uyumlu).

---

## 1. Ana Mimari & Mekanik Ozet

- **Ana Sablon Oda (Master Template Room):**
  - Iki kapili (Sol Mavi / Sag Kirmizi) temel sahne yapisi. Bu sablon mukemmellestirilerek gelecek bolumler icin kopyalanacaktir (Duplicate).
  - Sol duvarda organik kuf lekesi (Isolated_Mold_TrueAlpha.mat), sag duvarda siva catlagi (Wall_Crack_TrueAlpha.mat) ve havalanan mikro toz zerreleri (Atmospheric_Dust) bulunur.
- **Gelismis Kapi Isiklari & Suzulme (Door Indicators):**
  - Sol kapi uzerinde Mavi, Sag kapi uzerinde Kirmizi 3D armatur lambalari bulunur.
  - Indicator objeleri dogrudan kapi Transform'larina ebeveynlenmistir (SetParent), boylece DoorEffects.cs kapilari havada suzdurdugunde isiklar kapilarla %100 senkronize bicimde hareket eder.
- **Aydinlatma Felsefesi ("Goldilocks Sweet Spot"):**
  - Environment Lighting -> Ambient Color koyu gri/mavi tonuna (RGB 45, 50, 60) ayarlanmistir. Ortam oyuncuyu sikacak kadar aydinlik, oyunu oynanamaz kilacak kadar kor karanlik degildir.
- **Kademeli Mekanik Ogretimi (Progressive Tutorial):**
  - 1. Oda (Tutorial): Mavi ve Kirmizi kapi isiklari ortami aydinlattigi icin Fener (FenerKontrol.cs) ve Envanter (InventoryManager.cs) pasif tutulur. Yerdeki piller SetActive(false) yapilarak ortam temizlenmistir.
  - Ileriki Karanlik Odalar: Isiklar sondugunde ekranda *"Fenerini acmak icin [F], Envanter icin [TAB] tusuna bas"* uyarisi cikacak ve mekanikler acilacaktir.

---

## 2. Son Yapilan Degisiklikler ve Eklenen Sistemler (09.08.2026)

### A. Sinematik Giris Sekansi ve Goz Acilisi ([IntroCinematic.cs](file:///C:/Users/engin/OneDrive/Belgeler/Kisisel%20Dosyalarim/ozel/Oyun%20Gelistirme/Denemelik%20projelerim/Dink/Assets/_Project/Scripts/In%20Game/IntroCinematic.cs))
- **5 Fazli Coroutine Akisi:**
  1. **Faz 0 (Hazirlik):** Oyuncudan kamera/fener kontrolu alinir, ekran siyahla kaplanir, kamera yere bakacak sekilde dondurulur (X: 60°).
  2. **Faz 1 (Goz Acilisi):** Ust ve alt siyah bantlar birbirinden ayrilarak karakterin gozunu acmasi simule edilir. Ayni anda Turkce ses ve altyazi tetiklenir.
  3. **Faz 2 (Basini Kaldirma):** Quaternion.Slerp ile kamera rotasyonu ground acisindan duz bakis acisina (X: 0°) organik olarak yukseltilir (ayaga kalkma hissi).
  4. **Faz 3 (Bulanikliktan Netlige):** URP Depth of Field (Gaussian) kullanilarak hicbir renk mudahalesi olmadan saf seffaf bulaniklik kademeli olarak temizlenir.
  5. **Faz 4 (Kontrol Iadesi):** Oyuncuya kamera ve etkilesim kontrolleri geri verilir.
- **Dinamik Override Destegi:** Profilde DepthOfField override'i yoksa oyun basladigi an otomatik olarak profili doldurur.

### B. Turkce Seslendirme & 1:1 Donanimsal Altyazi Sistemi ([SubtitleManager.cs](file:///C:/Users/engin/OneDrive/Belgeler/Kisisel%20Dosyalarim/ozel/Oyun%20Gelistirme/Denemelik%20projelerim/Dink/Assets/_Project/Scripts/In%20Game/SubtitleManager.cs))
- **Donanimsal Oynatma Takibi (isPlaying):** Matematiksel sure hesaplamalari yerine Unity ses motorunun donanimsal oynatma durumunu (while(!voiceAudioSource.isPlaying) ve while(voiceAudioSource.isPlaying)) takip eder.
- **Kusursuz Senkronizasyon:** Ses hoparlorden ciktigi ILK KAREDE altyazi belirir (0.2s Fade-In). Ses tamamen sustugu ILK KAREDE altyazi yavasca kaybolur (0.4s Fade-Out). Altyazinin sesten once bitmesi veya uyumsuz kalmasi imkansizlastirilmistir.
- **Localization Hazirligi:** LocalizedText.cs dili ve dynamic binding bilesenleri ile %100 uyumludur.

### C. Gercek Alpha Decal Donusturucu ve Material Iyilestirmeleri ([CreateTrueAlphaDecal.cs](file:///C:/Users/engin/OneDrive/Belgeler/Kisisel%20Dosyalarim/ozel/Oyun%20Gelistirme/Denemelik%20projelerim/Dink/Assets/_Project/Scripts/Editor/CreateTrueAlphaDecal.cs))
- JPG kaynakli kuf ve siva catlagi kaplamalarindaki kirli beyaz kenar halkalarini (halo) gidermek icin dogrudan RGBA PNG ureten editor araci yazildi (isolated_mold_patch_alpha.png, wall_crack_decal_alpha.png).
- URP Transparent kaplamalardaki plastik parlamayi onlemek icin Smoothness = 0.0 yapildi.

### D. Yordamsal 3D Model Ureticileri ([ProceduralMeshGenerator.cs](file:///C:/Users/engin/OneDrive/Belgeler/Kisisel%20Dosyalarim/ozel/Oyun%20Gelistirme/Denemelik%20projelerim/Dink/Assets/_Project/Scripts/Editor/ProceduralMeshGenerator.cs))
- 3D Ahsap Tablo Cercevesi (Frame_Mesh.asset).
- 3D Kapi Ustu Ikaz Armatur Lambasi (Door_Light_Mesh.asset).
- 3D Pil (Battery_Mesh.asset) ve Mektup Kagidi (Letter_Mesh.asset).

---

## 3. Dikkat Edilmesi Gereken Kurallar & Ayarlar

- **Fener Pil Tuketim Katsayisi:** Kullanici talimati geregi Fener pil tuketim hizi kesinlikle degistirilmemelidir.
- **Dosya Silme Izni:** Kullanicidan izin almadan varsayilan olarak proje dosyasi silinmemelidir.
- **Turkce Anlasilir Anlatim:** Iletisim sade ve teknik karmasadan uzak Turkce ile surdurulmelidir.
- **Master Template Korunmasi:** Sahne 1 (Giris/Tutorial) tamamlanmis olup yeni sahneler bu Master sablon duplicate edilerek turetilecektir.

---

## 4. Siradaki Isler

1. **Unity Play Testi & Ince Ayarlar:**
   - Sinematik giris sekansi (IntroCinematic) ve altyazi senkronizasyonunun Play modunda son kontrollerini yapmak.
2. **Sonraki Seviyeler Icin Sahne Cogaltma (Duplicate Level 2/3):**
   - Sablon sahneyi Game 2 / Game 3 olarak kopyalayip isiklari sondurmek ve karanlik oda mekaniklerini (Fener + Envanter acilis uyarisi) test etmek.
3. **Hikaye & Mektup Icerikleri:**
   - Yerde duran mektup kagidinin (LetterManager.cs) iceriklerini ve sonraki odalara yerlestirilecek ipuclarini zenginlestirmek.
