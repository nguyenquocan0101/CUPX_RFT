import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:abc_androidapp/config/themes/app_palette.dart';
import 'package:abc_androidapp/app/core/kiosk_mode/kiosk_service.dart';

class AdminExitDetector extends StatefulWidget {
  final Widget child;
  
  const AdminExitDetector({super.key, required this.child});

  @override
  State<AdminExitDetector> createState() => _AdminExitDetectorState();
}

class _AdminExitDetectorState extends State<AdminExitDetector> 
    with WidgetsBindingObserver {
  int _tapCount = 0;
  DateTime? _lastTap;
  
  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addObserver(this);
  }
  
  @override
  void dispose() {
    WidgetsBinding.instance.removeObserver(this);
    super.dispose();
  }
  
  @override
  void didChangeAppLifecycleState(AppLifecycleState state) {
    super.didChangeAppLifecycleState(state);
    
    // Đảm bảo kiosk mode được duy trì khi app resume
    if (state == AppLifecycleState.resumed) {
      KioskService.refreshKioskMode();
    }
  }
  
  void _handleSecretTap() {
    final now = DateTime.now();
    
    if (_lastTap == null || now.difference(_lastTap!) > const Duration(seconds: 3)) {
      _tapCount = 1;
    } else {
      _tapCount++;
    }
    
    _lastTap = now;
    
    // Chỉ hiển thị feedback khi gần đạt đủ số lần tap
    if (_tapCount >= 5 && _tapCount < 7) {
      // Vibration nhẹ để báo hiệu
      HapticFeedback.lightImpact();
      
      // // Show subtle indicator
      // ScaffoldMessenger.of(context).showSnackBar(
      //   SnackBar(
      //     content: Row(
      //       mainAxisSize: MainAxisSize.min,
      //       children: [
      //         Icon(Icons.lock_outline, size: 16, color: Colors.white),
      //         SizedBox(width: 8),
      //         Text('$_tapCount/7', style: TextStyle(fontSize: 12)),
      //       ],
      //     ),
      //     duration: Duration(milliseconds: 800),
      //     backgroundColor: Colors.black54,
      //     behavior: SnackBarBehavior.floating,
      //     margin: EdgeInsets.only(
      //       bottom: MediaQuery.of(context).size.height - 100,
      //       left: 20,
      //       right: 20,
      //     ),
      //     shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
      //   ),
      // );
    }
    
    if (_tapCount >= 7) {
      HapticFeedback.heavyImpact();
      _showAdminDialog();
      _tapCount = 0;
    }
  }
  
  void _showAdminDialog() async {
    final kioskInfo = await KioskService.getKioskInfo();
    final TextEditingController passwordController = TextEditingController();
    
    showDialog(
      context: context,
      barrierDismissible: false,
      builder: (context) => AlertDialog(
        backgroundColor: Colors.white,
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
        title: Row(
          children: [
            Icon(Icons.admin_panel_settings, color: AppPalette.blue.primary),
            const SizedBox(width: 8),
            const Text(
              'Xác thực quản trị viên',
              style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
            ),
          ],
        ),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Kiosk Status - compact version
            Container(
              padding: const EdgeInsets.all(12),
              decoration: BoxDecoration(
                color: Colors.grey.shade100,
                borderRadius: BorderRadius.circular(8),
              ),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    'Trạng thái kiosk:',
                    style: TextStyle(
                      fontSize: 12,
                      fontWeight: FontWeight.w600,
                      color: Colors.grey.shade700,
                    ),
                  ),
                  const SizedBox(height: 4),
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: [
                      Text('Chế độ khóa kiosk:', style: TextStyle(fontSize: 11, color: Colors.grey.shade600)),
                      Text(
                        kioskInfo['isServiceEnabled'] ? "KÍCH HOẠT" : "CHƯA KÍCH HOẠT",
                        style: TextStyle(
                          fontSize: 11,
                          color: kioskInfo['isServiceEnabled'] ? Colors.green : Colors.red,
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                    ],
                  ),
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: [
                      Text('Hình thức:', style: TextStyle(fontSize: 11, color: Colors.grey.shade600)),
                      Text(
                        '${kioskInfo['isInKioskMode'] ? "KHÓA" : "MỞ KHÓA"}',
                        style: TextStyle(
                          fontSize: 11,
                          color: kioskInfo['isInKioskMode'] ? Colors.orange : Colors.blue,
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                    ],
                  ),
                ],
              ),
            ),
            
            const SizedBox(height: 16),
            
            TextField(
              controller: passwordController,
              obscureText: true,
              autofocus: true,
              decoration: InputDecoration(
                hintText: 'Mật khẩu quản trị viên',
                prefixIcon: const Icon(Icons.lock_outline),
                border: OutlineInputBorder(borderRadius: BorderRadius.circular(8)),
                focusedBorder: OutlineInputBorder(
                  borderRadius: BorderRadius.circular(8),
                  borderSide: BorderSide(color: AppPalette.blue.primary),
                ),
                contentPadding: EdgeInsets.symmetric(horizontal: 12, vertical: 12),
              ),
              onSubmitted: (password) => _checkPassword(password),
            ),
          ],
        ),
        actions: [
          TextButton(
            onPressed: () {
              Navigator.pop(context);
              _tapCount = 0;
            },
            child: Text('Hủy', style: TextStyle(color: Colors.grey.shade600)),
          ),
          ElevatedButton(
            onPressed: () => _checkPassword(passwordController.text),
            style: ElevatedButton.styleFrom(
              backgroundColor: AppPalette.blue.primary,
              shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
              padding: EdgeInsets.symmetric(horizontal: 16, vertical: 8),
            ),
            child: const Text('Xác thực', style: TextStyle(color: Colors.white)),
          ),
        ],
      ),
    );
  }
  
  void _checkPassword(String password) {
    String adminPassword = _generatePasswordFromDate();
    
    if (password == adminPassword) {
      Navigator.pop(context);
      _showAdminMenu();
    } else {
      Navigator.pop(context);
      HapticFeedback.heavyImpact();
      
      // Show error với animation
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Row(
            children: [
              Icon(Icons.error_outline, color: Colors.white, size: 20),
              SizedBox(width: 8),
              Text('Mật khẩu không chính xác'),
            ],
          ),
          backgroundColor: Colors.red.shade600,
          duration: Duration(seconds: 2),
          behavior: SnackBarBehavior.floating,
          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
        ),
      );
    }
  }

  static String _generatePasswordFromDate() {
    final now = DateTime.now();
    final year = now.year % 100;
    final dateStr =
        '${now.day.toString().padLeft(2, '0')}${now.month.toString().padLeft(2, '0')}${year.toString().padLeft(2, '0')}';
    return dateStr.split('').reversed.join();
  }

  
  void _showAdminMenu() {
    showDialog(
      context: context,
      barrierDismissible: false,
      builder: (context) => AlertDialog(
        backgroundColor: Colors.white,
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
        title: const Text('Quản lý Admin', style: TextStyle(fontWeight: FontWeight.bold)),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            // Exit App
            _buildMenuTile(
              icon: Icons.exit_to_app,
              iconColor: Colors.red,
              title: 'Thoát ứng dụng',
              subtitle: 'Tắt chế độ khóa kiosk & thoát',
              onTap: () async {
                await KioskService.disableKioskMode();
                SystemNavigator.pop();
              },
            ),
            
            Divider(height: 1, color: Colors.grey.shade200),
            
            // Toggle Kiosk Mode
            _buildMenuTile(
              icon: KioskService.isKioskEnabled ? Icons.lock_open : Icons.lock,
              iconColor: KioskService.isKioskEnabled ? Colors.orange : Colors.green,
              title: KioskService.isKioskEnabled ? 'Tắt chế độ khóa Kiosk' : 'Chế độ khóa Kiosk',
              subtitle: KioskService.isKioskEnabled ? 'Cho phép thoát ứng dụng' : 'Khóa ứng dụng',
              onTap: () async {
                if (KioskService.isKioskEnabled) {
                  await KioskService.disableKioskMode();
                } else {
                  await KioskService.enableKioskMode();
                }
                Navigator.pop(context);
                setState(() {}); // Refresh UI
                
                // Show status
                ScaffoldMessenger.of(context).showSnackBar(
                  SnackBar(
                    content: Text(
                      KioskService.isKioskEnabled ? 'Kích hoạt khóa Kiosk' : 'Vô hiệu hóa khóa Kiosk'
                    ),
                    backgroundColor: KioskService.isKioskEnabled ? Colors.orange : Colors.green,
                  ),
                );
              },
            ),
            
            Divider(height: 1, color: Colors.grey.shade200),
            
            // Restart Kiosk
            _buildMenuTile(
              icon: Icons.refresh,
              iconColor: Colors.blue,
              title: 'Khởi động lại chế độ khóa Kiosk',
              subtitle: 'Làm mới chế độ khóa Kiosk',
              onTap: () async {
                Navigator.pop(context);
                await KioskService.disableKioskMode();
                await Future.delayed(Duration(milliseconds: 500));
                await KioskService.enableKioskMode();
                
                ScaffoldMessenger.of(context).showSnackBar(
                  SnackBar(
                    content: Text('Chế độ khóa Kiosk đã được khởi động lại'),
                    backgroundColor: Colors.blue,
                  ),
                );
              },
            ),
          ],
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: const Text('Đóng'),
          ),
        ],
      ),
    );
  }
  
  Widget _buildMenuTile({
    required IconData icon,
    required Color iconColor,
    required String title,
    required String subtitle,
    required VoidCallback onTap,
  }) {
    return ListTile(
      leading: Icon(icon, color: iconColor, size: 24),
      title: Text(title, style: TextStyle(fontSize: 14, fontWeight: FontWeight.w500)),
      subtitle: Text(subtitle, style: TextStyle(fontSize: 12, color: Colors.grey.shade600)),
      onTap: onTap,
      contentPadding: EdgeInsets.symmetric(horizontal: 8, vertical: 4),
      dense: true,
    );
  }

  @override
  Widget build(BuildContext context) {
    return PopScope(
      canPop: false, // Chặn back gesture
      onPopInvokedWithResult: (bool didPop, Object? result) {
        // Không làm gì cả - chặn hoàn toàn back action
        return;
      },
      child: Directionality(
      textDirection: TextDirection.ltr,
      child: Stack(
        alignment: Alignment.topLeft, // Explicit alignment instead of default
        children: [
          widget.child,
          
          // Secret tap area - INVISIBLE và ở góc trên trái
          Positioned(
            top: 0,
            left: 0,
            child: GestureDetector(
              onTap: _handleSecretTap,
              child: Container(
                width: 60, // Giảm size để kín đáo hơn
                height: 60,
                color: Colors.transparent, // Hoàn toàn trong suốt
                // Debug border - REMOVE trong production
                // decoration: BoxDecoration(
                //   border: Border.all(color: Colors.red.withOpacity(0.2), width: 1),
                // ),
              ),
            ),
          ),
        ],
      ),
    ),
    );
  }
}