-- Add PaymentDeadline and PaymentMethod columns to Bookings table

ALTER TABLE Bookings
ADD PaymentDeadline DATETIME2 NULL;

ALTER TABLE Bookings
ADD PaymentMethod NVARCHAR(50) NULL;

GO

PRINT 'Successfully added PaymentDeadline and PaymentMethod columns to Bookings table';
