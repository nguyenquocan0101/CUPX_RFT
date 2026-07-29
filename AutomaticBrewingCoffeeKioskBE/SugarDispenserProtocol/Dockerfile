# Sử dụng Python base image
FROM python:3.12-slim

# Set thư mục làm việc trong container
WORKDIR /app

# Copy toàn bộ mã nguồn vào container
COPY . .

# Cài thư viện từ requirements.txt
RUN pip install --no-cache-dir -r requirements.txt

# Chạy pump.py mặc định (hoặc bạn có thể chỉnh sửa)
CMD ["python", "pump.py"]
