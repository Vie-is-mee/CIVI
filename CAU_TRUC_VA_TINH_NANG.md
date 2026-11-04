# CIVI_2 - Cấu trúc dự án và đặc tả tính năng

Ngày phân tích: 21/10/2026. Thư mục đích: `D:\VSIC\GAME\CIVI_2`.

Cập nhật triển khai tài liệu 22/10/2026: cây thư mục đã tạo; bộ biên soạn TV1 0.1 có tại [Bắt đầu TV1](docs/story/TV1_BAT_DAU_O_DAY.md). Dữ liệu và thoại chờ kiểm nhận, chưa có Unity runtime hoặc kết quả playtest. Cây dưới đây vẫn gồm cả những file kỹ thuật dự kiến chưa được tạo.

Đây là bản thiết kế để bắt đầu lập trình, dựa trên mã CIVI hiện có, hai PDF và đủ 40 đầu việc trong XLSX. Các thư mục, lớp, schema và API dưới đây là **đề xuất**, chưa được triển khai. Trong lần làm tài liệu này chỉ tạo file Markdown này; không sao chép, sửa hay build dự án cũ.

## 1. Nguồn và quy tắc đối chiếu

| Mã nguồn tham chiếu | Tài liệu thực tế | Dùng cho |
| --- | --- | --- |
| STORY | [CIVI_kich_ban_v4_1.pdf](../../CIVI_kich_ban_v4_1.pdf) - 20 trang | Chuẩn sự kiện, nhân vật, cảnh S00–S10, kết thúc, quyền, MVP và QA. Ưu tiên phần triển khai trang 17–20. |
| AI | [CIVI_AI_2_1_FIXED.pdf](../../CIVI_AI_2_1_FIXED.pdf) - 15 trang | Kiến trúc nhận diện ý định, kiểm luật, dự phòng, cách so A/B/C và đo chi phí. Đặc biệt trang 9–14. |
| PLAN | [CIVI_Phan_cong_5_thanh_vien.xlsx](../../CIVI_Phan_cong_5_thanh_vien.xlsx) | Ba sheet `01_Phan_cong`, `02_Cong_viec`, `03_Ban_giao`; phạm vi, owner, phụ thuộc, đầu ra, mốc và tiêu chí nhận. |
| CODE | [CIVI](../CIVI/README.md), đặc biệt `Game/Assets/Civi41/*.cs` | Nền tảng Unity, hành vi bản thử, tài nguyên và công cụ build có thể kế thừa. |

Tài liệu được tìm thấy tại `D:\VSIC`, không nằm trong repo CIVI. Workbook ở `D:\VSIC\output\CIVI_5_thanh_vien` không được dùng thay bản gốc người dùng chỉ định. Không sử dụng PDF ôn tập CNN hoặc các bản render trong `tmp` làm nguồn của game.

**Thứ tự áp dụng:** STORY v4.1 quyết định nội dung; PLAN quyết định phạm vi sản xuất và bàn giao; AI 2.1 cung cấp cơ chế kỹ thuật sau khi thay ví dụ v3.0. Mã hiện tại là bản rút gọn, không phải chuẩn đầy đủ để chép lại nguyên trạng.

Những khác biệt phải xử lý trước khi code:

- AI trang 5 mô tả mở rộng khung ảnh để thấy Nhi theo tuyến cũ. Với v4.1, ảnh **0443 thật sự chỉ có An, Hạ, Minh**; không crop hay dùng AI để đưa Nhi trở lại ảnh đó. 0441/0442 và video là các nguồn khác.
- Không mang bảng ca trực, giờ máy ảnh lệch, trò ghép chứng từ hoặc tuyến kiểm ghi công của v3.0 vào bản mới (STORY trang 3, 20; PLAN `W1.1`, `W4.1`).
- Các giờ 16:15/16:31 thuộc ngày hội chợ quá khứ và vẫn được v4.1 xác lập ở trang 14–15. Không nhầm chúng với buổi chụp lại 16:45 ngày 08/05.
- AI chỉ thí nghiệm tại **S03**, theo PLAN; không làm thêm cảnh AI ở S05 dù tài liệu AI cũ nêu phương án đó.
- A/B/C của **thí nghiệm đầu vào** và E-A/E-B/E-C của **kết thúc truyện** là hai bộ mã độc lập.
- Ngày hẹn bổ sung không cố định là 11/05 chỉ vì bản thử dùng ngày đó. Cần lời xác nhận và lịch phù hợp; nếu chưa xác nhận thì không dựng buổi gặp.

## 2. Hiểu game và cốt truyện cần giữ

### 2.1. Trải nghiệm chính

CIVI là game kể chuyện tương tác 2D tiếng Việt trong bối cảnh học sinh cuối cấp làm kỷ yếu. Người chơi vào vai An, tìm hiểu vì sao trang 17 biến mất và vì sao lời hẹn chụp đủ bốn người lại thành một bản dựng ba người. Lõi chơi là phối hợp chụp ảnh, quan sát đồ vật và tư liệu, hỏi bạn, dựng nháp, thu xếp cuộc hẹn và hoàn thành trang kỷ yếu.

Nhi khởi xướng trang hai ảnh trước/sau ở buổi tập bảy tháng trước. Đến hội chợ, An đưa điện thoại cho Nhi chụp giúp và hứa lát chụp đủ, rồi quên. Ảnh 0443 là ký ức vui thật của ba người, đồng thời thiếu Nhi vì cô đứng sau máy. Hạ muốn giữ ảnh đó; điều này không biến Hạ thành phản diện. Minh rút trang khỏi bản in thử theo tin nhắn mơ hồ của An, không phá hoại. Nhi muốn được hỏi về việc tham gia và cách sử dụng hình ảnh của mình, có lịch riêng và có quyền từ chối.

Người chơi có thể hiểu sớm, hỏi Nhi trực tiếp và tránh bản nháp tôn vinh không phù hợp. Khi đó xung đột chuyển sang thương lượng, chuẩn bị và thực hiện cuộc gặp. Không ép người chơi làm sai để kích hoạt cảnh tác giả đã viết. Không có điểm đạo đức, thanh tình bạn hoặc một câu xin lỗi giải quyết mọi việc. (STORY trang 2–15.)

### 2.2. Các hướng kết

| Hướng | Điều kiện và hiện vật phải có |
| --- | --- |
| E-A - Trước và bây giờ | Trang có tư liệu cũ và ảnh mới được phép dùng. Ảnh mới có thể bốn hoặc ba người; chú thích phản ánh đúng người thực sự có mặt. Quan hệ không tự hồi phục hoàn toàn. |
| E-B - Không chụp lại | Hoàn thành thu đồ, bỏ khung chờ nếu không còn phù hợp, dựng trang bằng ảnh cũ/sơ đồ được đồng ý. Có trang hoàn chỉnh dù không có ảnh mới trong trang 17. |
| E-C - Hẹn ngày khác | Có lịch, người tham gia và mục đích được xác nhận; thực hiện cuộc gặp hoặc giao ảnh/tờ bổ sung. Không kết thúc ngay ở một lời hứa. Chưa được xác nhận thì không tự mở cảnh gặp. |

Lưu ý: kết B vẫn có thể xảy ra khi đã chụp ảnh ba người nhưng quyết định chỉ giữ ảnh đó ở album riêng. Không suy kết thúc chỉ từ số người trong ảnh như bản thử. MVP cần hai đường sân và trang kết có hệ quả, chưa bắt buộc mọi biến thể của cả ba kết. (STORY trang 12–14, 19; PLAN `03_Ban_giao!A20:E22`.)

### 2.3. Lịch truyện

| Thời điểm | Quy tắc |
| --- | --- |
| Buổi tập tháng Mười năm trước | Chụp bằng hẹn giờ, đủ bốn, có hai ảnh hỏng và sơ đồ tia chớp. |
| Ngày hội chợ | Hẹn 16:15; Nhi đến 16:31. An đưa máy cho Nhi; ảnh 0443 không có Nhi. |
| 07/05/2026, thứ Năm, 10:00 | Phát hiện trang mất, điều tra, dựng nháp và thu xếp lời mời. |
| 08/05, thứ Sáu, 16:45 | Buổi chụp lại. Nhi đi lúc 17:10; quầy tháo lúc 17:30. |
| Trước 17:00 thứ Sáu | Chỉ khi giữ lịch gia hạn hợp lệ mới có hạn giao file 12:00 thứ Bảy. |
| 09/05, thứ Bảy, 10:00 | Hạn giao thông thường; Minh có thi thử, phải tổ chức bàn giao. |
| 11/05 hoặc 12/05 | Phát sách theo lịch thường/gia hạn đã chọn. Hẹn bổ sung có xác nhận riêng. |

Đọc thoại, xem ảnh, mở sổ và chỉnh thử khung **không làm giờ truyện trôi theo đồng hồ thật**. Chỉ hành động được khai báo có thời lượng mới tiến giờ; hiển thị tác động trước khi chọn. Lịch sản xuất tháng 10–11/2026 tách khỏi lịch truyện tháng 5. (STORY trang 11, 14–15.)

## 3. Dự án cũ đang có gì?

Nhận định dưới đây dựa trên đọc mã, chưa chạy Unity hoặc kiểm lại binary trong lượt phân tích này.

| Thành phần hiện tại | Có trong nguồn | Cách dùng ở CIVI_2 |
| --- | --- | --- |
| Nền tảng | Unity `6000.6.5f1`; C#; Built-in; uGUI, TMP và Input System. Manifest có Input System `1.20.0`, uGUI `2.6.0`. | Giữ nền tảng và phiên bản ban đầu để giảm việc chuyển đổi. Chưa cần đổi URP hoặc engine. |
| `Civi41Story.cs` | `Run`, `Beat`, `Story`; node 0–22, luật `Act/Advance`, ba mã kết theo `photoCount`. | Dùng làm nguồn đối chiếu và luồng mẫu; thay node số bằng scene/beat ID ổn định, tách luật thành module. |
| `Civi41Player.cs` | Khoảng 59 KB; dựng UI bằng code, thoại, tư liệu, nháp, ảnh cuối, âm thanh, input, save/load và QA runtime. | Tách lớp theo trách nhiệm. Có thể giữ phong cách, font và hành vi đọc đã có. |
| `Civi41Checks.cs` | Bốn route mẫu, kiểm điều kiện chứng cứ, thương lượng, quyền in tổng, lệnh sai và chống publish lặp. | Giữ các ca có ý nghĩa rồi bổ sung trạng thái và quyền chi tiết. Chưa đủ chứng minh toàn bộ bảng nhánh v4.1. |
| `Civi41Editor.cs` | Menu preview, kiểm lựa chọn và build Windows; tạo scene khi build. | Chuyển vào thư mục `Editor/`, cập nhật đường dẫn và thêm kiểm dữ liệu trước build. |
| Art và audio | Hai bối cảnh, atlas nhân vật/biểu cảm/ảnh, font Việt, âm nền/click/shutter trong `Resources`. | Tái sử dụng tài nguyên phù hợp, kiểm số người/đạo cụ, bổ sung sơ đồ và video/animatic theo đầu việc. |
| Đóng gói | Build/Package PowerShell; launcher BAT tải, kiểm hash, cài và mở offline. | Giữ công cụ có ích, đổi tên/version/output để không trỏ nhầm release cũ. |

