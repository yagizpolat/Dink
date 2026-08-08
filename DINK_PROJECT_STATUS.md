# Dink - Proje Durumu

## Genel Bakış

Dink, psikolojik korku, puzzle ve first-person türlerini bir araya getiren bir oyun. Oyuncu fiziksel olarak hareket etmiyor; sabit bir noktadan kamerayı çevirerek çevreyi inceliyor ve ekranın merkezindeki crosshair üzerinden objelerle etkileşime giriyor.

Demo hedefi yaklaşık 15 dakikalık oynanış. Tam oyun için hedeflenen süre yaklaşık 2-2,5 saat.

## Tasarım Kararları

- Oyuncu hareket etmiyor, yalnızca kamerayı kontrol ediyor.
- Etkileşimler merkezden gönderilen raycast ile yapılıyor.
- Her bölümde iki kapı bulunuyor.
- Doğru kapı her oyun başlangıcında rastgele belirleniyor; doğru kapı önceden sabit olarak belirlenmiyor.
- Yanlış kapı seçimi jumpscare ve ölümle sonuçlanıyor.
- Harfler dünyada okunuyor, envantere eklenmiyor.
- Demo kapsamında envanterde yalnızca piller bulunuyor.
- Level design kesinleşmeden yeni sahneler çoğaltılmayacak.

## Tamamlanan Sistemler

- Ana menü ve intro akışı
- Kulaklık ve içerik uyarıları
- ESC/Pause menüsü
- Crosshair sistemi
- Dünyadaki mektupları okuma
- El feneri ve pil tüketimi
- Pil toplama
- Envanter ve slot seçimi
- Pil kullanımı ve tam pil uyarısı
- Kapı etkileşimi
- Rastgele doğru kapı seçimi
- Kapı seçimi sonrasında gameplay mekaniklerinin kilitlenmesi
- Doğru kapıda fade ve sonraki sahne geçişi altyapısı
- Yanlış kapıda UI tabanlı jumpscare ve ana menüye dönüş
- Yeni bölüm yokken doğru kapıda siyah ekranda kalmayı önleyen Demo Tamamlandı paneli
- Demo panelinde Ana Menü ve Yeniden Oyna butonları
- Demo paneli için aktif Canvas arama ve Canvas bulunamadığında gameplay'i kilitlememe fallback'i
- Unity Input/EventSystem sorunlarına karşı runtime mouse tıklama fallback'i

## Klasör Yapısı

Proje dosyaları `Assets/_Project` altında tutuluyor:

```text
Assets/_Project
├── Scenes
├── Scripts
│   ├── Editor
│   ├── In Game
│   └── Menu Kodlar
└── Settings
```

Üçüncü parti ve görsel assetler `Assets/gorseller` altında tutuluyor. Unity dosyalarının `.meta` dosyalarıyla birlikte taşınmasına dikkat edilmeli.

## Kapı Sistemi

İlgili scriptler:

- `Assets/_Project/Scripts/In Game/Door.cs`
- `Assets/_Project/Scripts/In Game/DoorChoice.cs`
- `Assets/_Project/Scripts/In Game/DoorSequenceManager.cs`
- `Assets/_Project/Scripts/In Game/JumpscareController.cs`
- `Assets/_Project/Scripts/In Game/LevelProgressionManager.cs`
- `Assets/_Project/Scripts/In Game/temaskontrol.cs`

### Akış

```text
Oyun başlar
→ DoorChoice sol veya sağ kapıyı rastgele doğru seçer
→ Oyuncu merkez raycast ile kapıya bakıp tıklar
→ Door sonucu DoorSequenceManager'a gönderir
→ Gameplay scriptleri kapanır
```

Doğru kapıda geçerli bir sonraki bölüm varsa fade uygulanır ve sonraki sahne yüklenir. Yeni bölüm yoksa fade başlatılmaz; gameplay scriptleri kapanır, runtime oluşturulan Demo Tamamlandı paneli açılır ve cursor serbest bırakılır. Panelden Ana Menü (Build Index 0) veya Yeniden Oyna seçilebilir. Yanlış kapıda JumpscareController UI panelini açar, sesi oynatır, belirtilen süre kadar bekler ve ana menüye döner.

