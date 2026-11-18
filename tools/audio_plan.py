"""Where every sound goes. A cue is tied to a line of the script by a few words of that line, because the packages say their
timings are illustrative. (clip, words of the line, seconds after the line appears). Beds are the ambience layers of a step;
MID_BEDS change them part way through a scene."""

C1 = 'C01_{:02}__{}'
# Action confirmations (power_approved, write_field_note, send_proposal, forward_for_verification, submit_timesheet_request)
# are played by the game itself when the player commits, so they are not listed here.
CHAPTER1 = [
    (1, 'file_review', 'nhìn qua hồ sơ', 0), (1, 'typing_line1', 'Bà nhập bốn dòng dữ liệu', 0), (1, 'typing_line2', 'Bà nhập bốn dòng dữ liệu', .7),
    (1, 'typing_line3', 'Bà nhập bốn dòng dữ liệu', 1.4), (1, 'typing_line4', 'Bà nhập bốn dòng dữ liệu', 2.1), (1, 'green_mark', 'Bà nhập bốn dòng dữ liệu', 3),
    (2, 'power_panel_open', 'Buổi sáng hôm ấy, Mai chuyển', 0), (2, 'power_keying', 'Buổi sáng hôm ấy, Mai chuyển', .8), (2, 'power_submit', 'Buổi sáng hôm ấy, Mai chuyển', 1.8),
    (3, 'report_desk', 'Linh bước vào', 0), (3, 'report_open', 'Mai mở hồ sơ', 0), (3, 'image_zoom', 'Cô phóng to bức ảnh', 0), (3, 'query_open', 'Mai kiểm tra lại dữ liệu', 0),
    (3, 'query_key', 'Mai kiểm tra lại dữ liệu', .8), (3, 'close_report', 'Mai ghi vào hồ sơ', 2.5),
    (4, 'review_requests', 'Sáng hôm ấy, trước cuộc họp chiều', 0), (4, 'open_warehouse_channel', 'Mai mở kênh liên lạc', 0), (4, 'warehouse_report', 'Được. Tôi gửi ngay', 0),
    (4, 'switch_daycare', 'Mai chuyển sang đường dây', 0), (4, 'allocation_panel', 'Mai trải các phương án', 0), (4, 'propose_reroute', 'Nếu chuyển lô thuốc sang kho dự phòng', 0),
    (4, 'note_transport_notice', 'Mai ghi thêm một dòng', 0), (4, 'driver_returns_call', 'nhân viên vận tải gọi lại', 0), (4, 'new_alert', 'một cảnh báo mới xuất hiện', 0),
    (5, 'spread_report', 'Hồi còn là thực tập sinh', 0), (5, 'draft_report', 'Mai đã học cách làm theo', 0), (5, 'sign_report', 'Mai đã học cách làm theo', 1),
    (5, 'file_report', 'Mai đã học cách làm theo', 2), (5, 'green_status', 'Mai đã học cách làm theo', 3), (5, 'next_item', 'Mai đã học cách làm theo', 4),
    (6, 'mai_steps', 'Trên đường từ khu kiểm toán', 0), (6, 'group_leaves', 'dẫn đoàn học sinh đi tiếp', 0),
    (7, 'dish_arrive', 'Buổi trưa, Mai ăn cùng Linh', 0), (7, 'linh_push_peas', 'Buổi trưa, Mai ăn cùng Linh', 1.2), (7, 'small_cutlery', 'Hai người im lặng ăn tiếp', 0),
    (7, 'after_conversation', 'Hai người im lặng ăn tiếp', 1.6), (7, 'meeting_chime', 'Chuông báo cuộc họp chiều', 0),
    (8, 'approach_steps', 'Trên hành lang trước phòng họp', 0), (8, 'put_away_transfer_form', 'Anh cất bản đề nghị', 0), (8, 'open_work_notebook', 'Cô mở sổ việc', 0), (8, 'write_question', 'Cô mở sổ việc', .8),
    (9, 'energy_map_focus', 'Buổi chiều, trong phòng họp kín', 0), (9, 'show_evidence', 'Có ảnh thực địa', 0),
    (10, 'prune_rosemary', 'Mẹ cô đang tỉa cây hương thảo', 0), (10, 'dry_rosemary_leaf', 'Bà ngắt một chiếc lá khô', 0),
    (10, 'take_photo_album', 'Mai lấy cuốn album ra', 0), (10, 'turn_album_page', 'Bà nhìn tấm ảnh một lúc', 0), (10, 'album_between_them', 'đặt cuốn album giữa hai người', 0),
    (11, 'close_album', 'Mai khép cuốn album lại', 0), (11, 'open_work_notebook', 'Cô mở lại sổ việc', 0), (11, 'enter_timesheet_request', 'Cô mở lại sổ việc', .8), (11, 'reopen_district_photo', 'Mai mở lại bức ảnh căn nhà', 0),
]

