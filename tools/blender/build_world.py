"""CIVI world: Tầng 42 (office, gallery, canteen, meeting room, sky terrace, service corridor), Mai's flat, the ward
flashback and the dome city. Layout follows the 2D paintings: cream marble, teal panels, brass trim, lotus motifs.
Blender axes: +X east, +Y north (windows, towards the city), +Z up. Run through run.sh: build_world.py <marker> <out_dir>
"""
import math
import os
import random
import sys
import bpy
from mathutils import Vector

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from civi_geo import Geo, rgb, mix, shade, reset, export_fbx, marker

R = random.Random(42)
CEIL = 4.4
MARBLE = rgb('#f1e4cf')
WALL = rgb('#efe2cc')
TEAL = rgb('#2f7471')
TEAL_D = rgb('#1e5052')
TEAL_L = rgb('#6fb7ad')
BRASS = rgb('#cfa75c')
STEEL = rgb('#b4bfbf')
WOOD = rgb('#8a5a3a')
WOOD_D = rgb('#5e3b28')
CREAM = rgb('#f6efe2')
LEAF = rgb('#4f8a3f')
LEAF2 = rgb('#7bb552')
PORCELAIN = rgb('#eef2f3')
BLUE = rgb('#3b64a8')
LAMP = rgb('#ffd98f', .9)
PINK = rgb('#e8558f')


class Layer:
    """One stampable group: lit geometry, glass and animated screens."""

    def __init__(s):
        s.e, s.g, s.s, s.r = Geo(), Geo(), Geo(), Geo()

    def add(s, o, x=0, y=0, z=0, rot=0, scale=1.0):
        s.e.merge(o.e, x, y, z, rot, scale)
        s.g.merge(o.g, x, y, z, rot, scale)
        s.s.merge(o.s, x, y, z, rot, scale)
        s.r.merge(o.r, x, y, z, rot, scale)


AREAS = {}
COL = []


def area(name):
    return AREAS.setdefault(name, Layer())


def solid(x, y, sx, sy, h=1.4, z=0.0):
    """Invisible collision box. Written to a text file and rebuilt as BoxColliders, far cheaper than a mesh collider."""
    COL.append((x, z + h / 2, y, sx, h, sy))


def write_collision(out, name):
    with open(os.path.join(out, name + '.collision.txt'), 'w') as f:
        f.write(chr(10).join(' '.join(f'{v:.3f}' for v in box) for box in COL))
    COL.clear()


def place(a, prop, x, y, rot=0, z=0, col=None, h=1.4, scale=1.0):
    a.add(prop, x, y, z, rot, scale)
    if col:
        sx, sy = (col[1], col[0]) if rot % 180 == 90 else col
        solid(x, y, sx, sy, h, z)


# ---------------------------------------------------------------- props

def pot_plant(kind=0):
    p = Layer()
    e = p.e
    e.lathe(0, 0, [(.13, 0), (.2, .08), (.25, .24), (.25, .34), (.21, .42), (.23, .46)], PORCELAIN, 14)
    for a in range(0, 360, 60):
        x, y = math.cos(math.radians(a)) * .251, math.sin(math.radians(a)) * .251
        e.box(x, y, .29, .05, .05, .12, BLUE)
    e.lathe(0, 0, [(.21, .44), (.0, .46)], rgb('#3a2a20'), 10)
    n = 11 if kind == 0 else 16
    for i in range(n):
        a = i * 2.4
        tall = R.uniform(.6, 1.15) if kind == 0 else R.uniform(.5, .85)
        lean = R.uniform(.12, .42)
        base = Vector((0, 0, .46))
        tip = Vector((math.cos(a) * lean, math.sin(a) * lean, .46 + tall))
        e.tube(base, tip, .012, shade(LEAF, .7), 5, .006)
        leaf(e, tip, a, R.uniform(.26, .4) if kind == 0 else .22, mix(LEAF, LEAF2, R.random()))
    return p


def leaf(e, base, a, size, color):
    """Two folded blades; reads as a broad tropical leaf from any camera."""
    d = Vector((math.cos(a), math.sin(a), -.25)).normalized()
    side = Vector((-math.sin(a), math.cos(a), 0))
    tip = base + d * size
    mid = base + d * size * .45 + Vector((0, 0, .03))
    for s in (1, -1):
        wing = mid + side * s * size * .34 + Vector((0, 0, -.04))
        ids = [e.vert(q, c) for q, c in ((base, shade(color, .85)), (wing, color), (tip, shade(color, 1.1)))]
        e.face(ids if s > 0 else ids[::-1])
        e.face((ids[2], ids[1], ids[0]) if s > 0 else ids)


def vine(e, x, y, z, length, n=10):
    for i in range(n):
        t = i / n
        q = Vector((x + math.sin(t * 7 + x) * .08, y + math.cos(t * 5) * .05, z - t * length))
        leaf(e, q, R.uniform(0, 6.3), .17, mix(LEAF, LEAF2, R.random()))
    e.tube((x, y, z), (x, y, z - length), .006, shade(LEAF, .7), 4)


def chair():
    p = Layer()
    e = p.e
    e.bevel_box(0, 0, .5, .5, .48, .12, CREAM, .03)
    e.bevel_box(0, .24, .92, .46, .1, .7, CREAM, .03)
    e.box(0, .3, .92, .32, .02, .5, TEAL)
    e.tube((0, 0, .44), (0, 0, .14), .03, STEEL, 8)
    for a in range(0, 360, 72):
        dx, dy = math.cos(math.radians(a)) * .3, math.sin(math.radians(a)) * .3
        e.tube((0, 0, .14), (dx, dy, .05), .016, STEEL, 5)
        e.ball(dx, dy, .04, .03, .03, .03, rgb('#2c2c30'), 6, 4)
    for s in (1, -1):
        e.box(s * .29, -.02, .7, .05, .34, .03, TEAL_D)
        e.tube((s * .29, .06, .7), (s * .26, .06, .52), .014, STEEL, 5)
    return p


def monitor(w=.62, h=.36, tint=0):
    p = Layer()
    p.e.bevel_box(0, 0, .03, .22, .16, .03, rgb('#30383a'), .008)
    p.e.tube((0, .02, .04), (0, .02, .22), .018, rgb('#30383a'), 6)
    p.e.box(0, .03, .22 + h / 2, w + .05, .03, h + .05, rgb('#20282a'))
    p.s.quad((-w / 2, .012, .22), (w / 2, .012, .22), (w / 2, .012, .22 + h), (-w / 2, .012, .22 + h), (1, 1, 1, 1))
    return p


def desk(length=2.6, screens=2, lamp=True):
    p = Layer()
    e = p.e
    d = .86
    e.bevel_box(0, 0, .76, length, d, .06, CREAM, .018, rgb('#f7f0e2'))
    e.box(0, .01, .4, length - .1, d - .14, .66, TEAL)
    e.box(0, -d / 2 + .05, .4, length - .1, .02, .66, TEAL_D)
    for sx in (-1, 1):
        cx = sx * (length / 2 - .38)
        e.box(cx, -d / 2 + .02, .4, .62, .05, .66, rgb('#cdbfa4'))
        for z in (.2, .42, .62):
            e.box(cx, -d / 2 - .012, z, .52, .012, .15, shade(rgb('#cdbfa4'), 1.08))
            e.box(cx, -d / 2 - .02, z, .14, .012, .018, BRASS)
    e.box(0, -d / 2 + .01, .73, length, .02, .012, BRASS)
    for i in range(screens):
        x = (i - (screens - 1) / 2) * .72
        p.add(monitor(), x, .16, .79)
    e.box(0, -.12, .8, .5, .17, .012, rgb('#2d3438'))
    e.ball(.42, -.1, .805, .04, .06, .018, rgb('#3a4044'), 8, 4)
    if lamp:
        e.lathe(-length / 2 + .24, .1, [(.1, 0), (.08, .02), (.015, .04), (.012, .34), (.05, .36), (.09, .4), (.0, .42)], BRASS, 10, .79)
    # papers, a tablet and a cup keep the top lived-in
    e.box(length / 2 - .4, .05, .8, .3, .22, .01, rgb('#fbf8f0'))
    e.box(length / 2 - .42, .06, .812, .28, .2, .01, rgb('#efe7d6'))
    e.lathe(-.72, -.2, [(.035, 0), (.042, .08), (.042, .1), (.0, .1)], PORCELAIN, 10, .79)
    return p


def shelf_files(length=3.0, height=2.3):
    p = Layer()
    e = p.e
    e.box(0, 0, height / 2, length, .34, height, shade(WALL, .96))
    e.box(0, -.11, height / 2, length - .1, .14, height - .1, shade(WALL, .7))
    rows = 4
    for r in range(rows + 1):
        e.box(0, -.16, .08 + r * (height - .16) / rows, length, .3, .04, CREAM)
    for r in range(rows):
        x = -length / 2 + .12
        while x < length / 2 - .2:
            w = R.uniform(.05, .1)
            h = R.uniform(.26, .4)
            col = R.choice([rgb('#f4f2ec'), TEAL_L, BLUE, rgb('#c8d6d3'), TEAL_D])
            e.box(x, -.2, .1 + r * (height - .16) / rows + h / 2, w, .2, h, col)
            x += w + .012
    return p


def lantern(drop=.9, light=True):
    """Hanging brass lantern from the paintings; glow comes from vertex alpha, not a real light."""
    p = Layer()
    e = p.e
    e.tube((0, 0, 0), (0, 0, -drop), .01, BRASS, 5)
    z = -drop
    e.lathe(0, 0, [(.04, -.0), (.17, -.08), (.19, -.1), (.0, -.1)], BRASS, 8, z)
    e.lathe(0, 0, [(.13, -.1), (.16, -.28), (.13, -.46)], LAMP, 8, z)
    for a in range(0, 360, 90):
        e.box(math.cos(math.radians(a)) * .16, math.sin(math.radians(a)) * .16, z - .28, .025, .025, .38, BRASS)
    e.lathe(0, 0, [(.0, -.5), (.16, -.48), (.14, -.46)], BRASS, 8, z)
    return p


def stool():
    p = Layer()
    e = p.e
    e.lathe(0, 0, [(.2, 0), (.21, .02), (.05, .05), (.03, .1), (.03, .6), (.15, .63), (.19, .66), (.19, .72), (.0, .74)], STEEL, 10)
    e.lathe(0, 0, [(.2, .66), (.2, .74), (.0, .76)], TEAL, 12)
    return p


def round_table(r=.9):
    p = Layer()
    e = p.e
    e.lathe(0, 0, [(.42, 0), (.46, .04), (.4, .08), (.34, .12), (.3, .8), (.36, .86), (r, .88), (r + .03, .92), (r, .95), (0, .96)], TEAL, 22, top=CREAM)
    e.torus(0, 0, .885, r + .012, .012, BRASS, 28, 5)
    for a in range(0, 360, 18):
        e.box(math.cos(math.radians(a)) * .32, math.sin(math.radians(a)) * .32, .45, .03, .03, .68, BRASS)
    return p


