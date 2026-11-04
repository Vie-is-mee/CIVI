# CIVI — Chương 1: Tích hợp chọn lọc từ CIVI_5 (04/11/2026)

## Nguồn và phạm vi
- Integration target: phiên bản Lưu Quang trên branch `feat/chapter1-selective-integration-20261104`, xuất phát từ commit `9bb270c` (baseline PR #2).
- Source: [CIVI_5.zip của đồng đội](https://drive.google.com/file/d/1yJsjHukS72O4-GwZuAmGkxkdTEOCJ0Ro/view), bộ thoại `dialogue/Chuong_1_Ban_Duyet.md`, atlas `walk-v2.png`.
- Canon: [Những người không có chỗ dưới bầu trời](https://drive.google.com/file/d/1Q7kzTn73t3rwkCjAXxl2AXTU7s1ZpvQf/view), chỉ **Chương 1 — Thành phố công bằng**.
- Đây là baseline nâng cấp trước khi nhóm tác giả phê duyệt cuối cùng bản hội thoại; không chỉnh sửa Chương 2–10.

## Keep / Adapt / Integrate
| Thành phần | Quyết định | Thực hiện |
|---|---|---|
| 13 nhiệm vụ, hành động và điều kiện | KEEP | `CiviLiuChapter`, `LiuSave`, `chapter.json` giữ tiến trình, chỉ mở rộng mode đọc thoại. |
| Movement, tốc độ, camera và collision | KEEP | `CiviSchoolStudioV2` tiếp tục xử lý movement; không chuyển controller từ ZIP. |
| Dialogue authored | INTEGRATE | `source-assets/chapter1-dialogue/Chuong_1_Ban_Duyet.md` -> `tools/compile_liuquang_dialogue.py` -> `Resources/LiuQuangC1/dialogue.json`. |
| 12 cảnh / 148 cue / 154 trang | ADAPT | `CiviLiuDialogue.cs` ánh xạ sang 13 đoạn `before`/`after` không thiếu hoặc đảo trang; 5 điểm ngắt bảo vệ nhân quả. |
| Dialogue UI, speaker và nhật ký | EXTEND | Portrait Mai/Linh/Vinh/mẹ Mai, không gán sai vai khác; nhật ký phím J chỉ hiển thị trang đã đọc. |
| Atlas Mai 24 frame | INTEGRATE | `walk-v2.png`: 4 hướng x 6 tư thế tại 9 fps; giữ idle và collision cũ, fallback art cũ cho scene không dùng atlas. |
| Art background/NPC, menu, save manager của ZIP | DEFER | Không lấy nguyên bản Unity 6000.4.0f1 để tránh hạ chất lượng hoặc phá state/camera đang hoạt động. |

## Build và QA thực thi
- **Python compiler check:** PASS, 148 cues, 154 pages, 12 scenes, 13 gated tasks; `python tools/compile_liuquang_dialogue.py --check`.
- **Unity 6000.6.6f1 EditMode:** PASS **629 checks**, gồm 13 nhiệm vụ, toàn bộ subset điều kiện và toàn bộ trang thoại, save validation, walking reachability.
- **Unity Windows build:** PASS, log `artifacts/chapter1-integration-2026-11-04/unity-build.log`.
- **Windows Player 720p:** PASS end-to-end bằng Win32 keyboard/input, `qa-720p-third/input-trace.json`.
- **Windows Player 1080p:** PASS end-to-end và journal open/close, `qa-1080p-retry/input-trace.json`.
- Mỗi run đi qua 13 nhiệm vụ, 154 trang (153 trong vòng lặp, 1 tiêu thụ riêng trong bài test bookmark save/load), block submit sớm, lock movement, collision, save/relaunch ra màn kết thúc; không phát hiện text overflow trong ảnh QA.
- **School V2 legacy regression:** PASS **10 checks** ở 2 scene cũ; log `legacy-regression.log`.
- **StoryRuntimeTests:** PASS **69 assertions**.
- **Hạn chế:** Mouse injection từ remote Windows không ổn định, đã ghi sự kiện `mouse-*-not-received`; test dùng keyboard fallback và không công bố mouse PASS. Chưa manual playtest bằng bàn phím vật lý của người chơi hoặc kiểm tương tác gamepad. Chưa đánh giá thẩm mỹ sâu trên màn hình thiết bị thực.

## Bàn giao để chơi thử
- Double-click `PLAY_CIVI_LIUQUANG_CHAPTER1.bat` tại root; trỏ tới `release/LiuQuangChapter1/CIVI_LiuQuang_C1.exe`.
- Bản **trước** khi tích hợp được giữ ở `release/LiuQuangChapter1-before-dialogue-20261104`.
- Build tích hợp và screenshot QA nằm trong project cục bộ, được `.gitignore`; không commit Unity Library, binaries hoặc ảnh QA.
- Không tự merge vào `main` hoặc cập nhật PR #2; tích hợp đi trên branch riêng.

## Rủi ro còn lại
1. Bộ thoại do đồng đội chuyển thể cần tác giả chính duyệt về giọng nhân vật và cảm xúc trước khi công bố.
2. Thử toàn bộ input bằng người chơi; QA remote không chứng minh độ tin cậy chuột trong tình huống thực.
3. Mở rộng biểu cảm NPC/background chỉ khi có bản demo kiểm chứng tốt hơn; hiện tại giữ visual baseline.