CHAPTER2 = [
    ('C02_01__wet_steps', 'Dưới chân cô, những viên gạch', 0), ('C02_01__rib_clamp', 'Cô bé dùng chiếc kẹp tự chế', 0), ('C02_01__umbrella_test_1', 'Cô bé dùng chiếc kẹp tự chế', 1.2),
    ('C02_01__umbrella_test_2', 'Cô bé dùng chiếc kẹp tự chế', 2), ('C02_01__umbrella_test_3', 'Cô bé dùng chiếc kẹp tự chế', 2.8),
    ('C02_02__stone_cross_line1', 'vạch một dấu chữ thập', 0), ('C02_02__stone_cross_line2', 'vạch một dấu chữ thập', .9), ('C02_02__take_notebook', 'An lấy từ trong túi ra cuốn sổ', 0),
    ('C02_02__turn_notebook', 'Mai lật thêm một trang', 0), ('C02_02__bridge_step', 'dẫn Mai qua một cây cầu', 0),
    ('C02_03__wire_handoff', 'lấy một đoạn dây đồng', 0), ('C02_03__serve_porridge_1', 'múc hai bát', 0), ('C02_03__serve_porridge_2', 'múc hai bát', 1.1), ('C02_03__stir_porridge', 'Bà vừa nói vừa khuấy nồi', 0),
    ('C02_04__station_map_write', 'An mượn cây bút', 0), ('C02_04__maintenance_log_handoff', 'đưa Mai một cuốn sổ giấy', 0), ('C02_04__maintenance_page_turn', 'Sổ ghi bảy lần vá van', 0), ('C02_04__mai_write_notes', 'Mai ghi chép lại thông tin', 0),
    ('C02_05__umbrella_off_hook', 'Em lấy chiếc ô xuống', 0), ('C02_05__patchwork_open', 'Em lấy chiếc ô xuống', 1),
    ('C02_06__urgent_footsteps', 'An bỏ chiếc ô đang sửa dở', 0), ('C02_06__bottle_drip_closeup', 'một hàng chai nhựa', 0), ('C02_06__gasket_placement', 'An chèn tạm miếng đệm', 0),
    ('C02_06__pipe_recheck', 'Mai kiểm tra vết rò', 0), ('C02_06__hand_over_towel', 'đưa bà một chiếc khăn', 0),
    ('C02_07__shoes_in_corner', 'Hạ tháo giày', 0), ('C02_07__maintenance_photocopy', 'Mai lấy bản sao sổ bảo trì', 0), ('C02_07__porridge_bowl', 'An đặt trước mặt mẹ', 0), ('C02_07__wash_hands', 'Hạ quay đi rửa tay', 0),
    ('C02_08__tin_lid_open', 'Hạ lấy ra một hộp thiếc', 0), ('C02_08__documents_in_sleeves', 'Bên trong là đơn chuyển cư trú', 0), ('C02_08__sort_documents', 'Hạ xếp chúng lên bàn', 0),
    ('C02_08__smooth_receipt', 'Bà vuốt phẳng mép', 0), ('C02_08__plastic_sleeve_second', 'Mai lần lượt chụp từng tài liệu', 0),
    ('C02_09__old_photo_take', 'An kéo từ hộp ra một bức ảnh', 0), ('C02_09__photo_put_away', 'rồi cất bức ảnh đi', 0), ('C02_09__notebook_open', 'Em mở cuốn sổ', 0), ('C02_09__tea_pour', 'Hạ rót thêm trà', 0), ('C02_09__mai_write_promise', 'Mai viết việc ấy vào sổ', 0),
    ('C02_10__feet_stop_near_bridge', 'An tiễn Mai tới chân cầu', 0),
    ('C02_11__invoice_query', 'tra lại hóa đơn', 0), ('C02_11__leave_gate_steps', 'Cô đi qua cổng kiểm soát', 0),
]