def case_gallery():
    """Glass display case holding the old gas cards (step 3)."""
    p = Layer()
    e = p.e
    e.bevel_box(0, 0, .45, 2.3, 1.15, .9, TEAL, .03, CREAM)
    e.box(0, -.58, .25, 2.2, .02, .03, BRASS)
    e.box(0, -.58, .75, 2.2, .02, .03, BRASS)
    for i, c in enumerate((rgb('#9c7a58'), rgb('#e5dccb'), rgb('#cdb79c'))):
        x = (i - 1) * .7
        e.box(x, 0, .98, .55, .05, .2, shade(c, .8))
        e.quad((x - .24, -.03, .9), (x + .24, -.03, .9), (x + .22, .02, 1.14), (x - .22, .02, 1.14), c)
        for r in range(3):
            for k in range(5):
                e.box(x - .16 + k * .08, -.012, .95 + r * .055, .03, .01, .03, shade(c, .6))
    g = p.g
    for q in (((-1.12, -.56, .9), (1.12, -.56, .9), (1.12, -.56, 1.7), (-1.12, -.56, 1.7)), ((1.12, .56, .9), (-1.12, .56, .9), (-1.12, .56, 1.7), (1.12, .56, 1.7)),
              ((-1.12, .56, .9), (-1.12, -.56, .9), (-1.12, -.56, 1.7), (-1.12, .56, 1.7)), ((1.12, -.56, .9), (1.12, .56, .9), (1.12, .56, 1.7), (1.12, -.56, 1.7)),
              ((-1.12, -.56, 1.7), (1.12, -.56, 1.7), (1.12, .56, 1.7), (-1.12, .56, 1.7))):
        g.quad(*q, (1, 1, 1, 1))
    for x, y in ((-1.12, -.56), (1.12, -.56), (1.12, .56), (-1.12, .56)):
        e.box(x, y, 1.3, .04, .04, .84, BRASS)
    e.bevel_box(0, 0, 1.73, 2.34, 1.19, .06, TEAL_D, .015)
    e.box(0, 0, 1.69, 1.6, .05, .015, LAMP)
    return p


def holo_table(length=5.0, width=1.9):
    p = Layer()
    e = p.e
    e.lathe(0, 0, [(.6, 0), (.5, .1), (.42, .7), (.5, .74)], TEAL_D, 14)
    rows = []
    for i in range(32):
        a = i / 32 * math.tau
        rows.append(a)
    rings = []
    for rad, z in ((.86, .74), (1.0, .78), (.98, .82)):
        rings.append(dict(c=Vector((0, 0, z)), ex=Vector((length / 2 * rad, 0, 0)), ey=Vector((0, width / 2 * rad, 0)), color=CREAM))
    e.loft(rings, 36)
    # the "energy map": rings and a pulse sit on the animated screen material
    s = p.s
    base = len(s.v)
    n = 36
    centre = s.vert((0, 0, .83), (1, 1, 1, 1), (.5, .5))
    for i in range(n):
        a = i / n * math.tau
        s.vert((math.cos(a) * length / 2 * .92, math.sin(a) * width / 2 * .92, .83), (1, 1, 1, 1), (.5 + math.cos(a) * .5, .5 + math.sin(a) * .5))
    for i in range(n):
        s.face((centre, base + 1 + i, base + 1 + (i + 1) % n))
    return p


def coffee_counter(length=6.0):
    p = Layer()
    e = p.e
    e.bevel_box(0, 0, .55, length, .7, 1.1, TEAL, .02, CREAM)
    e.box(0, -.36, .98, length - .1, .02, .04, BRASS)
    for i in range(int(length / .9)):
        x = -length / 2 + .45 + i * .9
        e.box(x, -.355, .5, .74, .02, .78, TEAL_D)
    for x in (-1.9, -.9):
        e.lathe(x, .05, [(.14, 0), (.16, .34), (.14, .4), (.06, .44), (.03, .54), (.0, .55)], STEEL, 10, 1.1)
    for i in range(6):
        e.lathe(.4 + i * .22, .08, [(.035, 0), (.045, .09), (.045, .11), (.0, .11)], PORCELAIN, 8, 1.1)
    return p


def bench():
    p = Layer()
    e = p.e
    e.bevel_box(0, 0, .46, 2.0, .5, .06, WOOD, .02)
    e.bevel_box(0, .24, .78, 2.0, .05, .4, WOOD, .012)
    for x in (-.85, .85):
        e.box(x, 0, .22, .08, .44, .44, BRASS)
    return p


def bloom_bush(size=1.0):
    """Bougainvillea from the title painting: dark leaf mass with pink flower clusters."""
    p = Layer()
    e = p.e
    for i in range(6):
        a = i * 1.1
        e.ball(math.cos(a) * .4 * size, math.sin(a) * .3 * size, (.5 + .18 * (i % 3)) * size, .42 * size, .36 * size, .34 * size, mix(LEAF, rgb('#2f6b3a'), .5), 8, 5)
    for i in range(26):
        a, z = R.uniform(0, 6.3), R.uniform(.4, 1.0) * size
        rad = R.uniform(.5, .82) * size
        e.ball(math.cos(a) * rad, math.sin(a) * rad * .8, z, .075, .075, .06, mix(PINK, rgb('#ff9ec2'), R.random()), 5, 3)
    return p


def tree(h=3.4):
    p = Layer()
    e = p.e
    e.path_tube([(0, 0, 0), (.05, .03, 1.2), (.2, .1, 2.2), (.3, .1, h - .5)], .13, rgb('#6b4a35'), 8)
    e.tube((.1, .05, 1.8), (-.7, .3, 2.8), .07, rgb('#6b4a35'), 6, .04)
    for x, y, z, s in ((.3, .1, h, 1.1), (-.7, .3, h - .3, .8), (.9, -.3, h - .4, .75), (-.1, -.7, h - .2, .7), (.2, .8, h - .1, .8)):
        e.ball(x, y, z, s, s * .9, s * .68, mix(rgb('#3d7a3e'), LEAF2, R.random() * .4), 9, 6, None, shade(LEAF2, 1.1))
    return p


def valve():
    p = Layer()
    e = p.e
    e.tube((0, .12, 0), (0, -.08, 0), .05, STEEL, 8)
    e.torus(0, -.1, 0, .17, .022, rgb('#d05a3c'), 16, 6, 'y')
    for a in (0, 90):
        ca, sa = math.cos(math.radians(a)) * .17, math.sin(math.radians(a)) * .17
        e.tube((-ca, -.1, -sa), (ca, -.1, sa), .014, rgb('#d05a3c'), 5)
    return p


def lotus_panel(e, x, y, z, rot=0, size=.42, color=BRASS):
    """Flat brass lotus used on columns and the gallery wall."""
    p = Geo()
    for i in range(-2, 3):
        a = math.radians(i * 26)
        w = .15 * size
        tip = (math.sin(a) * size, 0, math.cos(a) * size)
        l = (math.sin(a - .35) * size * .5 - w * .2, 0, math.cos(a - .35) * size * .5)
        r = (math.sin(a + .35) * size * .5 + w * .2, 0, math.cos(a + .35) * size * .5)
        p.quad((0, 0, 0), r, tip, l, color)
    e.merge(p, x, y, z, rot)


# ---------------------------------------------------------------- shells

def tiles(e, x0, x1, y0, y1, size, base, north=True, inlay=BRASS):
    nx, ny = max(int((x1 - x0) / size), 1), max(int((y1 - y0) / size), 1)
    dx, dy = (x1 - x0) / nx, (y1 - y0) / ny
    for i in range(nx):
        for j in range(ny):
            a, b = x0 + i * dx, y0 + j * dy
            tone = R.uniform(.965, 1.02) * (1.0 + (.05 * (j / ny) if north else 0))
            e.quad((a + .006, b + .006, 0), (a + dx - .006, b + .006, 0), (a + dx - .006, b + dy - .006, 0), (a + .006, b + dy - .006, 0), shade(base, tone))
    e.box((x0 + x1) / 2, (y0 + y1) / 2, -.03, x1 - x0, y1 - y0, .05, shade(base, .82))


def rug(e, x, y, sx, sy, color=TEAL, trim=BRASS):
    e.bevel_box(x, y, .008, sx, sy, .016, trim, .004)
    e.bevel_box(x, y, .014, sx - .12, sy - .12, .02, color, .004)
    e.box(x, y, .012, sx - .36, sy - .36, .024, trim)
    e.box(x, y, .014, sx - .42, sy - .42, .026, shade(color, 1.1))


def wall_solid(a, x0, y0, x1, y1, color=WALL, base=TEAL, h=CEIL, t=.3):
    """Axis-aligned wall with teal wainscot and a brass line, plus its collision box."""
    e = a.e
    sx, sy = max(abs(x1 - x0), t), max(abs(y1 - y0), t)
    cx, cy = (x0 + x1) / 2, (y0 + y1) / 2
    e.box(cx, cy, 1.0 + (h - 1.0) / 2, sx, sy, h - 1.0, color)
    e.box(cx, cy, .5, sx + .06 * (sx < sy), sy + .06 * (sy < sx), 1.0, base)
    e.box(cx, cy, 1.03, sx + .08 * (sx < sy), sy + .08 * (sy < sx), .05, BRASS)
    e.box(cx, cy, .06, sx + .09 * (sx < sy), sy + .09 * (sy < sx), .12, shade(base, .7))
    solid(cx, cy, sx, sy, h)


def wall_door(a, x, y0, y1, gap_y, gap=3.0, top=3.1, h=CEIL, t=.5):
    """North-south wall with a framed opening; the opening itself stays clear for a sliding door."""
    wall_solid(a, x, y0, x, gap_y - gap / 2, t=t)
    wall_solid(a, x, gap_y + gap / 2, x, y1, t=t)
    e = a.e
    e.box(x, gap_y, (top + h) / 2, t, gap, h - top, WALL)
    solid(x, gap_y, t, gap, h - top, top)
    for s in (1, -1):
        e.box(x, gap_y + s * (gap / 2 + .05), top / 2, t + .14, .14, top, TEAL_D)
    e.box(x, gap_y, top + .08, t + .14, gap + .24, .16, BRASS)
    lotus_panel(e, x - t / 2 - .08, gap_y, top + .24, 90, .3)
    lotus_panel(e, x + t / 2 + .08, gap_y, top + .24, -90, .3)


