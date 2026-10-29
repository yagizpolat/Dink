# Market odası 2/3: duvar rafları, orta ada, kasa tezgâhı, içecek dolabı.
import bpy
exec(open(r"C:\Kisisel Dosyalarim\ozel\Oyun Gelistirme\Denemelik projelerim\Dink\Kaynak\BlenderBetikleri\market_ortak.py", encoding="utf-8").read())
from mathutils import Matrix

K = "Market_Mobilya"
for o in list(koleksiyon(K).objects):
    bpy.data.objects.remove(o, do_unlink=True)

# Raf metali: mavi boyalı sacın gerçek yüzey dokusu, rengi kırık beyaza çekilmiş
raf = pbr("M_Raf_Boyali", "blue_metal_plate", renk_carp=(0.86, 0.86, 0.83), doygunluk=0.0, parlaklik=3.0, puruz_carp=0.75, metal=0.0, normal_guc=0.35)
# Toz boya: fotoğraf dokusunun lekeleri yarıya indirilir, düz boya rengine karıştırılır
raf_bsdf = next(n for n in raf.node_tree.nodes if n.type == "BSDF_PRINCIPLED")
_kaynak = raf_bsdf.inputs["Base Color"].links[0].from_socket
_kar = raf.node_tree.nodes.new("ShaderNodeMix"); _kar.data_type = "RGBA"; _kar.inputs["Factor"].default_value = 0.55
raf.node_tree.links.new(_kaynak, _kar.inputs[6]); _kar.inputs[7].default_value = (0.78, 0.79, 0.77, 1)
raf.node_tree.links.new(_kar.outputs[2], raf_bsdf.inputs["Base Color"])
raf_koyu = pbr("M_Raf_Etek", "blue_metal_plate", renk_carp=(0.32, 0.33, 0.34), doygunluk=0.0, parlaklik=1.6, normal_guc=0.6)
ahsap = pbr("M_Tezgah_Ahsap", "laminate_floor_02", parlaklik=0.75, doygunluk=0.85)
tezgah_ust = duz("M_Tezgah_Ust", (0.18, 0.18, 0.17), puruz=0.35)
plastik_koyu = duz("M_Plastik_Koyu", (0.03, 0.03, 0.03), puruz=0.45)
cam = duz("M_Cam", (0.9, 0.95, 0.95), puruz=0.02, cam=True)
dolap_ici = duz("M_Dolap_Isik", (0.85, 0.92, 1.0), isik=6.0)
krom = duz("M_Krom", (0.8, 0.8, 0.8), puruz=0.15, metal=1.0)


def birlestir(ad, parcalar, konum=(0, 0, 0), donus=0.0):
    """Parçaları tek nesnede toplar (Unity'de tek mesh, az çizim çağrısı)."""
    for p in parcalar:
        p.select_set(True)
    bpy.context.view_layer.objects.active = parcalar[0]
    with bpy.context.temp_override(active_object=parcalar[0], selected_editable_objects=parcalar):
        bpy.ops.object.join()
    o = parcalar[0]
    o.name = ad
    o.data.name = ad
    return o


def raf_birimi(ad, x0, y0, en, derin, boy, katlar, cift_tarafli=False):
    """Duvar/ada rafı: ayaklar, arka pano, etek, raf tablaları ve ön dudak. Yerel x=en boyunca, y=derinlik."""
    p = []
    yk = derin
    # Ayaklar
    for xx in (0.02, en - 0.02):
        p.append(kutu(f"{ad}_ayak", (x0 + xx, y0 + derin - 0.02, boy / 2), (0.04, 0.04, boy), raf, K))
    # Arka pano (delikli görünüm normal haritadan gelir)
    p.append(kutu(f"{ad}_pano", (x0 + en / 2, y0 + derin - 0.01, boy / 2 + 0.05), (en - 0.06, 0.012, boy - 0.1), raf, K))
    # Etek
    p.append(kutu(f"{ad}_etek", (x0 + en / 2, y0 + 0.03, 0.06), (en, 0.02, 0.12), raf_koyu, K))
    for z in katlar:
        p.append(kutu(f"{ad}_tabla", (x0 + en / 2, y0 + yk / 2, z), (en - 0.02, yk - 0.02, 0.022), raf, K))
        p.append(kutu(f"{ad}_dudak", (x0 + en / 2, y0 + 0.015, z + 0.025), (en - 0.02, 0.008, 0.05), raf, K))
    return p


KATLAR = [0.14, 0.52, 0.9, 1.28, 1.66]

