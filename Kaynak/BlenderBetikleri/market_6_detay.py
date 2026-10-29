# Market odası 6: dokusuz kalan her yüzeye gerçek tarama tabanlı yüzey detayı; yazar kasa ve fiş yazıcısının
# ayrıntılı modeli; kapı etiketleri, dolap başlığı, armatür difüzörü. market_1..5'ten sonra çalıştırılır.
import bpy, bmesh, math
exec(open(r"C:\Kisisel Dosyalarim\ozel\Oyun Gelistirme\Denemelik projelerim\Dink\Kaynak\BlenderBetikleri\market_ortak.py", encoding="utf-8").read())
from mathutils import Vector

K = "Market_Detay"
for o in list(koleksiyon(K).objects):
    bpy.data.objects.remove(o, do_unlink=True)

# Detay dokularının dünyadaki tekrar boyu (m): küçük ölçek = ince yüzey dokusu
DOKU_BOYU.update({
    "detay_firca": 0.25,    # fırçalanmış beton çizgileri -> alüminyumun fırça izi
    "detay_plastik": 0.1,   # sıvanın ince kumu -> mat kumlu ABS plastik (desensiz)
    "detay_kaucuk": 0.25,   # süpürgelik kauçuğu (aynı kum, daha iri)
    "detay_cizik": 0.5,     # cilalı ahşaptaki çizikler -> krom/çelik çizikleri
    "lacquered_cherry_wood": 1.0,
})
KAYNAK = {"detay_firca": "brushed_concrete_04", "detay_plastik": "plastered_wall_04", "detay_kaucuk": "plastered_wall_04",
          "detay_cizik": "lacquered_cherry_wood"}


def detay(ad, anahtar, renk, metal=0.0, pmin=0.3, pmax=0.6, normal_guc=0.5, ton=0.12, isik=0.0, isik_renk=None, yansima=0.5):
    """Düz renk + taramanın pürüzlülüğü (pmin..pmax aralığına) + normal + tonundan hafif renk dalgası."""
    kaynak = KAYNAK[anahtar]
    m = bpy.data.materials.get(ad) or bpy.data.materials.new(ad)
    m.use_nodes = True
    nt = m.node_tree; nt.nodes.clear()
    out = nt.nodes.new("ShaderNodeOutputMaterial")
    b = nt.nodes.new("ShaderNodeBsdfPrincipled")
    nt.links.new(b.outputs[0], out.inputs[0])
    dif = nt.nodes.new("ShaderNodeTexImage"); dif.image = _img(f"{kaynak}_diff_2k.jpg", False)
    rgh = nt.nodes.new("ShaderNodeTexImage"); rgh.image = _img(f"{kaynak}_rough_2k.jpg", True)
    nor = nt.nodes.new("ShaderNodeTexImage"); nor.image = _img(f"{kaynak}_nor_gl_2k.jpg", True)
    # Renk: taramanın parlaklığı ana rengi ±ton oranında oynatır (düz boya görünmesin)
    bw = nt.nodes.new("ShaderNodeRGBToBW"); nt.links.new(dif.outputs["Color"], bw.inputs[0])
    mr = nt.nodes.new("ShaderNodeMapRange"); mr.inputs["To Min"].default_value = 1 - ton; mr.inputs["To Max"].default_value = 1 + ton
    nt.links.new(bw.outputs[0], mr.inputs["Value"])
    mul = nt.nodes.new("ShaderNodeMix"); mul.data_type = "RGBA"; mul.blend_type = "MULTIPLY"; mul.inputs["Factor"].default_value = 1
    mul.inputs[6].default_value = (*renk, 1)
    comb = nt.nodes.new("ShaderNodeCombineColor")
    for i in range(3):
        nt.links.new(mr.outputs[0], comb.inputs[i])
    nt.links.new(comb.outputs[0], mul.inputs[7])
    nt.links.new(mul.outputs[2], b.inputs["Base Color"])
    pr = nt.nodes.new("ShaderNodeMapRange"); pr.inputs["To Min"].default_value = pmin; pr.inputs["To Max"].default_value = pmax
    nt.links.new(rgh.outputs["Color"], pr.inputs["Value"]); nt.links.new(pr.outputs[0], b.inputs["Roughness"])
    nm = nt.nodes.new("ShaderNodeNormalMap"); nm.inputs["Strength"].default_value = normal_guc
    nt.links.new(nor.outputs["Color"], nm.inputs["Color"]); nt.links.new(nm.outputs[0], b.inputs["Normal"])
    b.inputs["Metallic"].default_value = metal
    b.inputs["Specular IOR Level"].default_value = yansima
    if isik > 0:
        b.inputs["Emission Color"].default_value = (*(isik_renk or renk), 1)
        b.inputs["Emission Strength"].default_value = isik
    m["doku"] = anahtar
    return m


