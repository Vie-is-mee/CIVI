"""Builds every CIVI character from the 28/10/2026 design sheets: same skeleton, parameters per person.
Run through run.sh:  build_characters.py <marker> <out_dir> [name ...]
"""
import math
import os
import random
import sys
import bpy
from mathutils import Vector

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from civi_geo import Geo, rgb, mix, shade, reset, export_fbx

BONES = [  # name, parent
    ('Hips', None), ('Spine', 'Hips'), ('Chest', 'Spine'), ('Neck', 'Chest'), ('Head', 'Neck'), ('Pony', 'Head'),
    ('ArmL', 'Chest'), ('ForeL', 'ArmL'), ('HandL', 'ForeL'), ('ArmR', 'Chest'), ('ForeR', 'ArmR'), ('HandR', 'ForeR'),
    ('LegL', 'Hips'), ('ShinL', 'LegL'), ('FootL', 'ShinL'), ('LegR', 'Hips'), ('ShinR', 'LegR'), ('FootR', 'ShinR'),
]
FRONT = -math.pi / 2  # ring angle that faces the character's front (-Y)


def ring(c, rx, ry, color, w=None, y=0.0):
    return dict(c=Vector(c) + Vector((0, y, 0)), ex=Vector((rx, 0, 0)), ey=Vector((0, ry, 0)), color=color, w=w)


def ring_y(c, rx, rz, color, w=None):
    """Ring standing in the XZ plane, for shapes lofted along Y (shoes)."""
    return dict(c=Vector(c), ex=Vector((rx, 0, 0)), ey=Vector((0, 0, rz)), color=color, w=w)


def blend(a, b, t):
    return {a: 1 - t, b: t} if 0 < t < 1 else ({a: 1.0} if t <= 0 else {b: 1.0})


def strip(g, rows, color, weight):
    """Open surface from rows of points (no wrap-around): jackets with a real opening, lapels, collars."""
    base = len(g.v)
    n = len(rows[0])
    for row in rows:
        for p in row:
            g.vert(p, color, (0, 0), weight(p) if callable(weight) else weight)
    for j in range(len(rows) - 1):
        for i in range(n - 1):
            a = base + j * n + i
            g.face((a, a + 1, a + n + 1, a + n))


def arc(z, rx, ry, a0, a1, n, y=0.0):
    """Points on an ellipse between two angles, bottom-to-top rows of these build a shell."""
    return [Vector((math.cos(a0 + (a1 - a0) * i / (n - 1)) * rx, y + math.sin(a0 + (a1 - a0) * i / (n - 1)) * ry, z)) for i in range(n)]


class Rig:
    """Joint positions for one person, derived from height, build and head size."""

    def __init__(s, p):
        H = s.H = p.get('H', 1.62)
        k = s.k = H / 1.62
        s.w = p.get('build', 1.0) * k
        s.head = p.get('head', 1.12)
        leg = p.get('leg', 1.0)
        s.hip = .87 * k * leg
        s.top = H
        s.chin = H - .255 * k * s.head
        s.neck = s.chin + .02 * k
        s.shoulder_z = s.chin - .05 * k
        torso = s.shoulder_z - s.hip
        s.chest = s.hip + torso * .6
        s.spine = s.hip + torso * .24
        sx = .14 * s.w
        s.sh = (sx, 0, s.shoulder_z - .012 * k)
        arm = .27 * k
        s.elbow = (sx + .03 * k, .006, s.sh[2] - arm)
        s.wrist = (sx + .05 * k, -.03 * k, s.sh[2] - arm * 1.9)
        s.hand = (sx + .058 * k, -.04 * k, s.sh[2] - arm * 2.26)
        hx = .074 * s.w
        s.thigh = (hx, 0, s.hip - .02)
        s.knee = (hx + .004, -.008, s.hip * .52)
        s.ankle = (hx + .006, 0, .085 * k)
        s.head_c = (0, -.012 * k, s.chin + .125 * k * s.head)

    def side(s, p, sign):
        return Vector((p[0] * sign, p[1], p[2]))

    def joints(s):
        c, rx, ry, rz = head_metrics(s)
        out = {'Hips': (0, 0, s.hip), 'Spine': (0, 0, s.spine), 'Chest': (0, 0, s.chest), 'Neck': (0, 0, s.neck - .02),
               'Head': (0, -.005, s.chin), 'Pony': (0, c[1] + ry, c[2] + rz * .55)}
        for n, sign in (('L', 1), ('R', -1)):
            out['Arm' + n] = tuple(s.side(s.sh, sign))
            out['Fore' + n] = tuple(s.side(s.elbow, sign))
            out['Hand' + n] = tuple(s.side(s.wrist, sign))
            out['Leg' + n] = tuple(s.side(s.thigh, sign))
            out['Shin' + n] = tuple(s.side(s.knee, sign))
            out['Foot' + n] = tuple(s.side(s.ankle, sign))
        return out


# ---------------------------------------------------------------- body parts