Khoảng thiếu rõ trong mã so với PLAN:

- `invite` chỉ có `unasked/accepted/declined/not_invited`, chưa có `pending` và trạng thái có mặt/rời sân độc lập.
- `publicationConsent` là một cờ tổng; chưa thể hiện quyền theo từng tư liệu, người, phạm vi và thời điểm thu hồi.
- `draft` là chỉ số mẫu; chưa có lịch sử người nhận, người đã xem, thông báo dừng bản cũ và trạng thái bảng cảm ơn.
- `haCollaborates` gắn trực tiếp với giữ ảnh trong luật mẫu; cần tách hỗ trợ dàn trang khỏi quyền/vị trí giữ ảnh theo thương lượng thực tế.
- Chưa có mô hình chuẩn bị thiết bị, chia việc, lịch in/gia hạn và tiến thời gian bằng hành động.
- `album` có giá trị mặc định nhưng chưa thấy UI chọn tên album ở luồng mở đầu; hai ảnh hỏng và sơ đồ cần thành hiện vật có thể xem.
- Video hiện là ba khung dựng thử; cần gói video/animatic có âm và phụ đề như `W3.3`, không mô tả là video đầy đủ đã có.
- Ảnh cuối hiện được dựng trong UI; chưa thấy chức năng xuất riêng ảnh trang 17 ra PNG cho người chơi.
- Chưa có bộ nhập tự do theo luật, dịch vụ AI, thí nghiệm A/B/C và log đánh giá tương ứng.

## 4. Phạm vi cần làm

### 4.1. Game MVP bắt buộc

| Mã tính năng | Tính năng | Điều kiện hoàn thành | Đầu việc chính |
| --- | --- | --- | --- |
| F01 | Chạy cảnh và thoại có nhánh | Cảnh/beat có ID; chuyển hợp lệ; khóa thao tác khi chuyển; lựa chọn làm đổi phản ứng hoặc sản phẩm. | W1.3–W1.6, W2.1–W2.5 |
| F02 | Mở đầu chơi được | Kê máy/kẹp, hẹn giờ, chọn cử chỉ, chọn tên album ngắn; xem ảnh đủ bốn, hai ảnh hỏng và sơ đồ. | W1.3, W3.2 |
| F03 | Bàn tư liệu | So 0441/0442/0443, cận kẹp, xem video/animatic có phụ đề; ghi điều đã thấy theo nguồn; hỏi Nhi trực tiếp không bị né sự thật. | W1.4, W2.3, W3.3 |
| F04 | Ba mẫu trang nháp | `tribute/paired/memory`; đặt ảnh/chú thích trong phạm vi mẫu; xem trước và chọn người nhận trước gửi. Không cần trình thiết kế tự do. | W1.5, W2.4, W3.4 |
| F05 | Phản hồi và thu hồi nháp | Ghi phiên bản đã gửi/đã xem; báo lại đúng người khi dừng bản; gỡ đạo cụ thật; không xóa ký ức của người đã xem. | W1.5, W2.4 |
| F06 | Thương lượng Hạ | Có `collaborate/handover_only`; giữ/chuyển ảnh theo quyền; An tự làm được khi Hạ chỉ bàn giao. Nhận lỗi không tự mua được hợp tác. | W1.5, W2.4 |
| F07 | Lời mời, có mặt, quyền ảnh | `unasked/pending/accepted/declined` là trạng thái lời mời; tách hiện diện và quyền công bố theo ảnh. | W1.2, W1.6, W2.2, W5.2 |
| F08 | Chuẩn bị sân và lịch | Máy/kẹp/pin, người giữ máy/xuất file; lịch hiển thị từ trước; chỉ hành động dài mới tiến giờ. | W1.6, W2.2, W2.5 |
| F09 | Sân có Nhi | Thương lượng khung, hẹn giờ, An vào ảnh; có thể gỡ bảng; Nhi chọn có cho dùng từng ảnh. Có thể chuyển sang đường vắng nếu Nhi đi. | W1.6, W2.5, W3.5 |
| F10 | Sân vắng Nhi | Ba hành động: chụp ba, không chụp và thu đồ, đề nghị hẹn khác. Mỗi lựa chọn có thao tác và hiện vật cuối. | W1.6, W2.5, W3.5 |
| F11 | Chốt trang và kết | Nội dung đúng người/ngày/nguồn/phạm vi; hiện vật phản ánh quyết định; xuất PNG trang cuối kèm dữ liệu manifest. | W1.6, W2.5, W3.5 |
| F12 | Lưu/đọc | Giữ cảnh, beat, lựa chọn, tư liệu, quyền, lịch, nháp và người nhận; file lỗi không phá lượt hiện tại; chơi lại không xóa save. | W2.2, W2.7 |
| F13 | UI và khả năng đọc | Font Việt; lịch sử thoại; sổ tư liệu/gợi ý theo luật; đọc nhanh, âm lượng/tắt âm, giảm chuyển động; trạng thái tải/lỗi rõ ràng. | W3.4, W3.6–W3.7 |
| F14 | Build Windows offline | Hoàn tất hai đường khi tắt dịch vụ; gói đủ tài nguyên và giấy phép; có hướng dẫn và kiểm trên máy khác. | W2.8, W5.7–W5.8 |

MVP mục tiêu 12–20 phút theo nguồn, phải đo khi chơi thử. Các thời lượng từng cảnh trong PDF là ước lượng và không cộng thành cam kết chính xác. Ưu tiên hai đường sân trước thêm trang trí hoặc biến thể phụ.

### 4.2. Thí nghiệm đầu vào S03

| Chế độ | Cách nhập | Thành phần |
| --- | --- | --- |
| `Buttons` (A) | Chọn câu/hành động viết sẵn | `ButtonIntentSource` |
| `Rules` (B) | Nhập câu, bộ quy tắc xác định ý định | `RuleIntentRecognizer` |
| `AI` (C) | Nhập câu, dịch vụ trả ý định có cấu trúc | `RemoteIntentRecognizer` |

Cả ba dùng **cùng danh sách ý định, sự thật, luật, thoại phản hồi và snapshot đầu cảnh S03**. Chỉ đường hiểu đầu vào khác nhau. Game ngoại tuyến và chế độ A là đường chơi hoàn chỉnh. C lỗi, timeout hoặc sai JSON phải đưa về nút, giữ nguyên tiến trình/nháp. B không hiểu thì hỏi lại và cũng có nút dự phòng.

Phần phải chuẩn bị theo W4.1–W4.8: schema, corpus dev/test tách nhau, rule parser, AI adapter, validator, phản hồi mẫu, fallback, log latency/cost, báo cáo và quyết định giữ/thu hẹp/tắt. Không tự sinh thoại cốt truyện ở MVP. Không gọi save/load, gợi ý theo trạng thái hay nhớ lời hứa là AI.

### 4.3. Đầu ra phục vụ nhóm và hồ sơ

Các đầu việc TV5 về playtest, khách hàng, kinh doanh, slide, video, diễn tập và xác nhận nộp là **sản phẩm vận hành**, không phải màn hình phải lập trình vào game. Dự án cần chỗ lưu biểu mẫu, kết quả và bằng chứng, cùng công cụ xuất log; không cần xây dashboard quản trị, tài khoản hay thanh toán để đáp ứng XLSX.

Chỉ mở rộng sau mốc G3 nếu còn nguồn lực: toàn bộ biến thể kết A/B/C, thêm mỹ thuật và cảnh AI thứ hai. Không xây lại engine, multiplayer, thế giới mở, hội thoại AI vô hạn hoặc tuyến điều tra mới.

## 5. Cấu trúc thư mục đề xuất

Thư mục mở trong Unity Hub sẽ là **`D:\VSIC\GAME\CIVI_2\Game`**. Root bên ngoài giữ tài liệu nhóm, nguồn mỹ thuật, dịch vụ AI và công cụ. Cây này mô tả đích triển khai, không có nghĩa các file đã được tạo.

