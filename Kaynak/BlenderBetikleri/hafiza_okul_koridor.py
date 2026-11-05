# Hafiza G3: depo koridoru okul koridoruna kayar. hafiza_depo_kapi.py'den SONRA calistir.
# Ilk 4 m depo (firca izli beton zemin, boyali beton duvar); sonra eski mozaik zemin (sinir duzensiz,
# yamali karolar betona tasar), sivali duvar, iki yanda 3.2 m'lik dev paslı yesil dolaplar
# (hafizada agir yer = devasa). Sonda buzlu camli sinif kapisi, ardinda isik.
import bpy, os, random

DOK = r"C:\Kisisel Dosyalarim\ozel\Oyun Gelistirme\Denemelik projelerim\Dink\Kaynak\Blender\Dokular"
X_DIS, Y0, Y1, DERIN = 6.15, 1.35, 2.35, 18.0
YA, YB = Y0 - 0.6, Y1 + 0.6          # koridor ic duvarlari (y)
XS = X_DIS + 4.0                     # depo / okul siniri
XE = X_DIS + DERIN                   # son duvar
H = 3.6                              # okul tavani: depodan 1 m yuksek, dolaplar yine de dev
HD = 2.6
col = bpy.data.collections["Market_Depo"]
random.seed(7)

def pbr(ad, klasor, isim, ton=None):
    if ad in bpy.data.materials: return bpy.data.materials[ad]
    m = bpy.data.materials.new(ad); m.use_nodes = True
    nt = m.node_tree; b = next(n for n in nt.nodes if n.type == "BSDF_PRINCIPLED")
    def img(son, renkli):
        n = nt.nodes.new("ShaderNodeTexImage")
        n.image = bpy.data.images.load(os.path.join(DOK, klasor, f"{isim}_{son}_2k.jpg"), check_existing=True)
        if not renkli: n.image.colorspace_settings.name = "Non-Color"
        return n
    d = img("diff", True); r = img("rough", False); nr = img("nor_gl", False)
    if ton:
        mx = nt.nodes.new("ShaderNodeMix"); mx.data_type = "RGBA"; mx.blend_type = "MULTIPLY"
        mx.inputs[0].default_value = 1; mx.inputs[7].default_value = (*ton, 1)
        nt.links.new(d.outputs[0], mx.inputs[6]); nt.links.new(mx.outputs[2], b.inputs["Base Color"])
    else:
        nt.links.new(d.outputs[0], b.inputs["Base Color"])
    nt.links.new(r.outputs[0], b.inputs["Roughness"])
    nm = nt.nodes.new("ShaderNodeNormalMap"); nt.links.new(nr.outputs[0], nm.inputs["Color"]); nt.links.new(nm.outputs[0], b.inputs["Normal"])
    return m

M_BETON = pbr("M_Depo_Beton", "Market", "brushed_concrete_04")
M_BDUVAR = pbr("M_Depo_Siva", "Market", "plastered_wall_04", (0.62, 0.62, 0.6))
M_MOZAIK = pbr("M_Okul_Mozaik", "Koridor", "old_mosaic_floor")
M_SIVA = pbr("M_Okul_Siva", "Koridor", "painted_plaster_wall", (0.78, 0.8, 0.72))
M_DOLAP = pbr("M_Okul_Dolap", "Koridor", "green_metal_rust")
M_KAPI = pbr("M_Okul_Kapi", "Koridor", "black_walnut_veneer_02")
for m in (M_BETON, M_BDUVAR, M_MOZAIK, M_SIVA, M_DOLAP, M_KAPI):
    m.name = m.name  # ad sabit

def kutu(ad, mn, mx, m, olcek=1.0):
    me = bpy.data.meshes.new(ad)
    x0,y0,z0 = mn; x1,y1,z1 = mx
    v = [(x0,y0,z0),(x1,y0,z0),(x1,y1,z0),(x0,y1,z0),(x0,y0,z1),(x1,y0,z1),(x1,y1,z1),(x0,y1,z1)]
    f = [(0,3,2,1),(4,5,6,7),(0,1,5,4),(1,2,6,5),(2,3,7,6),(3,0,4,7)]
    me.from_pydata(v, [], f)
    me.materials.append(m if isinstance(m, bpy.types.Material) else bpy.data.materials[m])
    o = bpy.data.objects.new(ad, me); col.objects.link(o)
    for s in bpy.context.selected_objects: s.select_set(False)
    bpy.context.view_layer.objects.active = o; o.select_set(True)
    bpy.ops.object.mode_set(mode='EDIT'); bpy.ops.mesh.select_all(action='SELECT')
    bpy.ops.uv.cube_project(cube_size=olcek); bpy.ops.object.mode_set(mode='OBJECT'); o.select_set(False)
    return o

