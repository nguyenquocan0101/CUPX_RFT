import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

abstract class AppTypography {
  static final displayLarge = GoogleFonts.inter(
    fontSize: 48,
    fontWeight: FontWeight.w700,
    letterSpacing: -0.5,
  );
  static final headlineMedium = GoogleFonts.inter(
    fontSize: 32,
    fontWeight: FontWeight.w700,
    letterSpacing: -0.25,
  );
  static final titleLarge = GoogleFonts.inter(
    fontSize: 28,
    fontWeight: FontWeight.w700,
    letterSpacing: 0,
  );
  static final titleMedium = GoogleFonts.inter(
    fontSize: 20,
    fontWeight: FontWeight.w400,
    letterSpacing: 0.15,
  );
  static final bodyLarge = GoogleFonts.inter(
    fontSize: 20,
    fontWeight: FontWeight.w400,
    letterSpacing: 0.15,
    height: 1.5,
  );
  static final bodyMedium = GoogleFonts.inter(
    fontSize: 14,
    fontWeight: FontWeight.w400,
    letterSpacing: 0.25,
    height: 1.4,
  );
  static final labelLarge = GoogleFonts.inter(
    fontSize: 14,
    fontWeight: FontWeight.w700,
    letterSpacing: 0.1,
  );
  static final labelSmall = GoogleFonts.inter(
    fontSize: 12,
    fontWeight: FontWeight.w400,
    letterSpacing: 0.4,
  );

  //*Bổ sung
  // Tiêu đề chính của màn hình - Thanh đơn giản, rõ ràng cho tablet
  static final screenTitle = GoogleFonts.inter(
    fontSize: 28,
    fontWeight: FontWeight.w600,
    letterSpacing: -0.5,
    height: 1.2,
  );

  // Tên sản phẩm - Nổi bật với kích thước lớn hơn cho tablet
  static final productName = GoogleFonts.inter(
    fontSize: 36,
    fontWeight: FontWeight.w700,
    letterSpacing: -0.5,
    height: 1.1,
  );

  // Mô tả sản phẩm - Tăng font size cho dễ đọc trên tablet
  static final productDescription = GoogleFonts.inter(
    fontSize: 18,
    fontWeight: FontWeight.w400,
    letterSpacing: 0,
    height: 1.6,
  );
  // Mô tả sản phẩm - Tăng font size cho dễ đọc trên tablet
  static final productPrice = GoogleFonts.inter(
    fontSize: 24,
    fontWeight: FontWeight.w400,
    letterSpacing: 0,
    height: 1.6,
  );

  // Tiêu đề các mục - Tạo sự tương phản giữa các section
  static final sectionTitle = GoogleFonts.inter(
    fontSize: 20,
    fontWeight: FontWeight.w600,
    letterSpacing: 0,
    height: 1.3,
  );

  // Text thông thường - Dễ đọc trên tablet
  static final normal = GoogleFonts.inter(
    fontSize: 16,
    fontWeight: FontWeight.w400,
    letterSpacing: 0,
    height: 1.5,
  );
}