def build_legs(g, r, p):
    pants, skin = p['pants'], p['skin']
    wide = p.get('pants_wide', 1.0)
    short = p.get('shorts', False)
    for n, sign in (('L', 1), ('R', -1)):
        leg, shin, foot = 'Leg' + n, 'Shin' + n, 'Foot' + n
        T, K, A = r.side(r.thigh, sign), r.side(r.knee, sign), r.side(r.ankle, sign)
        rings = []
        for t, rad in ((-.06, .07), (.06, .076), (.35, .074), (.75, .064), (1.0, .06), (1.08, .06)):
            c = T.lerp(K, t)
            weight = blend(leg, shin, (t - .82) / .3)
            flare = 1 + (wide - 1) * max(t, 0) ** 2  # trouser width grows towards the knee; hips stay inside the jacket
            rings.append(ring(c, rad * r.w * flare, rad * r.w * flare * 1.08, pants, weight))
        if short:
            g.loft(rings[:-1], 12)
            g.loft([ring(K, .046 * r.w, .05 * r.w, skin, blend(leg, shin, .5)), ring(K.lerp(A, .45), .046 * r.w, .052 * r.w, skin, {shin: 1}),
                    ring(A + Vector((0, 0, .02)), .03 * r.w, .034 * r.w, skin, {shin: 1})], 10)
        else:
            for t, rad in ((.3, .062), (.65, .066), (.93, .069), (1.0, .069)):
                rings.append(ring(K.lerp(A, t), rad * r.w * wide, rad * r.w * wide * 1.04, pants, {shin: 1}))
            rings.append(ring(rings[-1]['c'], .02, .02, shade(pants, .5), {shin: 1}))
            g.loft(rings, 12)
        build_shoe(g, r, p, A, foot, shin)


def build_shoe(g, r, p, A, foot, shin):
    shoe, sole = p['shoes'], p.get('sole', rgb('#2a2422'))
    k = r.k
    x = A.x
    rings = [ring_y((x, y * k, (rz + .014) * k), rx * k, rz * k, shoe, {foot: 1})
             for y, rx, rz in ((.052, .02, .03), (.032, .05, .05), (-.04, .052, .058), (-.1, .05, .046), (-.155, .04, .034), (-.19, .018, .018))]
    g.loft(rings, 10)
    g.box(x, -.066 * k, .012 * k, .1 * k, .252 * k, .024 * k, sole, weight={foot: 1})
    g.loft([ring(A + Vector((0, 0, -.015)), .05 * k, .052 * k, shoe, {foot: 1}),
            ring(A + Vector((0, 0, .03)), .044 * k, .046 * k, shade(shoe, .85), {shin: 1})], 10)


def torso_profile(r, p):
    """(z, half width, half depth) from the hem to the base of the neck."""
    k, w = r.k, r.w
    hem = r.hip - p.get('hem', .1) * k
    if p.get('female', True):
        return [(hem, .158 * w, .112 * w), (r.hip + .01, .15 * w, .104 * w), (r.hip + .1 * k, .121 * w, .086 * w), (r.spine + .07 * k, .116 * w, .084 * w),
                (r.chest, .134 * w, .105 * w), (r.chest + .08 * k, .142 * w, .096 * w), (r.shoulder_z - .012 * k, .138 * w, .074 * w), (r.shoulder_z + .012 * k, .1 * w, .058 * w)]
    return [(hem, .152 * w, .108 * w), (r.hip + .01, .146 * w, .102 * w), (r.hip + .1 * k, .134 * w, .094 * w), (r.spine + .07 * k, .136 * w, .096 * w),
            (r.chest, .156 * w, .106 * w), (r.chest + .08 * k, .165 * w, .1 * w), (r.shoulder_z - .012 * k, .16 * w, .076 * w), (r.shoulder_z + .014 * k, .1 * w, .058 * w)]


def torso_weight(r, z):
    if z < r.spine:
        return blend('Hips', 'Spine', (z - r.hip + .02) / .1)
    return blend('Spine', 'Chest', (z - r.spine) / max(r.chest - r.spine, .01))


def lerp_profile(profile, z):
    if z <= profile[0][0]:
        return profile[0]
    for a, b in zip(profile, profile[1:]):
        if a[0] <= z <= b[0]:
            t = (z - a[0]) / (b[0] - a[0] or 1)
            return (z, a[1] + (b[1] - a[1]) * t, a[2] + (b[2] - a[2]) * t)
    return profile[-1]


def build_torso(g, r, p):
    top, inner = p['top'], p.get('inner', p['top'])
    jacket = p.get('jacket')
    k = r.k
    profile = torso_profile(r, p)
    z0, z1 = profile[0][0], profile[-1][0]
    scale = .97 if jacket else 1.0
    color = inner if jacket else top
    # Waist hugging shirt under everything; the jacket shell sits outside it.
    g.loft([ring((0, 0, z), rx * scale, ry * scale, color, torso_weight(r, z)) for z, rx, ry in profile], 18)
    g.loft([ring((0, 0, r.hip - .1 * k), .13 * r.w, .09 * r.w, p['pants'], {'Hips': 1}), ring((0, 0, r.hip + .05), .126 * r.w, .09 * r.w, p['pants'], {'Hips': 1})], 16, (False, False))
    belt = p.get('belt')
    if belt:
        g.loft([ring((0, 0, r.hip + .05 * k), .129 * r.w, .094 * r.w, belt, {'Hips': 1}), ring((0, 0, r.hip + .085 * k), .124 * r.w, .091 * r.w, belt, {'Hips': 1})], 18)
    if jacket:
        build_jacket(g, r, p, jacket, inner, profile)
    if p.get('coat_long'):
        build_coat_tail(g, r, p, jacket)


