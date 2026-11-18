"""Rìa Mưa and the places of chapters 2-3: alley, bridge, tarp market, filter station 19, Hạ's and bà Dung's houses,
the Vành Đệm citizens' hall, the train car and the basement classroom. Described by dialogue/Chuong_2 and Chuong_3."""
import math
import os
import random
from civi_geo import Geo, rgb, mix, shade, export_fbx, marker
import build_world as bw
from build_world import Layer, area, solid, write_layer, write_collision, COL

R = random.Random(7)
EDGE = (420, -300, -110)  # far from everything else, so all of it can stay in one scene
OX, OY, OZ = EDGE
RUST = rgb('#7d4a32')
RUST_D = rgb('#55352a')
TIN = rgb('#7f8a88')
TIN_D = rgb('#596462')
TIN_B = rgb('#4f6f78')
MUD = rgb('#3e3a35')
BRICK_SET = [rgb('#7a5a4c'), rgb('#6b6f70'), rgb('#8c7a60'), rgb('#59626a'), rgb('#7b6a5e'), rgb('#4f5c58')]
PLANK = rgb('#6f5a44')
TARP = [rgb('#2f6c7a'), rgb('#a8562e'), rgb('#7f8a3c'), rgb('#3b5a86'), rgb('#94713a')]
CANVAS = rgb('#b9ad93')
LAMP = rgb('#ffcf8a', .95)
GREEN_LED = rgb('#5ef0b0', .95)


def m(name, x, y, z=0, yaw=0):
    bw.marker(name, OX + x, OY + y, OZ + z, yaw)


def wall_box(e, x, y, sx, sy, h, color, z=0):
    e.box(x, y, z + h / 2, sx, sy, h, color)
    solid(x, y, sx, sy, h, z)


def corrugated(e, x, y, sx, sy, z, color, tilt=.0, ribs=None):
    """Tin roof: one quad per rib, every other rib shaded, so it reads as ridged sheet."""
    n = ribs or max(int(sx / .16), 4)
    w = sx / n
    for i in range(n):
        x0 = x - sx / 2 + i * w
        c = shade(mix(color, RUST, R.random() * .35), .9 if i % 2 else 1.08)
        dz = R.uniform(-.02, .02)
        e.quad((x0, y - sy / 2, z + dz), (x0 + w, y - sy / 2, z + dz), (x0 + w, y + sy / 2, z + tilt + dz), (x0, y + sy / 2, z + tilt + dz), c)


def shack(a, x, y, w, d, h, color, roof=TIN, door=None, windows=1):
    """Patched house front: base wall, mismatched sheets, roof, gutter, a lit window or two."""
    e = a.e
    wall_box(e, x, y, w, d, h, color)
    for i in range(int(w * 1.2)):
        px, pz = x + R.uniform(-w / 2, w / 2), R.uniform(.4, h - .4)
        side = 1 if R.random() < .5 else -1
        e.box(px, y + side * (d / 2 + .012), pz, R.uniform(.4, 1.2), .02, R.uniform(.3, 1.0), mix(color, R.choice((RUST, TIN, CANVAS)), R.uniform(.25, .6)))
    a.r.box(0, 0, 0, 0, 0, 0, TIN)
    corrugated(a.r, x, y, w + .7, d + .7, h + .08, roof, .35)
    for i in range(windows):
        wx = x - w / 2 + (i + 1) * w / (windows + 1)
        for side in (-1, 1):
            e.box(wx, y + side * (d / 2 + .02), h * .58, .8, .03, .7, rgb('#2c3a40'))
            e.box(wx, y + side * (d / 2 + .03), h * .58, .62, .02, .5, rgb('#ffcf8a', .6) if R.random() < .6 else rgb('#27353a'))
    e.path_tube([(x + w / 2 + .05, y - d / 2 - .04, h), (x + w / 2 + .05, y - d / 2 - .04, .1)], .05, TIN_D)


def overhead_pipes(e, y0, y1):
    """The cooling lines that make the 'rain'. They fill the sky of the district."""
    for i in range(9):
        z = R.uniform(9, 15)
        x = -24 + i * 6 + R.uniform(-1, 1)
        r = R.uniform(.18, .55)
        e.path_tube([(x, y0, z), (x + R.uniform(-3, 3), (y0 + y1) / 2, z - R.uniform(0, 1)), (x, y1, z)], r, mix(TIN_D, RUST_D, R.random()), seg=10)
    for i in range(7):
        y = y0 + (i + .5) * (y1 - y0) / 7
        e.path_tube([(-34, y, R.uniform(10, 13)), (34, y + R.uniform(-2, 2), R.uniform(10, 13))], R.uniform(.25, .7), mix(TIN_D, rgb('#37424a'), R.random()), seg=10)
        e.box(R.uniform(-18, 18), y, 11.5, .3, .3, 7, TIN_D)
    # a solid dark lid: no sky reaches this district
    e.box(0, (y0 + y1) / 2, 17, 110, y1 - y0 + 40, .5, rgb('#1d2528'))


