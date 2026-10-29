# Market odası için ortak yardımcılar: PBR malzeme, metrik UV, kutu nesnesi.
# Blender'da: exec(open(r"...\market_ortak.py", encoding="utf-8").read())
import bpy, bmesh, os
from mathutils import Vector

DOKU = r"C:\Kisisel Dosyalarim\ozel\Oyun Gelistirme\Denemelik projelerim\Dink\Kaynak\Blender\Dokular\Market"

# Poly Haven dokularının gerçek boyu (metre); UV bu ölçüyle açılır, doku dünyada doğru büyüklükte durur
DOKU_BOYU = {
    "granite_tile": 2.3,
    "plastered_wall_04": 3.2,
    "polystyrene": 1.5,
    "blue_metal_plate": 2.5,
    "laminate_floor_02": 1.7,
    "long_white_tiles": 1.27,
}


def koleksiyon(ad):
    c = bpy.data.collections.get(ad)
    if c is None:
        c = bpy.data.collections.new(ad)
        bpy.context.scene.collection.children.link(c)
    return c


def _img(ad, nonColor):
    yol = os.path.join(DOKU, ad)
    img = bpy.data.images.load(yol, check_existing=True)
    if nonColor:
        img.colorspace_settings.name = "Non-Color"
    return img


def pbr(ad, doku, renk_carp=None, doygunluk=1.0, parlaklik=1.0, puruz_carp=1.0, metal=0.0, normal_guc=1.0):
    """Poly Haven doku setinden PBR malzeme. renk_carp ile ton değiştirilir (gerçek yüzey detayı korunur)."""
    m = bpy.data.materials.get(ad) or bpy.data.materials.new(ad)
    m.use_nodes = True
    nt = m.node_tree
    nt.nodes.clear()
    out = nt.nodes.new("ShaderNodeOutputMaterial"); out.location = (900, 0)
    bsdf = nt.nodes.new("ShaderNodeBsdfPrincipled"); bsdf.location = (600, 0)
    nt.links.new(bsdf.outputs[0], out.inputs[0])

    def tex(suf, nonColor, y):
        n = nt.nodes.new("ShaderNodeTexImage"); n.location = (-500, y)
        n.image = _img(f"{doku}_{suf}_2k.jpg", nonColor)
        return n

    diff = tex("diff", False, 300)
    col = diff.outputs["Color"]
    # Ton/doygunluk/parlaklık: aynı fotoğraf dokusunu farklı boyalı yüzeylere uyarlamak için
    if renk_carp is not None or doygunluk != 1.0 or parlaklik != 1.0:
        hsv = nt.nodes.new("ShaderNodeHueSaturation"); hsv.location = (-150, 300)
        hsv.inputs["Saturation"].default_value = doygunluk
        hsv.inputs["Value"].default_value = parlaklik
        nt.links.new(col, hsv.inputs["Color"])
        col = hsv.outputs["Color"]
        if renk_carp is not None:
            mix = nt.nodes.new("ShaderNodeMix"); mix.data_type = "RGBA"; mix.blend_type = "MULTIPLY"
            mix.location = (100, 300); mix.inputs["Factor"].default_value = 1.0
            nt.links.new(col, mix.inputs[6]); mix.inputs[7].default_value = (*renk_carp, 1.0)
            col = mix.outputs[2]
    aoP = os.path.join(DOKU, f"{doku}_ao_2k.jpg")
    if os.path.exists(aoP):
        ao = tex("ao", True, 600)
        mixao = nt.nodes.new("ShaderNodeMix"); mixao.data_type = "RGBA"; mixao.blend_type = "MULTIPLY"
        mixao.location = (350, 400); mixao.inputs["Factor"].default_value = 0.6
        nt.links.new(col, mixao.inputs[6]); nt.links.new(ao.outputs["Color"], mixao.inputs[7])
        col = mixao.outputs[2]
    nt.links.new(col, bsdf.inputs["Base Color"])

    rough = tex("rough", True, 0)
    if puruz_carp != 1.0:
        mul = nt.nodes.new("ShaderNodeMath"); mul.operation = "MULTIPLY"; mul.location = (100, 0)
        mul.inputs[1].default_value = puruz_carp
        nt.links.new(rough.outputs["Color"], mul.inputs[0]); nt.links.new(mul.outputs[0], bsdf.inputs["Roughness"])
    else:
        nt.links.new(rough.outputs["Color"], bsdf.inputs["Roughness"])

    nor = tex("nor_gl", True, -300)
    nmap = nt.nodes.new("ShaderNodeNormalMap"); nmap.location = (300, -300)
    nmap.inputs["Strength"].default_value = normal_guc
    nt.links.new(nor.outputs["Color"], nmap.inputs["Color"]); nt.links.new(nmap.outputs[0], bsdf.inputs["Normal"])
    bsdf.inputs["Metallic"].default_value = metal
    m["doku"] = doku
    return m