# eski tek parca zemin/duvarlari kaldir, bolgelere gore yeniden kur
for n in ("DepoGecit_Zemin", "DepoGecit_Duvar_A", "DepoGecit_Duvar_B", "DepoGecit_Son", "DepoGecit_Tavan"):
    o = bpy.data.objects.get(n)
    if o: bpy.data.objects.remove(o, do_unlink=True)
for o in [o for o in col.objects if o.name.startswith(("Okul_", "Mutfak_"))]:
    bpy.data.objects.remove(o, do_unlink=True)

kutu("Okul_DepoZemin", (X_DIS,YA,-0.1), (XS,YB,0.0), M_BETON, 2.0)
kutu("Okul_DepoDuvar_A", (X_DIS,YA-0.15,0), (XS,YA,HD), M_BDUVAR, 2.0)
kutu("Okul_DepoDuvar_B", (X_DIS,YB,0), (XS,YB+0.15,HD), M_BDUVAR, 2.0)
kutu("Okul_DepoTavan", (X_DIS,YA-0.15,HD), (XS,YB+0.15,HD+0.1), "M_Tavan_Panel")
kutu("Okul_Tavan", (XS,YA-0.15,H), (XE+0.15,YB+0.15,H+0.1), M_SIVA, 2.0)
kutu("Okul_Basamak", (XS-0.15,YA-0.15,HD), (XS,YB+0.15,H), M_BDUVAR, 2.0)   # tavan basamagi
# okul bolgesindeki armaturleri yeni tavana tasi
for o in col.objects:
    if o.name.startswith(("DepoGecit_Armatur", "DepoGecit_Difuzor")) and o.matrix_world.translation.x > XS - 0.7:
        pass
for o in [o for o in col.objects if o.name.startswith(("DepoGecit_Armatur", "DepoGecit_Difuzor"))]:
    if sum((o.matrix_world @ __import__("mathutils").Vector(c)).x for c in o.bound_box) / 8 > XS:
        # mutlak hedefe tasi: betik tekrar calisinca lambalar yukari kaymasin
        dz = (H - 0.08) - min(v.co.z for v in o.data.vertices)
        for v in o.data.vertices: v.co.z += dz
kutu("Okul_Zemin", (XS,YA,-0.1), (XE,YB,0.0), M_MOZAIK, 1.2)
kutu("Okul_Duvar_A", (XS,YA-0.15,0), (XE,YA,H), M_SIVA, 2.0)
kutu("Okul_Duvar_B", (XS,YB,0), (XE,YB+0.15,H), M_SIVA, 2.0)
YO = (Y0 + Y1) / 2
kutu("Okul_SonDuvar_A", (XE,YA-0.15,0), (XE+0.15,YO-0.5,H+0.1), M_SIVA, 2.0)
kutu("Okul_SonDuvar_B", (XE,YO+0.5,0), (XE+0.15,YB+0.15,H+0.1), M_SIVA, 2.0)
kutu("Okul_SonDuvar_Ust", (XE,YO-0.5,2.15), (XE+0.15,YO+0.5,H+0.1), M_SIVA, 2.0)

# duzensiz sinir: mozaik yamalari betona tasar (0.3 m karo parcalari, zeminden 2 mm yukarida)
for i in range(16):
    x = XS - random.uniform(0.15, 1.6) * random.random()
    y = random.uniform(YA, YB - 0.3)
    s = random.choice((0.3, 0.3, 0.6))
    kutu("Okul_Yama_%02d" % i, (x - s, y, 0.0), (x, y + 0.3, 0.002), M_MOZAIK, 1.2)

