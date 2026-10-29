# Market ürün etiketleri: 4x4 atlas (her hücre 512 px) + aynı düzende pürüzlülük atlası + raf fiyat etiketleri.
# İngilizce, uydurma markalar (gerçek marka kopyalanmaz). Baskı gerçekliği: karton lifi, kenar aşınması,
# kat izi, mürekkep kayması, mürekkep/karton parlaklık farkı.
# Çalıştır: python market_etiket_atlas.py
import math, os, random
from PIL import Image, ImageChops, ImageDraw, ImageFilter, ImageFont

DOKU = r"C:\Kisisel Dosyalarim\ozel\Oyun Gelistirme\Denemelik projelerim\Dink\Kaynak\Blender\Dokular\Market"
OUT = os.path.join(DOKU, "urun_etiket_atlas.png")
OUT_R = os.path.join(DOKU, "urun_etiket_puruz.png")
F = r"C:\Windows\Fonts" + "\\"
C = 512
random.seed(1142)


def font(ad, size):
    return ImageFont.truetype(F + ad, size)


# (marka, ürün, alt yazı, zemin, vurgu, yazı, yerleşim, marka fontu)
URUNLER = [
    ("GOLDEN FIELD", "Fusilli", "Durum wheat pasta · 500 g", (236, 192, 52), (28, 64, 132), (24, 36, 84), "bant", "georgiab.ttf"),
    ("GOLDEN FIELD", "Spaghetti", "No. 5 · 500 g", (28, 64, 132), (236, 192, 52), (250, 244, 228), "bant", "georgiab.ttf"),
    ("HOLLOWMERE", "Black Tea", "Loose leaf · 1 kg", (128, 22, 28), (212, 168, 70), (246, 230, 198), "pencere", "georgiab.ttf"),
    ("BRIGHTWELL", "Butter Biscuits", "Family pack · 150 g", (242, 228, 196), (176, 62, 40), (96, 42, 20), "capraz", "trebucbd.ttf"),
    ("CLEARSPRING", "Spring Water", "Natural · 1.5 L", (206, 232, 246), (36, 116, 188), (18, 66, 128), "sade", "segoeuib.ttf"),
    ("RED VALLEY", "Tomato Paste", "Double concentrate · 830 g", (176, 28, 24), (58, 136, 58), (250, 240, 222), "pencere", "impact.ttf"),
    ("RED VALLEY", "Chickpeas", "In water · 800 g", (226, 202, 140), (58, 118, 58), (60, 48, 28), "pencere", "impact.ttf"),
    ("SUNCREST", "Sunflower Oil", "Refined · 1 L", (250, 208, 38), (38, 108, 48), (58, 48, 10), "sade", "segoeuib.ttf"),
    ("BLOOM", "Dish Soap", "Lemon · 750 ml", (78, 168, 68), (250, 228, 58), (250, 250, 238), "capraz", "impact.ttf"),
    ("COTTON CLOUD", "Laundry Powder", "40 washes · 4 kg", (38, 88, 168), (228, 58, 58), (250, 250, 255), "capraz", "trebucbd.ttf"),
    ("STONEMILL", "Long Grain Rice", "Premium · 1 kg", (244, 238, 222), (158, 118, 48), (70, 50, 20), "bant", "georgiab.ttf"),
    ("STONEMILL", "Red Lentils", "Split · 1 kg", (228, 128, 58), (244, 238, 222), (60, 30, 10), "bant", "georgiab.ttf"),
    ("WHITEHALL", "Sugar Cubes", "Pure cane · 1 kg", (250, 250, 248), (198, 38, 58), (158, 20, 38), "sade", "georgiab.ttf"),
    ("NIGHT OWL", "Ground Coffee", "Dark roast · 100 g", (68, 38, 24), (208, 158, 88), (240, 220, 188), "pencere", "trebucbd.ttf"),
    ("FIZZWELL", "Lemon Soda", "Sparkling · 330 ml", (28, 138, 88), (240, 240, 240), (255, 255, 255), "capraz", "impact.ttf"),
    ("CRUNCHY CO.", "Potato Chips", "Sea salt · 110 g", (240, 188, 28), (198, 38, 28), (118, 18, 8), "pencere", "impact.ttf"),
]


