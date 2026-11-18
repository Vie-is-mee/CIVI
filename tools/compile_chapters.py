"""Compiles the reviewed dialogue of chapters 2 and 3 into Resources/ChapterN/chapter.json.
Every cue of the Markdown is kept, in order; this file only decides where each scene happens and who stands there."""
import hashlib
import json
from pathlib import Path
from compile_chapter_dialogue import pages
from dialogue_content import DIALOGUE, ROOT, parse

OUT = ROOT / "Game/Assets/_Civi/Resources"
EDGE = "Rìa Mưa · Phân khu 8-12"
KITCHEN = "Rìa Mưa · Nhà An"
HALL = "Vành Đệm · Sảnh tiếp dân"
OFFICE = "Cơ quan Điều phối · Tầng 42"

CH2 = [
    dict(scene=1, objective="Đi theo lối gạch vào Rìa Mưa, tìm đường đến trạm lọc số mười chín.", place="edge", start="M_E_start", mark="M_E_stall", location=EDGE,
         cast=["an@M_E_an_stall", "ong_lao@M_E_lao"]),
    dict(scene=2, objective="Đi cùng An đến con hẻm có dấu đo nước.", place="edge", mark="M_E_mark", location=EDGE, cast=["an@M_E_an_mark"],
         split="Những trang đầu là hình vẽ ô", summary="Cuốn sổ đóng bằng giấy bao hàng của An. Xem từng phần.", commit="Lật tiếp cuốn sổ",
         items=["Trang đầu: ô và nan|Hình vẽ ô, nan và các lỗi nhỏ như vòng khóa trượt, khớp bị gãy.",
                "Sơ đồ đường ống|Đường ống ngoằn ngoèo và vị trí nước tụ, đánh dấu bằng nhiều màu mực.",
                "Lịch mất điện|Các ngày mất điện, ghi cạnh tên từng người trong khu."]),
    dict(scene=3, objective="Qua cầu, vào khu chợ mái bạt cùng An.", place="edge", mark="M_E_market", location=EDGE,
         cast=["tin@M_E_tin", "an@M_E_an_market", "ba_chao@M_E_chao", "ong_lao@M_E_lao_chao"]),
    dict(scene=4, objective="Đến trạm lọc số mười chín, gặp quản đốc ca đêm.", place="edge", mark="M_E_station", location=EDGE, cast=["an@M_E_an_station", "quan_doc@M_E_quandoc"],
         split="Sổ ghi bảy lần vá van", summary="Sổ giấy của trạm, vì mạng ở vùng này thường xuyên mất kết nối.", commit="Trả lại sổ",
         items=["Bảy lần vá van|Từng lần được ghi tay, kèm ngày và ca trực.", "Ba lần thay ống|Vật tư ghi theo đơn vị thầu.",
                "Hai đợt trực ba mươi giờ|Phần lớn thợ chỉ ghi dưới mã Công ty Hợp tác Hạ tầng Tây Bắc."]),
    dict(scene=5, objective="Đi cùng An đến căn nhà trong bức ảnh của Linh.", place="edge", mark="M_E_house", location=EDGE, cast=["an@M_E_an_house"]),
    dict(scene=6, objective="Bà Dung ngã trong nhà. Chạy tới cuối hẻm.", place="edge", mark="M_E_dung", location=EDGE, cast=["an@M_E_an_dung", "tin@M_E_tin_dung", "dung@M_E_ba_dung"],
         split="Mai kiểm tra vết rò", summary="Vết rò trên đường ống phía trên giường bà Dung.", commit="Ghi mã đường ống",
         items=["Tuyến phân phối chung|Đường ống thuộc hệ thống của thành phố.", "Công trình tự phát|Căn nhà lại bị phân loại ngoài hệ thống. Hai phòng dùng hai loại dữ liệu mâu thuẫn."]),
    dict(scene=7, objective="Tối đến. Về nhà An chờ Hạ.", place="edge", mood="edge_night", mark="M_E_kitchen", location=KITCHEN, cast=["ha@M_E_ha_door", "an@M_E_an_kitchen"]),
    dict(scene=8, auto=True, place="edge", mood="edge_night", location=KITCHEN, cast=["ha@M_E_ha", "an@M_E_an_kitchen"], start="M_E_kitchen",
         split="Mai lần lượt chụp từng tài liệu", summary="Hộp thiếc của Hạ. Chụp từng tài liệu trước mặt bà.", commit="Đã chụp đủ",
         items=["Đơn chuyển cư trú|Gấp ngay ngắn, bọc trong túi nhựa mỏng.", "Biên bản kiểm tra hỏng máy|Có chữ ký của trạm lọc.",
                "Thẻ vào cổng bảo trì|Qua từng đợt, từng năm.", "Giấy khen nhòe mực nước|Vẫn còn đọc được tên Hạ."]),
    dict(scene=9, auto=True, place="edge", mood="edge_night", location=KITCHEN, cast=["ha@M_E_ha", "an@M_E_an_kitchen"]),
    dict(scene=10, objective="An tiễn Mai tới chân cầu.", place="edge", mood="edge_night", mark="M_E_bridge", location=EDGE, cast=["an@M_E_an_bridge"]),
    dict(scene=11, objective="Ghé trạm kiểm toán lưu động trước cổng kiểm soát.", place="edge", mood="edge_night", mark="M_E_audit", location=EDGE,
         split="Đơn vị thầu nhận đủ điểm", summary="Hóa đơn các công trình Hạ đã tham gia.", commit="Lưu đối chiếu",
         items=["Đơn vị thầu|Nhận đủ điểm đóng góp và khoản thưởng khẩn cấp.", "Mã lao động của Hạ|Chỉ nằm trong phụ lục giấy, chưa từng được nhập vào hệ thống."]),
]