def build_jacket(g, r, p, jacket, inner, profile):
    k = r.k
    z0, z1 = profile[0][0], profile[-1][0]
    button = r.hip + p.get('button', .15) * k
    wide = p.get('vneck', .62)

    def opening(z):
        if z > button:
            t = (z - button) / (z1 - button)
            return .035 + wide * t ** .85
        return .035 + (.26 * (button - z) / (button - z0) if p.get('flare_open', True) else 0)

    zs = [z0 + (z1 - z0) * i / 15 for i in range(16)]
    rows = []
    for z in zs:
        _, rx, ry = lerp_profile(profile, z)
        th = opening(z)
        rows.append(arc(z, rx * 1.05, ry * 1.07, FRONT + th, FRONT + 2 * math.pi - th, 26))
    weight = lambda q: torso_weight(r, q.z)
    strip(g, rows, jacket, weight)
    # Lapels: a darker strip following each edge of the opening, slightly proud of the jacket.
    lapel = shade(jacket, .82)
    for sign in (1, -1):
        rows = []
        for z in [button + (z1 - button) * i / 8 for i in range(9)]:
            _, rx, ry = lerp_profile(profile, z)
            th = opening(z)
            band = .17 + .28 * (z - button) / (z1 - button)
            a0, a1 = (FRONT + th - .02, FRONT + th + band) if sign > 0 else (FRONT - th - band, FRONT - th + .02)
            rows.append(arc(z, rx * 1.078, ry * 1.1, a0, a1, 3))
        strip(g, rows, lapel, weight)
    # Shirt collar sits inside the lapels at the neck.
    collar = p.get('collar', inner)
    for sign in (1, -1):
        rows = []
        for z, off in ((r.shoulder_z - .04 * k, 0), (r.shoulder_z + .022 * k, .0)):
            _, rx, ry = lerp_profile(profile, z)
            a0, a1 = (FRONT + .06, FRONT + .95) if sign > 0 else (FRONT - .95, FRONT - .06)
            rows.append(arc(z, rx * 1.02 * .86, ry * 1.1 * .96, a0, a1, 4))
        strip(g, rows, collar, {'Chest': 1.0})
    if p.get('buttons', True):
        s = len(g.v)
        g.ball(0, -lerp_profile(profile, button)[2] * 1.1, button, .009 * k, .006 * k, .009 * k, shade(jacket, 1.6), 6, 4)
        for i in range(s, len(g.v)):
            g.w[i] = {'Spine': 1.0}


def build_coat_tail(g, r, p, jacket):
    k, w = r.k, r.w
    rows = []
    for z, rx, ry in ((r.hip - .42 * k, .18, .15), (r.hip - .2 * k, .17, .135), (r.hip - .08 * k, .162, .12)):
        rows.append(arc(z, rx * w, ry * w, FRONT + .3, FRONT + 2 * math.pi - .3, 18, .012))
    strip(g, rows, jacket, {'Hips': 1.0})


def build_arms(g, r, p):
    sleeve = p.get('jacket') or p['top']
    skin = p['skin']
    k, w = r.k, r.w
    rolled = p.get('rolled', False)
    bare = p.get('short_sleeve', False)
    cuff = p.get('cuff', shade(sleeve, 1.08))
    for n, sign in (('L', 1), ('R', -1)):
        arm, fore, hand = 'Arm' + n, 'Fore' + n, 'Hand' + n
        S, E, W, H = r.side(r.sh, sign), r.side(r.elbow, sign), r.side(r.wrist, sign), r.side(r.hand, sign)
        rings = [ring(S + Vector((-sign * .03 * w, 0, .016 * k)), .03 * w, .044 * w, sleeve, {'Chest': 1}),
                 ring(S + Vector((-sign * .006 * w, 0, .006 * k)), .05 * w, .056 * w, sleeve, blend('Chest', arm, .6))]
        for t, rad in ((.25, .052), (.6, .048), (.88, .044), (1.0, .043)):
            rings.append(ring(S.lerp(E, t), rad * w, rad * w * 1.05, sleeve, blend(arm, fore, (t - .75) / .4)))
        if bare:
            g.loft(rings[:4], 10)
            rings = [ring(S.lerp(E, .4), .036 * w, .037 * w, skin, {arm: 1}), ring(E, .032 * w, .034 * w, skin, blend(arm, fore, .5))]
        for t, rad in ((.3, .042), (.68 if rolled else .9, .04), (.72 if rolled else .94, .046 if rolled else .04)):
            rings.append(ring(E.lerp(W, t), rad * w, rad * w, cuff if t > .66 else sleeve, {fore: 1}))
        rings.append(ring(rings[-1]['c'], .02 * w, .02 * w, skin, {fore: 1}))
        g.loft(rings, 10)
        start = E.lerp(W, .66 if rolled else .9)
        g.loft([ring(start, .029 * k, .029 * k, skin, {fore: 1}), ring(W, .024 * k, .021 * k, skin, {fore: 1}),
                ring(W.lerp(H, .4), .03 * k, .015 * k, skin, {hand: 1}), ring(W.lerp(H, .78), .027 * k, .013 * k, skin, {hand: 1}),
                ring(H, .014 * k, .008 * k, skin, {hand: 1})], 8)
        thumb = W.lerp(H, .25) + Vector((0, -.019 * k, 0))
        g.loft([ring(thumb, .009 * k, .009 * k, skin, {hand: 1}), ring(thumb + Vector((-sign * .009 * k, -.011 * k, -.036 * k)), .007 * k, .007 * k, skin, {hand: 1})], 6)