def brick_path(e, x0, x1, y0, y1, size=.55):
    """The mismatched tiles of the dialogue: a dry way laid by hand around the deep water."""
    ny = int((y1 - y0) / size)
    for j in range(ny):
        y = y0 + j * size
        nx = int((x1 - x0) / size)
        for i in range(nx):
            x = x0 + i * size + (j % 2) * size * .5 + R.uniform(-.03, .03)
            if x + size > x1 + .1:
                continue
            c = shade(R.choice(BRICK_SET), R.uniform(.85, 1.1))
            e.box(x + size / 2, y + size / 2, .03 + R.uniform(0, .025), size - .04, size - .04, .06, c)


def puddle(a, x, y, w, d):
    a.g.box(x, y, .012, w, d, .008, rgb('#6aa0b0'))


def crate(e, x, y, z=0, s=.6, color=PLANK):
    e.bevel_box(x, y, z + s / 2, s, s, s, shade(color, R.uniform(.8, 1.1)), .02)
    for k in (-.25, .25):
        e.box(x, y - s / 2 - .006, z + s / 2 + k * s, s, .012, .05, shade(color, .7))


def stall(a, x, y, w, d, tarp, table=True):
    """Market stall: four posts, a tarp that sags, a counter."""
    e = a.e
    for sx, sy in ((-1, -1), (1, -1), (-1, 1), (1, 1)):
        e.tube((x + sx * w / 2, y + sy * d / 2, 0), (x + sx * w / 2, y + sy * d / 2, 2.6), .045, TIN_D, seg=6)
    n = 6
    for i in range(n):
        for j in range(n):
            def corner(u, v):
                sag = -.28 * math.sin(u * math.pi) * math.sin(v * math.pi)
                return (x - w / 2 + u * w, y - d / 2 + v * d, 2.62 + sag)
            u0, u1, v0, v1 = i / n, (i + 1) / n, j / n, (j + 1) / n
            a.r.quad(corner(u0, v0), corner(u1, v0), corner(u1, v1), corner(u0, v1), shade(tarp, .9 + .2 * ((i + j) % 2)))
            a.r.quad(corner(u0, v1), corner(u1, v1), corner(u1, v0), corner(u0, v0), shade(tarp, .62))
    if table:
        e.bevel_box(x, y + d * .15, .45, w * .8, d * .4, .9, PLANK, .02, top=shade(PLANK, 1.2))
        solid(x, y + d * .15, w * .8, d * .4, 1.0)


def umbrella(e, x, y, z, r=.62, colors=None, open_up=True):
    """The blue one, patched in red and yellow; the same shape hangs for repair at An's stall."""
    colors = colors or (rgb('#2f5fa8'), rgb('#2f5fa8'), rgb('#b23a30'), rgb('#2f5fa8'), rgb('#d8a93a'), rgb('#2f5fa8'), rgb('#2f5fa8'), rgb('#2f5fa8'))
    n = len(colors)
    for i in range(n):
        a0, a1 = i * 2 * math.pi / n, (i + 1) * 2 * math.pi / n
        p0 = (x + math.cos(a0) * r, y + math.sin(a0) * r, z - .18)
        p1 = (x + math.cos(a1) * r, y + math.sin(a1) * r, z - .18)
        e.quad(p0, p1, (x, y, z), (x, y, z), colors[i])
        e.quad(p1, p0, (x, y, z), (x, y, z), shade(colors[i], .6))
    e.tube((x, y, z), (x, y, z - .85), .012, TIN_D, seg=5)


def laundry(a, x0, y0, x1, y1, z):
    e = a.e
    e.path_tube([(x0, y0, z), (x1, y1, z)], .01, TIN_D, seg=4)
    n = int(math.hypot(x1 - x0, y1 - y0) / .7)
    for i in range(n):
        t = (i + .5) / n
        x, y = x0 + (x1 - x0) * t, y0 + (y1 - y0) * t
        c = R.choice(TARP + [CANVAS])
        e.quad((x - .26, y, z), (x + .26, y, z), (x + .22, y + .03, z - .75), (x - .22, y + .03, z - .75), c)
        e.quad((x + .26, y, z), (x - .26, y, z), (x - .22, y + .03, z - .75), (x + .22, y + .03, z - .75), shade(c, .7))


