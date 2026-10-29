# Market odası 3/3: ürün modelleri (kutu, konserve, şişe) ve raflara dizilmeleri.
# Etiketler market_etiket_atlas.py ile üretilen 4x4 atlastan gelir.
import bpy, bmesh, math, random
exec(open(r"C:\Kisisel Dosyalarim\ozel\Oyun Gelistirme\Denemelik projelerim\Dink\Kaynak\BlenderBetikleri\market_ortak.py", encoding="utf-8").read())
from mathutils import Vector, Matrix

K = "Market_Urunler"
SABLON = "Market_Urun_Sablon"
for kol in (K, SABLON):
    for o in list(koleksiyon(kol).objects):
        bpy.data.objects.remove(o, do_unlink=True)
koleksiyon(SABLON).hide_render = True
koleksiyon(SABLON).hide_viewport = True
random.seed(2342)

# ── Malzemeler ──
atlas = bpy.data.images.load(os.path.join(DOKU, "urun_etiket_atlas.png"), check_existing=True)
atlas.reload()
puruz_atlas = bpy.data.images.load(os.path.join(DOKU, "urun_etiket_puruz.png"), check_existing=True)
puruz_atlas.reload(); puruz_atlas.colorspace_settings.name = "Non-Color"


def etiket_malz(ad, puruz, metal=0.0):
    m = bpy.data.materials.get(ad) or bpy.data.materials.new(ad)
    m.use_nodes = True
    nt = m.node_tree
    b = next(n for n in nt.nodes if n.type == "BSDF_PRINCIPLED")
    for n in [n for n in nt.nodes if n.type in ("TEX_IMAGE", "MATH", "BUMP")]:
        nt.nodes.remove(n)
    t = nt.nodes.new("ShaderNodeTexImage"); t.image = atlas
    nt.links.new(t.outputs["Color"], b.inputs["Base Color"])
    # Pürüzlülük atlası: mürekkepli yüzey yarı parlak, aşınmış kenar ve kat izi mat; malzemeye göre ölçeklenir
    r = nt.nodes.new("ShaderNodeTexImage"); r.image = puruz_atlas
    mul = nt.nodes.new("ShaderNodeMath"); mul.operation = "MULTIPLY"; mul.inputs[1].default_value = puruz / 0.47
    nt.links.new(r.outputs["Color"], mul.inputs[0]); nt.links.new(mul.outputs[0], b.inputs["Roughness"])
    # Karton lifi ve kat izinden hafif kabartı
    bump = nt.nodes.new("ShaderNodeBump"); bump.inputs["Strength"].default_value = 0.08
    nt.links.new(r.outputs["Color"], bump.inputs["Height"]); nt.links.new(bump.outputs["Normal"], b.inputs["Normal"])
    b.inputs["Metallic"].default_value = metal
    return m


KARTON = etiket_malz("M_Urun_Karton", 0.62)
TENEKE = etiket_malz("M_Urun_Teneke", 0.32, 0.35)
PLASTIK_ET = etiket_malz("M_Urun_PlastikEtiket", 0.35)
KAPAK = duz("M_Urun_Kapak", (0.75, 0.75, 0.74), puruz=0.3, metal=1.0)
SISE_MALZ = {
    "su": duz("M_Sise_Su", (0.75, 0.88, 0.95), puruz=0.05, cam=True),
    "yag": duz("M_Sise_Yag", (0.95, 0.75, 0.15), puruz=0.08, cam=True),
    "deterjan": duz("M_Sise_Deterjan", (0.25, 0.6, 0.22), puruz=0.3),
}
KAPAK_RENK = {
    "su": duz("M_Kapak_Mavi", (0.1, 0.3, 0.75), puruz=0.4),
    "yag": duz("M_Kapak_Yesil", (0.1, 0.35, 0.12), puruz=0.4),
    "deterjan": duz("M_Kapak_Sari", (0.95, 0.85, 0.15), puruz=0.4),
}


def hucre_uv(i):
    """Atlas hücresinin UV sınırları (u0, v0, u1, v1); PIL'de satır 0 üstte."""
    c, r = i % 4, i // 4
    return c / 4, 1 - (r + 1) / 4, (c + 1) / 4, 1 - r / 4


def on_yari(a):
    """Silindir sarması: etiketin tamamı ön yarım çevreye (oyuncuya bakan yüz) gelir, kare oranı korunur;
    arka yarı etiketin kenar zemin rengini alır."""
    f = (a - 0.25) * 2.0
    return f if 0.0 <= f <= 1.0 else (0.02 if f < 0 else 0.98)


