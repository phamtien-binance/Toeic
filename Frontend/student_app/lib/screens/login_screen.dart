import 'package:flutter/material.dart';

import '../theme.dart';
import 'exams_screen.dart';

class LoginScreen extends StatefulWidget {
  const LoginScreen({super.key});

  @override
  State<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends State<LoginScreen> {
  final _code = TextEditingController(text: '2380614025');
  final _name = TextEditingController(text: 'Phạm Duy Tiên');

  void _enter() {
    if (_code.text.trim().isEmpty) return;
    Navigator.of(context).push(MaterialPageRoute(
      builder: (_) => ExamsScreen(studentCode: _code.text.trim(), studentName: _name.text.trim()),
    ));
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Colors.white,
      body: SafeArea(
        child: SingleChildScrollView(
          padding: const EdgeInsets.fromLTRB(24, 40, 24, 24),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Container(
                width: 54,
                height: 54,
                decoration: BoxDecoration(
                  gradient: const LinearGradient(colors: [AppColors.blue600, AppColors.blue700]),
                  borderRadius: BorderRadius.circular(16),
                ),
                alignment: Alignment.center,
                child: const Text('TR',
                    style: TextStyle(color: Colors.white, fontWeight: FontWeight.w800, fontSize: 15)),
              ),
              const SizedBox(height: 28),
              const Text('Chào mừng trở lại',
                  style: TextStyle(fontSize: 27, fontWeight: FontWeight.w800, letterSpacing: -0.8, color: AppColors.ink)),
              const SizedBox(height: 8),
              const Text('Đăng nhập để vào phòng thi Reading của bạn.',
                  style: TextStyle(color: AppColors.muted, fontSize: 13.5)),
              const SizedBox(height: 28),
              const Text('Mã số sinh viên',
                  style: TextStyle(fontSize: 12.5, fontWeight: FontWeight.w600, color: AppColors.ink2)),
              const SizedBox(height: 6),
              TextField(controller: _code, decoration: const InputDecoration(hintText: 'VD: 2380614025')),
              const SizedBox(height: 14),
              const Text('Họ và tên',
                  style: TextStyle(fontSize: 12.5, fontWeight: FontWeight.w600, color: AppColors.ink2)),
              const SizedBox(height: 6),
              TextField(controller: _name, decoration: const InputDecoration(hintText: 'VD: Phạm Duy Tiên')),
              const SizedBox(height: 22),
              FilledButton(onPressed: _enter, child: const Text('Đăng nhập')),
              const SizedBox(height: 14),
              Container(
                padding: const EdgeInsets.all(14),
                decoration: BoxDecoration(color: AppColors.blue50, borderRadius: BorderRadius.circular(12)),
                child: Row(children: const [
                  Icon(Icons.info_outline, size: 18, color: AppColors.blue700),
                  SizedBox(width: 10),
                  Expanded(
                    child: Text(
                      'Dùng mã số sinh viên được cấp. Điểm và bài làm sẽ được lưu theo mã này.',
                      style: TextStyle(fontSize: 12, color: AppColors.blue700, height: 1.5),
                    ),
                  ),
                ]),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