def door(name, x, y, along='y', gap=3.0, h=3.05):
    """Two sliding panels the game moves apart; kept out of the area mesh so they can animate."""
    for tag, s in (('L', 1), ('R', -1)):
        g = Geo()
        g.bevel_box(0, 0, h / 2, .1, gap / 2 - .02, h, TEAL, .01, CREAM)
        g.box(.06, 0, h * .6, .012, gap / 2 - .24, h * .62, shade(TEAL_L, .9))
        g.box(-.06, 0, h * .6, .012, gap / 2 - .24, h * .62, shade(TEAL_L, .9))
        g.box(.06, -s * (gap / 4 - .1), 1.1, .03, .05, .5, BRASS)
        g.box(-.06, -s * (gap / 4 - .1), 1.1, .03, .05, .5, BRASS)
        o = g.to_object(f'Door_{name}_{tag}', ['Env'])
        if along == 'y':
            o.location = (x, y + s * gap / 4, 0)
        else:
            o.rotation_euler = (0, 0, math.radians(90))
            o.location = (x + s * gap / 4, y, 0)
    marker(f'M_DOOR_{name}', x, y, 0, 0 if along == 'y' else 90)


def window_wall(a, x0, x1, y, bay=6.0, sill=.85, h=CEIL, door_at=None):
    """North glazing: teal columns, brass mullions, arches at the head. Sun shadows on the floor come from these bars."""
    e, g = a.e, a.g
    n = max(int(round((x1 - x0) / bay)), 1)
    dx = (x1 - x0) / n
    for i in range(n + 1):
        cx = x0 + i * dx
        e.box(cx, y, h / 2, .62, .4, h, TEAL_D)
        e.box(cx, y - .21, h / 2, .34, .02, h - 1.2, TEAL)
        e.box(cx, y - .225, 2.1, .06, .01, h - 1.4, BRASS)
        lotus_panel(e, cx, y - .24, 2.5, 180, .3)
        solid(cx, y, .62, .4, h)
    for i in range(n):
        a0 = x0 + i * dx + .31
        a1 = x0 + (i + 1) * dx - .31
        is_door = door_at is not None and a0 <= door_at <= a1
        if not is_door:
            e.box((a0 + a1) / 2, y, sill / 2, a1 - a0, .3, sill, CREAM)
            e.box((a0 + a1) / 2, y - .17, sill + .02, a1 - a0, .1, .04, BRASS)
            solid((a0 + a1) / 2, y, a1 - a0, .3, h)
        e.box((a0 + a1) / 2, y, h - .32, a1 - a0, .3, .64, CREAM)  # head beam
        m = 3
        if not is_door:  # a door bay stays open from floor to head
            for k in range(m + 1):
                bx = a0 + (a1 - a0) * k / m
                e.box(bx, y, (sill + h - .64) / 2, .05, .08, h - .64 - sill, BRASS)
        for z in (sill + .02, sill + 1.1, h - .66):
            if not is_door or z > 3:
                e.box((a0 + a1) / 2, y, z, a1 - a0, .08, .05, BRASS)
        arch(e, a0 + .02, a1 - .02, y - .06, h - 1.32, .62)
        if not is_door:
            g.quad((a0, y, sill), (a1, y, sill), (a1, y, h - .64), (a0, y, h - .64), (1, 1, 1, 1))
            g.quad((a1, y, sill), (a0, y, sill), (a0, y, h - .64), (a1, y, h - .64), (1, 1, 1, 1))


def arch(e, a0, a1, y, spring, rise, color=BRASS, r=.035):
    """Round head over a window bay, the strongest shape in the office painting."""
    pts = []
    for i in range(17):
        t = i / 16 * math.pi
        pts.append(((a0 + a1) / 2 - math.cos(t) * (a1 - a0) / 2, y, spring + math.sin(t) * rise))
    e.path_tube(pts, r, color, 6)
    # cream spandrels fill the corners above the curve
    n = 8
    for i in range(n):
        t0, t1 = i / n * math.pi / 2, (i + 1) / n * math.pi / 2
        for side in (-1, 1):
            cx = (a0 + a1) / 2
            w = (a1 - a0) / 2
            p0 = (cx + side * math.cos(t0) * w, y + .02, spring + math.sin(t0) * rise)
            p1 = (cx + side * math.cos(t1) * w, y + .02, spring + math.sin(t1) * rise)
            top = spring + rise
            q0, q1 = (p0[0], y + .02, top), (p1[0], y + .02, top)
            face = (p0, p1, q1, q0) if side > 0 else (p1, p0, q0, q1)
            e.quad(*face, CREAM)
            e.quad(*face[::-1], CREAM)


def wall_detail(a, x0, x1, y, facing=1, h=CEIL, step=4.0):
    """Pilasters and framed panels so long walls read as architecture rather than a plain box."""
    e = a.e
    n = max(int((x1 - x0) / step), 1)
    dx = (x1 - x0) / n
    for i in range(n + 1):
        cx = x0 + i * dx
        e.box(cx, y + facing * .1, h / 2, .46, .2, h, shade(WALL, .97))
        e.box(cx, y + facing * .12, 3.86, .6, .24, .14, BRASS)
        e.box(cx, y + facing * .12, 1.06, .56, .24, .05, BRASS)
        lotus_panel(e, cx, y + facing * .215, 3.1, 0 if facing > 0 else 180, .28)
    for i in range(n):
        cx = x0 + (i + .5) * dx
        e.box(cx, y + facing * .04, 2.5, dx - .9, .04, 2.2, shade(WALL, 1.03))
        for dz in (-1.1, 1.1):
            e.box(cx, y + facing * .07, 2.5 + dz, dx - .84, .03, .03, BRASS)
        for dw in (-(dx - .9) / 2, (dx - .9) / 2):
            e.box(cx + dw, y + facing * .07, 2.5, .03, .03, 2.23, BRASS)


def drum_lamp(r=1.3):
    """The big teal ring lamp hanging over the desks in the painting."""
    p = Layer()
    e = p.e
    for dx in (.7, -.7):
        e.tube((dx * r, 0, 0), (dx * r, 0, -.7), .012, BRASS, 5)
    e.lathe(0, 0, [(r, -.7), (r + .05, -.8), (r + .05, -1.02), (r, -1.1), (r * .86, -1.1), (r * .86, -1.05), (0, -1.05)], TEAL_D, 26)
    e.torus(0, 0, -.74, r + .05, .02, BRASS, 30, 5)
    e.torus(0, 0, -1.06, r + .06, .02, BRASS, 30, 5)
    e.lathe(0, 0, [(r * .84, -1.09), (0, -1.09)], rgb('#fff0c8', .95), 24)
    return p


def medallion(e, x, y, r=2.6):
    """Brass lotus inlaid in the marble."""
    for ring_r, w in ((r, .03), (r - .16, .015)):
        pts = 48
        for i in range(pts):
            a0, a1 = i / pts * math.tau, (i + 1) / pts * math.tau
            c0, s0, c1, s1 = math.cos(a0), math.sin(a0), math.cos(a1), math.sin(a1)
            e.quad((x + c0 * (ring_r - w), y + s0 * (ring_r - w), .004), (x + c1 * (ring_r - w), y + s1 * (ring_r - w), .004),
                   (x + c1 * (ring_r + w), y + s1 * (ring_r + w), .004), (x + c0 * (ring_r + w), y + s0 * (ring_r + w), .004), BRASS)
    for i in range(12):
        ang = i / 12 * math.tau
        tip = (x + math.cos(ang) * (r - .3), y + math.sin(ang) * (r - .3), .004)
        l = (x + math.cos(ang - .22) * r * .46, y + math.sin(ang - .22) * r * .46, .004)
        m = (x + math.cos(ang + .22) * r * .46, y + math.sin(ang + .22) * r * .46, .004)
        c = (x + math.cos(ang) * r * .12, y + math.sin(ang) * r * .12, .004)
        e.quad(c, m, tip, l, mix(BRASS, CREAM, .25 if i % 2 else .55))


def trellis(length=8.0, h=3.0):
    """Terrace pergola carrying bougainvillea."""
    p = Layer()
    e = p.e
    for x in (-length / 2, 0, length / 2):
        for y in (-1.3, 1.3):
            e.lathe(x, y, [(.14, 0), (.1, .1), (.08, h - .1), (.13, h)], CREAM, 8)
    for y in (-1.3, 1.3):
        e.box(0, y, h + .06, length + .5, .14, .14, WOOD)
    for i in range(int(length / .6) + 1):
        e.box(-length / 2 + i * .6, 0, h + .18, .08, 3.2, .1, WOOD)
    for i in range(50):
        x, y = R.uniform(-length / 2, length / 2), R.uniform(-1.5, 1.5)
        e.ball(x, y, h + .36, R.uniform(.22, .4), R.uniform(.2, .36), .16, mix(LEAF, rgb('#2f6b3a'), R.random()), 6, 3)
        if i % 2 == 0:
            e.ball(x + R.uniform(-.2, .2), y, h + .28, .09, .09, .07, mix(PINK, rgb('#ff9ec2'), R.random()), 5, 3)
    return p


def fountain():
    p = Layer()
    e = p.e
    e.lathe(0, 0, [(1.7, 0), (1.76, .12), (1.76, .42), (1.6, .46), (1.5, .3), (0, .3)], CREAM, 24)
    e.lathe(0, 0, [(1.5, .31), (0, .31)], rgb('#7fd4e2', .2), 24)
    e.lathe(0, 0, [(.34, .3), (.2, .5), (.16, 1.1), (.56, 1.3), (.6, 1.38), (.4, 1.42), (.1, 1.5), (.06, 1.8), (.14, 2.0), (0, 2.1)], BRASS, 14)
    for i in range(10):
        ang = i / 10 * math.tau
        e.ball(math.cos(ang) * .9, math.sin(ang) * .9, .34, .2, .14, .03, rgb('#3f8f5a'), 6, 3)
    e.ball(.9, 0, .4, .08, .08, .06, PINK, 6, 3)
    return p


def lamp_post(h=3.0):
    p = Layer()
    e = p.e
    e.lathe(0, 0, [(.16, 0), (.1, .12), (.05, .2), (.04, h - .3), (.08, h - .25)], BRASS, 8)
    e.lathe(0, 0, [(.08, h - .25), (.15, h - .1), (.13, h + .2), (.0, h + .24)], LAMP, 8)
    e.lathe(0, 0, [(.17, h + .2), (.05, h + .34), (0, h + .36)], BRASS, 8)
    return p


def planter_rail(length=4.0):
    p = Layer()
    e = p.e
    e.bevel_box(0, 0, .2, length, .5, .4, TEAL, .02, rgb('#5a4030'))
    for i in range(int(length * 7)):
        x, y = R.uniform(-length / 2 + .1, length / 2 - .1), R.uniform(-.18, .18)
        e.ball(x, y, .47, .09, .09, .1, mix(LEAF, rgb('#2f6b3a'), R.random()), 5, 3)
        if i % 3 == 0:
            e.ball(x, y, .56, .05, .05, .04, R.choice([PINK, rgb('#ffd27a'), rgb('#f6efe2')]), 5, 3)
    return p


