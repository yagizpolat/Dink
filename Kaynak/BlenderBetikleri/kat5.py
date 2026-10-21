# Kat 5, "Kavga gecesi": Kat1 koridoru güneye ~30 m uzatılmış. Daire 3'ün kanadı ayrı nesne (Kat5KoridorKapi3),
# arkasında karanlık bir antre; kovalayan oradan çıkar. Koridorun ucunda aralık yangın merdiveni kapısı ve yeşil
# ÇIKIŞ levhası. Kovalayan (Kat5Arkadas) ayrı nesne: market yeleği giymiş iş arkadaşı, ayakları dibinde orijin.
import bpy, math
B = r"C:\Kisisel Dosyalarim\ozel\Oyun Gelistirme\Denemelik projelerim\Dink\Kaynak\BlenderBetikleri"
AD, ACIK3, Y0K = 'Kat5Koridor', 'ayri', -22.0
UC_KAPI = (0.92, 1.88, 2.1)
# 11 yok: koridor 10'dan 12'ye atlar
KAPILAR_EK = [('5', 0.6, -1), ('6', -1.5, 1), ('7', -3.6, -1), ('8', -6.0, 1), ('9', -8.2, -1), ('10', -10.5, 1),
              ('12', -12.8, -1), ('13', -15.0, 1), ('14', -17.3, -1), ('15', -19.4, 1)]
exec(open(B + r"\kat1.py", encoding='utf-8').read())
KAPI3.done()

def mat(n, rgb, rough=0.6, metal=0.0):
    m = bpy.data.materials.get(n) or bpy.data.materials.new(n); m.use_nodes = True
    p = next(x for x in m.node_tree.nodes if x.type == 'BSDF_PRINCIPLED')
    p.inputs['Base Color'].default_value = (*rgb, 1); p.inputs['Roughness'].default_value = rough; p.inputs['Metallic'].default_value = metal
    return n
yesil = mat('K5_CikisYesil', (0.1, 0.8, 0.3), 0.4); metal = mat('K5_YanginKapi', (0.35, 0.33, 0.3), 0.5, 0.5)
kara = mat('K5_Karanlik', (0.01, 0.01, 0.012), 1.0)
yelek = mat('K5_Yelek', (0.5, 0.06, 0.05), 0.8); kumas = mat('K5_Kumas', (0.06, 0.07, 0.09), 0.9)
ten = mat('K5_Ten', (0.42, 0.3, 0.24), 0.7); sac = mat('K5_Sac', (0.03, 0.025, 0.02), 0.9)

e = Yapici('Kat5Ek')
# Daire 3'ün arkasında karanlık antre (kovalayan buradan çıkar)
e.box((2.8, 3.6, -0.1), (4.4, 5.4, 0), kara); e.box((2.8, 3.6, 3.0), (4.4, 5.4, 3.1), kara)
e.box((4.4, 3.6, 0), (4.6, 5.4, 3.0), kara); e.box((2.8, 3.4, 0), (4.6, 3.6, 3.0), kara); e.box((2.8, 5.4, 0), (4.6, 5.6, 3.0), kara)
# tavan lambaları: koridor boyunca dört metrede bir
for yy in [7.0, 3.0, -1.0, -5.0, -9.0, -13.0, -17.0, -21.0]:
    e.cyl((1.4, yy, 2.97), 0.12, 0.06, 'L_PostaKrom', 'Z', 16)
    e.cyl((1.4, yy, 2.9), 0.1, 0.1, 'L_Ampul', 'Z', 16, r2=0.06)
# güney ucu: yangın merdiveni kapısı (aralık) ve arkasında karanlık merdiven boşluğu
SY = Y0K
e.box((0.85, SY - 0.03, 0), (0.92, SY + 0.03, 2.15), 'L_MasaAhsap'); e.box((1.88, SY - 0.03, 0), (1.95, SY + 0.03, 2.15), 'L_MasaAhsap')
e.box((0.85, SY - 0.03, 2.1), (1.95, SY + 0.03, 2.17), 'L_MasaAhsap')
leaf = e.box((0.92, SY - 0.05, 0.01), (1.88, SY - 0.01, 2.09), metal, 0.006)
e.box((1.7, SY + 0.0, 0.95), (1.8, SY + 0.04, 1.02), 'L_PostaKrom')
e.rotate(leaf, math.radians(-35), (0.92, SY, 0))
e.box((0.6, SY - 2.5, -0.1), (2.2, SY - 0.2, 0), kara); e.box((0.6, SY - 2.5, 3.0), (2.2, SY - 0.2, 3.1), kara)
e.box((0.4, SY - 2.7, 0), (0.6, SY - 0.2, 3.0), kara); e.box((2.2, SY - 2.7, 0), (2.4, SY - 0.2, 3.0), kara); e.box((0.6, SY - 2.7, 0), (2.2, SY - 2.5, 3.0), kara)
for i in range(6):  # yukarı çıkan basamaklar
    e.box((0.6, SY - 0.9 - i * 0.28, 0), (2.2, SY - 0.62 - i * 0.28, 0.17 * (i + 1)), 'L_Mozaik')
# ÇIKIŞ levhası
e.box((1.15, SY + 0.0, 2.25), (1.65, SY + 0.06, 2.45), yesil)
ek = e.done()
birlestir(ek, [yazi('Cikis', 'ÇIKIŞ', (1.4, SY + 0.065, 2.35), (math.radians(90), 0, math.pi), 0.1, 'L_Ampul')])

# Kovalayan: market yeleği, iş pantolonu; kollar hafif önde. Orijin ayakların dibinde, yüzü +y.
a = Yapici('Kat5Arkadas')
for sx in (-1, 1):
    a.box((sx * 0.11 - 0.07, -0.08, 0), (sx * 0.11 + 0.07, 0.12, 0.08), sac, 0.01)            # ayakkabı
    a.box((sx * 0.11 - 0.07, -0.07, 0.08), (sx * 0.11 + 0.07, 0.07, 0.92), kumas, 0.02)        # bacak
    arm = a.box((sx * 0.27 - 0.05, -0.05, 0.85), (sx * 0.27 + 0.05, 0.05, 1.5), kumas, 0.02)    # kol
    a.rotate(arm, math.radians(25), (sx * 0.27, 0, 1.5), 'X')
    a.box((sx * 0.27 - 0.045, 0.2, 0.85), (sx * 0.27 + 0.045, 0.3, 0.93), ten, 0.02)           # el
a.box((-0.22, -0.12, 0.9), (0.22, 0.12, 1.55), kumas, 0.03)                                     # gövde
a.box((-0.235, -0.135, 0.95), (0.235, 0.135, 1.52), yelek, 0.03)                                # yelek
a.box((-0.06, -0.06, 1.55), (0.06, 0.06, 1.62), ten)                                            # boyun
a.box((-0.1, -0.11, 1.62), (0.1, 0.11, 1.86), ten, 0.04)                                        # baş
a.box((-0.105, -0.12, 1.78), (0.105, 0.1, 1.88), sac, 0.03)                                     # saç
a.box((-0.07, 0.105, 1.73), (-0.03, 0.112, 1.75), kara); a.box((0.03, 0.105, 1.73), (0.07, 0.112, 1.75), kara)  # gözler
a.done()
print('kat5 tamam', len(ek.data.polygons))
