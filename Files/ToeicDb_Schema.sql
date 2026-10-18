/* ToeicDb - schema export tu SQL Server (sys catalog), sinh ngay 18/10/2026 */
USE [ToeicDb];
GO

CREATE TABLE [dbo].[RtAttemptAnswers] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [AttemptId] bigint NOT NULL,
    [QuestionId] bigint NOT NULL,
    [SelectedKey] nvarchar(4) NULL,
    [IsCorrect] bit NOT NULL,
    [AnsweredAt] datetime2(7) NOT NULL,
    [CreationTime] datetime2(7) NOT NULL,
    [CreatorUserId] bigint NULL,
    [LastModificationTime] datetime2(7) NULL,
    [LastModifierUserId] bigint NULL,
    [IsDeleted] bit NOT NULL,
    [DeleterUserId] bigint NULL,
    [DeletionTime] datetime2(7) NULL
);
GO

CREATE TABLE [dbo].[RtAttempts] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [ExamId] bigint NOT NULL,
    [UserId] bigint NOT NULL,
    [StudentCode] nvarchar(64) NULL,
    [StudentName] nvarchar(128) NULL,
    [StartedAt] datetime2(7) NOT NULL,
    [SubmittedAt] datetime2(7) NULL,
    [Status] int NOT NULL,
    [Score] int NOT NULL,
    [CorrectCount] int NOT NULL,
    [WrongCount] int NOT NULL,
    [SkippedCount] int NOT NULL,
    [DurationSeconds] int NOT NULL,
    [CreationTime] datetime2(7) NOT NULL,
    [CreatorUserId] bigint NULL,
    [LastModificationTime] datetime2(7) NULL,
    [LastModifierUserId] bigint NULL,
    [IsDeleted] bit NOT NULL,
    [DeleterUserId] bigint NULL,
    [DeletionTime] datetime2(7) NULL
);
GO

CREATE TABLE [dbo].[RtExams] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [Title] nvarchar(256) NOT NULL,
    [Description] nvarchar(512) NULL,
    [DurationMinutes] int NOT NULL,
    [TotalQuestions] int NOT NULL,
    [Status] int NOT NULL,
    [SourceFile] nvarchar(256) NULL,
    [PublishedAt] datetime2(7) NULL,
    [OpenAt] datetime2(7) NULL,
    [CloseAt] datetime2(7) NULL,
    [ShuffleQuestions] bit NOT NULL,
    [ShuffleOptions] bit NOT NULL,
    [CreationTime] datetime2(7) NOT NULL,
    [CreatorUserId] bigint NULL,
    [LastModificationTime] datetime2(7) NULL,
    [LastModifierUserId] bigint NULL,
    [IsDeleted] bit NOT NULL,
    [DeleterUserId] bigint NULL,
    [DeletionTime] datetime2(7) NULL
);
GO

CREATE TABLE [dbo].[RtPassageDocuments] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [PassageId] bigint NOT NULL,
    [Label] nvarchar(256) NULL,
    [DocumentType] nvarchar(64) NULL,
    [Text] nvarchar(MAX) NULL,
    [ImagePath] nvarchar(512) NULL,
    [OrderIndex] int NOT NULL,
    [CreationTime] datetime2(7) NOT NULL,
    [CreatorUserId] bigint NULL,
    [LastModificationTime] datetime2(7) NULL,
    [LastModifierUserId] bigint NULL,
    [IsDeleted] bit NOT NULL,
    [DeleterUserId] bigint NULL,
    [DeletionTime] datetime2(7) NULL
);
GO

CREATE TABLE [dbo].[RtPassages] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [ExamId] bigint NOT NULL,
    [GroupCode] nvarchar(64) NULL,
    [Part] int NOT NULL,
    [PassageType] nvarchar(64) NULL,
    [Header] nvarchar(512) NULL,
    [OrderIndex] int NOT NULL,
    [CreationTime] datetime2(7) NOT NULL,
    [CreatorUserId] bigint NULL,
    [LastModificationTime] datetime2(7) NULL,
    [LastModifierUserId] bigint NULL,
    [IsDeleted] bit NOT NULL,
    [DeleterUserId] bigint NULL,
    [DeletionTime] datetime2(7) NULL
);
GO

CREATE TABLE [dbo].[RtQuestionOptions] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [QuestionId] bigint NOT NULL,
    [OptionKey] nvarchar(4) NOT NULL,
    [Text] nvarchar(MAX) NULL,
    [OrderIndex] int NOT NULL,
    [CreationTime] datetime2(7) NOT NULL,
    [CreatorUserId] bigint NULL,
    [LastModificationTime] datetime2(7) NULL,
    [LastModifierUserId] bigint NULL,
    [IsDeleted] bit NOT NULL,
    [DeleterUserId] bigint NULL,
    [DeletionTime] datetime2(7) NULL
);
GO