def _yeni(ad):
    me = bpy.data.meshes.new(ad)
    o = bpy.data.objects.new(ad, me)
    koleksiyon(SABLON).objects.link(o)
    return o, me


def kutu_urun(ad, hucre, en, derin, boy):
    """Karton kutu; ön yüz (-y) etiketin tamamı, yan/üst yüzler etiketin şeritleri."""
    o, me = _yeni(ad)
    bm = bmesh.new()
    bmesh.ops.create_cube(bm, size=1.0)
    for v in bm.verts:
        v.co = Vector((v.co.x * en, v.co.y * derin, (v.co.z + 0.5) * boy))
    uv = bm.loops.layers.uv.verify()
    u0, v0, u1, v1 = hucre_uv(hucre)
    pay = 0.004
    # Ön yüz: etiket kutunun en/boy oranında kırpılır, yazı ezilmez. Geniş kutuda üst kısım (marka) alınır.
    oran = en / boy
    if oran >= 1:
        # Geniş kutu: etiketin üst kısmı (marka ve ürün adı) alınır
        kw, kh = 1.0, 1.0 / oran
        ky0, kx0 = 1.0 - kh - 0.03, 0.0
    else:
        # Uzun kutu: orta sütun; yazılar tasarımda orta %62'ye sığdırıldı
        kw, kh = max(oran, 0.64), 1.0
        kx0, ky0 = 0.5 - kw / 2, 0.0
    for f in bm.faces:
        n = f.normal
        for l in f.loops:
            p = l.vert.co
            if abs(n.y) > 0.5:      # ön/arka: kırpılmış etiket
                s = (p.x / en + 0.5) if n.y < 0 else (0.5 - p.x / en)
                t = p.z / boy
                s, t = kx0 + s * kw, ky0 + t * kh
            elif abs(n.x) > 0.5:    # yanlar: etiketin orta dikey şeridi
                s = 0.47 + (p.y / derin + 0.5) * 0.06
                t = p.z / boy
            else:                   # üst/alt: yazısız arka plan köşesi
                s = 0.03 + (p.x / en + 0.5) * 0.04
                t = 0.95 + (p.y / derin + 0.5) * 0.03
            l[uv].uv = (u0 + pay + s * (u1 - u0 - 2 * pay), v0 + pay + t * (v1 - v0 - 2 * pay))
    # Karton kenarları keskin değil: 3 mm pah ışığı yakalar
    bmesh.ops.bevel(bm, geom=bm.edges[:], offset=0.003, segments=2, profile=0.5, affect="EDGES")
    bm.to_mesh(me); bm.free()
    me.materials.append(KARTON)
    for p in me.polygons:
        p.use_smooth = p.area < 0.0004  # yalnız pah yüzleri yumuşak; düz yüzler düz kalır
    return o


def silindir(bm, r, z0, z1, seg, kapali_ust=True, kapali_alt=True):
    res = bmesh.ops.create_cone(bm, cap_ends=True, segments=seg, radius1=r, radius2=r, depth=z1 - z0)
    bmesh.ops.translate(bm, verts=res["verts"], vec=Vector((0, 0, (z0 + z1) / 2)))
    return res["verts"]


def konserve(ad, hucre, r, boy):
    o, me = _yeni(ad)
    bm = bmesh.new()
    silindir(bm, r, 0.0, boy, 28)
    uv = bm.loops.layers.uv.verify()
    u0, v0, u1, v1 = hucre_uv(hucre)
    for f in bm.faces:
        yan = abs(f.normal.z) < 0.5
        f.material_index = 0 if yan else 1
        for l in f.loops:
            p = l.vert.co
            if yan:
                a = (math.atan2(p.x, -p.y) / (2 * math.pi)) + 0.5
                # dikiş arkada kalsın; ön yüz etiketin ortası
                l[uv].uv = (u0 + on_yari(a) * (u1 - u0), v0 + 0.05 * (v1 - v0) + (p.z / boy) * 0.9 * (v1 - v0))
            else:
                l[uv].uv = (u0, v0)
    # Ön yüzde dikiş olmasın diye UV açısına göre yüzleri ayır
    # Teneke kapak kenarı yuvarlak
    bmesh.ops.bevel(bm, geom=[e for e in bm.edges if abs(e.verts[0].co.z - e.verts[1].co.z) < 1e-5 and abs(e.verts[0].co.z) < 1e-4 or abs(e.verts[0].co.z - boy) < 1e-4 and abs(e.verts[1].co.z - boy) < 1e-4], offset=0.004, segments=2, affect="EDGES")
    bmesh.ops.split_edges(bm, edges=[e for e in bm.edges if abs(e.verts[0].co.y - e.verts[1].co.y) < 1e-4 and e.verts[0].co.y > 0 and abs(e.verts[0].co.x) < 1e-3 and abs(e.verts[0].co.z - e.verts[1].co.z) > 1e-4])
    bm.to_mesh(me); bm.free()
    me.materials.append(TENEKE); me.materials.append(KAPAK)
    for p in me.polygons:
        p.use_smooth = abs(p.normal.z) < 0.5
    return o


