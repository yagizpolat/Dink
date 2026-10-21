# Kat 4, "Sofra": Kat1 koridoru (Daire 3 aralık) + Daire 3'ün içinde Hakan'ların oturma odası.
# Halı, duvarda 23:42'de durmuş saat, ortada dört kişilik kurulmuş sofra. Batı yanı çocuklar
# (Hakan'ın kapalı tabağı, kardeşin boya kalemli tabağı), doğu yanı anne-baba (dolu tabaklar).
# Kardeşin notu ayrı nesne (Kat4Not), Unity'de tıklanır.
import bpy, math, random
B = r"C:\Kisisel Dosyalarim\ozel\Oyun Gelistirme\Denemelik projelerim\Dink\Kaynak\BlenderBetikleri"
AD, ACIK3 = 'Kat4Koridor', 82
exec(open(B + r"\kat1.py", encoding='utf-8').read())

def mat(n, rgb, rough=0.6, metal=0.0):
    m = bpy.data.materials.get(n) or bpy.data.materials.new(n); m.use_nodes = True
    p = next(x for x in m.node_tree.nodes if x.type == 'BSDF_PRINCIPLED')
    p.inputs['Base Color'].default_value = (*rgb, 1); p.inputs['Roughness'].default_value = rough; p.inputs['Metallic'].default_value = metal
    return n
duvar = mat('K4_DuvarKagidi', (0.55, 0.47, 0.34), 0.85); alt = mat('K4_Lambri', (0.33, 0.22, 0.14), 0.6)
parke = mat('K3_Parke', (0.3, 0.19, 0.11), 0.7); hali = mat('K4_Hali', (0.38, 0.08, 0.07), 0.95)
hali2 = mat('K4_HaliDesen', (0.62, 0.48, 0.25), 0.95); masa = mat('K4_Masa', (0.24, 0.13, 0.07), 0.45)
ortu = mat('K4_MasaOrtusu', (0.85, 0.83, 0.76), 0.9); porselen = mat('K4_Porselen', (0.92, 0.91, 0.87), 0.25)
yemek = mat('K4_Yemek', (0.45, 0.2, 0.08), 0.6); pilav = mat('K4_Pilav', (0.9, 0.85, 0.7), 0.8)
kirmizi = mat('K4_Boya', (0.7, 0.1, 0.08), 0.5); mavi = mat('K4_BoyaMavi', (0.1, 0.25, 0.65), 0.5)
sari = mat('K4_BoyaSari', (0.85, 0.7, 0.1), 0.5); kagit = mat('K3_Sayfa', (0.85, 0.83, 0.76), 0.9)
saat = mat('K3_SaatYuz', (0.9, 0.88, 0.82), 0.4); cam = mat('K4_Bardak', (0.7, 0.75, 0.75), 0.05)

e = Yapici('Kat4Ev')
RX0, RX1, RY0, RY1, H = 2.8, 6.6, 2.2, 7.8, 3.0
e.box((RX0, RY0, -0.1), (RX1, RY1, 0), parke, uv=0.6)
e.box((RX0, RY0, H), (RX1, RY1, H + 0.1), 'L_Tavan')
for lo, hi in [((RX1, RY0, 0), (RX1 + 0.2, RY1, H)), ((RX0, RY0 - 0.2, 0), (RX1 + 0.2, RY0, H)), ((RX0, RY1, 0), (RX1 + 0.2, RY1 + 0.2, H))]:
    e.box(lo, (hi[0], hi[1], 0.9), alt, uv=0.5); e.box((lo[0], lo[1], 0.9), hi, duvar, uv=1.0)
# halı: kenar desenli
e.box((3.7, 3.7, 0), (5.7, 6.3, 0.012), hali, uv=0.5)
e.box((3.8, 3.8, 0.012), (5.6, 6.2, 0.014), hali2, uv=0.5)
e.box((3.9, 3.9, 0.014), (5.5, 6.1, 0.016), hali, uv=0.5)

# masa: uzun kenarı y boyunca, örtülü
MX, MY = 4.7, 5.0
e.box((MX - 0.45, MY - 0.8, 0.72), (MX + 0.45, MY + 0.8, 0.75), masa, 0.008, uv=0.5)
e.box((MX - 0.5, MY - 0.85, 0.75), (MX + 0.5, MY + 0.85, 0.755), ortu, uv=0.5)
for sx in (-1, 1):
    e.box((MX - 0.5 if sx < 0 else MX + 0.495, MY - 0.85, 0.55), (MX - 0.495 if sx < 0 else MX + 0.5, MY + 0.85, 0.755), ortu, uv=0.5)
    for sy in (-1, 1):
        e.box((MX + sx * 0.4 - 0.03, MY + sy * 0.72 - 0.03, 0), (MX + sx * 0.4 + 0.03, MY + sy * 0.72 + 0.03, 0.72), masa, uv=0.5)

def sandalye(x, y, yon):
    """yon=+1 masaya +x yönünde bakar (batı tarafı), -1 doğu tarafı."""
    e.box((x - 0.21, y - 0.21, 0.44), (x + 0.21, y + 0.21, 0.48), masa, 0.01, uv=0.5)
    bx = x - yon * 0.21
    e.box((min(bx, bx - yon * 0.04), y - 0.21, 0.48), (max(bx, bx - yon * 0.04), y + 0.21, 1.0), masa, 0.01, uv=0.5)
    for dx in (-0.18, 0.18):
        for dy in (-0.18, 0.18):
            e.box((x + dx - 0.02, y + dy - 0.02, 0), (x + dx + 0.02, y + dy + 0.02, 0.44), masa, uv=0.5)
