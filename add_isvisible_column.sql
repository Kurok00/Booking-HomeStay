-- Thêm cột IsVisible vào bảng Reviews
IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE object_id = OBJECT_ID(N'[dbo].[Reviews]') 
               AND name = 'IsVisible')
BEGIN
    ALTER TABLE [dbo].[Reviews]
    ADD [IsVisible] bit NOT NULL DEFAULT 1;
END
GO

-- Cập nhật tất cả reviews hiện tại thành visible
UPDATE [dbo].[Reviews]
SET [IsVisible] = 1
WHERE [IsVisible] IS NULL OR [IsVisible] = 0;
GO
