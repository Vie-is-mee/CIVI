"""Draws the expression atlases used by Civi/Face. Cells (4x2): neutral, blink, talk, smile / sad, surprised, serious, happy-closed."""
import math
import os
import sys
from PIL import Image, ImageDraw, ImageFilter

SS = 3  # draw big, downsample for clean anti-aliased lines
CELL = 256 * SS

STYLES = {
    'adult': dict(iris=(66, 44, 40), ink=(36, 24, 26), brow=(58, 40, 40), blush=(238, 150, 140), eye=1.0, lines=0),
    'kid': dict(iris=(74, 48, 40), ink=(36, 24, 26), brow=(60, 42, 40), blush=(240, 140, 130), eye=1.2, lines=0),
    'man': dict(iris=(52, 38, 36), ink=(30, 22, 24), brow=(40, 30, 30), blush=(0, 0, 0), eye=.78, lines=1),
    'old': dict(iris=(60, 46, 44), ink=(46, 34, 34), brow=(120, 112, 112), blush=(226, 150, 140), eye=.82, lines=2),
}


def eye(d, cx, cy, s, st, mode, side):
    ink, iris = st['ink'], st['iris']
    w, h = 36 * s * SS, 40 * s * SS
    x, y = cx * SS, cy * SS
    if mode in ('blink', 'happy'):
        pts = []
        for i in range(13):
            t = i / 12
            lift = math.sin(t * math.pi) * (h * .34 if mode == 'happy' else -h * .1)
            pts.append((x - w + 2 * w * t, y + h * .1 - lift))
        d.line(pts, fill=ink, width=int(6 * SS * max(s, .85)), joint='curve')
        return
    open_h = h * (1.16 if mode == 'surprised' else .86 if mode == 'sad' else .72 if mode == 'serious' else 1.0)
    top = y - open_h * .62
    # white of the eye
    d.ellipse((x - w * .9, top, x + w * .9, y + open_h * .72), fill=(255, 250, 246, 255))
    # iris with a darker pupil and a lighter lower crescent
    ir_w, ir_h = w * .62, open_h * .78
    iy = y + open_h * .08
    d.ellipse((x - ir_w, iy - ir_h, x + ir_w, iy + ir_h), fill=iris + (255,))
    lower = tuple(min(c + 72, 255) for c in iris) + (255,)
    d.ellipse((x - ir_w * .72, iy, x + ir_w * .72, iy + ir_h * .86), fill=lower)
    d.ellipse((x - ir_w * .46, iy - ir_h * .5, x + ir_w * .46, iy + ir_h * .36), fill=(24, 16, 20, 255))
    # highlights
    d.ellipse((x - ir_w * .62, iy - ir_h * .72, x - ir_w * .08, iy - ir_h * .2), fill=(255, 255, 255, 255))
    d.ellipse((x + ir_w * .16, iy + ir_h * .2, x + ir_w * .46, iy + ir_h * .5), fill=(255, 255, 255, 220))
    # upper lash: thick arc with an outward flick
    pts = []
    for i in range(17):
        t = i / 16
        px = x - w + 2.04 * w * t
        pts.append((px, top + open_h * .04 + (2 * t - 1) ** 2 * open_h * .3))
    d.line(pts, fill=ink, width=int(8.5 * SS * max(s, .85)), joint='curve')
    flick = pts[-1] if side > 0 else pts[0]
    d.line([flick, (flick[0] + side * 8 * SS * s, flick[1] + 7 * SS)], fill=ink, width=int(5 * SS))
    d.line([(x - w * .7, y + open_h * .74), (x + w * .7, y + open_h * .74)], fill=ink + (90,), width=int(1.6 * SS))


def brow(d, cx, cy, side, st, mode):
    x, y = cx * SS, cy * SS
    tilt = {'sad': -9, 'serious': 7, 'surprised': -2}.get(mode, -2)
    lift = {'surprised': -9, 'sad': -2, 'serious': 3}.get(mode, 0) * SS
    inner, outer = (x - side * 28 * SS, y + lift + tilt * SS * .5), (x + side * 32 * SS, y + lift - tilt * SS * .5)
    mid = ((inner[0] + outer[0]) / 2, min(inner[1], outer[1]) - 5 * SS)
    d.line([inner, mid, outer], fill=st['brow'], width=int(5 * SS), joint='curve')