# Upper half follows an ellipse so the hair shell can sit just outside it.
HEAD = [(-1.0, .0, .0), (-.97, .24, .22), (-.86, .47, .44), (-.64, .7, .68), (-.34, .88, .85), (-.02, .985, .96),
        (.26, .965, .98), (.56, .83, .86), (.8, .6, .63), (.94, .34, .36), (1.0, .0, .0)]


def head_ring(t):
    for a, b in zip(HEAD, HEAD[1:]):
        if a[0] <= t <= b[0]:
            f = (t - a[0]) / (b[0] - a[0])
            return a[1] + (b[1] - a[1]) * f, a[2] + (b[2] - a[2]) * f
    return 0, 0


def head_metrics(r):
    return r.head_c, .104 * r.k * r.head, .108 * r.k * r.head, .128 * r.k * r.head


def build_head(g, r, p):
    skin = p['skin']
    FACE_TINT[0] = skin
    c, rx, ry, rz = head_metrics(r)
    head = {'Head': 1.0}
    g.loft([ring((c[0], c[1], c[2] + t * rz), rx * a, ry * b, skin, head) for t, a, b in HEAD[1:-1]], 18)
    k = r.k * r.head
    neck_col = shade(skin, .9)
    g.loft([ring((0, 0, r.shoulder_z - .02), .047 * r.w, .049 * r.w, neck_col, {'Chest': 1}), ring((0, 0, r.chin - .02), .04 * r.k, .042 * r.k, neck_col, blend('Neck', 'Head', .4)),
            ring((0, .004, r.chin + .03), .037 * r.k, .039 * r.k, neck_col, head)], 10, (False, True))
    # nose and ears keep the profile from being a plain egg
    z = c[2] - .35 * rz
    g.loft([ring((0, c[1] - ry * .93, z + .02 * k), .006 * k, .004 * k, skin, head), ring((0, c[1] - ry * .99, z - .002), .007 * k, .007 * k, skin, head)], 6)
    for sign in (1, -1):
        g.ball(sign * rx * .99, c[1] + .012 * k, c[2] - .1 * rz, .009 * k, .019 * k, .03 * k, shade(skin, .96), 8, 5, head)
    build_face_decal(g, r)
    if p.get('glasses'):
        build_glasses(g, r, p)
    if p.get('beard'):
        g.ball(0, c[1] - ry * .9, c[2] - .5 * rz, .036 * k, .012 * k, .007 * k, p['beard'], 8, 4, head)


FACE_TINT = [(1, 1, 1, 1)]


def build_face_decal(g, r):
    """Thin patch hugging the front of the head; material 1 samples the expression atlas."""
    c, rx, ry, rz = head_metrics(r)
    n = 14
    base = len(g.v)
    head = {'Head': 1.0}
    for j in range(n + 1):
        v = j / n
        t = -.98 + v * 1.5
        wx, wy = head_ring(t)
        for i in range(n + 1):
            u = i / n
            x = (u - .5) * 2 * .86 * rx
            s = min(abs(x) / max(wx * rx, 1e-4), .999)
            y = c[1] - ry * wy * math.sqrt(1 - s * s) - .0016
            g.vert((c[0] + x, y, c[2] + t * rz), FACE_TINT[0], (u, v), head)
    for j in range(n):
        for i in range(n):
            a = base + j * (n + 1) + i
            g.face((a, a + 1, a + n + 2, a + n + 1), 1)


