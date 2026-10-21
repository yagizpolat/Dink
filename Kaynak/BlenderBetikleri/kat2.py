# Kat 2, "Sayaç dönüyor": Kat1 koridoru (bütün kapılar kapalı) + asansör yanında açık sayaç panosu
# + her daire kapısında bantlı kesme ihbarı. Dönen disk ayrı nesne (Kat2Disk), Unity döndürür.
import bpy, math
B = r"C:\Kisisel Dosyalarim\ozel\Oyun Gelistirme\Denemelik projelerim\Dink\Kaynak\BlenderBetikleri"
AD, ACIK3 = 'Kat2Koridor', 0
exec(open(B + r"\kat1.py", encoding='utf-8').read())

def mat(n, rgb, rough=0.6, metal=0.0, emit=None):
    m = bpy.data.materials.get(n) or bpy.data.materials.new(n); m.use_nodes = True
    p = next(x for x in m.node_tree.nodes if x.type == 'BSDF_PRINCIPLED')
    p.inputs['Base Color'].default_value = (*rgb, 1); p.inputs['Roughness'].default_value = rough; p.inputs['Metallic'].default_value = metal
    if emit: p.inputs['Emission Color'].default_value = (*emit, 1); p.inputs['Emission Strength'].default_value = 2.0
    return n
kagit = mat('K2_Kagit', (0.82, 0.8, 0.72), 0.9); kirmizi = mat('K2_IhbarKirmizi', (0.55, 0.06, 0.05), 0.8)
bant = mat('K2_Bant', (0.75, 0.68, 0.45), 0.4); sac = mat('K2_PanoSac', (0.32, 0.34, 0.3), 0.55, 0.6)
cam = mat('K2_SayacCam', (0.6, 0.65, 0.6), 0.1); govde = mat('K2_SayacGovde', (0.12, 0.12, 0.13), 0.5)
disk = mat('K2_Disk', (0.7, 0.7, 0.68), 0.3, 0.9); hane = mat('K2_Hane', (0.02, 0.02, 0.02), 0.7)

# Pano: batı duvarında asansörün yanında, kapağı aralık
p = Yapici('Kat2Pano')
PX, PY0, PY1, PZ0, PZ1 = X0, 7.55, 8.35, 1.0, 1.95
p.box((PX, PY0, PZ0), (PX + 0.2, PY0 + 0.02, PZ1), sac); p.box((PX, PY1 - 0.02, PZ0), (PX + 0.2, PY1, PZ1), sac)
p.box((PX, PY0, PZ0), (PX + 0.2, PY1, PZ0 + 0.02), sac); p.box((PX, PY0, PZ1 - 0.02), (PX + 0.2, PY1, PZ1), sac)
p.box((PX, PY0, PZ0), (PX + 0.01, PY1, PZ1), govde)
kap = p.box((PX + 0.2, PY0, PZ0), (PX + 0.215, PY1, PZ1), sac, 0.003)
p.rotate(kap, math.radians(115), (PX + 0.2, PY1, 0))
SAYAC = [(7.75, 1.62), (8.15, 1.62), (7.75, 1.25), (8.15, 1.25)]
for cy, cz in SAYAC:
    p.box((PX + 0.01, cy - 0.12, cz - 0.14), (PX + 0.13, cy + 0.12, cz + 0.14), govde, 0.01)
    p.box((PX + 0.13, cy - 0.09, cz - 0.02), (PX + 0.135, cy + 0.09, cz + 0.1), cam)
    p.box((PX + 0.131, cy - 0.06, cz + 0.05), (PX + 0.136, cy + 0.06, cz + 0.08), hane)
    p.cyl((PX + 0.07, cy - 0.06, cz - 0.16), 0.012, 0.06, sac)
    p.cyl((PX + 0.07, cy + 0.06, cz - 0.16), 0.012, 0.06, sac)
pano = p.done()
for i, (cy, cz) in enumerate(SAYAC):
    h = yazi('Hane%d' % i, ['0 4 1 7 3', '0 2 2 9 0', '1 1 1 1 1', '0 3 8 6 5'][i], (PX + 0.137, cy, cz + 0.065), (math.radians(90), 0, math.pi / 2), 0.022, 'K2_Kagit', r"C:\Windows\Fonts\cour.ttf")
    birlestir(pano, [h])
# Deli gibi dönen disk: üçüncü sayaç (1 1 1 1 1)
d = Yapici('Kat2Disk'); d.cyl((0, 0, 0), 0.07, 0.004, disk, 'Z', 32); d.box((0.05, -0.006, -0.003), (0.07, 0.006, 0.003), kirmizi)
dob = d.done(); dob.location = (PX + 0.08, SAYAC[2][0], SAYAC[2][1] + 0.0)

# Kesme ihbarları: her kapının göz hizasında, iki bant çapraz
ih = Yapici('Kat2Ihbarlar')
for cy, yon in [(2.6, 1), (6.4, 1), (6.9, -1), (4.5, -1)]:
    fx = (X0 + 0.071) if yon > 0 else (X1 - 0.071); s = yon
    lo, hi = (fx, fx + s * 0.002) if s > 0 else (fx + s * 0.002, fx)
    ih.box((lo, cy - 0.105, 1.18), (hi, cy + 0.105, 1.48), kagit, uv=0.3)
    ih.box((min(fx + s*0.002, fx + s*0.003), cy - 0.1, 1.43), (max(fx + s*0.002, fx + s*0.003), cy + 0.1, 1.465), kirmizi)
    for k in range(5):
        z = 1.39 - k * 0.035
        ih.box((min(fx + s*0.002, fx + s*0.0025), cy - 0.085, z), (max(fx + s*0.002, fx + s*0.0025), cy + (0.085 if k % 2 == 0 else 0.03), z + 0.008), hane)
    for sy in (-1, 1):
        t = ih.box((min(fx + s*0.003, fx + s*0.004), cy + sy * 0.09 - 0.07, 1.455), (max(fx + s*0.003, fx + s*0.004), cy + sy * 0.09 + 0.07, 1.48), bant)
        ih.rotate(t, sy * math.radians(35), (fx, cy + sy * 0.09, 1.467), 'X')
ihb = ih.done()
print('kat2 tamam', len(pano.data.polygons), len(ihb.data.polygons))
