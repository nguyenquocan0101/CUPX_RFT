import 'package:flutter/widgets.dart';
import 'dart:ui' as ui;

class SizeConfig {
  static double screenWidth = 0;
  static double screenHeight = 0;
  static double fem = 1;
  static double hem = 1;
  static double ffem = 1;

  // Giá trị tham chiếu thiết kế
  static const double _designWidth = 600.0;
  static const double _designHeight = 1000.0;

  static void init(BuildContext context) {
    // Lấy kích thước màn hình từ MediaQuery
    screenWidth = MediaQuery.of(context).size.width;
    screenHeight = MediaQuery.of(context).size.height;

    // Tính toán tỷ lệ dựa trên kích thước thiết kế cố định
    fem = screenWidth / _designWidth;
    hem = screenHeight / _designHeight;
    ffem = fem * 0.97;

    print('Thông số màn hình: ${screenWidth}x${screenHeight}');
    print('Tỷ lệ fem: $fem, hem: $hem, ffem: $ffem');
  }

  // Đọc kích thước màn hình không cần BuildContext (có thể gọi trước khi có context)
  static void preInit() {
    final window = ui.window;
    final pixelRatio = window.devicePixelRatio;
    final logicalSize = window.physicalSize / pixelRatio;

    screenWidth = logicalSize.width;
    screenHeight = logicalSize.height;

    fem = screenWidth / _designWidth;
    hem = screenHeight / _designHeight;
    ffem = fem * 0.97;

    print('Thông số màn hình (preInit): ${screenWidth}x${screenHeight}');
    print('Tỷ lệ fem: $fem, hem: $hem, ffem: $ffem');
  }
}