```text
CIVI_2/
├── CAU_TRUC_VA_TINH_NANG.md        # Tài liệu này
├── README.md                     # Mở project, chạy, build, vị trí save/export
├── .gitignore
├── .gitattributes
├── .github/
│   └── workflows/
│       └── validate.yml           # Kiểm content, tests và đóng gói khi đã cấu hình
├── docs/
│   ├── references/
│   │   └── sources.md             # Tên, phiên bản, hash, đường dẫn ba tài liệu nguồn
│   ├── story/
│   │   ├── canon-v4.1.md          # Sự kiện khóa; thay ví dụ AI v3
│   │   ├── scene-map.md           # S00–S10 và nhánh kết
│   │   ├── npc-knowledge.md       # Ai biết gì, từ nguồn nào, tại cảnh nào
│   │   └── dialogue-review.md
│   ├── design/
│   │   ├── mvp-scope.md
│   │   ├── state-and-consent.md
│   │   ├── timeline.md
│   │   └── asset-list.csv         # ID, file, kích thước, nơi dùng, nguồn/quyền
│   ├── engineering/
│   │   ├── architecture.md
│   │   ├── data-contracts.md
│   │   └── migration-from-civi41.md
│   ├── production/
│   │   ├── task-index.md          # W1.1–W5.8, owner, phụ thuộc, link đầu ra
│   │   ├── handoffs.md
│   │   ├── milestones.md
│   │   └── decisions/             # Ghi nhận khóa scope, schema, giữ/tắt AI
│   ├── qa/
│   │   ├── branch-matrix.csv
│   │   ├── playtest-protocol.md
│   │   ├── issue-template.md
│   │   └── acceptance-checklist.md
│   ├── business/
│   │   ├── interview-template.md
│   │   └── deployment-hypotheses.md
│   └── submission/
│       ├── requirements-checklist.md
│       ├── demo-script.md
│       └── release-notes.md
├── contracts/                    # TV1/TV2/TV4 thống nhất trước khi ghép
│   ├── scene.schema.json
│   ├── game-state.schema.json
│   ├── intent-request.schema.json
│   ├── intent-response.schema.json
│   ├── asset-manifest.schema.json
│   └── examples/
│       ├── S03.scene.json
│       ├── S03.request.json
│       └── S03.response.json
├── Game/                         # Unity project duy nhất
│   ├── Assets/
│   │   ├── _Civi/
│   │   │   ├── Scenes/
│   │   │   │   ├── Bootstrap.unity
│   │   │   │   ├── MainMenu.unity
│   │   │   │   └── Story.unity     # Các cảnh truyện là dữ liệu, dùng chung sân khấu
│   │   │   ├── Scripts/
│   │   │   │   ├── Domain/        # C# thuần, không phụ thuộc MonoBehaviour/HTTP/UI
│   │   │   │   │   ├── State/     # GameState, StoryPosition, state enums
│   │   │   │   │   ├── Story/     # SceneDefinition, Beat, Choice, Condition, Effect
│   │   │   │   │   ├── Evidence/  # EvidenceRecord, SourceReference, KnowledgeRules
│   │   │   │   │   ├── Drafts/    # DraftVersion, RecipientHistory, PropState
│   │   │   │   │   ├── Consent/   # ConsentGrant, ConsentScope, PublicationRules
│   │   │   │   │   ├── Scheduling/ # StoryClock, Invite, Attendance, PrintSlot
│   │   │   │   │   └── Endings/   # EndingRules, FinalPage, DeliveryPlan
│   │   │   │   ├── Application/   # Các thao tác người chơi được làm
│   │   │   │   │   ├── Flow/      # SceneRunner, CommandDispatcher, TransitionGuard
│   │   │   │   │   ├── Investigation/ # InspectEvidence, AskWitness, NotebookService
│   │   │   │   │   ├── Drafting/  # PreviewDraft, SendDraft, RetractDraft
│   │   │   │   │   ├── Courtyard/ # PrepareEquipment, CapturePhoto, PackEquipment
│   │   │   │   │   ├── Publishing/ # ValidateFinalPage, FinalizePage, ExportFinalPage
│   │   │   │   │   └── Intent/    # IIntentRecognizer, IntentValidator, IntentPipeline
│   │   │   │   ├── Infrastructure/
│   │   │   │   │   ├── Content/   # JsonContentRepository, ContentValidator
│   │   │   │   │   ├── Persistence/ # JsonSaveRepository, SaveMigration
│   │   │   │   │   ├── Media/     # AssetCatalog, VideoPlayerAdapter, SubtitleTrack
│   │   │   │   │   ├── Export/    # PngFinalPageExporter, ManifestWriter
│   │   │   │   │   └── Telemetry/ # JsonlEventSink, ExperimentSession
│   │   │   │   ├── Presentation/
│   │   │   │   │   ├── Bootstrap/ # CompositionRoot, GameSession, bindings
│   │   │   │   │   ├── Dialogue/  # DialoguePresenter, Typewriter, HistoryView
│   │   │   │   │   ├── Evidence/  # EvidenceDesk, PhotoCompareView, VideoView
│   │   │   │   │   ├── Drafting/  # DraftEditorView, RecipientsView, PreviewView
│   │   │   │   │   ├── Courtyard/ # StagePresenter, CameraRigView, EquipmentView
│   │   │   │   │   ├── FinalPage/ # FinalPageView, EndingPresenter
│   │   │   │   │   └── Common/    # Menu, Pause, Modal, Settings, Input, Audio
│   │   │   │   └── Integrations/
│   │   │   │       └── Intent/
│   │   │   │           ├── Buttons/ # ButtonIntentSource
│   │   │   │           ├── Rules/   # RuleIntentRecognizer
│   │   │   │           └── Remote/  # RemoteIntentRecognizer, DTO, timeout/cancel
│   │   │   ├── Content/          # JSON là bản nguồn nội dung được duyệt
│   │   │   │   ├── Story/v4_1/
│   │   │   │   │   ├── scenes/   # S00.json ... S10.json; S09_A/B; E_A/B/C
│   │   │   │   │   ├── dialogue/ # Thoại theo cảnh, speaker, mood, response_id
│   │   │   │   │   ├── facts.json
│   │   │   │   │   ├── npc-knowledge.json
│   │   │   │   │   ├── timeline.json
│   │   │   │   │   └── ending-rules.json
│   │   │   │   ├── Evidence/evidence-catalog.json
│   │   │   │   ├── Drafts/       # tribute.json, paired.json, memory.json
│   │   │   │   ├── Intents/S03/  # allowed-intents, rules, responses, clarifications
│   │   │   │   └── Manifests/asset-manifest.json
│   │   │   ├── Art/
│   │   │   │   ├── Backgrounds/  # DesignRoom, Courtyard
│   │   │   │   ├── Characters/   # An, Nhi, Ha, Minh; Vy khi cần
│   │   │   │   ├── Photos/       # S00_good, S00_bad_01/02, 0441/42/43, kết
│   │   │   │   ├── Props/        # Sơ đồ, kẹp, pin, bảng HẸN, bảng cảm ơn
│   │   │   │   └── UI/           # Khung giấy, icon, template trang
│   │   │   ├── Audio/            # Ambience, SFX, Mixer
│   │   │   ├── Video/            # Animatic/chín giây và phụ đề có ID
│   │   │   ├── Fonts/            # Font gốc, TMP asset, fallback tiếng Việt
│   │   │   ├── Prefabs/          # Stage, DialogueBox, Choice, Modal, Photo, Page17
│   │   │   ├── Config/           # GameConfig, ExperimentConfig; không chứa secret
│   │   │   ├── Editor/
│   │   │   │   ├── Content/      # Import, validate schema/ID/quyền/nhánh
│   │   │   │   ├── Preview/      # Mở nhanh cảnh, preview ảnh/nháp
│   │   │   │   └── Build/        # CiviBuild, BuildChecks
│   │   │   └── Tests/
│   │   │       ├── EditMode/     # Luật, dữ liệu, quyền, nhánh, save, intent
│   │   │       ├── PlayMode/     # Tương tác UI, media, export, fallback
│   │   │       └── Fixtures/     # Snapshot đầu S03, state cho hai đường sân
│   │   ├── TextMesh Pro/         # Package resources riêng của bên thứ ba
│   │   ├── ThirdParty/           # Chỉ khi có dependency được duyệt
│   │   └── StreamingAssets/
│   │       └── CiviGenerated/    # Bản JSON sinh khi build, không sửa tay
│   ├── Packages/                # manifest.json, packages-lock.json
│   └── ProjectSettings/         # Giữ ProjectVersion.txt và settings cần thiết
├── services/
│   └── intent-api/              # Chỉ phục vụ chế độ C; đề xuất Python HTTP nhỏ
│       ├── README.md
│       ├── pyproject.toml       # Chốt version khi thực sự triển khai
│       ├── .env.example         # Tên biến/cấu hình mẫu, không có khóa thật
│       ├── src/civi_intent/
│       │   ├── app.py           # POST /v1/recognize-intent, GET /health
│       │   ├── schemas.py
│       │   ├── recognizer.py
│       │   ├── provider.py      # Một adapter nhà cung cấp, thay được
│       │   ├── validation.py
│       │   ├── config.py
│       │   └── metrics.py
│       ├── prompts/s03-intent.md
│       └── tests/              # Schema, timeout, lỗi provider, giới hạn request
├── evaluation/
│   ├── datasets/s03/
│   │   ├── dev.jsonl
│   │   ├── test.jsonl           # Giữ độc lập với tập chỉnh luật/prompt
│   │   └── labels.md
│   ├── protocols/abc-s03.md     # Cùng snapshot; hoán đổi thứ tự; ẩn danh phiên
│   ├── scripts/                # Chạy bộ thử, tổng hợp accuracy/p95/cost
│   └── reports/                # Kết quả tổng hợp đã rà soát, quyết định AI
├── tools/
│   ├── Content/                # Validate-Content.ps1, Sync-Content.ps1
│   ├── QA/                     # Run-Checks.ps1, Run-IntentEval.ps1
│   ├── Build-Windows.ps1
│   ├── Package-Windows.ps1
│   └── Launcher/               # Bộ cài hiện có sau khi đổi config/version
├── source-assets/              # File vẽ, dựng video/âm thanh gốc của TV3
│   ├── art/
│   ├── audio/
│   └── video/
├── licenses/                   # Font, package và thông báo tài nguyên
├── local-data/                 # Bỏ qua Git: log, bản ghi playtest/interview gốc
├── artifacts/                  # Bỏ qua Git: báo cáo chạy, video, slide, biên nhận
│   ├── qa/
│   ├── evaluation/
│   ├── demo/
│   └── submission/
└── release/                    # Bỏ qua Git: Windows, ZIP, hash, manifest phiên bản
```

### 5.1. Quy tắc đặt mã và dữ liệu

- Một Unity `Story.unity` hiển thị nhiều cảnh truyện dữ liệu. Không cần tạo 13 Unity scene riêng chỉ để đổi thoại; chỉ tách Unity scene khi có nhu cầu kỹ thuật thực tế.
- `Domain` giữ điều kiện và bất biến; `Application` thực hiện use case; `Presentation` chỉ nhận input và hiển thị. Luật quyền không nằm trong button handler hoặc prompt.
- `Integrations` cài `IIntentRecognizer`; cả B/C trả DTO và không giữ tham chiếu để sửa `GameState`.
- Unity serialize asset reference qua `AssetCatalog` ScriptableObject được build vào cấu hình bootstrap. JSON chỉ dùng `asset_id`, không ghép đường dẫn ảnh rải rác trong code.
- Editor đọc JSON nguồn trong `_Civi/Content`, kiểm hợp lệ và đồng bộ một chiều sang `StreamingAssets/CiviGenerated` trước build. Runtime dùng repository đọc bản sinh. Không sửa đồng thời hai bản. Kiểm đồng bộ quá hạn phải làm build thất bại.
- Video có thể dùng clip được AssetCatalog tham chiếu; nếu cần phát theo URL file, build tool chép đúng media sang StreamingAssets và ghi manifest. Không để runtime đoán đường dẫn Editor.
- Schema gốc đặt trong `contracts/`; C# DTO và service model tuân thủ chúng, kiểm chung bằng fixtures. Không tạo hai bộ enum intent khác nghĩa.
- TV1 sửa nội dung; TV3 sửa art/prefab UI theo ID; TV2 giữ luật/runtime; TV4 giữ recognizer và service. Mọi thay đổi schema có version, ví dụ mới và người nhận nạp thử.
- Khi tách assembly: `Civi.Domain`; `Civi.Application` tham chiếu Domain; `Civi.Infrastructure` và `Civi.Integrations` tham chiếu Application/Domain; `Civi.Presentation` ghép adapter ở CompositionRoot; `Civi.Editor` chỉ Editor. Bố trí `.asmdef` tại mỗi nhánh Scripts tương ứng, tests tham chiếu assembly cần kiểm.
- Giữ `.meta` khi di chuyển asset và để Unity chuyển tên/đường dẫn để giữ GUID. Không copy thư mục `Library/Temp/Obj/Logs/UserSettings` hay toàn bộ `.git` từ dự án cũ.
- Git theo dõi Assets + `.meta`, Packages, ProjectSettings, content, contracts, docs, script và tập thử đã duyệt. Dùng LFS cho media lớn nếu nhóm có cấu hình; bỏ qua release, save, export, secrets, dữ liệu người thử gốc và artifacts sinh tự động.

Đề xuất dịch vụ Python là lựa chọn triển khai để khóa API nằm phía dịch vụ và TV4 làm độc lập; chưa phải công nghệ bắt buộc từ PDF. Nếu nhóm đã có dịch vụ khác phù hợp thì giữ nguyên contract và adapter Unity. Game không cần cơ sở dữ liệu hoặc đăng nhập để chạy MVP.

## 6. Hợp đồng dữ liệu để bắt đầu code

### 6.1. Scene và action

Một scene chứa `schema_version`, `content_version`, `scene_id`, `background_id`, beats, choices, điều kiện vào/ra và transition. Mỗi beat có ID ổn định, speaker, `text_id`, mood và media. Mỗi choice dẫn tới **command được phép**, với điều kiện và hiệu ứng có kiểu; không cho dữ liệu content chứa mã C# tùy ý.

