# Kat koridoru baskı dokuları (PIL; Blender Python'unda PIL yok). Dünya yazıları İngilizce (Steam; arayüz ayrı, 10 dilli): pirinç daire numaraları, zil isimlikleri,
# asansör çağırma paneli ve kat göstergesi, kat levhası, yönetim duyurusu, paspaslar, yangın dolabı etiketi,
# pencereden gece manzarası. Kullanım izi: parmakla parlamış pirinç, sararmış kâğıt, ezilmiş paspas lifi.
# Çalıştır: python koridor_baski.py
import os, random, math
from PIL import Image, ImageChops, ImageDraw, ImageFilter, ImageFont

DOKU = r"C:\Kisisel Dosyalarim\ozel\Oyun Gelistirme\Denemelik projelerim\Dink\Kaynak\Blender\Dokular\Koridor"
F = r"C:\Windows\Fonts" + "\\"
random.seed(11)


def font(ad, s):
    return ImageFont.truetype(F + ad, s)


def merkez(d, kutu, yazi, f, renk):
    x0, y0, x1, y1 = kutu
    bb = d.textbbox((0, 0), yazi, font=f)
    d.text(((x0 + x1 - (bb[2] - bb[0])) / 2 - bb[0], (y0 + y1 - (bb[3] - bb[1])) / 2 - bb[1]), yazi, font=f, fill=renk)


def gren(boyut, guc, renk=None):
    n = Image.effect_noise(boyut, guc).convert("L")
    return n if renk is None else Image.merge("RGB", [n] * 3)


def carp(im, k):
    """Gri katmanla çarpar (128 = değişmez)."""
    return ImageChops.multiply(im, Image.merge("RGB", [k.point(lambda v: min(255, v + 127))] * 3))


