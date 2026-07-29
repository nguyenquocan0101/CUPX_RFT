import 'package:abc_androidapp/config/themes/app_color_extension.dart';
import 'package:abc_androidapp/config/themes/app_palette.dart';
import 'package:abc_androidapp/config/themes/app_typography.dart';
import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

/// AppTheme defines both Light and Dark themes using AppColorsExtension.
class AppTheme {
  // Light theme

  static final light = () {
    final defaultTheme = ThemeData.light();

    return defaultTheme.copyWith(
      textTheme: GoogleFonts.notoSansTextTheme(defaultTheme.textTheme).copyWith(
        displayLarge: AppTypography.displayLarge.copyWith(color: Colors.black),
        headlineMedium:
            AppTypography.headlineMedium.copyWith(color: Colors.black),
        titleLarge: AppTypography.titleLarge.copyWith(color: Colors.black),
        titleMedium: AppTypography.titleMedium.copyWith(color: Colors.black),
        bodyLarge: AppTypography.bodyLarge.copyWith(color: Colors.black),
        bodyMedium: AppTypography.bodyMedium.copyWith(color: Colors.black),
        labelLarge: AppTypography.labelLarge.copyWith(color: Colors.black),
        labelSmall: AppTypography.labelSmall.copyWith(color: Colors.black),
      ),
      extensions: [
        _lightAppColors,
      ],
    );
  }(); //().. call closure to run immediately

  // Define light color values
  //* how to use: final colors = Theme.of(context).extension<AppColorsExtension>()!;
  static final AppColorsExtension _lightAppColors = AppColorsExtension(
    //for chosen section or button background
    primary: AppPalette.blue.primary,
    onPrimary: AppPalette.black, //icon or text of primary

    //for not chosen section or button background
    secondary: AppPalette.blue.blue2,
    onSecondary: AppPalette.black, //icon or text of secondary

    error: AppPalette.red,
    onError: AppPalette.white,

    background: AppPalette.grey.grey1,
    onBackground: AppPalette.black,

    surface: AppPalette.white,
    onSurface: AppPalette.black,

    menuHeaderBackground: AppPalette.grey.grey1,
    cardBackground: AppPalette.white,

    bottomBarBackground: AppPalette.white,
  );
}
