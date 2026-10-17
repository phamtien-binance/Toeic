/* =====================================================================
   XEM CẤU TRÚC CSDL ToeicDb BẰNG SQL (không cần Database Diagram)
   Cách dùng: SSMS → File → Open → File… → chọn tệp này → bấm F5 (Execute)
   Hoặc đổi "Results to Text" (Ctrl+T) để thấy sơ đồ dạng cây rõ hơn.
   ===================================================================== */

USE ToeicDb;
GO
SET NOCOUNT ON;
GO

/* ---------------------------------------------------------------------
   1. DANH SÁCH BẢNG NGHIỆP VỤ + SỐ DÒNG THẬT
   --------------------------------------------------------------------- */
PRINT N'===== 1. BẢNG VÀ SỐ DÒNG =====';
SELECT t.name                              AS Bang,
       SUM(p.rows)                         AS SoDong,
       (SELECT COUNT(*) FROM sys.columns c
         WHERE c.object_id = t.object_id)  AS SoCot
FROM sys.tables t
JOIN sys.partitions p ON p.object_id = t.object_id AND p.index_id IN (0,1)
WHERE t.is_ms_shipped = 0 AND t.name LIKE 'Rt%'
GROUP BY t.name, t.object_id
ORDER BY t.name;
GO

/* ---------------------------------------------------------------------
   2. CHI TIẾT CỘT: kiểu dữ liệu, NULL, khoá chính, khoá ngoại
   --------------------------------------------------------------------- */
PRINT N'===== 2. CỘT CỦA TỪNG BẢNG =====';
SELECT t.name                                        AS Bang,
       c.column_id                                   AS ThuTu,
       c.name                                        AS Cot,
       ty.name +
         CASE WHEN ty.name IN ('nvarchar','varchar','char','nchar')
              THEN '(' + CASE WHEN c.max_length = -1 THEN 'max'
                              ELSE CAST(c.max_length / CASE WHEN ty.name LIKE 'n%' THEN 2 ELSE 1 END AS varchar(10)) END + ')'
              WHEN ty.name IN ('decimal','numeric')
              THEN '(' + CAST(c.precision AS varchar(10)) + ',' + CAST(c.scale AS varchar(10)) + ')'
              ELSE '' END                           AS KieuDuLieu,
       CASE WHEN c.is_nullable = 1 THEN 'NULL' ELSE 'NOT NULL' END AS ChoPhepNull,
       CASE WHEN pk.column_id IS NOT NULL THEN 'PK' ELSE '' END     AS KhoaChinh,
       CASE WHEN fk.column_id IS NOT NULL THEN 'FK' ELSE '' END     AS KhoaNgoai
FROM sys.tables t
JOIN sys.columns c   ON c.object_id = t.object_id
JOIN sys.types  ty   ON ty.user_type_id = c.user_type_id
LEFT JOIN (SELECT ic.object_id, ic.column_id
             FROM sys.indexes i
             JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
            WHERE i.is_primary_key = 1) pk
       ON pk.object_id = t.object_id AND pk.column_id = c.column_id
LEFT JOIN (SELECT DISTINCT fkc.parent_object_id AS object_id, fkc.parent_column_id AS column_id
             FROM sys.foreign_key_columns fkc) fk
       ON fk.object_id = t.object_id AND fk.column_id = c.column_id
WHERE t.is_ms_shipped = 0 AND t.name LIKE 'Rt%'
ORDER BY t.name, c.column_id;
GO

/* ---------------------------------------------------------------------
   3. KHOÁ NGOẠI: bảng con → bảng cha (đây là "các đường nối" của sơ đồ)
   --------------------------------------------------------------------- */
PRINT N'===== 3. QUAN HỆ KHOÁ NGOẠI =====';
SELECT OBJECT_NAME(fk.parent_object_id)                                AS BangCon,
       COL_NAME(fkc.parent_object_id, fkc.parent_column_id)            AS CotFK,
       '->'                                                             AS Chieu,
       OBJECT_NAME(fk.referenced_object_id)                            AS BangCha,
       COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id)    AS CotDuocTro,
       CASE fk.delete_referential_action
            WHEN 0 THEN 'NO ACTION (chặn xoá)'
            WHEN 1 THEN 'CASCADE (xoá theo)'
            WHEN 2 THEN 'SET NULL'
            ELSE 'SET DEFAULT' END                                     AS KhiXoaCha,
       fk.name                                                         AS TenRangBuoc