def build_glasses(g, r, p):
    c, rx, ry, rz = head_metrics(r)
    k = r.k * r.head
    frame = p.get('glasses_color', rgb('#3a2c28'))
    z = c[2] - .06 * rz
    y = c[1] - ry * .985
    head = {'Head': 1.0}
    for sign in (1, -1):
        cx = sign * .043 * k
        pts = [(cx + math.cos(a) * .033 * k, y + abs(cx) * .3, z + math.sin(a) * .027 * k) for a in [i * math.tau / 14 for i in range(15)]]
        rigid_tube(g, pts, .0026 * k, frame, head)
        rigid_tube(g, [(sign * .076 * k, y + .012, z + .004), (sign * rx * 1.0, c[1] + .02, z + .002)], .0024 * k, frame, head)
    rigid_tube(g, [(.011 * k, y, z + .006), (-.011 * k, y, z + .006)], .0026 * k, frame, head)


def rigid_tube(g, pts, rad, color, weight):
    start = len(g.v)
    g.path_tube(pts, rad, color, 5)
    for i in range(start, len(g.v)):
        g.w[i] = weight


def build_hair(g, r, p):
    rng = random.Random(p.get('seed', 7))
    style = p.get('hair_style', 'short')
    hair, tip = p['hair'], p.get('hair_tip') or shade(p['hair'], 1.45)
    c, rx, ry, rz = head_metrics(r)
    cv = Vector(c)
    k = r.k * r.head
    build_cap(g, r, p, hair, tip)

    def surf(zn):
        """Radial size of the skull at a normalised height; below the temples hair falls straight."""
        return 1.0 if zn < .1 else max(head_ring(min(zn, .995))[0], .05)

    def strand(angle, height, length, width, flare=.0, tip_in=.0, sweep=.0, wiggle=.0, thick=.34, hug=1.07):
        """Closed-form ribbon: it runs down the skull from `height` (radians), then drops away from it."""
        z0 = rz * math.sin(height)
        rings = []
        n = 11
        for i in range(n):
            t = i / (n - 1)
            z = z0 - length * t
            zn = z / rz
            below = max(0.0, (.15 - zn)) / 1.2
            rr = surf(zn) * hug + flare * below - tip_in * t * t * t + wiggle * math.sin(t * 6.3) * t
            a = angle + sweep * t
            sa, ca = math.sin(a), math.cos(a)
            q = cv + Vector((sa * rx * rr, -ca * ry * rr, z))
            w = width * (.7 + .85 * t) * (1 - t ** 3.4) + .0012
            tang = Vector((ca, sa, 0))
            normal = Vector((sa, -ca, .35 * max(zn, 0))).normalized()
            rings.append(dict(c=q, ex=tang * w, ey=normal * w * thick, color=mix(hair, tip, t * t * .8), w={'Head': 1.0}))
        g.loft(rings, 6)

    def bangs(count, spread, length, width, sweep=0.0, center=0.0, height=1.0):
        for i in range(count):
            t = (i + .5) / count * 2 - 1
            strand(center + t * spread, height - abs(t) * .16, length * (1 - abs(t) * .12) * rng.uniform(.92, 1.08), width, sweep=sweep, hug=1.07)

    if style == 'bob':  # Mai: side-swept fringe, shoulder-brushing bob
        bangs(6, .72, .62 * rz, .023 * k, sweep=.28, center=-.1)
        for sign in (1, -1):  # jaw-framing locks
            strand(sign * 1.02, .62, 1.72 * rz, .03 * k, .08, .1, -sign * .18)
            strand(sign * 1.22, .62, 1.82 * rz, .034 * k, .1, .08)
        for i in range(15):
            a = 1.42 + i / 14 * (2 * math.pi - 2.84)
            strand(a, .82 + rng.uniform(-.1, .1), 2.02 * rz * rng.uniform(.95, 1.05), .044 * k, .16, .14)
    elif style == 'pony':  # Linh / An: high ponytail on its own bone so it can sway
        bangs(7, .86, .58 * rz, .02 * k, sweep=-.12)
        for sign in (1, -1):
            strand(sign * 1.08, .5, 1.02 * rz, .018 * k, .02, .02)
            strand(sign * 1.3, .5, .7 * rz, .018 * k)
        for i in range(11):
            strand(1.6 + i / 10 * (2 * math.pi - 3.2), .5, .62 * rz, .036 * k)
        anchor = cv + Vector((0, ry * .98, rz * .55))
        tie = p.get('tie', rgb('#3b4f73'))
        g.ball(anchor.x, anchor.y, anchor.z, .022 * k, .022 * k, .022 * k, tie, 8, 5, {'Pony': 1.0})
        for i in range(9):
            a = i / 9 * math.tau
            base = anchor + Vector((math.cos(a) * .016 * k, math.sin(a) * .01 * k + .012 * k, math.sin(a) * .012 * k))
            pts, d = [base], Vector((math.cos(a) * .2, .78, .36)).normalized()
            length = p.get('pony_len', .34) * k * rng.uniform(.8, 1.1)
            for _ in range(9):
                d = (d + Vector((0, -.03, -.27))).normalized()
                pts.append(pts[-1] + d * length / 9)
            rings = []
            for j, q in enumerate(pts):
                t = j / 9
                w = .032 * k * (.5 + 1.4 * t) * (1 - t ** 2.6) + .0015
                nxt = pts[min(j + 1, 9)] - pts[max(j - 1, 0)]
                side = nxt.cross(Vector((0, 1, 0))).normalized()
                normal = side.cross(nxt).normalized()
                rings.append(dict(c=q, ex=side * w, ey=normal * w * .55, color=mix(hair, tip, t * t * .75), w={'Pony': 1.0}))
            g.loft(rings, 6)
    elif style == 'bun':  # Hạ, the gallery guide: tied back with a low bun
        bangs(5, .78, .42 * rz, .021 * k, sweep=.2)
        for sign in (1, -1):
            strand(sign * 1.06, .45, .95 * rz, .015 * k, .01)
        for i in range(13):
            strand(1.35 + i / 12 * (2 * math.pi - 2.7), .7, 1.0 * rz, .036 * k, tip_in=.05)
        g.ball(0, c[1] + ry * 1.06, c[2] - rz * .02, .052 * k, .048 * k, .052 * k, shade(hair, 1.1), 10, 7, {'Head': 1.0}, tip)
    elif style == 'wave':  # Mẹ Mai: soft short waves, grey through dark
        bangs(6, .86, .5 * rz, .025 * k, sweep=.22)
        for i in range(19):
            a = 1.0 + i / 18 * (2 * math.pi - 2.0)
            strand(a, .8, 1.62 * rz * rng.uniform(.92, 1.08), .042 * k, .22, .1, wiggle=.05)
    elif style == 'men':  # Vinh: short, swept back, temples lighter
        for i in range(9):  # negative length: strands climb from the hairline over the crown
            t = (i + .5) / 9 * 2 - 1
            strand(t * .95, .8 - abs(t) * .22, -.7 * rz, .036 * k, hug=1.1)
        for i in range(21):
            a = .85 + i / 20 * (2 * math.pi - 1.7)
            strand(a, .86 + rng.uniform(-.08, .08), .86 * rz, .04 * k, .02, .04, hug=1.085)
    elif style == 'messy':  # kids: short tufts that lift away from the skull
        for i in range(26):
            a = rng.uniform(-3.1, 3.1)
            strand(a, rng.uniform(.55, 1.2), .55 * rz * rng.uniform(.8, 1.2), .032 * k, .3, -.1, sweep=rng.uniform(-.3, .3), hug=1.1)
    else:  # short neutral
        bangs(7, .9, .5 * rz, .022 * k, sweep=.15)
        for i in range(17):
            strand(1.15 + i / 16 * (2 * math.pi - 2.3), .7, 1.12 * rz, .038 * k, .05, .05)