def resim_malz(ad, dosya, puruz=0.5, puruz_dosya=None, isik=0.0, alfa=False, metal=0.0, normal_kaynak=None, yansima=0.5):
    """Nesneye özel baskı dokusu (tuş takımı, ekran, etiket)."""
    m = bpy.data.materials.get(ad) or bpy.data.materials.new(ad)
    m.use_nodes = True
    nt = m.node_tree; nt.nodes.clear()
    out = nt.nodes.new("ShaderNodeOutputMaterial")
    b = nt.nodes.new("ShaderNodeBsdfPrincipled"); nt.links.new(b.outputs[0], out.inputs[0])
    t = nt.nodes.new("ShaderNodeTexImage"); t.image = _img(dosya, False); t.image.reload()
    nt.links.new(t.outputs["Color"], b.inputs["Base Color"])
    if puruz_dosya:
        r = nt.nodes.new("ShaderNodeTexImage"); r.image = _img(puruz_dosya, True); r.image.reload()
        nt.links.new(r.outputs["Color"], b.inputs["Roughness"])
    else:
        b.inputs["Roughness"].default_value = puruz
    if normal_kaynak:  # baskının altındaki malzemenin yüzey kumu
        n = nt.nodes.new("ShaderNodeTexImage"); n.image = _img(f"{normal_kaynak}_nor_gl_2k.jpg", True)
        tc = nt.nodes.new("ShaderNodeTexCoord"); mp = nt.nodes.new("ShaderNodeMapping"); mp.inputs["Scale"].default_value = (6, 6, 6)
        nt.links.new(tc.outputs["UV"], mp.inputs[0]); nt.links.new(mp.outputs[0], n.inputs[0])
        nm = nt.nodes.new("ShaderNodeNormalMap"); nm.inputs["Strength"].default_value = 0.35
        nt.links.new(n.outputs["Color"], nm.inputs["Color"]); nt.links.new(nm.outputs[0], b.inputs["Normal"])
    if isik > 0:
        nt.links.new(t.outputs["Color"], b.inputs["Emission Color"]); b.inputs["Emission Strength"].default_value = isik
    if alfa:
        nt.links.new(t.outputs["Alpha"], b.inputs["Alpha"])
        m.surface_render_method = "DITHERED"
    b.inputs["Metallic"].default_value = metal
    b.inputs["Specular IOR Level"].default_value = yansima
    return m


def malz_uv(obj):
    """Her yüzün UV'si kendi malzemesinin doku boyuna göre metrik açılır (karışık malzemeli nesneler)."""
    me = obj.data
    if not me.uv_layers:
        me.uv_layers.new()
    bm = bmesh.new(); bm.from_mesh(me)
    uv = bm.loops.layers.uv.verify()
    mw = obj.matrix_world
    for f in bm.faces:
        mat = me.materials[f.material_index] if f.material_index < len(me.materials) else None
        boy = DOKU_BOYU.get(mat.get("doku", "") if mat else "", 1.0)
        n = (mw.to_3x3() @ f.normal).normalized()
        ax = max(range(3), key=lambda i: abs(n[i]))
        for l in f.loops:
            p = mw @ l.vert.co
            u, v = ((p.y, p.z) if ax == 0 else (p.x, p.z) if ax == 1 else (p.x, p.y))
            l[uv].uv = (u / boy, v / boy)
    bm.to_mesh(me); bm.free()