# dev dolaplar: iki yanda, 0.45 m genis, 3.2 m boy (tavan 2.6'yi deler gibi: ust kisim tavana gomulur)
D, W, HB = 0.42, 0.45, 3.2
x = XS + 1.2
i = 0
while x + W < XE - 1.6:
    for yan, (ya, yb) in (("A", (YA, YA + D)), ("B", (YB - D, YB))):
        ac = random.random() < 0.06           # tek tuk aralik kapak hissi: biraz one cikik
        on = 0.03 if ac else 0.0
        g = kutu(f"Okul_Dolap{yan}_{i:02d}", (x + 0.01, ya, 0.0), (x + W - 0.01, yb, HB), M_DOLAP, 1.0)
        yuz = ya + D if yan == "A" else yb - D
        sgn = 1 if yan == "A" else -1
        k = kutu(f"Okul_DolapKulp{yan}_{i:02d}", (x + W - 0.09, min(yuz, yuz + sgn * (0.025 + on)), 1.05),
                 (x + W - 0.06, max(yuz, yuz + sgn * (0.025 + on)), 1.2), "M_Krom")
    x += W; i += 1

# sinif kapisi (son duvarda, ortada): ceviz kanat, buzlu cam panel, aluminyum kasa; ardinda isik
YO = (Y0 + Y1) / 2
kutu("Okul_SinifKapi_Kasa_A", (XE - 0.04, YO - 0.5, 0), (XE, YO - 0.45, 2.15), "M_Aluminyum")
kutu("Okul_SinifKapi_Kasa_B", (XE - 0.04, YO + 0.45, 0), (XE, YO + 0.5, 2.15), "M_Aluminyum")
kutu("Okul_SinifKapi_Kasa_Ust", (XE - 0.04, YO - 0.5, 2.1), (XE, YO + 0.5, 2.15), "M_Aluminyum")
import math
for o in [o for o in col.objects if o.name.startswith("SinifKapi_Kanat")]:
    bpy.data.objects.remove(o, do_unlink=True)
kb = bpy.data.objects.new("SinifKapi_Kanat", None); col.objects.link(kb)
kb.location = (XE + 0.02, YO - 0.45, 0.0)       # mentese sol kenarda, sinifa (+x) acilir
def kp(ad, y0, z0, y1, z1, m, olc=1.0):
    o = kutu(ad, (-0.02, y0, z0), (0.02, y1, z1), m, olc); o.parent = kb; return o
kp("SinifKapi_Kanat_Alt", 0.0, 0.0, 0.9, 1.1, M_KAPI)
kp("SinifKapi_Kanat_Ust", 0.0, 1.85, 0.9, 2.1, M_KAPI)
kp("SinifKapi_Kanat_YanA", 0.0, 1.1, 0.2, 1.85, M_KAPI)
kp("SinifKapi_Kanat_YanB", 0.7, 1.1, 0.9, 1.85, M_KAPI)
kp("SinifKapi_Kanat_Cam", 0.2, 1.1, 0.7, 1.85, "M_Cam")
k = kutu("SinifKapi_Kanat_Kol", (-0.07, 0.75, 1.0), (-0.02, 0.85, 1.03), "M_Krom"); k.parent = kb

# S1: sinif (8 x 6 m, tavan 3.6); tahta dev (6 x 2.8 m), siralar normal
SX0, SX1, SY0, SY1 = XE + 0.15, XE + 8.15, YO - 3.0, YO + 3.0
M_PARKE = pbr("M_Sinif_Parke", "Market", "laminate_floor_02")
M_SIRA = pbr("M_Sinif_Sira", "Market", "wood_table_001")
M_TAHTA = pbr("M_Sinif_Tahta", "Koridor", "painted_plaster_wall", (0.12, 0.2, 0.15))
kutu("Okul_Sinif_Zemin", (SX0, SY0, -0.1), (SX1, SY1, 0.0), M_PARKE, 1.5)
kutu("Okul_Sinif_Tavan", (SX0, SY0, H), (SX1, SY1, H + 0.1), M_SIVA, 2.0)
kutu("Okul_Sinif_DuvarA", (SX0, SY0 - 0.15, 0), (SX1, SY0, H), M_SIVA, 2.0)
# arka kapi acikligi (sol duvar, sinifin arkasi): x AX0..AX1, 2.1 m
AX0, AX1 = SX0 + 0.85, SX0 + 1.75
kutu("Okul_Sinif_DuvarB_A", (SX0, SY1, 0), (AX0, SY1 + 0.15, H), M_SIVA, 2.0)
kutu("Okul_Sinif_DuvarB_B", (AX1, SY1, 0), (SX1, SY1 + 0.15, H), M_SIVA, 2.0)
kutu("Okul_Sinif_DuvarB_Ust", (AX0, SY1, 2.15), (AX1, SY1 + 0.15, H), M_SIVA, 2.0)
for ad, a, b in (("A", (AX0 - 0.05, SY1 - 0.04, 0), (AX0, SY1, 2.15)), ("B", (AX1, SY1 - 0.04, 0), (AX1 + 0.05, SY1, 2.15)),
                 ("Ust", (AX0 - 0.05, SY1 - 0.04, 2.1), (AX1 + 0.05, SY1, 2.15))):
    kutu("Okul_ArkaKapi_Kasa_" + ad, a, b, "M_Aluminyum")