def pirinc_zemin(boyut, firca_yatay=True):
    """Fırçalı pirinç: yatay çizgiler + kenarlar oksitli koyu, orta parmakla parlamış."""
    w, h = boyut
    im = Image.new("RGB", boyut, (178, 132, 62))
    cizgi = Image.effect_noise((w, max(1, h // 24)) if firca_yatay else (max(1, w // 24), h), 60).convert("L").resize(boyut, Image.BICUBIC)
    im = carp(im, cizgi.point(lambda v: (v - 128) // 3 + 128))
    oks = Image.new("L", boyut, 0); od = ImageDraw.Draw(oks)
    for k in range(14):
        od.rectangle([k, k, w - 1 - k, h - 1 - k], outline=110 - k * 7)
    oks = oks.filter(ImageFilter.GaussianBlur(3))
    return Image.composite(Image.new("RGB", boyut, (82, 60, 30)), im, oks)


# ── Daire numaraları (1536x256, 6 hücre): oval pirinç plaka, kazınmış siyah rakam ──
im = Image.new("RGB", (1536, 256), (0, 0, 0))
for i in range(6):
    p = pirinc_zemin((256, 256))
    d = ImageDraw.Draw(p)
    merkez(d, (0, 10, 256, 246), str(i + 1), font("georgiab.ttf", 190), (24, 18, 10))
    d.ellipse([10, 10, 246, 246], outline=(90, 66, 32), width=6)
    im.paste(p, (i * 256, 0))
im.save(os.path.join(DOKU, "daire_no.png"))

# ── Zil isimlikleri (1536x256, 6 hücre): beyaz plastik çerçevede kâğıt kart; el yazısı ya da daktilo ──
isimler = [("MILLER", "consolab.ttf"), ("Novak", "segoepr.ttf"), ("R. ADLER", "arialbd.ttf"), ("", None),
           ("HARTMANN", "consolab.ttf"), ("P. Lee", "segoepr.ttf")]
im = Image.new("RGB", (1536, 256), (230, 228, 220))
for i, (ad, f) in enumerate(isimler):
    x0 = i * 256
    d = ImageDraw.Draw(im)
    d.rectangle([x0, 0, x0 + 255, 255], fill=(232, 230, 222))
    kag = (244, 238, 220) if ad else (236, 226, 196)  # boş isimlik: eski kart, sararmış
    d.rectangle([x0 + 22, 70, x0 + 234, 186], fill=kag)
    if ad:
        merkez(d, (x0 + 22, 70, x0 + 234, 186), ad, font(f, 46 if f != "segoepr.ttf" else 50), (30, 32, 70) if f == "segoepr.ttf" else (20, 20, 20))
    else:  # eski adın üstü karalanmış, bant izi
        d.text((x0 + 40, 104), "STONE", font=font("arialbd.ttf", 40), fill=(60, 60, 60))
        for k in range(9):
            d.line([x0 + 34 + random.randint(-4, 4), 108 + k * 4, x0 + 168 + random.randint(-6, 6), 104 + k * 5], fill=(25, 25, 60), width=3)
    d.rectangle([x0 + 22, 70, x0 + 234, 186], outline=(200, 198, 190), width=3)
im = carp(im, gren((1536, 256), 14).point(lambda v: (v - 128) // 4 + 128))
im.filter(ImageFilter.GaussianBlur(0.6)).save(os.path.join(DOKU, "isimlik.png"))

# ── Asansör çağırma paneli (256x512): fırçalı çelik, tek buton halkası, kabartma ok, Braille ──
w, h = 256, 512
im = Image.new("RGB", (w, h), (160, 162, 164))
cizgi = Image.effect_noise((w, h // 30), 70).convert("L").resize((w, h), Image.BICUBIC)
im = carp(im, cizgi.point(lambda v: (v - 128) // 3 + 128))
d = ImageDraw.Draw(im)
for (x, y) in [(20, 20), (236, 20), (20, 492), (236, 492)]:  # vidalar
    d.ellipse([x - 8, y - 8, x + 8, y + 8], fill=(120, 120, 122)); d.line([x - 6, y, x + 6, y], fill=(70, 70, 72), width=2)
d.ellipse([68, 180, 188, 300], fill=(100, 100, 104))         # buton yuvası
d.ellipse([80, 192, 176, 288], fill=(205, 205, 200))          # buton (sık basıldığı için parlak)
d.polygon([(128, 212), (152, 252), (104, 252)], fill=(70, 70, 70))
for k in range(3):
    d.ellipse([112 + k * 14, 312, 120 + k * 14, 320], fill=(110, 110, 112))
d.rounded_rectangle([60, 360, 196, 420], 6, fill=(30, 30, 32))
merkez(d, (60, 360, 196, 420), "CALL", font("arialbd.ttf", 28), (200, 200, 196))
im.filter(ImageFilter.GaussianBlur(0.5)).save(os.path.join(DOKU, "asansor_panel.png"))

# ── Kat göstergesi (256x128): siyah cam arkasında kırmızı segment "1", söndük segmentler ──
im = Image.new("RGB", (256, 128), (10, 8, 8)); d = ImageDraw.Draw(im)
fs = font("consolab.ttf", 104)
merkez(d, (64, 0, 192, 128), "8", fs, (36, 10, 10))   # söndük segmentler, rakamla aynı hücrede
bb8 = d.textbbox((0, 0), "8", font=fs); bb1 = d.textbbox((0, 0), "1", font=fs)
# "1" sağ dikey segmentler gibi 8'in sağ kenarına hizalanır (gerçek 7 segment göstergede olduğu gibi)
x8 = (64 + 192 - (bb8[2] - bb8[0])) / 2 - bb8[0]
d.text((x8 + bb8[2] - bb1[2], (128 - (bb8[3] - bb8[1])) / 2 - bb8[1]), "1", font=fs, fill=(255, 40, 30))
d.polygon([(30, 64), (54, 40), (54, 88)], fill=(36, 10, 10))      # sönük aşağı/yukarı oku
d.polygon([(226, 64), (202, 40), (202, 88)], fill=(36, 10, 10))
im.filter(ImageFilter.GaussianBlur(1.2)).save(os.path.join(DOKU, "kat_gosterge.png"))

# ── Kat levhası (512x256): mermer taklidi plastik üstünde "FLOOR 1" ──
im = Image.new("RGB", (512, 256), (232, 228, 216))
dam = Image.effect_noise((64, 32), 80).convert("L").resize((512, 256), Image.BICUBIC).filter(ImageFilter.GaussianBlur(6))
im = carp(im, dam.point(lambda v: (v - 128) // 6 + 128))
d = ImageDraw.Draw(im)
merkez(d, (0, 20, 512, 236), "FLOOR 1", font("georgiab.ttf", 104), (40, 34, 28))
d.rectangle([8, 8, 503, 247], outline=(150, 140, 120), width=4)
im.save(os.path.join(DOKU, "kat_levha.png"))

# ── Yönetim duyurusu (512x724, A4 oranı): sararmış kâğıt, daktilo metni, imza, bant ──
w, h = 512, 724
im = Image.new("RGB", (w, h), (238, 232, 214))
lif = Image.effect_noise((w // 2, h // 2), 30).convert("L").resize((w, h), Image.BICUBIC)
im = carp(im, lif.point(lambda v: (v - 128) // 5 + 128))
# Kenarlara doğru sararma
sar = Image.new("L", (w, h), 0); sd = ImageDraw.Draw(sar)
for k in range(30):
    sd.rectangle([k, k, w - 1 - k, h - 1 - k], outline=120 - k * 4)
im = Image.composite(Image.new("RGB", (w, h), (214, 196, 150)), im, sar.filter(ImageFilter.GaussianBlur(8)))
d = ImageDraw.Draw(im)
fb, fm = font("courbd.ttf", 30), font("cour.ttf", 19)
merkez(d, (0, 60, w, 110), "KARANFIL APARTMENTS", fb, (30, 30, 30))
merkez(d, (0, 108, w, 150), "NOTICE TO RESIDENTS", font("courbd.ttf", 24), (30, 30, 30))
satirlar = ["Dear residents,", "", "Maintenance fees are due by", "the 15th of each month.",
            "", "Hallway lights are on a timer.", "Please do not hold the switch.", "", "Due to elevator maintenance,",
            "the elevator will be out of", "service Tuesday 10:00-14:00.", "", "Please keep noise to a minimum", "after 11 PM."]
y = 190
for s in satirlar:
    d.text((50, y), s, font=fm, fill=(36, 36, 40)); y += 26
d.text((280, y + 20), "Management", font=font("segoepr.ttf", 30), fill=(30, 40, 110))
im = im.filter(ImageFilter.GaussianBlur(0.5))
d = ImageDraw.Draw(im)
for (x, yy, a) in [(60, 6, -8), (w - 150, 10, 6)]:  # bant parçaları (yarı saydam sarımsı)
    bant = Image.new("RGBA", (100, 34), (240, 230, 190, 150))
    bant = bant.rotate(a, expand=True)
    im.paste(bant, (x, yy), bant)
im.save(os.path.join(DOKU, "duyuru.png"))

# ── Paspaslar (512x320, 2 çeşit üst üste: 512x640): hindistan cevizi lifi, çerçeve deseni ──
im = Image.new("RGB", (512, 640))
for k, (zemin, desen) in enumerate([((150, 104, 56), (60, 40, 22)), ((86, 64, 44), (150, 40, 34))]):
    p = Image.new("RGB", (512, 320), zemin)
    lif = Image.effect_noise((512, 320), 90).convert("L")
    lif2 = Image.effect_noise((128, 320), 70).convert("L").resize((512, 320), Image.BICUBIC)
    p = carp(p, ImageChops.add(lif.point(lambda v: (v - 128) // 2 + 128), lif2.point(lambda v: (v - 128) // 3), 1, 0))
    d = ImageDraw.Draw(p)
    d.rectangle([22, 22, 489, 297], outline=desen, width=16)
    if k == 0:
        merkez(d, (40, 40, 472, 280), "WELCOME", font("arialbd.ttf", 76), desen)
    else:
        for x in range(70, 460, 60):
            d.polygon([(x, 120), (x + 24, 160), (x, 200), (x - 24, 160)], outline=desen, width=8)
    # Kapı önünde ezilmiş, ortası koyulaşmış (ayak izi trafiği)
    ez = Image.new("L", (512, 320), 0); ImageDraw.Draw(ez).ellipse([110, 70, 400, 260], fill=90)
    p = Image.composite(Image.new("RGB", (512, 320), tuple(int(v * 0.75) for v in zemin)), p, ez.filter(ImageFilter.GaussianBlur(40)))
    im.paste(p, (0, k * 320))
im.save(os.path.join(DOKU, "paspas.png"))

# ── Yangın dolabı etiketi (512x256) ──
im = Image.new("RGB", (512, 256), (196, 24, 20)); d = ImageDraw.Draw(im)
merkez(d, (0, 20, 512, 150), "FIRE HOSE", font("arialbd.ttf", 72), (255, 255, 255))
merkez(d, (0, 150, 512, 230), "IN CASE OF FIRE BREAK GLASS", font("arial.ttf", 30), (255, 235, 230))
im = carp(im, gren((512, 256), 10).point(lambda v: (v - 128) // 5 + 128))
im.save(os.path.join(DOKU, "yangin_etiket.png"))

# ── Pencereden gece manzarası (1024x768): karşı apartman cephesi, birkaç yanan pencere, sokak lambası sızıntısı ──
w, h = 1024, 768
im = Image.new("RGB", (w, h)); d = ImageDraw.Draw(im)
for y in range(h):  # gökyüzü (üst %18) şehir ışığıyla turuncu-mor
    k = y / h
    d.line([0, y, w, y], fill=(int(30 + 30 * k), int(24 + 10 * k), int(40 - 10 * k)))
cephe = (34, 32, 34)
d.rectangle([0, 140, w, h], fill=cephe)
siva = Image.effect_noise((w // 4, (h - 140) // 4), 40).convert("L").resize((w, h - 140), Image.BICUBIC)
im.paste(carp(im.crop((0, 140, w, h)), siva.point(lambda v: (v - 128) // 4 + 128)), (0, 140))
d = ImageDraw.Draw(im)
for r in range(4):
    for c in range(6):
        x0, y0 = 40 + c * 168, 180 + r * 150
        yanik = random.random() < 0.22
        renk = random.choice([(250, 190, 110), (200, 220, 255), (255, 160, 80)]) if yanik else (14, 14, 18)
        d.rectangle([x0, y0, x0 + 90, y0 + 110], fill=renk)
        if yanik:  # perde: yarı kapalı, kıvrımlı
            for k in range(0, 90, 8):
                d.line([x0 + k, y0, x0 + k + 2, y0 + 110], fill=tuple(int(v * 0.8) for v in renk), width=3)
        d.rectangle([x0 - 4, y0 + 110, x0 + 94, y0 + 118], fill=(60, 58, 56))  # denizlik
im = im.filter(ImageFilter.GaussianBlur(2.2))  # pencere camı ardında, odak dışı
isik = Image.new("L", (w, h), 0); ImageDraw.Draw(isik).ellipse([600, 420, 1100, 900], fill=120)
im = Image.composite(Image.new("RGB", (w, h), (120, 80, 40)), im, isik.filter(ImageFilter.GaussianBlur(120)))
im.save(os.path.join(DOKU, "pencere_gece.png"))
print("koridor baskıları hazır")
