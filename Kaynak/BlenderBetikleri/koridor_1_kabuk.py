# Karanfil Apartmanı kat koridoru 1: kabuk (mozaik zemin, yağlı boya alt duvar, sıva üst duvar, tavan,
# gidiş-dönüşlü merdiven boşluğu, korkuluklar). Market kalite tarifiyle (market_ortak yardımcıları).
# Ölçüler (m): lobi x 0..1.9, y 0..3.4; koridor x 0..1.9, y 3.4..12; merdiven boşluğu x 1.9..4.2, y -1.5..3.4.
# Asansör kapısı y=0 duvarında, oyuncu asansörden +y'ye (koridora) bakarak çıkar. Tavan 2.75.
import bpy
exec(open(r"C:\Kisisel Dosyalarim\ozel\Oyun Gelistirme\Denemelik projelerim\Dink\Kaynak\BlenderBetikleri\market_ortak.py", encoding="utf-8").read())
DOKU = r"C:\Kisisel Dosyalarim\ozel\Oyun Gelistirme\Denemelik projelerim\Dink\Kaynak\Blender\Dokular\Koridor"
DOKU_BOYU.update({
    "old_mosaic_floor": 2.0,
    "painted_plaster_wall": 2.0,
    "beige_wall_001": 3.0,
    "pebble_embedded_concrete_02": 0.9,  # basamak mozaiği: gerçekte ince taneli, 2.15 iri çakıl gibiydi
    "black_walnut_veneer_02": 1.0,
})

K = "Koridor_Kabuk"
for o in list(koleksiyon(K).objects):
    bpy.data.objects.remove(o, do_unlink=True)

H, T, BOYA = 2.75, 0.15, 1.4  # tavan, duvar kalınlığı, yağlı boya bandının üstü
KAT = 2.72                     # kat yüksekliği (merdiven: 16 basamak x 0.17)

# Cilalı mozaik: yıllarca silinmiş, lambaları hafif yansıtır (mat hali çamurlu halı gibiydi)
zemin = pbr("M_K_Zemin_Mozaik", "old_mosaic_floor", parlaklik=1.0, doygunluk=0.8, puruz_carp=0.45)
# Yağlı boya: soluk mint yeşili, sıvadan daha parlak (yağlı boyanın hafif ipeksi yüzeyi)
boya = pbr("M_K_Duvar_YagliBoya", "painted_plaster_wall", renk_carp=(0.62, 0.78, 0.66), doygunluk=0.7, puruz_carp=0.55, normal_guc=1.2)
siva = pbr("M_K_Duvar_Siva", "beige_wall_001", renk_carp=(0.93, 0.95, 0.92), doygunluk=0.55, parlaklik=1.1, normal_guc=1.5)  # sıcak ışıkta pembeye kaçmasın
tavan = pbr("M_K_Tavan", "beige_wall_001", renk_carp=(1.0, 1.0, 0.97), parlaklik=1.3, normal_guc=1.0)
basamak = pbr("M_K_Basamak_Mozaik", "pebble_embedded_concrete_02", parlaklik=1.6, puruz_carp=0.75)
# Boya bandının üstünde koyu kahve şerit (eski apartmanlarda boyacının çektiği ayrım çizgisi)
serit = detay("M_K_Boya_Serit", "detay_plastik", (0.12, 0.07, 0.04), pmin=0.3, pmax=0.45, normal_guc=0.3, ton=0.08)
demir = detay("M_K_Demir_Siyah", "detay_cizik", (0.03, 0.03, 0.03), metal=0.6, pmin=0.35, pmax=0.6, normal_guc=0.2, ton=0.1)
kupeste = detay("M_K_Kupeste", "detay_plastik", (0.16, 0.08, 0.04), pmin=0.25, pmax=0.4, normal_guc=0.3, ton=0.08, yansima=0.4)


def duvar(ad, eksen, sabit, a, b, z0, z1, delikler=(), bant=True):
    """Duvar şeridi: eksen 'x' ise duvar x boyunca uzanır (yüzü ±y), 'y' ise y boyunca (yüzü ±x).
    sabit: duvarın orta çizgisi. delikler: (s0, s1, dz0, dz1) kapı/pencere boşlukları.
    Alt kısım yağlı boya, üstü sıva; sınırda koyu şerit."""
    kesim = sorted({a, b} | {d[0] for d in delikler} | {d[1] for d in delikler})
    for s0, s1 in zip(kesim, kesim[1:]):
        orta = (s0 + s1) / 2
        dolu = [(z0, z1)]
        for d in delikler:
            if d[0] <= orta <= d[1]:
                yeni = []
                for q0, q1 in dolu:
                    if d[3] <= q0 or d[2] >= q1:
                        yeni.append((q0, q1)); continue
                    if q0 < d[2]: yeni.append((q0, d[2]))
                    if d[3] < q1: yeni.append((d[3], q1))
                dolu = yeni
        for q0, q1 in dolu:
            parca = [(q0, min(q1, BOYA), boya), (max(q0, BOYA), q1, siva)] if bant else [(q0, q1, siva)]
            for p0, p1, m in parca:
                if p1 - p0 < 0.001:
                    continue
                if eksen == "x":
                    kutu(ad, (orta, sabit, (p0 + p1) / 2), (s1 - s0, T, p1 - p0), m, K)
                else:
                    kutu(ad, (sabit, orta, (p0 + p1) / 2), (T, s1 - s0, p1 - p0), m, K)


