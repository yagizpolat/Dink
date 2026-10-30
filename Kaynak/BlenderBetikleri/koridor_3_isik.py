# Karanfil Apartmanı kat koridoru 3: ışık. Tavanda opal cam glob armatürler (sıcak 2700K akkor),
# merdiven boşluğunda duvar apliği, pencerede gece. Biri hafif kısık: eski ampul.
import bpy, bmesh, math
exec(open(r"C:\Kisisel Dosyalarim\ozel\Oyun Gelistirme\Denemelik projelerim\Dink\Kaynak\BlenderBetikleri\market_ortak.py", encoding="utf-8").read())
DOKU = r"C:\Kisisel Dosyalarim\ozel\Oyun Gelistirme\Denemelik projelerim\Dink\Kaynak\Blender\Dokular\Koridor"
from mathutils import Vector

K = "Koridor_Isik"
for o in list(koleksiyon(K).objects):
    bpy.data.objects.remove(o, do_unlink=True)

H = 2.75
SICAK = (1.0, 0.72, 0.45)  # ~2700K
taban = detay("M_K_Armatur_Taban", "detay_plastik", (0.85, 0.83, 0.78), pmin=0.4, pmax=0.6, normal_guc=0.3, ton=0.06)
pirinc = detay("M_K_Pirinc", "detay_cizik", (0.78, 0.56, 0.26), metal=1.0, pmin=0.25, pmax=0.5, normal_guc=0.15, ton=0.08)


def glob_cam(guc):
    """Opal cam: içten yanan süt camı. Işık rengi kenara doğru düşer (kalın cam), merkez daha parlak."""
    ad = f"M_K_Opal_{guc:.1f}"
    m = bpy.data.materials.get(ad) or bpy.data.materials.new(ad)
    m.use_nodes = True
    nt = m.node_tree; nt.nodes.clear()
    out = nt.nodes.new("ShaderNodeOutputMaterial")
    b = nt.nodes.new("ShaderNodeBsdfPrincipled")
    b.inputs["Base Color"].default_value = (0.95, 0.93, 0.88, 1)
    b.inputs["Roughness"].default_value = 0.15
    b.inputs["Emission Color"].default_value = (*SICAK, 1)
    lw = nt.nodes.new("ShaderNodeLayerWeight"); lw.inputs[0].default_value = 0.45
    mr = nt.nodes.new("ShaderNodeMapRange"); mr.inputs["To Min"].default_value = guc; mr.inputs["To Max"].default_value = guc * 0.35
    nt.links.new(lw.outputs["Facing"], mr.inputs["Value"]); nt.links.new(mr.outputs[0], b.inputs["Emission Strength"])
    nt.links.new(b.outputs[0], out.inputs[0])
    return m


def glob(ad, x, y, guc=6.0, watt=160):
    """Tavana vidalı taban + pirinç bilezik + opal küre; içinde nokta ışık."""
    kutu(ad + "_Taban", (x, y, H - 0.015), (0.16, 0.16, 0.03), taban, K)
    me = bpy.data.meshes.new(ad + "_Bilezik"); bm = bmesh.new()
    bmesh.ops.create_cone(bm, cap_ends=True, segments=32, radius1=0.075, radius2=0.075, depth=0.03)
    bm.to_mesh(me); bm.free()
    o = bpy.data.objects.new(ad + "_Bilezik", me); o.location = (x, y, H - 0.045); koleksiyon(K).objects.link(o)
    me.materials.append(pirinc)
    bpy.context.view_layer.update(); malz_uv(o)
    me = bpy.data.meshes.new(ad + "_Kure"); bm = bmesh.new()
    bmesh.ops.create_uvsphere(bm, u_segments=32, v_segments=16, radius=0.13)
    bm.to_mesh(me); bm.free()
    for p in me.polygons: p.use_smooth = True
    o = bpy.data.objects.new(ad + "_Kure", me); o.location = (x, y, H - 0.19); koleksiyon(K).objects.link(o)
    o.visible_shadow = False  # ampul kürenin içinde: gölge düşürürse ışığı hapseder
    me.materials.append(glob_cam(guc))
    ld = bpy.data.lights.new(ad + "_Isik", "POINT"); ld.energy = watt; ld.color = SICAK; ld.shadow_soft_size = 0.12
    lo = bpy.data.objects.new(ad + "_Isik", ld); lo.location = (x, y, H - 0.19); koleksiyon(K).objects.link(lo)


