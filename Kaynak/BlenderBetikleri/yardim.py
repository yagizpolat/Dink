import bpy, bmesh, math
from mathutils import Vector, Matrix

class Yapici:
    """Blender'da kutulardan model kurmak için küçük yardımcı (Dink modüler set)."""
    def __init__(s, name):
        s.name = name; s.bm = bmesh.new(); s.mats = []; s.uvs = {}
    def mi(s, n):
        if n not in s.mats: s.mats.append(n)
        return s.mats.index(n)
    def _new(s, pre): return [v for v in s.bm.verts if v not in pre]
    def _tag(s, vs, m, smooth=False, uvscale=2.0):
        for f in {f for v in vs for f in v.link_faces}:
            f.material_index = s.mi(m); f.smooth = smooth; s.uvs[f] = uvscale
    def box(s, lo, hi, m, bev=0.0, seg=2, uv=2.0):
        lo, hi = Vector(lo), Vector(hi); pre = set(s.bm.verts)
        vs = bmesh.ops.create_cube(s.bm, size=1)['verts']
        bmesh.ops.scale(s.bm, vec=hi - lo, verts=vs)
        if bev > 0: bmesh.ops.bevel(s.bm, geom=list({e for v in vs for e in v.link_edges}), offset=bev, segments=seg, affect='EDGES', clamp_overlap=True)
        nv = s._new(pre); bmesh.ops.translate(s.bm, vec=(lo + hi) / 2, verts=nv); s._tag(nv, m, uvscale=uv); return nv
    def cyl(s, c, r, h, m, axis='Z', seg=16, r2=None, uv=0.5):
        pre = set(s.bm.verts)
        bmesh.ops.create_cone(s.bm, cap_ends=True, segments=seg, radius1=r, radius2=r if r2 is None else r2, depth=h)
        nv = s._new(pre)
        if axis == 'X': bmesh.ops.rotate(s.bm, cent=Vector(), matrix=Matrix.Rotation(math.pi / 2, 3, 'Y'), verts=nv)
        if axis == 'Y': bmesh.ops.rotate(s.bm, cent=Vector(), matrix=Matrix.Rotation(math.pi / 2, 3, 'X'), verts=nv)
        bmesh.ops.translate(s.bm, vec=Vector(c), verts=nv); s._tag(nv, m, True, uv); return nv
    def rotate(s, vs, angle, pivot, axis='Z'):
        bmesh.ops.rotate(s.bm, cent=Vector(pivot), matrix=Matrix.Rotation(angle, 3, axis), verts=vs)
    def done(s):
        bm = s.bm; uv = bm.loops.layers.uv.new()
        for f in bm.faces:
            sc = s.uvs.get(f, 0.5); n = f.normal; ax = max(range(3), key=lambda i: abs(n[i]))
            for l in f.loops:
                c = l.vert.co; u, v = [(c.y, c.z), (c.x, c.z), (c.x, c.y)][ax]; l[uv].uv = (u / sc, v / sc)
        bm.normal_update(); me = bpy.data.meshes.new(s.name); bm.to_mesh(me); bm.free()
        for n in s.mats: me.materials.append(bpy.data.materials[n])
        old = bpy.data.objects.get(s.name)
        if old: bpy.data.objects.remove(old)
        ob = bpy.data.objects.new(s.name, me); bpy.context.collection.objects.link(ob); return ob

def yazi(name, body, loc, rot, size, mat, font=r"C:\Windows\Fonts\arialbd.ttf"):
    old = bpy.data.objects.get(name)
    if old: bpy.data.objects.remove(old)
    cu = bpy.data.curves.new(name, 'FONT'); cu.body = body; cu.size = size; cu.align_x = 'CENTER'; cu.align_y = 'CENTER'
    cu.font = bpy.data.fonts.load(font, check_existing=True); cu.extrude = 0.001; cu.materials.append(bpy.data.materials[mat])
    t = bpy.data.objects.new(name, cu); bpy.context.collection.objects.link(t); t.location = loc; t.rotation_euler = rot; return t

def birlestir(hedef, parcalar):
    bpy.ops.object.select_all(action='DESELECT')
    for p in parcalar: p.select_set(True)
    if parcalar:
        bpy.context.view_layer.objects.active = parcalar[0]; bpy.ops.object.convert(target='MESH')
    hedef.select_set(True); bpy.context.view_layer.objects.active = hedef; bpy.ops.object.join()
    return hedef

def disa_aktar(names, path):
    bpy.ops.object.select_all(action='DESELECT')
    for n in names: bpy.data.objects[n].select_set(True)
    bpy.context.view_layer.objects.active = bpy.data.objects[names[0]]
    bpy.ops.export_scene.fbx(filepath=path, use_selection=True, apply_scale_options='FBX_SCALE_UNITS', bake_space_transform=True, path_mode='STRIP')
