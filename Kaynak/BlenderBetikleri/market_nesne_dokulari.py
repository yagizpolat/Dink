# Market nesnelerine özel baskı/arayüz dokuları: yazar kasa (tuş takımı, LCD, gövde, çekmece), fiş yazıcısı,
# kapı etiketleri, içecek dolabı başlığı, armatür difüzörü. Kullanım izi: parmakla parlamış tuşlar, solmuş baskı.
# Çalıştır: python market_nesne_dokulari.py  (Blender Python'unda PIL yok)
import os, random
from PIL import Image, ImageChops, ImageDraw, ImageFilter, ImageFont

DOKU = r"C:\Kisisel Dosyalarim\ozel\Oyun Gelistirme\Denemelik projelerim\Dink\Kaynak\Blender\Dokular\Market"
F = r"C:\Windows\Fonts" + "\\"
random.seed(23)


def font(ad, s):
    return ImageFont.truetype(F + ad, s)


def plastik(boyut, renk, gren=10):
    """Mat plastik zemin: ince kum dokusu ve çok hafif ton dalgası."""
    w, h = boyut
    im = Image.new("RGB", boyut, renk)
    n = Image.effect_noise(boyut, gren).convert("L")
    dalga = Image.effect_noise((max(1, w // 32), max(1, h // 32)), 30).convert("L").resize(boyut, Image.BICUBIC)
    k = ImageChops.add(n.point(lambda v: (v - 128) // 3 + 128), dalga.point(lambda v: (v - 128) // 4), 1, 0)
    return ImageChops.multiply(im, Image.merge("RGB", [k.point(lambda v: min(255, v + 100))] * 3))


def merkez(d, kutu, yazi, f, renk):
    x0, y0, x1, y1 = kutu
    bb = d.textbbox((0, 0), yazi, font=f)
    d.text(((x0 + x1 - (bb[2] - bb[0])) / 2 - bb[0], (y0 + y1 - (bb[3] - bb[1])) / 2 - bb[1]), yazi, font=f, fill=renk)


# ── Yazar kasa tuş takımı (512x512, kasiyerin gördüğü eğik üst yüz) ──
im = plastik((512, 512), (52, 54, 56)); d = ImageDraw.Draw(im)
pur = Image.new("L", (512, 512), 150); pd = ImageDraw.Draw(pur)  # pürüzlülük: plastik 0.59
tuslar = [["7", "8", "9", "CLR"], ["4", "5", "6", "X"], ["1", "2", "3", "VOID"], ["0", "00", ".", "SUB"], ["DEPT1", "DEPT2", "DEPT3", "TOTAL"]]
renk_ozel = {"CLR": (190, 50, 45), "VOID": (190, 120, 40), "TOTAL": (60, 140, 70), "SUB": (60, 90, 150)}
for r, sira in enumerate(tuslar):
    for c, t in enumerate(sira):
        x0, y0 = 36 + c * 112, 30 + r * 94
        x1, y1 = x0 + 96, y0 + 80
        tr = renk_ozel.get(t, (205, 205, 198))
        d.rounded_rectangle([x0 + 3, y0 + 5, x1 + 3, y1 + 5], 10, fill=(25, 25, 26))  # gölge
        d.rounded_rectangle([x0, y0, x1, y1], 10, fill=tr)
        # Sık basılan tuşun ortası parlamış ve kirlenmiş: hafif koyu oval, pürüzlülük düşük
        sik = t in ("0", "1", "5", "TOTAL", "SUB", "CLR", "00") or random.random() < 0.3
        if sik:
            ov = Image.new("L", (512, 512), 0); ImageDraw.Draw(ov).ellipse([x0 + 22, y0 + 18, x1 - 22, y1 - 16], fill=70)
            ov = ov.filter(ImageFilter.GaussianBlur(8))
            im = Image.composite(Image.new("RGB", (512, 512), tuple(int(v * 0.8) for v in tr)), im, ov)
            d = ImageDraw.Draw(im)
            pd.ellipse([x0 + 20, y0 + 16, x1 - 20, y1 - 14], fill=70)
        else:
            pd.rounded_rectangle([x0, y0, x1, y1], 10, fill=110)
        f = font("arialbd.ttf", 34 if len(t) <= 2 else 20)
        # Çok basılan tuşta yazı solmuş
        yazi_renk = (30, 30, 30) if t not in renk_ozel else (245, 245, 240)
        if sik and t.isdigit():
            yazi_renk = (85, 85, 82)
        merkez(d, (x0, y0, x1, y1), t, f, yazi_renk)
im.filter(ImageFilter.GaussianBlur(0.5)).save(os.path.join(DOKU, "yk_klavye.png"))
pur.filter(ImageFilter.GaussianBlur(2)).save(os.path.join(DOKU, "yk_klavye_puruz.png"))

# ── LCD ekran (512x256): koyu yeşil zemin, söndük segment gölgeleri, yazı ──
im = Image.new("RGB", (512, 256), (12, 28, 20)); d = ImageDraw.Draw(im)
fs = font("consolab.ttf", 64); fk = font("consola.ttf", 26)
d.text((40, 40), "88888888", font=fs, fill=(20, 42, 30))   # söndük segmentler hep biraz görünür
d.text((40, 40), "   23.42", font=fs, fill=(120, 255, 170))
d.text((40, 140), "SUBTOTAL", font=fk, fill=(90, 210, 140))
d.text((40, 180), "THANK YOU - COME AGAIN", font=fk, fill=(70, 170, 110))
for y in range(0, 256, 3):  # LCD satır çizgileri
    d.line([0, y, 512, y], fill=(0, 0, 0), width=1)
im = im.filter(ImageFilter.GaussianBlur(0.8))
cerceve = Image.new("RGB", (512, 256), (30, 30, 32)); maske = Image.new("L", (512, 256), 0)
ImageDraw.Draw(maske).rounded_rectangle([14, 14, 498, 242], 12, fill=255)
Image.composite(im, cerceve, maske).save(os.path.join(DOKU, "yk_ekran.png"))

# ── Yazar kasa gövde ön yüzü / çekmece (512x256) ──
im = plastik((512, 256), (58, 60, 62)); d = ImageDraw.Draw(im)
d.rectangle([0, 120, 512, 124], fill=(30, 30, 32))           # çekmece ayrım çizgisi
d.ellipse([440, 160, 470, 190], fill=(150, 150, 145)); d.rectangle([453, 166, 457, 184], fill=(40, 40, 40))  # anahtar
d.rounded_rectangle([30, 30, 230, 80], 6, fill=(200, 200, 195)); merkez(d, (30, 30, 230, 80), "CASHMATE 220", font("arialbd.ttf", 26), (40, 40, 40))
d.text((30, 150), "CASH DRAWER", font=font("arial.ttf", 18), fill=(150, 150, 146))
for _ in range(25):  # tutma yerinde çizikler
    x = random.randint(150, 360); y = random.randint(200, 250)
    d.line([x, y, x + random.randint(10, 40), y + random.randint(-3, 3)], fill=(90, 92, 94), width=1)
im.save(os.path.join(DOKU, "yk_govde.png"))

# ── Fiş yazıcısı üst yüzü (512x512) ──
im = plastik((512, 512), (40, 41, 43)); d = ImageDraw.Draw(im)
d.rectangle([40, 70, 472, 92], fill=(10, 10, 10))                     # kâğıt çıkış ağzı
d.rectangle([60, 74, 452, 88], fill=(236, 234, 226))                   # dışarı sarkan fiş ucu
d.line([30, 200, 482, 200], fill=(22, 22, 23), width=3)               # kapak ek yeri
d.text((40, 260), "THERMAL 80", font=font("arialbd.ttf", 40), fill=(165, 165, 160))
d.ellipse([380, 380, 440, 440], fill=(70, 72, 74)); merkez(d, (380, 380, 440, 440), "FEED", font("arial.ttf", 14), (200, 200, 195))
d.ellipse([400, 340, 414, 354], fill=(60, 230, 90))                    # yeşil LED
im.save(os.path.join(DOKU, "fis_yazici.png"))

# ── Kapı etiketleri (512x512): PULL (içeriden), çalışma saatleri, kart etiketi ──
im = Image.new("RGBA", (512, 512), (0, 0, 0, 0)); d = ImageDraw.Draw(im)
d.rounded_rectangle([20, 20, 240, 110], 8, fill=(250, 250, 246)); merkez(d, (20, 20, 240, 110), "PULL", font("arialbd.ttf", 56), (190, 30, 30))
d.rounded_rectangle([20, 140, 492, 380], 10, fill=(250, 250, 246))
merkez(d, (20, 150, 492, 210), "OPENING HOURS", font("arialbd.ttf", 34), (30, 30, 30))
merkez(d, (20, 215, 492, 275), "MON - SUN", font("arial.ttf", 30), (60, 60, 60))
merkez(d, (20, 280, 492, 360), "07:00 - 23:00", font("arialbd.ttf", 52), (30, 30, 30))
d.rounded_rectangle([20, 410, 300, 490], 8, fill=(30, 60, 140)); merkez(d, (20, 410, 300, 490), "CARDS ACCEPTED", font("arialbd.ttf", 22), (255, 255, 255))
# Güneşte solma: etiketler hafif sararmış
renk = im.convert("RGB"); renk = ImageChops.multiply(renk, Image.new("RGB", (512, 512), (255, 246, 225)))
im = Image.merge("RGBA", (*renk.split(), im.split()[3]))
im.save(os.path.join(DOKU, "kapi_etiketleri.png"))

# ── İçecek dolabı başlık paneli (1024x256, ışıklı) ──
im = Image.new("RGB", (1024, 256), (20, 70, 140)); d = ImageDraw.Draw(im)
for y in range(256):
    k = y / 255
    d.line([0, y, 1024, y], fill=(int(30 + 40 * k), int(110 + 60 * k), int(190 + 50 * k)))
merkez(d, (0, 20, 1024, 170), "ICE COLD DRINKS", font("impact.ttf", 120), (250, 252, 255))
merkez(d, (0, 175, 1024, 240), "keep door closed", font("arial.ttf", 34), (210, 230, 250))
im.filter(ImageFilter.GaussianBlur(0.6)).save(os.path.join(DOKU, "dolap_baslik.png"))

# ── Armatür prizmatik difüzörü (256x256, döşenir) ──
im = Image.new("L", (256, 256), 230); d = ImageDraw.Draw(im)
for x in range(0, 256, 8):
    for y in range(0, 256, 8):
        d.polygon([(x, y), (x + 8, y), (x + 4, y + 4)], fill=250)
        d.polygon([(x, y + 8), (x + 8, y + 8), (x + 4, y + 4)], fill=205)
im.filter(ImageFilter.GaussianBlur(0.6)).save(os.path.join(DOKU, "difuzor.png"))
print("dokular hazır")
