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

### B. Turkce Seslendirme & Donanimsal Altyazi Sistemi ([SubtitleManager.cs](file:///C:/Users/engin/OneDrive/Belgeler/Kisisel%20Dosyalarim/ozel/Oyun%20Gelistirme/Denemelik%20projelerim/Dink/Assets/_Project/Scripts/In%20Game/SubtitleManager.cs))
- **Clean Code & Yalin Zamanlama:** Performans emen ve tutarsiz davranabilen donanimsal `while(isPlaying)` döngüleri silindi; `displayDuration` ve `clip.length` baglantili yalin `WaitForSeconds` zamanlama yapisina gecildi.
- **AudioSource Fallback Destegi:** Ses klibi script parametresine atanmadiginda doğrudan `AudioSource.clip` bileseninden ses dosyasini ve uzunlugunu otomatik cekebilen esnek altyapi kuruldu.
- **Kesin Ekranda Kalma Garantisi (Alpha-Only Hiding):** Erken kapanmalari ve parent obje bağımlılıklarını önlemek icin `SetActive(false)` kullanimi silindi; paneller her zaman aktif tutulup sadece `alpha` saydamligiyla gosterilip gizlenmesi saglandi. `playOnAwake = false` korumasi eklendi.
- **Localization Hazirligi:** LocalizedText.cs dili ve dynamic binding bilesenleri ile %100 uyumludur.

### C. Gercek Alpha Decal Donusturucu ve Material Iyilestirmeleri ([CreateTrueAlphaDecal.cs](file:///C:/Users/engin/OneDrive/Belgeler/Kisisel%20Dosyalarim/ozel/Oyun%20Gelistirme/Denemelik%20projelerim/Dink/Assets/_Project/Scripts/Editor/CreateTrueAlphaDecal.cs))
- JPG kaynakli kuf ve siva catlagi kaplamalarindaki kirli beyaz kenar halkalarini (halo) gidermek icin dogrudan RGBA PNG ureten editor araci yazildi (isolated_mold_patch_alpha.png, wall_crack_decal_alpha.png).
- URP Transparent kaplamalardaki plastik parlamayi onlemek icin Smoothness = 0.0 yapildi.

### D. Yordamsal 3D Model Ureticileri ([ProceduralMeshGenerator.cs](file:///C:/Users/engin/OneDrive/Belgeler/Kisisel%20Dosyalarim/ozel/Oyun%20Gelistirme/Denemelik%20projelerim/Dink/Assets/_Project/Scripts/Editor/ProceduralMeshGenerator.cs))
- 3D Ahsap Tablo Cercevesi (Frame_Mesh.asset).
- 3D Kapi Ustu Ikaz Armatur Lambasi (Door_Light_Mesh.asset).
- 3D Pil (Battery_Mesh.asset) ve Mektup Kagidi (Letter_Mesh.asset).

### E. Oyun Baslangici Sessizlik & Menü Müzik Kesintisi ([AudioManager.cs](file:///C:/Users/engin/OneDrive/Belgeler/Kisisel%20Dosyalarim/ozel/Oyun%20Gelistirme/Denemelik%20projelerim/Dink/Assets/_Project/Scripts/Menu%20Kodlar/AudioManager.cs), [SceneTransition.cs](file:///C:/Users/engin/OneDrive/Belgeler/Kisisel%20Dosyalarim/ozel/Oyun%20Gelistirme/Denemelik%20projelerim/Dink/Assets/_Project/Scripts/Menu%20Kodlar/SceneTransition.cs))
- Oyuna gecis butonuna basildiginda arka plan menü muziginin yavasca kısılarak (Fade Out) 2 saniye icinde tamamen susmasi saglandi.
- Oyun ici atmosferik gerilimi artirmak ve psikolojik korku hissiyatini pekitirmek icin oyuna baslandiginda arkada hicbir müzik calmamasi, tam sessizlik saglanmasi kural haline getirildi.

### F. ESC Menüsü Ana Menüye Dönüş & Siyah Ekran Donma Düzeltmesi ([ButonEtkliesim.cs](file:///C:/Users/engin/OneDrive/Belgeler/Kisisel%20Dosyalarim/ozel/Oyun%20Gelistirme/Denemelik%20projelerim/Dink/Assets/_Project/Scripts/In%20Game/ButonEtkliesim.cs), [IntroManager.cs](file:///C:/Users/engin/OneDrive/Belgeler/Kisisel%20Dosyalarim/ozel/Oyun%20Gelistirme/Denemelik%20projelerim/Dink/Assets/_Project/Scripts/Menu%20Kodlar/IntroManager.cs))
- ESC durdurma menüsünden Ana Menüye dönülürken `Time.timeScale = 0f` (zaman durdurulmuş) kaldığı için menüdeki kararma paneli Coroutine'lerinin kilitlenmesi ve rastgele siyah ekranda kalma sorunu tespit edildi.
- Tüm buton etkileşimlerini tek çatı altında toplayan `ButonEtkliesim.cs` script'indeki `sahnedegis(int sahneno)` metoduna `Time.timeScale = 1f` ve imleç kilidi açma koruması yerleştirildi (`Escmenu.cs` temiz tutuldu).
- `IntroManager.cs` içerisindeki kararma döngüsü `Time.unscaledDeltaTime` ile güncellenerek zaman ölçeğinden bağımsız hale getirildi.