CH3 = [
    dict(scene=1, objective="Đối chiếu ba điều kiện của hồ sơ Hạ cùng Linh.", place="floor", start="M_SPAWN_start", mark="M_NPC_linh_office", location=OFFICE, cast=["linh@M_NPC_linh_office"],
         split="Từng điều kiện riêng lẻ", summary="Ba điều kiện, mỗi điều kiện đều cần điều kiện kế tiếp.", commit="Ghi lại vòng tròn",
         items=["Ghi nhận đóng góp|Cần mã lao động đã xác minh.", "Mã lao động|Cần mã cư trú ổn định.", "Mã cư trú ổn định|Cần đủ điểm đóng góp tối thiểu trong ba năm gần nhất."]),
    dict(scene=2, objective="Đến sảnh tiếp dân như một người nộp đơn bình thường. Tới quầy hướng dẫn.", place="hall", start="M_H_start", mark="M_H_guide", location=HALL,
         cast=["truc@M_H_staff", "dan_ong@M_H_man", "ba_chao@M_H_old"]),
    dict(scene=3, objective="Trên tàu về cơ quan. Lại gần người đàn ông bế con.", place="train", start="M_T_start", mark="M_T_man", location="Tàu về Lõi Sáng", cast=["dan_ong@M_T_man"]),
    dict(scene=4, objective="Mang hồ sơ của Hạ tới cửa sổ đặc cách.", place="hall", start="M_H_start", mark="M_H_window", location=HALL, cast=["dong_nghiep_a@M_H_officer"]),
    dict(scene=5, auto=True, hide=True, cam="M_C_cam", place="class", location="Lớp cộng đồng · Tầng hầm nhà thờ cũ", cast=["nhi@M_C_nhi", "an@M_C_an", "tin@M_C_tin", "hoc_sinh@M_C_hs"]),
    dict(scene=6, auto=True, hide=True, cam="M_C_cam", place="class", location="Lớp cộng đồng · Tầng hầm nhà thờ cũ", cast=["nhi@M_C_nhi", "an@M_C_an", "tin@M_C_tin", "hoc_sinh@M_C_hs"]),
    dict(scene=7, auto=True, hide=True, cam="M_E_kitchen_cam", place="edge", mood="edge_night", location=KITCHEN, cast=["ha@M_E_ha", "an@M_E_an_kitchen"]),
    dict(scene=8, auto=True, hide=True, cam="M_H_cam", place="hall", location=HALL, cast=["ha@M_H_ha", "an@M_H_an", "truc@M_H_staff2"]),
    dict(scene=9, objective="Đến nhà An. Em đang đợi trước cửa.", place="edge", start="M_E_bridge", mark="M_E_step", location=EDGE, cast=["an@M_E_an_step"]),
    dict(scene=10, objective="Tối. Về bàn kiểm toán, mở báo cáo dự báo bão.", place="floor", mood="night", start="M_SPAWN_start", mark="M_OBJ_audit", location=OFFICE,
         split="Mai gửi yêu cầu dừng tự động", summary="Kế hoạch tự động chuyển các phân khu về chế độ duy trì tối thiểu.", commit="Gửi yêu cầu dừng phê duyệt",
         items=["Phân khu 8-12|Nằm trong nhóm đầu tiên bị giảm điện.", "Số người chịu tác động|Hệ thống vẫn ghi: 0."]),
]


