"""Build a standalone copy of the CIVI web demo.

web-demo/index.html is written as page content (no <!doctype>/<head>), the format
the Claude Artifact viewer wraps itself. This script wraps it in a full document so
it can be opened directly or hosted on any static host (GitHub Pages, Netlify...).

Usage: python tools/build_web_demo.py   ->  web-demo/dist/
"""
import pathlib, shutil

ROOT = pathlib.Path(__file__).resolve().parents[1]
SRC = ROOT / "web-demo"
OUT = SRC / "dist"

HEAD = ('<!doctype html><html lang="vi"><head><meta charset="utf-8">'
        '<meta name="viewport" content="width=device-width,initial-scale=1,viewport-fit=cover">'
        '</head><body>\n')
TAIL = "\n</body></html>\n"

def main():
    if OUT.exists():
        shutil.rmtree(OUT)
    (OUT / "assets").mkdir(parents=True)
    page = (SRC / "index.html").read_text(encoding="utf-8")
    (OUT / "index.html").write_text(HEAD + page + TAIL, encoding="utf-8")
    for f in (SRC / "assets").iterdir():
        shutil.copy2(f, OUT / "assets" / f.name)
    print("built", OUT)

if __name__ == "__main__":
    main()
