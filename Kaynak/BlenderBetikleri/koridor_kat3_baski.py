# Kat 3 (okul) dokuları: Daire 4 isimliği "STONE" yerine elle yazılmış "KAYA".
# Unity'deki *_Renk.png = kaynak PNG'nin 1024x1024 BICUBIC büyütmesi (pişirme gerekmez).
# Çalıştır: python koridor_kat3_baski.py
import os, random
from PIL import Image, ImageDraw, ImageFilter, ImageFont

random.seed(33)
ROOT = r"C:\Kisisel Dosyalarim\ozel\Oyun Gelistirme\Denemelik projelerim\Dink"
KAYNAK = os.path.join(ROOT, "Kaynak", "Blender", "Dokular", "Koridor")
UNITY = os.path.join(ROOT, "Assets", "_Project", "Textures", "Koridor")
F = r"C:\Windows\Fonts" + "\\"

im = Image.open(os.path.join(KAYNAK, "isimlik.png")).convert("RGB")
d = ImageDraw.Draw(im)
x0 = 3 * 256                                     # 4. hücre = Daire 4
d.rectangle([x0, 0, x0 + 255, 255], fill=(232, 230, 222))
d.rectangle([x0 + 22, 70, x0 + 234, 186], fill=(244, 238, 220))   # taze kart
f = ImageFont.truetype(F + "segoepr.ttf", 52)
bb = d.textbbox((0, 0), "KAYA", font=f)
d.text((x0 + (256 - (bb[2] - bb[0])) / 2 - bb[0] + 2, 128 - (bb[3] - bb[1]) / 2 - bb[1] - 2), "KAYA", font=f, fill=(30, 32, 70))
d.rectangle([x0 + 22, 70, x0 + 234, 186], outline=(200, 198, 190), width=3)
im = im.filter(ImageFilter.GaussianBlur(0.3))
im.save(os.path.join(KAYNAK, "isimlik_kaya.png"))
im.resize((1024, 1024), Image.BICUBIC).save(os.path.join(UNITY, "K_Isimlik_Renk_Kaya.png"))
print("tamam")