CHAPTER3 = [
    ('C03_02__02_kiosk_missing_code', 'Máy phát phiếu yêu cầu', 1.5), ('C03_02__02_handbook_pages', 'lấy một cuốn cẩm nang dày', 0), ('C03_02__02_pen_set_down', 'Người nhân viên đặt bút xuống', 0),
    ('C03_02__02_write_ha_request', 'Mai lấy tờ giấy trắng', 0), ('C03_02__02_information_receipt_stamp', 'Nhân viên đóng dấu tiếp nhận', 0),
    ('C03_03__train_door_close_v03', 'Trên tàu về cơ quan', 0), ('C03_03__train_door_open_preview_v02', 'Mai không trả lời được', 0), ('C03_03__elevator_door_close_v03', 'Câu hỏi theo cô vào cả thang máy', 0),
    ('C03_03__elevator_door_open_v03', 'Câu hỏi theo cô vào cả thang máy', 6.5),
    ('C03_04__04_check_ha_dossier', 'Cán bộ xét duyệt kiểm tra từng giấy tờ', 0), ('C03_04__04_refuse_request_button', 'Cán bộ xét duyệt kiểm tra từng giấy tờ', 2.2),
    ('C03_05__05_pencil_math', 'An làm đúng gần hết bài số học', 0), ('C03_05__small_classroom_applause_v03', 'An làm đúng gần hết bài số học', 1.8), ('C03_05__05_chair_overturn', 'An nổi giận', 0), ('C03_05__05_paper_left_doorstep', 'An thấy một tập giấy', 0),
    ('C03_06__06_pull_empty_chair', 'Cô chỉ kéo chiếc ghế trống', 0), ('C03_06__06_calendar_handoff', 'đưa em tấm lịch ôn tập', 0), ('C03_06__06_books_to_bag', 'Cô Nhi xếp sách vào túi', 0),
    ('C03_07__07_toolkit_from_under_bed', 'Bà lấy từ dưới gầm giường', 0), ('C03_07__07_toolbox_contents', 'Bên trong có các đầu vít', 0), ('C03_07__07_gauge_hold', 'An cầm một chiếc đồng hồ áp suất', 0), ('C03_07__07_toolbox_close', 'Hạ khép hộp dụng cụ', 0),
    ('C03_08__08_cards_scanned_by_other_teens', 'Các bạn quét thẻ', 0), ('C03_08__08_tear_paper_corner', 'An xé một góc giấy nháp', 0), ('C03_08__08_ball_kicks_field__01', 'Tín gọi An vào chơi', 1),
    ('C03_08__08_ball_kicks_field__02', 'Tín gọi An vào chơi', 2.6), ('C03_08__08_ball_kicks_field__03', 'Tín gọi An vào chơi', 4.2), ('C03_08__08_ball_kicks_field__04', 'Tín gọi An vào chơi', 5.8),
    ('C03_09__09_exam_page_present', 'chìa ra tờ giấy', 0), ('C03_09__09_repaired_umbrella_move', 'An kéo chiếc ô vá tới', 0),
    ('C03_10__10_send_suspend_request', 'Mai gửi yêu cầu dừng tự động', 0), ('C03_10__10_system_notice_arrives', 'Chín phút sau', 0), ('C03_10__10_photo_paper_to_desk', 'Cô lấy bản chụp bài thi', 0),
]

# one list of layers per step of the chapter
BEDS2 = [
    ['C02_01__glass_ticks', 'C02_01__metal_drops', 'C02_01__pipe_condensate'], ['C02_02__irregular_drips', 'C02_02__pipe_flow'], ['C02_03__district_pipe', 'C02_03__market_murmur'], ['C02_04__market_exit'],
    ['C02_05__roof_specks', 'C02_05__street_condensate'], ['C02_06__alley_machinerain', 'C02_06__pipe_drip_again'], ['C02_07__ha_house_roomtone', 'C02_07__remote_condensate'], ['C02_08__ha_roomtone_papers'],
    ['C02_09__condensate_through_wall', 'C02_09__ha_home_quiet'], ['C02_10__bridge_pipewater', 'C02_10__roof_distant_tips'],
    ['C02_11__mechanical_rain_behind', 'C02_11__portable_audit_roomtone', 'C02_11__roof_patter_far', 'C02_11__terminal_vent'],
]
BEDS3 = [
    ['C03_01__B01__01__amb_office'], ['C03_02__B01__01__amb_public_wait', 'C03_02__B01__02__amb_office'], ['C03_03__B01__01__amb_train'], ['C03_04__B01__01__amb_office'],
    ['C03_05__B01__01__amb_class_blurred_v02'], ['C03_06__B01__01__amb_class_blurred_v02'], ['C03_07__B01__01__amb_home', 'C03_07__B01__02__amb_cooling_water'], ['C03_08__B01__01__amb_public_wait'],
    ['C03_09__B01__01__amb_rim_street'], ['C03_10__B01__01__amb_office'],
]
MID_BEDS2 = [
    ('Tại trạm lọc số mười chín', ['C02_04__filter_fan_close', 'C02_04__filter_vent_close', 'C02_04__water_pump']),
    ('Bà Dung không bị thương nặng', ['C02_06__dung_roomtone', 'C02_06__pipe_drip_inside']),
]
MID_BEDS3 = [
    ('Đến lượt, Mai tiến đến quầy', ['C03_02__B02__01__amb_office']), ('Câu hỏi theo cô vào cả thang máy', ['C03_03__elevator_ride_real_motor_v03']),
    ('Buổi thứ bảy', ['C03_05__B02__01__amb_class_blurred_v02']), ('Tối hôm ấy, An nói', ['C03_05__B03__01__amb_home']), ('Sáng hôm sau, An thấy', ['C03_05__B04__01__amb_home']),
    ('Khi tan học', ['C03_06__B02__01__amb_class_blurred_v02']), ('Ra khỏi trạm', ['C03_08__B02__01__amb_rim_street']), ('Tín gọi An vào chơi', ['C03_08__B03__01__amb_sports_field_authentic']),
    ('Mai ngồi xuống bậc thềm', ['C03_09__B02__01__sfx_fan_stutter', 'C03_09__B02__02__amb_cooling_water']), ('Hai vật ấy không chứng minh', ['C03_10__B02__01__amb_office']),
]
