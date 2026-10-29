# Market odası 5: vitrin ve kapı doğraması, camlar, dışarıda gece sokağı; tezgâh ve yazar kasa ayrıntısı.
import bpy, math
exec(open(r"C:\Kisisel Dosyalarim\ozel\Oyun Gelistirme\Denemelik projelerim\Dink\Kaynak\BlenderBetikleri\market_ortak.py", encoding="utf-8").read())

K = "Market_Cephe"
for o in list(koleksiyon(K).objects):
    bpy.data.objects.remove(o, do_unlink=True)

alu = duz("M_Aluminyum", (0.62, 0.63, 0.64), puruz=0.32, metal=1.0)
vitrin_cam = cam_kur("M_Vitrin_Cam")
dolap_cam = cam_kur("M_Cam")  # içecek dolabı camı da aynı kurulum
# Tek düz panel: blended sıralama sorunu çıkarmaz; dithered iken arkadaki saydam şişelerle üst üste binip karıncalanıyordu
dolap_cam.surface_render_method = "BLENDED"; dolap_cam.use_backface_culling = True
asfalt = pbr("M_Sokak_Kaldirim", "granite_tile", parlaklik=0.25, puruz_carp=0.6)
gece_duvar = pbr("M_Sokak_Duvar", "plastered_wall_04", renk_carp=(0.45, 0.42, 0.4), parlaklik=0.5)

y = -0.075  # ön duvarın ortası
P = 0.05    # profil kalınlığı

def cerceve(ad, x0, x1, z0, z1, kayit=None):
    parca = [
        kutu(ad + "_Sol", (x0 + P / 2, y, (z0 + z1) / 2), (P, 0.08, z1 - z0), alu, K, uv=False),
        kutu(ad + "_Sag", (x1 - P / 2, y, (z0 + z1) / 2), (P, 0.08, z1 - z0), alu, K, uv=False),
        kutu(ad + "_Ust", ((x0 + x1) / 2, y, z1 - P / 2), (x1 - x0, 0.08, P), alu, K, uv=False),
        kutu(ad + "_Alt", ((x0 + x1) / 2, y, z0 + P / 2), (x1 - x0, 0.08, P), alu, K, uv=False),
    ]
    for xx in (kayit or []):
        parca.append(kutu(ad + "_Kayit", (xx, y, (z0 + z1) / 2), (P * 0.8, 0.08, z1 - z0), alu, K, uv=False))
    kutu(ad + "_Cam", ((x0 + x1) / 2, y, (z0 + z1) / 2), (x1 - x0 - 2 * P, 0.01, z1 - z0 - 2 * P), vitrin_cam, K, uv=False)

cerceve("Vitrin", 2.0, 5.6, 0.8, 2.3, kayit=[3.2, 4.4])
# Kapı: kasa + iki kanatlı alüminyum kapı, ortada itme kolu
cerceve("Kapi", 0.5, 1.5, 0.0, 2.2, kayit=[1.0])
kutu("Kapi_Kol_1", (0.93, y - 0.06, 1.05), (0.025, 0.025, 0.5), alu, K, uv=False)
kutu("Kapi_Kol_2", (1.07, y - 0.06, 1.05), (0.025, 0.025, 0.5), alu, K, uv=False)
kutu("Kapi_Esik", (1.0, y, 0.01), (1.0, 0.2, 0.02), alu, K, uv=False)

# Dışarısı: kaldırım, karşı bina duvarı; uzakta tek sokak lambasının turuncu ışığı
kutu("Sokak_Kaldirim", (3.0, -2.0, -0.08), (10.0, 3.8, 0.1), asfalt, K)
kutu("Sokak_KarsiDuvar", (3.0, -4.2, 2.0), (10.0, 0.3, 4.5), gece_duvar, K)
ld = bpy.data.lights.new("Sokak_Lambasi", "POINT"); ld.energy = 160; ld.color = (1.0, 0.6, 0.28); ld.shadow_soft_size = 0.15
lo = bpy.data.objects.new("Sokak_Lambasi", ld); lo.location = (5.2, -2.6, 3.4)
koleksiyon(K).objects.link(lo)

# Tezgâh: parke dokusu yerine rafla aynı toz boyalı metal gövde, koyu laminat üst
tez = bpy.data.objects.get("Kasa_Tezgahi")
if tez:
    raf = bpy.data.materials["M_Raf_Boyali"]
    for i, m in enumerate(tez.data.materials):
        if m and m.name == "M_Tezgah_Ahsap":
            tez.data.materials[i] = raf

# Yazar kasa: ışıklı ekran, tuş takımı, para çekmecesi çizgisi
ekran = duz("M_YK_Ekran", (0.05, 0.12, 0.1), puruz=0.1, isik=1.4, isik_renk=(0.35, 0.9, 0.6))
tus = duz("M_YK_Tus", (0.55, 0.55, 0.52), puruz=0.5)
kutu("YK_EkranYuz", (4.0, 0.535, 1.21), (0.26, 0.004, 0.15), ekran, K, uv=False)
for r in range(4):
    for c in range(4):
        kutu("YK_Tus", (3.9 + c * 0.05, 0.53 + r * 0.04, 1.075), (0.035, 0.03, 0.01), tus, K, uv=False)
kutu("YK_Cekmece", (4.0, 0.46, 0.98), (0.34, 0.004, 0.004), tus, K, uv=False)
print("cephe tamam", len(koleksiyon(K).objects))