def ceiling(a, x0, x1, y0, y1, h=CEIL):
    # Roof meshes never cast shadows: the room stays sunlit and only the window bars draw on the floor, as in the paintings.
    e = a.r
    e.box((x0 + x1) / 2, (y0 + y1) / 2, h + .06, x1 - x0, y1 - y0, .12, shade(WALL, 1.02))
    # a floor slab above keeps sun out and casts the room's shadow
    e.box((x0 + x1) / 2, (y0 + y1) / 2, h + .3, x1 - x0 + 1, y1 - y0 + 1, .3, shade(WALL, .8))


def pipes(a, x0, x1, y, h=CEIL):
    e = a.e
    e.path_tube([(x0, y, h - .34), (x1, y, h - .34)], .13, TEAL, 10)
    e.path_tube([(x0, y - .36, h - .28), (x1, y - .36, h - .28)], .08, STEEL, 8)
    x = x0 + .8
    while x < x1:
        e.torus(x, y, h - .34, .14, .022, BRASS, 12, 5, 'x')
        e.box(x, y - .18, h - .12, .05, .6, .3, BRASS)
        x += 3.4


# ---------------------------------------------------------------- floor 42

def office():
    x0, x1, y0, y1 = -26, -2, -9, 9
    a = area('Office')
    e = a.e
    tiles(e, x0, x1, y0, y1, 1.5, MARBLE)
    wall_solid(a, x0, y0, x0, y1)
    wall_solid(a, x0, y0, -15.8, y0)
    wall_solid(a, -12.2, y0, x1, y0)
    e.box(-14, y0, 3.75, 3.6, .3, 1.3, WALL)
    solid(-14, y0, 3.6, .3, 1.3, 3.1)
    window_wall(a, x0, x1, y1)
    ceiling(a, x0, x1, y0, y1)
    pipes(a, x0, x1, 8.0)
    # Mai's audit desk with its rug, as in the office painting
    rug(e, -21, 5, 6.6, 5.2)
    place(a, desk(3.0, 3), -21, 3.2, 180, col=(3.0, .9), h=1.0)
    place(a, chair(), -19.5, 4.5, 40)  # pushed back so nothing stands between Mai and the desk
    marker('M_OBJ_audit', -21, 4.9, 0, 180)
    place(a, pot_plant(), -24.4, 3.0, col=(.5, .5), h=1.2)
    # Linh's technical bench
    place(a, desk(3.4, 3), -11, 6.6, 0, col=(3.4, .9), h=1.0)
    place(a, chair(), -11, 5.7, 180)
    marker('M_NPC_linh_office', -11, 4.9, 0, 0)
    e.box(-13.3, 7.0, .95, .36, .36, .36, TEAL_D)
    e.lathe(-13.3, 7.0, [(.14, 0), (.17, .1), (.0, .12)], STEEL, 10, 1.13)
    for x in (-8.9, -9.4):
        e.path_tube([(x, 7.1, .8), (x, 7.6, 1.8), (x + .3, 7.8, 2.8), (x + .3, 7.9, CEIL - .4)], .05, STEEL, 8)
        e.torus(x, 7.1 + .22, 1.2, .07, .012, BRASS, 10, 4, 'z')
    # coordination board on the south wall
    e.box(-20, y0 + .2, 2.2, 5.2, .1, 2.6, TEAL_D)
    e.box(-20, y0 + .26, 2.2, 5.3, .02, 2.7, BRASS)
    a.s.quad((-17.5, y0 + .28, 1.0), (-22.5, y0 + .28, 1.0), (-22.5, y0 + .28, 3.4), (-17.5, y0 + .28, 3.4), (1, 1, 1, 1))
    marker('M_SEE_office_board', -20, y0 + 1.7)
    marker('M_SEE_office_window', -5.5, 7.8)
    # shelves and side desks
    place(a, shelf_files(5.0), -9, y0 + .34, 0, col=(5.0, .4), h=2.3)
    place(a, shelf_files(3.0), x0 + .34, -3, 270, col=(3.0, .4), h=2.3)
    place(a, desk(2.4, 2), -7, -3.4, 90, col=(2.4, .9), h=1.0)
    place(a, chair(), -6.1, -3.4, 90)
    place(a, desk(2.4, 2), -7, 1.6, 90, col=(2.4, .9), h=1.0)
    place(a, chair(), -6.1, 1.6, 90)
    marker('M_NPC_dong_nghiep_office', -5.0, -1.4, 0, 90)
    marker('M_NPC_truc_office', -16.4, -5.4, 0, 180)
    place(a, pot_plant(1), -3.2, 7.6, col=(.5, .5), h=1.2)
    place(a, pot_plant(), -16.6, 7.5, col=(.5, .5), h=1.2)
    place(a, pot_plant(1), -24.8, -7.8, col=(.5, .5), h=1.2)
    for x, y in ((-20, 0), (-14, 0), (-8, 0)):
        place(a, lantern(1.0), x, y, z=CEIL)
        marker('M_LIGHT_warm', x, y, CEIL - 1.8)
    for x in (-25.6, -24.6, -23.8, -3.6, -2.8):
        vine(e, x, 8.3, CEIL - .1, R.uniform(1.0, 2.2))
    wall_detail(a, x0 + .2, -15.9, y0 + .15)
    medallion(e, -14, 0, 3.0)
    for x, y in ((-21, 5), (-11, 6.4)):
        place(a, drum_lamp(1.25), x, y, z=CEIL)
        marker('M_LIGHT_warm', x, y, CEIL - 1.5)
    place(a, pot_plant(), -25, 8, col=(.5, .5), h=1.2)
    place(a, pot_plant(1), -18.4, -7.9, col=(.5, .5), h=1.2)
    place(a, planter_rail(3.4), -17.6, 8.1)
    place(a, planter_rail(3.4), -5.4, 8.1)


def gallery():
    x0, x1, y0, y1 = 0, 14, -9, 9
    a = area('Gallery')
    e = a.e
    tiles(e, x0, x1, y0, y1, 1.5, shade(MARBLE, .97))
    wall_door(a, -1, y0, y1, 0)
    wall_solid(a, x0 - 2, y0, x1 + 2, y0)
    window_wall(a, -2, x1 + 2, y1, 8)
    ceiling(a, -2, x1 + 2, y0, y1)
    pipes(a, -2, x1 + 2, 8.0)
    rug(e, 7, 3, 7.4, 4.8)
    place(a, case_gallery(), 7, 4.4, 0, col=(2.4, 1.3), h=1.8)
    marker('M_OBJ_case', 7, 3.1, 0, 0)
    marker('M_SEE_gallery_card', 11.5, -7.4)
    marker('M_SEE_gallery_book', 2.6, -7.4)
    # framed history along the south wall
    for i, x in enumerate((2.6, 7, 11.4)):
        e.box(x, y0 + .22, 2.15, 3.2, .06, 1.8, BRASS)
        e.box(x, y0 + .26, 2.15, 3.04, .04, 1.64, mix(CREAM, TEAL_L, i * .2))
        for k in range(3):
            e.box(x - .9 + k * .9, y0 + .29, 2.2, .72, .02, .9, mix(rgb('#7c9eaa'), rgb('#caa77a'), k / 2))
        e.bevel_box(x, y0 + .7, .46, 1.9, .55, .92, TEAL, .02, CREAM)
        solid(x, y0 + .7, 1.9, .55, 1.0)
    # free-standing plinths
    for x, y in ((3.2, -1.8), (10.8, -1.8)):
        e.lathe(x, y, [(.42, 0), (.46, .05), (.36, .12), (.3, .95), (.4, 1.02), (.0, 1.04)], TEAL, 12, top=CREAM)
        e.ball(x, y, 1.26, .2, .2, .22, BRASS, 10, 6)
        solid(x, y, .8, .8, 1.3)
    medallion(e, 7, -3.6, 2.4)
    e.lathe(7, -3.6, [(.8, 0), (.84, .08), (.6, .16), (.5, .9), (.62, 1.0), (0, 1.02)], TEAL, 16, top=CREAM)
    solid(7, -3.6, 1.5, 1.5, 1.4)
    lotus = Geo()
    for i in range(8):
        lotus.tube((math.cos(i * .785) * .1, math.sin(i * .785) * .1, 1.02), (math.cos(i * .785) * .3, math.sin(i * .785) * .3, 1.75), .08, BRASS, 6, .02)
    lotus.ball(0, 0, 1.2, .16, .16, .2, rgb('#fff1b8', .7), 8, 5)
    e.merge(lotus, 7, -3.6)
    for x in (4.8, 9.2):
        e.lathe(x, -2.0, [(.14, 0), (.05, .06), (.03, .86), (.05, .92)], BRASS, 8)
    e.path_tube([(4.8, -2.0, .88), (5.6, -1.78, .78), (6.4, -1.72, .76), (7.6, -1.72, .76), (8.4, -1.78, .78), (9.2, -2.0, .88)], .016, rgb('#a4493b'), 6)
    place(a, pot_plant(), 13.2, -7.6, col=(.5, .5), h=1.2)
    place(a, lantern(.8), 7, 3.6, z=CEIL)
    marker('M_NPC_huong_dan', 5.0, 2.2, 0, 90)
    marker('M_NPC_hoc_sinh', 9.6, 1.4, 0, 0)
    place(a, pot_plant(), 1.0, 7.5, col=(.5, .5), h=1.2)
    place(a, pot_plant(1), 13.0, 7.5, col=(.5, .5), h=1.2)
    for x in (3.5, 10.5):
        place(a, lantern(.8), x, 0, z=CEIL)
        marker('M_LIGHT_warm', x, 0, CEIL - 1.6)


