import 'dart:async';

import 'package:flutter/material.dart';

import '../api_client.dart';
import '../models.dart';
import '../theme.dart';
import 'result_screen.dart';

class TakingScreen extends StatefulWidget {
  const TakingScreen({super.key, required this.session, required this.studentCode});

  final AttemptSession session;
  final String studentCode;

  @override
  State<TakingScreen> createState() => _TakingScreenState();
}

class _TakingScreenState extends State<TakingScreen> {
  late final List<QuestionItem> _questions = widget.session.questions;
  final Map<int, String> _answers = {};
  final Set<int> _flagged = {};
  int _index = 0;
  late int _remaining = widget.session.remainingSeconds;
  Timer? _timer;
  bool _submitting = false;

  @override
  void initState() {
    super.initState();
    for (final q in _questions) {
      if (q.selectedKey != null && q.selectedKey!.isNotEmpty) _answers[q.id] = q.selectedKey!;
    }
    _timer = Timer.periodic(const Duration(seconds: 1), (_) {
      if (!mounted) return;
      setState(() => _remaining = (_remaining - 1).clamp(0, 1 << 31));
      if (_remaining == 300) _warnFiveMinutes();
      if (_remaining == 0) _submit(auto: true);
    });
  }

  @override
  void dispose() {
    _timer?.cancel();
    super.dispose();
  }

  QuestionItem get _current => _questions[_index];

  void _warnFiveMinutes() {
    ScaffoldMessenger.of(context).showSnackBar(const SnackBar(
      backgroundColor: AppColors.amber,
      content: Text('Còn 5 phút — hệ thống sẽ tự thu bài khi hết giờ.'),
    ));
  }

  void _choose(String key) {
    if (_remaining == 0 || _submitting) return;
    setState(() {
      if (_answers[_current.id] == key) {
        _answers.remove(_current.id); // cho phép hủy lựa chọn
      } else {
        _answers[_current.id] = key;
      }
    });
    ApiClient.instance.saveAnswers(widget.session.id, {_current.id: _answers[_current.id] ?? ''}).catchError((_) {});
  }

