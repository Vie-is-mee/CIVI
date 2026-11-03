# CIVI / Chương mở đầu — REA investigation

**Ngày:** 01/11/2026. **Phạm vi:** game học đường canon v4.1, **S00 “Đừng bấm vội” → S01 “Trang ở giữa”**. Không trộn với truyện `Những người không có chỗ dưới bầu trời` / Lưu Quang đã từng nằm trong repo. Canon v4.1 là dữ liệu chuẩn **theo tài liệu đang tồn tại trong repo**; trạng thái file còn `tv1_draft`, cần người phụ trách nội dung xác nhận trước khi coi là bản cuối cùng.

## 1. REA setup thực sự đã làm

- Kết nối Remote Desktop Commander: ONLINE — máy `lzu`.
- Node `24.13.0`, npm `11.6.2`.
- `rea-agents@6.8.0` (npm latest 01/11/2026) yêu cầu Node `^22.23.4 || ^24.22.1 || >=26.12.1`. Không tự đổi Node của hệ thống vì còn các project khác sử dụng.
- Dùng `rea-agents@5.0.0` tương thích Node `24.13.0`, chạy được `doctor`, `inspect-managed-artifact`, `inspect-managed-members`.
- Đã thực hiện `setup --client codex --yes` **sau khi xem dry-run**: thêm MCP `mcp_servers.rea` vào `%USERPROFILE%\.codex\config.toml`, sao lưu `config.toml.rea.backup`, thêm skill REA tại `%USERPROFILE%\.agents\skills\reverse-engineer-anything`. Không chọn `--all-detected`, không cài Hopper hay Ghidra, không thay đổi config Claude/Gemini/VS Code.
- REA doctor tổng thể **không hoàn toàn healthy** vì thiếu native analysis providers và các client không được đăng ký. Đây **không phải** lỗi của managed .NET static analysis; REA đã phân tích file đầu vào thành công.
- Khi mở một phiên Codex **mới**, server MCP mới có thể được nạp; ChatGPT hiện tại vẫn thao tác qua Remote Desktop Commander và REA CLI, không khẳng định MCP này đã được nạp vào ChatGPT.

## 2. Artifact/CIL evidence (không chạy file game trong quá trình kiểm tra static)

**Đối tượng hợp pháp:** binary do nhóm CIVI tự build, không phải Blue Archive.

`release/SchoolWorldV2/CIVI_SchoolWorld_V2_x86_Data/Managed/Civi.TV3.Preview.dll`

REA trả:
- SHA256: `1198ffda2d784fc602f33fc28c50aca7d0e5cfcdd2644aafca363a14b08509ce`.
- PE/CLI, Unity Mono CIL, managed architecture AnyCPU; module `Civi.TV3.Preview.dll`.
- 112 TypeDef, 579 MethodDef, 623 fields; khoảng 5.311 static call edges trong output member analysis. **Những con số này phản ánh DLL đã build, không phải số lượng gameplay hoàn thiện.**
- Các class đã xác nhận có mặt: `CiviFollowCamera2D` (5 methods), `CiviSchoolWorldV2` (26), `CiviSchoolStudioV2` (13), `S00PreviewPlayer` (40), `StorySession` (23), `S00Session` (17).
- `CiviSchoolWorldV2.Update` (CIL token `0x060000b7`), `FixedUpdate` (`0x060000b6`), `Boot` (`0x060000b1`) có mặt. Đây là **evidence về thành phần và CIL**, không khẳng định tất cả hoạt động mượt mà trong Play Mode.
- File kết quả thô tạm: `%LOCALAPPDATA%\Temp\CIVI_REA_RESEARCH_2026_11_01\identity.json` và `members.json`. Đã giữ bên ngoài repo để tránh đưa vào repo hàng MB dữ liệu phân tích có thể tạo lại.

## 3. Định hướng kỹ thuật rút ra

Giữ: `CiviFollowCamera2D`, hệ chuyển động và physics, map navigation, `StorySession`, `S00Session` và dữ liệu tác giả; chỉ hợp nhất qua **một scene/milestone Ch1 riêng**. Không tạo lại story state bằng flag tùy tiện.