def build_rain_edge():
    a = area('Edge')
    e = a.e
    # ground: dark wet concrete with the district's mud worked into the joints
    e.box(0, 32, -.2, 92, 118, .4, MUD, top=shade(MUD, 1.05))
    solid(0, 32, 92, 118, .4, -.4)
    brick_path(e, -1.6, 1.8, -6, 23)
    brick_path(e, -2.2, 2.2, 31, 62)
    brick_path(e, -8, -1.6, 47, 50.5)
    brick_path(e, -14, -2.2, 64, 67)
    for x, y, w, d in ((4.2, 4, 3.4, 5), (4.6, 15, 3, 4), (-3.6, 8, 1.8, 2.6), (-.2, 42, 1.2, 1.8), (3.6, 56, 2.4, 3), (-5, 58, 1.8, 1.2)):
        puddle(a, x, y, w, d)
    # gate of the district: the checkpoint Mai came through, with the mobile audit booth
    e.box(0, -9, 3, 12, .5, 6, rgb('#3c4648'))
    solid(0, -9, 12, .5, 6)
    e.box(-7, -7.6, 1.5, 3, 2.4, 3, rgb('#4d7a78'), top=rgb('#5b8886'))
    solid(-7, -7.6, 3, 2.4, 3)
    e.box(-7, -6.38, 1.7, 1.6, .03, .9, rgb('#7fe7d2', .85))
    # alley walls
    for x, y, w, d, h, col in ((-8, 2, 9, 8, 4.2, rgb('#6e7a78')), (8.6, 0, 8, 9, 3.6, rgb('#7a6f5e')), (-8.4, 12, 8, 7, 4.6, rgb('#7c6a5a')), (9, 12, 8, 7, 3.9, rgb('#5f6f76')),
                                (-9, 34, 9, 6, 4.1, rgb('#6a706c')), (9.6, 33, 8.4, 6, 4.4, rgb('#7d6f60')), (-10, 41, 10, 5, 3.6, rgb('#75695c')), (11, 41, 9, 6, 3.8, rgb('#63706e'))):
        shack(a, x, y, w, d, h, col, roof=R.choice((TIN, TIN_B, RUST)), windows=2)
    # An's umbrella stall, built from a freight crate
    sx, sy = 3.6, 10
    stall(a, sx, sy, 3, 2.2, TARP[1])
    e.bevel_box(sx + 1.0, sy + .7, .55, .9, .9, 1.1, rgb('#6a7a54'), .02)
    e.box(sx, sy - 1.13, 2.15, 1.5, .03, .42, CANVAS)  # the crooked sign
    for i in range(5):
        umbrella(e, sx - 1.1 + i * .55, sy + .95, 2.3 - (i % 2) * .15, .34, tuple(R.choice(TARP) for _ in range(8)))
    m('M_E_an_stall', sx - .2, sy + .25, 0, 180)
    m('M_E_stall', sx - 1.6, sy - 2.2, 0, 40)
    m('M_E_lao', sx - 1.9, sy - .2, 0, 90)
    m('M_E_start', 0, -5.5, 0, 0)
    m('M_E_audit', -6, -5.2, 0, 180)
    # water-level alley: the white cross marks on the wall
    e.box(-4.4, 18, 1.5, .3, 5.6, 3, rgb('#6e7468'))
    solid(-4.4, 18, .3, 5.6, 3)
    for i, z in enumerate((.45, .75, 1.05, 1.35)):
        e.box(-4.24, 17.2 + i * .3, z, .012, .18, .03, rgb('#e9e4d8'))
        e.box(-4.24, 17.2 + i * .3, z, .012, .03, .18, rgb('#e9e4d8'))
    e.path_tube([(-4.2, 15.2, 2.6), (-4.2, 15.2, .2), (-3.6, 15.2, .1)], .05, RUST_D)
    m('M_E_mark', -2.6, 17.8, 0, 270)
    m('M_E_an_mark', -3.4, 18.8, 0, 120)
    # canal and the plastic plank bridge
    e.box(-24, 27, -.75, 44, 4, 1.1, rgb('#2a3338'))
    e.box(25, 27, -.75, 42, 4, 1.1, rgb('#2a3338'))
    a.g.box(0, 27, -.5, 92, 4, .02, rgb('#5b8a96'))
    e.box(-24, 24.9, -.1, 44, .3, .4, rgb('#5b5f5c'))
    e.box(24, 24.9, -.1, 44, .3, .4, rgb('#5b5f5c'))
    e.box(-24, 29.1, -.1, 44, .3, .4, rgb('#5b5f5c'))
    e.box(24, 29.1, -.1, 44, .3, .4, rgb('#5b5f5c'))
    for x0, x1 in ((-46, -2.2), (2.2, 46)):
        solid((x0 + x1) / 2, 27, x1 - x0, 4.6, 1.6)
    for i in range(10):
        e.box(R.uniform(-.1, .1), 24.8 + i * .5, .03, 3.6, .46, .05, shade(rgb('#8aa0a8'), R.uniform(.8, 1.1)))
    for s in (-1, 1):
        e.path_tube([(s * 1.85, 24.6, 1.0), (s * 1.85, 29.4, 1.0)], .03, TIN)
        for i in range(5):
            e.tube((s * 1.85, 24.8 + i * 1.1, 0), (s * 1.85, 24.8 + i * 1.1, 1.0), .025, TIN_D, seg=5)
    m('M_E_bridge', .3, 23.2, 0, 0)
    m('M_E_an_bridge', -.7, 24.2, 0, 150)
    # market: tarps tied edge to edge across the street
    for i, (x, y, w, d) in enumerate(((-3.6, 33, 3.4, 3), (3.8, 34, 3.2, 3), (-3.8, 37.4, 3.2, 3.2), (4, 38, 3.2, 3.4), (-3.8, 42, 3.4, 3), (4, 42.6, 3.4, 3))):
        stall(a, x, y, w, d, TARP[i % len(TARP)])
    for x, y, w, d in ((0, 33.4, 4.2, 2.6), (0, 38, 4.2, 2.8), (0, 42.4, 4.2, 2.6)):
        stall(a, x, y, w, d, TARP[int(y) % len(TARP)], table=False)
    for i in range(9):
        crate(e, R.choice((-5.4, 5.6)), 31.4 + i * 1.4, 0, R.uniform(.45, .7))
    # porridge stall: pot on a battery burner, stools
    px, py = 4, 38
    e.lathe(px, py + .5, ((0, .9), (.3, .9), (.34, 1.25), (.3, 1.3), (0, 1.3)), rgb('#6d6f70'))
    e.box(px, py + .5, 1.34, .6, .6, .02, rgb('#ffe2b0', .5))
    for dx in (-.9, .9):
        e.lathe(px + dx, py - 1.2, ((0, 0), (.14, 0), (.12, .42), (.18, .46), (0, .48)), PLANK)
    m('M_E_chao', px + .2, py + 1.3, 0, 180)
    m('M_E_market', px - 1.3, py - 2.2, 0, 30)
    m('M_E_lao_chao', px + 1.0, py - 1.0, 0, 300)
    m('M_E_tin', -2.2, 35.4, 0, 90)
    m('M_E_an_market', -.8, 36.2, 0, 300)
    # Tín's bicycle with no front wheel, by the tailor's
    e.torus(-3.1, 34.8, .36, .34, .02, rgb('#2b2b2d'), axis='x')
    e.path_tube([(-3.1, 34.8, .36), (-3.1, 35.7, .85), (-3.1, 36.1, .45)], .022, rgb('#7b2f2a'))
    # filter station 19
    wall_box(e, 14, 56, 14, 10, 6.2, rgb('#5d6a6c'))
    corrugated(a.r, 14, 56, 15, 11, 6.3, TIN_D, .4)
    for i, (x, y) in enumerate(((7.2, 52.4), (7.2, 56), (7.2, 59.6))):
        e.lathe(x, y, ((0, 0), (1.05, 0), (1.05, 3.1), (.7, 3.6), (0, 3.7)), mix(TIN, RUST, i * .2), seg=18)
        solid(x, y, 2.1, 2.1, 3.6)
    e.path_tube([(7.2, 52.4, 3.4), (5.4, 52.4, 3.8), (5.4, 60, 3.8), (7.2, 59.6, 3.4)], .13, RUST_D)
    # the filter fan An asks about
    e.torus(6.92, 49.4, 1.4, .72, .06, TIN_D, axis='x')
    e.box(6.95, 49.4, 1.4, .02, 1.3, .12, TIN)
    e.box(6.95, 49.4, 1.4, .02, .12, 1.3, TIN)
    e.box(5.4, 49.7, .9, 1.2, .6, 1.8, rgb('#39484c'))
    e.box(4.79, 49.7, 1.3, .02, .4, .24, GREEN_LED)
    e.box(3.4, 53, .55, 1.8, .7, .05, PLANK, top=shade(PLANK, 1.1))
    e.box(3.4, 53, .28, 1.6, .6, .56, shade(PLANK, .7))
    solid(3.4, 53, 1.8, .7, 1)
    m('M_E_quandoc', 3.5, 54.1, 0, 180)
    m('M_E_station', 3.1, 51.2, 0, 10)
    m('M_E_an_station', 1.9, 51.8, 0, 60)
    # Hạ's house: front with the blue umbrella and the child's shoes on the sill, a real room inside
    hx, hy, W, D, H = -9, 54, 6.4, 5.6, 3.1
    c = rgb('#786c5e')
    wall_box(e, hx - W / 2, hy, .22, D, H, c)
    wall_box(e, hx + W / 2 - 1.4, hy - D / 2, 2.8, .22, H, c)
    wall_box(e, hx - W / 2 + 1.1, hy - D / 2, 2.2, .22, H, c)
    e.box(hx + .3, hy - D / 2, 2.6, 1.0, .22, 1.0, c)
    wall_box(e, hx + W / 2, hy, .22, D, H, c)
    wall_box(e, hx, hy + D / 2, W, .22, H, c)
    corrugated(a.r, hx, hy, W + .7, D + .7, H + .1, TIN_B, .3)
    e.box(hx, hy, .02, W, D, .04, rgb('#54453a'), top=rgb('#6a5748'))
    e.box(hx + 2.1, hy - D / 2 - .13, 1.5, .8, .04, .7, rgb('#2c3a40'))
    e.box(hx + 2.1, hy - D / 2 - .15, 1.5, .64, .02, .54, rgb('#ffcf8a', .6))
    e.box(hx + 2.1, hy - D / 2 - .19, 1.1, .9, .16, .04, rgb('#5a5650'))
    e.bevel_box(hx + 1.95, hy - D / 2 - .19, 1.17, .12, .07, .06, rgb('#d8d2c2'), .01)
    e.bevel_box(hx + 2.15, hy - D / 2 - .19, 1.17, .12, .07, .06, rgb('#d8d2c2'), .01)
    e.lathe(hx + 2.45, hy - D / 2 - .2, ((0, 1.12), (.09, 1.12), (.12, 1.26), (0, 1.26)), TIN)
    umbrella(e, hx + 1.0, hy - D / 2 - .5, 2.6)
    m('M_E_house', hx + .6, hy - D / 2 - 2.0, 0, 0)
    m('M_E_an_house', hx + 1.5, hy - D / 2 - 1.1, 0, 200)
    # inside: table, stove, bed, tin box shelf
    e.bevel_box(hx - .5, hy + .4, .4, 1.5, .9, .05, PLANK, .01, top=shade(PLANK, 1.2))
    for sx, sy in ((-1, -1), (1, -1), (-1, 1), (1, 1)):
        e.box(hx - .5 + sx * .65, hy + .4 + sy * .36, .2, .05, .05, .4, shade(PLANK, .7))
    solid(hx - .5, hy + .4, 1.5, .9, .8)
    e.box(hx + 2.3, hy + 2.1, .45, 1.2, .7, .9, rgb('#4a4f4f'))
    e.lathe(hx + 2.1, hy + 2.1, ((0, .9), (.16, .9), (.2, 1.06), (0, 1.1)), TIN_D)
    solid(hx + 2.3, hy + 2.1, 1.2, .7, 1)
    e.bevel_box(hx - 2.3, hy + 1.8, .3, 1.5, 1.8, .4, rgb('#5c4a3c'), .02, top=rgb('#7a6a5c'))
    solid(hx - 2.3, hy + 1.8, 1.5, 1.8, .7)
    e.box(hx - .5, hy + .4, .86, .28, .2, .12, rgb('#7c7f78'))  # the tin box
    m('M_E_kitchen', hx + .6, hy - .9, 0, 330)
    m('M_E_kitchen_cam', hx + 1.9, hy - 1.9, 1.5, 320)
    m('M_E_ha', hx - .7, hy + 1.5, 0, 180)
    m('M_E_ha_door', hx + .4, hy - 2.1, 0, 0)
    m('M_E_an_kitchen', hx - 1.6, hy + .1, 0, 90)
    m('M_E_step', hx + 1.2, hy - D / 2 - 2.8, 0, 20)
    m('M_E_an_step', hx + 1.8, hy - D / 2 - 1.5, 0, 200)
    bw.marker('M_LIGHT_edge_home', OX + hx, OY + hy, OZ + 2.5)
    # bà Dung's dark room at the end of the alley
    dx, dy, W2, D2, H2 = -8, 71, 5.4, 4.6, 2.7
    c = rgb('#655f58')
    wall_box(e, dx - W2 / 2, dy, .22, D2, H2, c)
    wall_box(e, dx + W2 / 2, dy, .22, D2, H2, c)
    wall_box(e, dx, dy + D2 / 2, W2, .22, H2, c)
    wall_box(e, dx - W2 / 2 + 1.5, dy - D2 / 2, 3, .22, H2, c)
    wall_box(e, dx + W2 / 2 - .5, dy - D2 / 2, 1, .22, H2, c)
    corrugated(a.r, dx, dy, W2 + .7, D2 + .7, H2 + .1, TIN_D, .3)
    e.box(dx, dy, .02, W2, D2, .04, rgb('#3d3a36'), top=rgb('#4a4640'))
    e.bevel_box(dx - 1.6, dy + 1.2, .3, 1.7, 1.6, .4, rgb('#4f4238'), .02, top=rgb('#6c5e52'))
    solid(dx - 1.6, dy + 1.2, 1.7, 1.6, .7)
    for i in range(7):
        e.lathe(dx - 2.3 + i * .22, dy + .25, ((0, 0), (.05, 0), (.05, .18), (.02, .24), (0, .24)), rgb('#a9c0c6'), seg=6)
    e.path_tube([(dx - 2.4, dy + 2.1, 2.3), (dx + 2.4, dy + 2.1, 2.3)], .06, RUST_D)
    a.g.box(dx - .6, dy + .9, .05, 1.4, 1, .005, rgb('#6aa0b0'))
    m('M_E_dung', dx + .4, dy - 1.2, 0, 330)
    m('M_E_ba_dung', dx - 1.3, dy + .4, 0, 150)
    m('M_E_an_dung', dx - .1, dy + 1.3, 0, 200)
    m('M_E_tin_dung', dx + 1.5, dy - .4, 0, 250)
    bw.marker('M_LIGHT_edge_dung', OX + dx, OY + dy, OZ + 2.2)
    # far side of the alley and yard: closes the walkable area
    wall_box(e, 0, 78, 44, .5, 6, rgb('#4c5553'))
    wall_box(e, -23, 40, .5, 76, 6, rgb('#4c5553'))
    wall_box(e, 23, 40, .5, 76, 6, rgb('#4c5553'))
    wall_box(e, -13, -1, 14, .4, 6, rgb('#4c5553'))
    wall_box(e, 13, -1, 14, .4, 6, rgb('#4c5553'))
    e.box(14, 42, .01, 12, 8, .02, rgb('#544c3f'))  # yard where the children play football
    for x, y in ((9, 39), (9, 45), (19, 39), (19, 45)):
        e.tube((x, y, 0), (x, y, 1.2), .03, TIN, seg=5)
    laundry(a, -5, 30.4, 5, 30.4, 3.1)
    laundry(a, -6, 46.4, 6, 46.6, 3.3)
    laundry(a, -6, 62, 6, 62.4, 3.2)
    for x, y in ((-2.6, 20), (2.6, 21), (-2.6, 46), (2.8, 47.4), (-2.4, 63), (.4, 74)):
        e.tube((x, y, 0), (x, y, 3.4), .05, TIN_D, seg=6)
        e.box(x, y, 3.42, .34, .2, .1, LAMP)
        bw.marker('M_LIGHT_edge_%d' % int(y), OX + x, OY + y, OZ + 3.2)
    overhead_pipes(e, -14, 84)
    a.r.box(0, 0, 0, 0, 0, 0, TIN)


