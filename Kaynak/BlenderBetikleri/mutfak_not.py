# Hafiza mutfagi M2: masadaki el yazisi not (dunya yazisi Ingilizce).
# Calistir: python mutfak_not.py
import os, random
from PIL import Image, ImageDraw, ImageFilter, ImageFont

random.seed(2342)
ROOT = r"C:\Kisisel Dosyalarim\ozel\Oyun Gelistirme\Denemelik projelerim\Dink"
OUT = os.path.join(ROOT, "Assets", "_Project", "Textures", "HafizaMarket", "HM_MutfakNot_Renk.png")
W, H = 1240, 1240
im = Image.new("RGB", (W, H), (238, 234, 214))
d = ImageDraw.Draw(im)
for _ in range(7000):
    x, y = random.randrange(W), random.randrange(H)
    c = random.randint(-9, 5); p = im.getpixel((x, y))
    d.point((x, y), fill=tuple(max(0, min(255, v + c)) for v in p))
for y in range(150, H - 60, 92):
    d.line((0, y, W, y), fill=(165, 185, 205), width=3)
el = ImageFont.truetype(r"C:\Windows\Fonts\segoepr.ttf", 76)
mur = (24, 28, 70)
y = 150 + 92 * 2 - 92
for i, satir in enumerate(["Brother,", "you're late again.", "", "It's 23:42.", "I'm going to sleep.", "", "      Z."]):
    if satir:
        d.text((95 + random.randint(-8, 8), y - 88), satir, font=el, fill=mur)
    y += 92 * 1.5 if False else 138
im = im.filter(ImageFilter.GaussianBlur(0.5))
im.save(OUT)
print("tamam", OUT)