FROM sys.foreign_keys fk
JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
WHERE OBJECT_NAME(fk.parent_object_id) LIKE 'Rt%'
ORDER BY BangCha, BangCon;
GO

/* ---------------------------------------------------------------------
   4. SƠ ĐỒ DẠNG CÂY (đọc như sơ đồ: bảng cha ⤷ bảng con)
   --------------------------------------------------------------------- */
PRINT N'===== 4. SƠ ĐỒ DẠNG CÂY =====';
WITH fk AS (
    SELECT OBJECT_NAME(fk.referenced_object_id) AS cha,
           OBJECT_NAME(fk.parent_object_id)     AS con,
           COL_NAME(fkc.parent_object_id, fkc.parent_column_id) AS cot,
           CASE fk.delete_referential_action WHEN 1 THEN N'cascade' ELSE N'no action' END AS xoa
    FROM sys.foreign_keys fk
    JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
    WHERE OBJECT_NAME(fk.parent_object_id) LIKE 'Rt%'
),
tree AS (
    SELECT t.name AS node, 0 AS lvl,
           CAST(N'■ ' + t.name AS nvarchar(400)) AS dong,
           CAST(t.name AS nvarchar(400)) AS duong
    FROM sys.tables t
    WHERE t.name LIKE 'Rt%'
      AND t.name NOT IN (SELECT con FROM fk)
    UNION ALL
    SELECT f.con, tr.lvl + 1,
           CAST(REPLICATE(N'     ', tr.lvl + 1) + N'⤷ ' + f.con + N'  (' + f.cot + N', ' + f.xoa + N')' AS nvarchar(400)),
           CAST(tr.duong + N'/' + f.con + N'.' + f.cot AS nvarchar(400))
    FROM tree tr
    JOIN fk f ON f.cha = tr.node
    WHERE tr.lvl < 5
)
SELECT dong AS SoDo FROM tree ORDER BY duong;
GO

/* ---------------------------------------------------------------------
   5. XUẤT SƠ ĐỒ MERMAID (dán vào mermaid.live hoặc README GitHub)
   --------------------------------------------------------------------- */
PRINT N'===== 5. MERMAID erDiagram (copy phần chữ bên dưới) =====';
SELECT 'erDiagram' + CHAR(10) +
       STRING_AGG('  ' + OBJECT_NAME(fk.referenced_object_id) + ' ||--o{ ' +
                  OBJECT_NAME(fk.parent_object_id) + ' : "' +
                  COL_NAME(fkc.parent_object_id, fkc.parent_column_id) + '"', CHAR(10))
       WITHIN GROUP (ORDER BY OBJECT_NAME(fk.referenced_object_id))
FROM sys.foreign_keys fk
JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
WHERE OBJECT_NAME(fk.parent_object_id) LIKE 'Rt%';
GO

/* ---------------------------------------------------------------------
   6. KIỂM TRA DỮ LIỆU ĐANG CÓ (đối chiếu với sơ đồ)
   --------------------------------------------------------------------- */
PRINT N'===== 6. DỮ LIỆU ĐANG CÓ =====';
SELECT N'Đề'                AS Loai, COUNT(*) AS SoLuong FROM RtExams
UNION ALL SELECT N'Bài đọc',          COUNT(*) FROM RtPassages
UNION ALL SELECT N'Thành phần bài đọc', COUNT(*) FROM RtPassageDocuments
UNION ALL SELECT N'Câu hỏi',          COUNT(*) FROM RtQuestions
UNION ALL SELECT N'Phương án',        COUNT(*) FROM RtQuestionOptions
UNION ALL SELECT N'Lượt làm bài',     COUNT(*) FROM RtAttempts
UNION ALL SELECT N'Đáp án đã chọn',   COUNT(*) FROM RtAttemptAnswers;

PRINT N'--- Câu hỏi theo Part ---';
SELECT Part, COUNT(*) AS SoCau FROM RtQuestions GROUP BY Part ORDER BY Part;

PRINT N'--- Kết quả từng lượt ---';
SELECT StudentCode, StudentName, Score, CorrectCount, WrongCount, SkippedCount, Status
FROM RtAttempts ORDER BY Id;
GO

/* ---------------------------------------------------------------------
   7. MẸO: xem nhanh 1 bảng bằng thủ tục có sẵn của SQL Server
   --------------------------------------------------------------------- */
-- EXEC sp_help 'RtQuestions';       -- cột, kiểu, index, khoá
-- EXEC sp_helpconstraint 'RtQuestions';  -- chỉ khoá và ràng buộc
GO
