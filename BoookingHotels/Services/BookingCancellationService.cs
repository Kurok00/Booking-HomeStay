using BoookingHotels.Data;
using BoookingHotels.Models;
using Microsoft.EntityFrameworkCore;

namespace BoookingHotels.Services
{
    public class BookingCancellationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BookingCancellationService> _logger;

        public BookingCancellationService(
            IServiceProvider serviceProvider,
            ILogger<BookingCancellationService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Booking Cancellation Service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CancelExpiredBookings();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while canceling expired bookings.");
                }

                // Check every 5 minutes
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        private async Task CancelExpiredBookings()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var now = DateTime.Now;

            // Find all ONLINE bookings that are STILL Pending and past deadline
            // Only cancel if status is still Pending (not Paid or Confirmed by admin)
            var expiredBookings = await context.Bookings
                .Where(b => b.Status == BookingStatus.Pending
                    && b.PaymentMethod == "ONLINE"
                    && b.PaymentDeadline.HasValue
                    && b.PaymentDeadline.Value < now)
                .ToListAsync();

            if (expiredBookings.Any())
            {
                _logger.LogInformation($"Found {expiredBookings.Count} expired bookings to cancel.");

                foreach (var booking in expiredBookings)
                {
                    // Double check status before canceling (in case admin just confirmed)
                    if (booking.Status == BookingStatus.Pending)
                    {
                        booking.Status = BookingStatus.Canceled;
                        _logger.LogInformation($"Auto-canceled booking #{booking.BookingId} - Payment deadline expired at {booking.PaymentDeadline}");
                    }
                    else
                    {
                        _logger.LogInformation($"Skipped booking #{booking.BookingId} - Status changed to {booking.Status} (admin confirmed payment)");
                    }
                }

                await context.SaveChangesAsync();
                _logger.LogInformation($"Successfully processed {expiredBookings.Count} expired bookings.");
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Booking Cancellation Service is stopping.");
            await base.StopAsync(stoppingToken);
        }
    }
}
