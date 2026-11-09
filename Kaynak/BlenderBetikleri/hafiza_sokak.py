# Hafiza M3a: mutfak penceresinin ardindaki gece sokagi blokaji. hafiza_okul_koridor.py'den SONRA calistir.
# Eksen: Blender (x, y) = Unity (x, z). Tum alan MarketNoExit yaricapinin icinde kalir.
import bpy, os

TEX = r"C:\Kisisel Dosyalarim\ozel\Oyun Gelistirme\Denemelik projelerim\Dink\Assets\_Project\Textures\Prolog"
col = bpy.data.collections["Market_Depo"]
X0, X1 = 17.0, 29.0
DUVAR = 9.65                 # mutfak arka duvarinin dis yuzu
PX0, PX1 = 25.45, 26.65      # pencere
Y_KALD, Y_YOL, Y_KARSI, Y_CEPHE = 11.2, 17.5, 19.0, 19.0
H = 7.0

for o in [o for o in col.objects if o.name.startswith(("Sokak_", "SokakKapi_"))]:
    bpy.data.objects.remove(o, do_unlink=True)

# oyuncu pencereden gecer: cam ve orta kayit kalkar
for n in ("Mutfak_Pencere_Cam", "Mutfak_Pencere_Orta"):
    o = bpy.data.objects.get(n)
    if o: bpy.data.objects.remove(o, do_unlink=True)

def pbr(ad, isim, ton=None):
    m = bpy.data.materials.get(ad)
    if m: bpy.data.materials.remove(m)
    m = bpy.data.materials.new(ad); m.use_nodes = True
    nt = m.node_tree; b = next(n for n in nt.nodes if n.type == "BSDF_PRINCIPLED")
    d = nt.nodes.new("ShaderNodeTexImage")
    d.image = bpy.data.images.load(os.path.join(TEX, isim + "_Diff.jpg"), check_existing=True)
    if ton:
        mx = nt.nodes.new("ShaderNodeMix"); mx.data_type = "RGBA"; mx.blend_type = "MULTIPLY"
        mx.inputs[0].default_value = 1; mx.inputs[7].default_value = (*ton, 1)
        nt.links.new(d.outputs[0], mx.inputs[6]); nt.links.new(mx.outputs[2], b.inputs["Base Color"])
    else:
        nt.links.new(d.outputs[0], b.inputs["Base Color"])
    n = nt.nodes.new("ShaderNodeTexImage")
    n.image = bpy.data.images.load(os.path.join(TEX, isim + "_Nor.jpg"), check_existing=True)
    n.image.colorspace_settings.name = "Non-Color"
    nm = nt.nodes.new("ShaderNodeNormalMap"); nt.links.new(n.outputs[0], nm.inputs["Color"]); nt.links.new(nm.outputs[0], b.inputs["Normal"])
    b.inputs["Roughness"].default_value = 0.85
    return m

M_YOL = pbr("M_Sokak_Asfalt", "asphalt_01")
M_KALD = pbr("M_Sokak_Kaldirim", "gravel_concrete_03", (0.75, 0.75, 0.72))
M_CEPHE = pbr("M_Sokak_Cephe", "painted_plaster_wall", (0.55, 0.55, 0.5))
M_BLOK = pbr("M_Sokak_Blok", "medieval_blocks_06", (0.6, 0.58, 0.55))

M_KARA = bpy.data.materials.get("M_Sokak_Kara") or bpy.data.materials.new("M_Sokak_Kara")
M_KARA.use_nodes = True
next(n for n in M_KARA.node_tree.nodes if n.type == "BSDF_PRINCIPLED").inputs["Base Color"].default_value = (0.012, 0.014, 0.02, 1)

def kutu(ad, mn, mx, m, olcek=2.0):
    me = bpy.data.meshes.new(ad)
    x0, y0, z0 = mn; x1, y1, z1 = mx
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

