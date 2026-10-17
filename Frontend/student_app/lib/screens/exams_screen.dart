import 'package:flutter/material.dart';

import '../api_client.dart';
import '../models.dart';
import '../theme.dart';
import 'result_screen.dart';
import 'taking_screen.dart';

class ExamsScreen extends StatefulWidget {
  const ExamsScreen({super.key, required this.studentCode, required this.studentName});

  final String studentCode;
  final String studentName;

  @override
  State<ExamsScreen> createState() => _ExamsScreenState();
}

class _ExamsScreenState extends State<ExamsScreen> {
  late Future<List<StudentExam>> _future;

  @override
  void initState() {
    super.initState();
    _future = ApiClient.instance.studentExams(widget.studentCode);
  }

  void _refresh() => setState(() => _future = ApiClient.instance.studentExams(widget.studentCode));

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.bg,
      body: RefreshIndicator(
        onRefresh: () async => _refresh(),
        child: CustomScrollView(
          slivers: [
            SliverToBoxAdapter(child: _header()),
            FutureBuilder<List<StudentExam>>(
              future: _future,
              builder: (context, snap) {
                if (snap.connectionState == ConnectionState.waiting) {
                  return const SliverFillRemaining(
                    child: Center(child: CircularProgressIndicator(color: AppColors.blue600)),
                  );
                }
                if (snap.hasError) {
                  return SliverFillRemaining(child: _error(snap.error.toString()));
                }
                final exams = snap.data ?? [];
                if (exams.isEmpty) {
                  return const SliverFillRemaining(
                    child: Center(
                      child: Text('Chưa có đề thi nào được xuất bản.',
                          style: TextStyle(color: AppColors.muted)),
                    ),
                  );
                }
                return SliverPadding(
                  padding: const EdgeInsets.fromLTRB(16, 0, 16, 24),
                  sliver: SliverList.separated(
                    itemCount: exams.length,
                    separatorBuilder: (_, __) => const SizedBox(height: 12),
                    itemBuilder: (_, i) => i == 0 ? _heroCard(exams[i]) : _examCard(exams[i]),
                  ),
                );
              },
            ),
          ],
        ),
      ),
    );
  }

  Widget _header() {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.fromLTRB(20, 18, 20, 26),
      decoration: const BoxDecoration(
        gradient: LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: [Color(0xFF2E7BEA), AppColors.blue700],
        ),
        borderRadius: BorderRadius.vertical(bottom: Radius.circular(22)),
      ),
      child: SafeArea(
        bottom: false,
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      const Text('Xin chào,', style: TextStyle(color: Color(0xFFD5E4FC), fontSize: 12)),
                      const SizedBox(height: 2),
                      Text(widget.studentName.isEmpty ? 'Sinh viên' : widget.studentName,
                          style: const TextStyle(
                              color: Colors.white, fontSize: 19, fontWeight: FontWeight.w700, letterSpacing: -0.4)),
                    ],
                  ),
                ),
                CircleAvatar(
                  radius: 19,
                  backgroundColor: Colors.white.withValues(alpha: 0.18),
                  child: Text(_initials(widget.studentName),
                      style: const TextStyle(color: Colors.white, fontWeight: FontWeight.w700, fontSize: 12)),
                ),
              ],
            ),
            const SizedBox(height: 14),
            Wrap(spacing: 8, runSpacing: 8, children: [
              _chip('MSSV ${widget.studentCode}'),
              _chip('Lớp 23DTHD5'),
            ]),
          ],
        ),
      ),
    );
  }

  String _initials(String name) {
    final parts = name.trim().split(RegExp(r'\s+')).where((e) => e.isNotEmpty).toList();
    if (parts.isEmpty) return 'SV';
    if (parts.length == 1) return parts.first.characters.take(2).toString().toUpperCase();
    return (parts.first.characters.first + parts.last.characters.first).toUpperCase();
  }

  Widget _chip(String text) => Container(
        padding: const EdgeInsets.symmetric(horizontal: 11, vertical: 5),
        decoration: BoxDecoration(
          color: Colors.white.withValues(alpha: 0.16),
          borderRadius: BorderRadius.circular(999),
        ),
        child: Text(text, style: const TextStyle(color: Colors.white, fontSize: 11, fontWeight: FontWeight.w600)),
      );

  Widget _heroCard(StudentExam exam) {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        gradient: const LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: [Color(0xFF2E7BEA), AppColors.blue700],
        ),
        borderRadius: BorderRadius.circular(18),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(exam.title,
              style: const TextStyle(color: Colors.white, fontSize: 16.5, fontWeight: FontWeight.w700)),
          const SizedBox(height: 6),
          Text('${exam.totalQuestions} câu · ${exam.durationMinutes} phút · Part 5 · 6 · 7',
              style: const TextStyle(color: Color(0xFFD5E4FC), fontSize: 11.5)),
          const SizedBox(height: 14),
          if (exam.alreadyAttempted)
            Row(children: [
              _softBadge('Đã làm · ${exam.lastScore ?? 0} điểm'),
              const SizedBox(width: 8),
              TextButton(
                onPressed: () => Navigator.of(context).push(MaterialPageRoute(
                  builder: (_) => ResultScreen(attemptId: exam.lastAttemptId!, examTitle: exam.title),
                )),
                child: const Text('Xem lại điểm', style: TextStyle(color: Colors.white)),
              ),
            ])
          else
            SizedBox(
              width: double.infinity,
              child: FilledButton(
                style: FilledButton.styleFrom(
                  backgroundColor: Colors.white,
                  foregroundColor: AppColors.blue700,
                  minimumSize: const Size.fromHeight(46),
                ),
                onPressed: () => _start(exam),
                child: const Text('Bắt đầu làm bài'),
              ),
            ),
        ],
      ),
    );
  }

  Widget _softBadge(String text) => Container(
        padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
        decoration: BoxDecoration(
          color: Colors.white.withValues(alpha: 0.18),
          borderRadius: BorderRadius.circular(999),
        ),
        child: Text(text, style: const TextStyle(color: Colors.white, fontSize: 11, fontWeight: FontWeight.w600)),
      );

  Widget _examCard(StudentExam exam) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(14),
        child: Row(
          children: [
            Container(
              width: 44,
              height: 44,
              decoration: BoxDecoration(
                color: AppColors.blue50,
                borderRadius: BorderRadius.circular(12),
              ),
              alignment: Alignment.center,
              child: Text(exam.title.split(' ').last.characters.take(2).toString().toUpperCase(),
                  style: const TextStyle(
                      color: AppColors.blue700, fontWeight: FontWeight.w800, fontSize: 12)),
            ),
            const SizedBox(width: 12),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(exam.title,
                      style: const TextStyle(fontSize: 14, fontWeight: FontWeight.w600, color: AppColors.ink)),
                  const SizedBox(height: 3),
                  Text('${exam.totalQuestions} câu · ${exam.durationMinutes} phút',
                      style: const TextStyle(fontSize: 11.5, color: AppColors.muted)),
                ],
              ),
            ),
            exam.alreadyAttempted
                ? TextButton(
                    onPressed: () => Navigator.of(context).push(MaterialPageRoute(
                      builder: (_) => ResultScreen(attemptId: exam.lastAttemptId!, examTitle: exam.title),
                    )),
                    child: Text('${exam.lastScore ?? 0} điểm'),
                  )
                : OutlinedButton(onPressed: () => _start(exam), child: const Text('Làm bài')),
          ],
        ),
      ),
    );
  }

  Future<void> _start(StudentExam exam) async {
    showDialog(
      context: context,
      barrierDismissible: false,
      builder: (_) => const Center(child: CircularProgressIndicator(color: AppColors.blue600)),
    );
    try {
      final session = await ApiClient.instance.startAttempt(exam.id, widget.studentCode, widget.studentName);
      if (!mounted) return;
      Navigator.of(context).pop();
      final result = await Navigator.of(context).push(MaterialPageRoute(
        builder: (_) => TakingScreen(session: session, studentCode: widget.studentCode),
      ));
      if (result == true) _refresh();
    } catch (e) {
      if (!mounted) return;
      Navigator.of(context).pop();
      ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('Không mở được đề: $e')));
    }
  }

  Widget _error(String message) {
    return Padding(
      padding: const EdgeInsets.all(24),
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          const Icon(Icons.cloud_off, size: 42, color: AppColors.muted),
          const SizedBox(height: 12),
          const Text('Không kết nối được máy chủ', style: TextStyle(fontWeight: FontWeight.w700)),
          const SizedBox(height: 6),
          Text(message, textAlign: TextAlign.center, style: const TextStyle(fontSize: 12, color: AppColors.muted)),
          const SizedBox(height: 16),
          FilledButton(onPressed: _refresh, child: const Text('Thử lại')),
        ],
      ),
    );
  }
}
