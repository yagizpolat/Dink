# Hafiza marketi G1: sag duvarda (x=6) kasa ile icecek dolabi arasinda cift kanatli depo kapisi.
# Kapi ardinda kisa beton gecit kutusu (G2'de koridora uzayacak). Tekrar calistirilabilir.
import bpy, math

KOL = "Market_Depo"
Y0, Y1, H = 1.35, 2.35, 2.1      # aciklik (y araligi, yukseklik)
X_IC, X_DIS = 6.0, 6.15          # duvar kalinligi
DERIN = 2.4                      # gecit kutusu derinligi (+x)

col = bpy.data.collections.get(KOL)
if col is None:
    col = bpy.data.collections.new(KOL); bpy.context.scene.collection.children.link(col)
for o in list(col.objects): bpy.data.objects.remove(o, do_unlink=True)

def mat(n): return bpy.data.materials[n]

def kutu(ad, mn, mx, m):
    me = bpy.data.meshes.new(ad)
    x0,y0,z0 = mn; x1,y1,z1 = mx
    v = [(x0,y0,z0),(x1,y0,z0),(x1,y1,z0),(x0,y1,z0),(x0,y0,z1),(x1,y0,z1),(x1,y1,z1),(x0,y1,z1)]
    f = [(0,3,2,1),(4,5,6,7),(0,1,5,4),(1,2,6,5),(2,3,7,6),(3,0,4,7)]
    me.from_pydata(v, [], f); me.materials.append(mat(m))
    o = bpy.data.objects.new(ad, me); col.objects.link(o)
    bpy.context.view_layer.objects.active = o
    o.select_set(True); bpy.ops.object.mode_set(mode='EDIT'); bpy.ops.mesh.select_all(action='SELECT')
    bpy.ops.uv.cube_project(cube_size=1.0); bpy.ops.object.mode_set(mode='OBJECT'); o.select_set(False)
    return o

# eski sag duvari aciklikli parcalara bol (orijinaller gizlenir, silinmez)
for n in ("Duvar_Sag_Alt", "Duvar_Sag_Ust", "Supurgelik_Sag"):
    o = bpy.data.objects[n]; o.hide_viewport = o.hide_render = True
kutu("Duvar_Sag_Alt_A", (X_IC,0,0), (X_DIS,Y0,1.1), "M_Duvar_Fayans")
kutu("Duvar_Sag_Alt_B", (X_IC,Y1,0), (X_DIS,4.5,1.1), "M_Duvar_Fayans")
kutu("Duvar_Sag_Ust_A", (X_IC,0,1.1), (X_DIS,Y0,2.9), "M_Duvar_Siva")
kutu("Duvar_Sag_Ust_B", (X_IC,Y1,1.1), (X_DIS,4.5,2.9), "M_Duvar_Siva")
kutu("Duvar_Sag_Lento", (X_IC,Y0,H), (X_DIS,Y1,2.9), "M_Duvar_Siva")
kutu("Supurgelik_Sag_A", (5.99,0,0), (6.0,Y0-0.05,0.1), "M_Supurgelik")
kutu("Supurgelik_Sag_B", (5.99,Y1+0.05,0), (6.0,4.5,0.1), "M_Supurgelik")

# kapi boyasi: raf boyasinin koyulastirilmis, kirli gri-yesil kopyasi
if "M_DepoKapi_Boya" in bpy.data.materials: bpy.data.materials.remove(bpy.data.materials["M_DepoKapi_Boya"])
km = mat("M_Raf_Boyali").copy(); km.name = "M_DepoKapi_Boya"
nt = km.node_tree; bsdf = next(n for n in nt.nodes if n.type == "BSDF_PRINCIPLED")
renk = bsdf.inputs["Base Color"]
mix = nt.nodes.new("ShaderNodeMix"); mix.data_type = "RGBA"; mix.blend_type = "MULTIPLY"
mix.inputs[0].default_value = 1.0; mix.inputs[7].default_value = (0.42, 0.46, 0.43, 1)
if renk.is_linked:
    nt.links.new(renk.links[0].from_socket, mix.inputs[6])
