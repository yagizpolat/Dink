# Hafıza marketi: rafın boş makarna bölmesinin önünde yerde duran, kapakları açık koli.
# İçinde ayakta dizilmiş makarna paketleri (raftan alınanlarla aynı model). Tekrar çalıştırılabilir.
# Blender'da: exec(open(r"...\hafiza_koli.py", encoding="utf-8").read())
import bpy, bmesh, math, os
from mathutils import Vector, Matrix

BET = r"C:\Kisisel Dosyalarim\ozel\Oyun Gelistirme\Denemelik projelerim\Dink\Kaynak\BlenderBetikleri"
exec(open(os.path.join(BET, "market_ortak.py"), encoding="utf-8").read())
KARTON = r"C:\Kisisel Dosyalarim\ozel\Oyun Gelistirme\Denemelik projelerim\Dink\Kaynak\Blender\Dokular\Karton"

KOL = "Hafiza_Koli"
eski = bpy.data.collections.get(KOL)
if eski:
    for o in list(eski.objects):
        bpy.data.objects.remove(o, do_unlink=True)
koleksiyon(KOL)

EN, DER, BOY, KAL = 0.50, 0.34, 0.30, 0.005   # dış ölçü ve karton kalınlığı (m)
MERKEZ = Vector((2.12, 3.52, 0.0))            # boş bölmenin önü, koridor zemini
DONUS = math.radians(-9)                       # rafa tam paralel değil: elle bırakılmış


def kraft():
    """ambientCG Cardboard004: renk + pürüzlülük + normal. Oluk çizgileri dikey durur."""
    m = bpy.data.materials.get("M_Koli_Karton") or bpy.data.materials.new("M_Koli_Karton")
    m.use_nodes = True; nt = m.node_tree; nt.nodes.clear()
    out = nt.nodes.new("ShaderNodeOutputMaterial")
    b = nt.nodes.new("ShaderNodeBsdfPrincipled"); nt.links.new(b.outputs[0], out.inputs[0])
    b.inputs["Specular IOR Level"].default_value = 0.3

    def tex(suf, non_color):
        n = nt.nodes.new("ShaderNodeTexImage")
        n.image = bpy.data.images.load(os.path.join(KARTON, f"Cardboard004_2K-JPG_{suf}.jpg"), check_existing=True)
        if non_color:
            n.image.colorspace_settings.name = "Non-Color"
        return n
    renk = tex("Color", False)
    # hafif koyulaştır: market floresanında sarı değil kahverengi kraft okunsun
    hsv = nt.nodes.new("ShaderNodeHueSaturation")
    hsv.inputs["Saturation"].default_value = 0.85; hsv.inputs["Value"].default_value = 0.78
    nt.links.new(renk.outputs[0], hsv.inputs["Color"]); nt.links.new(hsv.outputs[0], b.inputs["Base Color"])
    nt.links.new(tex("Roughness", True).outputs[0], b.inputs["Roughness"])
    nm = nt.nodes.new("ShaderNodeNormalMap"); nm.inputs["Strength"].default_value = 0.6
    nt.links.new(tex("NormalGL", True).outputs[0], nm.inputs["Color"]); nt.links.new(nm.outputs[0], b.inputs["Normal"])
    m["doku"] = "karton"
    return m


DOKU_BOYU["karton"] = 0.6
km = kraft()
bant = detay("M_Koli_Bant", "detay_plastik", (0.62, 0.48, 0.30), pmin=0.25, pmax=0.4, ton=0.06)

kok = bpy.data.objects.new("Koli", None); kok.location = MERKEZ; kok.rotation_euler.z = DONUS
koleksiyon(KOL).objects.link(kok)
bpy.context.view_layer.update()  # matrix_world ilk parçadan önce güncel olsun


