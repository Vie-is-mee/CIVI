"""Apply the reviewed, narrow editorial changes and add explicit reading routes."""
from pathlib import Path
import hashlib
import json
import shutil
from dialogue_content import ROOT, DIALOGUE, parse

NODES = {f"C{i}": f"Chuong_{i}_Ban_Duyet.md" for i in range(1, 11)}
NODES.update({"I_A":"Nhanh_I-A_Ban_Duyet.md", "I_B":"Nhanh_I-B_Ban_Duyet.md",
    "II_A":"Nhanh_II-A_Ban_Duyet.md", "II_B":"Nhanh_II-B_Ban_Duyet.md",
    "END_A":"Ending_A_Ban_Duyet.md", "END_B":"Ending_B_Ban_Duyet.md", "C9C":"Ending_C_Ban_Duyet.md"})
ROUTES = {**{f"C{i}":[f"C{i+1}"] for i in range(1,9)},
    "C5":["I_A","I_B"], "I_A":["C6"], "I_B":["C6"],
    "C7":["II_A","II_B"], "II_A":["C8"], "II_B":["C8"],
    "C9":["END_A","END_B","C9C"], "C9C":["C10"], "C10":[], "END_A":[], "END_B":[]}


def apply_review():
    backup = ROOT / "artifacts/dialogue-review/original"
    backup.mkdir(parents=True, exist_ok=True)
    changes = []
    for node, filename in NODES.items():
        path = DIALOGUE / filename
        if not (backup / filename).exists(): shutil.copy2(path, backup / filename)
        old = path.read_text(encoding="utf-8-sig")
        text = old
        if node == "C1":
            edits = [
                ("Trước cuộc họp buổi sáng, Mai còn phải giải quyết", "Sáng hôm ấy, trước cuộc họp chiều, Mai còn phải giải quyết"),
                ("Tôi hiểu. Tôi sẽ kiểm tra phương án khác. Trong lúc đó, xin cô đừng tự ý chuyển các em sang khu vực chưa được xác nhận an toàn.",
                 "Tôi hiểu. Tôi sẽ kiểm tra phương án khác. Nếu cần ghép lớp trong lúc chờ, nhờ cô xác nhận phòng thay thế đủ an toàn cho các em nhé."),
                ("Tôi gọi để cảm ơn chị. Chuyến hàng đã được đổi lịch. Tôi sẽ đưa thuốc đến bệnh viện đúng giờ.",
                 "Tôi gọi để cảm ơn chị đã báo lịch. Tôi đã đổi giờ nghỉ để đưa chuyến thuốc đến bệnh viện. Hàng tới nơi rồi, chị nhé."),
                ("**Người hướng dẫn:**", "**Người hướng dẫn thực tập:**"),
                ("**Cậu học sinh:**", "**Học sinh:**"),
            ]
            for before, after in edits:
                if before in text:
                    text = text.replace(before, after)
                    changes.append(dict(file=filename, before=before, after=after))
                elif after not in text: raise AssertionError(f"Review anchor changed in {filename}: {before}")
            # The album is a distinct interaction in the current game, not an unmarked scene jump.
            album = "*[Tối hôm ấy, Mai lấy cuốn album ra."
            if "## Cảnh 12 — Một hồ sơ nhỏ" not in text:
                assert text.count(album) == 1
                text = text.replace(album, "## Cảnh 11 — Phần ký ức còn lại\n\n" + album)
                text = text.replace("## Cảnh 11 — Một hồ sơ nhỏ", "## Cảnh 12 — Một hồ sơ nhỏ")
                changes.append(dict(file=filename, after="Tách cảnh album thành Cảnh 11; hồ sơ cuối ngày thành Cảnh 12."))
        if node == "C6":
            before = "sau đó phải công khai số liệu về những người chịu rủi ro để Hội đồng"
            after = "sau đó phải công khai số liệu tổng hợp về các nhóm chịu rủi ro, không công bố danh tính hay hồ sơ cá nhân, để Hội đồng"
            if before in text:
                text = text.replace(before, after)
                changes.append(dict(file=filename,before=before,after=after))
            elif after not in text: raise AssertionError("Chapter 6 aggregate-data review anchor changed")
        if "<!-- civi-route:" not in text:
            destinations = ROUTES[node]
            links = " · ".join(f"[{n}]({NODES[n]})" for n in destinations)
            note = "Đọc tiếp: " + links if links else "Kết thúc tuyến này. Không tự chuyển sang tuyến kết thúc khác."
            if node == "C10": note = "Chỉ đọc sau [tuyến C](Ending_C_Ban_Duyet.md). Không tiếp nối tuyến A/B. Hết truyện tuyến C."
            metadata = json.dumps(dict(node=node, next=destinations), ensure_ascii=False, separators=(",",":"))
            title, rest = text.split("\n",1)
            text = title + f"\n\n<!-- civi-route: {metadata} -->\n> **Luồng đọc:** {note}\n" + rest
            changes.append(dict(file=filename, after="Bổ sung ID tuyến và liên kết đọc tiếp tách khỏi lời thoại."))
        import re
        first_scene = re.search(r"(?m)^#{2,3} Cảnh ",text)
        if first_scene:
            header=text[:first_scene.start()]
            quoted=re.sub(r"(?m)^\*\*(Vị trí|Lựa chọn|Cấu trúc):\*\*",r"> **\1:**",header)
            if quoted != header:
                text=quoted+text[first_scene.start():]
                changes.append(dict(file=filename,after="Đưa ghi chú vị trí/lựa chọn/cấu trúc vào blockquote, tránh nhầm người nói."))
        path.write_text(text, encoding="utf-8")
    import difflib
    audit=[]; patch=[]
    for filename in NODES.values():
        original=(backup/filename).read_text(encoding="utf-8-sig")
        revised=(DIALOGUE/filename).read_text(encoding="utf-8-sig")
        audit.append(dict(file=filename,before=hashlib.sha256((backup/filename).read_bytes()).hexdigest(),after=hashlib.sha256((DIALOGUE/filename).read_bytes()).hexdigest()))
        patch.extend(difflib.unified_diff(original.splitlines(True),revised.splitlines(True),fromfile="original/"+filename,tofile="dialogue/"+filename))
    (backup.parent/"changes.json").write_text(json.dumps(audit,ensure_ascii=False,indent=2),encoding="utf-8")
    (backup.parent/"review.patch").write_text("".join(patch),encoding="utf-8")
    return changes