glob("Lamba_Lobi", 0.95, 1.7)
glob("Lamba_Koridor1", 0.95, 5.7)
glob("Lamba_Koridor2", 0.95, 8.7, guc=3.5, watt=80)   # eski ampul: kısık, biraz daha turuncu
bpy.data.lights["Lamba_Koridor2_Isik"].color = (1.0, 0.62, 0.35)
glob("Lamba_Koridor3", 0.95, 11.2)
for i, x in enumerate((-1.8, -4.3, -6.8)):   # L kolu
    glob(f"Lamba_Kol{i + 1}", x, 11.05)

# Merdiven boşluğu: dış duvarda, sahanlık hizasında aplik (aşağı ve yukarı kolu aydınlatır)
x, y, z = 4.15, -0.4, 2.9
kutu("Aplik_Taban", (x - 0.01, y, z), (0.02, 0.14, 0.14), taban, K)
me = bpy.data.meshes.new("Aplik_Kure"); bm = bmesh.new()
bmesh.ops.create_uvsphere(bm, u_segments=32, v_segments=16, radius=0.1)
bm.to_mesh(me); bm.free()
for p in me.polygons: p.use_smooth = True
o = bpy.data.objects.new("Aplik_Kure", me); o.location = (x - 0.11, y, z); koleksiyon(K).objects.link(o)
o.visible_shadow = False
me.materials.append(glob_cam(5.0))
ld = bpy.data.lights.new("Aplik_Isik", "POINT"); ld.energy = 130; ld.color = SICAK; ld.shadow_soft_size = 0.1
lo = bpy.data.objects.new("Aplik_Isik", ld); lo.location = (x - 0.11, y, z); koleksiyon(K).objects.link(lo)
# Alt kat sahanlığına yukarıdan sızan zayıf ışık (alt katın lambası)
ld = bpy.data.lights.new("AltKat_Sizinti", "POINT"); ld.energy = 40; ld.color = SICAK; ld.shadow_soft_size = 0.2
lo = bpy.data.objects.new("AltKat_Sizinti", ld); lo.location = (3.5, -0.6, -0.3); koleksiyon(K).objects.link(lo)

# Ortam dolgusu: lobiye merdivenden ve koridor ucundan sekip gelen ışık (çok zayıf, gölgesiz)
ld = bpy.data.lights.new("Sekme", "AREA"); ld.shape = "RECTANGLE"; ld.size = 1.8; ld.size_y = 11.5; ld.energy = 90
ld.color = (1.0, 0.85, 0.7)
lo = bpy.data.objects.new("Sekme", ld); lo.location = (0.95, 6.0, 0.05); lo.rotation_euler = (math.pi, 0, 0)
koleksiyon(K).objects.link(lo)
ld.use_shadow = False
ld = bpy.data.lights.new("Sekme_Kol", "AREA"); ld.shape = "RECTANGLE"; ld.size = 8.0; ld.size_y = 1.8; ld.energy = 60
ld.color = (1.0, 0.85, 0.7); ld.use_shadow = False
lo = bpy.data.objects.new("Sekme_Kol", ld); lo.location = (-4.0, 11.05, 0.05); lo.rotation_euler = (math.pi, 0, 0)
koleksiyon(K).objects.link(lo)

# Dünya: neredeyse siyah (gece), pencerenin dışı ayrı (koridor_4)
w = bpy.context.scene.world
w.use_nodes = True
bg = next(n for n in w.node_tree.nodes if n.type == "BACKGROUND")
bg.inputs["Color"].default_value = (0.004, 0.005, 0.008, 1); bg.inputs["Strength"].default_value = 1.0
print("koridor ışık tamam", len(koleksiyon(K).objects))
