using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Toeic.Migrations
{
    /// <inheritdoc />
    public partial class ReadingTest_Tables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RtExams",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    DurationMinutes = table.Column<int>(type: "int", nullable: false),
                    TotalQuestions = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SourceFile = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OpenAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CloseAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ShuffleQuestions = table.Column<bool>(type: "bit", nullable: false),
                    ShuffleOptions = table.Column<bool>(type: "bit", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RtExams", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RtAttempts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExamId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    StudentCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    StudentName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    CorrectCount = table.Column<int>(type: "int", nullable: false),
                    WrongCount = table.Column<int>(type: "int", nullable: false),
                    SkippedCount = table.Column<int>(type: "int", nullable: false),
                    DurationSeconds = table.Column<int>(type: "int", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RtAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RtAttempts_RtExams_ExamId",
                        column: x => x.ExamId,
                        principalTable: "RtExams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RtPassages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExamId = table.Column<long>(type: "bigint", nullable: false),
                    GroupCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Part = table.Column<int>(type: "int", nullable: false),
                    PassageType = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Header = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    OrderIndex = table.Column<int>(type: "int", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RtPassages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RtPassages_RtExams_ExamId",
                        column: x => x.ExamId,
                        principalTable: "RtExams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RtPassageDocuments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PassageId = table.Column<long>(type: "bigint", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    DocumentType = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImagePath = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    OrderIndex = table.Column<int>(type: "int", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RtPassageDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RtPassageDocuments_RtPassages_PassageId",
                        column: x => x.PassageId,
                        principalTable: "RtPassages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RtQuestions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExamId = table.Column<long>(type: "bigint", nullable: false),
                    PassageId = table.Column<long>(type: "bigint", nullable: true),
                    Number = table.Column<int>(type: "int", nullable: false),
                    Part = table.Column<int>(type: "int", nullable: false),
                    Stem = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AnswerKey = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: true),
                    Hint = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrderIndex = table.Column<int>(type: "int", nullable: false),
                    ShuffleQuestions = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RtQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RtQuestions_RtExams_ExamId",
                        column: x => x.ExamId,
                        principalTable: "RtExams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RtQuestions_RtPassages_PassageId",
                        column: x => x.PassageId,
                        principalTable: "RtPassages",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RtAttemptAnswers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AttemptId = table.Column<long>(type: "bigint", nullable: false),
                    QuestionId = table.Column<long>(type: "bigint", nullable: false),
                    SelectedKey = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: true),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false),
                    AnsweredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RtAttemptAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RtAttemptAnswers_RtAttempts_AttemptId",
                        column: x => x.AttemptId,
                        principalTable: "RtAttempts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RtAttemptAnswers_RtQuestions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "RtQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RtQuestionOptions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuestionId = table.Column<long>(type: "bigint", nullable: false),
                    OptionKey = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrderIndex = table.Column<int>(type: "int", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RtQuestionOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RtQuestionOptions_RtQuestions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "RtQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RtAttemptAnswers_AttemptId_QuestionId",
                table: "RtAttemptAnswers",
                columns: new[] { "AttemptId", "QuestionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RtAttemptAnswers_QuestionId",
                table: "RtAttemptAnswers",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_RtAttempts_ExamId_UserId",
                table: "RtAttempts",
                columns: new[] { "ExamId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_RtExams_Status",
                table: "RtExams",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_RtPassageDocuments_PassageId",
                table: "RtPassageDocuments",
                column: "PassageId");

            migrationBuilder.CreateIndex(
                name: "IX_RtPassages_ExamId_OrderIndex",
                table: "RtPassages",
                columns: new[] { "ExamId", "OrderIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_RtQuestionOptions_QuestionId_OptionKey",
                table: "RtQuestionOptions",
                columns: new[] { "QuestionId", "OptionKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RtQuestions_ExamId_Number",
                table: "RtQuestions",
                columns: new[] { "ExamId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RtQuestions_PassageId",
                table: "RtQuestions",
                column: "PassageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RtAttemptAnswers");

            migrationBuilder.DropTable(
                name: "RtPassageDocuments");

            migrationBuilder.DropTable(
                name: "RtQuestionOptions");

            migrationBuilder.DropTable(
                name: "RtAttempts");

            migrationBuilder.DropTable(
                name: "RtQuestions");

            migrationBuilder.DropTable(
                name: "RtPassages");

            migrationBuilder.DropTable(
                name: "RtExams");
        }
    }
}
