"""Inspect/download only selected source members of the supplied Drive ZIP."""
import io, json, pathlib, re, struct, urllib.parse, urllib.request, zipfile, sys, zlib
from concurrent.futures import ThreadPoolExecutor

ROOT = pathlib.Path(__file__).resolve().parents[1]
OUT = ROOT / 'artifacts/chapter1-integration-2026-11-04/source'
SIZE = 813488209
URL = 'https://drive.usercontent.google.com/download?id=1yJsjHukS72O4-GwZuAmGkxkdTEOCJ0Ro&export=download&confirm=t'

class RemoteZip(io.RawIOBase):
    def __init__(self):
        self.pos = 0
    def seekable(self): return True
    def seek(self, offset, whence=0):
        self.pos = offset if whence == 0 else self.pos + offset if whence == 1 else SIZE + offset
        return self.pos
    def tell(self): return self.pos
    def read(self, n=-1):
        n = SIZE - self.pos if n < 0 else min(n, SIZE - self.pos)
        if not n: return b''
        req = urllib.request.Request(URL, headers={'Range': f'bytes={self.pos}-{self.pos+n-1}'})
        with urllib.request.urlopen(req, timeout=90) as response:
            if response.status != 206:
                raise RuntimeError(f'Expected partial content, got {response.status}: {response.read(3000)!r}')
            data = response.read(n)
        if len(data) != n: raise RuntimeError('Truncated range')
        self.pos += n
        return data

def main():
    OUT.mkdir(parents=True, exist_ok=True)
    with zipfile.ZipFile(RemoteZip()) as archive:
        names = archive.namelist()
        (OUT/'inventory.json').write_text(json.dumps([{'name': i.filename, 'size': i.file_size} for i in archive.infolist()], ensure_ascii=False, indent=2), encoding='utf-8')
        selected = [n for n in names if not n.endswith('/') and '/Library/' not in n and '/artifacts/' not in n and (
            n in ('CIVI_5/dialogue/Chuong_1_Ban_Duyet.md', 'CIVI_5/dialogue/README.md') or
            '/Scripts/Application/LuuQuang/' in n or
            '/Resources/LuuQuangChapterOne/' in n or
            n.endswith(('/tools/compile_chapter_dialogue.py', '/tools/dialogue_content.py', '/tools/test_dialogue_content.py', '/docs/qa/dialogue-review.md', '/docs/production/chapter1-revision2-handoff.md')) or
            ('/Art/' in n and any(s in n.lower() for s in ('walk-v2', 'portraits', 'office.png', 'home.png', 'gallery-v2', 'cafeteria-v2', 'meeting-v2', 'npc-v2', 'provenance', 'license'))) or
            (n.endswith('.md') and any(s in n.lower() for s in ('provenance', 'license', 'attribution'))))]
        if '--text-first' in sys.argv:
            selected = [n for n in selected if n.endswith(('.cs','.md','.py','.json')) and ('/dialogue/' not in n or n.endswith(('Chuong_1_Ban_Duyet.md','README.md')))]
            selected += [n for n in names if n.endswith(('/chapter1-revision2-prompts.json','/luuquang-chapter1-imagegen-prompts.json'))]
        infos = {n: archive.getinfo(n) for n in selected}
        def download(name):
            dest = OUT / pathlib.PurePosixPath(name)
            if OUT.resolve() not in dest.resolve().parents: raise ValueError(name)
            if dest.exists(): return
            dest.parent.mkdir(parents=True, exist_ok=True)
            info = infos[name]
            remote = RemoteZip(); remote.seek(info.header_offset)
            # Fetch local header, filename, extra and payload in one range.
            raw = remote.read(info.compress_size + 30 + len(name.encode('utf-8')) + 65535)
            name_len, extra_len = struct.unpack_from('<HH', raw, 26)
            payload = raw[30+name_len+extra_len:30+name_len+extra_len+info.compress_size]
            data = zlib.decompress(payload, -15) if info.compress_type == 8 else payload
            if len(data) != info.file_size or zlib.crc32(data) != info.CRC: raise ValueError('CRC mismatch: '+name)
            dest.write_bytes(data)
            print(name, dest.stat().st_size, flush=True)
        with ThreadPoolExecutor(max_workers=6) as pool:
            list(pool.map(download, selected))
        print(f'Selected {len(selected)} / {len(names)} members')

if __name__ == '__main__': main()
