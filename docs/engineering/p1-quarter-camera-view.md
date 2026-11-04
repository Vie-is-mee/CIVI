# CIVI / P1 — 2D top-down 3/4 camera & depth (27/10/2026)

## Mục tiêu và kết quả

Nâng bản P0 (S02–S03) từ phòng hình khối + camera đứng yên thành prototype **orthographic top-down 3/4**. Scene `S02-2D-VerticalSlice.unity` **không cần tái tạo**; các thành phần mới được dựng khi Play qua `CiviExploration2D`. Không sửa story JSON, save format, S00/S01, consent, AI hay Build Settings.

**Mới trong P1**:
- Camera `CiviFollowCamera2D` bám theo An có easing, look-ahead theo hướng di chuyển, zoom bằng con lăn chuột, giới hạn theo biên level, viewport thay đổi tỷ lệ vẫn được clamp.
- Hai phối cảnh phòng **khác nhau**: S02 góc kiểm đồ và S03 bàn điều tra ảnh. Dựng sàn, tường, cửa sổ, bàn mặt trên/mặt trước, lớp bóng và các đồ trang trí bằng SpriteRenderer placeholder.
- `CiviDepthSort2D` + `SortingGroup`: nhân vật/đồ vật/bàn có thứ tự vẽ dựa trên tọa độ chân Y, hỗ trợ hiệu ứng đứng trước/sau đồ nội thất.
- `CiviActorVisual2D`: placeholder An có đầu/tóc/áo/giày, mặt trước/sau, nhìn trái/phải và bước chân nảy nhẹ; đây **không phải** sprite/animation 4 hướng production.
- UI hiển thị tiến độ manh mối bắt buộc của S02/S03, gợi ý trạng thái điểm tương tác; không cho Esc vô tình đóng đoạn thoại đang đọc.

## Chạy thử

1. Trong Unity Hub mở thư mục `Game` bằng editor tương ứng version hiện tại của project (đã được kiểm bằng Unity **6000.6.6f1**). **Không mở hai editor trên cùng một project**.
2. Chờ Unity biên dịch và xem Console.
3. Mở **`CIVI > TV2 > Mở scene khám phá 2D`**, hoặc double-click `Assets/_Civi/Scenes/S02-2D-VerticalSlice.unity`.
4. Nhấn Play, đọc mở đầu bằng Enter/Space; WASD/mũi tên để đi, E tương tác, cuộn chuột để zoom, B mở bảng ảnh tại S03, F5/F9 lưu/tải.
5. Chơi trọn S02 (kiểm kẹp/lịch/chú thích) → lối ra → S03 (ảnh 0441, 0442, 0443, kẹp trống) → nguồn video → dừng trước S04. Chơi từ S00/S01 vẫn chuyển được vào scene mới bằng nút `KHÁM PHÁ 2D / S02`.

## Source

- `Game/Assets/_Civi/Scripts/Application/Exploration/CiviQuarterView.cs`: camera, depth sort, avatar và builder phòng 3/4 tạm thời.
- `Game/Assets/_Civi/Scripts/Application/Exploration/CiviExploration2D.cs`: tích hợp P1, giữ P0 story/interaction/save.
- `Game/Assets/_Civi/Editor/Exploration/CiviExplorationSceneBuilder.cs`: kiểm tra smoke camera/avatar/stages/depth.
- `artifacts/backup-p1-quarterview`: mã P0 trước thay đổi.

## Validation

Biên dịch + smoke check trên **bản sao riêng** project bằng Unity 6000.6.6f1: pass (1 player, 12 interactables, camera follow đúng transform, 2 stage, depth sorting, story JSON S02). Đây là smoke chạy EditMode với bootstrap mô phỏng, **chưa phải QA chuyển động Physics trong Unity Play Mode**. Không cần thay đổi cảnh Unity và không cần import package Cinemachine hoặc download art tại P1.

## Production art / next gates

Với camera 3/4, TV3 cần bộ An/NPC idle/walk trước–sau–trái–phải, tileset chung chuẩn phối cảnh, ánh sáng/foreground có pixel hoặc illustrated style nhất quán; hiện chỉ dùng primitive sprite placeholders. Để production-ready còn cần kiểm thử PlayMode bằng bàn phím, collider với tất cả props, gamepad nếu nằm trong scope, responsive UI, render/animation/performance, kiểm licensing và bài toán điều tra suy luận có tương tác thực sự thay vì chỉ chạm vật chứng. S02–S03 là một vertical slice chứ chưa phải game hoàn chỉnh.

## Kiểm nghiệm thủ công tối thiểu

- Di chuyển 4/8 hướng và đổi hướng mặt, camera theo mượt, không thấy ra ngoài tường dù zoom.
- Va chạm tường/bàn không xuyên; An đi trước/sau bàn có thứ tự che đúng.
- S02/S03 có bố cục phòng khác nhau, 12 điểm tương tác không trùng gán scene; E/ESC/Enter không kẹt UI.
- Bảng ảnh và điều kiện mở lối ra/nguồn video đúng JSON; lưu F5/tải F9 không mất evidence/position hoặc gây giật camera.
- Test ở 1280×720, 1920×1080, các tỷ lệ khác và FPS.
