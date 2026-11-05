# Hafiza marketi on kapisi: gomulu cam kapiyi iki acilir kanada boler (Unity'de oyuncu yaklasinca iceri acilir).
# Tekrar calistirilabilir. hafiza_market_aktar.py'den ONCE calistir; kanatlar ayri FBX'e yazilir.
import bpy, bmesh
S = bpy.context.scene
col = bpy.data.collections["Market_Cephe"]
Y, X_SOL, X_SAG, ORTA = -0.08, 0.545, 1.455, 1.0
M_CAM = bpy.data.materials["M_Vitrin_Cam"]; M_AL = bpy.data.materials["M_Aluminyum"]

def kutu(ad, mn, mx, m, ebeveyn):
    me = bpy.data.meshes.new(ad); bm = bmesh.new()
    bmesh.ops.create_cube(bm, size=1.0)
    for v in bm.verts:
        v.co.x = mn[0] + (v.co.x + 0.5) * (mx[0] - mn[0])
        v.co.y = mn[1] + (v.co.y + 0.5) * (mx[1] - mn[1])
        v.co.z = mn[2] + (v.co.z + 0.5) * (mx[2] - mn[2])
    bm.to_mesh(me); bm.free(); me.materials.append(m)
    o = bpy.data.objects.new(ad, me); col.objects.link(o); o.parent = ebeveyn
    return o

for n in ("MarketKapi_KanatSol", "MarketKapi_KanatSag"):
    k = bpy.data.objects.get(n)
    if k:
        for c in list(k.children): bpy.data.objects.remove(c, do_unlink=True)
        bpy.data.objects.remove(k, do_unlink=True)
# eski gomulu parcalar: cam ve orta kayit gider, kollar + etiket kanatlarla yeniden kurulur
for n in ("Kapi_Cam", "Kapi_Kayit", "Kapi_Kol_1", "Kapi_Kol_2"):
    o = bpy.data.objects.get(n)
    if o: bpy.data.objects.remove(o, do_unlink=True)
etiket = bpy.data.objects.get("Kapi_Etiket")

for ad, hx, isaret in (("MarketKapi_KanatSol", X_SOL, 1.0), ("MarketKapi_KanatSag", X_SAG, -1.0)):
    k = bpy.data.objects.new(ad, None); col.objects.link(k); k.location = (hx, Y, 0.0)
    L = ORTA - X_SOL - 0.005                       # kanat boyu
    a, b = sorted((0.0, isaret * L))
    kutu(ad + "_Cam", (a + 0.03, -0.005, 0.1), (b - 0.03, 0.005, 2.1), M_CAM, k)
    kutu(ad + "_Alt", (a, -0.03, 0.06), (b, 0.03, 0.12), M_AL, k)
    kutu(ad + "_Ust", (a, -0.03, 2.08), (b, 0.03, 2.14), M_AL, k)
    kutu(ad + "_YanA", (a, -0.03, 0.06), (a + 0.035, 0.03, 2.14), M_AL, k)
    kutu(ad + "_YanB", (b - 0.035, -0.03, 0.06), (b, 0.03, 2.14), M_AL, k)
    kolx = isaret * (L - 0.07)
    kutu(ad + "_Kol", (kolx - 0.015, -0.07, 0.8), (kolx + 0.015, -0.04, 1.3), M_AL, k)
    if isaret > 0 and etiket:                     # etiket sol kanatla doner (dunya konumu korunur)
        bpy.context.view_layer.update()
        w = etiket.matrix_world.copy(); etiket.parent = k; etiket.matrix_world = w
bpy.context.view_layer.update()
print("market kapisi iki kanada bolundu")
