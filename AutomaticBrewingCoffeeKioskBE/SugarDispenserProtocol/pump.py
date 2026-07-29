import serial
import time

# #: chạy tất cả các bơm
# 0: dừng tất cả các bơm
# pumpindex1-valuepump1|pumpindex2-valuepump2 # ví dụ: 1-100|2-200|3-300 (valuepump đơn vị là ml)

def read_response(ser, timeout=1):
    """Đọc dữ liệu phản hồi từ UART với timeout"""
    start_time = time.time()
    response_complete = False
    
    while (time.time() - start_time) < timeout:
        if ser.in_waiting > 0:
            try:
                response = ser.readline().decode().strip()
                if response:
                    print(f"Phản hồi: {response}")
                    response_complete = True
                # Đợi thêm một chút để đảm bảo nhận hết dữ liệu
                time.sleep(0.5)
                if ser.in_waiting == 0 and response_complete:
                    return response
            except Exception as e:
                print(f"Lỗi đọc dữ liệu: {e}")
                return None
        time.sleep(0.01)
    
    if not response_complete:
        print("Hết thời gian chờ phản hồi")
    return None

def control_motor():
    try:
        ser = serial.Serial(
            port='COM3',        
            baudrate=9600,
            timeout=1
        )
        time.sleep(0.5)  # Đợi kết nối ổn định
        
    except Exception as e:
        print(f"Lỗi kết nối: {e}")
        return
    try:
        while True:
            cmd = input("Nhập lệnh: ").strip()
            if cmd.lower() == 'x':
                break
            else:
                ser.write(cmd.encode())
                read_response(ser)
            time.sleep(0.1)
    finally:
        ser.close()
        print("Đã đóng kết nối")

if __name__ == '__main__':
    control_motor()