for o in [o for o in col.objects if o.name.startswith("ArkaKapi_Kanat")]:
    bpy.data.objects.remove(o, do_unlink=True)
ak = bpy.data.objects.new("ArkaKapi_Kanat", None); col.objects.link(ak)
ak.location = (AX0, SY1 + 0.02, 0.0)            # mentese x=AX0, mutfaga (+y) acilir
def ap(ad, x0, z0, x1, z1, m):
    o = kutu(ad, (x0, -0.02, z0), (x1, 0.02, z1), m, 1.0); o.parent = ak; return o
ap("ArkaKapi_Kanat_Alt", 0.0, 0.03, 0.9, 1.1, M_KAPI)
ap("ArkaKapi_Kanat_Ust", 0.0, 1.85, 0.9, 2.1, M_KAPI)
ap("ArkaKapi_Kanat_YanA", 0.0, 1.1, 0.2, 1.85, M_KAPI)
ap("ArkaKapi_Kanat_YanB", 0.7, 1.1, 0.9, 1.85, M_KAPI)
ap("ArkaKapi_Kanat_Cam", 0.2, 1.1, 0.7, 1.85, "M_Cam")
k = kutu("ArkaKapi_Kanat_Kol", (0.75, -0.07, 1.0), (0.85, -0.02, 1.03), "M_Krom"); k.parent = ak
kutu("Okul_Sinif_DuvarSon", (SX1, SY0 - 0.15, 0), (SX1 + 0.15, SY1 + 0.15, H), M_SIVA, 2.0)
kutu("Okul_Sinif_DuvarOn_A", (SX0 - 0.15, SY0 - 0.15, 0), (SX0, YA - 0.15, H), M_SIVA, 2.0)
kutu("Okul_Sinif_DuvarOn_B", (SX0 - 0.15, YB + 0.15, 0), (SX0, SY1 + 0.15, H), M_SIVA, 2.0)
kutu("Okul_Sinif_Tahta", (SX1 - 0.04, YO - 3.0 + 0.1, 0.6), (SX1, YO + 3.0 - 0.1, 3.4), M_TAHTA, 2.0)
kutu("Okul_Sinif_TahtaCerceve", (SX1 - 0.06, YO - 2.95, 0.55), (SX1 - 0.04, YO + 2.95, 0.6), "M_Aluminyum")
kutu("Okul_Sinif_TahtaArmatur", (SX1 - 0.5, YO - 2.0, 3.45), (SX1 - 0.3, YO + 2.0, 3.55), "M_Armatur_Kasa")
kutu("Okul_Sinif_TahtaDifuzor", (SX1 - 0.48, YO - 1.97, 3.445), (SX1 - 0.32, YO + 1.97, 3.45), "M_Armatur_Difuzor_0")
# sira uzerinde iki sira tavan floresani: sinif gorunur olsun (isik Unity'de)
for i, ax in enumerate((SX0 + 2.0, SX0 + 5.0)):
    for j, ay in enumerate((YO - 1.5, YO + 1.5)):
        kutu("Okul_Sinif_Armatur_%d%d" % (i, j), (ax - 0.6, ay - 0.12, H - 0.08), (ax + 0.6, ay + 0.12, H), "M_Armatur_Kasa")
        kutu("Okul_Sinif_Difuzor_%d%d" % (i, j), (ax - 0.57, ay - 0.09, H - 0.085), (ax + 0.57, ay + 0.09, H - 0.08), "M_Armatur_Difuzor_0")
