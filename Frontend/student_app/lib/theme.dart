import 'package:flutter/material.dart';

/// Design tokens shared with the HTML prototype (white + blue).
class AppColors {
  static const blue600 = Color(0xFF1D6FE0);
  static const blue700 = Color(0xFF1450B4);
  static const blue500 = Color(0xFF3B8BF2);
  static const blue50 = Color(0xFFEEF4FF);
  static const blue100 = Color(0xFFDCE9FC);
  static const ink = Color(0xFF0F1F3D);
  static const ink2 = Color(0xFF33415C);
  static const muted = Color(0xFF6B7A90);
  static const line = Color(0xFFE3EAF5);
  static const bg = Color(0xFFF6F9FE);
  static const green = Color(0xFF12A150);
  static const greenBg = Color(0xFFE7F7EE);
  static const amber = Color(0xFFB4690E);
  static const amberBg = Color(0xFFFEF3E2);
  static const red = Color(0xFFD92D20);
  static const redBg = Color(0xFFFDECEA);
}

ThemeData buildTheme() {
  final scheme = ColorScheme.fromSeed(
    seedColor: AppColors.blue600,
    primary: AppColors.blue600,
    surface: Colors.white,
  );

  return ThemeData(
    useMaterial3: true,
    colorScheme: scheme,
    scaffoldBackgroundColor: AppColors.bg,
    fontFamily: 'Roboto',
    appBarTheme: const AppBarTheme(
      backgroundColor: Colors.white,
      foregroundColor: AppColors.ink,
      elevation: 0,
      centerTitle: false,
      surfaceTintColor: Colors.white,
      titleTextStyle: TextStyle(
        color: AppColors.ink,
        fontSize: 17,
        fontWeight: FontWeight.w700,
        letterSpacing: -0.2,
      ),
    ),
    cardTheme: CardThemeData(
      color: Colors.white,
      elevation: 0,
      margin: EdgeInsets.zero,
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(16),
        side: const BorderSide(color: AppColors.line),
      ),
    ),
    filledButtonTheme: FilledButtonThemeData(
      style: FilledButton.styleFrom(
        backgroundColor: AppColors.blue600,
        foregroundColor: Colors.white,
        minimumSize: const Size.fromHeight(50),
        textStyle: const TextStyle(fontSize: 15, fontWeight: FontWeight.w600),
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
      ),
    ),
    inputDecorationTheme: InputDecorationTheme(
      filled: true,
      fillColor: Colors.white,
      contentPadding: const EdgeInsets.symmetric(horizontal: 14, vertical: 14),
      border: OutlineInputBorder(
        borderRadius: BorderRadius.circular(12),
        borderSide: const BorderSide(color: AppColors.line),
      ),
      enabledBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(12),
        borderSide: const BorderSide(color: AppColors.line),
      ),
      focusedBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(12),
        borderSide: const BorderSide(color: AppColors.blue500, width: 1.6),
      ),
    ),
    dividerTheme: const DividerThemeData(color: AppColors.line, thickness: 1, space: 1),
    chipTheme: ChipThemeData(
      backgroundColor: AppColors.blue50,
      labelStyle: const TextStyle(color: AppColors.blue700, fontSize: 11.5, fontWeight: FontWeight.w600),
      side: BorderSide.none,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(999)),
    ),
  );
}
