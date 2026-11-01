# Hafıza marketi: Unity'ye aktarım (koridor_4_aktar.py'nin uyarlaması). Blender düğüm hesapları (ton, tarama karışımı, döndürme) Unity'ye geçmez;
# her malzemenin 0..1 UV karosu Cycles ile renk / pürüzlülük / normal dokusuna pişirilir. Metrik UV karoyu tekrarlar,
# böylece Unity'de doku aynı ölçüde durur. Tüm parçalar tek nesnede birleşip FBX'e yazılır; ışıklar JSON'a.
import bpy, bmesh, os, json, math

PROJE = r"C:\Kisisel Dosyalarim\ozel\Oyun Gelistirme\Denemelik projelerim\Dink"
TEX = os.path.join(PROJE, r"Assets\_Project\Textures\HafizaMarket")
MDL = os.path.join(PROJE, r"Assets\_Project\Models\HafizaMarket")
os.makedirs(TEX, exist_ok=True); os.makedirs(MDL, exist_ok=True)
BOY = 1024
OZEL = {"M_Cam", "M_Vitrin_Cam", "M_Sise_Su", "M_Sise_Yag", "M_BoslukZemin"}  # Unity'de elle kurulur

s = bpy.context.scene
eski_motor = s.render.engine
s.render.engine = "CYCLES"
s.cycles.samples = 4
s.cycles.device = "CPU"

# Pişirme düzlemi: UV 0..1
me = bpy.data.meshes.new("PisirDuzlem"); bm = bmesh.new()
v = [bm.verts.new(c) for c in [(0, 0, 0), (1, 0, 0), (1, 1, 0), (0, 1, 0)]]
f = bm.faces.new(v); uv = bm.loops.layers.uv.new()
for l, c in zip(f.loops, [(0, 0), (1, 0), (1, 1), (0, 1)]):
    l[uv].uv = c
bm.to_mesh(me); bm.free()
duz = bpy.data.objects.new("PisirDuzlem", me); s.collection.objects.link(duz)
for o in s.objects:
    o.select_set(False)
duz.select_set(True); bpy.context.view_layer.objects.active = duz


def kaynak(nt, b, giris):
    """BSDF girişine bağlı soket (yoksa None)."""
    ln = b.inputs[giris].links
    return ln[0].from_socket if ln else None


def pisir(m, tur, dosya, renkli):
    nt = m.node_tree
    b = next(n for n in nt.nodes if n.type == "BSDF_PRINCIPLED")
    out = next(n for n in nt.nodes if n.type == "OUTPUT_MATERIAL")
    img = bpy.data.images.new(dosya, BOY, BOY, alpha=False, float_buffer=False)
    if not renkli:
        img.colorspace_settings.name = "Non-Color"
    t = nt.nodes.new("ShaderNodeTexImage"); t.image = img
    nt.nodes.active = t
    eski = out.inputs[0].links[0].from_socket
    if tur == "NORMAL":
        with bpy.context.temp_override(object=duz, active_object=duz, selected_objects=[duz], selected_editable_objects=[duz]):
            bpy.ops.object.bake(type="NORMAL", normal_space="TANGENT", margin=4)
    else:
        em = nt.nodes.new("ShaderNodeEmission")
        src = kaynak(nt, b, "Base Color" if tur == "RENK" else "Roughness")
        if src:
            nt.links.new(src, em.inputs[0])
        else:
            d = b.inputs["Base Color" if tur == "RENK" else "Roughness"].default_value
            em.inputs[0].default_value = tuple(d) if tur == "RENK" else (d, d, d, 1)
        nt.links.new(em.outputs[0], out.inputs[0])
        with bpy.context.temp_override(object=duz, active_object=duz, selected_objects=[duz], selected_editable_objects=[duz]):
            bpy.ops.object.bake(type="EMIT", margin=4)
        nt.links.new(eski, out.inputs[0]); nt.nodes.remove(em)
    img.filepath_raw = os.path.join(TEX, dosya + ".png"); img.file_format = "PNG"; img.save()
    nt.nodes.remove(t); bpy.data.images.remove(img)


