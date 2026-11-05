# Hafiza sinifi S2: Hakan'in sirasindaki yarim sinav kagidi (dunya yazisi Ingilizce).
# Basili sorular duz font, cevaplar el yazisi; 3. cevap cumlenin ortasinda kesilir, 4. bos.
# Calistir: python sinif_sinav_kagidi.py
import os, random
from PIL import Image, ImageDraw, ImageFilter, ImageFont

random.seed(1411)
ROOT = r"C:\Kisisel Dosyalarim\ozel\Oyun Gelistirme\Denemelik projelerim\Dink"
OUT = os.path.join(ROOT, "Assets", "_Project", "Textures", "HafizaMarket", "HM_SinavKagidi_Renk.png")
F = r"C:\Windows\Fonts" + "\\"
W, H = 1240, 1754                                   # A4 oraninda
im = Image.new("RGB", (W, H), (236, 232, 220))
d = ImageDraw.Draw(im)
# kagit lifi + hafif sararma
for _ in range(9000):
    x, y = random.randrange(W), random.randrange(H)
    c = random.randint(-10, 6); p = im.getpixel((x, y))
    d.point((x, y), fill=tuple(max(0, min(255, v + c)) for v in p))
basli = ImageFont.truetype(F + "arialbd.ttf", 44)
duz = ImageFont.truetype(F + "arial.ttf", 30)
el = ImageFont.truetype(F + "segoepr.ttf", 34)
mur = (30, 34, 80)

d.text((90, 90), "PHYSICS  -  1st TERM EXAM", font=basli, fill=(25, 25, 25))
d.text((90, 160), "Class: 11-C        Date: 14.11.2023", font=duz, fill=(40, 40, 40))
d.text((90, 210), "Name:", font=duz, fill=(40, 40, 40))
d.text((190, 196), "Hakan Kaya", font=el, fill=mur)
d.line((90, 270, W - 90, 270), fill=(60, 60, 60), width=3)

sorular = [
    ("1. A car accelerates from rest at 2 m/s\u00b2. What is its speed after 5 s?", ["v = a \u00b7 t = 2 \u00b7 5 = 10 m/s"]),
    ("2. Define inertia in your own words.", ["Things keep doing what they were", "already doing unless something pushes them."]),
    ("3. Why does a heavier object not fall faster in a vacuum?", ["Because gravity pulls every"]),
    ("4. A force of 20 N moves a box 3 m. How much work is done?", []),
]
y = 320
for soru, cevap in sorular:
    d.text((90, y), soru, font=duz, fill=(30, 30, 30)); y += 60
    for k in range(4):                              # cevap cizgileri; yazi cizginin uzerine oturur
        d.line((130, y + 46, W - 130, y + 46), fill=(170, 170, 165), width=2)
        if k < len(cevap):
            d.text((132 + random.randint(-4, 4), y - 4), cevap[k], font=el, fill=mur)
            if soru.startswith("3."):               # kalem kayar gibi biten kisa iz
                bb = d.textbbox((132, y - 4), cevap[k], font=el)
                d.line((bb[2] + 6, y + 30, bb[2] + 46, y + 44), fill=mur, width=3)
        y += 52
    y += 50
# 3. cevabin kalemi kayar gibi biten kisa cizgi
d.text((W - 330, H - 120), "Score:  ___ / 100", font=duz, fill=(60, 60, 60))
im = im.filter(ImageFilter.GaussianBlur(0.6))
im.save(OUT)
print("tamam", OUT)
