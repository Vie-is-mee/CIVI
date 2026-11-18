"""Brings the audio packages in AUDIO/ into the Unity project: Ogg copies of every stem, and cues.json, which ties each sound to its
line of the script (tools/audio_plan.py). The package WAVs stay untouched. Preview mixes are never imported: the READMEs forbid
playing them on top of their own stems."""
import csv
import glob
import json
import os
import subprocess
import audio_plan as plan

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
AUDIO = os.path.join(ROOT, 'AUDIO')
CH1 = os.path.join(AUDIO, 'CIVI_CH1_AUDIO', 'VSIC26_CH1_AUDIO_GIAO_NHOM_v01')
PACK2 = os.path.join(AUDIO, 'VSIC26_CH2_AMB_SFX_FOOTSTEPS_20261105')
CH3 = os.path.join(AUDIO, 'CH3_AMB_SFX_DELIVERY_V03', '01_AMB_SFX')
DST = os.path.join(ROOT, 'Game', 'Assets', '_Civi', 'Resources', 'Audio')


def to_ogg(src, dst, quality=4):
    os.makedirs(os.path.dirname(dst), exist_ok=True)
    if not os.path.exists(dst):
        subprocess.run(['ffmpeg', '-v', 'error', '-y', '-i', src, '-c:a', 'libvorbis', '-q:a', str(quality), dst], check=True)


def copy_all(pattern, kind):
    files = glob.glob(pattern)
    assert files, pattern
    for f in files:
        to_ogg(f, os.path.join(DST, kind, os.path.basename(f)[:-4] + '.ogg'))
    return len(files)


def line_of(content, words, chapter):
    hits = [i for i, text in enumerate(content['paragraphs']) if words in text]
    assert hits, f'chapter {chapter}: no line says "{words}"'
    return hits[0]


def main():
    counts = {}
    counts['ch1'] = copy_all(os.path.join(CH1, '01_CHUONG_1_AMB_SFX', 'Scenes', '*', 'Ambience', '*.wav'), 'Amb')
    counts['ch1'] += copy_all(os.path.join(CH1, '01_CHUONG_1_AMB_SFX', 'Scenes', '*', 'SFX_OneShots', '*.wav'), 'Sfx')
    copy_all(os.path.join(CH1, '02_UI_SFX', 'WAV_48k_24bit', '*.wav'), 'Ui')
    counts['ch2'] = copy_all(os.path.join(PACK2, '01_Chuong2_SFX_AMB', 'AMB', '*.wav'), 'Amb')
    counts['ch2'] += copy_all(os.path.join(PACK2, '01_Chuong2_SFX_AMB', 'SFX', '*.wav'), 'Sfx')
    counts['ch3'] = copy_all(os.path.join(CH3, '*', 'AMB', '*.wav'), 'Amb')
    counts['ch3'] += copy_all(os.path.join(CH3, '*', 'SFX_UI', '*.wav'), 'Sfx')
    banks = {}
    for bank in sorted(os.listdir(os.path.join(PACK2, '02_Footsteps_ToanGame', 'WAV'))):
        banks[bank] = copy_all(os.path.join(PACK2, '02_Footsteps_ToanGame', 'WAV', bank, '*.wav'), 'Foot')

    res = os.path.join(ROOT, 'Game', 'Assets', '_Civi', 'Resources')
    chapters = {n: json.load(open(os.path.join(res, f'Chapter{n}', 'chapter.json'), encoding='utf8')) for n in (1, 2, 3)}
    sheet = {r['cue_id'] + '|' + r['scene'][-2:]: os.path.basename(r['wav_relative_path'])[:-4]
             for r in csv.DictReader(open(os.path.join(CH1, '01_CHUONG_1_AMB_SFX', 'CUE_SHEET.csv'), encoding='utf-8-sig'))}
    cues = []
    for scene, cue, words, delay in plan.CHAPTER1:
        cues.append(dict(chapter=1, line=line_of(chapters[1], words, 1), clip=sheet[f'{cue}|{scene:02}'], delay=delay))
    for chapter, table in ((2, plan.CHAPTER2), (3, plan.CHAPTER3)):
        for clip, words, delay in table:
            assert os.path.exists(os.path.join(DST, 'Sfx', clip + '.ogg')) or os.path.exists(os.path.join(DST, 'Amb', clip + '.ogg')), clip
            cues.append(dict(chapter=chapter, line=line_of(chapters[chapter], words, chapter), clip=clip, delay=delay))
    beds = []
    for chapter, table, mid in ((2, plan.BEDS2, plan.MID_BEDS2), (3, plan.BEDS3, plan.MID_BEDS3)):
        assert len(table) == len(chapters[chapter]['steps'])
        for step, layers in enumerate(table):
            beds.append(dict(chapter=chapter, step=step, line=chapters[chapter]['steps'][step]['before'][0], clips=layers))
        for words, layers in mid:
            beds.append(dict(chapter=chapter, step=-1, line=line_of(chapters[chapter], words, chapter), clips=layers))
    for entry in beds:
        for clip in entry['clips']:
            assert os.path.exists(os.path.join(DST, 'Amb', clip + '.ogg')), clip
    with open(os.path.join(DST, 'cues.json'), 'w', encoding='utf8') as f:
        json.dump(dict(cues=cues, beds=beds, banks=[dict(name=k, count=v) for k, v in banks.items()]), f, ensure_ascii=False, indent=0)
    planned = {c['clip'] for c in cues}
    unused = [os.path.basename(p)[:-4] for p in glob.glob(os.path.join(PACK2, '01_Chuong2_SFX_AMB', 'SFX', '*.wav')) + glob.glob(os.path.join(CH3, '*', 'SFX_UI', '*.wav')) if os.path.basename(p)[:-4] not in planned]
    print('imported', counts, 'foot banks', banks, 'cues', len(cues), 'beds', len(beds))
    print('sounds with no place in the script:', unused)


if __name__ == '__main__':
    main()