Ví dụ thiết kế tối thiểu:

```json
{
  "schema_version": 1,
  "content_version": "4.1",
  "scene_id": "S03",
  "background_id": "BG_DESIGN_ROOM",
  "beats": [
    {"beat_id": "S03_001", "speaker_id": "HA", "text_id": "S03_HA_OPEN", "mood": "neutral"}
  ],
  "choices": [
    {
      "choice_id": "S03_OPEN_0443",
      "text_id": "UI_OPEN_0443",
      "command": {"type": "INSPECT_EVIDENCE", "target": "PHOTO_0443"}
    }
  ]
}
```

Đây là ví dụ contract, không phải dữ liệu scene đầy đủ. SceneRunner phải kiểm ID, điều kiện, command hợp lệ, không cho đi tiếp khi media bắt buộc chưa sẵn sàng; đồng thời không khóa điều tra bằng mức hợp tác nhân vật. S04 cho xem video khi nguồn đã có; biết sớm từ Nhi không bị bắt giả vờ chưa biết.

### 6.2. GameState

| Nhóm | Trường cần có | Quy tắc |
| --- | --- | --- |
| Phiên và vị trí | `save_version`, `content_version`, `run_id`, `scene_id`, `beat_id`, `applied_command_ids` | Chống thực hiện lặp thao tác gửi/chụp/chốt khi double click hoặc load. |
| Mở đầu | `opening_gesture`, `album_name`, `opening_photo_ids`, `diagram_seen` | Giữ chi tiết người chơi tạo để dùng lại đúng nhánh cuối. |
| Điều tra | `evidence_seen`, `facts_discovered`, `npc_known_facts`, `witness_events` | Thấy ảnh không đồng nghĩa biết mọi nội dung video; lời khai có nguồn. |
| Trách nhiệm | `an_owned_camera_choice = yes/unresolved/deflected` | Chỉ phản ánh hành động/lời đã thực hiện; không là điểm thiện ác. |
| Nháp | `draft_type`, `draft_version`, `draft_sent_to`, `draft_seen_by`, `draft_retracted`, `retraction_notified_to` | Lưu version gửi; chỉnh bản mới không xóa lịch sử bản cũ. |
| Đạo cụ | `tribute_board_prepared`, `tribute_board_removed`, `prop_owners` | Bảng chỉ xuất hiện khi đã có người chuẩn bị và chưa gỡ. |
| Hỗ trợ | `ha_support = collaborate/handover_only`, `support_plan = assigned/assumed`, `assignments` | Mất hỗ trợ không khóa chứng cứ hoặc đường hoàn thành. |
| Mời và hiện diện | `nhi_invite_status = unasked/pending/accepted/declined`, `attendance`, `arrival_event`, `departure_event` | `pending` không là accepted; accepted không bảo đảm còn ở sân khi bấm máy. |
| Thiết bị | `equipment_ready = ready/repairable`, `equipment_checks`, `camera_holder` | Kẹp/pin thiếu phải có cách xử lý đã gieo trước. |
| Thời gian | `story_datetime`, `print_slot = normal/extended/renegotiate`, `extension_confirmed` | Không chạy bằng `Time.deltaTime`; gia hạn cần sự kiện xác nhận trước hạn. |
| Ảnh | `photo_result = four/three/none/later`, `captures[]` | Mỗi capture có asset/ảnh xuất, ngày, participants thật và nguồn phát sinh. |
| Quyền | `consent_grants[]`, `nhi_photo_consent` theo ảnh | Cờ yes/no/unasked chỉ là trạng thái xem theo ảnh hiện chọn; danh sách grant mới là dữ liệu gốc. |
| Cuối | `page17_final`, `delivery_plan`, `ending_id`, `export_manifest`, `finished` | Kết dựa vào nội dung được chọn và việc đã làm, không chỉ số người chụp. |
| Tương lai | `later_meeting`, `participant_confirmations`, `supplement_delivery` | Đề nghị không đồng nghĩa đã đồng ý; giao bổ sung phải được hoàn thành nếu mở E-C. |

Save ghi tạm rồi thay thế file để giảm rủi ro hỏng; validate trước khi nhận state; không ghi đè lượt đang chơi khi load thất bại. Save cũ v1/v2 của `Civi41.Run` thiếu nhiều dữ liệu quyền và lịch: phải có mapping bảo thủ hoặc báo không tương thích rồi cho chơi mới, không tự điền đồng ý. Tách vị trí lưu CIVI_2 khỏi CIVI cũ; runtime lưu ở `Application.persistentDataPath`, không trong repo.

### 6.3. Quyền dùng tư liệu

`ConsentGrant` tối thiểu: `subject_id`, `asset_id`, `scope`, `status`, `source_event_id`, `revision`. Scope đề xuất: `internal_review`, `private_album`, `yearbook_print`, `supplement_delivery`; nếu thêm đăng công khai thì phải là scope riêng được duyệt.

`EvidenceRecord` còn có chủ nguồn, người xuất hiện, quyền nguồn và các mệnh đề nó chứng minh. `CanPublish(page, scope)` kiểm từng ảnh/đoạn trích được chọn và các grant liên quan. `CanInspect` và `CanPublish` là hai hàm riêng. Đổi ảnh/chú thích/recipient phải kiểm lại quyền phù hợp; không tái dùng một cờ đồng ý cho mọi tư liệu.

Ví dụ: Vy cho xem video nội bộ để kiểm chứng không có nghĩa video được đưa vào sản phẩm; Nhi nhận lời tới sân không có nghĩa đã cho in ảnh; Nhi không chụp mới vẫn có thể đồng ý ảnh buổi tập. `no/unasked` không thể được AI hay điểm quan hệ ghi đè.

### 6.4. Intent S03 và luồng xử lý

Danh sách ban đầu dưới đây là đề xuất để TV1/TV4 duyệt ở W4.1:

| Intent | Đối tượng trong S03 | Hành vi sau kiểm luật |
| --- | --- | --- |
| `ASK_SOURCE` | PHOTO_0441/0442/0443 | Hỏi/mở nguồn được phép. |
| `ASK_COMPOSITION_REASON` | PHOTO_0443 | Hạ giải thích lựa chọn ảnh, không thú nhận một việc cắt Nhi không tồn tại. |
| `CHECK_VISIBILITY` | PHOTO_0441/0442/0443 | Phản hồi đúng những người trong ảnh đang có. |
| `COMPARE_PHOTOS` | Cặp ảnh trong tập cho phép | Mở chế độ đối chiếu; người chơi quan sát. |
| `ASK_CAMERA_HANDOFF` | PHOTO_0443 | Trả theo kiến thức NPC/nguồn đã mở; không tiết lộ video chưa biết. |
| `ASK_QUOTE` | VIDEO_VY | Nếu NPC chưa biết nguyên văn thì dùng câu chưa chắc; đề nghị tìm nguồn hợp lệ. |
| `CLARIFY` / `UNSUPPORTED` | Không rõ/ngoài phạm vi | Hỏi lại hoặc đưa nút; không sửa state ngoài lệnh hợp lệ. |

Không đưa `PUBLISH`, `SET_CONSENT`, `SET_ENDING`, `REWRITE_EVIDENCE` vào danh sách được nhận qua AI. Bộ corpus vẫn có câu nhiều ý, nhưng kết hợp các yêu cầu **trong S03**, ví dụ “Mở ảnh gốc 0443 rồi so với 0442, đừng gửi cho ai”; không mở rộng âm thầm sang sửa trang S05.

```text
UI A/B/C
  -> tạo IntentRequest với snapshot ngữ cảnh được phép
  -> IIntentRecognizer (nút / quy tắc / HTTP)
  -> validate JSON + allowlist + scene + target + ambiguity
  -> kiểm knowledge, nguồn, quyền và điều kiện hiện tại bằng luật game
  -> hỏi rõ nếu cần / từ chối / fallback
  -> CommandDispatcher thực hiện lệnh hợp lệ
  -> ResponseCatalog chọn thoại được TV1 duyệt
  -> lưu sự kiện và log đo, UI cập nhật
```

Request đề xuất: `request_id`, `run_id` ẩn danh, `scene_id`, `npc_id`, `utterance`, `allowed_intents`, `allowed_targets`, `visible_evidence_ids`, `permitted_fact_ids`, `recent_turns`, `schema_version`. Không gửi toàn bộ kho bí mật hoặc lịch sử không cần thiết cho dịch vụ.

Response đề xuất:

```json
{
  "schema_version": 1,
  "request_id": "s03-0007",
  "scene_id": "S03",
  "npc_id": "HA",
  "intents": [
    {"id": "ASK_SOURCE", "target": "PHOTO_0443"},
    {"id": "COMPARE_PHOTOS", "target": "PHOTO_0443__PHOTO_0442"}
  ],
  "ambiguity": false,
  "requires_confirmation": false
}
```

Đích ghép `PHOTO_0443__PHOTO_0442` phải có trong allowlist cặp ảnh của scene; không nhận chuỗi target tùy ý. `requires_confirmation` từ mô hình chỉ là gợi ý; game tự quyết định kiểm/xác nhận cần thiết. Không có field `state_patch`, `npc_dialogue` hoặc `ending` được thực thi từ model.

Mỗi lượt có request ID và state revision. Đổi cảnh/load/chuyển chế độ thì hủy request; response đến trễ sai revision bị bỏ. Không thực hiện một phần batch trước khi kiểm toàn bộ: nếu mơ hồ hoặc có intent không hợp lệ, hỏi rõ hoặc chỉ đề xuất các phần hợp lệ để người chơi chọn. Chỉ sau đó dispatch. Các thay đổi quan trọng cần preview/xác nhận theo use case, không dựa vào mô hình tự đánh giá.

Thông số trong PLAN `03_Ban_giao!A35:E36`: p95 ≤ 3 giây, timeout 5 giây là **đề xuất vận hành chưa được chứng minh/chốt**. Lưu chúng trong ExperimentConfig để nhóm xác nhận; hết timeout về nút, không tự retry vô hạn. Trần chi phí chưa có số nguồn nên để trạng thái chưa chốt. Khóa API chỉ ở môi trường dịch vụ; không nhúng vào build Unity, JSON, log hay Git.

## 7. Ánh xạ cảnh sang phần phải code