def canteen():
    x0, x1, y0, y1 = 16, 32, -9, 9
    a = area('Canteen')
    e = a.e
    tiles(e, x0, x1, y0, y1, 1.5, MARBLE)
    wall_door(a, 15, y0, y1, 0)
    wall_solid(a, 14, y0, 34, y0)
    window_wall(a, 14, 34, y1, 6.67, door_at=24)
    ceiling(a, 14, 34, y0, y1)
    pipes(a, 14, 34, 8.0)
    place(a, coffee_counter(7.2), 24, y0 + .6, 0, col=(7.2, .8), h=1.2)
    marker('M_SEE_canteen_tea', 22.3, y0 + 1.8)
    marker('M_SEE_canteen_window', 19.0, 7.8)
    for x, y in ((20, 2.5), (28, 2.5), (20, -3), (28, -3)):
        place(a, round_table(), x, y, col=(1.5, 1.5), h=.95)
        for s in range(4):
            r = math.radians(s * 90 + 45)
            place(a, stool(), x + math.cos(r) * 1.35, y + math.sin(r) * 1.35)
        e.bevel_box(x + .1, y - .1, .97, .42, .3, .02, CREAM, .006)
    place(a, round_table(1.1), 24, -2.4, col=(1.8, 1.8), h=.95)
    e.lathe(24, -2.4, [(.08, 0), (.14, .08), (.09, .24), (.12, .28), (.0, .3)], PORCELAIN, 10, .96)
    marker('M_OBJ_linh_lunch', 24.0, -.5, 0, 180)
    marker('M_NPC_linh_lunch', 24.0, -4.0, 0, 0)
    marker('M_NPC_dong_nghiep_canteen', 30.2, 5.4, 0, 180)
    place(a, pot_plant(), 17.2, -7.5, col=(.5, .5), h=1.2)
    place(a, pot_plant(1), 30.8, -7.5, col=(.5, .5), h=1.2)
    for x in (20, 28):
        place(a, lantern(1.0), x, 0, z=CEIL)
        marker('M_LIGHT_warm', x, 0, CEIL - 1.8)
    wall_detail(a, 16.2, 21, y0 + .15, step=2.4)
    wall_detail(a, 27.4, 31.8, y0 + .15, step=2.2)
    e.box(24, y0 + .22, 2.5, 7.0, .08, 1.5, TEAL_D)
    e.box(24, y0 + .26, 2.5, 7.1, .02, 1.58, BRASS)
    a.s.quad((27.2, y0 + .28, 1.86), (20.8, y0 + .28, 1.86), (20.8, y0 + .28, 3.14), (27.2, y0 + .28, 3.14), (1, 1, 1, 1))
    for x in (17.4, 18.9, 29, 30.6):
        place(a, bench(), x, 8.2, 0, scale=.72)
        solid(x, 8.2, 1.5, .5, .8)
    place(a, drum_lamp(1.1), 24, -2.4, z=CEIL)
    marker('M_LIGHT_warm', 24, -2.4, CEIL - 1.4)
    medallion(e, 24, 2.8, 1.9)
    for x, y in ((17.0, 1.2), (31.0, 1.2)):
        place(a, pot_plant(1), x, y, col=(.5, .5), h=1.2)
    # sliding terrace doors in the window wall
    door('T', 24, 9, 'x', 3.2)


def meeting():
    x0, x1, y0, y1 = 34, 46, -9, 9
    a = area('Meeting')
    e = a.e
    tiles(e, x0, x1, y0, y1, 1.5, shade(MARBLE, .95))
    wall_door(a, 33, y0, y1, 0)
    wall_solid(a, 32, y0, x1 + .5, y0)
    wall_solid(a, x1 + .2, y0, x1 + .2, -1.8)
    wall_solid(a, x1 + .2, 1.8, x1 + .2, y1)
    window_wall(a, 32, x1 + .5, y1, 7)
    ceiling(a, 32, x1 + .5, y0, y1)
    pipes(a, 32, x1 + .5, 8.0)
    rug(e, 40, 1, 8.4, 6.4, TEAL_D)
    place(a, holo_table(), 40, 1, col=(4.6, 1.8), h=.9)
    for i in range(3):
        x = 37.8 + i * 2.2
        place(a, chair(), x, 2.9, 180)
        place(a, chair(), x, -.9, 0)
    place(a, chair(), 43.6, 1, 90)
    marker('M_OBJ_vinh', 40, -2.1, 0, 0)
    marker('M_NPC_vinh', 40.6, 3.9, 0, 180)
    marker('M_NPC_linh_meeting', 37.0, -2.8, 0, 45)
    marker('M_SEE_meeting_map', 44.2, 4.6)
    marker('M_SEE_meeting_file', 35.6, -7.6)
    e.box(40, y0 + .2, 2.2, 7.2, .1, 2.7, TEAL_D)
    a.s.quad((43.4, y0 + .27, 1.0), (36.6, y0 + .27, 1.0), (36.6, y0 + .27, 3.4), (43.4, y0 + .27, 3.4), (1, 1, 1, 1))
    place(a, desk(2.2, 0, False), 35.8, -7.8, 0, col=(2.2, .9), h=1.0)
    place(a, pot_plant(), 45.0, 7.5, col=(.5, .5), h=1.2)
    place(a, pot_plant(1), 35.0, 7.5, col=(.5, .5), h=1.2)
    for x in (37, 43):
        place(a, lantern(.9), x, 1, z=CEIL)
        marker('M_LIGHT_warm', x, 1, CEIL - 1.7)
    wall_detail(a, 34.2, 36.4, y0 + .15, step=2.2)
    wall_detail(a, 43.6, 45.8, y0 + .15, step=2.2)
    place(a, drum_lamp(1.5), 40, 1, z=CEIL)
    marker('M_LIGHT_warm', 40, 1, CEIL - 1.4)
    place(a, planter_rail(3.2), 41, 8.2)
    # lift lobby: the end of the working day
    e.box(47.5, 0, 1.5, 2.8, .1, 3.0, TEAL)
    e.box(x1 + 2.8, 0, 1.6, .14, 3.8, 3.2, TEAL_D)
    door('L', x1 + .2, 0, 'y', 3.2)
    marker('M_LIFT', x1 - .8, 0)


def terrace():
    x0, x1, y0, y1 = 14, 34, 9.2, 26
    a = area('Terrace')
    e = a.e
    tiles(e, x0, x1, y0, y1, 2.0, rgb('#d9d2c4'), False)
    # low parapet with a brass rail looking over the city
    for (px, py, sx, sy) in (((x0 + x1) / 2, y1, x1 - x0 + .4, .4), (x0, (y0 + y1) / 2, .4, y1 - y0), (x1, (y0 + y1) / 2, .4, y1 - y0)):
        e.box(px, py, .5, sx, sy, 1.0, rgb('#c8c3b4'))
        solid(px, py, sx, sy, 1.6)
    for x in range(int(x0), int(x1) + 1, 2):
        e.box(x, y1, 1.25, .05, .05, .5, BRASS)
    for y in range(int(y0), int(y1) + 1, 2):
        e.box(x0, y, 1.25, .05, .05, .5, BRASS)
        e.box(x1, y, 1.25, .05, .05, .5, BRASS)
    e.box((x0 + x1) / 2, y1, 1.5, x1 - x0, .06, .05, BRASS)
    e.box(x0, (y0 + y1) / 2, 1.5, .06, y1 - y0, .05, BRASS)
    e.box(x1, (y0 + y1) / 2, 1.5, .06, y1 - y0, .05, BRASS)
    # gardens: raised beds and walks, the place for rosemary, side quests and Bé Điện
    for x, y, sx, sy in ((18, 14, 4.4, 2.2), (30, 14, 4.4, 2.2), (18, 21, 4.4, 2.2), (30, 21, 4.4, 2.2)):
        e.bevel_box(x, y, .24, sx, sy, .48, rgb('#b9b0a0'), .03, rgb('#6a4a35'))
        solid(x, y, sx, sy, .8)
        for i in range(int(sx * 3)):
            place(a, herb_tuft(), x + R.uniform(-sx / 2 + .3, sx / 2 - .3), y + R.uniform(-sy / 2 + .25, sy / 2 - .25), R.uniform(0, 360), .48)
    place(a, tree(4.0), 24, 18, col=(.7, .7), h=3.0)
    e.lathe(24, 18, [(1.6, 0), (1.66, .3), (1.5, .34), (.0, .36)], rgb('#b9b0a0'), 18, top=rgb('#6a4a35'))
    solid(24, 18, 2.6, 2.6, .6)
    for x, y in ((15.6, 24.4), (32.4, 24.4), (15.6, 11), (32.4, 11)):
        place(a, bloom_bush(1.3), x, y, col=(1.8, 1.8), h=1.2)
    # stone paths crossing at the tree, gold inlay at the junction
    for x0p, y0p, x1p, y1p in ((23, 9.2, 25, 25.8), (14.2, 17, 33.8, 19)):
        e.box((x0p + x1p) / 2, (y0p + y1p) / 2, .006, x1p - x0p, y1p - y0p, .012, rgb('#e6dfd0'))
    medallion(e, 24, 18, 3.4)
    place(a, trellis(6.6), 17.6, 22.2)
    place(a, trellis(6.6), 30.4, 22.2)
    place(a, fountain(), 18.0, 17.6, col=(2.6, 2.6), h=1.0)
    place(a, fountain(), 30.0, 17.6, col=(2.6, 2.6), h=1.0)
    for x in (15.2, 32.8):
        for y in (13.2, 20.6):
            place(a, lamp_post(), x, y, col=(.3, .3), h=3.0)
    for x, y in ((21.6, 10), (26.4, 10)):
        place(a, planter_rail(2.8), x, y, col=(2.8, .6), h=.7)
    for x in (16.5, 19.5, 28.5, 31.5):
        place(a, planter_rail(2.4), x, 25.4)
    place(a, bench(), 21.4, 24.9, 180, col=(2.0, .6), h=.9)
    place(a, bench(), 27, 24.9, 180, col=(2.0, .6), h=.9)
    for y in (12, 18, 24):
        place(a, pot_plant(1), 14.8, y)
        place(a, pot_plant(1), 33.2, y)
    marker('M_SPAWN_title_view', 24, 23.2, 0, 180)
    for i, (x, y) in enumerate(((22.0, 12.2), (26.0, 12.2), (24.0, 23.0))):
        marker(f'M_CAT_{i}', x, y)
    marker('M_NPC_cat', 21.2, 24.85, .52)
    marker('M_TITLE_mai', 23.5, 23.5, 0, 150)
    marker('M_TITLE_an', 24.6, 23.2, 0, 180)
    marker('M_TITLE_ha', 25.8, 23.4, 0, 205)


def herb_tuft():
    p = Layer()
    for i in range(5):
        a = i * 1.3
        tip = Vector((math.cos(a) * .1, math.sin(a) * .1, R.uniform(.2, .36)))
        p.e.tube((0, 0, 0), tip, .012, mix(LEAF, rgb('#5f9a78'), R.random()), 4, .003)
    return p


def corridor():
    x0, x1, y0, y1 = -26, -2, -14.5, -9.3
    a = area('Corridor')
    e = a.e
    tiles(e, x0, x1, y0, y1, 1.2, rgb('#8d9a9c'), False)
    wall_solid(a, x0, y0, x1, y0, rgb('#a9b4b5'), rgb('#4a5a60'))
    wall_solid(a, x0, y0, x0, y1, rgb('#a9b4b5'), rgb('#4a5a60'))
    wall_solid(a, x1, y0, x1, y1, rgb('#a9b4b5'), rgb('#4a5a60'))
    a.r.box((x0 + x1) / 2, (y0 + y1) / 2, CEIL + .06, x1 - x0, y1 - y0, .12, rgb('#7d8a8c'))
    for y, z, r, col in ((y0 + .5, 1.3, .22, TEAL), (y0 + .5, 2.2, .16, STEEL), (y0 + .9, 3.2, .26, rgb('#b5583f')), (y0 + .5, 3.8, .12, BRASS)):
        e.path_tube([(x0, y, z), (x1, y, z)], r, col, 10)
    x = x0 + 2
    while x < x1:
        e.torus(x, y0 + .5, 1.3, .24, .03, BRASS, 12, 5, 'x')
        e.box(x, y0 + .28, 2.2, .12, .5, 3.2, rgb('#60706f'))
        x += 4
    for i, xv in enumerate((-22.0, -14.0, -6.0)):
        place(a, valve(), xv, y0 + .86, z=1.3)
        marker(f'M_VALVE_{i}', xv, y0 + 1.5, 0, 0)
    for x in (-20, -8):
        e.box(x, (y0 + y1) / 2, CEIL - .06, 1.6, .24, .08, rgb('#d8f6ee', .9))
        marker('M_LIGHT_cool', x, (y0 + y1) / 2, CEIL - .6)
    door('C', -14, -9.0, 'x', 3.0)