for r in range(4):
    for c in range(3):
        x = SX0 + 1.6 + r * 1.3; y = YO - 2.0 + c * 2.0
        kutu(f"Okul_Sira_{r}{c}_Ust", (x, y - 0.6, 0.72), (x + 0.5, y + 0.6, 0.75), M_SIRA, 1.0)
        for dx in (0.03, 0.44):
            for dy in (-0.57, 0.54):
                kutu(f"Okul_Sira_{r}{c}_Ayak{dx}{dy}", (x + dx, y + dy, 0), (x + dx + 0.03, y + dy + 0.03, 0.72), "M_Aluminyum")
# kapinin ardinda mutfaga acilan kisa sicak gecit (mutfak bolgesi sonraki is)
M_MFAYANS = pbr("M_Mutfak_Fayans", "Market", "long_white_tiles")
M_MDUVAR = pbr("M_Mutfak_Duvar", "Koridor", "beige_wall_001")
# M1: Hakan'larin mutfagi (5 x 4.5 m, tavan 2.6). Masa dev (aile sofrasi: hafizada agir yer), gerisi normal.
# Karsi duvarda pencere: gece sokagi (23:42) sonraki bolge.
KY0 = SY1 + 0.15
MX0, MX1, MY0, MY1, MH = AX0 - 1.6, AX0 + 3.4, KY0, KY0 + 4.5, 2.6
M_MASA = pbr("M_Mutfak_Masa", "Market", "lacquered_cherry_wood")
kutu("Mutfak_Zemin", (MX0, MY0, -0.1), (MX1, MY1, 0.0), M_MFAYANS, 0.6)
kutu("Mutfak_Tavan", (MX0 - 0.15, MY0, MH), (MX1 + 0.15, MY1 + 0.15, MH + 0.1), M_MDUVAR, 2.0)
kutu("Mutfak_DuvarA", (MX0 - 0.15, MY0, 0), (MX0, MY1 + 0.15, MH), M_MDUVAR, 2.0)
kutu("Mutfak_DuvarB", (MX1, MY0, 0), (MX1 + 0.15, MY1 + 0.15, MH), M_MDUVAR, 2.0)
kutu("Mutfak_DuvarOnA", (MX0, MY0 - 0.05, 0), (AX0, MY0 + 0.01, MH), M_MDUVAR, 2.0)
kutu("Mutfak_DuvarOnB", (AX1, MY0 - 0.05, 0), (MX1, MY0 + 0.01, MH), M_MDUVAR, 2.0)
kutu("Mutfak_DuvarOnUst", (AX0, MY0 - 0.05, 2.15), (AX1, MY0 + 0.01, MH), M_MDUVAR, 2.0)
# arka duvar pencereli (pencere x PX0..PX1, z 0.95..2.05)
PX0, PX1 = MX0 + 1.9, MX0 + 3.1
kutu("Mutfak_ArkaA", (MX0, MY1, 0), (PX0, MY1 + 0.15, MH), M_MDUVAR, 2.0)
kutu("Mutfak_ArkaB", (PX1, MY1, 0), (MX1, MY1 + 0.15, MH), M_MDUVAR, 2.0)
kutu("Mutfak_ArkaAlt", (PX0, MY1, 0), (PX1, MY1 + 0.15, 0.95), M_MDUVAR, 2.0)
kutu("Mutfak_ArkaUst", (PX0, MY1, 2.05), (PX1, MY1 + 0.15, MH), M_MDUVAR, 2.0)
for ad, a_, b_ in (("Alt", (PX0, MY1 - 0.02, 0.92), (PX1, MY1 + 0.17, 0.95)), ("Ust", (PX0, MY1 - 0.02, 2.05), (PX1, MY1 + 0.17, 2.08)),
                   ("SolK", (PX0, MY1 - 0.02, 0.95), (PX0 + 0.04, MY1 + 0.17, 2.05)), ("SagK", (PX1 - 0.04, MY1 - 0.02, 0.95), (PX1, MY1 + 0.17, 2.05)),
                   ("Orta", ((PX0 + PX1) / 2 - 0.02, MY1 - 0.02, 0.95), ((PX0 + PX1) / 2 + 0.02, MY1 + 0.17, 2.05))):
    kutu("Mutfak_Pencere_" + ad, a_, b_, "M_Aluminyum")
