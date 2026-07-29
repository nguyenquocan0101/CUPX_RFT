import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:kiosk_mode/kiosk_mode.dart' as kiosk;
import 'package:wakelock_plus/wakelock_plus.dart';

class KioskService {
  static bool _isKioskEnabled = false;
  static bool _isInitialized = false;
  
  static bool get isKioskEnabled => _isKioskEnabled;
  static bool get isInitialized => _isInitialized;
  
  /// Khởi tạo Kiosk Mode
  static Future<void> initialize() async {
    if (_isInitialized) return;
    
    try {
      // Kiểm tra trạng thái hiện tại
      final currentMode = await kiosk.getKioskMode();
      _isKioskEnabled = currentMode == kiosk.KioskMode.enabled;
      
      _isInitialized = true;
      print('✅ Kiosk service initialized - Current mode: $currentMode');
    } catch (e) {
      print('❌ Error initializing kiosk service: $e');
      _isInitialized = true; // Still mark as initialized
    }
  }
  
  /// Bật Kiosk Mode hoàn toàn
  static Future<void> enableKioskMode() async {
    try {
      // 1. Start kiosk mode - sử dụng API đúng
      final success = await kiosk.startKioskMode();
      print('🔒 Kiosk mode start result: $success');
      
      // 2. Ẩn hoàn toàn system UI
      await SystemChrome.setEnabledSystemUIMode(
        SystemUiMode.immersiveSticky,
        overlays: [], // Ẩn tất cả overlays
      );
      
      // 3. Khóa orientation portrait
      await SystemChrome.setPreferredOrientations([
        DeviceOrientation.portraitUp,
      ]);
      
      // 4. Bật wakelock - không cho màn hình tắt
      await WakelockPlus.enable();
      
      // 5. Set system UI style
      SystemChrome.setSystemUIOverlayStyle(
        const SystemUiOverlayStyle(
          statusBarColor: Colors.transparent,
          statusBarIconBrightness: Brightness.dark,
          systemNavigationBarColor: Colors.transparent,
          systemNavigationBarIconBrightness: Brightness.dark,
        ),
      );
      
      _isKioskEnabled = success;
      print('🔒 Full Kiosk mode enabled: $success');
      
    } catch (e) {
      print('❌ Error enabling kiosk mode: $e');
      // Fallback: chỉ ẩn UI và lock orientation
      await _enableBasicKioskMode();
    }
  }
  
  /// Fallback kiosk mode cơ bản
  static Future<void> _enableBasicKioskMode() async {
    try {
      // Ẩn system UI
      await SystemChrome.setEnabledSystemUIMode(
        SystemUiMode.immersiveSticky,
        overlays: [],
      );
      
      // Lock orientation
      await SystemChrome.setPreferredOrientations([
        DeviceOrientation.portraitUp,
      ]);
      
      // Enable wakelock
      await WakelockPlus.enable();
      
      _isKioskEnabled = true;
      print('🔒 Basic kiosk mode enabled');
    } catch (e) {
      print('❌ Error enabling basic kiosk mode: $e');
    }
  }
  
  /// Tắt Kiosk Mode (chỉ dành cho admin)
  static Future<void> disableKioskMode() async {
    try {
      // 1. Stop kiosk mode
      final result = await kiosk.stopKioskMode();
      print('🔓 Kiosk mode stop result: $result');
      
      // 2. Khôi phục system UI
      await SystemChrome.setEnabledSystemUIMode(
        SystemUiMode.manual,
        overlays: SystemUiOverlay.values,
      );
      
      // 3. Cho phép tất cả orientations
      await SystemChrome.setPreferredOrientations([
        DeviceOrientation.portraitUp,
        DeviceOrientation.portraitDown,
        DeviceOrientation.landscapeLeft,
        DeviceOrientation.landscapeRight,
      ]);
      
      // 4. Tắt wakelock
      await WakelockPlus.disable();
      
      _isKioskEnabled = false;
      print('🔓 Kiosk mode disabled successfully');
      
    } catch (e) {
      print('❌ Error disabling kiosk mode: $e');
    }
  }
  
  /// Kiểm tra trạng thái kiosk mode
  static Future<bool> isInKioskMode() async {
    try {
      final mode = await kiosk.getKioskMode();
      return mode == kiosk.KioskMode.enabled;
    } catch (e) {
      print('❌ Error checking kiosk mode status: $e');
      return _isKioskEnabled; // Fallback to local state
    }
  }
  
  /// Kiểm tra managed kiosk (cho enterprise)
  static Future<bool> isManagedKiosk() async {
    try {
      return await kiosk.isManagedKiosk();
    } catch (e) {
      print('❌ Error checking managed kiosk: $e');
      return false;
    }
  }
  
  /// Lấy thông tin chi tiết kiosk mode
  static Future<Map<String, dynamic>> getKioskInfo() async {
    try {
      final mode = await kiosk.getKioskMode();
      final isInKiosk = mode == kiosk.KioskMode.enabled;
      final isManagedKioskMode = await kiosk.isManagedKiosk();
      
      return {
        'isSupported': true,
        'isInKioskMode': isInKiosk,
        'isManagedKiosk': isManagedKioskMode,
        'currentMode': mode.toString(),
        'isServiceEnabled': _isKioskEnabled,
        'isInitialized': _isInitialized,
      };
    } catch (e) {
      print('❌ Error getting kiosk info: $e');
      return {
        'isSupported': false,
        'isInKioskMode': false,
        'isManagedKiosk': false,
        'currentMode': 'unknown',
        'isServiceEnabled': _isKioskEnabled,
        'isInitialized': _isInitialized,
      };
    }
  }
  
  /// Watch kiosk mode changes
  static Stream<kiosk.KioskMode> watchKioskMode() {
    try {
      return kiosk.watchKioskMode(
        androidQueryPeriod: const Duration(seconds: 3),
      );
    } catch (e) {
      print('❌ Error watching kiosk mode: $e');
      // Return empty stream as fallback
      return Stream.empty();
    }
  }
  
  /// Force refresh kiosk mode (khi app resume)
  static Future<void> refreshKioskMode() async {
    if (_isKioskEnabled) {
      await Future.delayed(const Duration(milliseconds: 300));
      await enableKioskMode();
    }
  }
}