-- Script để loại bỏ các amenities trùng lặp trong database
USE [BoookingHotels]
GO

-- Bước 1: Xem các amenities bị duplicate
SELECT Name, Icon, COUNT(*) as Count
FROM Amenities
GROUP BY Name, Icon
HAVING COUNT(*) > 1
ORDER BY Count DESC, Name;

PRINT '=== Các amenities bị duplicate trên ===';
GO

-- Bước 2: Tạo bảng mapping từ duplicate ID sang master ID
WITH DuplicateAmenities AS (
    SELECT 
        AmenityId,
        Name,
        Icon,
        ROW_NUMBER() OVER (PARTITION BY Name, Icon ORDER BY AmenityId) AS RowNum
    FROM Amenities
)
SELECT 
    old_id = da.AmenityId,
    master_id = (
        SELECT TOP 1 AmenityId 
        FROM Amenities a 
        WHERE a.Name = da.Name AND a.Icon = da.Icon
        ORDER BY AmenityId
    ),
    Name = da.Name,
    Icon = da.Icon
INTO #AmenityMapping
FROM DuplicateAmenities da
WHERE da.RowNum > 1;

PRINT 'Đã tạo bảng mapping cho các amenities duplicate';
GO

-- Bước 3: Update RoomAmenities để trỏ sang master amenity
UPDATE ra
SET ra.AmenityId = m.master_id
FROM RoomAmenities ra
INNER JOIN #AmenityMapping m ON ra.AmenityId = m.old_id;

PRINT 'Đã update RoomAmenities trỏ sang master amenity';
GO

-- Bước 4: Xóa các amenity duplicate (giữ lại master)
DELETE FROM Amenities
WHERE AmenityId IN (SELECT old_id FROM #AmenityMapping);

PRINT 'Đã xóa các amenities duplicate';
GO

-- Bước 5: Drop bảng temp
DROP TABLE #AmenityMapping;
GO

-- Bước 6: Xem kết quả cuối cùng
SELECT 
    COUNT(DISTINCT AmenityId) as TotalUniqueAmenities,
    COUNT(*) as TotalRows
FROM Amenities;

PRINT '';
PRINT '=== Danh sách Amenities sau khi clean ===';
SELECT AmenityId, Name, Icon 
FROM Amenities 
ORDER BY Name;
GO

-- Bước 7: Kiểm tra không còn duplicate
SELECT Name, Icon, COUNT(*) as Count
FROM Amenities
GROUP BY Name, Icon
HAVING COUNT(*) > 1;

PRINT '';
PRINT '✅ Nếu không có dòng nào ở trên = Không còn duplicate!';
GO
