# Karanfil Apartmanı kat koridoru 5: köşe geçidi (Exit-8 tarzı kat döngüsü). İki kez sağa dönen U geçit; iki eş
# kopya: biri L kolunun kuzey duvarında (girilir), biri lobinin batı duvarında (çıkılır). Oyuncu iki dönemecin
# ortasında, iki ağız da görünmezken bir kopyadan ötekine taşınır (Unity: FloorLoop). Kopyalar birebir aynı olmalı.
# Yerel eksenler: f = ağızdan içeri, r = içeri bakınca sağ. Ağız A (giriş) r -0.6..0.6, ağız B (çıkış) r 2.2..3.4.
# koridor_1/2/3'ten sonra çalışır (malzemeler onlardan).
import bpy, bmesh
exec(open(r"C:\Kisisel Dosyalarim\ozel\Oyun Gelistirme\Denemelik projelerim\Dink\Kaynak\BlenderBetikleri\market_ortak.py", encoding="utf-8").read())
from mathutils import Vector

K = "Koridor_Gecit"
for o in list(koleksiyon(K).objects):
    bpy.data.objects.remove(o, do_unlink=True)

H, BOYA = 2.75, 1.4
M = bpy.data.materials
boya, siva, tavan = M["M_K_Duvar_YagliBoya"], M["M_K_Duvar_Siva"], M["M_K_Tavan"]
zemin, mozaik, serit = M["M_K_Zemin_Mozaik"], M["M_K_Basamak_Mozaik"], M["M_K_Boya_Serit"]
taban, pirinc, opal = M["M_K_Armatur_Taban"], M["M_K_Pirinc"], M["M_K_Opal_6.0"]
SICAK = (1.0, 0.72, 0.45)


def gecit(ad, O, F, R, kapali):
    """O: ağız düzleminin (duvarın koridor yüzü) ortası; F, R: dünya birim vektörleri. kapali: {'A','B'} alt kümesi."""
    O, F, R = Vector((*O, 0)), Vector((*F, 0)), Vector((*R, 0))

    def kutu_l(isim, f0, f1, r0, r1, z0, z1, m):
        merkez = O + F * ((f0 + f1) / 2) + R * ((r0 + r1) / 2) + Vector((0, 0, (z0 + z1) / 2))
        df, dr = abs(f1 - f0), abs(r1 - r0)
        boy = (df, dr) if abs(F.x) > 0.5 else (dr, df)   # F x ekseninde mi, y ekseninde mi
        return kutu(f"{ad}_{isim}", merkez, (boy[0], boy[1], z1 - z0), m, K)

    def duvar_l(isim, f0, f1, r0, r1):
        kutu_l(isim, f0, f1, r0, r1, 0, BOYA, boya)
        kutu_l(isim, f0, f1, r0, r1, BOYA, H, siva)

    def yuz(f0, f1, r0, r1, yon):
        """Bir duvar yüzü boyunca boya şeridi ve süpürgelik. yon: yüzün baktığı yerel eksen ('+f','-f','+r','-r')."""
        e = 0.003
        if yon[1] == "f":
            f = f0 + (e if yon[0] == "+" else -e)
            kutu_l("Serit", f - 0.003, f + 0.003, r0, r1, BOYA - 0.015, BOYA + 0.015, serit)
            kutu_l("Supurgelik", f - 0.006, f + 0.006, r0, r1, 0, 0.08, mozaik)
        else:
            r = r0 + (e if yon[0] == "+" else -e)
            kutu_l("Serit", f0, f1, r - 0.003, r + 0.003, BOYA - 0.015, BOYA + 0.015, serit)
            kutu_l("Supurgelik", f0, f1, r - 0.006, r + 0.006, 0, 0.08, mozaik)

    # Döşeme, tavan
    kutu_l("Zemin", 0, 3.5, -0.75, 3.55, -0.2, 0, zemin)
    kutu_l("Tavan", 0.15, 3.5, -0.75, 3.55, H, H + 0.16, tavan)
    # Duvarlar: ortadaki dolu blok (iki dönemeci birbirinden saklar), arka, iki yan
    duvar_l("Blok", 0.15, 2.15, 0.6, 2.2)
    duvar_l("Arka", 3.35, 3.5, -0.75, 3.55)
    duvar_l("Yan", 0.15, 3.35, -0.75, -0.6)
    duvar_l("Yan", 0.15, 3.35, 3.4, 3.55)
    if "A" in kapali and ad.endswith("Cikis"):   # çıkış kopyasında A'nın ardında koridor duvarı yok: kapak
        duvar_l("Kapak", 0, 0.15, -0.6, 0.6)
    # Yüzler (iç taraf)
    yuz(0.15, 2.15, 0.6, 0.6, "-r"); yuz(0.15, 2.15, 2.2, 2.2, "+r"); yuz(2.15, 2.15, 0.6, 2.2, "+f")
    yuz(3.35, 3.35, -0.6, 3.4, "-f"); yuz(0.15, 3.35, -0.6, -0.6, "+r"); yuz(0.15, 3.35, 3.4, 3.4, "-r")
    for agiz, (r0, r1) in (("A", (-0.6, 0.6)), ("B", (2.2, 3.4))):
        if agiz in kapali:
            yuz(0.15, 0.15, r0, r1, "+f")
    # Tavan lambası dönemeçlerin ortasında (iki kopyada aynı yerde)
    p = O + F * 2.75 + R * 1.4
    kutu(f"{ad}_Lamba_Taban", (p.x, p.y, H - 0.015), (0.16, 0.16, 0.03), taban, K)
    me = bpy.data.meshes.new(f"{ad}_Lamba_Kure"); bm = bmesh.new()
    bmesh.ops.create_uvsphere(bm, u_segments=32, v_segments=16, radius=0.13)
    bm.to_mesh(me); bm.free()
    for poly in me.polygons: poly.use_smooth = True
    o = bpy.data.objects.new(f"{ad}_Lamba_Kure", me); o.location = (p.x, p.y, H - 0.19); koleksiyon(K).objects.link(o)
    o.visible_shadow = False
    me.materials.append(opal)
    ld = bpy.data.lights.new(f"{ad}_Lamba_Isik", "POINT"); ld.energy = 120; ld.color = SICAK; ld.shadow_soft_size = 0.12
    lo = bpy.data.objects.new(f"{ad}_Lamba_Isik", ld); lo.location = (p.x, p.y, H - 0.19); koleksiyon(K).objects.link(lo)


# Giriş: L kolunun kuzey duvarı (y=12), köşenin yanında; içeri +y, sağ +x. B'nin ardı kol duvarı (kapalı).
gecit("Gecit_Giris", (-7.15, 12.0), (0, 1), (1, 0), {"B"})
# Çıkış: lobinin batı duvarı (x=0); içeri -x, sağ +y. A'nın ardında duvar yok (kapak), B lobiye açılır.
gecit("Gecit_Cikis", (0.0, -1.5), (-1, 0), (0, 1), {"A"})
print("koridor geçit tamam", len(koleksiyon(K).objects))
