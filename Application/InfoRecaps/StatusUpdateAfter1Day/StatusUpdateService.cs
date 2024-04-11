using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Persistence;

namespace Application.InfoRecaps.StatusUpdateAfter1Day
{
    public class StatusUpdateService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public StatusUpdateService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();

                    // Ambil semua InfoRecap dari database
                    var infoRecaps = dbContext.InfoRecaps.ToList();

                    foreach (var infoRecap in infoRecaps)
                    {
                        // Periksa apakah LastStatusChangeDate adalah hari sebelumnya
                        if (infoRecap.LastStatusChangeDate.Date < DateTime.Today)
                        {
                            // Ubah status menjadi 1
                            infoRecap.Status = 1;
                            infoRecap.LastStatusChangeDate = DateTime.Today;
                        }
                    }

                    await dbContext.SaveChangesAsync(stoppingToken);
                }

                // Tunggu satu hari sebelum memeriksa lagi
                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }
    }
}