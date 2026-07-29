import 'package:flutter/material.dart';

// App-wide color palette
abstract class AppPalette {
  static const red = Colors.red;
  static const transparent = Colors.transparent;
  static const white = Color(0xFFFFFFFF);
  static const black = Color(0xFF000000);
  static const yellow = _Yellow();
  static const blue = _Blue();
  static const grey = _Grey();
}

class _Yellow {
  const _Yellow();

  final yellow1 = const Color(0xFFfff8e5);
  final yellow2 = const Color(0xFFffebb8);
  final primary = const Color(0xFFfddf85); //yellow3
  final yellow4 = const Color(0xFFfddf85);
  final yellow5 = const Color(0xFF998552);
}

class _Blue {
  const _Blue();

  final blue1 = const Color(0xFFE0F3FC);
  final blue2 = const Color(0xFFA4D9F3);
  final primary = const Color(0xFF57B7E7); // primary
  final blue4 = const Color(0xFF2C7FB0);
}

class _Grey {
  const _Grey();
  final grey0 = const Color(0xFFf3f3f3);
  final grey1 = const Color(0xFFe8e8e8);
  final grey2 = const Color(0xFF999ca1);
  final grey3 = const Color(0xFF3f4042);
  final grey4 = const Color(0xFF1c1c1e);
  final grey5 = const Color(0xFF0a0a0a);
}
