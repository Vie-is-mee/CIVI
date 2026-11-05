"""Mesh helpers shared by the CIVI Blender scripts. Everything is built from code so models can be regenerated."""
import math
import bpy
from mathutils import Vector, Matrix


def rgb(hex_color, glow=0.0):
    """'#rrggbb' -> linear-free sRGB tuple; alpha carries self-glow (1 = none, 0 = full)."""
    h = hex_color.lstrip('#')
    return (int(h[0:2], 16) / 255, int(h[2:4], 16) / 255, int(h[4:6], 16) / 255, 1.0 - glow)


def mix(a, b, t):
    return tuple(a[i] + (b[i] - a[i]) * t for i in range(4))


def shade(c, f):
    return (min(c[0] * f, 1), min(c[1] * f, 1), min(c[2] * f, 1), c[3])


class Geo:
    """Collects vertices with colour, UV, skin weights and a material slot, then becomes one Blender mesh."""

    def __init__(self):
        self.v, self.c, self.uv, self.w, self.f, self.mat = [], [], [], [], [], []

    def vert(self, p, color, uv=(0, 0), weight=None):
        self.v.append(tuple(p))
        self.c.append(color)
        self.uv.append(uv)
        self.w.append(weight or {})
        return len(self.v) - 1

    def face(self, idx, mat=0):
        self.f.append(tuple(idx))
        self.mat.append(mat)

    # ---- flat-shaded primitives: every face owns its vertices, so edges stay crisp ----
    def quad(self, a, b, c, d, color, mat=0, uv=None, weight=None):
        uv = uv or [(0, 0), (1, 0), (1, 1), (0, 1)]
        ids = [self.vert(p, color, uv[i], weight) for i, p in enumerate((a, b, c, d))]
        self.face(ids, mat)

    def box(self, cx, cy, cz, sx, sy, sz, color, top=None, weight=None, mat=0):
        """Centre + full sizes. `top` tints the upper face so props read from a third-person camera."""
        x0, x1, y0, y1, z0, z1 = cx - sx / 2, cx + sx / 2, cy - sy / 2, cy + sy / 2, cz - sz / 2, cz + sz / 2
        t = top or color
        q = self.quad
        q((x0, y0, z1), (x1, y0, z1), (x1, y1, z1), (x0, y1, z1), t, mat, None, weight)
        q((x0, y1, z0), (x1, y1, z0), (x1, y0, z0), (x0, y0, z0), shade(color, .75), mat, None, weight)
        q((x0, y0, z0), (x1, y0, z0), (x1, y0, z1), (x0, y0, z1), color, mat, None, weight)
        q((x1, y1, z0), (x0, y1, z0), (x0, y1, z1), (x1, y1, z1), color, mat, None, weight)
        q((x0, y1, z0), (x0, y0, z0), (x0, y0, z1), (x0, y1, z1), shade(color, .92), mat, None, weight)
        q((x1, y0, z0), (x1, y1, z0), (x1, y1, z1), (x1, y0, z1), shade(color, .92), mat, None, weight)

    def bevel_box(self, cx, cy, cz, sx, sy, sz, color, b=.02, top=None, mat=0):
        """A chamfered box: catches a highlight on the edge for very little geometry."""
        self.box(cx, cy, cz - b / 2, sx, sy, sz - b, color, top, None, mat)
        x0, x1, y0, y1 = cx - sx / 2, cx + sx / 2, cy - sy / 2, cy + sy / 2
        z = cz + sz / 2
        t = top or color
        i0, i1, j0, j1 = x0 + b, x1 - b, y0 + b, y1 - b
        self.quad((i0, j0, z), (i1, j0, z), (i1, j1, z), (i0, j1, z), t, mat)
        self.quad((x0, y0, z - b), (x1, y0, z - b), (i1, j0, z), (i0, j0, z), shade(t, 1.05), mat)
        self.quad((x1, y1, z - b), (x0, y1, z - b), (i0, j1, z), (i1, j1, z), shade(t, 1.05), mat)
        self.quad((x0, y1, z - b), (x0, y0, z - b), (i0, j0, z), (i0, j1, z), shade(t, 1.05), mat)
        self.quad((x1, y0, z - b), (x1, y1, z - b), (i1, j1, z), (i1, j0, z), shade(t, 1.05), mat)

    # ---- smooth primitives: vertices are shared, so shading follows the curve ----
    def loft(self, rings, seg=12, cap=(True, True), mat=0):
        """rings: dicts with c (centre), ex, ey (ring axes), color (value or fn(angle)), w (skin weights)."""
        base = len(self.v)
        # Rings may run either way along a limb; keep every face pointing out or the outline pass shows through.
        inward = (rings[-1]['c'] - rings[0]['c']).dot(rings[0]['ex'].cross(rings[0]['ey'])) < 0
        if inward:
            mat_start = len(self.f)
        for r in rings:
            for k in range(seg):
                a = 2 * math.pi * k / seg
                p = r['c'] + r['ex'] * math.cos(a) + r['ey'] * math.sin(a)
                col = r['color'](a) if callable(r['color']) else r['color']
                self.vert(p, col, (0, 0), r.get('w'))
        for i in range(len(rings) - 1):
            for k in range(seg):
                a, b = base + i * seg + k, base + i * seg + (k + 1) % seg
                c, d = a + seg, b + seg
                self.face((a, b, d, c), mat)
        if cap[0]:
            self._cap(rings[0], base, seg, True, mat)
        if cap[1]:
            self._cap(rings[-1], base + (len(rings) - 1) * seg, seg, False, mat)
        if inward:
            self.f[mat_start:] = [f[::-1] for f in self.f[mat_start:]]

    def _cap(self, ring, start, seg, flip, mat):
        col = ring['color'](0) if callable(ring['color']) else ring['color']
        centre = self.vert(ring['c'], col, (0, 0), ring.get('w'))
        for k in range(seg):
            a, b = start + k, start + (k + 1) % seg
            self.face((centre, b, a) if flip else (centre, a, b), mat)

    def lathe(self, cx, cy, profile, color, seg=14, z0=0, top=None):
        """Turned shape around Z: profile = [(radius, z), ...]; pots, lanterns, stools, columns."""
        rings = []
        n = len(profile)
        for i, (r, z) in enumerate(profile):
            col = mix(color, top, i / max(n - 1, 1)) if top else color
            rings.append(dict(c=Vector((cx, cy, z0 + z)), ex=Vector((r, 0, 0)), ey=Vector((0, r, 0)), color=col))
        self.loft(rings, seg)

    def tube(self, p0, p1, r, color, seg=8, r1=None):
        p0, p1 = Vector(p0), Vector(p1)
        d = (p1 - p0).normalized()
        ex = d.orthogonal().normalized()
        ey = d.cross(ex).normalized()
        self.loft([dict(c=p0, ex=ex * r, ey=ey * r, color=color),
                   dict(c=p1, ex=ex * (r1 or r), ey=ey * (r1 or r), color=color)], seg)

    def path_tube(self, pts, r, color, seg=8):
        """Tube along a polyline, used for pipes, rails and cables."""
        pts = [Vector(p) for p in pts]
        rings = []
        ref = Vector((0, 0, 1))
        for i, p in enumerate(pts):
            d = (pts[min(i + 1, len(pts) - 1)] - pts[max(i - 1, 0)]).normalized()
            if abs(d.dot(ref)) > .95:
                ref = Vector((1, 0, 0))
            ex = d.cross(ref).normalized()
            ey = d.cross(ex).normalized()
            rings.append(dict(c=p, ex=ex * r, ey=ey * r, color=color))
        self.loft(rings, seg)

    def ball(self, cx, cy, cz, rx, ry, rz, color, seg=12, rows=8, weight=None, top=None):
        rings = []
        for i in range(1, rows):
            t = math.pi * i / rows
            s, c = math.sin(t), math.cos(t)
            col = mix(top, color, i / rows) if top else color
            rings.append(dict(c=Vector((cx, cy, cz + rz * c)), ex=Vector((rx * s, 0, 0)), ey=Vector((0, ry * s, 0)), color=col, w=weight))
        self.loft(rings[::-1], seg)

    def torus(self, cx, cy, cz, R, r, color, seg=20, tube_seg=6, axis='z', glow=None):
        pts = []
        for i in range(seg + 1):
            a = 2 * math.pi * i / seg
            ca, sa = math.cos(a) * R, math.sin(a) * R
            pts.append((cx + ca, cy + sa, cz) if axis == 'z' else (cx + ca, cy, cz + sa) if axis == 'y' else (cx, cy + ca, cz + sa))
        self.path_tube(pts, r, color, tube_seg)

    def merge(self, other, x=0, y=0, z=0, rot=0, scale=1.0):
        """Stamps a prop built around the origin into this mesh, rotated about Z in degrees."""
        base = len(self.v)
        ca, sa = math.cos(math.radians(rot)), math.sin(math.radians(rot))
        for i, p in enumerate(other.v):
            px, py, pz = p[0] * scale, p[1] * scale, p[2] * scale
            self.v.append((x + px * ca - py * sa, y + px * sa + py * ca, z + pz))
            self.c.append(other.c[i])
            self.uv.append(other.uv[i])
            self.w.append({})
        self.f.extend(tuple(i + base for i in f) for f in other.f)
        self.mat.extend(other.mat)

    def to_object(self, name, materials, collection=None):
        mesh = bpy.data.meshes.new(name)
        mesh.from_pydata(self.v, [], self.f)
        mesh.update()
        mesh.polygons.foreach_set('use_smooth', [True] * len(self.f))
        mesh.polygons.foreach_set('material_index', self.mat)
        attr = mesh.color_attributes.new('Col', 'BYTE_COLOR', 'POINT')
        flat = [x for c in self.c for x in c]
        attr.data.foreach_set('color_srgb', flat)
        uv = mesh.uv_layers.new(name='UV')
        for loop in mesh.loops:
            uv.data[loop.index].uv = self.uv[loop.vertex_index]
        mesh.update()
        for m in materials:
            mesh.materials.append(material(m))
        obj = bpy.data.objects.new(name, mesh)
        (collection or bpy.context.scene.collection).objects.link(obj)
        return obj


def material(name):
    m = bpy.data.materials.get(name)
    return m or bpy.data.materials.new(name)


def reset():
    bpy.ops.wm.read_factory_settings(use_empty=True)


def marker(name, x, y, z=0, yaw=0.0):
    """Empties travel through FBX as transforms; the game finds gameplay anchors by name."""
    e = bpy.data.objects.new(name, None)
    e.location = (x, y, z)
    e.rotation_euler = (0, 0, math.radians(yaw))
    bpy.context.scene.collection.objects.link(e)
    return e


def export_fbx(path, armature=False):
    kw = dict(filepath=path, use_selection=False, apply_unit_scale=True, apply_scale_options='FBX_SCALE_ALL',
              axis_forward='-Z', axis_up='Y', bake_space_transform=True, mesh_smooth_type='FACE',
              colors_type='SRGB', use_mesh_modifiers=True, add_leaf_bones=False, bake_anim=False,
              object_types={'MESH', 'ARMATURE', 'EMPTY'})
    if armature:
        kw.update(primary_bone_axis='Y', secondary_bone_axis='X', use_armature_deform_only=False, bake_space_transform=False)
    bpy.ops.export_scene.fbx(**kw)
