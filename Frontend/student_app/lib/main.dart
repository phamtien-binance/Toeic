import 'package:flutter/material.dart';

import 'screens/login_screen.dart';
import 'theme.dart';

void main() {
  runApp(const ToeicStudentApp());
}

class ToeicStudentApp extends StatelessWidget {
  const ToeicStudentApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'TOEIC Reading',
      debugShowCheckedModeBanner: false,
      theme: buildTheme(),
      home: const LoginScreen(),
    );
  }
}
