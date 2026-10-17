import 'dart:convert';
import 'dart:io' show Platform;

import 'package:flutter/foundation.dart' show kIsWeb;
import 'package:http/http.dart' as http;

import 'models.dart';

/// Thin client over the ABP dynamic Web API (ExamAppService / AttemptAppService).
class ApiClient {
  ApiClient._();
  static final ApiClient instance = ApiClient._();

  /// 10.0.2.2 is the host loopback as seen from the Android emulator.
  static String get baseUrl {
    const override = String.fromEnvironment('API_BASE');
    if (override.isNotEmpty) return override;
    if (!kIsWeb && Platform.isAndroid) return 'http://10.0.2.2:5000';
    return 'http://localhost:5000';
  }

  final http.Client _http = http.Client();

  dynamic _unwrap(http.Response r) {
    if (r.statusCode >= 400) {
      throw ApiException('HTTP ${r.statusCode}: ${r.body}');
    }
    final body = jsonDecode(r.body) as Map<String, dynamic>;
    if (body['success'] == false) {
      final err = body['error'];
      throw ApiException(err is Map ? (err['message']?.toString() ?? 'Lỗi không xác định') : 'Lỗi không xác định');
    }
    return body['result'];
  }

  Future<T> _get<T>(String path, T Function(dynamic) parse) async {
    final r = await _http.get(Uri.parse('$baseUrl/api/services/app/$path'));
    return parse(_unwrap(r));
  }

  Future<T> _post<T>(String path, Map<String, dynamic> body, T Function(dynamic) parse) async {
    final r = await _http.post(
      Uri.parse('$baseUrl/api/services/app/$path'),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode(body),
    );
    return parse(_unwrap(r));
  }

  Future<List<StudentExam>> studentExams(String studentCode) => _get(
        'Exam/GetStudentExams?studentCode=$studentCode',
        (d) => ((d as List?) ?? [])
            .map((e) => StudentExam.fromJson(e as Map<String, dynamic>))
            .toList(),
      );

  Future<AttemptSession> startAttempt(int examId, String code, String name) => _post(
        'Attempt/Start',
        {'examId': examId, 'studentCode': code, 'studentName': name},
        (d) => AttemptSession.fromJson(d as Map<String, dynamic>),
      );

  Future<void> saveAnswers(int attemptId, Map<int, String> answers) => _post(
        'Attempt/SaveAnswers',
        {
          'attemptId': attemptId,
          'answers': answers.entries
              .map((e) => {'questionId': e.key, 'selectedKey': e.value})
              .toList()
        },
        (_) => null,
      );

  Future<AttemptResult> submit(int attemptId, {bool auto = false}) => _post(
        'Attempt/Submit',
        {'attemptId': attemptId, 'autoSubmitted': auto},
        (d) => AttemptResult.fromJson(d as Map<String, dynamic>),
      );

  Future<AttemptResult> result(int attemptId) => _get(
        'Attempt/GetResult?attemptId=$attemptId',
        (d) => AttemptResult.fromJson(d as Map<String, dynamic>),
      );
}

class ApiException implements Exception {
  ApiException(this.message);
  final String message;
  @override
  String toString() => message;
}
