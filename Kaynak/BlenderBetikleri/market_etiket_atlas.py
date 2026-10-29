# Market ürün etiketleri: 4x4 atlas (her hücre 512 px). Uydurma markalar, gerçek marka kopyalanmaz.
# Çalıştır: python market_etiket_atlas.py  -> Kaynak/Blender/Dokular/Market/urun_etiket_atlas.png
import os, random
from PIL import Image, ImageDraw, ImageFont, ImageFilter

OUT = r"C:\Kisisel Dosyalarim\ozel\Oyun Gelistirme\Denemelik projelerim\Dink\Kaynak\Blender\Dokular\Market\urun_etiket_atlas.png"
FB = r"C:\Windows\Fonts\arialbd.ttf"
FR = r"C:\Windows\Fonts\arial.ttf"
FS = r"C:\Windows\Fonts\georgiab.ttf"
C = 512
random.seed(1142)

# (marka, ürün, alt yazı, arka plan, vurgu, yazı rengi, biçim)
URUNLER = [
    ("EKİN", "Burgu Makarna", "500 g", (232, 196, 60), (30, 70, 140), (30, 40, 90), "kutu"),
    ("EKİN", "Spagetti", "500 g", (30, 70, 140), (232, 196, 60), (250, 245, 230), "kutu"),
    ("KARANFİL", "Çay", "1000 g", (150, 25, 30), (215, 170, 70), (245, 230, 200), "kutu"),
    ("SARAY", "Bisküvi", "Sütlü · 150 g", (240, 225, 190), (170, 60, 40), (90, 40, 20), "kutu"),
    ("DAMLA", "Doğal Kaynak Suyu", "1,5 L", (200, 230, 245), (40, 120, 190), (20, 70, 130), "sise"),
    ("TARLA", "Domates Salçası", "830 g", (180, 30, 25), (60, 140, 60), (250, 240, 220), "konserve"),
    ("TARLA", "Nohut", "800 g", (225, 200, 140), (60, 120, 60), (60, 50, 30), "konserve"),
    ("GÖNÜL", "Ayçiçek Yağı", "1 L", (250, 210, 40), (40, 110, 50), (60, 50, 10), "sise"),
    ("BAHAR", "Bulaşık Deterjanı", "Limon · 750 ml", (80, 170, 70), (250, 230, 60), (250, 250, 240), "sise"),
    ("PAMUK", "Çamaşır Deterjanı", "4 kg", (40, 90, 170), (230, 60, 60), (250, 250, 255), "kutu"),
    ("KÖYÜM", "Pirinç", "Baldo · 1 kg", (245, 240, 225), (160, 120, 50), (70, 50, 20), "kutu"),
    ("KÖYÜM", "Kırmızı Mercimek", "1 kg", (230, 130, 60), (245, 240, 225), (60, 30, 10), "kutu"),
    ("ŞEKER EVİ", "Kesme Şeker", "1 kg", (250, 250, 250), (200, 40, 60), (160, 20, 40), "kutu"),
    ("AKŞAM", "Kahve", "Türk Kahvesi · 100 g", (70, 40, 25), (210, 160, 90), (240, 220, 190), "konserve"),
    ("YILDIZ", "Gazoz", "330 ml", (30, 140, 90), (240, 240, 240), (255, 255, 255), "konserve"),
    ("ÇITIR", "Patates Cipsi", "Klasik · 110 g", (240, 190, 30), (200, 40, 30), (120, 20, 10), "kutu"),
]


def font(path, size):
    return ImageFont.truetype(path, size)


def merkez(d, y, text, f, fill):
    w = d.textlength(text, font=f)
    d.text(((C - w) / 2, y), text, font=f, fill=fill)


def barkod(d, x, y):
    d.rectangle([x - 6, y - 6, x + 130, y + 62], fill=(255, 255, 255))
    xx = x
    while xx < x + 124:
        w = random.choice([1, 1, 2, 3])
        d.rectangle([xx, y, xx + w - 1, y + 44], fill=(10, 10, 10))
        xx += w + random.choice([1, 2, 2, 3])
    d.text((x, y + 46), "8 69" + "".join(str(random.randint(0, 9)) for _ in range(9)), font=font(FR, 12), fill=(10, 10, 10))


