# GEMINI / ANTIGRAVITY PROJECT CONTEXT & MEMORY: DINK

## Proje Kimliği & Hedef
- **Proje:** Dink (PC Psikolojik Korku / Labirent Kapı Seçimi Oyunu)
- **Motor:** Unity 6 (6000.0.14f1) LTS, URP
- **Geliştirici Rolü:** Kullanıcı baş geliştiricidir. Asistan Senior Danışman & Mimar rolündedir. Kodları kullanıcı yazar/düzenler.
- **Dil:** İletişim 100% Türkçe; Kodlar, değişkenler, yorumlar 100% İngilizce.

## Güncel Durum (14 Ağustos 2026)
- **Aktif C# Dosya Sayısı:** **38 Adet** (30 gereksiz/ölü script temizlendi).
- **Kaldırılan Sistemler:** Fener/Pil (`FenerKontrol`), Envanter (`InventoryManager`), 18 Editor `[MenuItem]` toolları, `LoreTriggerSystem`, `ZindanBoruMinigame`, `DoorEffects`.
- **Aktif Mimari:**
  - `temaskontrol.cs`: Raycast tıklama (Kapi -> `Door.Interact()`, İpucu -> `HintPropComponent.Interact()`).
  - `Door.cs` & `DoorSequenceManager.cs`: Doğru kapıda sonraki sahneye geçiş, yanlış kapıda `JumpscareController` & Zindan.
  - `DynamicHintSystem.cs` & `DoorChoice.cs`: Yazı-tura ile Sol/Sağ doğru kapı seçimi, Fake hint algoritması, 3D prop spawn ve mektup metnine yön yerleştirme.
  - `ZindanKurtulmaManager.cs`: 2 kurtulma hakkı, DontDestroyOnLoad ile yandığı odayı (`kayipSahneIndex`) tutma ve bulmaca bitince odaya geri gönderme. 8 Minigame aktif.
  - `RoomGraphData.cs` & `RoomGraphManager.cs`: ScriptableObject tabanlı doğrusal olmayan oda ağı.
  - `MektupNotHintUI.cs`: 18 hikaye mektubu, 7 dil desteği, timeScale=0 duraklatma.
  - `SettingsManager.cs`: PrimeTween ile pop-up yaylanma açılışı, PunchScale metin animasyonları, 7 dilli ayar metinleri, canlı SFX geri bildirimi.
  - `UIButtonJuice.cs`: Evrensel hover scale & click punch animasyonu.
  - `episode.asset` (URP Volume): 5 ana efekt (ACES Tonemapping, Bloom 1.0 threshold, Vignette 0.35, FilmGrain 0.25, ColorAdjustments Contrast +20 Saturation -15).

## Sonraki Oturum Planı
1. Odaya Masif/Paslı İpucu Masası Prefab'ı ve üstüne loş tepe ışığı eklenmesi.
2. Dinamik Geri Sayım (Countdown) Gerilim sayacı entegrasyonu.
3. ESC duraklatma menüsü polish.