# zemin: kaldirim 12 cm yuksek, yol asagida degil oyuncu basamagi cikabilsin diye rampa yok, step offset yeter
kutu("Sokak_Yol", (X0, Y_KALD, -0.1), (X1, Y_YOL, 0.0), M_YOL, 3.0)
kutu("Sokak_Kaldirim", (X0, DUVAR, -0.1), (X1, Y_KALD, 0.12), M_KALD, 2.0)
kutu("Sokak_KarsiKaldirim", (X0, Y_YOL, -0.1), (X1, Y_CEPHE, 0.12), M_KALD, 2.0)
# bizim bina cephesi: mutfagin iki yani ve ustu, pencere cevresi kaplama
kutu("Sokak_CepheSol", (X0, DUVAR - 0.15, 0), (23.4, DUVAR + 0.01, H), M_CEPHE, 3.0)
kutu("Sokak_CepheSag", (28.7, DUVAR - 0.15, 0), (X1, DUVAR + 0.01, H), M_CEPHE, 3.0)
kutu("Sokak_CepheUst", (23.4, DUVAR - 0.15, 2.75), (28.7, DUVAR + 0.01, H), M_CEPHE, 3.0)
kutu("Sokak_KaplamaSol", (23.4, DUVAR, 0), (PX0, DUVAR + 0.01, 2.75), M_CEPHE, 3.0)
kutu("Sokak_KaplamaSag", (PX1, DUVAR, 0), (28.7, DUVAR + 0.01, 2.75), M_CEPHE, 3.0)
kutu("Sokak_KaplamaAlt", (PX0, DUVAR, 0), (PX1, DUVAR + 0.01, 0.95), M_CEPHE, 3.0)
kutu("Sokak_KaplamaUst", (PX0, DUVAR, 2.05), (PX1, DUVAR + 0.01, 2.75), M_CEPHE, 3.0)
# karsi cephe ve iki ucu kapatan yuksek duvarlar
# karsi cephe giris kapisinda delikli (kapi x 19.2..20.8, z 0..2.4)
kutu("Sokak_KarsiCephe_A", (X0, Y_CEPHE, 0), (19.2, Y_CEPHE + 0.2, H), M_CEPHE, 3.0)
kutu("Sokak_KarsiCephe_B", (20.8, Y_CEPHE, 0), (X1, Y_CEPHE + 0.2, H), M_CEPHE, 3.0)
kutu("Sokak_KarsiCephe_Ust", (19.2, Y_CEPHE, 2.4), (20.8, Y_CEPHE + 0.2, H), M_CEPHE, 3.0)
kutu("Sokak_UcBati", (X0 - 0.2, DUVAR, 0), (X0, Y_CEPHE, H), M_CEPHE, 3.0)
kutu("Sokak_UcDogu", (X1, DUVAR, 0), (X1 + 0.2, Y_CEPHE, H), M_CEPHE, 3.0)
# apartman cephesi: giris kapisi ve kat pencereleri (yanan pencere Unity'de)
Y = Y_CEPHE
M_KAPI = pbr("M_Sokak_Kapi", "wooden_garage_door", (0.5, 0.42, 0.36))
kutu("Sokak_GirisKasaSol", (19.1, Y - 0.08, 0.12), (19.2, Y + 0.2, 2.5), "M_Aluminyum")
kutu("Sokak_GirisKasaSag", (20.8, Y - 0.08, 0.12), (20.9, Y + 0.2, 2.5), "M_Aluminyum")
kutu("Sokak_GirisKasaUst", (19.1, Y - 0.08, 2.4), (20.9, Y + 0.2, 2.5), "M_Aluminyum")
# M5: kanat ayri nesne, koken mentesede (x 19.2), iceri (+y) acilir; Unity HafizaSokakKapi.fbx
kk = bpy.data.objects.new("SokakKapi_Kanat", None); col.objects.link(kk)
kk.location = (19.2, Y + 0.1, 0.12)
def kp(ad, x0, z0, x1, z1, m, y0=-0.025, y1=0.025):
    o = kutu(ad, (x0, y0, z0), (x1, y1, z1), m, 2.0); o.parent = kk; return o