# Arka duvar: 5 birim x 1.0 m
for i in range(5):
    x0 = 0.5 + i * 1.0
    parca = raf_birimi(f"RafArka{i}", x0, 0, 1.0, 0.45, 2.0, KATLAR)
    o = birlestir(f"Raf_Arka_{i}", parca)
    # Yerel ön (y=0) zaten odaya (-y) bakıyor; pano arka duvara dayanır
    o.matrix_world = Matrix.Translation((0, 4.5 - 0.45, 0)) @ o.matrix_world
# Sol duvar: 3 birim (y 1.1..4.1); +90° dönüşle ön +x'e (odaya) bakar
for i in range(3):
    parca = raf_birimi(f"RafSol{i}", 0, 0, 1.0, 0.45, 2.0, KATLAR)
    o = birlestir(f"Raf_Sol_{i}", parca)
    o.matrix_world = Matrix.Translation((0.45, 1.1 + i * 1.0, 0)) @ Matrix.Rotation(1.5707963, 4, "Z") @ o.matrix_world
# Orta ada: 2 birim x iki taraf, 1.45 m
ADA_KAT = [0.14, 0.5, 0.86, 1.22]
for taraf in (0, 1):
    for i in range(2):
        parca = raf_birimi(f"Ada{taraf}{i}", 0, 0, 1.0, 0.4, 1.45, ADA_KAT)
        o = birlestir(f"Ada_{taraf}_{i}", parca)
        if taraf == 0:
            o.matrix_world = Matrix.Translation((2.2 + i * 1.0, 2.0, 0)) @ o.matrix_world
        else:
            o.matrix_world = Matrix.Translation((2.2 + (i + 1) * 1.0, 2.8, 0)) @ Matrix.Rotation(3.14159265, 4, "Z") @ o.matrix_world

# Kasa tezgâhı: ön vitrinin önünde, kasiyer duvar tarafında
tz = []
tz.append(kutu("Tezgah_On", (4.4, 0.95, 0.47), (1.8, 0.04, 0.94), ahsap, K))
tz.append(kutu("Tezgah_Yan1", (3.52, 0.68, 0.47), (0.04, 0.58, 0.94), ahsap, K))
tz.append(kutu("Tezgah_Yan2", (5.28, 0.68, 0.47), (0.04, 0.58, 0.94), ahsap, K))
tz.append(kutu("Tezgah_UstT", (4.4, 0.7, 0.955), (1.86, 0.66, 0.03), tezgah_ust, K, uv=False))
tz.append(kutu("Tezgah_Baza", (4.4, 0.92, 0.04), (1.8, 0.06, 0.08), plastik_koyu, K, uv=False))
birlestir("Kasa_Tezgahi", tz)
# Yazar kasa ve fiş yazıcısı (Kat1 fiş görevi buraya bağlanacak)
yk = [kutu("YK_Govde", (4.0, 0.62, 1.02), (0.36, 0.32, 0.1), plastik_koyu, K, uv=False),
      kutu("YK_Ekran", (4.0, 0.55, 1.2), (0.3, 0.03, 0.2), plastik_koyu, K, uv=False)]
birlestir("Yazar_Kasa", yk)
kutu("Fis_Yazici", (4.55, 0.6, 1.02), (0.16, 0.2, 0.12), plastik_koyu, K, uv=False)

# İçecek dolabı: sağ duvar, arka köşeye yakın
dl = []
dl.append(kutu("Dolap_Govde_Arka", (5.95, 3.3, 1.0), (0.05, 1.3, 2.0), raf_koyu, K))
dl.append(kutu("Dolap_Govde_Ust", (5.65, 3.3, 2.0), (0.65, 1.3, 0.12), raf_koyu, K))
dl.append(kutu("Dolap_Govde_Alt", (5.65, 3.3, 0.08), (0.65, 1.3, 0.16), raf_koyu, K))
for yy in (2.67, 3.93):
    dl.append(kutu("Dolap_Govde_Yan", (5.65, yy, 1.0), (0.65, 0.04, 2.0), raf_koyu, K))
for z in (0.55, 0.95, 1.35, 1.72):
    dl.append(kutu("Dolap_Tel", (5.68, 3.3, z), (0.55, 1.22, 0.01), krom, K, uv=False))
birlestir("Icecek_Dolabi", dl)
kutu("Dolap_Ic_Isik", (5.33, 3.3, 1.94), (0.04, 1.2, 0.02), dolap_ici, K, uv=False)
kutu("Dolap_Cam", (5.32, 3.3, 1.04), (0.02, 1.22, 1.76), cam, K, uv=False)
kutu("Dolap_Kulp", (5.29, 2.8, 1.1), (0.02, 0.03, 0.5), krom, K, uv=False)

print("mobilya tamam", len(koleksiyon(K).objects))
