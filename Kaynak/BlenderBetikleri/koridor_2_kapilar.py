# Karanfil Apartmanı kat koridoru 2: daire kapıları (ceviz kaplama, panel çıtaları, pirinç donanım, zil + isimlik,
# ışık otomatiği, paspas), menteşeli çelik asansör kapısı (dar camlı, kat göstergesi, çağırma paneli), lobi duyurusu,
# kat levhası, yangın dolabı, koridor sonu penceresi + petek, süpürgelik. koridor_1'den sonra, koridor_baski.py ile.
import bpy, bmesh, math, random
exec(open(r"C:\Kisisel Dosyalarim\ozel\Oyun Gelistirme\Denemelik projelerim\Dink\Kaynak\BlenderBetikleri\market_ortak.py", encoding="utf-8").read())
DOKU = r"C:\Kisisel Dosyalarim\ozel\Oyun Gelistirme\Denemelik projelerim\Dink\Kaynak\Blender\Dokular\Koridor"
DOKU_BOYU.update({"black_walnut_veneer_02": 1.0, "pebble_embedded_concrete_02": 0.9, "green_metal_rust": 1.0})
from mathutils import Vector

K = "Koridor_Kapilar"
for o in list(koleksiyon(K).objects):
    bpy.data.objects.remove(o, do_unlink=True)
random.seed(5)

# (daire no, duvar 'sol' x=0 / 'sag' x=1.9, kapının y merkezi, isimlik hücresi, paspas: 0 yazılı / 1 desenli / None)
DAIRELER = [(1, "sol", 5.0, 0, 0), (2, "sag", 6.5, 1, None), (3, "sol", 9.0, 2, 1), (4, "sag", 10.3, 3, 0)]
# L kolu (x -8..0, y 10.1..12): 5 kuzey duvarında, 6 güney duvarında. Sol duvara kurulup döndürülür.
KOL_DAIRELER = [(5, "kuzey", -3.0, 4, 1), (6, "guney", -5.6, 5, None)]
KX, KY0 = -8.0, 10.1
HUCRE = 6  # daire no ve isimlik atlasındaki hücre sayısı

# ── Malzemeler ──
# Ceviz kaplama: damar dikey (kanat boyunca), koyu cilalı; açık tonu sıcak ışıkta pembe laminat gibi duruyordu
ceviz = pbr("M_K_Kapi_Ceviz", "black_walnut_veneer_02", renk_carp=(0.62, 0.48, 0.36), parlaklik=0.55, doygunluk=0.9, puruz_carp=0.6, dondur=True)
ceviz_koyu = pbr("M_K_Kapi_Cita", "black_walnut_veneer_02", renk_carp=(0.5, 0.38, 0.28), parlaklik=0.4, doygunluk=0.9, puruz_carp=0.55, dondur=True)
pirinc = detay("M_K_Pirinc", "detay_cizik", (0.78, 0.56, 0.26), metal=1.0, pmin=0.25, pmax=0.5, normal_guc=0.15, ton=0.08)
beyaz_pl = detay("M_K_Plastik_Beyaz", "detay_plastik", (0.82, 0.8, 0.74), pmin=0.45, pmax=0.65, normal_guc=0.4, ton=0.08, yansima=0.35)
krem_pl = detay("M_K_Plastik_Krem", "detay_plastik", (0.75, 0.68, 0.52), pmin=0.3, pmax=0.5, normal_guc=0.3, ton=0.08)
otomat_isik = detay("M_K_Otomat_Dugme", "detay_plastik", (0.9, 0.4, 0.08), pmin=0.3, pmax=0.45, normal_guc=0.2, ton=0.05, isik=1.6, isik_renk=(1.0, 0.45, 0.1))
# Asansör kapısı ve kasası: gerçek boyalı sac taraması (kalkıklar, pas akıntısı), grimsi yeşile çekilmiş
asansor_boya = pbr("M_K_Asansor_Boya", "green_metal_rust", renk_carp=(0.78, 0.86, 0.82), doygunluk=0.45, parlaklik=0.85, puruz_carp=0.75)
celik = detay("M_K_Celik", "detay_firca", (0.6, 0.61, 0.62), metal=1.0, pmin=0.22, pmax=0.4, normal_guc=0.25, ton=0.06)
kirmizi = detay("M_K_Yangin_Kirmizi", "detay_plastik", (0.55, 0.04, 0.03), pmin=0.25, pmax=0.4, normal_guc=0.3, ton=0.08)
hortum = detay("M_K_Hortum", "detay_kaucuk", (0.6, 0.05, 0.04), pmin=0.5, pmax=0.7, normal_guc=0.5, ton=0.15)
pvc = detay("M_K_PVC", "detay_plastik", (0.86, 0.85, 0.8), pmin=0.35, pmax=0.5, normal_guc=0.25, ton=0.05)
petek = detay("M_K_Petek", "detay_firca", (0.85, 0.84, 0.8), pmin=0.3, pmax=0.45, normal_guc=0.15, ton=0.04)
karanlik = duz("M_K_Kuyu_Karanlik", (0.005, 0.005, 0.005), puruz=0.9)
mozaik = bpy.data.materials["M_K_Basamak_Mozaik"]
cam = cam_kur("M_K_Cam", saydamlik=0.85, ton=(0.86, 0.9, 0.88))
tel_cam = cam_kur("M_K_TelCam", saydamlik=0.55, ton=(0.55, 0.6, 0.58))  # asansör kapısı: tel camlı, koyu kuyu görünür

