"""Refresh TV3 progress from actual files and the production map. Python 3, stdlib only.

Run from CIVI_2: python tools/QA/update_tv3_progress.py
Use --check to verify the current report without writing files.
This verifies a file handoff, not Unity import or gameplay.
"""
from pathlib import Path
from collections import Counter
import argparse
import hashlib
import json
import re

ROOT = Path(__file__).resolve().parents[2]
MAP = ROOT / 'docs/production/tv3-asset-map.json'
LABELS = {
    'candidate': 'Đã có ứng viên · chờ duyệt và tích hợp',
    'candidate_needs_variants': 'Đã có ảnh · còn thiếu biến thể',
    'candidate_needs_revision': 'Đã có ảnh · cần chỉnh theo brief',
    'missing': 'Chưa có file',
    'composition_pending': 'Chưa ghép nền và đạo cụ',
    'animatic_pending': 'Chưa có animatic độc lập',
    'authored_dialogue': 'Nội dung TV1 · chờ nối thoại',
    'prefab_pending': 'Chưa có prefab/layer riêng',
    'runtime_composite_pending': 'Chưa có trang dựng động',
    'prefab_candidate': 'Đã có prefab riêng · chờ nhóm duyệt',
    'animatic_candidate': 'Đã có animatic kéo lại được · chờ nhóm duyệt',
}

def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

def read_json(path):
    return json.loads(path.read_text(encoding='utf-8-sig'))

def local(path):
    resolved = (ROOT / path).resolve()
    resolved.relative_to(ROOT.resolve())
    return resolved

