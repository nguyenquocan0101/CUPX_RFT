# Brainstorm: Khôi phục khả năng build APK Android trên Flutter mới

**Date:** 2026-07-22

## Ideas Explored

- **Giữ `wakelock_plus` từ pub.dev và làm sạch dependency state:** sửa đúng nguyên nhân lỗi `path` trỏ tới thư mục không tồn tại, giữ nguyên API và hành vi kiosk.
- **Bỏ hoặc thay `wakelock_plus` bằng native Android:** giảm một dependency nhưng làm thay đổi hành vi giữ màn hình sáng, trái với mục tiêu giữ nguyên app.
- **Nâng toàn bộ Android toolchain:** cập nhật Gradle/AGP/Kotlin và SDK cấu hình để tương thích Flutter stable mới; cần kiểm soát phạm vi để không kéo theo thay đổi giao diện.
- **Cấu hình keystore production ngay:** hữu ích khi phát hành thật nhưng không cần cho mục tiêu kiểm thử APK và làm tăng phạm vi/bảo mật cần xử lý.

## User's Direction

Người dùng muốn giữ nguyên mã nguồn/giao diện, ưu tiên build APK Android trước và sửa ít nhất có thể. Đã chọn hướng giữ `wakelock_plus` từ package hosted, dùng debug key hiện có để kiểm thử release APK; keystore production để giai đoạn phát hành riêng.

## Open Questions

- Cần xác nhận chính xác Flutter/Dart/Java/Android SDK đang được dùng trên máy build và tái hiện `flutter build apk --release` sau khi làm sạch generated state.
- Cần thống nhất sử dụng một file Gradle cho module app (`build.gradle` hoặc `build.gradle.kts`) để kết quả không phụ thuộc cách Gradle chọn file.
- Cần kiểm tra các thay đổi chưa commit hiện có trước khi chỉnh sửa, tránh ghi đè công việc của người dùng.

## Risks

- `pubspec.lock`, `.dart_tool` hoặc `.flutter-plugins-dependencies` có thể còn đường dẫn package cũ; nếu không tái tạo, lỗi `depends on ... from path` tiếp tục xuất hiện dù `pubspec.yaml` đã đúng.
- Flutter stable mới có thể yêu cầu Java/AGP/Gradle/compileSdk tương thích; nâng quá rộng có thể tạo lỗi native ngoài phạm vi dependency.
- APK release hiện dùng debug signing; phù hợp kiểm thử cài đặt nhưng không được dùng để phát hành production.
