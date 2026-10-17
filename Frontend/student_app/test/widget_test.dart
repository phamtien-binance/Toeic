import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';

import 'package:student_app/main.dart';

void main() {
  testWidgets('Login screen shows the student form', (WidgetTester tester) async {
    await tester.pumpWidget(const ToeicStudentApp());
    expect(find.text('Chào mừng trở lại'), findsOneWidget);
    expect(find.text('Mã số sinh viên'), findsOneWidget);
    expect(find.widgetWithText(FilledButton, 'Đăng nhập'), findsOneWidget);
  });
}