no_m = resim_malz("M_K_DaireNo", "daire_no.png", puruz=0.3, metal=1.0)
isim_m = resim_malz("M_K_Isimlik", "isimlik.png", puruz=0.55, normal_kaynak="plastered_wall_04", yansima=0.35)
panel_m = resim_malz("M_K_AsansorPanel", "asansor_panel.png", puruz=0.3, metal=1.0)
gosterge_m = resim_malz("M_K_KatGosterge", "kat_gosterge.png", puruz=0.08, isik=2.5)
levha_m = resim_malz("M_K_KatLevha", "kat_levha.png", puruz=0.25, yansima=0.5)
duyuru_m = resim_malz("M_K_Duyuru", "duyuru.png", puruz=0.85, normal_kaynak="plastered_wall_04", yansima=0.2)
paspas_m = resim_malz("M_K_Paspas", "paspas.png", puruz=0.95, normal_kaynak="plastered_wall_04", yansima=0.15)
yangin_m = resim_malz("M_K_YanginEtiket", "yangin_etiket.png", puruz=0.35)
gece_m = resim_malz("M_K_Gece", "pencere_gece.png", puruz=1.0, isik=0.9)


def hucre(o, u0, v0, u1, v1):
    """Plakanın UV'sini atlas hücresine daralt (PIL: satır 0 üstte -> v ters)."""
    for l in o.data.uv_layers[0].data:
        u, v = l.uv
        l.uv = (u0 + u * (u1 - u0), 1 - v1 + v * (v1 - v0))


def silindir(ad, merkez, r, boy, malz, eksen="z", seg=24):
    me = bpy.data.meshes.new(ad); bm = bmesh.new()
    bmesh.ops.create_cone(bm, cap_ends=True, segments=seg, radius1=r, radius2=r, depth=boy)
    bm.to_mesh(me); bm.free()
    for p in me.polygons: p.use_smooth = abs(p.normal.z) < 0.5
    o = bpy.data.objects.new(ad, me); o.location = merkez; koleksiyon(K).objects.link(o)
    o.rotation_euler = {"z": (0, 0, 0), "x": (0, math.pi / 2, 0), "y": (math.pi / 2, 0, 0)}[eksen]
    me.materials.append(malz)
    bpy.context.view_layer.update()
    malz_uv(o)  # UV'siz nesnede detay dokusu tek piksel okunur, düz renk görünür
    return o