def build_cap(g, r, p, hair, tip):
    """Skull-hugging hair shell with the face window cut out, plus a soft highlight band."""
    c, rx, ry, rz = head_metrics(r)
    head = {'Head': 1.0}
    rows, seg = 14, 22
    grid = []
    phis = [-.12 + (math.pi / 2 + .12) * j / rows for j in range(rows + 1)]
    for j, phi in enumerate(phis):
        row = []
        band = math.exp(-((phi - .86) / .11) ** 2)
        col = mix(hair, tip, band * .75)
        for i in range(seg):
            a = 2 * math.pi * i / seg
            cz, sz = math.cos(phi), math.sin(phi)
            row.append(g.vert((c[0] + math.sin(a) * cz * rx * 1.06, c[1] - math.cos(a) * cz * ry * 1.06, c[2] + sz * rz * 1.065), col, (0, 0), head))
        grid.append(row)
    for j in range(rows):
        for i in range(seg):
            a = abs(((2 * math.pi * (i + .5) / seg + math.pi) % (2 * math.pi)) - math.pi)
            hairline = .82 - .68 * (a / 1.45) ** 2  # forehead high at the centre, sides fall to the ears
            if a < 1.45 and phis[j] < hairline:
                continue
            g.face((grid[j][i], grid[j][(i + 1) % seg], grid[j + 1][(i + 1) % seg], grid[j + 1][i]))


def build_props(g, r, p):
    k, w = r.k, r.w
    bag = p.get('bag')
    if bag:  # shoulder strap and satchel, as on Mai's and Linh's sheets
        strap = [Vector((-.1 * w, -.07 * w, r.shoulder_z + .012)), Vector((-.04 * w, -.122 * w, r.chest + .02)), Vector((.1 * w, -.118 * w, r.hip + .08)), Vector((.17 * w, -.02, r.hip - .02))]
        back = [Vector((.17 * w, .02, r.hip - .02)), Vector((.13 * w, .12 * w, r.hip + .12 * k)), Vector((.02 * w, .135 * w, r.chest)), Vector((-.1 * w, .092 * w, r.shoulder_z + .012))]
        rigid_tube(g, strap, .008 * k, bag, {'Chest': 1.0})
        rigid_tube(g, back, .008 * k, bag, {'Chest': 1.0})
        s = len(g.v)
        g.bevel_box(.195 * w, .01, r.hip - .09 * k, .07 * k, .22 * k, .16 * k, bag, .012, shade(bag, 1.15))
        for i in range(s, len(g.v)):
            g.w[i] = {'Hips': 1.0}
    if p.get('lanyard'):
        rigid_tube(g, [(-.05 * w, -.08 * w, r.shoulder_z), (0, -.118 * w, r.chest + .02), (.05 * w, -.08 * w, r.shoulder_z)], .004 * k, p['lanyard'], {'Chest': 1.0})
        s = len(g.v)
        g.box(0, -.122 * w, r.chest - .02, .05 * k, .006, .07 * k, rgb('#f2efe6'))
        for i in range(s, len(g.v)):
            g.w[i] = {'Chest': 1.0}
    if p.get('toolbelt'):
        s = len(g.v)
        g.bevel_box(-.15 * w, -.05, r.hip - .07 * k, .06 * k, .11 * k, .14 * k, p['toolbelt'], .01)
        for i in range(s, len(g.v)):
            g.w[i] = {'Hips': 1.0}