def build_floor(out):
    solid(11, 6, 80, 46, .5, -.5)  # walkable slab under the whole floor
    office()
    gallery()
    canteen()
    meeting()
    terrace()
    corridor()
    door('A', -1, 0, 'y')
    door('B', 15, 0, 'y')
    door('M', 33, 0, 'y')
    marker('M_SPAWN_start', -18.6, 1.2, 0, 200)
    marker('M_SPAWN_lift', 44.0, 0, 0, 90)
    # pickups, side-quest stations and lore spots. Names are read by the game; see CiviQuests data.
    for name, x, y, z in (('leaf_0', 19.0, -7.8, 1.2), ('leaf_1', 27.6, -7.8, 1.2), ('leaf_2', 18.0, 14.0, .6),
                          ('card_0', 3.2, -7.4, 1.0), ('card_1', 12.6, 6.2, .1), ('card_2', 10.8, -2.6, .1),
                          ('herb_0', 18.8, 21.2, .6), ('herb_1', 29.4, 14.2, .6), ('herb_2', 30.6, 21.0, .6),
                          ('sensor_0', -24.0, -6.6, 1.1), ('sensor_1', -3.4, -6.8, .1), ('sensor_2', -24.6, 7.6, .9)):
        marker('M_PICK_' + name, x, y, z)
    for i, (x, y, z) in enumerate(((-16, 6.8, 1.5), (-3.2, 3.2, 1.3), (-24.8, -4.2, 2.4), (6.6, -6.8, 2.1), (13.0, 2.6, 2.0), (22.8, 4.8, 1.3),
                                   (30.8, 7.0, 1.6), (37.6, 6.4, 1.6), (44.8, -6.8, 2.0), (16.4, 17.8, 1.4), (31.6, 17.8, 1.4), (-24.4, -12.6, 2.0))):
        marker(f'M_ORB_{i}', x, y, z)
    for name, x, y in (('stamp', -17.8, 3.0), ('terminal', -13.3, 5.8), ('storm', 44.8, -3.4)):
        marker('M_STATION_' + name, x, y)
    marker('M_NOTE_0', -5.2, 6.8, 1.0)
    marker('M_NOTE_1', 12.8, -7.6, 1.0)
    marker('M_NOTE_2', 31.0, -7.8, 1.2)
    marker('M_NOTE_3', 45.0, 5.8, 1.0)
    marker('M_CHEST_0', -25.0, -13.2, 0)
    marker('M_CHEST_1', 33.0, 25.0, 0)
    for name, a in AREAS.items():
        write_layer(name, a)
    write_collision(out, 'Floor42')
    export_fbx(os.path.join(out, 'Floor42.fbx'))


def write_layer(name, a):
    a.e.to_object(name, ['Env'])
    if a.g.f:
        a.g.to_object(name + '_Glass', ['Glass'])
    if a.s.f:
        a.s.to_object(name + '_Screen', ['Screen'])
    if a.r.f:
        a.r.to_object(name + '_Roof', ['Env'])


# ---------------------------------------------------------------- home, ward

HOME = Vector((-260, 150, -130))


def armchair():
    p = Layer()
    e = p.e
    e.bevel_box(0, 0, .3, .86, .84, .6, WOOD_D, .03)
    e.bevel_box(0, -.03, .5, .7, .72, .22, rgb('#d8c9b0'), .05)
    e.bevel_box(0, .36, .82, .76, .2, .78, rgb('#d8c9b0'), .05)
    for s in (1, -1):
        e.bevel_box(s * .42, 0, .6, .1, .84, .12, WOOD, .03)
    e.bevel_box(.02, .12, .78, .46, .14, .4, rgb('#e9d5b7'), .05, PINK)
    return p


def sofa(length=2.3):
    p = Layer()
    e = p.e
    green = rgb('#6d8467')
    e.bevel_box(0, 0, .25, length, .9, .24, WOOD_D, .03)
    e.bevel_box(0, -.03, .48, length - .3, .76, .24, green, .06)
    e.bevel_box(0, .38, .82, length - .1, .22, .7, green, .07)
    for s in (1, -1):
        e.bevel_box(s * (length / 2 - .1), 0, .62, .2, .86, .34, green, .05)
    e.bevel_box(-.5, .22, .78, .42, .14, .4, rgb('#e6d5be'), .06)
    e.bevel_box(.45, .22, .76, .4, .14, .38, rgb('#b5584f'), .06)
    return p


def plank_floor(e, x0, x1, y0, y1):
    y = y0
    while y < y1:
        x = x0 + R.uniform(-1.2, 0)
        while x < x1:
            ln = R.uniform(1.4, 2.6)
            a, b = max(x, x0), min(x + ln, x1)
            e.quad((a + .008, y + .008, 0), (b - .008, y + .008, 0), (b - .008, y + .232, 0), (a + .008, y + .232, 0), shade(rgb('#9c6a44'), R.uniform(.9, 1.1)))
            x += ln
        y += .24
    e.box((x0 + x1) / 2, (y0 + y1) / 2, -.03, x1 - x0, y1 - y0, .05, rgb('#4a2d1c'))


def build_home(out):
    reset()
    AREAS.clear()
    x, y, z = HOME
    a = area('Home')
    r = Layer()
    e = r.e
    W, D = 7.5, 5.5
    plank_floor(e, -W, W, -D, D)
    warm = rgb('#d9c6ac')
    H = 3.0

    def wall(px, py, sx, sy, zc=H / 2, sz=H, color=warm):
        e.box(px, py, zc, sx, sy, sz, color)

    wall(-W, 0, .3, 2 * D)
    wall(W, 0, .3, 2 * D)
    wall(0, -D, 2 * W, .3)
    wall(0, D, 2 * W, .3, .4, .8)
    wall(0, D, 2 * W, .3, H - .25, .5)
    for bx in (-W + .75, W - .75):
        wall(bx, D, 1.5, .3)
    for bx in (-2.8, 0, 2.8):
        e.box(bx, D, 1.45, .1, .12, 1.3, WOOD_D)
    e.box(0, D, 2.1, 12, .12, .06, WOOD_D)
    e.box(0, D, .82, 12, .36, .05, WOOD)
    r.g.quad((-6, D, .8), (6, D, .8), (6, D, H - .5), (-6, D, H - .5), (1, 1, 1, 1))
    r.r.box(0, 0, H + .06, 2 * W + .6, 2 * D + .6, .12, rgb('#e9dcc6'))
    # curtains frame the window
    for s in (1, -1):
        for i in range(5):
            e.box(s * (5.9 + i * .09), D - .24, 1.55, .1, .14 + (i % 2) * .08, 2.7, rgb('#e6dcc6'))
    rug(e, 2.6, -.2, 5.0, 3.4, rgb('#a4564a'), rgb('#d9c3a0'))
    r.add(sofa(), 5.8, 0, 0, 270)
    r.add(armchair(), -1.0, 3.2, 0, 165)
    # coffee table and the album (step 7)
    e.bevel_box(2.6, -.2, .44, 2.2, 1.1, .07, WOOD, .02)
    e.box(2.6, -.2, .16, 2.0, .9, .03, WOOD_D)
    for sx, sy in ((1, 1), (1, -1), (-1, 1), (-1, -1)):
        e.box(2.6 + sx * 1.0, -.2 + sy * .46, .22, .08, .08, .44, WOOD_D)
    e.bevel_box(2.4, -.2, .5, .72, .52, .05, rgb('#5a3b2a'), .01)
    e.box(2.22, -.2, .532, .32, .46, .012, rgb('#f2ecdc'))
    e.box(2.58, -.2, .532, .32, .46, .012, rgb('#f2ecdc'))
    e.box(2.22, -.2, .54, .22, .3, .006, rgb('#8d8880'))
    e.box(2.58, -.2, .54, .22, .3, .006, rgb('#8d8880'))
    e.lathe(3.3, -.3, [(.05, 0), (.09, .06), (.06, .15), (.0, .17)], PORCELAIN, 10, .48)
    # Mai's desk (step 8), lamp and notebook
    r.add(desk(2.4, 0, True), -5.4, 3.9, 180)
    e.box(-5.2, 3.8, .8, .3, .22, .012, rgb('#f6f1e3'))
    e.box(-5.7, 3.86, .92, .46, .03, .28, rgb('#22303a'))
    r.add(chair(), -5.4, 3.0, 0)
    # shelf with books and vines
    r.add(shelf_files(3.0, 2.2), -W + .3, -1, 270)
    for vx in (-7, -6.3):
        vine(e, vx, 4.9, H - .1, 1.4)
    # window sill rosemary: mother's corner (step 6)
    e.lathe(-.2, 4.9, [(.12, 0), (.16, .16), (.18, .26), (.0, .28)], PORCELAIN, 12, .85)
    for i in range(36):
        ang, rad = R.uniform(0, 6.3), R.uniform(.02, .14)
        tip = Vector((math.cos(ang) * rad * 2.2, 4.9 + math.sin(ang) * rad * 2.2, 1.1 + R.uniform(.2, .55)))
        e.tube((-.2 + math.cos(ang) * rad, 4.9 + math.sin(ang) * rad, 1.1), tip + Vector((-.2, 0, 0)), .007, mix(LEAF, rgb('#5f9a78'), R.random()), 4, .002)
    # lamps
    r.add(lantern(.6), 4.8, 2.6, H)
    r.add(lantern(.6), -3.4, -2.2, H)
    e.tube((6.7, 3.9, 0), (6.7, 3.9, 1.7), .02, BRASS, 5)
    e.lathe(6.7, 3.9, [(.14, 0), (.18, .3), (.14, .55)], LAMP, 8, 1.7)
    # lotus painting
    e.box(4.0, -D + .2, 1.7, 1.6, .06, 1.1, WOOD_D)
    e.box(4.0, -D + .24, 1.7, 1.46, .04, .96, rgb('#efe6d3'))
    lotus_panel(e, 4.0, -D + .28, 1.42, 0, .5, PINK)
    a.add(r, x, y, z)
    solid(x, y, 2 * W, 2 * D, .5, z - .5)
    for px, py, sx, sy, h in ((-W, 0, .3, 2 * D, H), (W, 0, .3, 2 * D, H), (0, -D, 2 * W, .3, H), (0, D, 2 * W, .3, H),
                              (5.8, 0, .9, 2.3, .9), (-1.0, 3.2, .9, .9, 1.0), (2.6, -.2, 2.2, 1.1, .6), (-5.4, 3.9, 2.4, .9, 1.0), (-W + .3, -1, .4, 3.0, 2.2)):
        solid(x + px, y + py, sx, sy, h, z)
    marker('M_SPAWN_home', x - 5.4, y - 3.6, z, 40)
    marker('M_NPC_me_mai', x - .6, y + 4.0, z, 180)
    marker('M_OBJ_rosemary', x - .4, y + 2.8, z, 0)
    marker('M_OBJ_album', x + 2.6, y - 1.5, z, 0)
    marker('M_OBJ_desk', x - 5.4, y + 2.6, z, 0)
    marker('M_SEE_home_plant', x + 1.6, y + 4.4)
    marker('M_SEE_home_window', x + 4.2, y + 4.4)
    for n, (dx, dy) in enumerate(((4.8, 2.6), (-3.4, -2.2), (6.7, 3.9))):
        marker('M_LIGHT_warm', x + dx, y + dy, z + 2.0)
    # ward flashback sits next door in world space so it streams with the flat
    build_ward()
    for name, l in AREAS.items():
        write_layer(name, l)
    write_collision(out, 'Home')
    export_fbx(os.path.join(out, 'Home.fbx'))