def duz(ad, renk, puruz=0.5, metal=0.0, isik=0.0, isik_renk=None, alfa=1.0, cam=False):
    """Doku gerektirmeyen küçük parçalar (cam, ışık tüpü, plastik) için."""
    m = bpy.data.materials.get(ad) or bpy.data.materials.new(ad)
    m.use_nodes = True
    nt = m.node_tree
    b = next((n for n in nt.nodes if n.type == "BSDF_PRINCIPLED"), None)
    if b is None:  # başka bir kurulum (ör. cam_kur) düğümleri değiştirmişse baştan kur
        nt.nodes.clear()
        out = nt.nodes.new("ShaderNodeOutputMaterial")
        b = nt.nodes.new("ShaderNodeBsdfPrincipled")
        nt.links.new(b.outputs[0], out.inputs[0])
    m.surface_render_method = "DITHERED"
    b.inputs["Base Color"].default_value = (*renk, 1.0)
    b.inputs["Roughness"].default_value = puruz
    b.inputs["Metallic"].default_value = metal
    if isik > 0:
        b.inputs["Emission Color"].default_value = (*(isik_renk or renk), 1.0)
        b.inputs["Emission Strength"].default_value = isik
    if cam:
        b.inputs["Transmission Weight"].default_value = 1.0
        b.inputs["IOR"].default_value = 1.45
    if alfa < 1.0:
        b.inputs["Alpha"].default_value = alfa
    return m


def metrik_uv(obj, boy):
    """Küp izdüşümü: 1 UV birimi = boy metre; doku gerçek boyutunda tekrarlar."""
    me = obj.data
    bm = bmesh.new(); bm.from_mesh(me)
    uv = bm.loops.layers.uv.verify()
    mw = obj.matrix_world
    for f in bm.faces:
        n = (mw.to_3x3() @ f.normal).normalized()
        ax = max(range(3), key=lambda i: abs(n[i]))
        for l in f.loops:
            p = mw @ l.vert.co
            if ax == 0: u, v = p.y, p.z
            elif ax == 1: u, v = p.x, p.z
            else: u, v = p.x, p.y
            l[uv].uv = (u / boy, v / boy)
    bm.to_mesh(me); bm.free()


def kutu(ad, merkez, boyut, malz, kol="Market", uv=True):
    """Gerçek ölçülü kutu (metre). Ölçek uygulanmış gelir; UV metrik açılır."""
    me = bpy.data.meshes.new(ad)
    bm = bmesh.new()
    bmesh.ops.create_cube(bm, size=1.0)
    for v in bm.verts:
        v.co = Vector((v.co.x * boyut[0], v.co.y * boyut[1], v.co.z * boyut[2]))
    bm.to_mesh(me); bm.free()
    o = bpy.data.objects.new(ad, me)
    o.location = merkez
    koleksiyon(kol).objects.link(o)
    o.data.materials.append(malz)
    if uv:
        bpy.context.view_layer.update()
        metrik_uv(o, DOKU_BOYU.get(malz.get("doku", ""), 1.0))
    return o


def cam_kur(ad, saydamlik=0.88, ton=(0.85, 0.93, 0.95)):
    """Saydam + açıya bağlı yansıma. Principled alfa karışımı EEVEE'de süt beyazı çıkıyordu."""
    m = bpy.data.materials.get(ad) or bpy.data.materials.new(ad)
    m.use_nodes = True
    nt = m.node_tree; nt.nodes.clear()
    out = nt.nodes.new("ShaderNodeOutputMaterial")
    tr = nt.nodes.new("ShaderNodeBsdfTransparent"); tr.inputs[0].default_value = (*ton, 1)
    gl = nt.nodes.new("ShaderNodeBsdfGlossy"); gl.inputs["Roughness"].default_value = 0.03
    fr = nt.nodes.new("ShaderNodeLayerWeight"); fr.inputs[0].default_value = 0.2
    add = nt.nodes.new("ShaderNodeMath"); add.operation = "ADD"; add.inputs[1].default_value = 1 - saydamlik
    mx = nt.nodes.new("ShaderNodeMixShader")
    nt.links.new(fr.outputs["Fresnel"], add.inputs[0])
    nt.links.new(add.outputs[0], mx.inputs[0]); nt.links.new(tr.outputs[0], mx.inputs[1]); nt.links.new(gl.outputs[0], mx.inputs[2])
    nt.links.new(mx.outputs[0], out.inputs[0])
    m.surface_render_method = "DITHERED"
    return m