def make_character(name, p, out):
    reset()
    g = Geo()
    r = Rig(p)
    build_legs(g, r, p)
    build_torso(g, r, p)
    build_arms(g, r, p)
    build_head(g, r, p)
    build_hair(g, r, p)
    build_props(g, r, p)
    for i, w in enumerate(g.w):  # unassigned verts ride the chest instead of collapsing to the origin
        if not w:
            g.w[i] = {'Chest': 1.0}
    obj = g.to_object('Body', ['Body', 'Face'])
    joints = r.joints()
    arm = bpy.data.armatures.new('Skeleton')
    rig = bpy.data.objects.new('Skeleton', arm)
    bpy.context.scene.collection.objects.link(rig)
    bpy.context.view_layer.objects.active = rig
    rig.select_set(True)
    bpy.ops.object.mode_set(mode='EDIT')
    # Every bone points straight up, so at runtime rotations can be written in character space.
    for n, parent in BONES:
        b = arm.edit_bones.new(n)
        b.head = joints[n]
        b.tail = Vector(joints[n]) + Vector((0, 0, .06))
    for n, parent in BONES:
        if parent:
            arm.edit_bones[n].parent = arm.edit_bones[parent]
    bpy.ops.object.mode_set(mode='OBJECT')
    for n, _ in BONES:
        obj.vertex_groups.new(name=n)
    for i, w in enumerate(g.w):
        total = sum(w.values()) or 1
        for bone, value in w.items():
            obj.vertex_groups[bone].add([i], value / total, 'REPLACE')
    obj.parent = rig
    obj.modifiers.new('Skin', 'ARMATURE').object = rig
    bpy.ops.object.select_all(action='SELECT')
    export_fbx(os.path.join(out, name + '.fbx'), armature=True)
    print('CIVI built', name, len(g.v), 'verts', len(g.f), 'faces')