| Scene ID | Nội dung và tương tác tối thiểu | Module / dữ liệu chính | Owner |
| --- | --- | --- | --- |
| S00 | Kê máy, chọn cử chỉ/album, hẹn giờ, xem đủ bốn + hai ảnh hỏng, sơ đồ. | Courtyard + CameraRigView; `scenes/S00.json`; Photos/Props. | TV1/TV2/TV3 |
| S01 | So trang 16/18, ảnh, tin nhắn; giải thích Minh rút trang theo lời An. | Investigation + EvidenceDesk; S01; nguồn tin nhắn. | TV1/TV2 |
| S02 | Kiểm đồ với Nhi, kẹp và lịch; hỏi sớm được đáp thật; gộp ngắn trong MVP. | Investigation + Scheduling; S02; witness events. | TV1/TV2 |
| S03 | So ba ảnh và kẹp trống; Hạ có kiến thức theo nguồn; điểm thí nghiệm A/B/C. | Evidence + IntentPipeline; Intents/S03; fixtures chung. | TV1/TV2/TV3/TV4 |
| S04 | Video/animatic, kéo mốc/so ảnh, phụ đề; An đưa máy và hứa lát chụp đủ. | Media/VideoView; knowledge rules; S04. | TV1/TV2/TV3 |
| S05 | Chọn ba mẫu, ảnh/chữ, preview và người nhận; gửi có chủ đích. | Drafting; DraftVersion/RecipientHistory; S05. | TV1/TV2/TV3 |
| S06 | Đối thoại Hạ, chọn cách dùng 0443, phản hồi nháp/thu hồi khi có. | Drafting + Consent; `ha_support`; S06. | TV1/TV2 |
| S07 | Hỏi Nhi, nhận lời/chờ/từ chối; phân biệt lời mời và quyền tư liệu. | Invite/Attendance/Consent; S07. | TV1/TV2 |
| S08 | Kiểm máy/kẹp/pin, chia việc, dừng bản cũ, bàn giao/gia hạn nếu cần. | Courtyard + StoryClock/PrintSlot; S08. | TV1/TV2/TV3 |
| S09_A | Nhi còn ở sân; đổi khung, gỡ bảng nếu tồn tại, hẹn giờ, chọn ảnh/quyền. | Courtyard + CapturePhoto/Consent; S09_A. | TV1/TV2/TV3 |
| S09_B | Nhi vắng/chưa nhận lời/đã rời; chụp ba, thu đồ hoặc đề nghị hẹn khác. | Courtyard + PackEquipment/LaterMeeting; S09_B. | TV1/TV2/TV3 |
| S10 | Chọn nội dung thật được dùng, xác định ngày/giao, preview và lưu ảnh trang. | Publishing + FinalPage/DeliveryPlan; S10. | TV1/TV2/TV3 |
| E_A/B/C | Xem/giao hiện vật, hoàn thành việc hẹn và giao bổ sung nếu có; không chấm điểm. | EndingRules + EndingPresenter; E_A/B/C. | TV1/TV2/TV3 |

Một số cảnh rút gọn/gộp trong runtime vẫn giữ ID nguồn và log để TV1 kiểm nội dung, TV5 đo được thời lượng S00/S04/S09. Giữ thứ tự tuyến chính S00 trước S01 theo tài liệu; cold-open trên bàn tư liệu ở bản thử là thay đổi nhịp cần TV1 duyệt nếu muốn kế thừa.

## 8. Cách chuyển từ CIVI cũ sang CIVI_2

1. Tạo project Unity ở `CIVI_2/Game` từ Assets/Packages/ProjectSettings cần thiết của CIVI hiện có; không chép cache, save hoặc thư mục Git. Giữ nguyên bản gốc làm mốc so sánh. Thêm Unity Test Framework khi bắt đầu triển khai kiểm thử, vì manifest hiện tại chưa khai báo gói này.
2. Chạy lại baseline hiện có trước khi sửa. Lưu phiên bản nguồn và kết quả thật; ghi chú release cũ chỉ là lịch sử do tác giả ghi, không thay thế kiểm trên CIVI_2.
3. Tách `Run/Beat/Story` sang Domain và Application; `Player` sang presenters/services; tách bootstrap khỏi UI. Ban đầu vẫn chạy được bằng nút trước khi đổi toàn bộ dữ liệu.
4. Chuyển nội dung sang JSON có scene/beat ID, schema và validator; đối chiếu từng scene với STORY/PLAN. Mỗi bước có fixture đầu vào và đường hoàn thành.
5. Bổ sung invite pending, attendance, consent theo asset, nháp/recipients/thu hồi, thiết bị, lịch và hai đường sân. Chỉnh điều kiện kết theo sản phẩm thực tế; tách hỗ trợ Hạ khỏi giữ ảnh.
6. Nạp và kiểm asset ID, ảnh hỏng, sơ đồ, animatic/phụ đề; giữ giấy phép font và nguồn tài nguyên. UI ưu tiên prefab/presenter để TV3 chỉnh được, tránh tiếp tục dồn mọi màn vào một file.
7. Thêm export trang cuối: render trang 17 vào texture/PNG, ghi manifest nội dung/ngày/người/quyền đã kiểm; báo vị trí lưu và lỗi ghi file. Đây là xuất hiện vật trong máy người chơi, khác với thao tác gửi nhà in trong truyện.
8. Khi luồng A ổn định mới nối B rồi C ở S03. Không để HTTP hoặc cấu hình dịch vụ cản game khởi chạy. Dùng cùng snapshot và luật để so sánh.
9. Chạy matrix nhánh/quyền/save/export, đóng gói và kiểm máy khác. Đổi toàn bộ đường dẫn build, ID sản phẩm, save, launcher/version/release URL và checksum cho CIVI_2 trước phát hành; không để bộ cài tải bản 0.41.1 cũ.

Để triển khai nhanh, ưu tiên tạo cây làm việc theo thứ tự: `contracts` + docs chuẩn; `Domain/Application` + fixture; `Presentation` + content S00–S04; nháp/mời/sân/kết; cuối cùng dịch vụ và đánh giá AI. Không cần tạo trước mọi file rỗng trong cây.

## 9. Kiểm thử và điều kiện nhận bản

### 9.1. Kiểm tự động có ý nghĩa

| Nhóm | Ca tối thiểu |
| --- | --- |
| Nội dung | ID không trùng, target/text/asset tồn tại, mọi lựa chọn có đường hợp lệ; JSON đúng schema; không có bẫy bắt người hiểu sớm quay lại giả vờ chưa biết. |
| Hai đường sân | Accepted + còn ở sân; pending; declined; accepted nhưng rời trước chụp; Hạ chỉ bàn giao; thiếu kẹp/pin có cách xử lý; nhánh vắng vẫn có thao tác hoàn tất. |
| Quyền | Cho xem video nhưng không cho công bố; đồng ý ảnh cũ nhưng không ảnh mới; nhận lời chụp nhưng từ chối in một ảnh; đổi nội dung phải kiểm lại. |
| Nháp | Chưa gửi thì không có người nhớ; gửi nhóm mới sinh bảng nếu có sự kiện chuẩn bị; thu hồi không xóa người đã xem; gỡ bảng rồi không tự xuất hiện lại. |
| Thời gian | Đọc/xem không tiến giờ; hành động có thời lượng mới tiến; vượt giờ Nhi đi phải chuyển đúng đường; không gia hạn nếu chưa xác nhận trước mốc. |
| Kết | Ảnh ba người không ghi đủ bốn; ảnh chụp nhưng giữ riêng có thể kết B; đề nghị hẹn chưa xác nhận không mở E-C; supplement phải có thao tác hoàn thành. |
| Save/load | Round trip ở S03, sau gửi/thu hồi nháp, trước chụp và trước chốt; file sai/version lạ không phá state; không tự suy quyền cho save cũ. |
| Intent | Câu đồng nghĩa, nhiều ý, mơ hồ, bịa sự thật, lệnh bỏ luật, target sai, JSON sai, mạng lỗi, timeout, response trễ sau load/chuyển cảnh. |
| UI/export | Bàn phím và click không dispatch hai lần; modal chặn input nền; phụ đề/font đủ dấu; ảnh export đúng hiện vật, không bị UI menu che. |

Các checks route của CIVI cũ là điểm bắt đầu, không đủ thay matrix trên. Tests và CI chưa được tạo/chạy trong lần viết tài liệu này.

### 9.2. Đánh giá A/B/C và playtest

- Tách tập dev/test; ghi version content, parser/prompt, model và config. Không chỉnh prompt trên test rồi báo kết quả lại như tập độc lập.
- Dữ liệu log tối thiểu: session ẩn danh, mode, scene, request/state revision, intent/target dự kiến và thực, validation result, fallback reason, latency, số gọi, usage và chi phí thực. Văn bản gõ thô chỉ lưu khi cần và đã được cho phép trong quy trình thử.
- Đo riêng hiểu intent/target, lỗi nghiêm trọng về nguồn/quyền/state, p95, tỷ lệ fallback và chi phí/lượt. A đo thời gian thao tác; không ép có chỉ số hiểu ngôn ngữ như B/C.
- Playtest theo PLAN: ít nhất 5 bạn nữ và thêm người khác nếu được; hoán đổi thứ tự A/B/C, bắt đầu cùng snapshot; quan sát hoàn thành, hỏi lại, chủ động, điểm kẹt, cảm nhận Hạ và độ đầy đủ nhánh vắng. Không gợi câu trả lời đạo đức.
- Giữ dữ liệu thô trong `local-data`, báo cáo tổng hợp vào `evaluation/reports`, log/video sinh vào `artifacts`; không commit thông tin nhận diện người thử.
- Giữ AI chỉ khi có lợi ích quan sát được hơn cả A và B, không lỗi nghiêm trọng trên bộ thử, độ trễ/chi phí phù hợp ngưỡng nhóm đã chốt. Mẫu nhỏ không chứng minh tác động giáo dục diện rộng.

### 9.3. Điều kiện nhận MVP

- [ ] Máy Windows khác mở được, không cài Unity; chạy offline trọn hai đường sân.
- [ ] S00 có ảnh đủ bốn, hai ảnh hỏng, cử chỉ và sơ đồ; 0443 không có Nhi.
- [ ] Người hiểu sớm đi tiếp được; Hạ chỉ bàn giao vẫn hoàn tất được.
- [ ] Pending/declined không tự thành có mặt; rời sớm đổi đúng nhánh.
- [ ] Có ít nhất một hệ quả từ việc đã làm nhìn thấy trên nháp/đạo cụ/thoại/phần việc/trang cuối.
- [ ] Chọn ảnh và chốt trang kiểm đúng quyền; xem nội bộ không tự thành công bố.
- [ ] Nhánh vắng có chụp ba/thu đồ/đề nghị hẹn; không cắt ngay tới tổng kết.
- [ ] Trang cuối được lưu PNG thành công, đúng người/ngày/nguồn; lỗi export có thể thử lại.
- [ ] Save/load giữ đúng nhánh, thời gian, quyền và lịch sử nháp.
- [ ] S03 có A/B dùng chung luật; C hoặc hoạt động có kiểm soát hoặc tắt, lỗi mạng vẫn về nút.
- [ ] Build không có API key; hướng dẫn, giấy phép và gói offline đầy đủ.
- [ ] TV5 và người nhận kiểm được đầu ra trước khi cập nhật “Đã tích hợp”.

## 10. Tổ chức nhóm, mốc và giới hạn tiến độ

| Owner theo XLSX | Phạm vi sở hữu chính | Việc / giờ ước lượng |
| --- | --- | --- |
| TV1 | `docs/story`, content, facts/knowledge/thoại, duyệt intent responses | 8 việc / 44 giờ |
| TV2 | Unity runtime, schemas tích hợp, luật quyền, save/load, sân/kết, build | 8 việc / 54 giờ |
| TV3 | `source-assets`, Art/Audio/Video/Fonts, UI prefab, demo | 8 việc / 47 giờ |
| TV4 | Intents/S03, recognizers, intent-api, evaluation, số đo/chi phí | 8 việc / 45 giờ |
| TV5 | production/qa/business/submission, playtest, hồ sơ, bằng chứng nộp | 8 việc / 44 giờ |