CREATE TABLE [dbo].[RtQuestions] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [ExamId] bigint NOT NULL,
    [PassageId] bigint NULL,
    [Number] int NOT NULL,
    [Part] int NOT NULL,
    [Stem] nvarchar(MAX) NULL,
    [AnswerKey] nvarchar(4) NULL,
    [Hint] nvarchar(MAX) NULL,
    [OrderIndex] int NOT NULL,
    [ShuffleQuestions] nvarchar(16) NULL,
    [CreationTime] datetime2(7) NOT NULL,
    [CreatorUserId] bigint NULL,
    [LastModificationTime] datetime2(7) NULL,
    [LastModifierUserId] bigint NULL,
    [IsDeleted] bit NOT NULL,
    [DeleterUserId] bigint NULL,
    [DeletionTime] datetime2(7) NULL
);
GO

ALTER TABLE [dbo].[RtAttemptAnswers] ADD CONSTRAINT [PK_RtAttemptAnswers] PRIMARY KEY ([Id]);
GO
ALTER TABLE [dbo].[RtAttempts] ADD CONSTRAINT [PK_RtAttempts] PRIMARY KEY ([Id]);
GO
ALTER TABLE [dbo].[RtExams] ADD CONSTRAINT [PK_RtExams] PRIMARY KEY ([Id]);
GO
ALTER TABLE [dbo].[RtPassageDocuments] ADD CONSTRAINT [PK_RtPassageDocuments] PRIMARY KEY ([Id]);
GO
ALTER TABLE [dbo].[RtPassages] ADD CONSTRAINT [PK_RtPassages] PRIMARY KEY ([Id]);
GO
ALTER TABLE [dbo].[RtQuestionOptions] ADD CONSTRAINT [PK_RtQuestionOptions] PRIMARY KEY ([Id]);
GO
ALTER TABLE [dbo].[RtQuestions] ADD CONSTRAINT [PK_RtQuestions] PRIMARY KEY ([Id]);
GO

ALTER TABLE [dbo].[RtAttemptAnswers] ADD CONSTRAINT [FK_RtAttemptAnswers_RtAttempts_AttemptId] FOREIGN KEY ([AttemptId]) REFERENCES [dbo].[RtAttempts] ([Id]) ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[RtAttemptAnswers] ADD CONSTRAINT [FK_RtAttemptAnswers_RtQuestions_QuestionId] FOREIGN KEY ([QuestionId]) REFERENCES [dbo].[RtQuestions] ([Id]) ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[RtAttempts] ADD CONSTRAINT [FK_RtAttempts_RtExams_ExamId] FOREIGN KEY ([ExamId]) REFERENCES [dbo].[RtExams] ([Id]);
GO
ALTER TABLE [dbo].[RtPassageDocuments] ADD CONSTRAINT [FK_RtPassageDocuments_RtPassages_PassageId] FOREIGN KEY ([PassageId]) REFERENCES [dbo].[RtPassages] ([Id]) ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[RtPassages] ADD CONSTRAINT [FK_RtPassages_RtExams_ExamId] FOREIGN KEY ([ExamId]) REFERENCES [dbo].[RtExams] ([Id]);
GO
ALTER TABLE [dbo].[RtQuestionOptions] ADD CONSTRAINT [FK_RtQuestionOptions_RtQuestions_QuestionId] FOREIGN KEY ([QuestionId]) REFERENCES [dbo].[RtQuestions] ([Id]) ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[RtQuestions] ADD CONSTRAINT [FK_RtQuestions_RtExams_ExamId] FOREIGN KEY ([ExamId]) REFERENCES [dbo].[RtExams] ([Id]);
GO
ALTER TABLE [dbo].[RtQuestions] ADD CONSTRAINT [FK_RtQuestions_RtPassages_PassageId] FOREIGN KEY ([PassageId]) REFERENCES [dbo].[RtPassages] ([Id]);
GO

CREATE UNIQUE INDEX [IX_RtAttemptAnswers_AttemptId_QuestionId] ON [dbo].[RtAttemptAnswers] ([AttemptId], [QuestionId]);
GO
CREATE INDEX [IX_RtAttemptAnswers_QuestionId] ON [dbo].[RtAttemptAnswers] ([QuestionId]);
GO
CREATE INDEX [IX_RtAttempts_ExamId_UserId] ON [dbo].[RtAttempts] ([ExamId], [UserId]);
GO
CREATE INDEX [IX_RtExams_Status] ON [dbo].[RtExams] ([Status]);
GO
CREATE INDEX [IX_RtPassageDocuments_PassageId] ON [dbo].[RtPassageDocuments] ([PassageId]);
GO
CREATE INDEX [IX_RtPassages_ExamId_OrderIndex] ON [dbo].[RtPassages] ([ExamId], [OrderIndex]);
GO
CREATE UNIQUE INDEX [IX_RtQuestionOptions_QuestionId_OptionKey] ON [dbo].[RtQuestionOptions] ([QuestionId], [OptionKey]);
GO
CREATE UNIQUE INDEX [IX_RtQuestions_ExamId_Number] ON [dbo].[RtQuestions] ([ExamId], [Number]);
GO
CREATE INDEX [IX_RtQuestions_PassageId] ON [dbo].[RtQuestions] ([PassageId]);
GO
