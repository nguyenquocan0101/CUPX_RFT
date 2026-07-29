# Brainstorm: Chuyen he thong Automatic Brewing Coffee sang full local

**Date:** 2026-07-29

## Ideas Explored

- **Giữ nguyên kiến trúc, thay dependency cloud bằng local:** giữ 4 repo và các hợp đồng API hiện tại; chạy SQL Server/PostgreSQL, Redis, RabbitMQ, CouchDB và MinIO bằng Docker Compose.
- **Gom tất cả vào một container stack:** đưa API, frontend và mọi controller vào cùng Compose. Không chọn làm mặc định vì Flutter không phù hợp với workflow container trên Windows và controller phần cứng cần quyền serial/USB riêng.
- **Mock-heavy để chạy nhanh:** thay Firebase Auth, email SMTP, VNPay/MPOS, webhook callback, Cloudflare và Azure IoT bằng adapter local/mock có endpoint trigger và log. Đây là hướng được chọn cho MVP local.
- **Chạy hardware thật nhưng có simulator:** giữ khả năng kết nối máy pha/thả cốc/đá/đường thật; thêm chế độ simulator để test khi không cắm đủ thiết bị. Cần tách hardware transport khỏi business workflow.
- **Gemini là ngoại lệ cloud duy nhất:** repo hiện chưa có provider AI. Nếu có use case AI, thêm interface provider và tích hợp Gemini qua server-side key; không đưa key vào Flutter/Next client.

## User's Direction

Người dùng muốn cả hệ thống chạy local trên máy, dùng Docker Compose cho dependency, chạy FE/Flutter/API bằng dev command riêng. Auth/OAuth, email, payment và webhook dùng mock/local để triển khai nhanh. Máy có hardware thật, nhưng vẫn cần simulator để kiểm thử khi thiết bị không kết nối. Gemini API key được chấp nhận là dependency bên ngoài duy nhất nếu hệ thống thực sự có chức năng AI.

## Codegraph Findings

- **Backend chính:** `Program.cs` đăng ký Sentry, CAP/RabbitMQ, SMTP, Cloudflare, Redis, MPOS, Hangfire, Firebase, Supabase, VNPay, Azure IoT, JWT và SignalR. `ServicesDependency` khởi tạo Firebase credential, Supabase client, SQL Server, Redis, Hangfire và RabbitMQ.
- **Auth:** `AuthService.LoginFirebase` gọi `IFirebaseAuthService`; implementation gọi Firebase Identity Toolkit qua URL Google. Mock auth nên giữ interface và trả JWT nội bộ cùng refresh token để FE/Flutter giữ flow hiện tại.
- **Storage:** `SupabaseStorageService` dùng trực tiếp Supabase bucket/public URL. Cần thay bằng `IObjectStorageService` và implementation MinIO hoặc local filesystem; MinIO ít phá hợp đồng bucket hơn.
- **Payment:** payment flow có cả VNPay và MPOS callback, CAP subscriber cập nhật order/payment và phát SignalR. Mock cần mô phỏng các trạng thái `Pending`, `Success`, `Failed`, `Cancelled`, `Refunded` và callback idempotency.
- **Kiosk backend:** `AddDatabase` hiện đăng ký CouchDB, `AddAppServices` tạo `CloudClient` từ `CloudConfig:BaseUrl`; `CloudClient` gọi `/orders/complete` và `/orders/fail` qua `X-API-Key`. Khi full local, base URL phải là backend local và cần bỏ tên gọi cloud khỏi boundary.
- **Kiosk hardware:** Compose hiện build các controller và map `/dev/ttyUSB*`; cần profile thật/simulator và cấu hình port theo biến môi trường. Không nên bắt API phụ thuộc controller thật trong local smoke test.
- **Frontend Next.js:** dev proxy đã trỏ mặc định `/api/v1` tới `http://localhost:5100`; SignalR và một số export call vẫn phụ thuộc `NEXT_PUBLIC_*` nên phải đồng bộ endpoint local.
- **Flutter:** URL/API key/SignalR đọc từ `.env`, nhưng refresh token đang hardcode một domain public trong `api_interceptor.dart`; phải chuyển về `ApiConstants.baseUrl` hoặc config riêng.

## Open Questions

- Cần xác nhận API chính local sẽ dùng port nào cố định và Kiosk API local sẽ gọi API chính bằng hostname nào trong Compose.
- Cần xác định dữ liệu khởi tạo: dùng SQL script hiện có cho backend chính, migration/seed cho kiosk, hay một bộ seed chung.
- Cần xác định AI feature cụ thể trước khi thêm Gemini; repo hiện không có call site Gemini/OpenAI/LLM.

## Risks

- **Startup failure:** một số integration được khởi tạo eagerly; thiếu credential cloud hoặc file Firebase có thể làm API không boot dù feature đó không dùng. Cần conditional registration theo `LOCAL_MODE`/provider settings.
- **State divergence:** backend chính dùng SQL Server, kiosk dùng CouchDB và có code PostgreSQL bị comment. Cần chốt source of truth và chiến lược sync local trước khi E2E.
- **Hardware coupling:** controller serial/USB và các service phụ thuộc RabbitMQ có thể làm Compose fail trên Windows. Cần simulator profile và test từng thiết bị độc lập.
- **Secret exposure:** workspace có nhiều secret-like values/credential references trong env/config/summary. Cần rotate các credential đã từng dùng, đưa secret vào local-only `.env`, và không commit giá trị thật.
- **Behavioral mismatch from mocks:** mock payment/auth/webhook phải giữ đúng response shape, status transition, token claims và SignalR event names để frontend không cần fork logic.

## Delivery Note

Kết quả cuối sẽ được gom vào remote mới `https://github.com/nguyenquocan0101/CUPX_RFT.git` dưới dạng một repo chứa 4 thư mục cấp cao. Chỉ push sau khi loại secret thật, kiểm tra clone/setup local và xác nhận commit push thành công.