def _yonlendir(n, ust):
    """Yerel -y -> n, yerel +z -> ust'e en yakın dik yön."""
    from mathutils import Matrix
    y = -n
    z = (ust - ust.dot(y) * y).normalized()
    x = y.cross(z)
    return Matrix((x, y, z)).transposed().to_euler()


def plaka(ad, merkez, en, boy, malz, normal, kol=K):
    """Etiket/ekran gibi tek yüzlü ince plaka; UV 0..1 tam görsel. normal: yüzün baktığı yön."""
    me = bpy.data.meshes.new(ad)
    bm = bmesh.new()
    v = [bm.verts.new(c) for c in [(-en / 2, 0, -boy / 2), (en / 2, 0, -boy / 2), (en / 2, 0, boy / 2), (-en / 2, 0, boy / 2)]]
    f = bm.faces.new(v)
    uvl = bm.loops.layers.uv.new()
    for l, c in zip(f.loops, [(0, 0), (1, 0), (1, 1), (0, 1)]):
        l[uvl].uv = c
    bm.to_mesh(me); bm.free()
    o = bpy.data.objects.new(ad, me)
    koleksiyon(kol).objects.link(o)
    me.materials.append(malz)
    # Yerel -y yüzü ön, yerel +z görselin üstü. Ön yüz normal yönüne bakar; üst, dünya yukarısına
    # (yatay plakada uzak kenara) yakın tutulur. rotation_difference 180° dönüşte ekseni rastgele seçiyordu.
    n = Vector(normal).normalized()
    ust = Vector((0, 0, 1)) if abs(n.z) < 0.9 else Vector((0, 1, 0))
    o.rotation_euler = _yonlendir(n, ust)
    o.location = merkez
    return o


# ── 1. Malzemeler ──
ALU = detay("M_Aluminyum", "detay_firca", (0.62, 0.63, 0.64), metal=1.0, pmin=0.22, pmax=0.42, normal_guc=0.25, ton=0.06)
detay("M_Tavan_Profil", "detay_firca", (0.78, 0.78, 0.76), metal=0.0, pmin=0.35, pmax=0.5, normal_guc=0.2, ton=0.05)
detay("M_Armatur_Kasa", "detay_firca", (0.85, 0.85, 0.83), metal=0.0, pmin=0.35, pmax=0.5, normal_guc=0.2, ton=0.05)
KROM = detay("M_Krom", "detay_cizik", (0.82, 0.82, 0.82), metal=1.0, pmin=0.06, pmax=0.3, normal_guc=0.1, ton=0.03)
# Kumlu ABS: mat, zayıf yansıma (yoksa tavandaki panelleri gri pus gibi yansıtıyordu)
PLASTIK = detay("M_Plastik_Koyu", "detay_plastik", (0.05, 0.052, 0.055), pmin=0.72, pmax=0.9, normal_guc=0.6, ton=0.2, yansima=0.28)
detay("M_Supurgelik", "detay_kaucuk", (0.07, 0.07, 0.07), pmin=0.55, pmax=0.8, normal_guc=0.5, ton=0.3)
detay("M_Urun_Kapak", "detay_firca", (0.78, 0.78, 0.76), metal=1.0, pmin=0.2, pmax=0.4, normal_guc=0.3, ton=0.05)
for ad, renk in [("M_Kapak_Mavi", (0.08, 0.25, 0.7)), ("M_Kapak_Yesil", (0.08, 0.3, 0.1)), ("M_Kapak_Sari", (0.9, 0.78, 0.1))]:
    detay(ad, "detay_plastik", renk, pmin=0.35, pmax=0.55, normal_guc=0.4, ton=0.1)
