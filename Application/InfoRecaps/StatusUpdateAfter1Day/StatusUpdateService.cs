using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Persistence;

namespace Application.InfoRecaps.StatusUpdateAfter1Day
{
    public class StatusUpdateService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private Timer _timer;

        public StatusUpdateService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            // Membuat timer yang akan dipicu setiap hari pada pukul 00:00 waktu sistem
            var nowUtc = DateTime.UtcNow;
            var nowWib = nowUtc.AddHours(7); // Menambahkan offset waktu Indonesia (WIB)
            var midnightWib = nowWib.Date.AddDays(1);
            var dueTime = midnightWib - nowUtc;
            _timer = new Timer(DoWork, null, dueTime, TimeSpan.FromDays(1));

            return base.StartAsync(cancellationToken);
        }

        private void DoWork(object state)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();

                // Ambil entitas dengan status 2 dari database
                var infoRecaps = dbContext.InfoRecaps.Where(ir => ir.Status == 2).ToList();

                foreach (var infoRecap in infoRecaps)
                {
                    // Ubah status menjadi 1
                    infoRecap.Status = 1;
                    infoRecap.LastStatusChangeDate = DateTime.UtcNow.AddHours(7); // Tambahkan 7 jam untuk sesuaikan dengan waktu Indonesia
                }

                dbContext.SaveChanges(); // Simpan perubahan status ke database
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            // Memastikan timer dihentikan ketika layanan berhenti
            _timer?.Change(Timeout.Infinite, 0);
            return base.StopAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Metode ini tidak digunakan dalam implementasi ini
            return Task.CompletedTask;
        }
    }
}