def build_hall():
    """Sảnh tiếp dân, Vành Đệm: ticket machine, plastic seats, three counters and the special window."""
    a = area('Hall')
    e = a.e
    x0, y0 = 0, -80
    W, D, H = 18, 12, 4.2
    wall = rgb('#c9cdc6')
    e.box(x0, y0, -.1, W, D, .2, rgb('#9aa09c'), top=rgb('#a9afaa'))
    solid(x0, y0, W, D, .2, -.2)
    for x, y, sx, sy in ((x0 - W / 2, y0, .3, D), (x0 + W / 2, y0, .3, D), (x0, y0 - D / 2, W, .3), (x0, y0 + D / 2, W, .3)):
        wall_box(e, x, y, sx, sy, H, wall)
    e.box(x0, y0, H, W, D, .1, rgb('#d9dcd6'))
    for i in range(3):
        e.box(x0 - 5 + i * 5, y0, H - .08, 3, .3, .06, rgb('#f6f3e4', .8))
        bw.marker('M_LIGHT_hall_%d' % i, OX + x0 - 5 + i * 5, OY + y0, OZ + H - .5)
    for i in range(int(W / 1.5)):
        e.box(x0 - W / 2 + .75 + i * 1.5, y0 - .0, .01, 1.46, D, .004, rgb('#a3a9a4') if i % 2 else rgb('#9fa5a0'))
    # counters along the north wall
    for i, label in enumerate((rgb('#3b6f8a'), rgb('#3b6f8a'), rgb('#8a5a3b'))):
        cx = x0 - 5 + i * 5
        e.bevel_box(cx, y0 + 4.7, .55, 3.6, .8, 1.1, rgb('#8d9692'), .02, top=rgb('#b4bab5'))
        solid(cx, y0 + 4.7, 3.6, .8, 1.2)
        e.box(cx, y0 + 5.7, 2.6, 2.4, .05, .6, label)
        e.box(cx - 1, y0 + 4.5, 1.2, .5, .04, .34, rgb('#26322f'))
        e.box(cx - 1, y0 + 4.47, 1.2, .44, .01, .28, rgb('#7fe7d2', .7))
        a.g.box(cx, y0 + 4.36, 1.6, 3.6, .02, .9, rgb('#a4c4cc'))
    # ticket machine
    e.bevel_box(x0 - 7.6, y0 - 3.8, .8, .7, .55, 1.6, rgb('#46565a'), .02)
    e.box(x0 - 7.6, y0 - 3.5, 1.2, .46, .02, .34, rgb('#7fe7d2', .8))
    solid(x0 - 7.6, y0 - 3.8, .7, .55, 1.6)
    # waiting seats
    for r in range(3):
        for c in range(6):
            sx, sy = x0 - 4 + c * .62, y0 - 2.6 + r * 1.7
            e.bevel_box(sx, sy, .44, .54, .5, .05, rgb('#3e7a8a'), .01)
            e.bevel_box(sx, sy + .24, .68, .54, .05, .46, rgb('#3e7a8a'), .01)
        e.box(x0 - 2.45, y0 - 2.6 + r * 1.7, .2, 3.8, .1, .4, rgb('#6f7874'))
        solid(x0 - 2.45, y0 - 2.6 + r * 1.7, 3.8, .55, .8)
    m('M_H_start', x0 - 6.5, y0 - 2.4, 0, 20)
    m('M_H_guide', x0 - 5, y0 + 3.2, 0, 330)
    m('M_H_staff', x0 - 5, y0 + 5.0, 0, 180)
    m('M_H_staff2', x0, y0 + 5.0, 0, 180)
    m('M_H_officer', x0 + 5, y0 + 5.0, 0, 180)
    m('M_H_window', x0 + 5, y0 + 3.3, 0, 0)
    m('M_H_clerk', x0, y0 + 3.3, 0, 0)
    m('M_H_man', x0 - 3.2, y0 + 2.4, 0, 10)
    m('M_H_old', x0 - 1.8, y0 + 1.8, 0, 350)
    m('M_H_ha', x0 + .4, y0 + 3.1, 0, 0)
    m('M_H_an', x0 - .6, y0 + 3.0, 0, 20)
    m('M_H_cam', x0 + 6.5, y0 - 4.6, 1.8, 310)