def serit_ciz(eksen, sabit, a, b, yon, delikler=()):
    """Boya sınırı şeridi; kapı boşluklarında kesilir. yon: odaya bakan taraf (+1/-1)."""
    kesim = sorted({a, b} | {d[0] for d in delikler} | {d[1] for d in delikler})
    for s0, s1 in zip(kesim, kesim[1:]):
        if any(d[0] <= (s0 + s1) / 2 <= d[1] and d[2] < BOYA < d[3] for d in delikler):
            continue
        k = sabit + yon * (T / 2 + 0.003)
        if eksen == "x":
            kutu("Boya_Serit", ((s0 + s1) / 2, k, BOYA), (s1 - s0, 0.006, 0.03), serit, K)
        else:
            kutu("Boya_Serit", (k, (s0 + s1) / 2, BOYA), (0.006, s1 - s0, 0.03), serit, K)


# ── Zemin: lobi + koridor, merdiven başı şeridi (merdiven boşluğu açık) ──
kutu("Zemin", (0.95, 6.0, -0.1), (1.9, 12.0, 0.2), zemin, K)
kutu("Zemin_MerdivenBasi", (3.05, 3.2, -0.1), (2.3, 0.4, 0.2), basamak, K)

# ── Duvarlar ──
KAPI = 1.0  # kasa dahil kapı boşluğu
sol_delik = [(5.0 - KAPI / 2, 5.0 + KAPI / 2, 0, 2.15), (9.0 - KAPI / 2, 9.0 + KAPI / 2, 0, 2.15)]
sag_delik = [(6.5 - KAPI / 2, 6.5 + KAPI / 2, 0, 2.15), (10.3 - KAPI / 2, 10.3 + KAPI / 2, 0, 2.15)]
asansor_delik = [(0.95 - 0.55, 0.95 + 0.55, 0, 2.2)]
pencere = [(0.45, 1.45, 0.95, 2.15)]
duvar("Duvar_Sol", "y", -T / 2, -T, 12.0 + T, 0, H, sol_delik)
duvar("Duvar_Sag", "y", 1.9 + T / 2, 3.4, 12.0 + T, 0, H, sag_delik)
duvar("Duvar_Asansor", "x", -T / 2, 0, 1.9, 0, H, asansor_delik)
duvar("Duvar_Son", "x", 12.0 + T / 2, 0, 1.9, 0, H, pencere)
serit_ciz("y", -T / 2, 0, 12.0, +1, sol_delik)
serit_ciz("y", 1.9 + T / 2, 3.4, 12.0, -1, sag_delik)
serit_ciz("x", -T / 2, 0, 1.9, +1, asansor_delik)
serit_ciz("x", 12.0 + T / 2, 0, 1.9, -1, pencere)

# Merdiven boşluğu duvarları: bir kat aşağıdan bir kat yukarıya kadar
ALT, UST = -1.6, KAT + 2.0
duvar("Merdiven_Duvar_Dis", "y", 4.2 + T / 2, -1.5 - T, 3.4 + T, ALT, UST, bant=False)
duvar("Merdiven_Duvar_Arka", "x", -1.5 - T / 2, 1.9 - T, 4.2, ALT, UST, bant=False)
duvar("Merdiven_Duvar_On", "x", 3.4 + T / 2, 1.9 + T / 2, 4.2, ALT, UST, bant=False)
duvar("Merdiven_Duvar_Kuyu", "y", 1.9 - T / 2, -1.5, 0, ALT, UST, bant=False)  # asansör kuyusunun arkası
kutu("Merdiven_Tavan", (3.05, 0.95, UST + 0.05), (2.3, 4.9, 0.1), tavan, K)

# ── Tavan: lobi + koridor; merdiven boşluğuna bakan kenarda kiriş ──
kutu("Tavan", (0.95, 6.0, H + 0.08), (1.9, 12.0, 0.16), tavan, K)
kutu("Tavan_Kiris", (1.9, 1.7, H - 0.12), (0.12, 3.4, 0.24), siva, K)
# Bir üst katın merdiven başı döşemesi (boşluktan yukarı bakınca görünür)
kutu("UstKat_MerdivenBasi", (3.05, 3.2, KAT - 0.1), (2.3, 0.4, 0.2), basamak, K)


