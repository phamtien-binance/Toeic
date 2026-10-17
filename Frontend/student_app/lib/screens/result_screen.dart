import 'package:flutter/material.dart';

import '../api_client.dart';
import '../models.dart';
import '../theme.dart';

class ResultScreen extends StatefulWidget {
  const ResultScreen({super.key, required this.attemptId, required this.examTitle, this.initial});

  final int attemptId;
  final String examTitle;
  final AttemptResult? initial;

  @override
  State<ResultScreen> createState() => _ResultScreenState();
}

class _ResultScreenState extends State<ResultScreen> {
  late Future<AttemptResult> _future;
  String _filter = 'all';

  @override
  void initState() {
    super.initState();
    _future = widget.initial != null
        ? Future.value(widget.initial)
        : ApiClient.instance.result(widget.attemptId);
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.bg,
      appBar: AppBar(
        title: const Text('Kết quả bài làm'),
        leading: IconButton(
          icon: const Icon(Icons.arrow_back),
          onPressed: () => Navigator.of(context).pop(true),
        ),
      ),
      body: FutureBuilder<AttemptResult>(
        future: _future,
        builder: (context, snap) {
          if (!snap.hasData) {
            if (snap.hasError) {
              return Center(
                child: Padding(
                  padding: const EdgeInsets.all(24),
                  child: Text('Không tải được kết quả: ${snap.error}',
                      style: const TextStyle(color: AppColors.muted)),
                ),
              );
            }
            return const Center(child: CircularProgressIndicator(color: AppColors.blue600));
          }
          final r = snap.data!;
          final filtered = r.questions.where((q) {
            switch (_filter) {
              case 'correct':
                return q.isCorrect == true;
              case 'wrong':
                return q.isCorrect == false;
              case 'skipped':
                return q.isCorrect == null;
              default:
                return true;
            }
          }).toList();

          return ListView(
            padding: const EdgeInsets.fromLTRB(16, 16, 16, 28),
            children: [
              _scoreCard(r),
              const SizedBox(height: 14),
              _counters(r),
              const SizedBox(height: 14),
              _partsCard(r),
              const SizedBox(height: 14),
              Row(
                children: [
                  const Text('Xem lại đáp án',
                      style: TextStyle(fontSize: 14, fontWeight: FontWeight.w700, color: AppColors.ink)),
                  const Spacer(),
                  Text('${filtered.length} câu', style: const TextStyle(fontSize: 12, color: AppColors.muted)),
                ],
              ),
              const SizedBox(height: 10),
              SingleChildScrollView(
                scrollDirection: Axis.horizontal,
                child: Row(children: [
                  _filterChip('all', 'Tất cả'),
                  const SizedBox(width: 8),
                  _filterChip('correct', 'Đúng ${r.correctCount}'),
                  const SizedBox(width: 8),
                  _filterChip('wrong', 'Sai ${r.wrongCount}'),
                  const SizedBox(width: 8),
                  _filterChip('skipped', 'Bỏ qua ${r.skippedCount}'),
                ]),
              ),
              const SizedBox(height: 12),
              ...filtered.take(60).map((q) => _reviewCard(q, r)),
              if (filtered.length > 60)
                Padding(
                  padding: const EdgeInsets.only(top: 12),
                  child: Text('Hiển thị 60/${filtered.length} câu đầu tiên.',
                      textAlign: TextAlign.center,
                      style: const TextStyle(fontSize: 12, color: AppColors.muted)),
                ),
            ],
          );
        },
      ),
    );
  }

  Widget _filterChip(String key, String label) {
    final on = _filter == key;
    return InkWell(
      onTap: () => setState(() => _filter = key),
      borderRadius: BorderRadius.circular(999),
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: 13, vertical: 7),
        decoration: BoxDecoration(
          color: on ? AppColors.blue600 : Colors.white,
          borderRadius: BorderRadius.circular(999),
          border: Border.all(color: on ? AppColors.blue600 : AppColors.line),
        ),
        child: Text(label,
            style: TextStyle(
                fontSize: 12, fontWeight: FontWeight.w600, color: on ? Colors.white : AppColors.ink2)),
      ),
    );
  }

  Widget _scoreCard(AttemptResult r) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.symmetric(vertical: 22, horizontal: 16),
        child: Column(
          children: [
            Text(r.examTitle,
                textAlign: TextAlign.center,
                style: const TextStyle(fontSize: 12.5, color: AppColors.muted)),
            const SizedBox(height: 16),
            SizedBox(
              width: 138,
              height: 138,
              child: Stack(
                alignment: Alignment.center,
                children: [
                  SizedBox(
                    width: 138,
                    height: 138,
                    child: CircularProgressIndicator(
                      value: r.totalQuestions == 0 ? 0 : r.correctCount / r.totalQuestions,
                      strokeWidth: 12,
                      backgroundColor: const Color(0xFFE6EEFB),
                      valueColor: const AlwaysStoppedAnimation(AppColors.blue600),
                      strokeCap: StrokeCap.round,
                    ),
                  ),
                  Column(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      Text('${r.score}',
                          style: const TextStyle(
                              fontSize: 32, fontWeight: FontWeight.w800, letterSpacing: -1, color: AppColors.ink)),
                      Text('/ 100 điểm',
                          style: const TextStyle(fontSize: 11, color: AppColors.muted, fontWeight: FontWeight.w600)),
                    ],
                  ),
                ],
              ),
            ),
            const SizedBox(height: 14),
            Text('${r.correctCount}/${r.totalQuestions} câu đúng',
                style: const TextStyle(fontSize: 13.5, fontWeight: FontWeight.w700, color: AppColors.ink)),
            const SizedBox(height: 4),
            Text('Thời gian làm bài: ${(r.durationSeconds / 60).round()} phút',
                style: const TextStyle(fontSize: 11.5, color: AppColors.muted)),
          ],
        ),
      ),
    );
  }

  Widget _counters(AttemptResult r) {
    Widget cell(int value, String label, Color color) => Expanded(
          child: Container(
            padding: const EdgeInsets.symmetric(vertical: 14),
            decoration: BoxDecoration(
              color: Colors.white,
              borderRadius: BorderRadius.circular(14),
              border: Border.all(color: AppColors.line),
            ),
            child: Column(children: [
              Text('$value', style: TextStyle(fontSize: 20, fontWeight: FontWeight.w800, color: color)),
              const SizedBox(height: 2),
              Text(label, style: const TextStyle(fontSize: 10.5, color: AppColors.muted, fontWeight: FontWeight.w600)),
            ]),
          ),
        );
    return Row(children: [
      cell(r.correctCount, 'Đúng', AppColors.green),
      const SizedBox(width: 10),
      cell(r.wrongCount, 'Sai', AppColors.red),
      const SizedBox(width: 10),
      cell(r.skippedCount, 'Bỏ qua', AppColors.muted),
    ]);
  }

  Widget _partsCard(AttemptResult r) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const Text('Chi tiết theo phần',
                style: TextStyle(fontSize: 13, fontWeight: FontWeight.w700, color: AppColors.ink)),
            const SizedBox(height: 14),
            ...r.parts.map((p) {
              final ratio = p.total == 0 ? 0.0 : p.correct / p.total;
              return Padding(
                padding: const EdgeInsets.only(bottom: 12),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(children: [
                      Text('Part ${p.part}',
                          style: const TextStyle(fontSize: 12.5, fontWeight: FontWeight.w600, color: AppColors.ink2)),
                      const Spacer(),
                      Text('${p.correct}/${p.total}',
                          style: const TextStyle(fontSize: 12.5, fontWeight: FontWeight.w700, color: AppColors.ink)),
                    ]),
                    const SizedBox(height: 6),
                    ClipRRect(
                      borderRadius: BorderRadius.circular(99),
                      child: LinearProgressIndicator(
                        value: ratio,
                        minHeight: 8,
                        backgroundColor: const Color(0xFFEFF3FA),
                        valueColor: AlwaysStoppedAnimation(
                          ratio >= 0.7 ? AppColors.blue600 : (ratio >= 0.4 ? AppColors.amber : AppColors.red),
                        ),
                      ),
                    ),
                  ],
                ),
              );
            }),
          ],
        ),
      ),
    );
  }

  Widget _reviewCard(QuestionItem q, AttemptResult r) {
    final good = q.isCorrect == true;
    final skipped = q.isCorrect == null;
    final color = skipped ? AppColors.muted : (good ? AppColors.green : AppColors.red);
    final bg = skipped ? const Color(0xFFF1F4F9) : (good ? AppColors.greenBg : AppColors.redBg);

    return Padding(
      padding: const EdgeInsets.only(bottom: 10),
      child: Card(
        child: Padding(
          padding: const EdgeInsets.all(14),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(children: [
                Container(
                  padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
                  decoration: BoxDecoration(color: bg, borderRadius: BorderRadius.circular(6)),
                  child: Text('Câu ${q.number}',
                      style: TextStyle(fontSize: 11, fontWeight: FontWeight.w700, color: color)),
                ),
                const SizedBox(width: 8),
                Text(skipped ? 'Bỏ qua' : (good ? 'Đúng' : 'Sai'),
                    style: TextStyle(fontSize: 11.5, fontWeight: FontWeight.w700, color: color)),
                const Spacer(),
                Text('Đáp án ${q.answerKey ?? '-'}',
                    style: const TextStyle(fontSize: 11, color: AppColors.muted, fontWeight: FontWeight.w600)),
              ]),
              const SizedBox(height: 10),
              Text(q.stem, style: const TextStyle(fontSize: 12.8, height: 1.5, color: AppColors.ink2)),
              const SizedBox(height: 10),
              ...q.options.map((o) {
                final isAnswer = o.key == q.answerKey;
                final isChosen = o.key == q.selectedKey;
                final tileColor = isAnswer
                    ? AppColors.greenBg
                    : isChosen
                        ? AppColors.redBg
                        : Colors.white;
                final borderColor = isAnswer
                    ? const Color(0xFFBFE6CF)
                    : isChosen
                        ? const Color(0xFFF6C9C5)
                        : AppColors.line;
                return Container(
                  margin: const EdgeInsets.only(bottom: 6),
                  padding: const EdgeInsets.symmetric(horizontal: 11, vertical: 9),
                  decoration: BoxDecoration(
                    color: tileColor,
                    borderRadius: BorderRadius.circular(9),
                    border: Border.all(color: borderColor),
                  ),
                  child: Row(children: [
                    Text('${o.key}.',
                        style: TextStyle(
                            fontSize: 12.5,
                            fontWeight: FontWeight.w800,
                            color: isAnswer ? AppColors.green : (isChosen ? AppColors.red : AppColors.muted))),
                    const SizedBox(width: 8),
                    Expanded(
                      child: Text(o.text, style: const TextStyle(fontSize: 12.5, color: AppColors.ink2)),
                    ),
                    if (isChosen && !isAnswer)
                      const Text('bạn chọn',
                          style: TextStyle(fontSize: 10, color: AppColors.red, fontWeight: FontWeight.w700)),
                  ]),
                );
              }),
              if ((q.hint ?? '').isNotEmpty) ...[
                const SizedBox(height: 4),
                Container(
                  padding: const EdgeInsets.all(11),
                  decoration: BoxDecoration(
                    color: AppColors.blue50,
                    borderRadius: BorderRadius.circular(9),
                    border: const Border(left: BorderSide(color: AppColors.blue500, width: 3)),
                  ),
                  child: Text(q.hint!, style: const TextStyle(fontSize: 12, height: 1.55, color: AppColors.blue700)),
                ),
              ],
            ],
          ),
        ),
      ),
    );
  }
}