def build_ward():
    a = area('Ward')
    r = Layer()
    e = r.e
    W, D, H = 5.0, 4.0, 3.2
    gray = rgb('#8e9aa3')
    tiles(e, -W, W, -D, D, 1.0, rgb('#a7b0b2'), False)
    e.box(-W, 0, H / 2, .3, 2 * D, H, gray)
    e.box(W, 0, H / 2, .3, 2 * D, H, gray)
    e.box(0, -D, H / 2, 2 * W, .3, H, gray)
    e.box(0, D, H / 2, 2 * W, .3, H, gray)
    r.r.box(0, 0, H + .06, 2 * W, 2 * D, .12, shade(gray, .9))
    e.box(0, D - .17, .5, 2 * W, .04, 1.0, rgb('#6f8088'))
    # bed, the failing purifier and the console that shows the green line
    e.bevel_box(-1, .6, .4, 2.3, 1.1, .3, rgb('#5a666c'), .03)
    e.bevel_box(-1, .6, .62, 2.2, 1.0, .16, rgb('#dce0dc'), .05)
    e.bevel_box(-1.8, .6, .74, .5, .7, .12, rgb('#f1efe8'), .05)
    e.bevel_box(-.7, .6, .76, 1.5, .96, .1, rgb('#7c8c94'), .04)
    e.box(2.8, 3.4, 1.0, 1.2, .5, 2.0, rgb('#6c787d'))
    e.box(2.8, 3.13, 1.4, .9, .02, .6, rgb('#2b3438'))
    e.box(-3.4, 3.2, 1.3, .9, .6, .7, rgb('#39464c'))
    e.box(-3.4, 2.9, 1.3, .7, .02, .46, rgb('#2fe0a0', .9))
    e.box(0, 0, H - .05, 1.8, .3, .06, rgb('#cfe6e4', .9))
    a.add(r, 90, -120, -130)
    marker('M_WARD_cam', 90 + 2.4, -120 - 2.8, -130 + 1.5, 0)
    marker('M_WARD_mai', 90 + .9, -120 - .4, -130, 90)
    marker('M_WARD_me', 90 - 1.1, -120 + .6, -130 + .86, 0)
    marker('M_WARD_grey', 90 + 2.2, -120 + .9, -130, 200)
    marker('M_WARD_staff', 90 + 2.9, -120 - .6, -130, 240)
    marker('M_LIGHT_cool', 90, -120, -130 + 2.6)


# ---------------------------------------------------------------- the dome city

C = Vector((60, 500, -180))
DOME_R = 560


def building(e, x, y, h, w, d, base, roof, glow=.0, z0=0.0, tiers=3):
    """Stepped terraces with planted roofs: from the windows the city should read as gardens on white stone."""
    band = rgb('#7ab4c4', .14 + glow)
    z = z0
    for t in range(tiers):
        f = 1 - t * .24
        th = h / tiers
        tw, td = w * f, d * f
        e.box(x, y, z + th / 2, tw, td, th, base)
        for k in range(1, max(int(th / 4.4), 1) + 1):
            bz = z + k * 4.4 - 1
            if bz > z + th - 1:
                break
            e.box(x, y - td / 2 - .1, bz, tw * .86, .2, 1.7, band)
            e.box(x, y + td / 2 + .1, bz, tw * .86, .2, 1.7, band)
            e.box(x - tw / 2 - .1, y, bz, .2, td * .86, 1.7, band)
            e.box(x + tw / 2 + .1, y, bz, .2, td * .86, 1.7, band)
        e.box(x, y, z + th + .4, tw + .9, td + .9, .8, roof)
        for k in range(3):
            e.ball(x + R.uniform(-tw, tw) * .42, y + R.uniform(-td, td) * .42, z + th + 1.4, R.uniform(1.6, 3.2), R.uniform(1.6, 3.2), R.uniform(1.2, 2.2), mix(rgb('#4d9050'), rgb('#7bb552'), R.random()), 6, 3)
        z += th


def curved_tower(e, x, y, z0, h, r, base, band):
    """Round tiered tower, the white stacked forms around the core in the title painting."""
    z = z0
    rings = 4
    for i in range(rings):
        th = h / rings
        rr = r * (1 - i * .16)
        e.lathe(x, y, [(rr + 1.4, 0), (rr, 1.2), (rr, th - 1.6), (rr + 1.8, th - .8), (rr + 1.8, th), (0, th)], base, 14, z, top=rgb('#6fae78'))
        e.lathe(x, y, [(rr + .15, th * .4), (rr + .15, th * .62)], band, 14, z)
        z += th


def pagoda(e, x, y, z, s=1.0):
    e.box(x, y, z + 3 * s, 12 * s, 9 * s, 6 * s, rgb('#f2ebdc'))
    rings = []
    for t, rx, ry in ((0, 11, 8.5), (.3, 8.6, 6.6), (.7, 5, 3.8), (1, .4, .4)):
        rings.append(dict(c=Vector((x, y, z + (6 + t * 4.4) * s)), ex=Vector((rx * s, 0, 0)), ey=Vector((0, ry * s, 0)), color=rgb('#a4493b')))
    e.loft(rings, 8)