else:
    mix.inputs[6].default_value = renk.default_value
nt.links.new(mix.outputs[2], renk)
bsdf.inputs["Roughness"].default_value = 0.7

# kasa (aluminyum pervaz)
P = 0.05
kutu("DepoKapi_Pervaz_A", (5.98,Y0-P,0), (X_DIS+0.01,Y0,H+P), "M_Aluminyum")
kutu("DepoKapi_Pervaz_B", (5.98,Y1,0), (X_DIS+0.01,Y1+P,H+P), "M_Aluminyum")
kutu("DepoKapi_Pervaz_Ust", (5.98,Y0,H), (X_DIS+0.01,Y1,H+P), "M_Aluminyum")

# kanatlar: menteseler aciklik kenarlarinda, depoya (+x) acilir; govde boyali, alt tekme plakasi koyu plastik
W = (Y1 - Y0) / 2 - 0.005
def kanat(ad, mentese_y, yon, aci):
    bos = bpy.data.objects.new(ad, None); col.objects.link(bos)
    bos.location = (X_IC + 0.08, mentese_y, 0.02)
    bos.rotation_euler = (0, 0, math.radians(aci) * yon)
    ys = sorted([0, -yon * W])
    a, b = ys; Z0, Z1, K = 1.3, 1.72, 0.14     # dikey dar pencere
    parcalar = [
        kutu(ad + "_Govde", (-0.02, a, 0), (0.02, b, Z0), "M_DepoKapi_Boya"),
        kutu(ad + "_GovdeUst", (-0.02, a, Z1), (0.02, b, H - 0.04), "M_DepoKapi_Boya"),
        kutu(ad + "_GovdeA", (-0.02, a, Z0), (0.02, a + K, Z1), "M_DepoKapi_Boya"),
        kutu(ad + "_GovdeB", (-0.02, b - K, Z0), (0.02, b, Z1), "M_DepoKapi_Boya"),
        kutu(ad + "_Cam", (-0.004, a + K, Z0), (0.004, b - K, Z1), "M_Cam"),
    ]
    g = parcalar[0]
    t = kutu(ad + "_Tampon", (-0.025, ys[0], 0.0), (0.025, ys[1], 0.35), "M_Plastik_Koyu")
    k = kutu(ad + "_Kenar", (-0.025, min(ys)+ (0 if yon>0 else W-0.03), 0), (0.025, min(ys)+(0.03 if yon>0 else W), H-0.04), "M_Plastik_Koyu")
    for c in parcalar + [t, k]: c.parent = bos
    return bos
kanat("DepoKapi_KanatSol", Y0, -1, 0.0)
kanat("DepoKapi_KanatSag", Y1, 1, 0.0)   # Unity raf dolunca aralar (DepoKapiSizinti)

# kapi ardinda beton gecit kutusu (ici karanlik, G2'de uzar)
kutu("DepoGecit_Zemin", (X_DIS,Y0-0.6,-0.1), (X_DIS+DERIN,Y1+0.6,0.0), "M_Zemin_Granit")
kutu("DepoGecit_Duvar_A", (X_DIS,Y0-0.75,0), (X_DIS+DERIN,Y0-0.6,2.6), "M_Duvar_Siva")
kutu("DepoGecit_Duvar_B", (X_DIS,Y1+0.6,0), (X_DIS+DERIN,Y1+0.75,2.6), "M_Duvar_Siva")
kutu("DepoGecit_Tavan", (X_DIS,Y0-0.75,2.6), (X_DIS+DERIN,Y1+0.75,2.7), "M_Tavan_Panel")
kutu("DepoGecit_Son", (X_DIS+DERIN,Y0-0.75,0), (X_DIS+DERIN+0.15,Y1+0.75,2.7), "M_Duvar_Siva")
print("depo kapisi kuruldu")
