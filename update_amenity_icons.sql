-- Script để update TẤT CẢ icons từ Font Awesome sang Bootstrap Icons
USE [BoookingHotels]
GO

-- Update WiFi
UPDATE Amenities SET Icon = 'bi bi-wifi' WHERE Icon LIKE '%wifi%' OR Icon LIKE '%internet%' OR Name LIKE '%Wi-Fi%' OR Name LIKE '%WiFi%';

-- Update Điều hòa / Air Conditioning
UPDATE Amenities SET Icon = 'bi bi-snow' WHERE Icon LIKE '%snow%' OR Icon LIKE '%air%' OR Name LIKE '%Điều hòa%';

-- Update TV
UPDATE Amenities SET Icon = 'bi bi-tv' WHERE Icon LIKE '%tv%' OR Name LIKE '%TV%';

-- Update Tủ lạnh / Fridge
UPDATE Amenities SET Icon = 'bi bi-box' WHERE Icon LIKE '%cube%' OR Icon LIKE '%fridge%' OR Name LIKE '%Tủ lạnh%';

-- Update Ban công / Balcony  
UPDATE Amenities SET Icon = 'bi bi-sunglasses' WHERE Icon LIKE '%home%' OR Icon LIKE '%balcony%' OR Name LIKE '%Ban công%';

-- Update Đỗ xe / Parking
UPDATE Amenities SET Icon = 'bi bi-car-front' WHERE Icon LIKE '%car%' OR Icon LIKE '%parking%' OR Name LIKE '%xe%' OR Name LIKE '%Bãi%';

-- Update Bể bơi / Pool
UPDATE Amenities SET Icon = 'bi bi-water' WHERE Icon LIKE '%pool%' OR Icon LIKE '%swim%' OR Name LIKE '%bơi%' OR Name LIKE '%Hồ%';

-- Update Gym
UPDATE Amenities SET Icon = 'bi bi-heart-pulse' WHERE Icon LIKE '%dumbbell%' OR Icon LIKE '%gym%' OR Name LIKE '%Gym%';

-- Update Spa
UPDATE Amenities SET Icon = 'bi bi-star' WHERE Icon LIKE '%spa%' OR Name LIKE '%Spa%';

-- Update Nhà hàng / Restaurant
UPDATE Amenities SET Icon = 'bi bi-shop' WHERE Icon LIKE '%utensil%' OR Icon LIKE '%restaurant%' OR Name LIKE '%Nhà hàng%';

-- Update Bar
UPDATE Amenities SET Icon = 'bi bi-cup-straw' WHERE Icon LIKE '%cocktail%' OR Icon LIKE '%bar%' OR Name LIKE '%Bar%';

-- Update Phòng họp / Meeting Room
UPDATE Amenities SET Icon = 'bi bi-people' WHERE Icon LIKE '%handshake%' OR Icon LIKE '%meeting%' OR Name LIKE '%họp%';

-- Update Máy sấy tóc / Hair Dryer
UPDATE Amenities SET Icon = 'bi bi-wind' WHERE Icon LIKE '%wind%' OR Icon LIKE '%dryer%' OR Name LIKE '%sấy%';

-- Update Két an toàn / Safe
UPDATE Amenities SET Icon = 'bi bi-shield-check' WHERE Icon LIKE '%shield%' OR Icon LIKE '%safe%' OR Name LIKE '%Két%';

-- Update Phục vụ 24/7 / 24h Service
UPDATE Amenities SET Icon = 'bi bi-clock' WHERE Icon LIKE '%clock%' OR Icon LIKE '%24%' OR Name LIKE '%24%' OR Name LIKE '%Phục vụ%';

-- Update Máy giặt / Washing Machine
UPDATE Amenities SET Icon = 'bi bi-disc' WHERE Icon LIKE '%tshirt%' OR Icon LIKE '%wash%' OR Name LIKE '%giặt%';

-- Update Bàn làm việc / Desk
UPDATE Amenities SET Icon = 'bi bi-person-workspace' WHERE Icon LIKE '%desk%' OR Icon LIKE '%workspace%' OR Name LIKE '%Bàn làm%';

-- Update Minibar (đã có bi bi-cup-straw)
-- Không cần update

-- Update Máy pha cà phê / Coffee Machine
UPDATE Amenities SET Icon = 'bi bi-cup-hot' WHERE Icon LIKE '%coffee%' OR Name LIKE '%cà phê%' OR Name LIKE '%Máy pha%';

-- Update View biển / Sea View
UPDATE Amenities SET Icon = 'bi bi-water' WHERE Icon LIKE '%water%' OR Icon LIKE '%sea%' OR Icon LIKE '%ocean%' OR Name LIKE '%View%' OR Name LIKE '%biển%';

-- Xem kết quả
SELECT AmenityId, Name, Icon FROM Amenities ORDER BY AmenityId;

PRINT 'Đã update xong TẤT CẢ icons cho Amenities sang Bootstrap Icons!'
GO