def build_city(out):
    reset()
    AREAS.clear()
    e = Geo()
    g = Geo()
    cx, cy, cz = C
    white = rgb('#f3efe4')
    cream = rgb('#e9e0cd')
    green = rgb('#4d9050')
    green2 = rgb('#7bb552')
    # ground, ring lake and the bright core
    e.lathe(cx, cy, [(DOME_R + 30, 0), (DOME_R, 1), (0, 2)], rgb('#b8cfc0'), 72, cz - 2)
    e.lathe(cx, cy, [(330, 0), (325, .4), (200, .4), (195, 0)], rgb('#79c6d8', .18), 64, cz + 1.4)
    tiers = [(190, 34), (150, 40), (112, 44), (80, 46), (54, 44), (34, 40)]
    z = cz + 2
    for i, (radius, height) in enumerate(tiers):
        e.lathe(cx, cy, [(radius + 6, 0), (radius, 6), (radius - 2, height - 6), (radius + 4, height - 3), (radius + 4, height), (0, height + .5)], white, 44, z, top=cream)
        for k in range(18 + (6 - i) * 2):
            a = k * 2.399 + i
            rx, ry = cx + math.cos(a) * (radius - 4), cy + math.sin(a) * (radius - 4)
            e.ball(rx, ry, z + height + 3, R.uniform(4, 9), R.uniform(4, 9), R.uniform(3, 6), mix(green, green2, R.random()), 7, 4)
        for k in range(10):
            a = k / 10 * math.tau + i * .4
            rx, ry = cx + math.cos(a) * (radius + 1.5), cy + math.sin(a) * (radius + 1.5)
            e.box(rx, ry, z + height * .52, 3.4, 3.4, height * .6, rgb('#8fb7c6', .25))
        if i in (1, 3):
            for k in range(5):
                a = k / 5 * math.tau + i
                pagoda(e, cx + math.cos(a) * (radius - 10), cy + math.sin(a) * (radius - 10), z + height, 1.5)
        if i % 2 == 0:  # waterfalls down the south face, towards the windows of Floor 42
            for k in range(3):
                a = -2.0 + k * .36 + i * .1
                rx, ry = cx + math.cos(a) * (radius + 5), cy + math.sin(a) * (radius + 5)
                e.box(rx, ry, z + height / 2, 7, 2, height, rgb('#dff5fb', .55))
        z += height
    # crystal lotus towers and the two halo rings from the paintings
    e.lathe(cx, cy, [(24, 0), (20, 8), (11, 44), (7, 86), (2.4, 120), (0, 132)], rgb('#eaf6f6', .1), 12, z, top=rgb('#9fe4e4', .35))
    for s, off in ((1, 0), (.62, 28), (.62, -28)):
        e.lathe(cx + off, cy + 12, [(9 * s, 0), (6 * s, 30 * s), (2.2 * s, 78 * s), (0, 92 * s)], rgb('#eaf6f6', .1), 8, z - 6, top=rgb('#9fe4e4', .35))
    halo = rgb('#5ff0dc', .9)
    e.torus(cx, cy, z + 26, 82, 2.4, halo, 48, 6)
    e.torus(cx, cy, z - 44, 150, 2.8, halo, 56, 6)
    # districts
    for i in range(330):
        a = R.uniform(0, math.tau)
        d = R.uniform(215, DOME_R - 40)
        x, y = cx + math.cos(a) * d, cy + math.sin(a) * d
        if 196 < d < 332:
            d += 136
            x, y = cx + math.cos(a) * d, cy + math.sin(a) * d
        if -170 < x < 200 and -110 < y < 170:  # clear air in front of the Floor 42 windows and terrace
            continue
        east = math.cos(a) > .55 and d > 400  # Rìa Mưa: low, dark, wet
        if east:
            h = R.uniform(8, 18)
            col = mix(rgb('#5b6168'), rgb('#3f4a50'), R.random())
            e.box(x, y, cz + h / 2, R.uniform(12, 22), R.uniform(12, 22), h, col, top=rgb('#4a4d50'))
            if i % 9 == 0:
                e.lathe(x, y, [(4, 0), (3, 40), (3.4, 44)], rgb('#3b3f44'), 8, cz)
            continue
        h = R.uniform(26, 90) * (1.3 - d / DOME_R)
        w = R.uniform(22, 40)
        if i % 3 == 0:
            curved_tower(e, x, y, cz, h * 1.2, w * .42, mix(white, cream, R.random()), rgb('#7ab4c4', .16))
            if i % 2 == 0:
                pagoda(e, x, y, cz + h * 1.2, 1.1)
        else:
            building(e, x, y, h, w, R.uniform(22, 40), mix(white, cream, R.random()), R.choice([rgb('#3b8a84'), rgb('#7bb552'), cream]), z0=cz, tiers=R.choice([2, 3, 3, 4]))
    for i in range(150):
        a = R.uniform(0, math.tau)
        d = R.uniform(340, DOME_R - 30)
        e.ball(cx + math.cos(a) * d, cy + math.sin(a) * d, cz + R.uniform(5, 9), R.uniform(8, 16), R.uniform(8, 16), R.uniform(6, 10), mix(green, green2, R.random()), 7, 4)
    # viaduct and pillars for the tram loop
    rail_z = cz + 96
    pts = [(cx + math.cos(i / 64 * math.tau) * 300, cy + math.sin(i / 64 * math.tau) * 300, rail_z) for i in range(65)]
    e.path_tube(pts, 3.2, rgb('#dfe6e4'), 6)
    for i in range(0, 64, 2):
        a = i / 64 * math.tau
        px, py = cx + math.cos(a) * 300, cy + math.sin(a) * 300
        e.lathe(px, py, [(5, 0), (3, 6), (2.6, 88), (6, 95)], rgb('#e9e4d8'), 8, cz)
    # the two towers the playable floors sit on, so nothing floats in the title flyover
    tower = rgb('#efe8da')
    for bx, by, sx, sy, top_z in ((11, -4, 80, 28, -.5), (24, 17, 22, 18, -.5), (HOME.x, HOME.y, 18, 14, HOME.z - .3)):
        h = top_z - cz
        e.box(bx, by, cz + h / 2, sx, sy, h, tower)
        for z in range(int(cz) + 6, int(top_z) - 2, 6):
            band = rgb('#7ab4c4', .14)
            e.box(bx, by - sy / 2 - .15, z, sx * .92, .3, 2.4, band)
            e.box(bx, by + sy / 2 + .15, z, sx * .92, .3, 2.4, band)
            e.box(bx - sx / 2 - .15, by, z, .3, sy * .92, 2.4, band)
            e.box(bx + sx / 2 + .15, by, z, .3, sy * .92, 2.4, band)
    e.box(11, -4, 14, 78, 26, 19, tower, top=rgb('#3b8a84'))  # upper floors over the offices, not over the terrace
    e.box(11, -4, 30, 40, 14, 14, tower, top=rgb('#3b8a84'))
    # geodesic frame and shell
    bm_mesh = dome_edges()
    for p, q in bm_mesh:
        e.tube(p, q, 1.0, rgb('#e8f6f8', .12), 4)
    shell = Geo()
    rows, seg = 18, 48
    grid = []
    for j in range(rows + 1):
        phi = j / rows * math.pi / 2
        row = []
        for i in range(seg):
            a = i / seg * math.tau
            row.append(shell.vert((cx + math.cos(a) * math.cos(phi) * DOME_R, cy + math.sin(a) * math.cos(phi) * DOME_R, cz + math.sin(phi) * DOME_R), (1, 1, 1, 1)))
        grid.append(row)
    for j in range(rows):
        for i in range(seg):
            shell.face((grid[j][i], grid[j][(i + 1) % seg], grid[j + 1][(i + 1) % seg], grid[j + 1][i]))
    e.to_object('City', ['Env'])
    shell.to_object('DomeShell', ['Glass'])
    # the tram is its own object so the game can drive it round the viaduct
    t = Geo()
    for i in range(4):
        y = (i - 1.5) * 21
        t.bevel_box(0, y, 4.6, 6.4, 20, 7.4, rgb('#f5f6f2'), .6, rgb('#2f7471'))
        t.box(0, y, 5.4, 6.6, 18, 2.2, rgb('#46d6c4', .6))
    tram = t.to_object('Tram', ['Env'])
    marker('M_CITY_centre', cx, cy, rail_z)
    export_fbx(os.path.join(out, 'City.fbx'))


def dome_edges():
    import bmesh
    bm = bmesh.new()
    bmesh.ops.create_icosphere(bm, subdivisions=4, radius=DOME_R)
    out = []
    for edge in bm.edges:
        a, b = edge.verts[0].co, edge.verts[1].co
        if a.z < -2 or b.z < -2:
            continue
        out.append((C + a, C + b))
    bm.free()
    return out


# ---------------------------------------------------------------- pickups, drone, cat

def build_props(out):
    reset()
    objs = {}

    def item(name):
        g = Geo()
        objs[name] = g
        return g

    g = item('P_leaf')
    leafs = Geo()
    for i in range(3):
        leaf(g, Vector((0, 0, .02 + i * .03)), i * 2.1, .2, mix(LEAF, LEAF2, i / 2))
    g = item('P_card')
    g.bevel_box(0, 0, .12, .24, .016, .15, rgb('#cdb79c'), .004)
    for r in range(3):
        for k in range(5):
            g.box(-.08 + k * .04, -.01, .08 + r * .04, .022, .006, .022, rgb('#7c5f42'))
    g = item('P_herb')
    for i in range(14):
        a = i * 2.4
        g.tube((0, 0, 0), (math.cos(a) * .08, math.sin(a) * .08, R.uniform(.2, .32)), .006, mix(LEAF, rgb('#5f9a78'), R.random()), 4, .002)
    g.torus(0, 0, .06, .03, .008, rgb('#c5533f'), 10, 4)
    g = item('P_sensor')
    g.lathe(0, 0, [(.09, 0), (.1, .03), (.07, .07), (.0, .08)], STEEL, 10)
    g.ball(0, 0, .09, .04, .04, .04, rgb('#3ff0c0', .95), 8, 5)
    g = item('P_orb')
    g.ball(0, 0, 0, .14, .14, .14, rgb('#a6f7e1', .95), 10, 7)
    g.torus(0, 0, 0, .22, .008, rgb('#fff1b8', .95), 18, 4)
    g.torus(0, 0, 0, .22, .008, rgb('#fff1b8', .95), 18, 4, 'y')
    g = item('P_note')
    g.bevel_box(0, 0, .02, .3, .22, .03, rgb('#f2e9d6'), .006)
    g.box(0, 0, .04, .22, .012, .004, rgb('#7c5f42'))
    g = item('P_chest')
    g.bevel_box(0, 0, .24, .8, .5, .48, TEAL, .03, CREAM)
    for s in (1, -1):
        g.box(s * .3, 0, .25, .05, .52, .5, BRASS)
    g.box(0, -.26, .3, .14, .03, .14, BRASS)
    g = item('P_chest_lid')
    g.bevel_box(0, 0, .05, .82, .52, .1, TEAL_D, .03, BRASS)
    g = item('P_marker')
    # the gold diamond shown over quest givers
    top, bottom, mid = (0, 0, .2), (0, 0, -.2), .12
    pts = [(mid, 0, 0), (0, mid, 0), (-mid, 0, 0), (0, -mid, 0)]
    col = rgb('#ffd27a', .85)
    for i in range(4):
        p0, p1 = pts[i], pts[(i + 1) % 4]
        g.quad(p0, p1, top, top, col)
        g.quad(p1, p0, bottom, bottom, shade(col, .8))
    g = item('P_data')
    # lens-only data marks shown over evidence
    g.box(0, 0, 0, .34, .34, .02, rgb('#3ff0c0', .95))
    g.box(0, 0, 0, .24, .24, .03, rgb('#0d2422', .8))
    for name, geo in objs.items():
        if geo.f:
            geo.to_object(name, ['Env'])
    export_fbx(os.path.join(out, 'Props.fbx'))

    reset()
    # CIVI: a confident little drone with a screen face; the body hovers on code, not on rotors
    g = Geo()
    g.ball(0, 0, 0, .19, .17, .15, rgb('#eef3f2'), 14, 9, None, rgb('#ffffff'))
    g.loft([dict(c=Vector((0, -.145, .015)), ex=Vector((.125, 0, 0)), ey=Vector((0, 0, .08)), color=rgb('#0d2422')),
            dict(c=Vector((0, -.16, .015)), ex=Vector((.11, 0, 0)), ey=Vector((0, 0, .066)), color=rgb('#0d2422'))], 16)
    for s in (1, -1):
        g.ball(s * .05, -.168, .025, .017, .006, .028, rgb('#3ff0c0', .98), 8, 5)
        g.torus(s * .22, 0, .01, .06, .012, TEAL, 14, 5, 'x')
        g.ball(s * .22, 0, .01, .012, .05, .05, rgb('#3ff0c0', .8), 8, 5)
    g.tube((0, 0, .14), (0, .02, .25), .006, STEEL, 5)
    g.ball(0, .02, .26, .018, .018, .018, rgb('#ff6a58', .9), 6, 4)
    g.to_object('Civi', ['Env'])
    export_fbx(os.path.join(out, 'Civi.fbx'))

    reset()
    # Bé Điện: cat from the web demo; separate tail object so the game can wag it
    g = Geo()
    fur, dark = rgb('#e8a65c'), rgb('#b5733a')
    g.ball(0, 0, .17, .085, .19, .085, fur, 10, 7)
    g.ball(0, -.2, .25, .075, .07, .07, fur, 10, 7)
    for s in (1, -1):
        ids = [g.vert(q, dark) for q in ((s * .06, -.2, .3), (s * .03, -.2, .31), (s * .055, -.19, .37))]
        g.face(ids if s < 0 else ids[::-1])
        g.face(ids[::-1] if s < 0 else ids)
        for y in (-.1, .12):
            g.tube((s * .05, y, .12), (s * .05, y, .01), .022, fur, 6)
        g.ball(s * .028, -.265, .26, .012, .005, .014, rgb('#22302a'), 6, 4)
    g.to_object('Cat', ['Env'])
    t = Geo()
    t.path_tube([(0, 0, 0), (0, .08, .06), (0, .12, .16), (.02, .1, .25)], .018, dark, 6)
    tail = t.to_object('CatTail', ['Env'])
    tail.location = (0, .18, .2)
    export_fbx(os.path.join(out, 'Cat.fbx'))


def main():
    argv = sys.argv[sys.argv.index('--') + 1:]
    marker_file, out = argv[0], argv[1]
    os.makedirs(out, exist_ok=True)
    try:
        reset()
        build_floor(out)
        build_home(out)
        build_city(out)
        build_props(out)
    except Exception:
        import traceback
        with open(marker_file + '.error', 'w') as f:
            traceback.print_exc(file=f)
    with open(marker_file, 'w') as f:
        f.write('ok')


if __name__ == '__main__':
    main()