kutu("Mutfak_Pencere_Cam", (PX0 + 0.04, MY1 + 0.07, 0.95), (PX1 - 0.04, MY1 + 0.08, 2.05), "M_Cam")
# tezgah (sol duvar boyunca) + ust dolaplar + buzdolabi
kutu("Mutfak_Tezgah_Govde", (MX0, MY0 + 0.6, 0), (MX0 + 0.6, MY1 - 0.1, 0.86), M_MASA, 1.0)
kutu("Mutfak_Tezgah_Ust", (MX0, MY0 + 0.55, 0.86), (MX0 + 0.63, MY1 - 0.05, 0.9), "M_Tezgah_Ust", 1.0)
kutu("Mutfak_UstDolap", (MX0, MY0 + 0.6, 1.5), (MX0 + 0.35, MY1 - 0.1, 2.2), M_MASA, 1.0)
kutu("Mutfak_Buzdolabi", (MX1 - 0.7, MY1 - 0.75, 0), (MX1 - 0.02, MY1 - 0.05, 1.85), "M_Raf_Boyali", 1.0)
kutu("Mutfak_Buzdolabi_Kulp", (MX1 - 0.74, MY1 - 0.2, 0.9), (MX1 - 0.7, MY1 - 0.17, 1.5), "M_Krom")
# dev aile masasi: normalde 0.75 m, burada 1.25 m; ayaklar kalin
TX0, TX1, TY0, TY1, TH = MX0 + 1.4, MX0 + 3.6, MY0 + 1.5, MY0 + 3.1, 1.25
kutu("Mutfak_Masa_Ust", (TX0, TY0, TH - 0.06), (TX1, TY1, TH), M_MASA, 1.0)
for i, (x, y) in enumerate(((TX0 + 0.05, TY0 + 0.05), (TX1 - 0.13, TY0 + 0.05), (TX0 + 0.05, TY1 - 0.13), (TX1 - 0.13, TY1 - 0.13))):
    kutu("Mutfak_Masa_Ayak%d" % i, (x, y, 0), (x + 0.08, y + 0.08, TH - 0.06), M_MASA, 1.0)
# iki sandalye (normal boy: masanin altinda kalir, cocuk gibi hissettirir)
for i, x in enumerate((TX0 + 0.4, TX1 - 0.85)):
    y = TY0 - 0.5
    kutu("Mutfak_Sandalye%d_Oturak" % i, (x, y, 0.44), (x + 0.45, y + 0.45, 0.48), M_MASA, 1.0)
    kutu("Mutfak_Sandalye%d_Arka" % i, (x, y - 0.03, 0.48), (x + 0.45, y, 0.95), M_MASA, 1.0)
    for j, (dx, dy) in enumerate(((0, 0), (0.41, 0), (0, 0.41), (0.41, 0.41))):
        kutu("Mutfak_Sandalye%d_Ayak%d" % (i, j), (x + dx, y + dy, 0), (x + dx + 0.04, y + dy + 0.04, 0.44), "M_Aluminyum")
kutu("Mutfak_Lamba", ((TX0 + TX1) / 2 - 0.2, (TY0 + TY1) / 2 - 0.2, 2.45), ((TX0 + TX1) / 2 + 0.2, (TY0 + TY1) / 2 + 0.2, 2.55), "M_Armatur_Difuzor_3")
# Depo kolileri duvar diplerine: ortada 1.5 m gecit acik kalsin (rastgele yerlesim koridoru tikiyordu)
for o in [o for o in col.objects if o.name.startswith("DepoGecit_Koli_")]:
    bpy.data.objects.remove(o, do_unlink=True)
for i, (x, duvar, boy) in enumerate(((6.7, 0, 2), (7.5, 1, 1), (8.2, 0, 1), (8.8, 1, 3), (9.4, 0, 3), (6.6, 1, 2))):
    ya0 = YA + 0.02 if duvar == 0 else YB - 0.5
    z = 0.0
    for k in range(boy):
        h = (0.42, 0.34, 0.38)[k]; dx = (0.0, 0.04, -0.03)[k]
        kutu("DepoGecit_Koli_%d_%d" % (i, k), (x + dx, ya0, z), (x + dx + 0.52, ya0 + 0.48, z + h), "M_Koli_Karton")
        z += h
print("okul koridoru + sinif + mutfak kuruldu")