detay("M_Sise_Deterjan", "detay_cizik", (0.22, 0.55, 0.2), pmin=0.25, pmax=0.45, normal_guc=0.05, ton=0.04)
# Tezgâh üstü: koyu cilalı kiraz kaplama (gerçek tarama); kasiyerin dirseği yıllardır aynı yere değmiş gibi parlak
pbr("M_Tezgah_Ust", "lacquered_cherry_wood", parlaklik=0.55, doygunluk=0.8)
# Dolap içi ışık şeridi ve difüzörler: prizmatik desen
dif_img = "difuzor.png"
for i in range(5):
    ad = f"M_Armatur_Difuzor_{i}"
    eski = bpy.data.materials.get(ad)
    guc = 9.0 if i != 3 else 3.5
    m = resim_malz(ad, dif_img, puruz=0.6, isik=guc)
    b = next(n for n in m.node_tree.nodes if n.type == "BSDF_PRINCIPLED")
    # Prizma deseni ışığı biraz kırar; renk soğuk beyaz
    t = next(n for n in m.node_tree.nodes if n.type == "TEX_IMAGE")
    mix = m.node_tree.nodes.new("ShaderNodeMix"); mix.data_type = "RGBA"; mix.blend_type = "MULTIPLY"; mix.inputs["Factor"].default_value = 1
    m.node_tree.links.new(t.outputs["Color"], mix.inputs[6]); mix.inputs[7].default_value = (0.88, 0.94, 1.0, 1)
    m.node_tree.links.new(mix.outputs[2], b.inputs["Emission Color"]); m.node_tree.links.new(mix.outputs[2], b.inputs["Base Color"])
    tc = m.node_tree.nodes.new("ShaderNodeTexCoord"); mp = m.node_tree.nodes.new("ShaderNodeMapping"); mp.inputs["Scale"].default_value = (12, 24, 1)
    m.node_tree.links.new(tc.outputs["Generated"], mp.inputs[0]); m.node_tree.links.new(mp.outputs[0], t.inputs[0])

# Su ve yağ şişeleri: kırılma dolabın karanlığını gösterip şişeyi bira şişesi gibi koyulaştırıyordu;
# saydam + açıya bağlı parlama (vitrin camıyla aynı kurulum), plastiğin rengiyle
cam_kur("M_Sise_Su", saydamlik=0.7, ton=(0.86, 0.94, 0.98))
cam_kur("M_Sise_Yag", saydamlik=0.45, ton=(0.98, 0.78, 0.2))

# ── 2. UV: dokusuz kalan mimari ve mobilya nesneleri malzemelerine göre metrik açılır ──
for kol in ("Market_Kabuk", "Market_Mobilya", "Market_Isik", "Market_Cephe"):
    for o in koleksiyon(kol).objects:
        if o.type == "MESH" and not o.name.startswith("Armatur") or (o.type == "MESH" and o.name.endswith("_Kasa")):
            malz_uv(o)

# Ürün kapakları ve plastik şişeler UV'si atlas noktasında; detay için nesne koordinatı kullan
for ad in ("M_Urun_Kapak", "M_Kapak_Mavi", "M_Kapak_Yesil", "M_Kapak_Sari", "M_Sise_Deterjan"):
    nt = bpy.data.materials[ad].node_tree
    tc = nt.nodes.new("ShaderNodeTexCoord"); mp = nt.nodes.new("ShaderNodeMapping"); mp.inputs["Scale"].default_value = (8, 8, 8)
    nt.links.new(tc.outputs["Object"], mp.inputs[0])
    for t in [n for n in nt.nodes if n.type == "TEX_IMAGE"]:
        t.projection = "BOX"; t.projection_blend = 0.3
        nt.links.new(mp.outputs[0], t.inputs[0])

# ── 3. Yazar kasa ve fiş yazıcısı: ayrıntılı model ──
for ad in ("Yazar_Kasa", "Fis_Yazici"):
    o = bpy.data.objects.get(ad)
    if o:
        bpy.data.objects.remove(o, do_unlink=True)
for o in [o for o in koleksiyon("Market_Cephe").objects if o.name.startswith("YK_")]:
    bpy.data.objects.remove(o, do_unlink=True)