bilgi = {}
mats = sorted({m for o in s.objects if o.type == "MESH" and o.name != "PisirDuzlem" and o.visible_get() and o.name != "Bosluk_Zemin" for m in o.data.materials if m}, key=lambda m: m.name)
for m in mats:
    if m.name in OZEL:
        continue
    duz.data.materials.clear(); duz.data.materials.append(m)
    b = next(n for n in m.node_tree.nodes if n.type == "BSDF_PRINCIPLED")
    ad = "HM_" + m.name.replace("M_", "", 1)
    pisir(m, "RENK", ad + "_Renk", True)
    pisir(m, "PURUZ", ad + "_Puruz", False)
    if b.inputs["Normal"].links:
        pisir(m, "NORMAL", ad + "_Normal", False)
    em = b.inputs["Emission Strength"]
    bilgi[m.name] = {"metal": b.inputs["Metallic"].default_value, "normal": bool(b.inputs["Normal"].links),
                     "isik": em.default_value if not em.links else 3.0,
                     "isik_renk": list(b.inputs["Emission Color"].default_value)[:3],
                     "alfa": bool(b.inputs["Alpha"].links)}
    print("pişti", m.name)

bpy.data.objects.remove(duz, do_unlink=True)
s.render.engine = eski_motor

# Işıklar: Unity'de yeniden kurulmak üzere
isiklar = []
for o in s.objects:
    if o.type == "LIGHT" and o.visible_get():
        L = o.data
        isiklar.append({"ad": o.name, "tur": L.type, "konum": list(o.matrix_world.translation), "yon": list(o.matrix_world.to_3x3() @ __import__("mathutils").Vector((0, 0, -1))),
                        "guc": L.energy, "renk": list(L.color), "boy": [getattr(L, "size", 0), getattr(L, "size_y", 0)], "golge": L.use_shadow})
json.dump({"malzeme": bilgi, "isik": isiklar}, open(os.path.join(MDL, "HafizaMarket_bilgi.json"), "w"), indent=1)

# Birleştir ve dışa aktar (dosya kaydedilmez: birleştirme yalnız aktarım için)
meshler = [o for o in s.objects if o.type == "MESH" and o.visible_get() and o.name != "Bosluk_Zemin"]
for o in s.objects:
    o.select_set(o in meshler)
with bpy.context.temp_override(object=meshler[0], active_object=meshler[0], selected_objects=meshler, selected_editable_objects=meshler):
    bpy.ops.object.join()
meshler[0].name = "HafizaMarket"
# Birleşen nesne ilk parçanın konumunu taşır: dünya koordinatını köke uygula (Unity'de nesne 0,0,0)
k = meshler[0]; k.data.transform(k.matrix_world); k.matrix_world = __import__("mathutils").Matrix.Identity(4)
# Cam yüzleri ayrı nesneye: Unity'de gölge düşürmesin, çarpışmasın (vitrin ışığı dışarı vursun)
import bmesh
CAM = {"M_Cam", "M_Vitrin_Cam", "M_Sise_Su", "M_Sise_Yag"}
cam_slot = {i for i, m in enumerate(k.data.materials) if m and m.name in CAM}
bm = bmesh.new(); bm.from_mesh(k.data)
cam_bm = bm.copy()
bmesh.ops.delete(bm, geom=[f for f in bm.faces if f.material_index in cam_slot], context="FACES")
bmesh.ops.delete(cam_bm, geom=[f for f in cam_bm.faces if f.material_index not in cam_slot], context="FACES")
bm.to_mesh(k.data); bm.free()
cam = bpy.data.objects.new("HafizaMarket_Cam", k.data.copy()); s.collection.objects.link(cam)
cam_bm.to_mesh(cam.data); cam_bm.free()
for o in (k, cam):  # kullanılmayan malzeme yuvaları Unity'de boş alt mesh olmasın
    with bpy.context.temp_override(object=o, active_object=o, selected_objects=[o]):
        bpy.ops.object.material_slot_remove_unused()
for o in s.objects:
    o.select_set(o in (k, cam))
with bpy.context.temp_override(object=k, active_object=k, selected_objects=[k, cam]):
    bpy.ops.export_scene.fbx(filepath=os.path.join(MDL, "HafizaMarket.fbx"), use_selection=True, object_types={"MESH"},
                             apply_scale_options="FBX_SCALE_UNITS", bake_space_transform=True, path_mode="STRIP")
bpy.ops.wm.revert_mainfile()
print("aktarım tamam")