def sise(ad, hucre, tur, r, boy):
    """Gövde + omuz + boyun + kapak; ortada etiket bandı."""
    o, me = _yeni(ad)
    bm = bmesh.new()
    seg = 24
    govde = boy * 0.68
    silindir(bm, r, 0.0, govde, seg)
    omuz = bmesh.ops.create_cone(bm, cap_ends=False, segments=seg, radius1=r, radius2=r * 0.32, depth=boy * 0.2)
    bmesh.ops.translate(bm, verts=omuz["verts"], vec=Vector((0, 0, govde + boy * 0.1)))
    silindir(bm, r * 0.3, govde + boy * 0.2, boy * 0.94, 16)
    kp = silindir(bm, r * 0.36, boy * 0.92, boy, 16)
    uv = bm.loops.layers.uv.verify()
    u0, v0, u1, v1 = hucre_uv(hucre)
    bant0, bant1 = govde * 0.25, govde * 0.85
    for f in bm.faces:
        c = f.calc_center_median()
        if c.z > boy * 0.92:
            f.material_index = 2
        elif abs(f.normal.z) < 0.5 and bant0 <= c.z <= bant1 and c.z < govde:
            f.material_index = 1
            for l in f.loops:
                p = l.vert.co
                a = (math.atan2(p.x, -p.y) / (2 * math.pi)) + 0.5
                l[uv].uv = (u0 + on_yari(a) * (u1 - u0), v0 + ((p.z - bant0) / (bant1 - bant0)) * (v1 - v0))
        else:
            f.material_index = 0
    # Etiket bandını gövdeye ayrı halka yap: kesimi bant sınırlarına göre bisect
    for z in (bant0, bant1):
        bmesh.ops.bisect_plane(bm, geom=bm.verts[:] + bm.edges[:] + bm.faces[:], plane_co=(0, 0, z), plane_no=(0, 0, 1))
    for f in bm.faces:
        c = f.calc_center_median()
        if abs(f.normal.z) < 0.5 and bant0 <= c.z <= bant1 and c.z < govde:
            f.material_index = 1
            for l in f.loops:
                p = l.vert.co
                a = (math.atan2(p.x, -p.y) / (2 * math.pi)) + 0.5
                l[uv].uv = (u0 + on_yari(a) * (u1 - u0), v0 + ((p.z - bant0) / (bant1 - bant0)) * (v1 - v0))
    bm.to_mesh(me); bm.free()
    me.materials.append(SISE_MALZ[tur]); me.materials.append(PLASTIK_ET); me.materials.append(KAPAK_RENK[tur])
    for p in me.polygons:
        p.use_smooth = abs(p.normal.z) < 0.5
    return o


# ── Şablonlar: (nesne, en (x), derinlik (y), raf aralığı payı) ──
S = {}
S["makarna"] = (kutu_urun("U_Makarna", 0, 0.16, 0.06, 0.25), 0.165, 0.065)
S["spagetti"] = (kutu_urun("U_Spagetti", 1, 0.09, 0.04, 0.29), 0.095, 0.045)
S["cay"] = (kutu_urun("U_Cay", 2, 0.12, 0.08, 0.2), 0.125, 0.085)
S["biskuvi"] = (kutu_urun("U_Biskuvi", 3, 0.2, 0.05, 0.07), 0.205, 0.055)
S["su"] = (sise("U_Su", 4, "su", 0.045, 0.32), 0.095, 0.095)
S["salca"] = (konserve("U_Salca", 5, 0.055, 0.12), 0.115, 0.115)
S["nohut"] = (konserve("U_Nohut", 6, 0.05, 0.115), 0.105, 0.105)
S["yag"] = (sise("U_Yag", 7, "yag", 0.042, 0.3), 0.09, 0.09)
S["bulasik"] = (sise("U_Bulasik", 8, "deterjan", 0.035, 0.24), 0.08, 0.08)
S["camasir"] = (kutu_urun("U_Camasir", 9, 0.28, 0.12, 0.3), 0.285, 0.125)
S["pirinc"] = (kutu_urun("U_Pirinc", 10, 0.14, 0.06, 0.22), 0.145, 0.065)
S["mercimek"] = (kutu_urun("U_Mercimek", 11, 0.14, 0.06, 0.22), 0.145, 0.065)
S["seker"] = (kutu_urun("U_Seker", 12, 0.18, 0.09, 0.11), 0.185, 0.095)
S["kahve"] = (konserve("U_Kahve", 13, 0.04, 0.1), 0.085, 0.085)
S["gazoz"] = (konserve("U_Gazoz", 14, 0.033, 0.115), 0.07, 0.07)
S["cips"] = (kutu_urun("U_Cips", 15, 0.17, 0.07, 0.24), 0.175, 0.075)