def kapi(no, duvar, yc, isim, paspas):
    """u: duvar boyunca (koridordan bakınca sağa), w: koridora doğru (0 = duvar iç yüzü), z: yukarı."""
    if duvar == "sol":
        P = lambda u, w, z: (w, yc - u, z); n = Vector((1, 0, 0))
    else:
        P = lambda u, w, z: (1.9 - w, yc + u, z); n = Vector((-1, 0, 0))
    def k(ad, u0, u1, w0, w1, z0, z1, m):
        return kutu(f"D{no}_{ad}", P((u0 + u1) / 2, (w0 + w1) / 2, (z0 + z1) / 2), (abs(w1 - w0), abs(u1 - u0), abs(z1 - z0)), m, K)
    # Kasa: duvar kalınlığını kaplayan astar + koridor yüzünde pervaz
    k("Astar_Sol", -0.5, -0.47, -0.15, 0.0, 0, 2.15, ceviz_koyu); k("Astar_Sag", 0.47, 0.5, -0.15, 0.0, 0, 2.15, ceviz_koyu)
    k("Astar_Ust", -0.5, 0.5, -0.15, 0.0, 2.12, 2.15, ceviz_koyu)
    k("Pervaz_Sol", -0.57, -0.47, 0.0, 0.016, 0, 2.22, ceviz_koyu); k("Pervaz_Sag", 0.47, 0.57, 0.0, 0.016, 0, 2.22, ceviz_koyu)
    k("Pervaz_Ust", -0.57, 0.57, 0.0, 0.016, 2.12, 2.22, ceviz_koyu)
    # Kanat ve iki panel çıtası
    k("Kanat", -0.47, 0.47, -0.09, -0.04, 0.01, 2.12, ceviz)
    for z0, z1 in [(0.2, 0.95), (1.15, 1.95)]:
        for (a, b, c, d) in [(-0.32, 0.32, z0, z0 + 0.035), (-0.32, 0.32, z1 - 0.035, z1), (-0.32, -0.285, z0, z1), (0.285, 0.32, z0, z1)]:
            k("Cita", a, b, -0.04, -0.03, c, d, ceviz_koyu)
    # Pirinç donanım: uzun ayna (rozet), kol, silindir, dürbün; kolun ucu elle parlamış
    k("Ayna", 0.33, 0.38, -0.04, -0.034, 0.9, 1.16, pirinc)
    k("Kol_Boyun", 0.345, 0.365, -0.034, 0.0, 1.07, 1.09, pirinc)
    k("Kol", 0.22, 0.365, 0.0, 0.016, 1.07, 1.092, pirinc)
    k("Silindir", 0.345, 0.365, -0.034, -0.026, 0.95, 0.985, pirinc)
    k("Durbun", -0.013, 0.013, -0.04, -0.028, 1.507, 1.533, pirinc)
    o = plaka(f"D{no}_No", P(0, -0.0335, 1.72), 0.085, 0.085, no_m, n, K); hucre(o, (no - 1) / HUCRE, 0, no / HUCRE, 1)
    k("Esik", -0.5, 0.5, -0.15, 0.02, 0.0, 0.015, mozaik)  # mozaik eşik: kanadın altındaki aralık karanlık boşluk göstermesin
    # Zil + isimlik (menteşe tarafı duvarda) ve ışık otomatiği (kol tarafında)
    k("Zil_Taban", -0.82, -0.75, 0.0, 0.012, 1.28, 1.42, beyaz_pl)
    k("Zil_Dugme", -0.798, -0.772, 0.012, 0.02, 1.3, 1.326, krem_pl)
    o = plaka(f"D{no}_Isimlik", P(-0.785, 0.0125, 1.38), 0.06, 0.06, isim_m, n, K); hucre(o, isim / HUCRE, 0, (isim + 1) / HUCRE, 1)
    k("Otomat_Taban", 0.72, 0.79, 0.0, 0.012, 1.17, 1.24, beyaz_pl)
    k("Otomat_Dugme", 0.737, 0.773, 0.012, 0.02, 1.187, 1.223, otomat_isik)
    if paspas is not None:
        o = plaka(f"D{no}_Paspas", (0, 0, 0), 0.62, 0.4, paspas_m, (0, 0, 1), K)
        hucre(o, 0, paspas * 0.5, 1, paspas * 0.5 + 0.5)
        o.location = P(random.uniform(-0.04, 0.04), 0.27, 0.013)
        o.rotation_euler.z += (math.pi / 2 if duvar == "sol" else -math.pi / 2) + random.uniform(-0.06, 0.06)
        kenar = k("Paspas_Kenar", -0.3, 0.3, 0.08, 0.46, 0, 0.012, krem_pl)
        kenar.rotation_euler.z = o.rotation_euler.z - (math.pi / 2 if duvar == "sol" else -math.pi / 2)
        kenar.location = o.location - Vector((0, 0, 0.007)); kenar.scale = (0.98, 0.98, 1)
        kenar.data.materials[0] = bpy.data.materials["M_K_Kupeste"]


