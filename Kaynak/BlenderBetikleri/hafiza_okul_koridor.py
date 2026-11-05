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
for o in [o for o in col.objects if o.name.startswith("Okul_")]:
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
        for v in o.data.vertices: v.co.z += H - HD
kutu("Okul_Zemin", (XS,YA,-0.1), (XE,YB,0.0), M_MOZAIK, 1.2)
kutu("Okul_Duvar_A", (XS,YA-0.15,0), (XE,YA,H), M_SIVA, 2.0)
kutu("Okul_Duvar_B", (XS,YB,0), (XE,YB+0.15,H), M_SIVA, 2.0)
kutu("Okul_SonDuvar", (XE,YA-0.15,0), (XE+0.15,YB+0.15,H+0.1), M_SIVA, 2.0)

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
kutu("Okul_SinifKapi_Alt", (XE - 0.03, YO - 0.45, 0), (XE - 0.01, YO + 0.45, 1.1), M_KAPI, 1.0)
kutu("Okul_SinifKapi_Ust", (XE - 0.03, YO - 0.45, 1.85), (XE - 0.01, YO + 0.45, 2.1), M_KAPI, 1.0)
kutu("Okul_SinifKapi_YanA", (XE - 0.03, YO - 0.45, 1.1), (XE - 0.01, YO - 0.25, 1.85), M_KAPI, 1.0)
kutu("Okul_SinifKapi_YanB", (XE - 0.03, YO + 0.25, 1.1), (XE - 0.01, YO + 0.45, 1.85), M_KAPI, 1.0)
kutu("Okul_SinifKapi_Cam", (XE - 0.022, YO - 0.25, 1.1), (XE - 0.018, YO + 0.25, 1.85), "M_Armatur_Difuzor_3", 1.0)
kutu("Okul_SinifKapi_Kol", (XE - 0.08, YO + 0.3, 1.0), (XE - 0.03, YO + 0.4, 1.03), "M_Krom")
print("okul koridoru kuruldu, dolap sayisi", i * 2)
