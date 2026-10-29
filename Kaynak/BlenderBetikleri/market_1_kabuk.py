# Market odası 1/3: kabuk (zemin, duvarlar, asma tavan, süpürgelik, kapı ve vitrin boşluğu).
# İç ölçü 6.0 x 4.5 m, tavan 2.9 m. Ön duvar y=0 (sokak), arka duvar y=4.5. Oda x=0..6.
import bpy
exec(open(r"C:\Kisisel Dosyalarim\ozel\Oyun Gelistirme\Denemelik projelerim\Dink\Kaynak\BlenderBetikleri\market_ortak.py", encoding="utf-8").read())

G, D, H, T = 6.0, 4.5, 2.9, 0.15  # genişlik, derinlik, yükseklik, duvar kalınlığı

# Eski kabuğu sil (betik tekrar çalıştırılabilsin)
for o in list(koleksiyon("Market_Kabuk").objects):
    bpy.data.objects.remove(o, do_unlink=True)
K = "Market_Kabuk"

zemin = pbr("M_Zemin_Granit", "granite_tile", parlaklik=0.95, puruz_carp=0.85)
duvar = pbr("M_Duvar_Siva", "plastered_wall_04", renk_carp=(0.93, 0.9, 0.82), parlaklik=0.95)
fayans = pbr("M_Duvar_Fayans", "long_white_tiles", renk_carp=(0.95, 0.95, 0.92))
tavan = pbr("M_Tavan_Panel", "polystyrene", parlaklik=0.9)
supurge = duz("M_Supurgelik", (0.08, 0.08, 0.08), puruz=0.6)
profil = duz("M_Tavan_Profil", (0.75, 0.75, 0.73), puruz=0.35, metal=1.0)

kutu("Zemin", (G / 2, D / 2, -0.05), (G + 2 * T, D + 2 * T, 0.1), zemin, K)
kutu("Tavan", (G / 2, D / 2, H + 0.02), (G, D, 0.04), tavan, K)

# Arka ve yan duvarlar: üst sıva, alt 1.1 m fayans (market ve kasap tezgâhlarının arkası gibi)
ALT = 1.1
for ad, m, b in [
    ("Duvar_Arka", (G / 2, D + T / 2, 0), (G + 2 * T, T, 0)),
    ("Duvar_Sol", (-T / 2, D / 2, 0), (T, D, 0)),
    ("Duvar_Sag", (G + T / 2, D / 2, 0), (T, D, 0)),
]:
    kutu(ad + "_Alt", (m[0], m[1], ALT / 2), (b[0], b[1], ALT), fayans, K)
    kutu(ad + "_Ust", (m[0], m[1], ALT + (H - ALT) / 2), (b[0], b[1], H - ALT), duvar, K)

# Ön duvar: kapı (x 0.5..1.5, h 2.2) ve vitrin (x 2.0..5.6, z 0.8..2.3) boşlukları
on_y = -T / 2
parcalar = [
    ((0.25, on_y, H / 2), (0.5 + 0.0, T, H)),               # kapı solu
    ((1.0, on_y, 2.2 + (H - 2.2) / 2), (1.0, T, H - 2.2)),  # kapı üstü
    ((1.75, on_y, H / 2), (0.5, T, H)),                     # kapı ile vitrin arası
    ((3.8, on_y, 0.4), (3.6, T, 0.8)),                      # vitrin altı
    ((3.8, on_y, 2.3 + (H - 2.3) / 2), (3.6, T, H - 2.3)),  # vitrin üstü
    ((5.8, on_y, H / 2), (0.4 + 2 * T, T, H)),              # sağ köşe
]
for i, (m, b) in enumerate(parcalar):
    kutu(f"Duvar_On_{i}", m, b, duvar, K)

# Süpürgelik
for ad, m, b in [
    ("Supurgelik_Arka", (G / 2, D - 0.005, 0.05), (G, 0.01, 0.1)),
    ("Supurgelik_Sol", (0.005, D / 2, 0.05), (0.01, D, 0.1)),
    ("Supurgelik_Sag", (G - 0.005, D / 2, 0.05), (0.01, D, 0.1)),
]:
    kutu(ad, m, b, supurge, K, uv=False)

# Asma tavan profilleri: 60 cm ızgara
x = 0.0
while x <= G + 0.001:
    kutu(f"Profil_X_{x:.1f}", (x, D / 2, H - 0.005), (0.024, D, 0.01), profil, K, uv=False); x += 0.6
y = 0.0
while y <= D + 0.001:
    kutu(f"Profil_Y_{y:.1f}", (G / 2, y, H - 0.005), (G, 0.024, 0.01), profil, K, uv=False); y += 0.6

print("kabuk tamam", len(koleksiyon(K).objects))