SKIN = rgb('#f3d3ba')
SKIN_TAN = rgb('#e2b892')
SKIN_OLD = rgb('#e6c4a8')
PEOPLE = {
    # Sheet 00
    'mai': dict(H=1.62, skin=SKIN, hair=rgb('#2b2029'), hair_tip=rgb('#6a4a52'), hair_style='bob', jacket=rgb('#3f6a7c'), inner=rgb('#efe6d8'), top=rgb('#efe6d8'),
                pants=rgb('#38343a'), shoes=rgb('#2c2422'), belt=rgb('#4a2f22'), bag=rgb('#6e4a30'), rolled=True, cuff=rgb('#4d7a8c'), pants_wide=1.2, seed=11, vneck=.6, hem=.12),
    'an': dict(H=1.32, head=1.28, leg=.96, build=.92, skin=SKIN, hair=rgb('#33262a'), hair_tip=rgb('#6a4a44'), hair_style='pony', pony_len=.3, jacket=rgb('#d3a22e'), inner=rgb('#efe6d8'), top=rgb('#efe6d8'),
               pants=rgb('#6a6767'), shoes=rgb('#efe8dc'), sole=rgb('#c5533f'), rolled=True, cuff=rgb('#c2902a'), pants_wide=1.3, seed=23, hem=.14, tie=rgb('#c5533f'), buttons=False, button=.1, face='kid'),
    'ha': dict(H=1.6, build=1.06, skin=SKIN_TAN, hair=rgb('#2b2226'), hair_tip=rgb('#5a4240'), hair_style='bun', jacket=rgb('#a14a3a'), inner=rgb('#4a4240'), top=rgb('#4a4240'),
               pants=rgb('#4a4442'), shoes=rgb('#6b4d36'), belt=rgb('#7a5c3e'), rolled=True, cuff=rgb('#c5b49c'), pants_wide=1.34, toolbelt=rgb('#7a5c3e'), seed=31, vneck=.74, buttons=False, button=.06),
    # Sheet 01
    'linh': dict(H=1.6, skin=SKIN, hair=rgb('#33262a'), hair_tip=rgb('#6d4a44'), hair_style='pony', pony_len=.3, glasses=True, jacket=rgb('#5b7f93'), inner=rgb('#d8c9b4'), top=rgb('#d8c9b4'),
                 pants=rgb('#3d3836'), shoes=rgb('#cfc7bd'), sole=rgb('#8895ad'), belt=rgb('#2e2826'), bag=rgb('#6b4b34'), rolled=True, cuff=rgb('#6c8ea2'), pants_wide=1.28, seed=42, vneck=.78, buttons=False, button=.04),
    'vinh': dict(H=1.76, build=1.06, female=False, head=1.06, skin=SKIN_TAN, hair=rgb('#332d30'), hair_tip=rgb('#9a989a'), hair_style='men', jacket=rgb('#3b3536'), inner=rgb('#596077'), top=rgb('#596077'),
                 pants=rgb('#332e2f'), shoes=rgb('#3a2c26'), belt=rgb('#231d1c'), rolled=True, pants_wide=1.15, seed=53, vneck=.5, hem=.15, face='man'),
    # Sheet 04
    'me_mai': dict(H=1.55, build=.98, skin=SKIN_OLD, hair=rgb('#4d474c'), hair_tip=rgb('#c4c0c2'), hair_style='wave', jacket=rgb('#4d7385'), inner=rgb('#ecdfd0'), top=rgb('#ecdfd0'),
                   pants=rgb('#5b3f30'), shoes=rgb('#2c2422'), cuff=rgb('#6c8ea2'), pants_wide=1.24, seed=61, face='old', vneck=.6, hem=.17),
    # Unnamed roles in chapter 1: built from the same wardrobe language
    'huong_dan': dict(H=1.6, skin=SKIN, hair=rgb('#2e2428'), hair_tip=rgb('#5a4240'), hair_style='bun', jacket=rgb('#2d6d6e'), inner=rgb('#f4efe5'), top=rgb('#f4efe5'), pants=rgb('#2c4a4e'), shoes=rgb('#2c2422'), lanyard=rgb('#d3b36a'), seed=71, hem=.11),
    'hoc_sinh': dict(H=1.34, head=1.26, build=.9, skin=SKIN, hair=rgb('#2c2126'), hair_style='messy', top=rgb('#f4efe5'), pants=rgb('#39566a'), shoes=rgb('#f2ece2'), sole=rgb('#39566a'), short_sleeve=True, seed=72, face='kid', hem=.09),
    'dong_nghiep_a': dict(H=1.72, build=1.02, female=False, head=1.06, skin=SKIN_TAN, hair=rgb('#2a2428'), hair_style='men', jacket=rgb('#54737f'), inner=rgb('#efe6d8'), top=rgb('#efe6d8'), pants=rgb('#3b4047'), shoes=rgb('#332824'), lanyard=rgb('#3ff0c0'), seed=73, face='man', vneck=.5),
    'dong_nghiep_b': dict(H=1.6, skin=SKIN, hair=rgb('#3c2c2c'), hair_tip=rgb('#7a5a4e'), hair_style='short', jacket=rgb('#7b8f76'), inner=rgb('#f3ece0'), top=rgb('#f3ece0'), pants=rgb('#4a4344'), shoes=rgb('#2f2826'), lanyard=rgb('#3ff0c0'), seed=74, pants_wide=1.15),
    'truc': dict(H=1.7, female=False, head=1.06, skin=SKIN_TAN, hair=rgb('#262024'), hair_style='short', top=rgb('#35626c'), pants=rgb('#2f3b42'), shoes=rgb('#2c2422'), lanyard=rgb('#d3b36a'), seed=75, face='man', hem=.09),
    # Flashback, scene 1
    'mai_nho': dict(H=1.2, head=1.32, build=.84, leg=.94, skin=SKIN, hair=rgb('#2b2029'), hair_style='short', top=rgb('#d8cbbb'), pants=rgb('#5f6a78'), shoes=rgb('#6b5a4a'), seed=81, face='kid', hem=.17),
    'ao_xam': dict(H=1.62, skin=SKIN_OLD, hair=rgb('#3d3a3e'), hair_tip=rgb('#8c8a8c'), hair_style='bun', jacket=rgb('#7a7c80'), coat_long=True, inner=rgb('#dad6cf'), top=rgb('#dad6cf'), pants=rgb('#55565c'), shoes=rgb('#2c2826'), seed=82, face='old'),
    'nv_benh_vien': dict(H=1.72, female=False, head=1.06, skin=SKIN_TAN, hair=rgb('#262024'), hair_style='men', jacket=rgb('#e9ebe8'), coat_long=True, inner=rgb('#7ea59a'), top=rgb('#7ea59a'), pants=rgb('#7ea59a'), shoes=rgb('#e9e6df'), seed=83, face='man'),
}


def main():
    argv = sys.argv[sys.argv.index('--') + 1:]
    marker_file, out = argv[0], argv[1]
    os.makedirs(out, exist_ok=True)
    try:
        for name in argv[2:] or list(PEOPLE):
            make_character(name, PEOPLE[name], out)
    except Exception:
        import traceback
        with open(marker_file + '.error', 'w') as f:
            traceback.print_exc(file=f)
    with open(marker_file, 'w') as f:
        f.write('ok')


if __name__ == '__main__':
    main()