Tổng 234 giờ là **ước lượng của XLSX**, không phải số giờ đã làm. Tên người còn trống, 40 trạng thái đầu việc đều “Chưa cập nhật”; không tự suy đã hoàn thành dù mã thử có chức năng tương tự. TV2 có tải lớn nhất, TV4 hỗ trợ kiểm thử/tích hợp sau adapter theo kế hoạch.

| Mốc PLAN | Lịch kế hoạch | Kết quả / cách xử lý |
| --- | --- | --- |
| G0 | 18–20/10 | Khóa v4.1, S03, schema, tài sản mẫu. Tại ngày phân tích 21/10, mốc này đã qua nhưng chưa có xác nhận hoàn thành trong workbook. |
| G1 | 21–24/10 | S03 có A/B, C thử nếu sẵn; nội dung MVP sẵn. C chưa ổn không chặn A/B. |
| G2 | 25–28/10 | Hai đường sân, playtest, đo A/B/C; ưu tiên thao tác đường vắng. |
| G3 | 29–31/10 | Giữ/thu hẹp/tắt AI, sửa lỗi nghiêm trọng, khóa nội dung. |
| G4 | 01–04/11 | Build ổn, video, hồ sơ và diễn tập; dự phòng offline. |
| G5 | 05/11 | Mục tiêu nộp nội bộ 20:00, lưu xác nhận trước 23:59 theo tài liệu nguồn. |

Hạn 05/11/2026 23:59 được ghi trong PDF AI/XLSX, **chưa kiểm với thể lệ bên ngoài**; W5.1 vẫn cần kiểm nguồn cuộc thi. Đây là phân tích tài liệu nội bộ, không xác nhận hạn cuộc thi hiện hành.

Hai phụ thuộc cần làm rõ khi triển khai:

1. W3.8 cần bản khóa W2.8 ngày 01/11 để quay, nhưng hạn cuối W2.8 là 05/11. Nên giao release candidate ngày 01/11, giữ W2.8 cho kiểm và đóng gói cuối; ghi cả hai mốc thay vì chờ 05/11 mới quay.
2. W1.8 kết thúc 03/11 dù G3 ghi khóa nội dung 31/10. Nên khóa nội dung chơi ngày 31/10, W1.8 sau đó duyệt mô tả/phụ đề hồ sơ và chỉ sửa lỗi được chấp thuận. Không coi khoảng này là thời gian thêm cảnh.

Các lựa chọn phải chốt trước triển khai: TV1/TV2 duyệt scope/mapping v4.1; TV4/TV5 chốt ngưỡng và ngân sách AI; TV3/TV2 chốt media/asset format; TV5 gán tên và lịch thực tế. Các điểm này không cản bắt đầu nền runtime/offline và contract mẫu.

## 11. Ma trận đủ 40 đầu việc từ XLSX

Mỗi hàng bên dưới giữ nguyên mã, tên, hạn, phụ thuộc, sản phẩm và tiêu chí nguồn; thêm đường dẫn đề xuất để người code biết đặt phần việc ở đâu. Tất cả hiện vẫn là yêu cầu cần kiểm, không phải xác nhận đã triển khai. Sheet nguồn: `02_Cong_viec`, hàng 7–46. Các đường dẫn trong ma trận đều tương đối từ `CIVI_2`; `Content/` viết tắt cho `Game/Assets/_Civi/Content/`, `Scripts/` cho `Game/Assets/_Civi/Scripts/`.


### TV1 - Kịch bản và thiết kế chơi

| Mã / dòng nguồn | Đầu việc, nơi đặt đầu ra | Bàn giao và điều kiện đạt | Phụ thuộc / hạn / kiểm nhận |
| --- | --- | --- | --- |
| **W1.1**<br>hàng 7 | Đối chiếu AI 2.1 với truyện v4.1; khóa phạm vi MVP<br>**Đặt tại:** `docs/story/canon-v4.1.md; docs/design/mvp-scope.md` | **Đầu ra:** Bảng thay thế chi tiết v3; danh sách cảnh giữ/rút gọn<br>**Đạt khi:** Loại bảng ca/ghép chứng từ; ảnh 0443 thật có ba người; chọn S03 để thử AI. | **Phụ thuộc:** Không<br>**Hạn nguồn:** 2026-10-19<br>**Giờ dự kiến:** 5<br>**Kiểm/nhận:** TV2 + TV4 |
| **W1.2**<br>hàng 8 | Khóa trạng thái, quyền ảnh/video, kiến thức NPC<br>**Đặt tại:** `docs/design/state-and-consent.md; docs/story/npc-knowledge.md; Content/Story/v4_1/facts.json` | **Đầu ra:** Bảng trạng thái + kho sự thật v4.1 + quyền theo cảnh<br>**Đạt khi:** Phân biệt nhận lời, có mặt và cho dùng ảnh; NPC chỉ biết nguồn đã được phép. | **Phụ thuộc:** W1.1<br>**Hạn nguồn:** 2026-10-20<br>**Giờ dự kiến:** 5<br>**Kiểm/nhận:** TV2 + TV4 |
| **W1.3**<br>hàng 9 | Viết gói cảnh S00/S01 và S02 rút gọn<br>**Đặt tại:** `Content/Story/v4_1/scenes/S00.json, S01.json, S02.json; Content/Story/v4_1/dialogue/` | **Đầu ra:** Thoại, lựa chọn, chuyển cảnh, chỉ dẫn đạo cụ<br>**Đạt khi:** S00 có đủ bốn, hai ảnh hỏng, cử chỉ; S01 giải thích Minh rút trang theo tin An. | **Phụ thuộc:** W1.1<br>**Hạn nguồn:** 2026-10-21<br>**Giờ dự kiến:** 6<br>**Kiểm/nhận:** TV2 + TV3 |
| **W1.4**<br>hàng 10 | Viết S03/S04 và phản hồi câu tự do của Hạ<br>**Đặt tại:** `Content/Story/v4_1/scenes/S03.json, S04.json; Content/Intents/S03/responses.json` | **Đầu ra:** Cảnh ảnh/video + phản hồi theo intent và trạng thái<br>**Đạt khi:** Không kéo Nhi ra từ 0443; video cho thấy An đưa máy và quên lời hẹn; không lộ nguồn chưa xem. | **Phụ thuộc:** W1.2<br>**Hạn nguồn:** 2026-10-22<br>**Giờ dự kiến:** 6<br>**Kiểm/nhận:** TV4 + TV2 |
| **W1.5**<br>hàng 11 | Viết S05/S06/S07 và lựa chọn thu hồi nháp<br>**Đặt tại:** `Content/Story/v4_1/scenes/S05.json, S06.json, S07.json; Content/Drafts/` | **Đầu ra:** Ba mẫu nháp; phản hồi Nhi; tranh luận Hạ; lời mời<br>**Đạt khi:** Hiểu sớm không bị ép sai; thu hồi nháp cần báo người từng xem; Hạ có thể chỉ bàn giao. | **Phụ thuộc:** W1.2<br>**Hạn nguồn:** 2026-10-23<br>**Giờ dự kiến:** 6<br>**Kiểm/nhận:** TV2 + TV5 |
| **W1.6**<br>hàng 12 | Viết S08/S09-A/S09-B/S10 và ảnh kết MVP<br>**Đặt tại:** `Content/Story/v4_1/scenes/S08.json, S09_A.json, S09_B.json, S10.json; ending-rules.json` | **Đầu ra:** Hai đường ở sân; ba lựa chọn khi Nhi vắng; trang kết<br>**Đạt khi:** Nhi từ chối/chưa nhận lời không tự xuất hiện; nhánh vắng vẫn có thao tác và hiện vật cuối. | **Phụ thuộc:** W1.5<br>**Hạn nguồn:** 2026-10-24<br>**Giờ dự kiến:** 7<br>**Kiểm/nhận:** TV2 + TV3 |
| **W1.7**<br>hàng 13 | Sửa nhịp truyện theo quan sát người thử<br>**Đặt tại:** `Content/Story/v4_1/; docs/story/dialogue-review.md; docs/production/decisions/` | **Đầu ra:** Gói thoại/cảnh đã sửa có mã phiên bản<br>**Đạt khi:** Hạ không bị hiểu thành phản diện đơn giản; hai nhánh sân có hệ quả nhìn thấy; không giáo huấn. | **Phụ thuộc:** W5.3; W5.4<br>**Hạn nguồn:** 2026-10-29<br>**Giờ dự kiến:** 5<br>**Kiểm/nhận:** TV5 + TV2 |
| **W1.8**<br>hàng 14 | Khóa nội dung, duyệt video và mô tả tính năng<br>**Đặt tại:** `docs/submission/demo-script.md; docs/story/dialogue-review.md; artifacts/demo/` | **Đầu ra:** Nội dung cuối + lời giới thiệu và phụ đề đã duyệt<br>**Đạt khi:** Video và hồ sơ phản ánh bản chạy thật; giữ nguyên sự kiện, quyền từ chối và giới hạn MVP. | **Phụ thuộc:** W1.7; W5.5<br>**Hạn nguồn:** 2026-11-03<br>**Giờ dự kiến:** 4<br>**Kiểm/nhận:** TV5 |

### TV2 - Lập trình và tích hợp