def compile_chapter(number, title, source, plan):
    path = DIALOGUE / source
    scenes, cues, errors = parse(path)
    assert not errors, errors
    assert [p["scene"] for p in plan] == [n for n, _, _ in scenes], "every scene needs a step"
    titles = {n: t for n, t, _ in scenes}
    records, expanded = [], {}
    count = {}
    for index, cue in enumerate(cues):
        count[cue.scene] = count.get(cue.scene, 0) + 1
        cue_id = f"C{number}.S{cue.scene:02}.{count[cue.scene]:03}"
        expanded[index] = []
        for page, text in enumerate(pages(cue.text), 1):
            expanded[index].append(len(records))
            records.append(dict(id=f"{cue_id}.p{page}", speaker=cue.speaker, kind=cue.kind, text=text, scene=cue.scene, title=titles[cue.scene], line=cue.line))
    steps = []
    for p in plan:
        indices = [i for i, c in enumerate(cues) if c.scene == p["scene"]]
        cut = len(indices)
        if "split" in p:
            found = [k for k, i in enumerate(indices) if p["split"] in cues[i].text]
            assert len(found) == 1, f"anchor changed: {p['split']}"
            cut = found[0]
        before = [n for i in indices[:cut] for n in expanded[i]]
        after = [n for i in indices[cut:] for n in expanded[i]]
        items = p.get("items", [])
        assert before, "a step needs something to read before its action"
        steps.append(dict(title=titles[p["scene"]], objective=p.get("objective", ""), location=p["location"], speaker="Mai", portrait=0, action=f"inspect{len(items)}" if items else "",
                          x=0, y=0, before=before, after=after, place=p["place"], mark=p.get("mark", ""), start=p.get("start", ""), cam=p.get("cam", ""), mood=p.get("mood", ""),
                          cast=p.get("cast", []), auto=p.get("auto", False), hide=p.get("hide", False), items=items, summary=p.get("summary", ""), commit=p.get("commit", "Xác nhận")))
    payload = json.dumps(records, ensure_ascii=False, separators=(",", ":"))
    data = dict(title="NHỮNG NGƯỜI KHÔNG CÓ CHỖ DƯỚI BẦU TRỜI", chapter=title, number=number, sourceHash=hashlib.sha256(payload.encode()).hexdigest(),
                canonHash="", dialogueSourceHash=hashlib.sha256(path.read_bytes()).hexdigest(), dialogueSource=source, sourceCueCount=len(cues), sourceSceneCount=len(scenes),
                canonParagraphs=[], legacyIntroCount=0, legacyBeforeCounts=[], legacyAfterCounts=[], paragraphs=[r["text"] for r in records], speakers=[r["speaker"] for r in records],
                portraits=[0 if r["speaker"] == "Mai" else 1 if r["speaker"] == "Linh" else -1 for r in records], kinds=[r["kind"] for r in records], cueIds=[r["id"] for r in records],
                sceneIds=[r["scene"] for r in records], sceneTitles=[r["title"] for r in records], sourceLines=[r["line"] for r in records], intro=[], steps=steps)
    order = [n for s in steps for n in s["before"] + s["after"]]
    assert order == list(range(len(records))), "Dialogue omitted, duplicated or reordered"
    folder = OUT / f"Chapter{number}"
    folder.mkdir(exist_ok=True)
    (folder / "chapter.json").write_text(json.dumps(data, ensure_ascii=False, indent=1), encoding="utf-8")
    print(f"Chapter {number}: {len(cues)} cues, {len(records)} pages, {len(steps)} steps")


if __name__ == "__main__":
    compile_chapter(2, "Chương 2 · Những chiếc ô hỏng", "Chuong_2_Ban_Duyet.md", CH2)
    compile_chapter(3, "Chương 3 · Người có điểm bằng không", "Chuong_3_Ban_Duyet.md", CH3)