for d in DAIRELER:
    kapi(*d)


def tasi(objs, kaynak, aci, hedef):
    """Nesneleri kaynak (x, y) noktası çevresinde z ekseninde aci kadar döndürüp hedefe taşır."""
    from mathutils import Matrix
    M = Matrix.Translation((hedef[0], hedef[1], 0)) @ Matrix.Rotation(aci, 4, "Z") @ Matrix.Translation((-kaynak[0], -kaynak[1], 0))
    bpy.context.view_layer.update()
    for o in objs:
        o.matrix_world = M @ o.matrix_world


for no, duvar, xc, isim, paspas in KOL_DAIRELER:
    once = set(koleksiyon(K).objects)
    kapi(no, "sol", 40.0, isim, paspas)  # boş bir yerde sol duvar düzeniyle kur
    yeni = [o for o in koleksiyon(K).objects if o not in once]
    if duvar == "kuzey":
        tasi(yeni, (0, 40.0), -math.pi / 2, (xc, 12.0))   # iç yüz +x -> -y
    else:
        tasi(yeni, (0, 40.0), math.pi / 2, (xc, KY0))     # iç yüz +x -> +y

# ── Asansör kapısı (y=0 duvarı, koridora +y bakar; boşluk x 0.4..1.5, z 0..2.2) ──
def a(ad, x0, x1, y0, y1, z0, z1, m):
    return kutu("Asansor_" + ad, ((x0 + x1) / 2, (y0 + y1) / 2, (z0 + z1) / 2), (x1 - x0, y1 - y0, z1 - z0), m, K)
a("Kasa_Sol", 0.4, 0.46, -0.15, 0.02, 0, 2.2, asansor_boya); a("Kasa_Sag", 1.44, 1.5, -0.15, 0.02, 0, 2.2, asansor_boya)
a("Kasa_Ust", 0.4, 1.5, -0.15, 0.02, 2.14, 2.2, asansor_boya)
a("Pervaz", 0.34, 1.56, 0.0, 0.012, 2.2, 2.28, asansor_boya)
for x in (0.34, 1.5):
    a("Pervaz_Yan", x, x + 0.06, 0.0, 0.012, 0, 2.28, asansor_boya)
# Kanat: dar camın etrafından parçalı (cam boşluğu x 0.98..1.1, z 1.15..1.9)
CX0, CX1, CZ0, CZ1 = 0.98, 1.1, 1.15, 1.9
for (x0, x1, z0, z1) in [(0.46, CX0, 0.01, 2.14), (CX1, 1.44, 0.01, 2.14), (CX0, CX1, 0.01, CZ0), (CX0, CX1, CZ1, 2.14)]:
    a("Kanat", x0, x1, -0.07, -0.03, z0, z1, asansor_boya)
a("Cam", CX0, CX1, -0.055, -0.045, CZ0, CZ1, tel_cam)
for (x0, x1, z0, z1) in [(CX0 - 0.015, CX1 + 0.015, CZ0 - 0.015, CZ0), (CX0 - 0.015, CX1 + 0.015, CZ1, CZ1 + 0.015),
                         (CX0 - 0.015, CX0, CZ0, CZ1), (CX1, CX1 + 0.015, CZ0, CZ1)]:
    a("Cam_Cerceve", x0, x1, -0.03, -0.022, z0, z1, celik)
