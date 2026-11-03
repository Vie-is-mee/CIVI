# CIVI — School Visual V2 | handoff 31/10/2026

## Chạy bản mới (không cần thao tác code)

Từ repo `C:\Users\asus\Documents\GITHUB PROJECT\VSIC 2026 - CIVI`, mở **`PLAY_CIVI_SCHOOL_V2.bat`**. Bản Windows độc lập ở `release/SchoolWorldV2/CIVI_SchoolWorld_V2_x86.exe` (không copy riêng .exe; cần thư mục Data đi cùng).

Trong Unity **6000.6.6f1**: menu **CIVI > School World V2 > Open Campus**, nhấn **Play**. Tên scene `CIVI-School-VisualV2.unity`. Bên phải hành lang có cánh cửa dẫn tới `CIVI-School-DesignStudio-V2.unity`; nhấn **E** khi đến gần cửa, di chuyển trong phòng rồi đến cửa ra nhấn **E** để về sân. Cả hai scene đều có camera follow và bố cục liền mạch trong nội bộ scene.

## Nâng cấp đã thực hiện
- **Art direction mới**: nền sáng, cây bóng mát, sân cờ, lối đi dễ nhìn, bãi xe đạp, cửa sổ lớp học và không gian phòng CLB. Không còn dùng các bức ảnh Canva thumbnail ghép nối. Bốn lớp floor/buildings/props/foreground độc lập; ảnh 2432 × 960 pixel dựng bằng code tại `tools/school_modular/build_school_visual_v2.py`.
- **Nhân vật học sinh mới**: 32 × 48 pixel, 4 hướng × 4 frame (idle + 3 frame walk), art được tạo cho v2, không phải sprite 16px cũ. Chuyển động animation theo phím, bóng dưới chân và camera follow.
- **Scene thật**: campus 152 × 60 tile và design studio 32 × 22 tile, mỗi scene có collider grid, hai scene được đăng ký trong Build Settings và trigger cửa liên kết bằng E. Khi quay lại campus, vị trí và các điểm đã khám phá vẫn còn trong phiên chạy.
- **Tương tác/lưu tải**: giữ 6 hotspot, WASD, E, H, F5/F9; dùng save riêng `civi-school-world-v2.json`, không đụng dữ liệu bản cũ.
- **Khả năng rollback**: v1 `CIVI-School-SeamlessCampus.unity`, project S00/S02 cũ và bản Lưu Quang đều được giữ, chỉ disabled khỏi build v2. Backup Project Settings/Build Settings/README tại `artifacts/school-visual-v2-backup-2026-10-31`.

## Checklist test cho người chơi
1. Mở file BAT; quan sát trường học, tỷ lệ nhân vật, HUD, chi tiết sân/cổng.
2. Giữ WASD: đi từ cổng qua sân, hành lang; camera phải cuộn theo nhân vật, không bị ranh giới ảnh hay loading.
3. Đi đến cửa phòng thiết kế ở khu hành lang gần x=119, y=39 của map; ấn E. Phải tải scene **phòng thiết kế**.
4. Di chuyển trong phòng, nhìn bàn/tài liệu/máy tính; đi xuống gần cửa và ấn E để quay lại hành lang. Nhân vật phải xuất hiện lại gần cửa.
5. Thử lách xuyên bàn/tường/cây; xác nhận collider chặn.
6. Test hotspot E và F5/F9 lưu/tải ở campus, test ở 1280×720 và 2560×1440.

## Kiểm tra đã thực hiện
- ✅ Python xuất **23 PNG** gồm outdoor/indoor layers và 16 animation frames.
- ✅ Unity 6000.6.6f1 biên dịch; **Editor smoke PASS 2 scene** (camera, nhân vật, hotspot, nav, Build Settings).
- ✅ Windows x86 Player build thành công; x64 Editor machine hiện thiếu WindowsPlayer runtime x64.
- ✅ EditMode smoke được chạy lại trên **repo chính** sau khi copy.
- ⏳ Chưa xác minh toàn bộ thao tác **PlayMode đi hết trường / cửa ra-vào bằng bàn phím thực**; người dùng cần test.
- ⚠️ **Mức chất lượng**: visual v2 rõ ràng và nhất quán hơn nhưng vẫn là stylized/pixel-art **prototype**, chưa phải bản production final (chưa có custom animated NPC, thoại kịch bản, soundscape, lighting VFX, animation 6–8 frames và art do illustrator duyệt).
- ⚠️ Nhiệm vụ school-world này tập trung vào visual/environment gameplay theo yêu cầu mới, **không chuyển đổi bản truyện sci-fi Lưu Quang**. Hai tuyến nội dung đang được giữ song song trong repo để nhóm xác nhận canon.

## Source
Asset mỹ thuật mới được dựng trực tiếp bằng Pillow script, không sử dụng font file riêng để phát hành và không phụ thuộc Canva thumbnail; các asset Kenney CC0 cũ vẫn được giữ nguyên trong thư mục source, không ghi đè.