def build_train():
    a = area('Train')
    e = a.e
    x0, y0 = 60, -80
    L, W, H = 16, 3, 2.5
    body = rgb('#d7dcd8')
    e.box(x0, y0, -.08, L, W, .16, rgb('#5a6468'))
    solid(x0, y0, L, W, .16, -.16)
    wall_box(e, x0, y0 - W / 2, L, .12, .8, body)
    wall_box(e, x0, y0 + W / 2, L, .12, .8, body)
    e.box(x0, y0 - W / 2, H - .4, L, .12, .8, body)
    e.box(x0, y0 + W / 2, H - .4, L, .12, .8, body)
    solid(x0, y0 - W / 2, L, .12, H)
    solid(x0, y0 + W / 2, L, .12, H)
    wall_box(e, x0 - L / 2, y0, .12, W, H, body)
    wall_box(e, x0 + L / 2, y0, .12, W, H, body)
    e.box(x0, y0, H, L, W, .08, rgb('#e6e8e4'))
    e.box(x0, y0, H - .06, L - 1, .3, .03, rgb('#f6f3e4', .85))
    bw.marker('M_LIGHT_train', OX + x0, OY + y0, OZ + H - .3)
    for i in range(int(L / 2)):
        x = x0 - L / 2 + 1 + i * 2
        for s in (-1, 1):
            a.g.box(x, y0 + s * W / 2, 1.3, 1.7, .02, .9, rgb('#a4c4cc'))
        if i % 2 == 0:
            e.tube((x, y0, 0), (x, y0, H), .022, rgb('#c8a24e'), seg=6)
    for i in range(6):
        x = x0 - 6 + i * 2.4
        for s in (-1, 1):
            e.bevel_box(x, y0 + s * (W / 2 - .3), .3, 1.5, .5, .6, rgb('#3e6f7c'), .02)
            solid(x, y0 + s * (W / 2 - .3), 1.5, .5, .6)
    m('M_T_start', x0 - 5.5, y0, 0, 90)
    m('M_T_man', x0 - 2.6, y0 + .3, 0, 270)
    m('M_T_cam', x0 - 6.8, y0 - .6, 1.5, 90)