a("Kuyu", 0.4, 1.5, -0.6, -0.5, -0.2, 2.6, karanlik)          # camın arkası: karanlık kuyu
a("Kol", 1.33, 1.355, 0.0, 0.02, 0.85, 1.35, celik)           # dikey çekme kolu
for z in (0.88, 1.32):
    a("Kol_Ayak", 1.335, 1.35, -0.03, 0.0, z - 0.01, z + 0.01, celik)
a("Gosterge_Kutu", 0.82, 1.08, -0.01, 0.05, 2.32, 2.45, celik)
plaka("Asansor_Gosterge", (0.95, 0.0505, 2.385), 0.22, 0.11, gosterge_m, (0, 1, 0), K)
a("Panel_Taban", 1.62, 1.73, -0.005, 0.006, 1.02, 1.26, celik)
plaka("Asansor_Panel", (1.675, 0.0065, 1.14), 0.1, 0.2, panel_m, (0, 1, 0), K)

# ── Lobi duvarı: duyuru (hafif eğri asılmış), kat levhası, ışık otomatiği ──
o = plaka("Duyuru", (0.008, 3.0, 1.62), 0.21, 0.297, duyuru_m, (1, 0, 0), K)  # boya şeridinin (z 1.4) üstünde; o.rotation_euler.rotate_axis("Y", 0.03)
plaka("Kat_Levha", (0.0075, 2.5, 1.75), 0.3, 0.15, levha_m, (1, 0, 0), K)
kutu("Kat_Levha_Arka", (0.003, 2.5, 1.75), (0.006, 0.31, 0.16), pvc, K)
kutu("Lobi_Otomat_Taban", (0.006, 0.3, 1.2), (0.012, 0.07, 0.07), beyaz_pl, K)
kutu("Lobi_Otomat_Dugme", (0.016, 0.3, 1.2), (0.008, 0.036, 0.036), otomat_isik, K)

# ── Yangın dolabı (sağ duvar, iki kapı arası) ──
YX, YY = 1.9, 8.4
# İçi boş sac kutu (arka, yanlar, üst, alt) + önde camlı kapak; içinde hortum makarası görünür
ON = YX - 0.18  # ön yüz
for ad, m, b in [("Arka", (YX - 0.005, YY, 1.27), (0.01, 0.62, 0.76)),
                 ("Yan", (YX - 0.09, YY - 0.305, 1.27), (0.18, 0.01, 0.76)), ("Yan", (YX - 0.09, YY + 0.305, 1.27), (0.18, 0.01, 0.76)),
                 ("Ust", (YX - 0.09, YY, 1.645), (0.18, 0.62, 0.01)), ("Alt", (YX - 0.09, YY, 0.895), (0.18, 0.62, 0.01))]:
    kutu("Yangin_" + ad, m, b, kirmizi, K)
# Kapak çerçevesi: üst bant (etiket), alt bant, iki yan; ortası cam
for (y0, y1, z0, z1) in [(-0.31, 0.31, 1.48, 1.65), (-0.31, 0.31, 0.89, 0.98), (-0.31, -0.24, 0.98, 1.48), (0.24, 0.31, 0.98, 1.48)]:
    kutu("Yangin_Kapak", (ON - 0.005, YY + (y0 + y1) / 2, (z0 + z1) / 2), (0.01, y1 - y0, z1 - z0), kirmizi, K)
kutu("Yangin_Cam", (ON - 0.004, YY, 1.23), (0.004, 0.48, 0.5), cam, K)
silindir("Yangin_Makara", (YX - 0.09, YY, 1.23), 0.2, 0.09, hortum, eksen="x", seg=48)
silindir("Yangin_Gobek", (YX - 0.09, YY, 1.23), 0.06, 0.11, celik, eksen="x")
kutu("Yangin_Lule", (YX - 0.1, YY + 0.15, 1.0), (0.04, 0.04, 0.12), celik, K)
plaka("Yangin_Etiket", (ON - 0.0105, YY, 1.565), 0.34, 0.15, yangin_m, (-1, 0, 0), K)

