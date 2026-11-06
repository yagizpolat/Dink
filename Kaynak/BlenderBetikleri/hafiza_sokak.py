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

for o in [o for o in col.objects if o.name.startswith("Sokak_")]:
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
kutu("Sokak_KarsiCephe", (X0, Y_CEPHE, 0), (X1, Y_CEPHE + 0.2, H), M_BLOK, 3.0)
kutu("Sokak_UcBati", (X0 - 0.2, DUVAR, 0), (X0, Y_CEPHE, H), M_BLOK, 3.0)
kutu("Sokak_UcDogu", (X1, DUVAR, 0), (X1 + 0.2, Y_CEPHE, H), M_BLOK, 3.0)
# sokak lambasi: karsi kaldirimda direk ve armatur (isik Unity'de)
kutu("Sokak_Direk", (22.94, 18.1, 0.12), (23.06, 18.22, 4.6), "M_Aluminyum")
kutu("Sokak_Kol", (22.97, 16.9, 4.5), (23.03, 18.2, 4.58), "M_Aluminyum")
kutu("Sokak_Armatur", (22.86, 16.6, 4.42), (23.14, 17.1, 4.54), "M_Aluminyum")
print("sokak tamam")
