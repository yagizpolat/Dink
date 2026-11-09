# Hafiza M6: Daire 11 kapisindaki pirinc numara plakasi (dunya yazisi: yalniz rakam).
# Calistir: python daire11_numara.py
import os
from PIL import Image, ImageDraw, ImageFilter, ImageFont
ROOT = r"C:\Kisisel Dosyalarim\ozel\Oyun Gelistirme\Denemelik projelerim\Dink"
OUT = os.path.join(ROOT, "Assets", "_Project", "Textures", "HafizaMarket", "HM_D11_Numara_Renk.png")
W, H = 256, 160
im = Image.new("RGB", (W, H), (150, 116, 52))
d = ImageDraw.Draw(im)
for y in range(H):                                   # firca izli pirinc
    t = 0.85 + 0.15 * ((y * 37) % 11) / 11
    d.line((0, y, W, y), fill=(int(176 * t), int(138 * t), int(66 * t)))
d.rectangle((6, 6, W - 7, H - 7), outline=(92, 70, 30), width=4)
f = ImageFont.truetype(r"C:\Windows\Fonts\georgiab.ttf", 110)
bb = d.textbbox((0, 0), "11", font=f)
x, y = (W - (bb[2] - bb[0])) / 2 - bb[0], (H - (bb[3] - bb[1])) / 2 - bb[1]
d.text((x + 2, y + 3), "11", font=f, fill=(70, 52, 20))
d.text((x, y), "11", font=f, fill=(40, 30, 12))
im = im.filter(ImageFilter.GaussianBlur(0.4))
im.save(OUT); print("tamam", OUT)