def build_class():
    """Basement of the old church: chairs made of freight crates, a chalk wall, one high window."""
    a = area('Class')
    e = a.e
    x0, y0 = 60, -40
    W, D, H = 10, 8, 3.1
    wall = rgb('#8f8878')
    e.box(x0, y0, -.08, W, D, .16, rgb('#5f5a50'), top=rgb('#6d675c'))
    solid(x0, y0, W, D, .16, -.16)
    for x, y, sx, sy in ((x0 - W / 2, y0, .4, D), (x0 + W / 2, y0, .4, D), (x0, y0 - D / 2, W, .4), (x0, y0 + D / 2, W, .4)):
        wall_box(e, x, y, sx, sy, H, wall)
    e.box(x0, y0, H, W, D, .12, rgb('#6f6a5e'))
    for x in (-3, 0, 3):
        e.box(x0 + x, y0, H - .1, .2, D, .2, rgb('#5a4638'))
    e.box(x0, y0 + D / 2 - .23, 1.6, 4, .04, 1.4, rgb('#2c3a36'))
    e.box(x0 - 1.2, y0 + D / 2 - .26, 1.8, 1.4, .01, .02, rgb('#dad6c8'))
    e.box(x0 + .8, y0 + D / 2 - .26, 1.5, 1.0, .01, .02, rgb('#dad6c8'))
    e.box(x0 + 4.75, y0 - 1.2, 2.3, .06, 1.4, .7, rgb('#cfe0e4', .55))
    for c in range(3):
        for r in range(3):
            crate(e, x0 - 2.4 + c * 2.0, y0 - 2 + r * 1.5, 0, .48)
            e.bevel_box(x0 - 2.4 + c * 2.0, y0 - 1.5 + r * 1.5, .62, .8, .36, .05, PLANK, .01)
    for i in range(3):
        bw.marker('M_LIGHT_class_%d' % i, OX + x0 - 2.5 + i * 2.5, OY + y0, OZ + 2.6)
        e.box(x0 - 2.5 + i * 2.5, y0, 2.92, .5, .14, .05, rgb('#ffe2b0', .9))
    m('M_C_cam', x0 + 3.8, y0 - 3.2, 1.6, 325)
    m('M_C_nhi', x0, y0 + 2.9, 0, 180)
    m('M_C_an', x0 + 2.0, y0 - .8, 0, 0)
    m('M_C_tin', x0 - .4, y0 - .4, 0, 0)
    m('M_C_hs', x0 - 2.4, y0 + .5, 0, 0)


def build(out):
    build_rain_edge()
    build_hall()
    build_train()
    build_class()
    for name in ('Edge', 'Hall', 'Train', 'Class'):
        layer = bw.AREAS.pop(name)
        layer.e.merge(Geo(), 0, 0, 0)
        for g in (layer.e, layer.g, layer.s, layer.r):
            shifted = Geo()
            shifted.merge(g, OX, OY, OZ)
            g.v, g.c, g.uv, g.w, g.f, g.mat = shifted.v, shifted.c, shifted.uv, shifted.w, shifted.f, shifted.mat
        write_layer(name, layer)
    # collision boxes were written in local space
    for i, (x, y, z, sx, sy, sz) in enumerate(COL):
        COL[i] = (x + OX, y + OZ, z + OY, sx, sy, sz)
    write_collision(out, 'Edge')
    export_fbx(os.path.join(out, 'Edge.fbx'))