# ── Merdivenler: gidiş-dönüşlü, 8+8 basamak (0.17 x 0.27) ──
R, G, N = 0.17, 0.27, 8


def kol(ad, x0, x1, y_bas, z_bas, yon_y, yon_z):
    """Bir merdiven kolu. yon_y: basamakların ilerleme yönü (±1), yon_z: çıkış (+1) ya da iniş (-1).
    Basamaklar mozaik, altı gerçek betonarme merdiven gibi eğik sıvalı plak (testere dişi değil)."""
    import math
    for i in range(N):
        ust = z_bas + yon_z * (i + 1) * R if yon_z > 0 else z_bas + yon_z * i * R - R
        ya = y_bas + yon_y * i * G
        yb = y_bas + yon_y * (i + 1) * G
        kutu(ad, ((x0 + x1) / 2, (ya + yb) / 2, ust - R / 2), (x1 - x0, G, R), basamak, K)
    # Eğik alt plak: basamak diplerinin altından geçer, 0.14 m kalın
    uz, yuk = N * G, N * R
    egim = math.atan2(yuk, uz)
    boy = math.hypot(uz, yuk)
    y_orta = y_bas + yon_y * uz / 2
    # Basamak diplerinin orta noktası: çıkışta yuk/2 - R/2, inişte -yuk/2 - 1.5R (ilk iniş basamağı -R'de başlar)
    z_orta = z_bas + yon_z * yuk / 2 - (0.5 * R if yon_z > 0 else 1.5 * R) - 0.07 / math.cos(egim)
    p = kutu(ad + "_AltPlak", ((x0 + x1) / 2, y_orta, z_orta), (x1 - x0, boy, 0.14), siva, K, uv=False)
    p.rotation_euler = (yon_y * yon_z * egim, 0, 0)
    return y_bas + yon_y * N * G


SAG_X, SOL_X = (2.95, 4.15), (1.95, 2.85)
y_son = kol("Merdiven_Cikis1", *SAG_X, 3.0, 0.0, -1, +1)                       # lobiden yukarı
kutu("Sahanlik_Ust", (3.05, (-1.5 + y_son) / 2, N * R - 0.09), (2.2, y_son + 1.5, 0.18), basamak, K)
kol("Merdiven_Cikis2", *SOL_X, y_son, N * R, +1, +1)                          # sahanlıktan üst kata
kol("Merdiven_Inis", *SOL_X, 3.0, 0.0, -1, -1)                                # lobiden aşağı
kutu("Sahanlik_Alt", (3.05, (-1.5 + y_son) / 2, -N * R - 0.09), (2.2, y_son + 1.5, 0.18), basamak, K)


# ── Korkuluklar: siyah demir dikmeler, kahve PVC küpeşte ──
def korkuluk(ad, p0, p1, h=0.95, ara=0.12):
    """p0 -> p1 (x, y, z) boyunca korkuluk; eğimli kolda zemin z'si basamak burnunu izler."""
    from mathutils import Vector
    a, b = Vector(p0), Vector(p1)
    uz = (b - a).length
    n = max(2, int(uz / ara))
    for i in range(n + 1):
        p = a.lerp(b, i / n)
        kutu(ad + "_Dikme", (p.x, p.y, p.z + h / 2), (0.014, 0.014, h), demir, K, uv=False)
    # Küpeşte: iki ucu birleştiren eğimli çubuk
    orta = (a + b) / 2 + Vector((0, 0, h + 0.025))
    o = kutu(ad + "_Kupeste", orta, (0.045, uz + 0.08, 0.05), kupeste, K, uv=False)
    yon = (b - a).normalized()
    o.rotation_euler = Vector((0, 1, 0)).rotation_difference(yon).to_euler()
    alt = kutu(ad + "_AltLama", (a + b) / 2 + Vector((0, 0, 0.06)), (0.03, uz, 0.008), demir, K, uv=False)
    alt.rotation_euler = o.rotation_euler


korkuluk("Korkuluk_Lobi", (1.95, 0.05, 0.0), (1.95, 3.0, 0.0))                 # lobiyle iniş boşluğu arası
korkuluk("Korkuluk_Inis", (2.9, 3.0, 0.0), (2.9, y_son, -N * R))                # iki kol arası, aşağı
korkuluk("Korkuluk_Cikis1", (2.9, 3.0, R), (2.9, y_son, N * R))                 # iki kol arası, yukarı
korkuluk("Korkuluk_Cikis2", (2.9, y_son, N * R + R), (2.9, 3.0, KAT))

print("koridor kabuk tamam", len(koleksiyon(K).objects))
