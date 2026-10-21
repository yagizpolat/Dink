# Kat 3, "Yoklama": Kat1 koridoru (Daire 3 aralık) + Daire 3'ün içinde bir lise sınıfının köşesi.
# Yeşil tahta, iki sıra dizisi, 23:42'de durmuş saat, öğretmen masasında açık sınıf defteri (Kat3Defter ayrı nesne).
import bpy, math, random
B = r"C:\Kisisel Dosyalarim\ozel\Oyun Gelistirme\Denemelik projelerim\Dink\Kaynak\BlenderBetikleri"
AD, ACIK3 = 'Kat3Koridor', 82
exec(open(B + r"\kat1.py", encoding='utf-8').read())

def mat(n, rgb, rough=0.6, metal=0.0):
    m = bpy.data.materials.get(n) or bpy.data.materials.new(n); m.use_nodes = True
    p = next(x for x in m.node_tree.nodes if x.type == 'BSDF_PRINCIPLED')
    p.inputs['Base Color'].default_value = (*rgb, 1); p.inputs['Roughness'].default_value = rough; p.inputs['Metallic'].default_value = metal
    return n
tahta = mat('K3_Tahta', (0.07, 0.16, 0.11), 0.85); tebesir = mat('K3_Tebesir', (0.75, 0.75, 0.7), 0.95)
sira = mat('K3_SiraAhsap', (0.36, 0.22, 0.12), 0.55); demir = mat('K3_Demir', (0.12, 0.2, 0.17), 0.5, 0.6)
parke = mat('K3_Parke', (0.3, 0.19, 0.11), 0.7); kapak = mat('K3_DefterKapak', (0.35, 0.05, 0.05), 0.7)
sayfa = mat('K3_Sayfa', (0.85, 0.83, 0.76), 0.9); saat = mat('K3_SaatYuz', (0.9, 0.88, 0.82), 0.4)

s = Yapici('Kat3Sinif')
RX0, RX1, RY0, RY1, H = 2.8, 6.6, 2.2, 7.8, 3.0
s.box((RX0, RY0, -0.1), (RX1, RY1, 0), parke, uv=0.6)
s.box((RX0, RY0, H), (RX1, RY1, H + 0.1), 'L_Tavan')
for lo, hi in [((RX1, RY0, 0), (RX1 + 0.2, RY1, H)), ((RX0, RY0 - 0.2, 0), (RX1 + 0.2, RY0, H)), ((RX0, RY1, 0), (RX1 + 0.2, RY1 + 0.2, H))]:
    s.box(lo, (hi[0], hi[1], 1.2), 'L_YagliBoya'); s.box((lo[0], lo[1], 1.2), hi, 'L_Siva')
# tahta: doğu duvarında, tebeşir oluğu ve yarım silinmiş yazı lekeleri
s.box((RX1 - 0.03, 3.4, 0.9), (RX1, 6.6, 2.1), 'L_MasaAhsap', 0.005, uv=0.5)
s.box((RX1 - 0.035, 3.45, 0.95), (RX1 - 0.03, 6.55, 2.05), tahta)
s.box((RX1 - 0.1, 3.45, 0.88), (RX1 - 0.03, 6.55, 0.9), 'L_MasaAhsap')
random.seed(14)
for i in range(9):
    yy = random.uniform(3.7, 6.1); z = random.uniform(1.2, 1.9)
    s.box((RX1 - 0.037, yy, z), (RX1 - 0.035, yy + random.uniform(0.15, 0.45), z + 0.02), tebesir)
# floresan armatürler
for yy in (3.6, 6.4):
    s.box((4.2, yy - 0.6, H - 0.06), (4.5, yy + 0.6, H), 'L_PostaKrom', 0.005)
    s.box((4.25, yy - 0.57, H - 0.075), (4.45, yy + 0.57, H - 0.06), 'L_Ampul')

