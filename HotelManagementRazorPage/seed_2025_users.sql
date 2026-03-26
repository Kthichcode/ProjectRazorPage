-- =====================================================================
-- Seed 5 sample users + bookings for year 2025
-- Database: HotelManagementRazorPageDb
-- =====================================================================
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
GO


-- Lấy một RoomId hợp lệ để gắn vào BookingRoom
DECLARE @RoomId INT = (SELECT TOP 1 Id FROM Rooms ORDER BY Id);
IF @RoomId IS NULL
BEGIN
    PRINT 'Không tìm thấy phòng nào. Hãy thêm phòng trước!';
    RETURN;
END

-- ── Xóa seed cũ nếu chạy lại ──────────────────────────────────────
DELETE FROM BookingRooms WHERE BookingId IN (
    SELECT Id FROM Bookings WHERE CustomerId LIKE 'seed-2025-%'
);
DELETE FROM Bookings WHERE CustomerId LIKE 'seed-2025-%';
DELETE FROM UserRoles WHERE UserId LIKE 'seed-2025-%';
DELETE FROM Users WHERE Id LIKE 'seed-2025-%';

-- ── Tạo 5 user mẫu ────────────────────────────────────────────────
DECLARE @PwHash NVARCHAR(MAX) =
    'AQAAAAIAAYagAAAAEJ6UdnlD4g3h3RX/lk63z3nqmKU2rH7O0VKR6Yc5Jkqm5TXqvbEFi6ys4e2hv7ghpQ==';

INSERT INTO Users (Id, UserName, NormalizedUserName, Email, NormalizedEmail,
                   EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp,
                   PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled,
                   LockoutEnabled, AccessFailedCount, FullName)
VALUES
('seed-2025-u1','nguyen.an.2025@mail.com','NGUYEN.AN.2025@MAIL.COM',
 'nguyen.an.2025@mail.com','NGUYEN.AN.2025@MAIL.COM',
 1, @PwHash, NEWID(), NEWID(), NULL, 0, 0, 1, 0, N'Nguyễn An'),

('seed-2025-u2','tran.binh.2025@mail.com','TRAN.BINH.2025@MAIL.COM',
 'tran.binh.2025@mail.com','TRAN.BINH.2025@MAIL.COM',
 1, @PwHash, NEWID(), NEWID(), NULL, 0, 0, 1, 0, N'Trần Bình'),

('seed-2025-u3','le.chi.2025@mail.com','LE.CHI.2025@MAIL.COM',
 'le.chi.2025@mail.com','LE.CHI.2025@MAIL.COM',
 1, @PwHash, NEWID(), NEWID(), NULL, 0, 0, 1, 0, N'Lê Chí'),

('seed-2025-u4','pham.dung.2025@mail.com','PHAM.DUNG.2025@MAIL.COM',
 'pham.dung.2025@mail.com','PHAM.DUNG.2025@MAIL.COM',
 1, @PwHash, NEWID(), NEWID(), NULL, 0, 0, 1, 0, N'Phạm Dũng'),

('seed-2025-u5','hoang.em.2025@mail.com','HOANG.EM.2025@MAIL.COM',
 'hoang.em.2025@mail.com','HOANG.EM.2025@MAIL.COM',
 1, @PwHash, NEWID(), NEWID(), NULL, 0, 0, 1, 0, N'Hoàng Em');

-- ── Tạo booking cho từng user (rải đều qua các tháng 2025) ─────────
INSERT INTO Bookings (CustomerId, CheckInDate, CheckOutDate, Status, TotalAmount, CreatedAt)
VALUES
('seed-2025-u1', '2025-02-10', '2025-02-13', 3, 2400000, '2025-02-08 09:00:00'),
('seed-2025-u2', '2025-04-15', '2025-04-18', 3, 1800000, '2025-04-12 11:00:00'),
('seed-2025-u3', '2025-06-20', '2025-06-23', 3, 3600000, '2025-06-18 14:00:00'),
('seed-2025-u4', '2025-08-05', '2025-08-08', 3, 2100000, '2025-08-03 08:30:00'),
('seed-2025-u5', '2025-10-11', '2025-10-14', 3, 4500000, '2025-10-09 16:00:00');

-- ── Gắn phòng vào BookingRooms ────────────────────────────────────
INSERT INTO BookingRooms (BookingId, RoomId)
SELECT b.Id, @RoomId
FROM Bookings b
WHERE b.CustomerId LIKE 'seed-2025-%';

PRINT 'Seed hoan tat! 5 user + 5 booking nam 2025 da duoc them.';

SELECT b.Id, b.CustomerId, b.CreatedAt, b.TotalAmount, b.Status
FROM Bookings b
WHERE b.CustomerId LIKE 'seed-2025-%'
ORDER BY b.CreatedAt;
