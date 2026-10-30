# Kat 2 (iş) dokuları: iş emri (duyurunun yerine geçer), asansör göstergesi "23:42", mesai fişi.
# Pişirme gerekmez: Unity'deki *_Renk.png dokuları kaynak PNG'nin 1024x1024'e BICUBIC büyütülmüş hali (fark < 1/255).
# Çalıştır: python koridor_kat2_baski.py
import os, random
from PIL import Image, ImageChops, ImageDraw, ImageFilter, ImageFont

random.seed(22)
ROOT = r"C:\Kisisel Dosyalarim\ozel\Oyun Gelistirme\Denemelik projelerim\Dink"
KAYNAK = os.path.join(ROOT, "Kaynak", "Blender", "Dokular", "Koridor")
UNITY = os.path.join(ROOT, "Assets", "_Project", "Textures", "Koridor")
F = r"C:\Windows\Fonts" + "\\"


def font(ad, s):
    return ImageFont.truetype(F + ad, s)


def merkez(d, kutu, yazi, f, renk):
    x0, y0, x1, y1 = kutu
    bb = d.textbbox((0, 0), yazi, font=f)
    d.text(((x0 + x1 - (bb[2] - bb[0])) / 2 - bb[0], (y0 + y1 - (bb[3] - bb[1])) / 2 - bb[1]), yazi, font=f, fill=renk)


def carp(im, k):
    return ImageChops.multiply(im, Image.merge("RGB", [k.point(lambda v: min(255, v + 127))] * 3))


def kaydet(im, kaynak_adi, unity_adi):
    im.save(os.path.join(KAYNAK, kaynak_adi))
    im.resize((1024, 1024), Image.BICUBIC).save(os.path.join(UNITY, unity_adi))


# ── İş emri (512x724): duyuruyla aynı kâğıt, daktilo form + tükenmez kalemle düşülmüş emir ──
w, h = 512, 724
im = Image.new("RGB", (w, h), (238, 232, 214))
lif = Image.effect_noise((w // 2, h // 2), 30).convert("L").resize((w, h), Image.BICUBIC)
im = carp(im, lif.point(lambda v: (v - 128) // 5 + 128))
sar = Image.new("L", (w, h), 0); sd = ImageDraw.Draw(sar)
for k in range(30):
    sd.rectangle([k, k, w - 1 - k, h - 1 - k], outline=120 - k * 4)
im = Image.composite(Image.new("RGB", (w, h), (214, 196, 150)), im, sar.filter(ImageFilter.GaussianBlur(8)))
d = ImageDraw.Draw(im)
fb, fm = font("courbd.ttf", 30), font("cour.ttf", 19)
merkez(d, (0, 60, w, 110), "WORK ORDER", fb, (30, 30, 30))
merkez(d, (0, 108, w, 150), "SHIFT SUPERVISOR", font("courbd.ttf", 22), (30, 30, 30))
d.line([(50, 160), (w - 50, 160)], fill=(60, 60, 64), width=2)
satirlar = ["To:   H. Kaya", "Re:   Stock, cold storage", "Date: 14.11.2023", "", "- Unload the cold storage.", "- Restock aisles 1 to 6.",
            "- Count is tomorrow morning.", ""]
y = 180
for s in satirlar:
    d.text((50, y), s, font=fm, fill=(36, 36, 40)); y += 26
# Emir: elle, kalem basıncıyla hafif eğik, iki satır
kalem = font("segoepr.ttf", 29)
for i, s in enumerate(["DON'T LEAVE UNTIL", "THE SHELVES ARE DONE."]):
    d.text((46 + i * 4, y + 16 + i * 44), s, font=kalem, fill=(24, 30, 92))
d.text((330, y + 130), "- M.", font=font("segoepr.ttf", 44), fill=(24, 30, 92))
im = im.filter(ImageFilter.GaussianBlur(0.5))
d = ImageDraw.Draw(im)
for (x, yy, a) in [(60, 6, -8), (w - 150, 10, 6)]:
    bant = Image.new("RGBA", (100, 34), (240, 230, 190, 150)).rotate(a, expand=True)
    im.paste(bant, (x, yy), bant)
kaydet(im, "is_emri.png", "K_Duyuru_Renk_Is.png")

# ── Asansör göstergesi "23:42" (256x128): kırmızı segment, söndük segmentler ──
im = Image.new("RGB", (256, 128), (10, 8, 8)); d = ImageDraw.Draw(im)
fs = font("consolab.ttf", 70)
cw = d.textbbox((0, 0), "8", font=fs)
adv = d.textlength("8", font=fs)
x0 = (256 - adv * 5) / 2
ydig = (128 - (cw[3] - cw[1])) / 2 - cw[1]
for i, ch in enumerate("23:42"):
    d.text((x0 + i * adv, ydig), "8" if ch != ":" else "", font=fs, fill=(36, 10, 10))   # söndük segmentler
    d.text((x0 + i * adv, ydig), ch, font=fs, fill=(255, 40, 30))
kaydet(im.filter(ImageFilter.GaussianBlur(1.2)), "kat_gosterge_2342.png", "K_KatGosterge_Renk_2342.png")

# ── Mesai fişi (256x384): termal kâğıt, solmuş mürekkep, bitiş saati boş ──
im = Image.new("RGB", (256, 384), (236, 234, 226))
fis = Image.effect_noise((256, 384), 14).convert("L")
im = carp(im, fis.point(lambda v: (v - 128) // 6 + 128))
d = ImageDraw.Draw(im)
fm, fb = font("cour.ttf", 20), font("courbd.ttf", 24)
merkez(d, (0, 24, 256, 64), "TIME SLIP", fb, (60, 60, 64))
d.line([(20, 72), (236, 72)], fill=(110, 110, 114), width=1)
for i, s in enumerate(["SHIFT   08:00", "END     --:--", "", "H. KAYA", "14.11.2023"]):
    d.text((24, 96 + i * 32), s, font=fm, fill=(70, 70, 76))
d.line([(20, 280), (236, 280)], fill=(110, 110, 114), width=1)
merkez(d, (0, 292, 256, 340), "END: --", font("courbd.ttf", 30), (60, 60, 64))
kaydet(im.filter(ImageFilter.GaussianBlur(0.4)), "mesai_fisi.png", "K_MesaiFisi_Renk.png")
print("tamam")
