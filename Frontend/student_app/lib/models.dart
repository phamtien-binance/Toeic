// Data models mirroring the ABP application service DTOs.
class OptionItem {
  final String key;
  final String text;
  OptionItem({required this.key, required this.text});

  factory OptionItem.fromJson(Map<String, dynamic> j) =>
      OptionItem(key: j['key'] as String? ?? '', text: j['text'] as String? ?? '');
}

class QuestionItem {
  final int id;
  final int number;
  final int part;
  final int? passageId;
  final String stem;
  final List<OptionItem> options;
  final String? selectedKey;
  final String? answerKey;
  final String? hint;
  final bool? isCorrect;

  QuestionItem({
    required this.id,
    required this.number,
    required this.part,
    required this.passageId,
    required this.stem,
    required this.options,
    this.selectedKey,
    this.answerKey,
    this.hint,
    this.isCorrect,
  });

  factory QuestionItem.fromJson(Map<String, dynamic> j) => QuestionItem(
        id: j['id'] as int,
        number: j['number'] as int,
        part: j['part'] as int,
        passageId: j['passageId'] as int?,
        stem: j['stem'] as String? ?? '',
        options: ((j['options'] as List?) ?? [])
            .map((e) => OptionItem.fromJson(e as Map<String, dynamic>))
            .toList(),
        selectedKey: j['selectedKey'] as String?,
        answerKey: j['answerKey'] as String?,
        hint: j['hint'] as String?,
        isCorrect: j['isCorrect'] as bool?,
      );
}

class PassageDocument {
  final String? label;
  final String? documentType;
  final String? text;
  final String? imagePath;

  PassageDocument({this.label, this.documentType, this.text, this.imagePath});

  factory PassageDocument.fromJson(Map<String, dynamic> j) => PassageDocument(
        label: j['label'] as String?,
        documentType: j['documentType'] as String?,
        text: j['text'] as String?,
        imagePath: j['imagePath'] as String?,
      );
}

class Passage {
  final int id;
  final String? groupCode;
  final int part;
  final String? passageType;
  final String? header;
  final List<PassageDocument> documents;

  Passage({
    required this.id,
    this.groupCode,
    required this.part,
    this.passageType,
    this.header,
    required this.documents,
  });

  factory Passage.fromJson(Map<String, dynamic> j) => Passage(
        id: j['id'] as int,
        groupCode: j['groupCode'] as String?,
        part: j['part'] as int,
        passageType: j['passageType'] as String?,
        header: j['header'] as String?,
        documents: ((j['documents'] as List?) ?? [])
            .map((e) => PassageDocument.fromJson(e as Map<String, dynamic>))
            .toList(),
      );
}

class StudentExam {
  final int id;
  final String title;
  final int durationMinutes;
  final int totalQuestions;
  final String? closeAt;
  final bool alreadyAttempted;
  final int? lastAttemptId;
  final int? lastScore;

  StudentExam({
    required this.id,
    required this.title,
    required this.durationMinutes,
    required this.totalQuestions,
    this.closeAt,
    required this.alreadyAttempted,
    this.lastAttemptId,
    this.lastScore,
  });

  factory StudentExam.fromJson(Map<String, dynamic> j) => StudentExam(
        id: j['id'] as int,
        title: j['title'] as String? ?? '',
        durationMinutes: j['durationMinutes'] as int? ?? 75,
        totalQuestions: j['totalQuestions'] as int? ?? 0,
        closeAt: j['closeAt'] as String?,
        alreadyAttempted: j['alreadyAttempted'] as bool? ?? false,
        lastAttemptId: j['lastAttemptId'] as int?,
        lastScore: j['lastScore'] as int?,
      );
}

class AttemptSession {
  final int id;
  final int examId;
  final String examTitle;
  final int durationMinutes;
  final int remainingSeconds;
  final List<Passage> passages;
  final List<QuestionItem> questions;

  AttemptSession({
    required this.id,
    required this.examId,
    required this.examTitle,
    required this.durationMinutes,
    required this.remainingSeconds,
    required this.passages,
    required this.questions,
  });

  factory AttemptSession.fromJson(Map<String, dynamic> j) => AttemptSession(
        id: j['id'] as int,
        examId: j['examId'] as int,
        examTitle: j['examTitle'] as String? ?? '',
        durationMinutes: j['durationMinutes'] as int? ?? 75,
        remainingSeconds: j['remainingSeconds'] as int? ?? 4500,
        passages: ((j['passages'] as List?) ?? [])
            .map((e) => Passage.fromJson(e as Map<String, dynamic>))
            .toList(),
        questions: ((j['questions'] as List?) ?? [])
            .map((e) => QuestionItem.fromJson(e as Map<String, dynamic>))
            .toList(),
      );
}

class PartScore {
  final int part;
  final int total;
  final int correct;
  PartScore({required this.part, required this.total, required this.correct});
  factory PartScore.fromJson(Map<String, dynamic> j) => PartScore(
      part: j['part'] as int, total: j['total'] as int, correct: j['correct'] as int);
}

class AttemptResult {
  final int attemptId;
  final String examTitle;
  final int score;
  final int totalQuestions;
  final int correctCount;
  final int wrongCount;
  final int skippedCount;
  final int durationSeconds;
  final List<PartScore> parts;
  final List<Passage> passages;
  final List<QuestionItem> questions;

  AttemptResult({
    required this.attemptId,
    required this.examTitle,
    required this.score,
    required this.totalQuestions,
    required this.correctCount,
    required this.wrongCount,
    required this.skippedCount,
    required this.durationSeconds,
    required this.parts,
    required this.passages,
    required this.questions,
  });

  factory AttemptResult.fromJson(Map<String, dynamic> j) => AttemptResult(
        attemptId: j['attemptId'] as int,
        examTitle: j['examTitle'] as String? ?? '',
        score: j['score'] as int? ?? 0,
        totalQuestions: j['totalQuestions'] as int? ?? 0,
        correctCount: j['correctCount'] as int? ?? 0,
        wrongCount: j['wrongCount'] as int? ?? 0,
        skippedCount: j['skippedCount'] as int? ?? 0,
        durationSeconds: j['durationSeconds'] as int? ?? 0,
        parts: ((j['parts'] as List?) ?? [])
            .map((e) => PartScore.fromJson(e as Map<String, dynamic>))
            .toList(),
        passages: ((j['passages'] as List?) ?? [])
            .map((e) => Passage.fromJson(e as Map<String, dynamic>))
            .toList(),
        questions: ((j['questions'] as List?) ?? [])
            .map((e) => QuestionItem.fromJson(e as Map<String, dynamic>))
            .toList(),
      );
}