def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--check', action='store_true')
    args = parser.parse_args()
    data = read_json(MAP)
    records = data['copied_media']
    mapping = data['brief_mapping']
    errors = []
    guids = []
    for record in records:
        file = local(record['file'])
        meta = file.with_name(file.name + '.meta')
        if not file.is_file() or sha(file) != record['sha256']:
            errors.append('Media thiếu hoặc hash khác: ' + record['file'])
        if not meta.is_file() or sha(meta) != record['meta_sha256']:
            errors.append('Meta thiếu hoặc hash khác: ' + record['file'])
        if meta.is_file():
            found = re.search(r'^guid: ([a-f0-9]{32})$', meta.read_text(), re.M)
            if not found or found.group(1) != record['guid']:
                errors.append('GUID không khớp: ' + record['file'])
    for meta in (ROOT / 'Game/Assets').rglob('*.meta'):
        match = re.search(r'^guid: ([a-f0-9]{32})$', meta.read_text(encoding='utf-8-sig'), re.M)
        if match:
            guids.append(match.group(1))
    errors.extend('GUID trùng: ' + key for key, count in Counter(guids).items() if count > 1)
    known_files = {record['file'] for record in records}
    for item in mapping:
        if item['production_status'] not in LABELS:
            errors.append('Trạng thái chưa nhận diện: ' + item['asset_id'])
        if item['candidate_file'] and item['candidate_file'] not in known_files:
            errors.append('File ứng viên không có trong kiểm kê: ' + item['asset_id'])
        for variant in item.get('variant_files', {}).values():
            if variant not in known_files:
                errors.append('Biến thể chưa kiểm kê: ' + variant)
        if item.get('artifact_file') and not local(item['artifact_file']).is_file():
            errors.append('Thiếu thành phần: ' + item['artifact_file'])
    actual_media = {p.relative_to(ROOT).as_posix() for p in (ROOT / 'Game/Assets/_Civi').rglob('*')
                    if p.is_file() and p.suffix in ('.png', '.wav', '.ttf')}
    if actual_media != known_files:
        errors.append('Kiểm kê media khác cây thực tế')
    if len(known_files) != len(records):
        errors.append('Đường dẫn media bị lặp')
    content = ROOT / 'Game/Assets/_Civi/Content'
    docs = {p.relative_to(content).as_posix(): read_json(p) for p in content.rglob('*.json')}
    briefs = docs['Manifests/asset-manifest.json']['assets']
    for label, ids in [('brief', [x['asset_id'] for x in briefs]),
                       ('TV3', [x['asset_id'] for x in mapping])]:
        if len(ids) != len(set(ids)):
            errors.append(label + ': mã bị lặp')
    if {x['asset_id'] for x in mapping} != {x['asset_id'] for x in briefs}:
        errors.append('Các mã brief TV1 và TV3 không khớp')
    scenes = [v for k, v in docs.items() if k.startswith('Story/v4_1/scenes/')]
    dialogue = [line for k, v in docs.items() if k.startswith('Story/v4_1/dialogue/') for line in v['lines']]
    line_ids = {line['text_id'] for line in dialogue}
    scene_ids = {scene['scene_id'] for scene in scenes}
    asset_ids = {item['asset_id'] for item in briefs}
    choices = []
    for scene in scenes:
        steps = {step['step_id'] for step in scene['steps']}
        if len(steps) != len(scene['steps']):
            errors.append('Mã bước lặp: ' + scene['scene_id'])
        if scene['entry_step'] not in steps:
            errors.append('Thiếu bước mở: ' + scene['scene_id'])
        if not set(scene['asset_ids']) <= asset_ids:
            errors.append('Thiếu mã tài nguyên: ' + scene['scene_id'])
        for step in scene['steps']:
            if not set(step['lines']) <= line_ids:
                errors.append('Thiếu thoại: ' + step['step_id'])
            for choice in step['choices']:
                choices.append(choice)
                if not set(choice['lines']) <= line_ids:
                    errors.append('Thiếu thoại lựa chọn: ' + choice['choice_id'])
                nxt = choice['next']
                if nxt.startswith('step:') and nxt[5:] not in steps:
                    errors.append('Thiếu bước đích: ' + nxt)
                if nxt.startswith('scene:') and nxt[6:] not in scene_ids:
                    errors.append('Thiếu cảnh đích: ' + nxt)
    for label, ids in [('scene', [s['scene_id'] for s in scenes]),
                       ('dialogue', [line['text_id'] for line in dialogue]),
                       ('choice', [choice['choice_id'] for choice in choices])]:
        if len(ids) != len(set(ids)):
            errors.append('Mã bị lặp: ' + label)
    if errors:
        raise SystemExit('\n'.join(errors))
    counts = Counter(Path(record['file']).suffix for record in records)
    candidates = sum(bool(item['candidate_file']) for item in mapping)
    report = {
        'date': data['date'], 'result': 'PASS_FILE_HANDOFF_ONLY',
        'media_files': len(records), 'matching_meta_files': len(records),
        'media_bytes': sum(local(record['file']).stat().st_size for record in records),
        'media_by_extension': dict(sorted(counts.items())),
        'all_media_hashes_match_inventory': True, 'duplicate_guids': 0,
        'brief_ids_covered': len(mapping), 'brief_ids_with_candidate_file': candidates,
        'production_status_counts': dict(Counter(item['production_status'] for item in mapping)),
        'content_json_files': len(docs), 'scenes': len(scenes),
        'dialogue_entries': len(dialogue), 'choices': len(choices), 'reference_errors': [],
        'not_validated': ['Unity import/build', 'AssetCatalog binding', 'new runtime playthrough', 'TV1 acceptance of visual candidates'],
    }
    row_status = '\n'.join(f"| {LABELS[key]} | {value} |" for key, value in report['production_status_counts'].items())
    missing = '\n'.join('- `' + item['asset_id'] + '`: ' + item['next_action']
                        for item in mapping if item['production_status'] in ('missing', 'candidate_needs_revision', 'candidate_needs_variants'))
    proof_path = ROOT / 'artifacts/qa/s00-integration-checks.json'
    proof = read_json(proof_path) if proof_path.exists() else {}
    s00_verified = proof.get('result') == 'PASS' and bool(proof.get('verified_files'))
    if s00_verified:
        s00_verified = all(local(item['file']).is_file() and sha(local(item['file'])) == item['sha256'] for item in proof['verified_files'])
    report['s00_integration_verified'] = s00_verified
    report['s00_scope'] = 'S00 only; S01 and later scenes not integrated'
    if s00_verified:
        report['not_validated'] = ['Full story runtime S01-S10/endings', 'TV1/TV2/TV5 team acceptance', 'Human playtest']
    milestone = 'ĐÃ TÍCH HỢP VÀ KIỂM S00 · chờ nhóm duyệt' if s00_verified else 'CÓ MÃ/BẢN THỬ S00 · cần kiểm lại bằng chứng tích hợp'
    rows = '\n'.join(f"| {LABELS[key]} | {value} |" for key, value in report['production_status_counts'].items())
    unfinished = '\n'.join('- `' + item['asset_id'] + '`: ' + item['next_action'] for item in mapping if item['production_status'] in ('missing', 'candidate_needs_revision', 'candidate_needs_variants'))
    page = f'''# TV3 — BẮT ĐẦU Ở ĐÂY

**{data['date']} · TV3 · {milestone}**

**Mở [01-CHOI-THU-S00.bat](01-CHOI-THU-S00.bat)** hoặc [CIVI-S00.exe](release/S00-TV3-Windows/CIVI-S00.exe). Giữ cả thư mục bản chạy khi gửi bạn; người chơi không cần cài Unity. Đọc hết S00 rồi chọn **Thử ba mẫu trang nháp** để xem phần W3.4 mới. Truyện dừng tại điểm nối S01; bàn nháp chỉ thử UI, chưa gửi nội dung thật. File lưu riêng, không thay tiến trình bản cũ.

## Nhìn cây là biết đã làm tới đâu

```text
CIVI_2/
├── 00-TIEN-DO-TV3.md                        ← tiến độ hiện tại
├── 01-CHOI-THU-S00.bat                      ← mở bản thử
├── Game/Assets/_Civi/
│   ├── Art/Backgrounds/                     ← {sum('/Backgrounds/' in x['file'] for x in records)} nền ứng viên
│   ├── Art/Characters/                      ← {sum('/Characters/' in x['file'] for x in records)} atlas; có portrait giữ suốt câu
│   ├── Art/Photos/                          ← {sum('/Photos/' in x['file'] for x in records)} ảnh, gồm 3 cặp callback
│   ├── Art/Props/                           ← 4 prefab: sơ đồ, kẹp, pin, HẸN
│   ├── Art/UI/README-TIEN-DO.md              ← UI hiện dựng bằng component, chưa có atlas nút riêng
│   ├── Audio/                              ← 6 WAV; đã gắn hiệu ứng S00/animatic
│   ├── Fonts/                              ← 3 TTF + giấy phép đi kèm
│   ├── Prefabs/UI/                          ← 6 prefab: thoại, lựa chọn, 3 mẫu, DraftReview
│   ├── Config/TV3-AssetCatalog.asset         ← tham chiếu dữ liệu, ảnh, âm, prefab
│   ├── Scenes/S00-TV3-Preview.unity          ← scene tích hợp S00
│   ├── Scripts/                            ← S00Session + bộ trình bày tách riêng
│   ├── Scripts/Presentation/Animatic/       ← VyAnimatic: 32s, mốc chính 09–18
│   ├── Scripts/Presentation/Drafting/       ← ba mẫu nháp và DraftReview
│   ├── Video/README-TIEN-DO.md               ← animatic đã chạy; chưa xuất video phim
│   └── Content/                            ← nguyên dữ liệu TV1
├── release/S00-TV3-Windows/                 ← bản Windows một cảnh
├── docs/production/tv3-task-status.md        ← 8 nhiệm vụ riêng của thành viên 3
├── docs/qa/TV3-NHAN-XET-S00.md               ← chờ nhóm/người chơi nhận xét
├── docs/engineering/tv3-s00-integration.md   ← TV2 nhận mã và nối tiếp
└── artifacts/qa/                           ← kiểm model/build/UI + ảnh kiểm
```

## Bốn bước đã yêu cầu

| Bước | Đã làm được | Còn phải xác nhận |
| --- | --- | --- |
| 1 · Ảnh S00 và gesture | BAD_01 đúng đèn tắt, BAD_02 đủ bốn; ba biến thể pose_1/2/3 ở hai thời điểm; 0442 đã sửa Nhi nhìn An | TV1 duyệt hình; 0441 đã rà, chờ duyệt continuity với 0442 |
| 2 · UI, đạo cụ, animatic | Sáu prefab UI, bốn prefab đạo cụ, portrait giữ suốt câu; animatic 32s; ba mẫu nháp/người nhận/xác nhận/lỗi đã thử 9 tổ hợp ở mỗi độ phân giải | Chưa có diễn xuất toàn thân hoặc giọng thu; UI nháp chưa nối vào S05; các đạo cụ còn lại và UI cảnh sau còn thiếu |
| 3 · Tích hợp S00 | AssetCatalog + scene + bản Windows; {'27 tổ hợp, 135 roundtrip dấu trang và UI 720p/1080p đạt' if s00_verified else 'cần kiểm lại báo cáo của bản hiện tại'} | TV2 nhận mã; S01–S10 và các kết chưa tích hợp cây mới |
| 4 · Nhóm kiểm nhận | Đã có [phiếu thử](docs/qa/TV3-NHAN-XET-S00.md), ID câu/cảnh để ghi lỗi và bằng chứng kỹ thuật | Chưa có xác nhận TV1/TV5 hoặc nhận xét người chơi thật; chưa chốt demo toàn game |

**Kiểm kê:** {len(records)} media + {len(records)} .meta ({counts['.png']} PNG, {counts['.wav']} WAV, {counts['.ttf']} TTF); đủ {len(mapping)} mã brief được đối chiếu. Có {candidates} mã có file ảnh ứng viên; không quy con số này thành phần trăm hoàn thành.

| Trạng thái mã brief | Số mã |
| --- | ---: |
{rows}

## Làm gì tiếp?

1. Đọc [tám nhiệm vụ TV3](docs/production/tv3-task-status.md). Nhóm chơi bản S00 và bàn thử ba mẫu, rồi điền [phiếu nhận xét](docs/qa/TV3-NHAN-XET-S00.md); ưu tiên câu khó hiểu, gesture chưa rõ, hình lệch ý và nhịp đọc.
2. TV2 nhận [mã tích hợp](docs/engineering/tv3-s00-integration.md), chốt contract rồi nối S01 → S03 và UI nháp vào S05. Không dùng S00Session làm bộ chạy toàn truyện mà bỏ kiểm quyền/điều kiện.
3. TV3 sửa theo nhận xét, bổ sung đạo cụ/mẫu nháp và nền thiếu. Chỉ gói demo toàn truyện sau khi các cảnh nối được và đã thử nhánh.

### Ảnh/nền còn thiếu hoặc cần chỉnh

{unfinished}

## Bằng chứng và cách cập nhật

- [Kiểm tích hợp S00](artifacts/qa/s00-integration-checks.json): phạm vi, hash nguồn/bản chạy, kết quả và phần chưa được nhóm duyệt.
- [Kiểm model](artifacts/qa/s00-model-checks.json), [build](artifacts/qa/s00-build-checks.json), [UI 720p](artifacts/qa/S00-1280x720/runtime-checks.json), [UI 1080p](artifacts/qa/S00-1920x1080/runtime-checks.json).
- [Ảnh UI](artifacts/qa/S00-1280x720/02-choices.png), [cặp callback](artifacts/qa/S00-1280x720/05-callback.png), [phụ đề](artifacts/qa/S00-1280x720/06-animatic-full-quote.png).
- [Kiểm UI nháp](artifacts/qa/draft-ui-integration-checks.json), [ảnh tribute](artifacts/qa/S00-1280x720/07-draft-tribute.png), [paired](artifacts/qa/S00-1280x720/07-draft-paired.png), [memory](artifacts/qa/S00-1280x720/07-draft-memory.png).
- [Bảng tài nguyên](docs/production/tv3-asset-map.json), [bàn giao từng mã](docs/production/TV3-BAN-GIAO.md), [prompt ảnh mới](source-assets/TV3/imagegen-production.json), [nguồn/giấy phép](licenses/TV3-ASSETS.md).
- Chạy `python tools/QA/update_tv3_progress.py --check`. Sau khi sửa asset/mã cần cập nhật map và kiểm lại; hash khác sẽ không tiếp tục ghi S00 đã xác minh. Không đổi tên thư mục/GUID để biểu diễn phần trăm tiến độ.

Chưa push cây này lên GitHub. “Đã kiểm kỹ thuật” không thay “nhóm đã duyệt” hoặc “người chơi đã thử”.
'''

    guide = ROOT / 'docs/production/TV3-BAN-GIAO.md'
    text = guide.read_text(encoding='utf-8')
    start = text.index('## Đối chiếu brief')
    end = text.index('## Kiểm tra đã thực hiện')
    rows = '\n'.join(f"| `{item['asset_id']}` | {LABELS[item['production_status']]} | {item['next_action']} |" for item in mapping)
    section = f'''## Đối chiếu brief

Có {candidates}/{len(mapping)} mã có file ứng viên; đây không phải tỷ lệ hoàn thành. Nhiều mã cần prefab, hội thoại hoặc trang dựng động; mọi ứng viên vẫn chờ nhóm duyệt; chỉ các thành phần S00 đã được tích hợp ở mốc hiện tại.

| Mã | Tình trạng | Điều cần biết / việc tiếp |
| --- | --- | --- |
{rows}

'''
    text = text[:start] + section + text[end:]
    outputs = {
        ROOT / '00-TIEN-DO-TV3.md': page,
        ROOT / 'artifacts/qa/tv3-handoff-checks.json': json.dumps(report, ensure_ascii=False, indent=2) + '\n',
        guide: text,
    }
    stale = []
    for path, content in outputs.items():
        current = path.read_text(encoding='utf-8') if path.exists() else None
        if args.check:
            if current != content:
                stale.append(path.relative_to(ROOT).as_posix())
        else:
            path.write_text(content, encoding='utf-8', newline='\n')
    if stale:
        raise SystemExit('Báo cáo chưa cập nhật: ' + ', '.join(stale))
    print(f"PASS: {len(records)} media/.meta, {len(mapping)} brief IDs, {len(scenes)} scenes, {len(dialogue)} dialogue entries, {len(choices)} choices. S00 proof is checked separately; no full-story or team-approval claim.")

if __name__ == '__main__':
    main()