def mouth(d, mode, st):
    x, y = 128 * SS, 201 * SS
    ink = st['ink']
    lip = (176, 84, 84, 255)
    if mode == 'talk':
        d.ellipse((x - 11 * SS, y - 5 * SS, x + 11 * SS, y + 11 * SS), fill=(120, 44, 52, 255))
        d.ellipse((x - 7 * SS, y + 4 * SS, x + 7 * SS, y + 11 * SS), fill=(214, 110, 110, 255))
    elif mode == 'surprised':
        d.ellipse((x - 8 * SS, y - 6 * SS, x + 8 * SS, y + 12 * SS), fill=(120, 44, 52, 255))
    elif mode in ('smile', 'happy'):
        d.arc((x - 21 * SS, y - 15 * SS, x + 21 * SS, y + 11 * SS), 22, 158, fill=lip, width=int(4.5 * SS))
    elif mode == 'sad':
        d.arc((x - 15 * SS, y + 4 * SS, x + 15 * SS, y + 21 * SS), 205, 335, fill=lip, width=int(4 * SS))
    else:
        d.line([(x - 13 * SS, y + 2 * SS), (x + 13 * SS, y + 2 * SS)], fill=lip, width=int(4 * SS))


def cell(style, mode):
    st = STYLES[style]
    img = Image.new('RGBA', (CELL, CELL), (0, 0, 0, 0))
    glow = Image.new('RGBA', (CELL, CELL), (0, 0, 0, 0))
    if st['blush'][0]:
        # Blur only the mask: blurring the colour too would pull in the transparent black and turn the cheeks grey.
        mask = Image.new('L', (CELL, CELL), 0)
        g = ImageDraw.Draw(mask)
        for cx in (62, 194):
            g.ellipse(((cx - 27) * SS, 158 * SS, (cx + 27) * SS, 184 * SS), fill=128)
        glow = Image.new('RGBA', (CELL, CELL), st['blush'] + (255,))
        glow.putalpha(mask.filter(ImageFilter.GaussianBlur(9 * SS)))
        img.alpha_composite(glow)
    d = ImageDraw.Draw(img, 'RGBA')
    s = st['eye']
    for side, cx in ((-1, 80), (1, 176)):
        eye(d, cx, 122, s, st, mode, side)
        brow(d, cx, 74, side, st, mode)
    # nose is only a soft shadow in this style
    d.line([(126 * SS, 166 * SS), (130 * SS, 172 * SS)], fill=(196, 130, 112, 200), width=int(3 * SS))
    mouth(d, mode, st)
    if st['lines']:
        d.line([(100 * SS, 188 * SS), (92 * SS, 202 * SS)], fill=(150, 104, 92, 120), width=int(2.5 * SS))
        d.line([(156 * SS, 188 * SS), (164 * SS, 202 * SS)], fill=(150, 104, 92, 120), width=int(2.5 * SS))
    if st['lines'] > 1:
        for cx in (56, 200):
            d.arc(((cx - 16) * SS, 144 * SS, (cx + 16) * SS, 162 * SS), 20, 160, fill=(150, 104, 92, 90), width=int(2 * SS))
    return img.resize((256, 256), Image.LANCZOS)


def main(out):
    os.makedirs(out, exist_ok=True)
    modes = ['neutral', 'blink', 'talk', 'smile', 'sad', 'surprised', 'serious', 'happy']
    for style in STYLES:
        atlas = Image.new('RGBA', (1024, 512), (0, 0, 0, 0))
        for i, m in enumerate(modes):
            atlas.paste(cell(style, m), ((i % 4) * 256, (i // 4) * 256))
        # The decal is mapped with V up, so keep the sheet in reading order for FaceCell().
        atlas.save(os.path.join(out, f'face_{style}.png'))


if __name__ == '__main__':
    main(sys.argv[1])
