"""Brings the Chapter 1 audio package into the Unity project.
Source WAVs stay untouched in source-assets/audio. Unity gets Ogg copies plus cues.json, which spreads the cue sheet's
prototype timings over each dialogue scene (the sheet's own README says its times are illustrative).
"""
import csv
import json
import math
import os
import random
import struct
import subprocess
import wave

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
SRC = os.path.join(ROOT, 'source-assets', 'audio')
DST = os.path.join(ROOT, 'Game', 'Assets', '_Civi', 'Resources', 'Audio')
ALBUM = {'take_photo_album', 'turn_album_page', 'album_between_them'}


def to_ogg(src, dst):
    os.makedirs(os.path.dirname(dst), exist_ok=True)
    if os.path.exists(dst):
        return
    subprocess.run(['ffmpeg', '-v', 'error', '-y', '-i', src, '-c:a', 'libvorbis', '-q:a', '4', dst], check=True)


def footstep(path, seed, hard):
    """Soft filtered thump: the package has walking beds but no single footstep."""
    rng = random.Random(seed)
    rate, length = 44100, int(44100 * .13)
    low, out = 0.0, []
    for i in range(length):
        t = i / rate
        env = math.exp(-t * (55 if hard else 40))
        low += (rng.uniform(-1, 1) - low) * (.34 if hard else .2)
        out.append(int(max(-1, min(1, (low * 1.2 + math.sin(t * 2 * math.pi * 92) * .5) * env)) * 11000))
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with wave.open(path, 'wb') as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(rate)
        w.writeframes(struct.pack('<' + 'h' * length, *out))


def main():
    scenes = os.path.join(SRC, '01_CHUONG_1_AMB_SFX', 'Scenes')
    for scene in sorted(os.listdir(scenes)):
        for sub, kind in (('Ambience', 'Amb'), ('SFX_OneShots', 'Sfx')):
            folder = os.path.join(scenes, scene, sub)
            if os.path.isdir(folder):
                for f in os.listdir(folder):
                    if f.endswith('.wav'):
                        to_ogg(os.path.join(folder, f), os.path.join(DST, kind, f[:-4] + '.ogg'))
    ui = os.path.join(SRC, '02_UI_SFX', 'WAV_48k_24bit')
    for f in os.listdir(ui):
        to_ogg(os.path.join(ui, f), os.path.join(DST, 'Ui', f[:-4] + '.ogg'))
    for i in range(4):
        footstep(os.path.join(DST, 'Sfx', f'foot_{i}.wav'), i, i % 2 == 1)

    rows = list(csv.DictReader(open(os.path.join(SRC, '01_CHUONG_1_AMB_SFX', 'CUE_SHEET.csv'), encoding='utf-8-sig')))
    span = {}
    for r in rows:
        span[r['scene']] = max(span.get(r['scene'], 0), float(r['time_s']))
    cues = []
    for r in rows:
        n = int(r['scene'].split('-')[1])
        # The package has 11 scenes and the script 12: album and "Một hồ sơ nhỏ" were split apart in review.
        scene = 11 if n == 10 and r['cue_id'] in ALBUM else 12 if n == 11 else n
        name = os.path.basename(r['wav_relative_path'])[:-4]
        cues.append(dict(scene=scene, at=round(float(r['time_s']) / span[r['scene']], 3), clip=name))
    with open(os.path.join(DST, 'cues.json'), 'w', encoding='utf8') as f:
        json.dump(dict(cues=cues), f, ensure_ascii=False, indent=0)
    print('cues', len(cues))


if __name__ == '__main__':
    main()