def validate():
    import re
    result = []
    for node, filename in NODES.items():
        path = DIALOGUE/filename
        scenes, cues, errors = parse(path)
        route_tag=re.search(r"<!-- civi-route:\s*(\{.+\})\s*-->",path.read_text(encoding="utf-8"))
        assert route_tag, (filename,"Missing route metadata")
        route=json.loads(route_tag[1])
        assert route['node']==node and route['next']==ROUTES[node], (filename,"Route metadata conflicts with reading order")
        assert not errors, (filename, errors)
        assert [s[0] for s in scenes] == list(range(1,len(scenes)+1)), filename
        assert cues and all(c.scene > 0 for c in cues), filename
        for target in re.findall(r"\]\(([^)]+\.md)\)", path.read_text(encoding="utf-8")):
            assert (DIALOGUE/target).is_file(), (filename,target)
        result.append(dict(node=node,file=filename,scenes=len(scenes),cues=len(cues),
            speech=sum(c.kind=="speech" for c in cues),sha256=hashlib.sha256(path.read_bytes()).hexdigest(),next=ROUTES[node]))
    routes = []
    for first in ["I_A","I_B"]:
        for second in ["II_A","II_B"]:
            for ending in ["END_A","END_B","C9C"]:
                node="C1"; seen=[]
                while node:
                    assert node not in seen
                    seen.append(node)
                    choices=ROUTES[node]
                    node = (first if node=="C5" else second if node=="C7" else ending if node=="C9" else choices[0]) if choices else None
                assert ("C10" in seen) == (ending=="C9C")
                assert len(set(seen)&{"END_A","END_B","C9C"})==1
                routes.append(seen)
    return dict(files=result,routes=routes)


if __name__ == "__main__":
    import sys
    sys.stdout.reconfigure(encoding="utf-8")
    if "--apply" in sys.argv: print("Applied",len(apply_review()),"review changes; originals backed up")
    report=validate()
    output=ROOT/"docs/qa/dialogue-catalog.json"
    output.write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding="utf-8")
    print("PASS",len(report['files']),"files /",sum(f['scenes'] for f in report['files']),"scenes /",len(report['routes']),"routes")