| Mã / dòng nguồn | Đầu việc, nơi đặt đầu ra | Bàn giao và điều kiện đạt | Phụ thuộc / hạn / kiểm nhận |
| --- | --- | --- | --- |
| **W2.1**<br>hàng 15 | Chọn nền tảng game; chốt dữ liệu và mẫu tích hợp<br>**Đặt tại:** `Game/; contracts/; contracts/examples/; docs/engineering/data-contracts.md` | **Đầu ra:** Dự án chạy được; scene/state/intent schemas; ví dụ JSON<br>**Đạt khi:** TV1/3/4 nạp cùng mẫu được; giữ nền tảng hiện có nếu có; không xây lại engine. | **Phụ thuộc:** W1.1 (bản nháp)<br>**Hạn nguồn:** 2026-10-20<br>**Giờ dự kiến:** 5<br>**Kiểm/nhận:** TV1 + TV4 |
| **W2.2**<br>hàng 16 | Làm bộ chạy cảnh, trạng thái và lưu/đọc tiến độ<br>**Đặt tại:** `Scripts/Domain/State, Consent, Scheduling; Scripts/Application/Flow; Scripts/Infrastructure/Persistence` | **Đầu ra:** Runtime, save/load, lịch sự kiện và kiểm quyền<br>**Đạt khi:** Pending không thành accepted; đọc thoại không làm thời gian truyện tự trôi; lưu/đọc giữ đúng nhánh. | **Phụ thuộc:** W2.1; W1.2<br>**Hạn nguồn:** 2026-10-22<br>**Giờ dự kiến:** 8<br>**Kiểm/nhận:** TV1 + TV5 |
| **W2.3**<br>hàng 17 | Ghép S00/S01/S02 ngắn và S03/S04<br>**Đặt tại:** `Scripts/Presentation/Dialogue, Evidence; Scripts/Infrastructure/Media; Content/Story/v4_1/scenes/` | **Đầu ra:** Bản đầu chơi được bằng nút, xem ảnh và video<br>**Đạt khi:** Đủ bốn ở mở đầu; vật chứng mở được theo luật; placeholder được thay khi tài sản bàn giao. | **Phụ thuộc:** W2.1; W1.3; W1.4<br>**Hạn nguồn:** 2026-10-23<br>**Giờ dự kiến:** 8<br>**Kiểm/nhận:** TV1 + TV3 |
| **W2.4**<br>hàng 18 | Ghép dựng nháp, phản hồi, Hạ và lời mời Nhi<br>**Đặt tại:** `Scripts/Application/Drafting; Scripts/Presentation/Drafting; Scripts/Domain/Drafts, Consent, Scheduling` | **Đầu ra:** S05/S06/S07 với bản xem trước và người nhận<br>**Đạt khi:** Không tự gửi nhóm lớp; người đã xem nháp vẫn nhớ; chỉ bàn giao vẫn cho phép đi tiếp. | **Phụ thuộc:** W2.2; W1.5<br>**Hạn nguồn:** 2026-10-24<br>**Giờ dự kiến:** 7<br>**Kiểm/nhận:** TV1 + TV5 |
| **W2.5**<br>hàng 19 | Ghép hai nhánh sân và trang kết rút gọn<br>**Đặt tại:** `Scripts/Application/Courtyard, Publishing; Scripts/Domain/Endings; Scripts/Infrastructure/Export` | **Đầu ra:** S08/S09-A/S09-B/S10 + lưu ảnh kết<br>**Đạt khi:** Nhi vắng có chụp ba người/thu đồ/đề nghị hẹn khác; đúng người và phạm vi ảnh; đủ luồng 12–20 phút để thử. | **Phụ thuộc:** W2.2; W1.6<br>**Hạn nguồn:** 2026-10-25<br>**Giờ dự kiến:** 8<br>**Kiểm/nhận:** TV1 + TV5 |
| **W2.6**<br>hàng 20 | Kết nối ba chế độ A/B/C và dự phòng mất mạng<br>**Đặt tại:** `Scripts/Application/Intent; Scripts/Integrations/Intent; Game/Assets/_Civi/Config/ExperimentConfig` | **Đầu ra:** Một cảnh S03 dùng chung luật cho A/B/C<br>**Đạt khi:** A = nút; B = quy tắc; C = AI. AI lỗi vẫn quay lại nút, giữ nháp và tiến độ. | **Phụ thuộc:** W4.2; W4.3; W4.4<br>**Hạn nguồn:** 2026-10-25<br>**Giờ dự kiến:** 5<br>**Kiểm/nhận:** TV4 + TV5 |
| **W2.7**<br>hàng 21 | Sửa lỗi nhánh, quyền ảnh và lỗi tích hợp<br>**Đặt tại:** `Game/Assets/_Civi/Tests/; docs/qa/branch-matrix.csv; artifacts/qa/` | **Đầu ra:** Bản ổn định + kết quả chạy lại hai nhánh<br>**Đạt khi:** Không có lỗi chặn hoàn thành, dùng ảnh trái quyền hay sai sự thật trên bộ thử; kiểm lại save/load. | **Phụ thuộc:** W5.3; W5.4<br>**Hạn nguồn:** 2026-10-31<br>**Giờ dự kiến:** 8<br>**Kiểm/nhận:** TV5 + TV1 |
| **W2.8**<br>hàng 22 | Đóng gói, hướng dẫn chạy và kiểm bản nộp<br>**Đặt tại:** `tools/Build-Windows.ps1; tools/Package-Windows.ps1; tools/Launcher/; release/; README.md` | **Đầu ra:** Bản build cuối; hướng dẫn; bản dự phòng offline<br>**Đạt khi:** Máy khác khởi chạy và hoàn tất hai đường; gói nộp không chứa khóa API; khóa bản trước quay video. | **Phụ thuộc:** W2.7; W4.7<br>**Hạn nguồn:** 2026-11-05<br>**Giờ dự kiến:** 5<br>**Kiểm/nhận:** TV5 |

### TV3 - Hình ảnh, UI và âm thanh

| Mã / dòng nguồn | Đầu việc, nơi đặt đầu ra | Bàn giao và điều kiện đạt | Phụ thuộc / hạn / kiểm nhận |
| --- | --- | --- | --- |
| **W3.1**<br>hàng 23 | Chốt phong cách, danh mục và quy cách tài sản<br>**Đặt tại:** `docs/design/asset-list.csv; Content/Manifests/asset-manifest.json; source-assets/` | **Đầu ra:** Asset list có ID, kích thước, quyền dùng, nơi dùng lại<br>**Đạt khi:** Ưu tiên ảnh S00, 0443, sơ đồ, phòng thiết kế và sân; thống nhất format với TV2. | **Phụ thuộc:** W1.1; W2.1 (bản nháp)<br>**Hạn nguồn:** 2026-10-20<br>**Giờ dự kiến:** 4<br>**Kiểm/nhận:** TV1 + TV2 |
| **W3.2**<br>hàng 24 | Làm ảnh mở đầu và sơ đồ tia chớp<br>**Đặt tại:** `Game/Assets/_Civi/Art/Photos, Props; source-assets/art/` | **Đầu ra:** Ảnh đủ bốn, hai ảnh hỏng, sơ đồ hai ô, bảng HẸN<br>**Đạt khi:** Nhân vật/đạo cụ nhất quán; opening_gesture có biến thể hiển thị; nguồn tài sản rõ. | **Phụ thuộc:** W3.1; W1.3<br>**Hạn nguồn:** 2026-10-22<br>**Giờ dự kiến:** 7<br>**Kiểm/nhận:** TV1 |
| **W3.3**<br>hàng 25 | Làm bộ 0441/0442/0443 và đoạn video điều tra<br>**Đặt tại:** `Game/Assets/_Civi/Art/Photos; Game/Assets/_Civi/Video; source-assets/video/` | **Đầu ra:** Ba ảnh, cận kẹp trống, video/animatic có âm và phụ đề<br>**Đạt khi:** 0443 thực sự có ba người; video kể được chín giây và lời An; không dùng tư liệu chưa có quyền. | **Phụ thuộc:** W3.1; W1.4<br>**Hạn nguồn:** 2026-10-23<br>**Giờ dự kiến:** 8<br>**Kiểm/nhận:** TV1 + TV2 |
| **W3.4**<br>hàng 26 | Làm giao diện thoại, ảnh, ba mẫu nháp và bản xem trước<br>**Đặt tại:** `Game/Assets/_Civi/Art/UI, Prefabs; Scripts/Presentation/Drafting; Content/Drafts/` | **Đầu ra:** UI kit và mẫu tribute/paired/memory, trạng thái loading/lỗi<br>**Đạt khi:** Chữ dễ đọc; thấy người nhận/phạm vi trước gửi; nút dự phòng dễ tìm; không hiện điểm đạo đức. | **Phụ thuộc:** W3.1; W1.5<br>**Hạn nguồn:** 2026-10-24<br>**Giờ dự kiến:** 7<br>**Kiểm/nhận:** TV2 + TV1 |
| **W3.5**<br>hàng 27 | Làm sân, đạo cụ và ảnh kết hai đường<br>**Đặt tại:** `Game/Assets/_Civi/Art/Backgrounds, Props, Photos; Game/Assets/_Civi/Prefabs/` | **Đầu ra:** Phòng thiết kế, sân, pin/kẹp/bảng, ảnh bốn/ba/không ảnh mới<br>**Đạt khi:** Bảng cảm ơn chỉ hiện khi đã chuẩn bị và chưa gỡ; nhánh vắng có hình kết riêng. | **Phụ thuộc:** W1.6; W3.2<br>**Hạn nguồn:** 2026-10-25<br>**Giờ dự kiến:** 7<br>**Kiểm/nhận:** TV1 + TV2 |
| **W3.6**<br>hàng 28 | Âm thanh, phụ đề và ghép tài sản vào build<br>**Đặt tại:** `Game/Assets/_Civi/Audio, Video, Fonts; licenses/; Scripts/Infrastructure/Media` | **Đầu ra:** Gói tài sản cuối + âm hẹn giờ + phụ đề kiểm tra<br>**Đạt khi:** Không thiếu file/font/âm; âm không che thoại; mọi tài sản chính đã được TV2 nạp thử. | **Phụ thuộc:** W3.2; W3.3; W3.4; W3.5<br>**Hạn nguồn:** 2026-10-26<br>**Giờ dự kiến:** 4<br>**Kiểm/nhận:** TV2 + TV5 |
| **W3.7**<br>hàng 29 | Sửa độ rõ, nhịp nhìn và tính nhất quán từ playtest<br>**Đặt tại:** `Game/Assets/_Civi/Prefabs, Art/UI; Scripts/Presentation/; artifacts/qa/` | **Đầu ra:** UI/ảnh/âm đã chỉnh trên bản tích hợp<br>**Đạt khi:** Người thử tìm được thao tác; ảnh/video dễ so; nhánh Nhi vắng không trông như thiếu nội dung. | **Phụ thuộc:** W5.3; W3.6<br>**Hạn nguồn:** 2026-10-31<br>**Giờ dự kiến:** 4<br>**Kiểm/nhận:** TV5 + TV1 |
| **W3.8**<br>hàng 30 | Quay, dựng video demo và hình cho hồ sơ<br>**Đặt tại:** `source-assets/video/; artifacts/demo/; docs/submission/demo-script.md` | **Đầu ra:** Video, phụ đề, ảnh chụp build thật<br>**Đạt khi:** Thể hiện hai hệ quả; AI chỉ giới thiệu nếu còn trong build; thời lượng/định dạng theo thể lệ TV5 kiểm. | **Phụ thuộc:** W5.5; W2.8 (bản khóa 01/11)<br>**Hạn nguồn:** 2026-11-04<br>**Giờ dự kiến:** 6<br>**Kiểm/nhận:** TV1 + TV5 |

### TV4 - AI và đánh giá kỹ thuật