`DoorSequenceManager.cs` demo panelini runtime'da oluşturur. Panel, aktif Canvas altına eklenir; başlık ve butonlar `LegacyRuntime.ttf` kullanır. Normal UI tıklaması çalışmazsa panel, mouse konumunu doğrudan kontrol eden runtime fallback ile butonları çalıştırır.

Kapı seçildikten sonra şu scriptler devre dışı bırakılır:

- `Kamera`
- `FenerKontrol`
- `InventoryManager`
- `Escmenu`
- `temaskontrol`

`DoorSequenceManager` açık kalır; çünkü geçiş, jumpscare ve ilerleme akışını yönetir.

## Mevcut Sahneler

- `Game.unity`: Ana menü
- `Game 1.unity`: Mevcut oynanış prototipi

Build Settings şu anda bu iki sahneyi içeriyor. Yeni bölüm sahneleri level design planı kesinleşmeden oluşturulmamalı.

## LevelProgressionManager Durumu

`LevelProgressionManager` oluşturuldu ancak level design kesinleşmeden aktif bir bölüm listesiyle kullanılmamalı. Amacı, ileride Build Settings içindeki bölüm sahnelerini sırayla yönetmek ve son bölüme gelindiğinde final sinematiğine geçişi hazırlamak.

Örnek bölüm listesi:

```text
[1, 2, 3, 4]
```

Bu sistem şu an final sinematiğini başlatmıyor; son bölüme gelindiğinde yalnızca Console'a bilgi yazıyor.

## Dikkat Edilmesi Gerekenler

- Kapı collider'larında `Kapi` tag'i bulunmalı.
- `Door` component'i raycast'in çarptığı collider üzerinde olmalı.
- `DoorChoice` içindeki sol ve sağ kapı referansları atanmalı.
- `DoorSequenceManager` ve `JumpscareController` referansları Inspector'da kontrol edilmeli.
- Jumpscare paneli başlangıçta pasif olmalı.
- Ana menü Build Index'i `0`.
- Unity sahneleri veya scriptleri taşınırken `.meta` dosyaları korunmalı.
- Ana menüye dönüldüğünde `IntroManager` cursor'ı serbest bırakıyor; bu, intro atlandığında mouse'un kilitli kalmasını önlüyor.
- `AudioManager` sahneler arasında yaşamaya devam ediyor ve ana menüye dönüldüğünde menü müziğini tekrar başlatıyor.
- Demo paneli için `Game 1.unity` sahnesine kalıcı UI/prefab eklenmedi; panel oyun çalışırken oluşturulur.
- Demo paneli doğru kapıda açılır; yanlış kapı jumpscare akışı korunur.
- Fener pil tüketim katsayısı kullanıcı tarafından özellikle onaylanmıştır ve değiştirilmemelidir.
- `DoorSequenceManager.cs` içinde runtime panel, Canvas arama ve mouse fallback kodu bulunur; değişiklik sonrası Play Mode'da doğru kapı/panel/butonlar test edilmelidir.

## Sıradaki İşler

1. Demo panelini Play Mode'da test etmek: doğru kapı, panel, Ana Menü, Yeniden Oyna.
2. Mevcut prototipte kapı ve jumpscare akışını test etmeye devam etmek.
2. Demo için bölüm sayısını ve genel oynanış akışını belirlemek.
3. Sabit kamera konumuna uygun ilk bölüm level design'ını hazırlamak.
4. Mektup, pil, ışık ve çevresel ipuçlarının ilk bölümdeki yerlerini planlamak.
5. Level design netleşince sahneleri kontrollü şekilde çoğaltmak.
6. `LevelProgressionManager` bölüm listesine bağlamak.
7. Son bölüm için final sinematiği ve huzura kavuşma temasını eklemek.