def diz(sablon_ad, baslangic, yon_boy, yon_on, uzunluk, derinlik, z, bosluk=0.12):
    """Raf tablasına sıra sıra ürün koyar. yon_boy: raf boyunca birim vektör, yon_on: oyuncuya doğru birim vektör."""
    o, en, derin = S[sablon_ad]
    yon_boy = Vector(yon_boy); yon_on = Vector(yon_on)
    aci = math.atan2(-yon_on.x, yon_on.y) + math.pi  # yerel -y (ön yüz) -> yon_on
    n_boy = int(uzunluk // en)
    n_derin = max(1, int(derinlik // derin))
    kenar = (uzunluk - n_boy * en) / 2
    # Bazen bir sütun tamamen boş: satılmış ve yerine konmamış
    for i in range(n_boy):
        if random.random() < bosluk:
            continue
        sutun_derin = n_derin - (random.random() < 0.3)
        for j in range(sutun_derin):
            p = Vector(baslangic) + yon_boy * (kenar + en * (i + 0.5)) - yon_on * (0.03 + derin * (j + 0.5))
            k = o.copy()
            k.location = (p.x, p.y, z)
            k.rotation_euler = (0, 0, aci + random.uniform(-0.05, 0.05))
            k.location += Vector((random.uniform(-0.006, 0.006), random.uniform(-0.006, 0.006), 0))
            koleksiyon(K).objects.link(k)


UST = 0.011  # tabla kalınlığının yarısı
KAT = [0.14, 0.52, 0.9, 1.28, 1.66]
ADA = [0.14, 0.5, 0.86, 1.22]

# Arka duvar (x 0.5..5.5, ön y=4.05, oyuncuya doğru -y)
arka = [
    ["camasir", "bulasik", "bulasik", "camasir", "biskuvi"],
    ["pirinc", "mercimek", "seker", "seker", "cips"],
    ["makarna", "makarna", "spagetti", "cay", "cips"],
    ["salca", "nohut", "salca", "kahve", "biskuvi"],
    ["yag", "yag", "su", "su", "cay"],
]
for k, z in enumerate(KAT):
    for i in range(5):
        diz(arka[k][i], (0.5 + i * 1.0, 4.05, 0), (1, 0, 0), (0, -1, 0), 1.0, 0.38, z + UST, bosluk=0.1 + 0.05 * k)

# Sol duvar (y 1.1..4.1, ön x=0.45, oyuncuya doğru +x)
sol = [["su", "su", "yag"], ["seker", "pirinc", "mercimek"], ["biskuvi", "cips", "cips"], ["kahve", "cay", "cay"], ["camasir", "camasir", "bulasik"]]
for k, z in enumerate(KAT):
    for i in range(3):
        diz(sol[k][i], (0.45, 1.1 + i * 1.0, 0), (0, 1, 0), (1, 0, 0), 1.0, 0.38, z + UST)

# Orta ada: ön yüz (y=2.0, -y yönü), arka yüz (y=2.8, +y yönü)
ada_on = [["salca", "nohut"], ["makarna", "spagetti"], ["biskuvi", "cips"], ["kahve", "cay"]]
ada_arka = [["su", "yag"], ["pirinc", "mercimek"], ["seker", "seker"], ["bulasik", "bulasik"]]
for k, z in enumerate(ADA):
    for i in range(2):
        diz(ada_on[k][i], (2.2 + i * 1.0, 2.0, 0), (1, 0, 0), (0, -1, 0), 1.0, 0.36, z + UST)
        diz(ada_arka[k][i], (3.2 + i * 1.0, 2.8, 0), (-1, 0, 0), (0, 1, 0), 1.0, 0.36, z + UST)

# İçecek dolabı (x 5.33..5.95, y 2.7..3.9, cam -x yönünde)
for z, ad in zip([0.17, 0.555, 0.955, 1.355, 1.725], ["su", "su", "gazoz", "gazoz", "gazoz"]):
    diz(ad, (5.33, 3.9, 0), (0, -1, 0), (-1, 0, 0), 1.2, 0.58, z, bosluk=0.08)

print("urun", len(koleksiyon(K).objects))
