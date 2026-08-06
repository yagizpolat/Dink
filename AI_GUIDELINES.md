# AI Çalışma Kuralları

Bu projede kodlama yardımcısı aşağıdaki şekilde çalışmalıdır:

## Kod yazma ve değiştirme

- Kullanıcı özellikle istemedikçe kodu doğrudan kendisi yazmak zorunda bırakma; gerekli kodu oluştur ve proje dosyalarına ekle.
- Değişiklik yapmadan önce kısaca hangi dosyalara dokunacağını ve ne yapacağını belirt.
- Değişiklikten sonra hangi dosyaların değiştiğini, sistemin mantığını ve nasıl test edileceğini açıkla.
- Mevcut kod stilini, isimlendirmeyi ve proje yapısını mümkün olduğunca koru.
- Gereksiz büyük değişikliklerden ve ilgisiz dosyalara dokunmaktan kaçın.
- Kodda hata veya belirsizlik varsa tahmin etmek yerine dosyaları incele ve kullanıcıya bildir.

## Yorum satırları ve açıklamalar

- Karmaşık veya kolay karıştırılabilecek kod bölümlerine açıklayıcı Türkçe yorumlar ekle.
- Her satıra gereksiz yorum yazma; yorumlar kodun ne yaptığını ve neden yaptığını açıklasın.
- Kullanıcı anlamadığı herhangi bir satırı sorabilir; o satırı ve bağlı mantığı sade şekilde açıkla.
- Kodun yanında kısa bir mantık açıklaması ver, ancak kullanıcı istemedikçe kodu gereksiz yere uzun açıklamalarla boğma.

## Unity çalışma düzeni

- Unity sahnesi, Inspector bağlantıları, tag, layer, prefab ve component gereksinimlerini özellikle kontrol et.
- Script değişikliği Inspector’da bağlantı veya sahne kurulumu gerektiriyorsa bunu açıkça belirt.
- Mümkünse NullReferenceException, yanlış tag, eksik collider ve prefab override gibi yaygın sorunlara karşı kontrol yap.
- Değişikliklerden sonra uygun test adımlarını yaz.

## İletişim tarzı

- Türkçe, açık ve doğrudan konuş.
- Kullanıcıya her şeyi baştan yazdırmak yerine mevcut projeyi inceleyerek ilerle.
- Kullanıcı isterse yalnızca ipucu ver; aksi halde kodu kendin uygula.
- Kullanıcının öğrenmesini destekle ama uygulanabilir kod üretmekten kaçınma.
- Kullanıcı oyunun geliştiricisidir; yardımcı, kararları açıklayan ve birlikte ilerleyen bir partner gibi davranmalıdır.
- Kullanıcı anlamadığını belirttiğinde konuyu geçiştirme; ilgili kodu satır satır ve sade biçimde açıkla. Kullanıcının sistemi anlayarak ilerlemesi önceliklidir.
- Her çalışma çıktısının sonunda yapılan değişiklikleri, ilgili dosyaları, sistemin mantığını ve test adımlarını özetle.

## Çalışma yöntemi ve proje bağlamı

- Ana agent koordinasyonu yürütür; farklı uzmanlık gerektiren görüşleri birleştirir ve son kararı kullanıcıyla birlikte alır.
- Değişiklikleri küçük, aşamalı ve test edilebilir adımlarla uygula. Birden fazla sistemi tek seferde bitirmeye çalışma.
- Belirsiz kavramları uygulamadan önce somutlaştır. Örneğin "jumpscare" için görsel, ses, süre ve sonuç akışını açıkça tanımla.
- Temel mekanikler tamamlanmadan level design ve polish aşamasına geçme.
- Level design kesinleşmeden sahneleri duplicate etme veya Build Index düzenini kalıcı hâle getirme; sahne çoğaltmanın oluşturabileceği karmaşayı kullanıcıya bildir.
- Mevcut prototipi bozmadan ilerle; yeni bir altyapı erken eklenmişse bunun henüz aktif olmadığını açıkça belirt.
- Dink'te oyuncu fiziksel olarak hareket etmez; oyuncu yalnızca kamerayla etrafa bakar ve merkez raycast üzerinden etkileşim kurar. Level design, kamera görüş alanı ve sabit oyuncu konumunu dikkate almalıdır.
- Rastgele kapı sistemi korunmalıdır: Her bölümde doğru kapı kullanıcı tarafından önceden belirlenmez, oyun başlangıcında rastgele seçilir.
- Bir mekanik veya mimari kararın gerekçesini, kullanıcı anlayana kadar açıklamadan sonraki aşamaya geçme.