SAND = {'hakan': (MX - 0.75, MY - 0.4, 1), 'kardes': (MX - 0.75, MY + 0.4, 1), 'anne': (MX + 0.75, MY + 0.4, -1), 'baba': (MX + 0.75, MY - 0.4, -1)}
for x, y, yon in SAND.values(): sandalye(x, y, yon)

ZT = 0.755
def tabak(x, y, dolu=True):
    e.cyl((x, y, ZT + 0.008), 0.12, 0.016, porselen, 'Z', 32, r2=0.1)
    if dolu:
        e.cyl((x, y - 0.03, ZT + 0.025), 0.06, 0.03, pilav, 'Z', 16, r2=0.045)
        e.cyl((x, y + 0.045, ZT + 0.022), 0.05, 0.022, yemek, 'Z', 16, r2=0.04)
    e.box((x - 0.006, y - 0.2, ZT), (x + 0.006, y - 0.14, ZT + 0.004), 'L_PostaKrom')  # çatal
    e.cyl((x + 0.1, y + 0.12, ZT + 0.05), 0.03, 0.1, cam, 'Z', 16)                       # su bardağı
PX = {'hakan': (MX - 0.28, MY - 0.4), 'kardes': (MX - 0.28, MY + 0.4), 'anne': (MX + 0.28, MY + 0.4), 'baba': (MX + 0.28, MY - 0.4)}
tabak(*PX['anne']); tabak(*PX['baba'])
tabak(*PX['hakan'])
hx, hy = PX['hakan']
e.cyl((hx, hy, ZT + 0.05), 0.115, 0.05, porselen, 'Z', 32, r2=0.09)                     # üstü kapatılmış tabak
kx, ky = PX['kardes']
tabak(kx, ky, dolu=False)
e.cyl((kx, ky - 0.02, ZT + 0.02), 0.05, 0.02, pilav, 'Z', 16, r2=0.04)                   # yarısı yenmiş
v = e.box((kx - 0.1, ky + 0.13, ZT), (kx + 0.1, ky + 0.27, ZT + 0.002), kagit, uv=0.3)    # yarım resim
e.rotate(v, math.radians(-12), (kx, ky + 0.2, 0))
for i, mm in enumerate([kirmizi, mavi, sari]):
    v = e.cyl((kx - 0.05 + i * 0.04, ky + 0.08, ZT + 0.006), 0.005, 0.09, mm, 'X', 8)
    e.rotate(v, math.radians(20 * i - 15), (kx - 0.05 + i * 0.04, ky + 0.08, 0))
# resimde çöp adamlar: dört kişi, ikisi tam boyanmış, ikisi yarım
for i in range(4):
    x0 = kx - 0.075 + i * 0.05
    e.box((x0, ky + 0.18, ZT + 0.0021), (x0 + 0.004, ky + 0.235, ZT + 0.0023), [kirmizi, mavi, 'L_Karanlik', 'L_Karanlik'][i])
# ortada ekmek sepeti ve sürahi
e.cyl((MX, MY, ZT + 0.04), 0.1, 0.08, 'L_MasaAhsap', 'Z', 16, r2=0.12)
e.cyl((MX, MY + 0.25, ZT + 0.11), 0.05, 0.22, cam, 'Z', 16, r2=0.04)

# avize: masanın üstünde, iki kollu (anne-baba yanı / çocuklar yanı)
e.cyl((MX, MY, H - 0.3), 0.008, 0.6, 'L_PostaKrom', 'Z', 8)
e.box((MX - 0.32, MY - 0.01, H - 0.61), (MX + 0.32, MY + 0.01, H - 0.59), 'L_PostaKrom')
for sx in (-1, 1):
    e.cyl((MX + sx * 0.3, MY, H - 0.68), 0.09, 0.12, 'L_Ampul', 'Z', 16, r2=0.04)

# duvarda saat 23:42 (kuzey duvarı) ve konsolun üstünde çerçeve
CX, CZ = 4.7, 2.2
e.cyl((CX, RY1 - 0.03, CZ), 0.16, 0.04, 'L_Bakalit', 'Y', 32); e.cyl((CX, RY1 - 0.055, CZ), 0.14, 0.01, saat, 'Y', 32)
def ibre(aci, boy, kal):
    v = e.box((CX - kal / 2, RY1 - 0.065, CZ), (CX + kal / 2, RY1 - 0.06, CZ + boy), 'L_Karanlik')
    e.rotate(v, -math.radians(aci), (CX, RY1 - 0.06, CZ), 'Y')
ibre((23 % 12 + 42 / 60) * 30, 0.075, 0.012); ibre(42 * 6, 0.115, 0.008)
e.box((5.6, RY1 - 0.4, 0), (6.4, RY1, 0.8), masa, 0.01, uv=0.5)                          # konsol
e.box((5.85, RY1 - 0.2, 0.8), (6.15, RY1 - 0.17, 1.15), 'L_Bakalit', 0.005)              # çerçeve
e.box((5.88, RY1 - 0.205, 0.83), (6.12, RY1 - 0.2, 1.12), kagit)
ev = e.done()

# Kardeşin notu: Hakan'ın tabağının yanında, katlanmış defter yaprağı
n = Yapici('Kat4Not')
nv = n.box((hx - 0.06, hy - 0.36, ZT), (hx + 0.06, hy - 0.22, ZT + 0.003), kagit, uv=0.3)
n.rotate(nv, math.radians(8), (hx, hy - 0.29, 0))
n.done()
print('kat4 tamam', len(ev.data.polygons))
