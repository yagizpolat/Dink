# Market odası 4: floresan armatürler (asma tavanda 60x120 paneller), raf fiyat etiketleri, dolap camı.
import bpy, math, random
exec(open(r"C:\Kisisel Dosyalarim\ozel\Oyun Gelistirme\Denemelik projelerim\Dink\Kaynak\BlenderBetikleri\market_ortak.py", encoding="utf-8").read())

K = "Market_Isik"
for o in list(koleksiyon(K).objects):
    bpy.data.objects.remove(o, do_unlink=True)
for o in list(bpy.data.objects):
    if o.name.startswith("Gecici_"):
        bpy.data.objects.remove(o, do_unlink=True)
random.seed(7)

# Armatür: alüminyum kasa + opal difüzör (ışık veren yüzey). Biri hafif sönük: eskimiş tüp.
kasa = duz("M_Armatur_Kasa", (0.8, 0.8, 0.78), puruz=0.35, metal=1.0)
H = 2.9
yerler = [(1.5, 1.2), (4.5, 1.2), (1.5, 3.0), (4.5, 3.0), (3.0, 2.1)]
for i, (x, y) in enumerate(yerler):
    guc = 9.0 if i != 3 else 3.5
    dif = duz(f"M_Armatur_Difuzor_{i}", (0.92, 0.95, 1.0), puruz=0.6, isik=guc, isik_renk=(0.88, 0.94, 1.0))
    kutu(f"Armatur_{i}_Kasa", (x, y, H - 0.02), (0.62, 1.22, 0.04), kasa, K, uv=False)
    kutu(f"Armatur_{i}_Difuzor", (x, y, H - 0.042), (0.58, 1.18, 0.004), dif, K, uv=False)
    ld = bpy.data.lights.new(f"Armatur_{i}_Isik", "AREA")
    ld.shape = "RECTANGLE"; ld.size = 0.58; ld.size_y = 1.18
    ld.energy = 260 if i != 3 else 90
    ld.color = (0.88, 0.94, 1.0)  # ~5000K soğuk beyaz
    lo = bpy.data.objects.new(f"Armatur_{i}_Isik", ld)
    lo.location = (x, y, H - 0.05)
    koleksiyon(K).objects.link(lo)

# Tavana sekme ışığı: zayıf, yukarı bakan geniş alan ışığı (gerçekte zeminden yansıyan ışık)
ld = bpy.data.lights.new("Tavan_Sekme", "AREA"); ld.shape = "RECTANGLE"; ld.size = 5.5; ld.size_y = 4.0; ld.energy = 140
ld.color = (0.85, 0.85, 0.82)
lo = bpy.data.objects.new("Tavan_Sekme", ld); lo.location = (3.0, 2.25, 0.3); lo.rotation_euler = (math.pi, 0, 0)
koleksiyon(K).objects.link(lo)

# Dolap içi ışık
ld = bpy.data.lights.new("Dolap_Isik", "AREA"); ld.shape = "RECTANGLE"; ld.size = 0.5; ld.size_y = 1.2; ld.energy = 40
ld.color = (0.8, 0.9, 1.0)
lo = bpy.data.objects.new("Dolap_Isik", ld); lo.location = (5.62, 3.3, 1.9)
koleksiyon(K).objects.link(lo)

# Raf fiyat etiketleri: dudak üstüne küçük beyaz/sarı kartlar, fiyatlar elle yazılmış gibi
# Görsel market_etiket_atlas.py ile üretilir (Blender Python'unda PIL yok)
ETK = os.path.join(DOKU, "fiyat_etiketleri.png")
img = bpy.data.images.load(ETK, check_existing=True); img.reload()
em = bpy.data.materials.get("M_Fiyat_Etiketi") or bpy.data.materials.new("M_Fiyat_Etiketi")
em.use_nodes = True
eb = next(n for n in em.node_tree.nodes if n.type == "BSDF_PRINCIPLED")
et = next((n for n in em.node_tree.nodes if n.type == "TEX_IMAGE"), None) or em.node_tree.nodes.new("ShaderNodeTexImage")
et.image = img
em.node_tree.links.new(et.outputs["Color"], eb.inputs["Base Color"])
eb.inputs["Roughness"].default_value = 0.25


def etiket(ad, merkez, aci):
    o = kutu(ad, merkez, (0.06, 0.002, 0.04), em, K, uv=False)
    o.rotation_euler = (0, 0, aci)
    k = random.randint(0, 7)
    uvl = o.data.uv_layers.new() if not o.data.uv_layers else o.data.uv_layers[0]
    for poly in o.data.polygons:
        for li, vi in zip(poly.loop_indices, poly.vertices):
            v = o.data.vertices[vi].co
            u = (k + (v.x / 0.06 + 0.5)) / 8
            uvl.data[li].uv = (u, v.z / 0.04 + 0.5)


KAT = [0.14, 0.52, 0.9, 1.28, 1.66]
ADA = [0.14, 0.5, 0.86, 1.22]
for z in KAT:
    for i in range(5):
        etiket("Etiket", (0.65 + i * 1.0 + random.uniform(0, 0.5), 4.05 - 0.002, z + 0.025), 0)
    for i in range(3):
        etiket("Etiket", (0.45 + 0.002, 1.25 + i * 1.0 + random.uniform(0, 0.5), z + 0.025), math.pi / 2)
for z in ADA:
    for i in range(2):
        etiket("Etiket", (2.35 + i * 1.0 + random.uniform(0, 0.5), 2.0 - 0.002, z + 0.025), 0)
        etiket("Etiket", (2.35 + i * 1.0 + random.uniform(0, 0.5), 2.8 + 0.002, z + 0.025), math.pi)

# Render: EEVEE ışın izleme (sekme ışığı ve yansımalar), soğuk gece havası
sc = bpy.context.scene
e = sc.eevee
e.use_raytracing = True
if hasattr(e, "use_fast_gi"):
    e.use_fast_gi = True
sc.view_settings.view_transform = "AgX"
sc.view_settings.look = "AgX - Medium High Contrast" if "AgX - Medium High Contrast" in [l for l in bpy.types.ColorManagedViewSettings.bl_rna.properties["look"].enum_items.keys()] else sc.view_settings.look
print("isik tamam", len(koleksiyon(K).objects))