### G. Tekli Lamba Isik Kurulumu Editor Araci ([SingleLampLightSetup.cs](file:///C:/Users/engin/OneDrive/Belgeler/Kisisel%20Dosyalarim/ozel/Oyun%20Gelistirme/Denemelik%20projelerim/Dink/Assets/_Project/Scripts/Editor/SingleLampLightSetup.cs))
- `Hanging_Lamp_Mesh` objesine zemin seviyesinden tavana bakan ideal cılız loş ışığı (`Upward_Lamp_Light`), soft shadow ayarlarını ve `Hanging_Lamp_Material` materyalini tek tıkla otomatik bağlayan Editor aracı yazıldı.

### H. Seviye 3 Sis ve Titreyen Isik Kurulumu ([Room3AtmosphereSetup.cs](file:///C:/Users/engin/OneDrive/Belgeler/Kisisel%20Dosyalarim/ozel/Oyun%20Gelistirme/Denemelik%20projelerim/Dink/Assets/_Project/Scripts/Editor/Room3AtmosphereSetup.cs), [FlickerLight.cs](file:///C:/Users/engin/OneDrive/Belgeler/Kisisel%20Dosyalarim/ozel/Oyun%20Gelistirme/Denemelik%20projelerim/Dink/Assets/_Project/Scripts/Menu%20Kodlar/FlickerLight.cs))
- Sahneye havadaki ışıklı sis zerrelerini (`Atmospheric_Fog_Motes`) otomatik kuran ve lambaya tekinsiz cızırtılı/titreyen organik ışık sistemini (`FlickerLight.cs`) bağlayan Editor aracı oluşturuldu.

### I. Yavaşça Sallanan Oda Sistemi / Sinüs Salınımı ([RoomRotator.cs](file:///C:/Users/engin/OneDrive/Belgeler/Kisisel%20Dosyalarim/ozel/Oyun%20Gelistirme/Denemelik%20projelerim/Dink/Assets/_Project/Scripts/In%20Game/RoomRotator.cs))
- Odayı sonsuz dönme yerine belirlenen açı sınırları arasında (örneğin `-25°` ile `+25°` arasında) yumuşak bir sinüs dalgası (Sarkaç/Salınım) ile ileri-geri sallandıran yeni mantık kuruldu. Oda asla kameranın açısından çıkmaz veya ters dönmez.

### J. Demo Sürümü Fenersiz & Envantersiz Oynanış Yapılandırması ([FenerKontrol.cs](file:///C:/Users/engin/OneDrive/Belgeler/Kisisel%20Dosyalarim/ozel/Oyun%20Gelistirme/Denemelik%20projelerim/Dink/Assets/_Project/Scripts/In%20Game/FenerKontrol.cs), [InventoryManager.cs](file:///C:/Users/engin/OneDrive/Belgeler/Kisisel%20Dosyalarim/ozel/Oyun%20Gelistirme/Denemelik%20projelerim/Dink/Assets/_Project/Scripts/In%20Game/InventoryManager.cs), [Escmenu.cs](file:///C:/Users/engin/OneDrive/Belgeler/Kisisel%20Dosyalarim/ozel/Oyun%20Gelistirme/Denemelik%20projelerim/Dink/Assets/_Project/Scripts/In%20Game/Escmenu.cs))
- Kullanıcı talimatı doğrultusunda demo sürümünde Fener (`[F]`) ve Envanter (`[TAB]`) sistemleri geçici olarak tamamen devre dışı bırakıldı.
- `FenerKontrol.cs`'e `demoModuFenersiz = true`, `InventoryManager.cs`'e `demoModuEnvantersiz = true` toggle'ları eklendi.
- Koddaki temel yapılar silinmeden ve pil tüketim katsayısı bozulmadan saf fenersiz/envantersiz oynanış sağlandı.

### K. Demo v0.3 Gerçekçi & Organik Oyun İkonu ([dink_game_icon.jpg](file:///C:/Users/engin/OneDrive/Belgeler/Kisisel%20Dosyalarim/ozel/Oyun%20Gelistirme/Denemelik%20projelerim/Dink/Assets/_Project/Textures/dink_game_icon.jpg))
- Yapay zeka hissini kıran, 35mm film dokusunda, dökülen boyalar, eski ahşap kapılar ve sinematik karanlık atmosfer içeren 1:1 kare çözünürlüklü gerçekçi organik oyun ikonu oluşturuldu.
- Projede `Assets/_Project/Textures/dink_game_icon.jpg` konumuna kaydedildi.

