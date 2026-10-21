import bpy, bmesh, math
from mathutils import Vector
exec(open(r"C:\Kisisel Dosyalarim\ozel\Oyun Gelistirme\Denemelik projelerim\Dink\Kaynak\BlenderBetikleri\yardim.py", encoding='utf-8').read())

# kat2.py aynı koridoru AD/ACIK3 vererek kurar (Daire 3 kapalı)
AD = globals().get('AD', 'Kat1Koridor'); ACIK3 = globals().get('ACIK3', 82)
y = Yapici(AD)
X0, X1, Y0, Y1, H = 0.2, 2.6, 0.5, 8.99, 3.0
y.box((0, 0.3, -0.1), (2.8, 9.2, 0), 'L_Mozaik')
y.box((0, 0.3, H), (2.8, 9.2, H + 0.1), 'L_Tavan')
y.box((0, 0.3, 0), (2.8, Y0, 1.2), 'L_YagliBoya'); y.box((0, 0.3, 1.2), (2.8, Y0, H), 'L_Siva')

def duvar(x0, x1, ya, yb, za=0, zb=H):
    if za < 1.2: y.box((x0, ya, za), (x1, yb, min(1.2, zb)), 'L_YagliBoya')
    if zb > 1.2: y.box((x0, ya, max(1.2, za)), (x1, yb, zb), 'L_Siva')

def kusak(x, ya, yb, s):
    a, b = (x, x + 0.02) if s > 0 else (x - 0.02, x)
    y.box((a, ya, 1.18), (b, yb, 1.24), 'L_Supurgelik', 0.004, uv=0.5)
    y.box((a, ya, 0), (b, yb, 0.12), 'L_Supurgelik', 0.003, uv=0.5)

duvar(0, X0, Y0, Y1); kusak(X0, Y0, Y1, 1)
D3a, D3b = 4.05, 4.95
duvar(X1, 2.8, Y0, D3a); duvar(X1, 2.8, D3b, Y1); duvar(X1, 2.8, D3a, D3b, 2.05, H)
kusak(X1, Y0, D3a - 0.07, -1); kusak(X1, D3b + 0.07, Y1, -1)
y.box((X0, Y0, 0), (X1, Y0 + 0.015, 0.12), 'L_Supurgelik', 0.003, uv=0.5)
y.box((X0, Y0, 1.18), (X1, Y0 + 0.02, 1.24), 'L_Supurgelik', 0.004, uv=0.5)

def kapi(cy, yon, acik=0.0):
    """yon=+1: batı duvarı (kapı +x'e bakar); -1: doğu duvarı. Menteşe cy-w/2 tarafında."""
    wx = X0 if yon > 0 else X1; s = yon; w, h = 0.9, 2.05
    for (ya, yb, za, zb) in [(cy - w/2 - 0.07, cy - w/2, 0, h + 0.07), (cy + w/2, cy + w/2 + 0.07, 0, h + 0.07), (cy - w/2 - 0.07, cy + w/2 + 0.07, h, h + 0.07)]:
        y.box((min(wx, wx + s*0.025), ya, za), (max(wx, wx + s*0.025), yb, zb), 'L_MasaAhsap', 0.006, 3, uv=0.5)
    t = 0.05; xa = wx - s*t if acik else wx + s*0.004
    leaf = []
    leaf += y.box((min(xa, xa + s*t), cy - w/2 + 0.005, 0.01), (max(xa, xa + s*t), cy + w/2 - 0.005, h - 0.005), 'L_Ahsap', 0.008, 3, uv=0.5)
    fx = xa + s*t
    for (za, zb) in [(0.15, 0.95), (1.1, 1.9)]:
        leaf += y.box((min(fx, fx + s*0.012), cy - w/2 + 0.12, za), (max(fx, fx + s*0.012), cy + w/2 - 0.12, zb), 'L_Ahsap', 0.01, 3, uv=0.5)
    hy = cy + w/2 - 0.09
    leaf += y.box((min(fx, fx + s*0.008), hy - 0.025, 0.9), (max(fx, fx + s*0.008), hy + 0.025, 1.15), 'T_BronzAlu', 0.004, uv=0.5)
    leaf += y.cyl((fx + s*0.05, hy - 0.05, 1.06), 0.01, 0.12, 'T_BronzAlu', 'Y')
    leaf += y.cyl((fx + s*0.012, hy, 1.0), 0.007, 0.02, 'L_Karanlik', 'X')
    leaf += y.cyl((fx + s*0.005, cy, 1.55), 0.012, 0.012, 'T_BronzAlu', 'X')
    leaf += y.box((min(fx, fx + s*0.006), cy - 0.06, 1.72), (max(fx, fx + s*0.006), cy + 0.06, 1.82), 'L_PostaKrom', 0.003, uv=0.5)
    if acik: y.rotate(leaf, s*math.radians(acik), (wx, cy - w/2, 0))
    y.box((min(wx, wx + s*0.55), cy - 0.35, 0), (max(wx, wx + s*0.55), cy + 0.35, 0.012), 'L_Cilt', 0.004, uv=0.5)
    return (fx + s*0.0075, cy, 1.77)

plak = {'1': kapi(2.6, 1), '2': kapi(6.4, 1), '4': kapi(6.9, -1)}
if ACIK3: kapi(4.5, -1, acik=ACIK3)
else: plak['3'] = kapi(4.5, -1)
ob = y.done()

texts = []
for no, (x, cy, z) in plak.items():
    rz = math.pi/2 if x < 1.4 else -math.pi/2
    texts.append(yazi('No' + no, no, (x, cy, z), (math.radians(90), 0, rz), 0.06, 'L_Karanlik'))
if ACIK3: texts.append(yazi('No3', '3', (X1 - 0.03, 4.5, 2.2), (math.radians(90), 0, -math.pi/2), 0.07, 'T_BronzAlu'))
birlestir(ob, texts)

# Asansör duvarı giriş katının arka duvarından kopyalanır (aynı kapı, kasa, buton, gösterge)
g = bpy.data.objects['GirisKati']; gm = g.data; gns = [m.name for m in gm.materials]
src = bmesh.new(); src.from_mesh(gm)
pick = [f for f in src.faces if f.calc_center_median().y > 8.7 and f.calc_center_median().z < 3.3
        and (f.calc_center_median().x > 0.0 or f.calc_area() > 1)]
dst = bmesh.new(); dst.from_mesh(ob.data); uvs = src.loops.layers.uv.active; uvd = dst.loops.layers.uv.active
names = [m.name for m in ob.data.materials]
vm = {}
for f in pick:
    n = gns[f.material_index]
    if n not in names: ob.data.materials.append(bpy.data.materials[n]); names.append(n)
    vs = []
    for v in f.verts:
        if v not in vm: vm[v] = dst.verts.new(v.co)
        vs.append(vm[v])
    try:
        nf = dst.faces.new(vs); nf.material_index = names.index(n); nf.smooth = f.smooth
        for a, b in zip(f.loops, nf.loops): b[uvd].uv = a[uvs].uv
    except ValueError: pass
dst.to_mesh(ob.data); dst.free(); src.free()
print('koridor', len(ob.data.polygons), 'duvar', len(pick))