def hucre(marka, urun, alt, bg, vurgu, yazi, bicim):
    im = Image.new("RGB", (C, C), bg)
    d = ImageDraw.Draw(im)
    # Baskı dokusu: hafif ton farkı, düz renk görünmesin
    gr = Image.effect_noise((C, C), 18).convert("L").filter(ImageFilter.GaussianBlur(1.2))
    im = Image.composite(im, Image.new("RGB", (C, C), tuple(max(0, c - 22) for c in bg)), gr.point(lambda v: 200 + (v - 128) // 4))
    d = ImageDraw.Draw(im)
    # Vurgu bandı ve dalga
    d.rectangle([0, int(C * 0.55), C, int(C * 0.72)], fill=vurgu)
    d.ellipse([-C * 0.3, C * 0.66, C * 1.3, C * 1.2], fill=tuple(int(c * 0.85) for c in bg))
    # Marka
    fm = font(FS, 92 if len(marka) <= 6 else 70)
    merkez(d, 70, marka, fm, yazi)
    d.line([C * 0.25, 175, C * 0.75, 175], fill=vurgu, width=5)
    # Ürün adı
    fu = font(FB, 48 if len(urun) < 14 else 38)
    merkez(d, 205, urun, fu, yazi)
    # Görsel: basit simge (tane/damla/yaprak)
    cx, cy = 95, int(C * 0.86)
    if bicim == "sise":
        d.ellipse([cx - 28, cy - 40, cx + 28, cy + 30], fill=vurgu)
    else:
        for k in range(5):
            a = k * 72
            ox = int(46 * __import__("math").cos(__import__("math").radians(a)))
            oy = int(18 * __import__("math").sin(__import__("math").radians(a)))
            d.ellipse([cx + ox - 16, cy + oy - 10, cx + ox + 16, cy + oy + 10], fill=vurgu)
    # Alt bilgi
    merkez(d, int(C * 0.585), alt, font(FB, 40), bg if sum(vurgu) < 300 else (30, 30, 30))
    d.text((175, int(C * 0.84)), "Yerli üretim", font=font(FR, 22), fill=yazi)
    barkod(d, C - 170, C - 95)
    # Kenar: baskı kayması gibi ince koyu çizgi
    d.rectangle([0, 0, C - 1, C - 1], outline=tuple(int(c * 0.6) for c in bg), width=3)
    return im


atlas = Image.new("RGB", (C * 4, C * 4))
for i, u in enumerate(URUNLER):
    atlas.paste(hucre(*u), ((i % 4) * C, (i // 4) * C))
atlas.save(OUT)
print("kaydedildi", OUT)

# Raf fiyat etiketleri: 8 kart yan yana (1024x256)
ETK = OUT.replace("urun_etiket_atlas.png", "fiyat_etiketleri.png")
im = Image.new("RGB", (1024, 256), (240, 240, 232)); d = ImageDraw.Draw(im)
fb = ImageFont.truetype(r"C:\Windows\Fonts\arialbd.ttf", 40)
fs = ImageFont.truetype(r"C:\Windows\Fonts\arial.ttf", 18)
for i in range(8):
    x0 = i * 128
    sari = i % 3 == 0
    d.rectangle([x0, 0, x0 + 127, 255], fill=(250, 220, 40) if sari else (245, 245, 238))
    d.rectangle([x0, 0, x0 + 127, 34], fill=(200, 30, 30) if sari else (30, 60, 140))
    d.text((x0 + 8, 6), "İNDİRİM" if sari else "FİYAT", font=fs, fill=(255, 255, 255))
    fiyat = f"{random.randint(9, 189)},{random.choice(['90', '50', '95', '00'])}"
    d.text((x0 + 8, 60), fiyat, font=fb, fill=(20, 20, 20))
    d.text((x0 + 8, 140), "TL", font=fs, fill=(20, 20, 20))
    d.text((x0 + 8, 200), "KDV dahil", font=fs, fill=(90, 90, 90))
im.save(ETK)
print("kaydedildi", ETK)