UST = 0.97  # tezgâh üstü
X, Y = 4.0, 0.62
klavye = resim_malz("M_YK_Klavye", "yk_klavye.png", puruz_dosya="yk_klavye_puruz.png", normal_kaynak="plastered_wall_04", yansima=0.3)
ekran = resim_malz("M_YK_LCD", "yk_ekran.png", puruz=0.08, isik=1.6)
govde = resim_malz("M_YK_Govde", "yk_govde.png", puruz=0.75, normal_kaynak="plastered_wall_04", yansima=0.28)
yazici = resim_malz("M_Fis_Yazici", "fis_yazici.png", puruz=0.72, normal_kaynak="plastered_wall_04", yansima=0.28)
kagit = duz("M_Fis_Kagidi", (0.93, 0.92, 0.88), puruz=0.7)

g = kutu("YK_Govde", (X, Y, UST + 0.055), (0.42, 0.38, 0.11), PLASTIK, K)
malz_uv(g)
plaka("YK_GovdeOn", (X, Y - 0.191, UST + 0.055), 0.42, 0.11, govde, (0, -1, 0))
# Eğik tuş takımı: kasiyere doğru 14° yatık kama
EGIM = math.radians(14)
kama_m = Vector((X, Y + 0.03, UST + 0.125))
kama = kutu("YK_Kama", tuple(kama_m), (0.36, 0.26, 0.03), PLASTIK, K)
kama.rotation_euler = (EGIM, 0, 0)  # arka kenar yukarı, yüzü kasiyere (−y) doğru eğik
malz_uv(kama)
ust_normal = Vector((0, -math.sin(EGIM), math.cos(EGIM)))
plaka("YK_Klavye", tuple(kama_m + ust_normal * 0.0155), 0.33, 0.24, klavye, tuple(ust_normal))
# Ekran direği ve kasiyer ekranı (−y), arkada müşteri ekranı (+y)
kutu("YK_Direk", (X, Y + 0.16, UST + 0.2), (0.035, 0.035, 0.2), PLASTIK, K)
eh = kutu("YK_EkranKasa", (X, Y + 0.16, UST + 0.35), (0.27, 0.05, 0.15), PLASTIK, K)
eh.rotation_euler = (math.radians(-10), 0, 0)  # ekran kasiyerin gözüne doğru hafif yukarı bakar
malz_uv(eh)
lcd = plaka("YK_LCD", (X, Y + 0.134, UST + 0.35), 0.25, 0.125, ekran, (0, -math.cos(math.radians(10)), math.sin(math.radians(10))))
plaka("YK_MusteriLCD", (X, Y + 0.186, UST + 0.35), 0.25, 0.125, ekran, (0, 1, 0))

# Fiş yazıcısı + sarkan fiş
py = kutu("FY_Govde", (4.55, 0.6, UST + 0.06), (0.16, 0.2, 0.12), PLASTIK, K)
malz_uv(py)
plaka("FY_Ust", (4.55, 0.6, UST + 0.1205), 0.16, 0.2, yazici, (0, 0, 1))
fis = plaka("FY_Fis", (4.55, 0.52, UST + 0.16), 0.07, 0.09, kagit, (0, -1, 0.3))

# ── 4. Etiketler ve başlık ──
kapi = resim_malz("M_Kapi_Etiketleri", "kapi_etiketleri.png", puruz=0.35, alfa=True)
# Kapının iç yüzü (+y odaya bakar) camına yapışık; içeriden okunur
plaka("Kapi_Etiket", (0.75, -0.06, 1.35), 0.4, 0.4, kapi, (0, 1, 0))
baslik = resim_malz("M_Dolap_Baslik", "dolap_baslik.png", puruz=0.3, isik=1.2)
plaka("Dolap_Baslik", (5.315, 3.3, 2.0), 1.2, 0.11, baslik, (-1, 0, 0))

print("detay tamam", len(koleksiyon(K).objects))
