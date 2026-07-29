# Spec: Khôi phục build APK Android trên Flutter stable mới

**Date:** 2026-07-22
**Status:** Ready

---

## Problem Statement

Dự án `AutomaticBrewingCoffeeApp` đang fail ở bước resolve dependency vì `wakelock_plus` được trỏ tới thư mục local không tồn tại. Mục tiêu là tạo được APK Android release để kiểm thử trên Flutter stable mới mà không thay đổi mã nguồn/giao diện hoặc hành vi kiosk.

---

## User Stories

- **[P1]** Là developer, tôi muốn dependency `wakelock_plus` được resolve từ nguồn hợp lệ để `flutter build apk --release` không dừng ở bước version solving.
  Accepted when: `flutter pub get` hoàn tất với exit code 0 và không có thông báo package `from path` trỏ tới thư mục không tồn tại.

- **[P1]** Là developer, tôi muốn Android Gradle toolchain tương thích với Flutter stable đang cài để tạo APK release.
  Accepted when: `flutter build apk --release` hoàn tất với exit code 0 và tạo file APK trong `build/app/outputs/flutter-apk/`.

- **[P1]** Là người kiểm thử kiosk, tôi muốn chức năng giữ màn hình sáng vẫn hoạt động như trước.
  Accepted when: các lời gọi `WakelockPlus.enable/disable` và permission Android liên quan được giữ nguyên, không có thay đổi UI/runtime ngoài tương thích build.

- **[P2]** Là maintainer, tôi muốn cấu hình Android chỉ có một nguồn Gradle module rõ ràng để build ổn định giữa các máy.
  Accepted when: module `app` không bị cấu hình đồng thời bởi hai DSL/file gây phụ thuộc thứ tự ưu tiên.

- **[P3]** _(out of scope — noted for future)_ Cấu hình keystore production, signing CI/CD và phát hành lên Play Store.

---

## Functional Requirements

1. FR-01: Khai báo `wakelock_plus` bằng hosted dependency tương thích với SDK Dart/Flutter đang dùng; không dùng `path` tới thư mục không tồn tại.
2. FR-02: Tái tạo dependency metadata (`pubspec.lock`, `.dart_tool` và plugin registrants khi cần) từ `pubspec.yaml`, không chỉnh tay generated files nếu Flutter có thể sinh lại.
3. FR-03: Đồng bộ Gradle wrapper, Android Gradle Plugin, Kotlin plugin, Java target và Android SDK với phiên bản Flutter stable được xác nhận trên máy build.
4. FR-04: Giữ nguyên Dart source, asset, layout/UI, API và logic kiosk; chỉ sửa import/API nếu package version mới bắt buộc.
5. FR-05: Dùng debug signing hiện có cho APK release kiểm thử; không thêm hoặc commit secrets/keystore.

---

## Non-Functional Requirements

- Performance: không được làm tăng thời gian khởi động hoặc thay đổi hành vi runtime ngoài phạm vi build compatibility.
- Security: không ghi khóa ký, mật khẩu hoặc thông tin nhạy cảm vào repository; debug key chỉ dùng kiểm thử cục bộ.
- Availability: cùng một checkout sạch phải resolve dependency và build APK lặp lại được trên máy có Flutter/Android SDK tương thích.

---

## Success Criteria

- [ ] `flutter pub get` kết thúc exit code 0 trên môi trường Flutter stable mục tiêu.
- [ ] `flutter analyze` không phát sinh lỗi mới do thay đổi tương thích.
- [ ] `flutter build apk --release` kết thúc exit code 0.
- [ ] File `build/app/outputs/flutter-apk/app-release.apk` tồn tại và có kích thước lớn hơn 0 byte.
- [ ] APK cài được trên thiết bị/emulator Android mục tiêu và màn hình không tự tắt trong flow kiosk đã có.

---

## Out of Scope

- Thiết kế lại UI hoặc thay đổi business logic.
- Nâng cấp package không liên quan nếu không cần cho Android build.
- Production keystore, Play App Signing, CI/CD và phát hành store.

---

## Assumptions

- Flutter SDK được tham chiếu bởi `android/local.properties` là Flutter stable và có thể chạy trên máy build.
- Debug signing hiện có đủ để tạo APK release phục vụ kiểm thử nội bộ.
- Các thay đổi chưa commit trong working tree thuộc phạm vi người dùng và phải được bảo toàn.

---

## [NEEDS CLARIFICATION]

<!-- Không còn mục blocking sau khi người dùng chọn debug signing cho giai đoạn kiểm thử. -->