def parca(ad, yerel, boyut, malz, egim=None, mentese=None):
    """Kolinin yerel uzayında kutu; egim=(eksen, açı) menteşe kenarı etrafında döndürür."""
    o = kutu(ad, (0, 0, 0), boyut, malz, kol=KOL, uv=False)
    M = Matrix.Translation(Vector(yerel))
    if egim:
        eksen, aci = egim
        piv = Vector(mentese)
        M = Matrix.Translation(piv) @ Matrix.Rotation(aci, 4, eksen) @ Matrix.Translation(-piv) @ M
    o.matrix_world = kok.matrix_world @ M
    bpy.context.view_layer.update()
    metrik_uv(o, DOKU_BOYU[malz.get("doku", "karton")] if malz.get("doku") else 0.3)
    return o


h = BOY / 2
parca("Koli_Taban", (0, 0, KAL / 2), (EN, DER, KAL), km)
parca("Koli_On", (0, -DER / 2 + KAL / 2, h), (EN, KAL, BOY), km)
parca("Koli_Arka", (0, DER / 2 - KAL / 2, h), (EN, KAL, BOY), km)
parca("Koli_Sol", (-EN / 2 + KAL / 2, 0, h), (KAL, DER - 2 * KAL, BOY), km)
parca("Koli_Sag", (EN / 2 - KAL / 2, 0, h), (KAL, DER - 2 * KAL, BOY), km)

# Kapaklar düz açık konumda (dışa doğru yatay) modellenir, sonra menteşe etrafında aşağı sarkıtılır; her biri farklı açıda
uzun, kisa = DER / 2, DER / 2 * 0.9
parca("Kapak_On", (0, -DER / 2 - uzun / 2 + KAL, BOY), (EN - 0.004, uzun, KAL), km,
      egim=("X", math.radians(38)), mentese=(0, -DER / 2, BOY))
parca("Kapak_Arka", (0, DER / 2 + uzun / 2 - KAL, BOY), (EN - 0.004, uzun, KAL), km,
      egim=("X", math.radians(-18)), mentese=(0, DER / 2, BOY))
parca("Kapak_Sol", (-EN / 2 - kisa / 2 + KAL, 0, BOY), (kisa, DER - 0.01, KAL), km,
      egim=("Y", math.radians(-55)), mentese=(-EN / 2, 0, BOY))
parca("Kapak_Sag", (EN / 2 + kisa / 2 - KAL, 0, BOY), (kisa, DER - 0.01, KAL), km,
      egim=("Y", math.radians(12)), mentese=(EN / 2, 0, BOY))

# Kesilmiş koli bandı artığı: ön ve arka duvarın üst kenarından sarkan iki kısa şerit
for ad, y in (("Bant_On", -DER / 2 - 0.001), ("Bant_Arka", DER / 2 + 0.001)):
    parca(ad, (0.012, y, BOY - 0.035), (0.048, 0.0008, 0.07), bant)

# İçerik: raftan eksilen makarnalarla aynı model, ayakta 3 x 5 dizi; dizi kusursuz değil
kaynak = next(o for o in bpy.data.collections["Eksik_Makarna"].objects)
# Rafta ayakta duruş yönü korunur: boyutlar dünya ekseninde ölçülür
ROT = kaynak.matrix_world.to_3x3().normalized()
bb = [ROT @ Vector(v) for v in kaynak.bound_box]
w, d, t = (max(b[i] for b in bb) - min(b[i] for b in bb) for i in range(3))
import random
random.seed(7)
for i in range(3):
    for j in range(5):
        if (i, j) in ((2, 4), (0, 0)):
            continue  # iki paket eksik: biri raftaki boşluğa zaten gitmiş gibi
        o = kaynak.copy()
        koleksiyon(KOL).objects.link(o)
        yerel = Vector((-EN / 2 + KAL + 0.008 + w / 2 + i * (w + 0.002),
                        -DER / 2 + KAL + 0.006 + d / 2 + j * (d + 0.001),
                        KAL + 0.001))  # paket modelinin orijini tabanında
        o.matrix_world = kok.matrix_world @ Matrix.Translation(yerel) @ Matrix.Rotation(random.uniform(-0.04, 0.04), 4, "Z") @ ROT.to_4x4()
print("koli hazır")