def merkez(d, y, text, f, fill, x0=0, w=C):
    tw = d.textlength(text, font=f)
    d.text((x0 + (w - tw) / 2, y), text, font=f, fill=fill)


def sigdir(d, text, ad, boy, genislik):
    """Yazıyı verilen genişliğe sığacak en büyük boyutta döndürür."""
    while boy > 14 and d.textlength(text, font=font(ad, boy)) > genislik:
        boy -= 2
    return font(ad, boy)


def koyu(c, k):
    return tuple(max(0, min(255, int(v * k))) for v in c)


def barkod(d, x, y):
    d.rectangle([x - 6, y - 6, x + 130, y + 62], fill=(250, 250, 246))
    xx = x
    while xx < x + 124:
        w = random.choice([1, 1, 2, 3])
        d.rectangle([xx, y, xx + w - 1, y + 44], fill=(14, 14, 14))
        xx += w + random.choice([1, 2, 2, 3])
    d.text((x, y + 46), "0 " + "".join(str(random.randint(0, 9)) for _ in range(11)), font=font("arial.ttf", 12), fill=(14, 14, 14))


def zemin(bg, vurgu, desen):
    """Düz renk yerine basılı ambalaj zemini: dikey gradyan + ince desen (çizgi, nokta yarım ton, dalga)."""
    im = Image.new("RGB", (C, C), bg); d = ImageDraw.Draw(im)
    for y in range(C):
        k = 1.08 - 0.2 * (y / C)
        d.line([0, y, C, y], fill=koyu(bg, k))
    desen_renk = koyu(bg, 0.9) if sum(bg) > 300 else koyu(bg, 1.25)
    if desen == 0:      # ince çapraz çizgiler
        for x in range(-C, C, 14):
            d.line([x, 0, x + C, C], fill=desen_renk, width=2)
    elif desen == 1:    # yarım ton noktalar, aşağı doğru büyür
        for y in range(8, C, 16):
            for x in range(8 + (y // 16 % 2) * 8, C, 16):
                r = 1 + 3 * y / C
                d.ellipse([x - r, y - r, x + r, y + r], fill=desen_renk)
    else:               # yumuşak dalgalar
        for y0 in range(0, C + 40, 26):
            pts = [(x, y0 + 7 * math.sin(x / 30 + y0)) for x in range(0, C + 8, 8)]
            d.line(pts, fill=desen_renk, width=3)
    return im


def _golgeli(katman):
    """Çizim katmanına yumuşak gölge ekler (basılı fotoğraf/illüstrasyon derinliği)."""
    a = katman.split()[3]
    golge = Image.new("RGBA", katman.size, (0, 0, 0, 0))
    golge.putalpha(a.point(lambda v: v * 0.45).filter(ImageFilter.GaussianBlur(5)))
    alt = Image.new("RGBA", katman.size, (0, 0, 0, 0))
    alt.paste(golge, (4, 6), golge)
    return Image.alpha_composite(alt, katman)


def _top(d, x, y, r, renk, isik=1.35):
    """Gölgeli küre/tane: koyu kenar, açık merkez, parlama noktası."""
    d.ellipse([x - r, y - r, x + r, y + r], fill=koyu(renk, 0.7) + (255,))
    d.ellipse([x - r * 0.85, y - r * 0.9, x + r * 0.75, y + r * 0.7], fill=renk + (255,))
    d.ellipse([x - r * 0.5, y - r * 0.6, x - r * 0.1, y - r * 0.25], fill=koyu(renk, isik) + (255,))


def cizim(i, w, h):
    """Ürünün kendisini gösteren illüstrasyon (RGBA, w x h)."""
    k = Image.new("RGBA", (w, h), (0, 0, 0, 0)); d = ImageDraw.Draw(k)
    R = random.Random(100 + i)
    def yer():
        return R.randint(int(w * 0.12), int(w * 0.88)), R.randint(int(h * 0.15), int(h * 0.85))
    if i == 0:      # fusilli: burgulu makarna
        for _ in range(9):
            x, y = yer(); a = R.uniform(-0.6, 0.6)
            for s in range(6):
                cx, cy = x + math.cos(a) * (s - 2.5) * 12, y + math.sin(a) * (s - 2.5) * 12
                d.ellipse([cx - 9, cy - 13, cx + 9, cy + 13], fill=(232, 186, 70, 255), outline=(176, 128, 36, 255), width=2)
    elif i == 1:    # spagetti demeti
        for s in range(40):
            y = h * 0.3 + s * h * 0.012
            d.line([w * 0.05, y + R.uniform(-3, 3), w * 0.95, y + R.uniform(-8, 8)], fill=(238, 200, 96, 255) if s % 3 else (200, 156, 60, 255), width=3)
        d.rectangle([w * 0.45, h * 0.26, w * 0.55, h * 0.86], fill=(200, 40, 40, 255))
    elif i == 2:    # çay yaprakları
        for _ in range(16):
            x, y = yer(); a = R.uniform(0, 180)
            yap = Image.new("RGBA", (60, 30), (0, 0, 0, 0)); yd = ImageDraw.Draw(yap)
            yd.ellipse([0, 0, 59, 29], fill=(R.randint(40, 70), R.randint(90, 130), 40, 255)); yd.line([4, 15, 56, 15], fill=(30, 70, 25, 255), width=2)
            yap = yap.rotate(a, expand=True); k.paste(yap, (x - yap.width // 2, y - yap.height // 2), yap)
    elif i == 3:    # tereyağlı bisküvi
        for _ in range(4):
            x, y = yer(); r = R.randint(38, 52)
            _top(d, x, y, r, (222, 168, 92), 1.15)
            for _ in range(7):
                a, rr = R.uniform(0, 6.28), R.uniform(0, r * 0.6)
                px, py = x + math.cos(a) * rr, y + math.sin(a) * rr
                d.ellipse([px - 3, py - 3, px + 3, py + 3], fill=(150, 98, 40, 255))
    elif i == 4:    # su damlaları ve dağ
        d.polygon([(0, h), (w * 0.35, h * 0.25), (w * 0.6, h * 0.6), (w * 0.8, h * 0.35), (w, h)], fill=(90, 150, 200, 255))
        d.polygon([(w * 0.28, h * 0.42), (w * 0.35, h * 0.25), (w * 0.42, h * 0.42)], fill=(250, 250, 255, 255))
        for _ in range(6):
            x, y = yer(); r = R.randint(8, 16)
            d.ellipse([x - r, y - r, x + r, y + r], fill=(220, 240, 255, 220)); d.polygon([(x - r * 0.8, y - r * 0.5), (x, y - r * 2.4), (x + r * 0.8, y - r * 0.5)], fill=(220, 240, 255, 220))
    elif i in (5, 6, 10, 11, 13):   # domates, nohut, pirinç, mercimek, kahve çekirdeği
        ayar = {5: ((206, 40, 32), 34, 4), 6: ((226, 196, 128), 15, 26), 10: ((248, 246, 236), 6, 90), 11: ((222, 110, 46), 7, 80), 13: ((96, 54, 30), 13, 34)}[i]
        renk, r, n = ayar
        for _ in range(n):
            x, y = yer()
            if i == 10:
                d.ellipse([x - r * 2, y - r * 0.7, x + r * 2, y + r * 0.7], fill=renk + (255,), outline=(200, 196, 180, 255))
            elif i == 13:
                d.ellipse([x - r, y - r * 1.35, x + r, y + r * 1.35], fill=renk + (255,)); d.arc([x - r * 0.3, y - r * 1.2, x + r * 0.3, y + r * 1.2], 100, 260, fill=(40, 20, 10, 255), width=2)
            else:
                _top(d, x, y, r, koyu(renk, R.uniform(0.9, 1.05)))
                if i == 5:
                    d.polygon([(x - 10, y - r), (x, y - r - 9), (x + 10, y - r), (x, y - r + 6)], fill=(50, 130, 50, 255))
    elif i == 7:    # ayçiçeği
        x, y = w // 2, h // 2
        for s in range(16):
            a = s * math.pi / 8
            px, py = x + math.cos(a) * 46, y + math.sin(a) * 46
            d.ellipse([px - 22, py - 12, px + 22, py + 12], fill=(250, 200, 30, 255), outline=(210, 150, 20, 255))
        _top(d, x, y, 36, (110, 70, 30), 1.2)
    elif i in (8, 14):  # limon dilimi ve köpük
        x, y = int(w * 0.4), h // 2
        d.ellipse([x - 60, y - 60, x + 60, y + 60], fill=(250, 220, 40, 255)); d.ellipse([x - 52, y - 52, x + 52, y + 52], fill=(255, 246, 170, 255))
        for s in range(8):
            a = s * math.pi / 4
            d.pieslice([x - 46, y - 46, x + 46, y + 46], math.degrees(a) + 4, math.degrees(a) + 41, fill=(252, 232, 100, 255))
        for _ in range(10):
            bx, by = yer(); r = R.randint(6, 18)
            d.ellipse([bx - r, by - r, bx + r, by + r], outline=(255, 255, 255, 230), width=3); d.ellipse([bx - r * 0.5, by - r * 0.6, bx - r * 0.1, by - r * 0.2], fill=(255, 255, 255, 230))
    elif i == 9:    # köpük ve gömlek
        d.polygon([(w * 0.3, h * 0.2), (w * 0.42, h * 0.12), (w * 0.58, h * 0.12), (w * 0.7, h * 0.2), (w * 0.66, h * 0.36), (w * 0.62, h * 0.32), (w * 0.62, h * 0.9), (w * 0.38, h * 0.9), (w * 0.38, h * 0.32), (w * 0.34, h * 0.36)], fill=(250, 250, 255, 255), outline=(170, 190, 220, 255))
        for _ in range(14):
            bx, by = yer(); r = R.randint(6, 20)
            d.ellipse([bx - r, by - r, bx + r, by + r], fill=(200, 225, 255, 160), outline=(255, 255, 255, 230), width=2)
    elif i == 12:   # küp şeker
        for s in range(5):
            x, y = int(w * (0.22 + s * 0.14)), int(h * (0.6 - (s % 2) * 0.2))
            z = 26
            d.polygon([(x, y), (x + z, y - z * 0.5), (x + 2 * z, y), (x + z, y + z * 0.5)], fill=(255, 255, 255, 255))
            d.polygon([(x, y), (x + z, y + z * 0.5), (x + z, y + z * 1.5), (x, y + z)], fill=(222, 222, 226, 255))
            d.polygon([(x + z, y + z * 0.5), (x + 2 * z, y), (x + 2 * z, y + z), (x + z, y + z * 1.5)], fill=(200, 200, 206, 255))
    elif i == 15:   # dalgalı cips
        for _ in range(7):
            x, y = yer(); a = R.uniform(0, 180)
            c = Image.new("RGBA", (90, 64), (0, 0, 0, 0)); cd = ImageDraw.Draw(c)
            cd.ellipse([0, 0, 89, 63], fill=(236, 184, 64, 255), outline=(190, 130, 30, 255), width=3)
            for s in range(4):
                cd.arc([10, 6 + s * 12, 80, 30 + s * 12], 200, 340, fill=(210, 150, 40, 255), width=2)
            c = c.rotate(a, expand=True); k.paste(c, (x - c.width // 2, y - c.height // 2), c)
    return _golgeli(k.filter(ImageFilter.GaussianBlur(0.6)))


def ince_baski(d, yazi, i, yer):
    """Küçük baskı: net ağırlık rozeti, geri dönüşüm işareti, okunmayacak kadar küçük içerik satırları."""
    f = font("arial.ttf", 11)
    for s in range(4 if yer != "capraz" else 0):  # çaprazda alt yazının üstüne binmesin
        d.line([20, C - 70 + s * 9, 20 + random.randint(120, 210), C - 70 + s * 9], fill=koyu(yazi, 1.0), width=2)
    d.ellipse([C - 230, C - 92, C - 186, C - 48], outline=yazi, width=3)
    d.polygon([(C - 208, C - 84), (C - 196, C - 62), (C - 220, C - 62)], outline=yazi)
    if i % 3 == 0 and yer in ("bant", "capraz"):  # "NEW" yıldız rozeti; diğer yerleşimlerde markaya biner
        x, y = C - 70, 150
        pts = [(x + math.cos(a * math.pi / 9) * (44 if a % 2 else 32), y + math.sin(a * math.pi / 9) * (44 if a % 2 else 32)) for a in range(18)]
        d.polygon(pts, fill=(220, 30, 40)); d.text((x - 22, y - 10), "NEW", font=font("arialbd.ttf", 18), fill=(255, 255, 255))


def tasarim(marka, urun, alt, bg, vurgu, yazi, yer, mfont, i=0):
    im = zemin(bg, vurgu, i % 3)
    d = ImageDraw.Draw(im)
    # Ürün çizimi: yerleşime göre boş alana
    alan = {"bant": (0.12, 0.73, 0.82, 0.97), "pencere": (0.18, 0.46, 0.82, 0.86), "capraz": (0.36, 0.56, 0.98, 0.84), "sade": (0.22, 0.3, 0.78, 0.58)}[yer]
    if yer == "bant":
        d.rectangle([0, int(C * 0.56), C, int(C * 0.72)], fill=vurgu)
        d.ellipse([-C * 0.3, C * 0.68, C * 1.3, C * 1.25], fill=koyu(bg, 0.86))
        f = sigdir(d, marka, mfont, 84, C * 0.62); merkez(d, 64, marka, f, yazi)
        d.line([C * 0.25, 172, C * 0.75, 172], fill=vurgu, width=5)
        f = sigdir(d, urun, "arialbd.ttf", 52, C * 0.62); merkez(d, 198, urun, f, yazi)
        f = sigdir(d, alt, "arialbd.ttf", 32, C * 0.62); merkez(d, int(C * 0.6), alt, f, bg if sum(vurgu) < 360 else (30, 30, 30))
    elif yer == "pencere":
        # Ürünün "fotoğrafı" gibi koyu oval pencere: içinde yumuşak gradyan ve tanecikler
        ov = Image.new("L", (C, C), 0); od = ImageDraw.Draw(ov)
        od.ellipse([C * 0.16, C * 0.44, C * 0.84, C * 0.88], fill=255)
        # Odak dışı ürün fotoğrafı: koyu zemin üstünde birkaç büyük, çok bulanık ışık lekesi
        foto = Image.new("RGB", (C, C), koyu(vurgu, 0.42)); fd = ImageDraw.Draw(foto)
        for k in range(7):
            r = random.randint(50, 95); x = random.randint(int(C * 0.25), int(C * 0.75)); y = random.randint(int(C * 0.48), int(C * 0.78))
            fd.ellipse([x - r, y - r * 0.8, x + r, y + r * 0.8], fill=koyu(bg if k % 2 else vurgu, random.uniform(0.75, 1.05)))
        foto = foto.filter(ImageFilter.GaussianBlur(22))
        isik = Image.new("L", (C, C), 0); ImageDraw.Draw(isik).ellipse([C * 0.3, C * 0.42, C * 0.62, C * 0.6], fill=90)
        foto = Image.composite(Image.new("RGB", (C, C), (255, 250, 235)), foto, isik.filter(ImageFilter.GaussianBlur(30)))
        im.paste(foto, (0, 0), ov.filter(ImageFilter.GaussianBlur(1)))
        d = ImageDraw.Draw(im)
        d.ellipse([C * 0.16, C * 0.44, C * 0.84, C * 0.88], outline=vurgu, width=8)
        f = sigdir(d, marka, mfont, 80, C * 0.62); merkez(d, 40, marka, f, yazi)
        f = sigdir(d, urun, "arialbd.ttf", 50, C * 0.62); merkez(d, 132, urun, f, yazi)
        f = sigdir(d, alt, "arial.ttf", 28, C * 0.62); merkez(d, 196, alt, f, yazi)
    elif yer == "capraz":
        d.polygon([(0, C * 0.42), (C, C * 0.22), (C, C * 0.5), (0, C * 0.7)], fill=vurgu)
        d.polygon([(0, C * 0.72), (C, C * 0.52), (C, C * 0.56), (0, C * 0.76)], fill=koyu(vurgu, 0.7))
        f = sigdir(d, marka, mfont, 92, C * 0.62); merkez(d, 36, marka, f, yazi)
        f = sigdir(d, urun, "arialbd.ttf", 54, C * 0.62)
        katman = Image.new("RGBA", (C, 90), (0, 0, 0, 0)); kd = ImageDraw.Draw(katman)
        tw = kd.textlength(urun, font=f); kd.text(((C - tw) / 2, 10), urun, font=f, fill=bg)
        katman = katman.rotate(math.degrees(math.atan2(C * 0.2, C)), resample=Image.BICUBIC, expand=False)
        im.paste(katman, (0, int(C * 0.36)), katman)
        d = ImageDraw.Draw(im)
        f = sigdir(d, alt, "arialbd.ttf", 30, C - 200); d.text((24, int(C * 0.84)), alt, font=f, fill=yazi)
    else:  # sade: geniş boşluk, büyük ince marka, küçük ürün adı
        d.rectangle([C * 0.08, C * 0.08, C * 0.92, C * 0.92], outline=vurgu, width=4)
        f = sigdir(d, marka, mfont, 70, C * 0.62); merkez(d, int(C * 0.22), marka, f, vurgu)
        f = sigdir(d, urun, "arialbd.ttf", 46, C * 0.62); merkez(d, int(C * 0.6), urun, f, yazi)
        f = sigdir(d, alt, "arial.ttf", 28, C * 0.62); merkez(d, int(C * 0.71), alt, f, yazi)
    x0, y0, x1, y1 = (int(v * C) for v in alan)
    ciz = cizim(i, x1 - x0, y1 - y0)
    if yer == "pencere":  # çizim oval pencerenin dışına taşmasın
        m = Image.new("L", (C, C), 0); ImageDraw.Draw(m).ellipse([C * 0.17, C * 0.45, C * 0.83, C * 0.87], fill=255)
        tam = Image.new("RGBA", (C, C), (0, 0, 0, 0)); tam.paste(ciz, (x0, y0), ciz)
        tam.putalpha(ImageChops.multiply(tam.split()[3], m))
        im.paste(tam, (0, 0), tam)
        d = ImageDraw.Draw(im); d.ellipse([C * 0.16, C * 0.44, C * 0.84, C * 0.88], outline=vurgu, width=8)
    else:
        im.paste(ciz, (x0, y0), ciz)
    d = ImageDraw.Draw(im)
    ince_baski(d, yazi, i, yer)
    barkod(d, C - 168, C - 92)
    return im


def baski(im, bg):
    """Düz çizimi basılmış ambalaja çevirir: karton lifi, mürekkep kayması, kat izi, kenar aşınması.
    Döndürür: (renk, pürüzlülük)"""
    # Mürekkep kayması: koyu kanal 1 px kayık, hafif hale
    r, g, b = im.split()
    im = Image.merge("RGB", (ImageChops.offset(r, 1, 0), g, ImageChops.offset(b, 0, 1)))
    im = im.filter(ImageFilter.GaussianBlur(0.55))
    # Karton lifi: iki ölçekli gürültü, yatay uzatılmış
    lif = Image.effect_noise((C // 4, C), 40).convert("L").resize((C, C), Image.BICUBIC).filter(ImageFilter.GaussianBlur(0.8))
    ince = Image.effect_noise((C, C), 30).convert("L")
    karisim = ImageChops.add(lif.point(lambda v: (v - 128) // 3 + 128), ince.point(lambda v: (v - 128) // 5), 1, 0)
    im = ImageChops.multiply(im, Image.merge("RGB", [karisim.point(lambda v: 225 + (v - 128) // 4)] * 3).point(lambda v: min(255, v + 30)))
    # Işık düşüşü: üst/alt kenarlar çok hafif koyu (kutunun kıvrılan kenarı)
    vin = Image.new("L", (C, C), 255); vd = ImageDraw.Draw(vin)
    for k in range(18):
        a = 255 - (18 - k) * 2
        vd.rectangle([k, k, C - 1 - k, C - 1 - k], outline=a)
    im = ImageChops.multiply(im, Image.merge("RGB", [vin] * 3))
    # Kat izi: rastgele bir yatay/dikey ince açık çizgi (kartonun kırıldığı yer)
    d = ImageDraw.Draw(im)
    puruz = Image.new("L", (C, C), 120)  # mürekkepli yüzey yarı parlak (0.47)
    pd = ImageDraw.Draw(puruz)
    for _ in range(random.randint(1, 2)):
        if random.random() < 0.5:
            y = random.randint(C // 5, C * 4 // 5)
            d.line([0, y, C, y + random.randint(-6, 6)], fill=koyu(bg, 1.12), width=2)
            pd.line([0, y, C, y], fill=200, width=4)
        else:
            x = random.randint(C // 5, C * 4 // 5)
            d.line([x, 0, x + random.randint(-6, 6), C], fill=koyu(bg, 1.12), width=2)
            pd.line([x, 0, x, C], fill=200, width=4)
    # Kenar aşınması: kenarda düzensiz açık lekeler (mürekkep kalkmış, karton görünür)
    asinma = Image.new("L", (C, C), 0); ad = ImageDraw.Draw(asinma)
    for _ in range(45):
        kenar = random.choice("ustr")
        t = random.randint(0, C)
        r_ = random.randint(1, 4)
        x, y = {"u": (t, random.randint(0, 6)), "s": (t, C - random.randint(0, 6)), "t": (random.randint(0, 6), t), "r": (C - random.randint(0, 6), t)}[kenar]
        ad.ellipse([x - r_ * 3, y - r_, x + r_ * 3, y + r_], fill=random.randint(60, 150))
    asinma = asinma.filter(ImageFilter.GaussianBlur(1.5))
    karton = Image.new("RGB", (C, C), (196, 176, 140))
    im = Image.composite(karton, im, asinma)
    puruz = ImageChops.lighter(puruz, asinma.point(lambda v: v * 0.9))
    puruz = ImageChops.add(puruz, ince.point(lambda v: (v - 128) // 6 + 128), 1, -128)
    return im, puruz


atlas = Image.new("RGB", (C * 4, C * 4))
patlas = Image.new("L", (C * 4, C * 4))
for i, u in enumerate(URUNLER):
    renk, puruz = baski(tasarim(*u, i=i), u[3])
    atlas.paste(renk, ((i % 4) * C, (i // 4) * C))
    patlas.paste(puruz, ((i % 4) * C, (i // 4) * C))
atlas.save(OUT)
patlas.save(OUT_R)
print("kaydedildi", OUT)

# Raf fiyat etiketleri: 8 kart yan yana (1024x256)
ETK = os.path.join(DOKU, "fiyat_etiketleri.png")
im = Image.new("RGB", (1024, 256), (240, 240, 232)); d = ImageDraw.Draw(im)
fb = font("arialbd.ttf", 46)
fs = font("arial.ttf", 18)
for i in range(8):
    x0 = i * 128
    sale = i % 3 == 0
    d.rectangle([x0, 0, x0 + 127, 255], fill=(250, 220, 40) if sale else (245, 245, 238))
    d.rectangle([x0, 0, x0 + 127, 34], fill=(200, 30, 30) if sale else (30, 60, 140))
    d.text((x0 + 8, 6), "SALE" if sale else "PRICE", font=fs, fill=(255, 255, 255))
    fiyat = f"{random.randint(0, 9)}.{random.choice(['99', '49', '29', '79'])}"
    d.text((x0 + 8, 64), fiyat, font=fb, fill=(20, 20, 20))
    d.text((x0 + 8, 150), "each", font=fs, fill=(20, 20, 20))
    d.text((x0 + 8, 205), f"SKU {random.randint(10000, 99999)}", font=fs, fill=(90, 90, 90))
im = im.filter(ImageFilter.GaussianBlur(0.4))
im.save(ETK)
print("kaydedildi", ETK)
