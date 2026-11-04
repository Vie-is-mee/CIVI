"""Build a standalone copy of the CIVI web demo.

web-demo/index.html (the 3D game) and web-demo/classic.html (the older text version)
are written as page content (no <!doctype>/<head>), the format the Claude Artifact
viewer wraps itself. This script wraps both in a full document, with the same small
reset the viewer adds, and copies the game scripts and assets next to them, so the
demo can be opened directly or hosted on any static host (GitHub Pages, Netlify...).

Usage: python tools/build_web_demo.py   ->  web-demo/dist/
"""
import pathlib, shutil

ROOT = pathlib.Path(__file__).resolve().parents[1]
SRC = ROOT / "web-demo"
OUT = SRC / "dist"

HEAD = ('<!doctype html><html lang="vi"><head><meta charset="utf-8">'
        '<meta name="viewport" content="width=device-width,initial-scale=1,viewport-fit=cover">'
        '<style>:root{color-scheme:light;padding-top:env(safe-area-inset-top,0px);'
        'padding-bottom:env(safe-area-inset-bottom,0px)}body{margin:0;font:14px system-ui,sans-serif}'
        'img{max-width:100%}[hidden]{display:none!important}</style>'
        '</head><body>\n')
PAGES = ("index.html", "classic.html")
TAIL = "\n</body></html>\n"

def main():
    if OUT.exists():
        shutil.rmtree(OUT)
    (OUT / "assets").mkdir(parents=True)
    (OUT / "game").mkdir()
    for name in PAGES:
        page = (SRC / name).read_text(encoding="utf-8")
        (OUT / name).write_text(HEAD + page + TAIL, encoding="utf-8")
    for f in (SRC / "assets").iterdir():
        shutil.copy2(f, OUT / "assets" / f.name)
    for f in sorted((SRC / "game").glob("*.js")):
        shutil.copy2(f, OUT / "game" / f.name)
    print("built", OUT)

if __name__ == "__main__":
    main()
