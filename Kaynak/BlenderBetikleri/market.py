import bpy, bmesh, math, random
from mathutils import Vector
exec(open(r"C:\Kisisel Dosyalarim\ozel\Oyun Gelistirme\Denemelik projelerim\Dink\Kaynak\BlenderBetikleri\yardim.py", encoding='utf-8').read())

# Daire 3'ün içi: tek reyonluk market. Kapı boşluğu x 2.6..2.8, y 4.05..4.95
m = Yapici('Kat1Market')
RX0, RX1, RY0, RY1, H = 2.8, 6.6, 2.2, 7.8, 3.0
m.box((RX0, RY0, -0.1), (RX1, RY1, 0), 'L_Mermer')
m.box((RX0, RY0, H), (RX1, RY1, H + 0.1), 'L_Tavan')
m.box((RX1, RY0, 0), (RX1 + 0.2, RY1, H), 'L_Tavan')
m.box((RX0, RY0 - 0.2, 0), (RX1 + 0.2, RY0, H), 'L_Tavan')
m.box((RX0, RY1, 0), (RX1 + 0.2, RY1 + 0.2, H), 'L_Tavan')
# market şeridi: duvar dibinde kırmızı bant
for (a, b) in [((RX0, RY0, 0), (RX1, RY0 + 0.01, 0.1)), ((RX0, RY1 - 0.01, 0), (RX1, RY1, 0.1)), ((RX1 - 0.01, RY0, 0), (RX1, RY1, 0.1))]:
    m.box(a, b, 'L_Raptiye')

# floresan armatürler
for yy in (3.4, 5.0, 6.6):
    m.box((4.0, yy - 0.6, H - 0.06), (4.3, yy + 0.6, H), 'L_PostaKrom', 0.005)
    m.box((4.05, yy - 0.57, H - 0.075), (4.25, yy + 0.57, H - 0.06), 'L_Ampul')

random.seed(11)
URUN = ['L_Kagit', 'L_Cilt', 'L_CayBardak', 'L_Saksi', 'L_Kalem', 'L_LambaBasi', 'L_Raptiye', 'L_Cay', 'L_Tabak', 'L_Bakalit']

def raf(x0, x1, y0, y1, yon):
    """Gondol raf: arka pano + 4 kat + ürünler. yon: ürünlerin baktığı taraf (+1 +x, -1 -x)."""
    m.box((x0, y0, 0), (x1, y1, 0.12), 'L_PostaKasa', 0.004, uv=0.5)
    px = x0 if yon < 0 else x1
    bx = (x1 - 0.03, x1) if yon < 0 else (x0, x0 + 0.03)
    m.box((bx[0], y0, 0.12), (bx[1], y1, 1.75), 'L_PostaKasa', 0.004, uv=0.5)
    for yy in (y0, y1 - 0.03):
        m.box((x0, yy, 0), (x1, yy + 0.03, 1.8), 'L_PostaKasa', 0.004, uv=0.5)
    for z in (0.12, 0.52, 0.92, 1.32):
        m.box((x0, y0 + 0.03, z), (x1, y1 - 0.03, z + 0.02), 'L_PostaKrom', 0.003, uv=0.5)
        m.box((px - 0.005 if yon > 0 else px - 0.005, y0 + 0.03, z - 0.035), (px + 0.005, y1 - 0.03, z + 0.02), 'L_Kagit', uv=0.5)  # fiyat şeridi
        yy = y0 + 0.05
        while yy < y1 - 0.12:
            w = random.uniform(0.06, 0.14); hgt = random.uniform(0.12, 0.3); d = random.uniform(0.12, (x1 - x0) - 0.08)
            if random.random() < 0.12: yy += w; continue  # boş yer: biri bir şey almış
            mat = random.choice(URUN)
            if random.random() < 0.3:
                m.cyl((px - yon * 0.06, yy + w / 2, z + 0.02 + hgt / 2), w / 2.2, hgt, mat, 'Z', 12)
            else:
                xa = px - yon * d if yon > 0 else px
                xb = px if yon > 0 else px + d
                xa, xb = (px - d, px) if yon > 0 else (px, px + d)
                m.box((xa, yy, z + 0.02), (xb, yy + w, z + 0.02 + hgt), mat, 0.004, uv=0.5)
            yy += w + random.uniform(0.005, 0.02)

raf(4.6, 5.1, 3.0, 7.2, -1)      # orta gondol, kapıya bakan yüz
raf(6.1, 6.6, 2.6, 7.6, -1)      # duvar rafı

# kasa tezgâhı (kapının hemen sağında)
m.box((3.1, 5.3, 0), (3.9, 7.0, 0.9), 'L_MasaAhsap', 0.01, 3, uv=0.5)
m.box((3.05, 5.25, 0.9), (3.95, 7.05, 0.94), 'L_PostaKrom', 0.006, uv=0.5)
m.box((3.3, 5.5, 0.94), (3.65, 5.9, 1.12), 'L_Bakalit', 0.015, 3, uv=0.5)          # yazar kasa
m.box((3.32, 5.52, 1.12), (3.63, 5.88, 1.18), 'L_Karanlik', 0.01, uv=0.5)
m.box((3.4, 5.95, 0.94), (3.6, 6.12, 1.06), 'L_Karanlik', 0.01, 2, uv=0.5)          # fiş yazıcısı
# yazıcıdan sarkan ve yere yığılan fişler
m.box((3.395, 6.04, 0.985), (3.405, 6.1, 1.06), 'L_Kagit', uv=0.5)
for i in range(18):
    x = random.uniform(2.95, 3.6); yy = random.uniform(5.9, 6.6); a = random.uniform(0, math.pi)
    v = m.box((x - 0.04, yy - 0.12, 0.001 + i * 0.0006), (x + 0.04, yy + 0.12, 0.0016 + i * 0.0006), 'L_Kagit', uv=0.5)
    m.rotate(v, a, (x, yy, 0))
# tezgâhın önündeki sakız/şeker standı
m.box((3.0, 4.95, 0), (3.1, 5.25, 1.1), 'L_PostaKasa', 0.004, uv=0.5)
for z in (0.3, 0.6, 0.9):
    for i in range(3):
        m.box((2.97, 4.97 + i * 0.09, z), (3.0, 5.04 + i * 0.09, z + 0.1), random.choice(URUN), 0.003, uv=0.5)

mk = m.done()
print('market', len(mk.data.polygons))