Cần cải tiến:
1. **Scene S00 sân tập chụp:** bố cục gameplay theo props `PROP_CLAMP`, `PROP_SIGN_HEN`, `PROP_DIAGRAM`, `PROP_BATTERY`, máy ảnh và hai ảnh hỏng, một ảnh tốt. Có An, Nhi, Hạ, Minh đúng sự kiện F01/F02.
2. **Mở đầu có hành động:** cho An di chuyển và tới vùng tương tác chụp ảnh; lựa chọn cách kê máy lấy trực tiếp từ `S00.json`, không tạo nhánh ngoài source.
3. **NPC animation:** tác giả cần duyệt sprite riêng An/Nhi/Hạ/Minh, idle/walk và tương tác; không sao chép Blue Archive. Dùng modular scene asset mới hoặc có phép tương đương.
4. **Transition cửa S00 → S01:** chỉ sau khi S00 StorySession đã hoàn tất hợp lệ; sang phòng thiết kế, không tự bỏ qua điều kiện.
5. **Scene S01:** trình bày `PROP_PROOF_16_18`, `PROP_CHAT_REMOVE`, `PHOTO_0443`, `PROP_THREE_LAYOUT`, `PROP_DIAGRAM` theo đúng hội thoại và điều kiện trong S01.
6. **Không sửa canon:** không tạo ảnh 0443 có Nhi, không gán thủ phạm, không biến sự đồng ý thành điểm quan hệ. Kịch bản v4.1 phân biệt tư liệu với quyền công bố.
7. **Mỹ thuật:** môi trường campus hiện tại là pixel-art prototype. Nếu chuyển sang 3D stylized/2.5D học đường để đạt quality bar cao, cần quyết định pipeline nhân vật & backdrop thống nhất; không ghép cảnh 3D Blue Archive vào 2D CIVI.

## 4. Tham khảo Blue Archive theo đường hợp pháp

- **Cơ sở công khai:** báo cáo NDC25 ngày 24/06/2025 về quy trình sản xuất background Blue Archive của nhóm quy mô nhỏ: https://www.inven.co.kr/webzine/news/?news=306977.
- **Phạm vi học:** visual hierarchy, bố trí công trình, điểm nhấn màu, quy trình modular và nhân vật có đời sống. Đây là **public developer talk**, không phải kết quả decompile Blue Archive.
- **Không làm:** trích xuất và ship texture/scene/model/animation của Blue Archive vào CIVI khi chưa có quyền; không bỏ khóa bảo vệ hay vượt DRM; không giả nhận đã reverse engineer file Blue Archive.

## 5. Acceptance gates cho Chương 1

- Cảnh S00 và S01 **có thể chơi đầu-cuối**, không chỉ bấm qua ảnh.
- Chuyển camera liền mạch khi đi trong scene; cửa/trigger đưa đúng scene khác, vị trí và state được phục hồi.
- An idle/walk 4 hướng; các NPC S00 có pose/animation đủ phân biệt, di chuyển không xuyên collider.
- Một sân và một phòng có cùng palette, art được cấp phép, props gần camera che đúng sorting layer.
- Player thấy đủ **PHOTO_S00_GOOD + 2 ảnh hỏng** đúng số người và đủ bằng chứng trong S01, không thay đổi nội dung câu thoại gốc.
- 10 phút playtest trên Windows: không lỗi Console, UI đọc được ở QHD, camera không rung, lối đi rõ, lựa chọn S00 đạt S01 theo data contract.
- Kiểm tra Story Runtime regressions và 3 đường lựa chọn mở đầu S00, S01 không bỏ các điều kiện.

**Hiện trạng:** REA setup + static evidence + scope validation đã hoàn tất. **Chưa dựng hoặc xác minh playable Chapter 1 mới**, chưa có Blue Archive input được cấp phép để phân tích nội bộ. Các bước code và art còn lại được theo dõi trong file này, không coi research là sản phẩm game cuối.

## 6. Test content Chương 1 (tự chạy trong repo)

Lệnh: `python tools/research/check_ch1_scope.py`

- PASS S00: 5 steps, 11 choices, 29 dialogue lines, 27 authored routes, tất cả dẫn sang S01.
- PASS S01: 2 steps, 2 choices, 10 dialogue lines, 1 authored route, dẫn sang S02.
- PASS: các asset cần có trong source S00/S01 và toàn bộ tham chiếu dialogue/transition hợp lệ về cú pháp.
- Regression: `dotnet run --project tools/StoryRuntimeTests/StoryRuntimeTests.csproj -- <repo-root>`: **69/69 assertions PASS**.
- Script tái hiện REA: `tools/research/run_rea_managed.ps1`. Phân tích read-only; không thực thi code trong DLL.
- Giới hạn: test chỉ kiểm **dữ liệu tác giả và cấu trúc tuyến**, chưa chứng minh hình ảnh asset đạt yêu cầu, animation đẹp hay hai cảnh thực sự liền mạch trong Unity.