kp("Sokak_Kanat_Govde", 0.01, 0.0, 1.59, 2.27, bpy.data.materials["M_Okul_Kapi"])
kp("Sokak_Kanat_Kol", 1.38, 1.0, 1.48, 1.04, "M_Krom", -0.075, -0.025)
# kapinin ardinda giris holu; M6: hol arkasindan yukari merdiven, ustte sahanlik ve Daire 11 kapisi
M_HZEMIN = bpy.data.materials["M_Okul_Mozaik"]
M_HDUVAR = bpy.data.materials["M_Okul_Siva"]
YS, ADIM, YUK, BASAMAK = Y + 1.0, 0.225, 0.17, 16          # merdiven y 20.0'dan baslar
YL = YS + ADIM * BASAMAK                                   # sahanlik basi (23.6)
ZL = 0.12 + YUK * BASAMAK                                  # sahanlik zemini (2.84)
YA = YL + 1.6                                              # arka duvar ic yuzu (25.2)
kutu("Sokak_Hol_Zemin", (18.75, Y + 0.2, -0.1), (21.25, YL, 0.12), M_HZEMIN, 2.0)
kutu("Sokak_Hol_Tavan", (18.75, Y + 0.2, 2.75), (21.25, YS, 2.85), M_HDUVAR, 2.0)
kutu("Sokak_Hol_TavanUst", (18.75, Y + 0.2, ZL + 2.75), (21.25, YA + 0.15, ZL + 2.85), M_HDUVAR, 2.0)
kutu("Sokak_Hol_DuvarBati", (18.75, Y + 0.2, 0), (18.9, YA + 0.15, ZL + 2.85), M_HDUVAR, 2.0)
kutu("Sokak_Hol_DuvarDogu", (21.1, Y + 0.2, 0), (21.25, YA + 0.15, ZL + 2.85), M_HDUVAR, 2.0)
kutu("Sokak_Hol_DuvarArka", (18.75, YA, 0), (21.25, YA + 0.15, ZL + 2.85), M_HDUVAR, 2.0)
for i in range(BASAMAK):
    kutu("Sokak_Basamak_%02d" % i, (18.9, YS + i * ADIM, 0.12), (21.1, YS + (i + 1) * ADIM, 0.12 + (i + 1) * YUK), M_HZEMIN, 1.0)
kutu("Sokak_Sahanlik", (18.9, YL, 0.12), (21.1, YA, ZL), M_HZEMIN, 2.0)
# Daire 11 kapisi (simdilik kapali, M7'de acilir): kasa + kanat, numara Unity'de
kutu("Sokak_D11_KasaSol", (19.45, YA - 0.06, ZL), (19.55, YA, ZL + 2.15), "M_Aluminyum")
kutu("Sokak_D11_KasaSag", (20.45, YA - 0.06, ZL), (20.55, YA, ZL + 2.15), "M_Aluminyum")
kutu("Sokak_D11_KasaUst", (19.45, YA - 0.06, ZL + 2.05), (20.55, YA, ZL + 2.15), "M_Aluminyum")
kutu("Sokak_D11_Kanat", (19.55, YA - 0.05, ZL), (20.45, YA - 0.01, ZL + 2.05), bpy.data.materials["M_Okul_Kapi"], 2.0)
kutu("Sokak_D11_Kol", (20.3, YA - 0.1, ZL + 1.0), (20.4, YA - 0.05, ZL + 1.04), "M_Krom")
kutu("Sokak_D11_Paspas", (19.5, YA - 0.75, ZL), (20.5, YA - 0.1, ZL + 0.015), M_KALD, 1.0)
kutu("Sokak_ZilPlaka", (21.1, Y - 0.03, 1.05), (21.5, Y, 1.75), "M_Aluminyum")
kutu("Sokak_GirisSacak", (18.8, Y - 0.6, 2.6), (21.2, Y, 2.72), M_CEPHE, 3.0)
for kat, z in enumerate((3.6, 5.6)):
    for i, x in enumerate((19.4, 22.4, 25.4, 27.6)):
        kutu(f"Sokak_Pencere_{kat}_{i}", (x - 0.55, Y - 0.04, z - 0.65), (x + 0.55, Y, z + 0.65), "M_Aluminyum")
        kutu(f"Sokak_PencereCam_{kat}_{i}", (x - 0.48, Y - 0.05, z - 0.58), (x + 0.48, Y - 0.04, z + 0.58), M_KARA)
# sokak lambasi: karsi kaldirimda direk ve armatur (isik Unity'de)
kutu("Sokak_Direk", (22.94, 18.1, 0.12), (23.06, 18.22, 4.6), "M_Aluminyum")
kutu("Sokak_Kol", (22.97, 16.9, 4.5), (23.03, 18.2, 4.58), "M_Aluminyum")
kutu("Sokak_Armatur", (22.86, 16.6, 4.42), (23.14, 17.1, 4.54), "M_Aluminyum")
print("sokak tamam")