  Future<void> _submit({bool auto = false}) async {
    if (_submitting) return;
    setState(() => _submitting = true);
    _timer?.cancel();
    try {
      await ApiClient.instance.saveAnswers(widget.session.id, _answers);
      final res = await ApiClient.instance.submit(widget.session.id, auto: auto);
      if (!mounted) return;
      final keep = await Navigator.of(context).pushReplacement(MaterialPageRoute(
        builder: (_) => ResultScreen(attemptId: res.attemptId, examTitle: res.examTitle, initial: res),
      ));
      if (keep == null && mounted) {
        // ResultScreen already replaced this route.
      }
    } catch (e) {
      if (!mounted) return;
      setState(() => _submitting = false);
      ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('Nộp bài lỗi: $e')));
    }
  }

  Future<void> _confirmSubmit() async {
    final unanswered = _questions.where((q) => !_answers.containsKey(q.id)).length;
    final ok = await showModalBottomSheet<bool>(
      context: context,
      backgroundColor: Colors.white,
      shape: const RoundedRectangleBorder(borderRadius: BorderRadius.vertical(top: Radius.circular(20))),
      builder: (ctx) => Padding(
        padding: const EdgeInsets.fromLTRB(20, 12, 20, 24),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Center(
              child: Container(
                width: 44,
                height: 4,
                decoration: BoxDecoration(color: AppColors.line, borderRadius: BorderRadius.circular(99)),
              ),
            ),
            const SizedBox(height: 16),
            const Text('Xác nhận nộp bài?',
                style: TextStyle(fontSize: 17, fontWeight: FontWeight.w700, color: AppColors.ink)),
            const SizedBox(height: 8),
            Text(
              unanswered > 0
                  ? 'Bạn còn $unanswered câu chưa trả lời. Sau khi nộp, bạn không thể sửa đáp án.'
                  : 'Bạn đã trả lời tất cả ${_questions.length} câu. Sau khi nộp, bạn không thể sửa đáp án.',
              style: const TextStyle(fontSize: 12.5, color: AppColors.muted, height: 1.55),
            ),
            const SizedBox(height: 16),
            Row(
              children: [
                Expanded(
                  child: OutlinedButton(
                    onPressed: () => Navigator.pop(ctx, false),
                    child: const Text('Làm tiếp'),
                  ),
                ),
                const SizedBox(width: 10),
                Expanded(
                  flex: 2,
                  child: FilledButton(onPressed: () => Navigator.pop(ctx, true), child: const Text('Nộp bài ngay')),
                ),
              ],
            ),
          ],
        ),
      ),
    );
    if (ok == true) await _submit();
  }

  String get _clock {
    final m = (_remaining ~/ 60).toString().padLeft(2, '0');
    final s = (_remaining % 60).toString().padLeft(2, '0');
    return '$m:$s';
  }

  Passage? get _passage {
    final pid = _current.passageId;
    if (pid == null) return null;
    for (final p in widget.session.passages) {
      if (p.id == pid) return p;
    }
    return null;
  }

  @override
  Widget build(BuildContext context) {
    final p = _passage;
    final answered = _answers.length;

    return PopScope(
      canPop: false,
      onPopInvokedWithResult: (didPop, _) {
        if (!didPop) _confirmSubmit();
      },
      child: Scaffold(
        backgroundColor: AppColors.bg,
        appBar: AppBar(
          automaticallyImplyLeading: false,
          title: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text('Câu ${_current.number} · ${answered}/${_questions.length} đã trả lời',
                  style: const TextStyle(fontSize: 14, fontWeight: FontWeight.w700)),
              const SizedBox(height: 6),
              ClipRRect(
                borderRadius: BorderRadius.circular(99),
                child: LinearProgressIndicator(
                  value: _questions.isEmpty ? 0 : answered / _questions.length,
                  minHeight: 5,
                  backgroundColor: AppColors.line,
                  valueColor: const AlwaysStoppedAnimation(AppColors.blue600),
                ),
              ),
            ],
          ),
          actions: [
            Container(
              margin: const EdgeInsets.only(right: 12),
              padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 7),
              decoration: BoxDecoration(
                color: _remaining <= 300 ? AppColors.redBg : Colors.white,
                borderRadius: BorderRadius.circular(999),
                border: Border.all(color: _remaining <= 300 ? const Color(0xFFF6C9C5) : AppColors.line),
              ),
              child: Row(children: [
                Icon(Icons.timer_outlined,
                    size: 15, color: _remaining <= 300 ? AppColors.red : AppColors.ink2),
                const SizedBox(width: 6),
                Text(_clock,
                    style: TextStyle(
                      fontSize: 13,
                      fontWeight: FontWeight.w800,
                      color: _remaining <= 300 ? AppColors.red : AppColors.ink,
                      fontFeatures: const [FontFeature.tabularFigures()],
                    )),
              ]),
            ),
          ],
        ),
        body: ListView(
          padding: const EdgeInsets.fromLTRB(16, 14, 16, 24),
          children: [
            Row(
              children: [
                Container(
                  padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                  decoration: BoxDecoration(color: AppColors.blue50, borderRadius: BorderRadius.circular(999)),
                  child: Text('Part ${_current.part}',
                      style: const TextStyle(
                          fontSize: 11.5, fontWeight: FontWeight.w700, color: AppColors.blue700)),
                ),
                const Spacer(),
                TextButton.icon(
                  onPressed: () => setState(() {
                    if (!_flagged.remove(_current.id)) _flagged.add(_current.id);
                  }),
                  icon: Icon(_flagged.contains(_current.id) ? Icons.flag : Icons.flag_outlined,
                      size: 17, color: AppColors.amber),
                  label: Text(_flagged.contains(_current.id) ? 'Đã đánh dấu' : 'Đánh dấu',
                      style: const TextStyle(fontSize: 12, color: AppColors.amber)),
                ),
              ],
            ),
            if (p != null) _passageCard(p),
            const SizedBox(height: 8),
            Text(_current.stem,
                style: const TextStyle(fontSize: 14.5, fontWeight: FontWeight.w600, height: 1.55, color: AppColors.ink)),
            const SizedBox(height: 14),
            ..._current.options.map(_optionTile),
            const SizedBox(height: 16),
            Row(children: [
              Expanded(
                child: OutlinedButton(
                  onPressed: _index == 0 ? null : () => setState(() => _index--),
                  child: const Text('Câu trước'),
                ),
              ),
              const SizedBox(width: 10),
              Expanded(
                flex: 2,
                child: FilledButton(
                  onPressed: _index == _questions.length - 1 ? _confirmSubmit : () => setState(() => _index++),
                  child: Text(_index == _questions.length - 1 ? 'Nộp bài' : 'Câu tiếp'),
                ),
              ),
            ]),
            const SizedBox(height: 10),
            OutlinedButton.icon(
              onPressed: _openPalette,
              icon: const Icon(Icons.grid_view_rounded, size: 18),
              label: Text('Bảng câu hỏi · ${_flagged.length} đánh dấu'),
            ),
          ],
        ),
      ),
    );
  }

  Widget _optionTile(OptionItem o) {
    final selected = _answers[_current.id] == o.key;
    return Padding(
      padding: const EdgeInsets.only(bottom: 10),
      child: InkWell(
        borderRadius: BorderRadius.circular(12),
        onTap: () => _choose(o.key),
        child: Container(
          padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 13),
          decoration: BoxDecoration(
            color: selected ? AppColors.blue50 : Colors.white,
            borderRadius: BorderRadius.circular(12),
            border: Border.all(color: selected ? AppColors.blue600 : AppColors.line, width: selected ? 1.5 : 1),
          ),
          child: Row(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Container(
                width: 24,
                height: 24,
                decoration: BoxDecoration(
                  color: selected ? AppColors.blue600 : Colors.white,
                  shape: BoxShape.circle,
                  border: Border.all(color: selected ? AppColors.blue600 : AppColors.line, width: 1.5),
                ),
                alignment: Alignment.center,
                child: Text(o.key,
                    style: TextStyle(
                        fontSize: 11.5,
                        fontWeight: FontWeight.w800,
                        color: selected ? Colors.white : AppColors.muted)),
              ),
              const SizedBox(width: 12),
              Expanded(
                child: Text(o.text,
                    style: const TextStyle(fontSize: 13.5, height: 1.45, color: AppColors.ink2)),
              ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _passageCard(Passage p) {
    return Card(
      clipBehavior: Clip.antiAlias,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Container(
            width: double.infinity,
            padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 10),
            decoration: const BoxDecoration(
              color: Color(0xFFF8FAFE),
              border: Border(bottom: BorderSide(color: AppColors.line)),
            ),
            child: Text((p.passageType ?? 'passage').toUpperCase(),
                style: const TextStyle(
                    fontSize: 10.5, fontWeight: FontWeight.w700, letterSpacing: 0.6, color: AppColors.muted)),
          ),
          Padding(
            padding: const EdgeInsets.all(14),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: p.documents.map((d) {
                if (d.documentType == 'image' && (d.imagePath ?? '').isNotEmpty) {
                  final url = '${ApiClient.baseUrl}/${d.imagePath}';
                  return Padding(
                    padding: const EdgeInsets.only(top: 8),
                    child: ClipRRect(
                      borderRadius: BorderRadius.circular(8),
                      child: Image.network(url,
                          errorBuilder: (_, __, ___) => Container(
                                padding: const EdgeInsets.all(10),
                                color: AppColors.blue50,
                                child: Text('Hình ảnh: ${d.label ?? d.imagePath}',
                                    style: const TextStyle(fontSize: 11, color: AppColors.blue700)),
                              )),
                    ),
                  );
                }
                return Padding(
                  padding: const EdgeInsets.only(bottom: 8),
                  child: Text(d.text ?? '',
                      style: const TextStyle(fontSize: 12.5, height: 1.65, color: AppColors.ink2)),
                );
              }).toList(),
            ),
          ),
        ],
      ),
    );
  }

  void _openPalette() {
    showModalBottomSheet(
      context: context,
      backgroundColor: Colors.white,
      shape: const RoundedRectangleBorder(borderRadius: BorderRadius.vertical(top: Radius.circular(20))),
      builder: (ctx) => StatefulBuilder(
        builder: (ctx, setSheet) => Padding(
          padding: const EdgeInsets.fromLTRB(18, 14, 18, 20),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              const Text('Bảng câu hỏi', style: TextStyle(fontSize: 15, fontWeight: FontWeight.w700)),
              const SizedBox(height: 12),
              Wrap(
                spacing: 8,
                runSpacing: 8,
                children: List.generate(_questions.length, (i) {
                  final q = _questions[i];
                  final done = _answers.containsKey(q.id);
                  final flag = _flagged.contains(q.id);
                  final cur = i == _index;
                  return InkWell(
                    onTap: () {
                      setState(() => _index = i);
                      Navigator.pop(ctx);
                    },
                    borderRadius: BorderRadius.circular(9),
                    child: Container(
                      width: 46,
                      height: 38,
                      alignment: Alignment.center,
                      decoration: BoxDecoration(
                        color: cur
                            ? AppColors.blue600
                            : flag
                                ? AppColors.amberBg
                                : done
                                    ? AppColors.blue50
                                    : Colors.white,
                        borderRadius: BorderRadius.circular(9),
                        border: Border.all(
                          color: cur
                              ? AppColors.blue600
                              : flag
                                  ? const Color(0xFFF3DDB8)
                                  : done
                                      ? AppColors.blue100
                                      : AppColors.line,
                        ),
                      ),
                      child: Text('${q.number}',
                          style: TextStyle(
                            fontSize: 12,
                            fontWeight: FontWeight.w600,
                            color: cur
                                ? Colors.white
                                : flag
                                    ? AppColors.amber
                                    : done
                                        ? AppColors.blue700
                                        : AppColors.ink2,
                          )),
                    ),
                  );
                }),
              ),
              const SizedBox(height: 14),
              FilledButton(onPressed: () { Navigator.pop(ctx); _confirmSubmit(); }, child: const Text('Nộp bài')),
            ],
          ),
        ),
      ),
    );
  }
}