# ── Koridor sonu penceresi (y=12, boşluk x 0.45..1.45, z 0.95..2.15) + denizlik + petek ──
PX0, PX1, PZ0, PZ1 = 0.45, 1.45, 0.95, 2.15
for (x0, x1, z0, z1) in [(PX0, PX0 + 0.06, PZ0, PZ1), (PX1 - 0.06, PX1, PZ0, PZ1), (PX0, PX1, PZ1 - 0.06, PZ1),
                         (PX0, PX1, PZ0, PZ0 + 0.06), (0.92, 0.98, PZ0, PZ1)]:
    kutu("Pencere_Kasa", ((x0 + x1) / 2, 12.06, (z0 + z1) / 2), (x1 - x0, 0.07, z1 - z0), pvc, K)
kutu("Pencere_Cam", ((PX0 + PX1) / 2, 12.06, (PZ0 + PZ1) / 2), (PX1 - PX0 - 0.1, 0.008, PZ1 - PZ0 - 0.1), cam, K)
kutu("Pencere_Kol", (0.88, 12.015, 1.5), (0.02, 0.025, 0.12), pvc, K)
kutu("Denizlik", (0.95, 11.97, PZ0 - 0.015), (1.15, 0.2, 0.03), mozaik, K)
dis = plaka("Pencere_Disi", (0.95, 15.5, 1.6), 5.0, 3.75, gece_m, (0, -1, 0), K)
# Petek: 10 dilimli panel radyatör, duvardan 4 cm uzakta, iki ayaklı
for i in range(14):
    kutu("Petek_Dilim", (0.55 + i * 0.058, 11.86, 0.5), (0.045, 0.08, 0.56), petek, K)
for z in (0.25, 0.75):
    kutu("Petek_Boru", (0.95, 11.86, z), (0.83, 0.03, 0.03), petek, K)
silindir("Petek_Vana", (1.42, 11.86, 0.17), 0.016, 0.1, celik)

# Pencere ve petek y=12 duvarı için kuruldu; L kolunun ucundaki duvara (x=KX, iç yüz +x) taşı
tasi([o for o in koleksiyon(K).objects if o.name.startswith(("Pencere_", "Denizlik", "Petek_"))],
     (0.95, 12.0), math.pi / 2, (KX, (KY0 + 12.0) / 2))

# ── Süpürgelik: 8 cm mozaik, kapı ve asansör boşluklarında kesilir ──
def supurge(eksen, sabit, a0, a1, bosluklar):
    kes = sorted({a0, a1} | {b for d in bosluklar for b in d})
    for s0, s1 in zip(kes, kes[1:]):
        if any(d[0] <= (s0 + s1) / 2 <= d[1] for d in bosluklar):
            continue
        if eksen == "y":
            kutu("Supurgelik", (sabit, (s0 + s1) / 2, 0.04), (0.012, s1 - s0, 0.08), mozaik, K)
        else:
            kutu("Supurgelik", ((s0 + s1) / 2, sabit, 0.04), (s1 - s0, 0.012, 0.08), mozaik, K)
supurge("y", 0.006, 0, KY0, [(0.7, 1.9), (4.43, 5.57), (8.43, 9.57)])
supurge("y", 1.894, 3.4, 12, [(5.93, 7.07), (9.73, 10.87)])
supurge("x", 0.006, 0, 1.9, [(0.34, 1.56)])
supurge("x", 11.994, KX, 1.9, [(-7.75, -6.55), (-3.57, -2.43)])
supurge("x", KY0 + 0.006, KX, 0, [(-6.17, -5.03)])
supurge("y", KX + 0.006, KY0, 12, [])
print("koridor kapılar tamam", len(koleksiyon(K).objects))