def sira_kur(x, y):
    """İkili okul sırası: ahşap tabla + demir ayak + oturak, tahtaya (+x) bakar."""
    s.box((x - 0.25, y - 0.6, 0.72), (x + 0.2, y + 0.6, 0.75), sira, 0.005, uv=0.5)
    s.box((x - 0.25, y - 0.6, 0.6), (x + 0.15, y + 0.6, 0.62), sira, uv=0.5)
    for yy in (y - 0.55, y + 0.55):
        s.box((x + 0.1, yy - 0.015, 0), (x + 0.13, yy + 0.015, 0.72), demir)
        s.box((x - 0.7, yy - 0.015, 0), (x - 0.67, yy + 0.015, 0.42), demir)
        s.box((x - 0.7, yy - 0.015, 0.02), (x + 0.13, yy + 0.015, 0.05), demir)
    s.box((x - 0.75, y - 0.6, 0.42), (x - 0.4, y + 0.6, 0.45), sira, 0.005, uv=0.5)
    s.box((x - 0.78, y - 0.6, 0.45), (x - 0.75, y + 0.6, 0.85), sira, 0.005, uv=0.5)
SIRALAR = [(3.9, 3.4), (3.9, 6.4), (4.9, 3.4), (4.9, 6.4)]
for x, y in SIRALAR: sira_kur(x, y)
# öğretmen masası: tahta önünde, kapıya yakın uçta
s.box((5.6, 2.6, 0.72), (6.2, 3.6, 0.76), 'L_MasaAhsap', 0.006, uv=0.5)
s.box((5.62, 2.62, 0.1), (6.18, 2.68, 0.72), 'L_MasaAhsap', uv=0.5); s.box((5.62, 3.52, 0.1), (6.18, 3.58, 0.72), 'L_MasaAhsap', uv=0.5)
s.box((6.12, 2.62, 0.1), (6.18, 3.58, 0.72), 'L_MasaAhsap', uv=0.5)
# saat: kapının karşısındaki kuzey duvarında, 23:42'de durmuş
s.cyl((4.7, RY1 - 0.03, 2.35), 0.17, 0.04, 'L_Bakalit', 'Y', 32); s.cyl((4.7, RY1 - 0.055, 2.35), 0.15, 0.01, saat, 'Y', 32)
def ibre(aci, boy, kal):
    v = s.box((4.7 - kal / 2, RY1 - 0.065, 2.35), (4.7 + kal / 2, RY1 - 0.06, 2.35 + boy), 'L_Karanlik')
    s.rotate(v, -math.radians(aci), (4.7, RY1 - 0.06, 2.35), 'Y')
ibre((23 % 12 + 42 / 60) * 30, 0.08, 0.012); ibre(42 * 6, 0.12, 0.008)
for k in range(12):
    v = s.box((4.698, RY1 - 0.062, 2.35 + 0.125), (4.702, RY1 - 0.06, 2.35 + 0.14), 'L_Karanlik'); s.rotate(v, -math.radians(k * 30), (4.7, RY1 - 0.06, 2.35), 'Y')
sn = s.done()
birlestir(sn, [yazi('Sira11', '11', (4.95, 6.85, 0.752), (0, 0, -math.pi / 2), 0.07, 'L_Karanlik', r"C:\Windows\Fonts\segoesc.ttf")])

# Açık sınıf defteri (ayrı nesne, Unity'de tıklanır)
d = Yapici('Kat3Defter')
d.box((5.72, 2.8, 0.76), (6.05, 3.42, 0.765), kapak, 0.002)
for sgn in (-1, 1):
    d.box((5.74, 3.11 + (0.005 if sgn > 0 else -0.29), 0.765), (6.03, 3.11 + (0.29 if sgn > 0 else -0.005), 0.775), sayfa)
for i in range(14):
    d.box((5.76 + i * 0.019, 2.84, 0.7752), (5.762 + i * 0.019, 3.38, 0.7754), 'L_Karanlik')
d.done()
print('kat3 tamam', len(sn.data.polygons))