### L. Otomatik Dil ve Metin Yerelleştirme Altyapısı ([LocalizedText.cs](file:///C:/Users/engin/OneDrive/Belgeler/Kisisel%20Dosyalarim/ozel/Oyun%20Gelistirme/Denemelik%20projelerim/Dink/Assets/_Project/Scripts/LocalizedText.cs), [LocalizationHelper.cs](file:///C:/Users/engin/OneDrive/Belgeler/Kisisel%20Dosyalarim/ozel/Oyun%20Gelistirme/Denemelik%20projelerim/Dink/Assets/_Project/Scripts/Editor/LocalizationHelper.cs))
- `LocalizedText.cs` hem TextMeshProUGUI hem Legacy UI Text destekleyecek ve `OnEnable` durumunda kendini otomatik tazeleyecek şekilde güçlendirildi.
- `LocalizationHelper.cs` adında Editor aracı yazılarak tek tıkla sahnedeki tüm metin bileşenlerine `LocalizedText` eklenmesi ve Türkçe metinlerin otomatik doldurulması sağlandı.

### M. Çok Dilli Mektup Okuma Sistemi ([Letter.cs](file:///C:/Users/engin/OneDrive/Belgeler/Kisisel%20Dosyalarim/ozel/Oyun%20Gelistirme/Denemelik%20projelerim/Dink/Assets/_Project/Scripts/In%20Game/Letter.cs), [LetterManager.cs](file:///C:/Users/engin/OneDrive/Belgeler/Kisisel%20Dosyalarim/ozel/Oyun%20Gelistirme/Denemelik%20projelerim/Dink/Assets/_Project/Scripts/In%20Game/LetterManager.cs))
- `Letter.cs` içine Türkçe ve İngilizce başlık (`trTitle`, `enTitle`) ile mektup içeriği (`trContent`, `enContent`) alanları eklendi.
- `LetterManager.cs` mektup okuma ekranını açtığında `GetTitle()` ve `GetContent()` metodlarıyla o an seçili dildeki mektup metnini ekrana dinamik olarak basacak şekilde yapılandırıldı.

### N. Çok Dilli Seslendirme & Altyazı Sistemi ([SubtitleManager.cs](file:///C:/Users/engin/OneDrive/Belgeler/Kisisel%20Dosyalarim/ozel/Oyun%20Gelistirme/Denemelik%20projelerim/Dink/Assets/_Project/Scripts/In%20Game/SubtitleManager.cs), [IntroCinematic.cs](file:///C:/Users/engin/OneDrive/Belgeler/Kisisel%20Dosyalarim/ozel/Oyun%20Gelistirme/Denemelik%20projelerim/Dink/Assets/_Project/Scripts/In%20Game/IntroCinematic.cs))
- `SubtitleManager.cs` içerisine Türkçe ve İngilizce ses klipleri (`trVoiceClip`, `enVoiceClip`) ve altyazı metinleri (`trSubtitleText`, `enSubtitleText`) tanımlandı.
- `ShowIntroSubtitle()` metodu yazılarak oyuncunun seçtiği dile göre hem doğru Türkçe/İngilizce ses klibinin oynatılması hem de doğru dilde altyazı çıkarılması sağlandı.

### O. Standalone Build Hatası Düzeltmesi (CS0234) ([LetterManager.cs](file:///C:/Users/engin/OneDrive/Belgeler/Kisisel%20Dosyalarim/ozel/Oyun%20Gelistirme/Denemelik%20projelerim/Dink/Assets/_Project/Scripts/In%20Game/LetterManager.cs))
- Build sırasında kilitlenmeye ve `CS0234: The type or namespace name 'AppUI' does not exist` hatasına yol açan gereksiz `using Unity.AppUI.UI;` satırı `LetterManager.cs`'ten temizlendi. Build derlemesi engelsiz hale getirildi.

---

## 3. Dikkat Edilmesi Gereken Kurallar & Ayarlar

- **Fener Pil Tuketim Katsayisi:** Kullanici talimati geregi Fener pil tuketim hizi kesinlikle degistirilmemelidir.
- **Dosya Silme Izni:** Kullanicidan izin almadan varsayilan olarak proje dosyasi silinmemelidir.
- **Turkce Anlasilir Anlatim:** Iletisim sade ve teknik karmasadan uzak Turkce ile surdurulmelidir.
- **Master Template Korunmasi:** Sahne 1 (Giris/Tutorial) tamamlanmis olup yeni sahneler bu Master sablon duplicate edilerek turetilecektir.

---

## 4. Siradaki Isler

1. **Sonraki Seviyeler Icin Sahne Cogaltma (Duplicate Level 2/3):**
   - Sablon sahneyi Game 2 / Game 3 olarak kopyalayip isiklari sondurmek ve karanlik oda mekaniklerini (Fener + Envanter acilis uyarisi) test etmek.
2. **Etkilesim Ses Efektleri (SFX Integration):**
   - El feneri acma/kapama, pil toplama, envanter slot secimi ve kapi suzulme/acilma ses efektlerini scriptlere baglamak.
3. **Hikaye & Mektup Icerikleri:**
   - Yerde duran mektup kagidinin (LetterManager.cs) iceriklerini ve sonraki odalara yerlestirilecek ipuclarini zenginlestirmek.