| Mã / dòng nguồn | Đầu việc, nơi đặt đầu ra | Bàn giao và điều kiện đạt | Phụ thuộc / hạn / kiểm nhận |
| --- | --- | --- | --- |
| **W4.1**<br>hàng 31 | Chuyển đặc tả AI sang S03 của v4.1<br>**Đặt tại:** `contracts/intent-*.schema.json; Content/Intents/S03/; docs/story/npc-knowledge.md` | **Đầu ra:** Danh sách intent, target, schema và phản hồi được duyệt<br>**Đạt khi:** Bỏ ví dụ bảng ca/ghi công v3; không kết luận Hạ xóa Nhi khỏi 0443; không làm thêm cảnh AI thứ hai. | **Phụ thuộc:** W1.1; W1.2<br>**Hạn nguồn:** 2026-10-20<br>**Giờ dự kiến:** 5<br>**Kiểm/nhận:** TV1 + TV2 |
| **W4.2**<br>hàng 32 | Làm bản B: nhập tự do bằng từ khóa/quy tắc<br>**Đặt tại:** `Scripts/Integrations/Intent/Rules; Content/Intents/S03/rules.json; evaluation/datasets/s03/dev.jsonl` | **Đầu ra:** Bộ nhận diện quy tắc + bộ câu minh họa<br>**Đạt khi:** Không nhận rõ thì hỏi lại; cùng intent và cùng hành động hợp lệ với A/C. | **Phụ thuộc:** W4.1; W2.1<br>**Hạn nguồn:** 2026-10-22<br>**Giờ dự kiến:** 5<br>**Kiểm/nhận:** TV1 + TV2 |
| **W4.3**<br>hàng 33 | Làm bản C: AI trả ý định có cấu trúc<br>**Đặt tại:** `services/intent-api/; Scripts/Integrations/Intent/Remote; contracts/intent-response.schema.json` | **Đầu ra:** AI adapter, cấu hình timeout và validate JSON<br>**Đạt khi:** Chỉ trả mã được phép và ambiguity; khóa API ở phía dịch vụ phù hợp; không trực tiếp sửa state/chứng cứ. | **Phụ thuộc:** W4.1; W2.1<br>**Hạn nguồn:** 2026-10-24<br>**Giờ dự kiến:** 8<br>**Kiểm/nhận:** TV2 |
| **W4.4**<br>hàng 34 | Chặn sai quyền, lộ nguồn và xử lý lỗi<br>**Đặt tại:** `Scripts/Application/Intent/IntentValidator; services/intent-api/src/civi_intent/validation.py; Content/Intents/S03/` | **Đầu ra:** Validator + phản hồi mẫu + đường lui về nút<br>**Đạt khi:** Game kiểm quyền sau AI; lỗi/chậm/JSON sai dùng dự phòng; không tự gửi nháp hoặc công bố ảnh/video. | **Phụ thuộc:** W4.1; W4.3 (bản nháp)<br>**Hạn nguồn:** 2026-10-24<br>**Giờ dự kiến:** 6<br>**Kiểm/nhận:** TV2 + TV1 |
| **W4.5**<br>hàng 35 | Làm bộ thử độc lập và ghi số đo A/B/C<br>**Đặt tại:** `evaluation/datasets/s03/dev.jsonl, test.jsonl; Scripts/Infrastructure/Telemetry; evaluation/scripts/` | **Đầu ra:** Tập câu dev/test tách nhau; log intent/target/lỗi/độ trễ/chi phí<br>**Đạt khi:** Có câu nhiều ý, mơ hồ, sai sự thật, dụ bỏ luật; không dùng câu test để chỉnh rồi báo cùng tập là kết quả mới. | **Phụ thuộc:** W4.1; W4.2; W4.3<br>**Hạn nguồn:** 2026-10-25<br>**Giờ dự kiến:** 6<br>**Kiểm/nhận:** TV5 + TV1 |
| **W4.6**<br>hàng 36 | Chạy đánh giá, phân tích lỗi và đề xuất giữ/thu hẹp/bỏ<br>**Đặt tại:** `evaluation/scripts/; evaluation/reports/; artifacts/evaluation/` | **Đầu ra:** Bảng so A/B/C + số gọi và chi phí/lượt chơi thực đo<br>**Đạt khi:** Đo độ đúng intent/target, thời gian, fallback, lỗi nghiêm trọng; tách số kỹ thuật và nhận xét mẫu nhỏ. | **Phụ thuộc:** W2.6; W4.5; W5.4<br>**Hạn nguồn:** 2026-10-29<br>**Giờ dự kiến:** 6<br>**Kiểm/nhận:** TV5 + TV1 |
| **W4.7**<br>hàng 37 | Chốt phương án AI và cấu hình bản cuối<br>**Đặt tại:** `docs/production/decisions/; Game/Assets/_Civi/Config/ExperimentConfig; docs/engineering/` | **Đầu ra:** Quyết định + cấu hình final + bản không AI dự phòng<br>**Đạt khi:** Giữ khi có lợi ích hơn A/B và đạt ngưỡng đã chốt; thiếu bằng chứng thì thu hẹp hoặc tắt AI. | **Phụ thuộc:** W4.6; W5.5<br>**Hạn nguồn:** 2026-10-31<br>**Giờ dự kiến:** 5<br>**Kiểm/nhận:** TV2 + TV5 |
| **W4.8**<br>hàng 38 | Viết bằng chứng AI, chi phí và hỗ trợ bản nộp<br>**Đặt tại:** `evaluation/reports/; docs/submission/; docs/business/` | **Đầu ra:** Mô tả kỹ thuật, số đo và giới hạn đã kiểm<br>**Đạt khi:** Không gọi state/hint là AI; không bịa đơn giá; không khẳng định hiệu quả giáo dục rộng từ mẫu nhỏ. | **Phụ thuộc:** W4.7<br>**Hạn nguồn:** 2026-11-04<br>**Giờ dự kiến:** 4<br>**Kiểm/nhận:** TV5 |

### TV5 - Điều phối, QA, kinh doanh và hồ sơ

| Mã / dòng nguồn | Đầu việc, nơi đặt đầu ra | Bàn giao và điều kiện đạt | Phụ thuộc / hạn / kiểm nhận |
| --- | --- | --- | --- |
| **W5.1**<br>hàng 39 | Điều phối, xác nhận nguồn lực và yêu cầu hồ sơ<br>**Đặt tại:** `docs/production/task-index.md, milestones.md; docs/submission/requirements-checklist.md` | **Đầu ra:** Gán tên TV1–TV5, lịch rảnh, checklist thể lệ và đầu ra<br>**Đạt khi:** Cả nhóm xác nhận vai trò; kiểm lại hạn 05/11 23:59 từ nguồn cuộc thi; chọn nền tảng hiện có. | **Phụ thuộc:** Không<br>**Hạn nguồn:** 2026-10-20<br>**Giờ dự kiến:** 5<br>**Kiểm/nhận:** Cả nhóm |
| **W5.2**<br>hàng 40 | Lập bộ thử nhánh và kế hoạch A/B/C<br>**Đặt tại:** `docs/qa/branch-matrix.csv, playtest-protocol.md; evaluation/protocols/abc-s03.md` | **Đầu ra:** Test matrix, biểu mẫu quan sát, thứ tự A/B/C hoán đổi<br>**Đạt khi:** Có Nhi accepted/pending/declined/rời sớm; kiểm Hạ chỉ bàn giao, người hiểu sớm, quyền nội bộ và save/load. | **Phụ thuộc:** W1.2; W4.1<br>**Hạn nguồn:** 2026-10-24<br>**Giờ dự kiến:** 6<br>**Kiểm/nhận:** TV1 + TV4 |
| **W5.3**<br>hàng 41 | Chơi thử game, ghi lỗi và kiểm lại bản sửa<br>**Đặt tại:** `local-data/playtests/; artifacts/qa/; docs/qa/issue-template.md` | **Đầu ra:** Ghi chép ít nhất 5 bạn nữ và người khác nếu có; danh sách lỗi<br>**Đạt khi:** Mời theo tài liệu AI; ghi thời lượng, điểm kẹt, Hạ/nhánh vắng; mỗi lỗi có bước lặp lại, owner và mức độ. | **Phụ thuộc:** W2.5; W3.5; W5.2<br>**Hạn nguồn:** 2026-10-28<br>**Giờ dự kiến:** 8<br>**Kiểm/nhận:** TV1 + TV2 + TV3 |
| **W5.4**<br>hàng 42 | Tổ chức so sánh A/B/C trên cùng cảnh S03<br>**Đặt tại:** `local-data/playtests/; artifacts/evaluation/; evaluation/reports/` | **Đầu ra:** Kết quả trải nghiệm theo phiên bản và thứ tự thử<br>**Đạt khi:** Không gợi đáp án về công bằng; ghi tỷ lệ hoàn thành, số hỏi lại, cảm giác chủ động; nêu hạn chế mẫu nhỏ. | **Phụ thuộc:** W2.6; W4.5; W5.2<br>**Hạn nguồn:** 2026-10-28<br>**Giờ dự kiến:** 5<br>**Kiểm/nhận:** TV4 + TV1 |
| **W5.5**<br>hàng 43 | Họp chốt AI, phạm vi và ưu tiên sửa trước khóa build<br>**Đặt tại:** `docs/production/decisions/; docs/production/task-index.md` | **Đầu ra:** Biên bản giữ/thu hẹp/bỏ AI + danh sách lỗi phải sửa<br>**Đạt khi:** Mọi lỗi sai quyền/sự thật và chặn hoàn thành có người sửa; không mở rộng full game lúc này. | **Phụ thuộc:** W5.3; W5.4; W4.6 (bản nháp)<br>**Hạn nguồn:** 2026-10-29<br>**Giờ dự kiến:** 3<br>**Kiểm/nhận:** Cả nhóm |
| **W5.6**<br>hàng 44 | Kiểm chứng nhu cầu và cập nhật mô hình kinh doanh<br>**Đặt tại:** `docs/business/interview-template.md, deployment-hypotheses.md; local-data/interviews/; evaluation/reports/` | **Đầu ra:** Ghi chép trao đổi khách hàng tiềm năng, nhu cầu, gói triển khai, chi phí<br>**Đạt khi:** Tách nhu cầu đã nghe khỏi giả thuyết; dùng chi phí AI thực đo; không tự khẳng định có khách hàng/doanh thu. | **Phụ thuộc:** W5.1; W4.6 (chi phí thực đo đến sau)<br>**Hạn nguồn:** 2026-10-31<br>**Giờ dự kiến:** 8<br>**Kiểm/nhận:** TV4 + TV1 |
| **W5.7**<br>hàng 45 | Biên tập hồ sơ FORGE, tổng hợp bằng chứng và diễn tập<br>**Đặt tại:** `docs/submission/; artifacts/submission/; artifacts/demo/` | **Đầu ra:** Hồ sơ, slide theo yêu cầu, link build/video, nhật ký kiểm tra<br>**Đạt khi:** Mô tả đúng bản final; link mở được trên máy khác; có demo offline và người trình bày dự phòng. | **Phụ thuộc:** W5.5; W5.6; W4.8; W3.8 (bản nháp)<br>**Hạn nguồn:** 2026-11-04<br>**Giờ dự kiến:** 6<br>**Kiểm/nhận:** TV1 + TV3 + TV4 |
| **W5.8**<br>hàng 46 | Kiểm checklist cuối và nộp bài<br>**Đặt tại:** `docs/submission/requirements-checklist.md; artifacts/submission/; release/` | **Đầu ra:** Bản nộp + ảnh/xác nhận nộp + bản sao lưu<br>**Đạt khi:** Mục tiêu nội bộ 20:00 ngày 05/11; kiểm đủ tệp/link/quyền truy cập; lưu xác nhận trước 23:59. | **Phụ thuộc:** W2.8; W5.7; W3.8; W1.8<br>**Hạn nguồn:** 2026-11-05<br>**Giờ dự kiến:** 3<br>**Kiểm/nhận:** TV2 + một người kiểm chéo |
