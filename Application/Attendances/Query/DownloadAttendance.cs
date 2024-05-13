using System.Globalization;
using Application.Core;
using Domain.Attendances;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using Persistence;

public class DownloadAttendance
{
    public class AttendanceQuery : IRequest<Result<(byte[], string)>>
    {
        public string UniqueNumberOfClassRoom { get; set; }
        public string Year { get; set; }
        public string Month { get; set; }
    }

    public class AttendanceQueryHandler : IRequestHandler<AttendanceQuery, Result<(byte[], string)>>
    {
        private readonly DataContext _context;

        public AttendanceQueryHandler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<(byte[], string)>> Handle(AttendanceQuery request, CancellationToken cancellationToken)
        {
            /** Langkah 1: Validasi input parameter **/
            if (string.IsNullOrEmpty(request.UniqueNumberOfClassRoom) || string.IsNullOrEmpty(request.Year) || string.IsNullOrEmpty(request.Month))
            {
                return Result<(byte[], string)>.Failure("Parameter Unik Nomor Ruang Kelas, Tahun, dan Bulan diperlukan.");
            }

            /** Langkah 2: Cari kelas yang sesuai dengan UniqueNumberOfClassRoom **/
            var classRoom = await _context.ClassRooms.FirstOrDefaultAsync(cr => cr.UniqueNumberOfClassRoom == request.UniqueNumberOfClassRoom);

            /** Langkah 3: Periksa apakah kelas ditemukan **/
            if (classRoom == null)
            {
                return Result<(byte[], string)>.Failure("Kelas tidak ditemukan.");
            }

            /** Langkah 4: Tentukan rentang tanggal dari bulan yang diminta **/
            var startDate = new DateOnly(int.Parse(request.Year), int.Parse(request.Month), 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            /** Langkah 5: Ambil data kehadiran dari database sesuai rentang tanggal dan kelas **/
            var attendances = await _context.Attendances
                .Include(a => a.Student)
                .ThenInclude(s => s.ClassRoom)
                .Where(a => a.Date >= startDate && a.Date <= endDate && a.Student.ClassRoom.UniqueNumberOfClassRoom == request.UniqueNumberOfClassRoom)
                .ToListAsync(cancellationToken);

            /** Langkah 6: Periksa apakah ada data kehadiran yang ditemukan **/
            if (!attendances.Any())
            {
                return Result<(byte[], string)>.Failure("Data kehadiran tidak ditemukan.");
            }

            /** Langkah 7: Buat workbook Excel dan inisialisasi sheet **/
            var workbook = new XSSFWorkbook();
            var sheet = workbook.CreateSheet("Attendance");

            /** Langkah 8: Buat style untuk sel **/
            var styles = CreateStyles(workbook);

            /** Langkah 9: Tulis header pada sheet Excel **/
            WriteHeader(sheet, startDate, endDate, styles);

            /** Langkah 10: Tulis data kehadiran pada sheet Excel **/
            WriteAttendanceData(sheet, attendances, startDate, styles);

            /** Langkah 11: Konversi workbook ke byte array dan berikan nama file **/
            using (var ms = new MemoryStream())
            {
                workbook.Write(ms);
                var fileName = $"{classRoom.LongClassName}, {CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(startDate.Month)} {startDate.Year}";
                return Result<(byte[], string)>.Success((ms.ToArray(), fileName));
            }
        }

        // Method untuk membuat berbagai style sel pada Excel
        private Dictionary<string, ICellStyle> CreateStyles(XSSFWorkbook workbook)
        {
            // Dictionary untuk menyimpan berbagai styles
            var styles = new Dictionary<string, ICellStyle>();

            /** Langkah 1: Membuat style border untuk sel **/
            var borderedStyle = CreateCellStyle(workbook);
            styles.Add("bordered", borderedStyle);

            /** Langkah 2: Membuat styles khusus untuk status kehadiran **/
            styles.Add("present", GetCellStyle(workbook, 1));
            styles.Add("sick", GetCellStyle(workbook, 2));
            styles.Add("absent", GetCellStyle(workbook, 3));

            return styles;
        }

        private void WriteHeader(ISheet sheet, DateOnly startDate, DateOnly endDate, Dictionary<string, ICellStyle> styles)
        {
            var headerRow = sheet.CreateRow(0); // Membuat baris header pertama
            var headerRow2 = sheet.CreateRow(1); // Membuat baris header kedua

            /** Langkah 1: Menulis nama kolom "Nama Siswa" **/
            var nameCell = headerRow.CreateCell(0);
            nameCell.SetCellValue("Nama Siswa");
            nameCell.CellStyle = GetCellStyle(sheet.Workbook, 0); // Mendapatkan gaya sel dengan kode status 0
            sheet.AddMergedRegion(new CellRangeAddress(0, 1, 0, 0)); // Menggabungkan sel untuk nama siswa

            /** Langkah 2: Menulis nama kolom "NIS (Nomor Induk Siswa)" **/
            var nisCell = headerRow.CreateCell(1);
            nisCell.SetCellValue("NIS");
            nisCell.CellStyle = GetCellStyle(sheet.Workbook, 0); // Mendapatkan gaya sel dengan kode status 0
            sheet.AddMergedRegion(new CellRangeAddress(0, 1, 1, 1)); // Menggabungkan sel untuk NIS

            /** Langkah 3: Menulis bulan dan tahun **/
            var monthYearCell = headerRow.CreateCell(2);
            monthYearCell.SetCellValue(startDate.ToString("MMMM yyyy"));
            monthYearCell.CellStyle = GetCellStyle(sheet.Workbook, 0); // Mendapatkan gaya sel dengan kode status 0
            var daysInMonth = endDate.Day - startDate.Day + 1; // Mendapatkan jumlah hari dalam bulan
            sheet.AddMergedRegion(new CellRangeAddress(0, 0, 2, 1 + daysInMonth)); // Menggabungkan sel untuk bulan dan tahun

            /** Langkah 4: Menulis tanggal dalam bulan **/
            for (int i = 0; i < daysInMonth; i++)
            {
                var date = startDate.AddDays(i);
                var dateCell = headerRow2.CreateCell(2 + i);
                dateCell.SetCellValue(date.Day);

                // Periksa apakah hari Sabtu atau Minggu dan beri warna sesuai
                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                {
                    dateCell.CellStyle = GetCellStyle(sheet.Workbook, 4); // Warna merah untuk hari Sabtu dan Minggu
                }
                else
                {
                    dateCell.CellStyle = GetCellStyle(sheet.Workbook, 0); // Warna default
                }
            }

            /** Langkah 5: Menulis keterangan hadir, sakit, dan tidak hadir **/
            var keteranganCell = headerRow.CreateCell(2 + daysInMonth);
            keteranganCell.SetCellValue("Keterangan");
            keteranganCell.CellStyle = GetCellStyle(sheet.Workbook, 0); // Mendapatkan gaya sel dengan kode status 0
            sheet.AddMergedRegion(new CellRangeAddress(0, 0, 2 + daysInMonth, 4 + daysInMonth)); // Menggabungkan sel untuk keterangan

            /** Langkah 6: Menulis label "Hadir", "Izin", dan "Alpha" pada baris header kedua **/
            var hadirCell = headerRow2.CreateCell(2 + daysInMonth);
            hadirCell.SetCellValue("Hadir");
            hadirCell.CellStyle = styles["present"]; // Menggunakan gaya sel untuk hadir

            var sakitCell = headerRow2.CreateCell(3 + daysInMonth);
            sakitCell.SetCellValue("Izin");
            sakitCell.CellStyle = styles["sick"]; // Menggunakan gaya sel untuk izin

            var tidakHadirCell = headerRow2.CreateCell(4 + daysInMonth);
            tidakHadirCell.SetCellValue("Alpha");
            tidakHadirCell.CellStyle = styles["absent"]; // Menggunakan gaya sel untuk tidak hadir
        }

        private void WriteAttendanceData(ISheet sheet, List<Attendance> attendances, DateOnly startDate, Dictionary<string, ICellStyle> styles)
        {
            int rowIndex = 2; // Mulai menulis data dari baris ke-3
            int lastRow = rowIndex + attendances.Count - 1; // Menghitung baris terakhir setelah menulis semua data
            var daysInMonth = startDate.AddMonths(1).AddDays(-1).Day; // Menghitung jumlah hari dalam bulan

            /** Langkah 1: Iterasi melalui setiap kelompok kehadiran siswa berdasarkan ID siswa **/
            foreach (var attendanceGroup in attendances.GroupBy(a => a.StudentId))
            {
                var row = sheet.CreateRow(rowIndex++); // Membuat baris baru untuk setiap siswa

                var student = attendanceGroup.First().Student; // Mendapatkan objek siswa dari kelompok kehadiran

                /** Langkah 2: Menulis Nama Siswa ke sel pertama di setiap baris **/
                var nameCell = row.CreateCell(0);
                nameCell.SetCellValue(student.NameStudent); // Menetapkan nilai Nama Siswa
                nameCell.CellStyle = styles["bordered"]; // Menetapkan gaya sel dengan border

                /** Langkah 3: Menulis NIS (Nomor Induk Siswa) ke sel kedua di setiap baris **/
                var nisCell = row.CreateCell(1);
                nisCell.SetCellValue(student.Nis); // Menetapkan nilai NIS dari objek siswa
                nisCell.CellStyle = styles["bordered"]; // Menetapkan gaya sel dengan border

                /** Langkah 4: Menulis status kehadiran untuk setiap hari dalam bulan **/
                for (int i = 0; i < daysInMonth; i++)
                {
                    var date = startDate.AddDays(i); // Mendapatkan tanggal untuk setiap hari dalam bulan
                    var attendance = attendanceGroup.FirstOrDefault(a => a.Date == date); // Mencari kehadiran siswa pada tanggal tersebut
                    var statusCell = row.CreateCell(2 + i); // Membuat sel baru untuk status kehadiran

                    // Periksa apakah hari Sabtu atau Minggu dan atur gaya sel sesuai
                    if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                    {
                        statusCell.CellStyle = GetCellStyle(sheet.Workbook, 4); // Menggunakan gaya sel untuk hari Sabtu dan Minggu (merah)
                    }
                    else
                    {
                        statusCell.CellStyle = styles["bordered"]; // Menggunakan gaya sel default dengan border
                    }

                    if (attendance != null)
                    {
                        statusCell.SetCellValue(GetStatusString(attendance.Status)); // Menetapkan nilai status kehadiran
                        statusCell.CellStyle = styles[GetStatusKey(attendance.Status)]; // Menetapkan gaya sel sesuai status kehadiran
                    }
                    else
                    {
                        statusCell.SetCellValue(""); // Jika tidak ada data kehadiran, set nilai sel menjadi kosong
                    }
                }

                /** Langkah 5: Menulis jumlah hadir, sakit, dan tidak hadir di akhir baris **/
                var attendanceCounts = attendanceGroup.GroupBy(a => a.Status).ToDictionary(g => g.Key, g => g.Count());
                var presentCellStyle = styles["present"]; // Mendapatkan gaya sel untuk jumlah hadir
                var sickCellStyle = styles["sick"]; // Mendapatkan gaya sel untuk jumlah sakit
                var absentCellStyle = styles["absent"]; // Mendapatkan gaya sel untuk jumlah tidak hadir

                // Menulis jumlah hadir di kolom terakhir + 1
                row.CreateCell(2 + daysInMonth).SetCellValue(attendanceCounts.GetValueOrDefault(1, 0));
                row.GetCell(2 + daysInMonth).CellStyle = presentCellStyle; // Menggunakan gaya sel untuk jumlah hadir

                // Menulis jumlah sakit di kolom terakhir + 2
                row.CreateCell(3 + daysInMonth).SetCellValue(attendanceCounts.GetValueOrDefault(2, 0));
                row.GetCell(3 + daysInMonth).CellStyle = sickCellStyle; // Menggunakan gaya sel untuk jumlah sakit

                // Menulis jumlah tidak hadir di kolom terakhir + 3
                row.CreateCell(4 + daysInMonth).SetCellValue(attendanceCounts.GetValueOrDefault(3, 0));
                row.GetCell(4 + daysInMonth).CellStyle = absentCellStyle; // Menggunakan gaya sel untuk jumlah tidak hadir
            }
        }

        private string GetStatusKey(int statusCode)
        {
            switch (statusCode)
            {
                case 1: return "present";
                case 2: return "sick";
                case 3: return "absent";
                default: return "bordered";
            }
        }

        // Method to convert status code to string
        private string GetStatusString(int statusCode)
        {
            switch (statusCode)
            {
                case 1: // Hadir
                    return "H";
                case 2: // Sakit
                    return "I";
                case 3: // Tidak Hadir
                    return "A";
                default:
                    return "";
            }
        }

        private ICellStyle CreateCellStyle(XSSFWorkbook workbook, short? fillColor = null)
        {
            /** Langkah 1: Membuat objek gaya sel baru **/
            var cellStyle = workbook.CreateCellStyle();
            var font = workbook.CreateFont(); // Membuat objek font baru

            /** Langkah 2: Menetapkan ukuran font **/
            font.FontHeightInPoints = 10;

            /** Langkah 3: Menetapkan alignment untuk gaya sel **/
            cellStyle.Alignment = HorizontalAlignment.Center; // Pusatkan horizontal
            cellStyle.VerticalAlignment = VerticalAlignment.Center; // Pusatkan vertikal

            /** Langkah 4: Menerapkan gaya border ke semua sisi sel **/
            cellStyle.BorderTop = BorderStyle.Thin;
            cellStyle.BorderBottom = BorderStyle.Thin;
            cellStyle.BorderLeft = BorderStyle.Thin;
            cellStyle.BorderRight = BorderStyle.Thin;

            /** Langkah 5: Jika warna diisi, tentukan warna latar belakang sel **/
            if (fillColor != null)
            {
                cellStyle.FillForegroundColor = fillColor.Value;
                cellStyle.FillPattern = FillPattern.SolidForeground;
            }

            cellStyle.SetFont(font); /** Langkah 6: Menerapkan font ke gaya sel **/
            return cellStyle; // Mengembalikan objek gaya sel yang telah dibuat
        }

        // Method to get cell style based on status
        private ICellStyle GetCellStyle(IWorkbook workbook, int statusCode)
        {
            /** Langkah 1: Membuat objek gaya sel baru **/
            var cellStyle = workbook.CreateCellStyle();
            var font = workbook.CreateFont(); // Membuat objek font baru

            /** Langkah 2: Menetapkan ukuran font **/
            font.FontHeightInPoints = 10;

            /** Langkah 3: Menetapkan alignment untuk gaya sel **/
            cellStyle.Alignment = HorizontalAlignment.Center; // Pusatkan horizontal
            cellStyle.VerticalAlignment = VerticalAlignment.Center; // Pusatkan vertikal

            /** Langkah 4: Menerapkan gaya border ke semua sisi sel **/
            cellStyle.BorderTop = BorderStyle.Thin;
            cellStyle.BorderBottom = BorderStyle.Thin;
            cellStyle.BorderLeft = BorderStyle.Thin;
            cellStyle.BorderRight = BorderStyle.Thin;

            /** Langkah 5: Set font bold jika kode status adalah 0 **/
            if (statusCode == 0)
            {
                font.IsBold = true;
            }

            /** Langkah 6: Berdasarkan status kode, tentukan warna latar belakang sel **/
            switch (statusCode)
            {
                case 0: // Warna biru untuk nama siswa dan kelas
                    cellStyle.FillForegroundColor = IndexedColors.SkyBlue.Index;
                    cellStyle.FillPattern = FillPattern.SolidForeground;
                    break;
                case 1: // Hadir
                    cellStyle.FillForegroundColor = IndexedColors.LightGreen.Index;
                    cellStyle.FillPattern = FillPattern.SolidForeground;
                    break;
                case 2: // Sakit
                    cellStyle.FillForegroundColor = IndexedColors.Yellow.Index;
                    cellStyle.FillPattern = FillPattern.SolidForeground;
                    break;
                case 3: // Tidak Hadir
                    cellStyle.FillForegroundColor = IndexedColors.LightOrange.Index;
                    cellStyle.FillPattern = FillPattern.SolidForeground;
                    break;
                case 4: // Sabtu & Minggu
                    cellStyle.FillForegroundColor = IndexedColors.Red.Index;
                    cellStyle.FillPattern = FillPattern.SolidForeground;
                    break;
            }

            /** Langkah 7: Menerapkan font ke gaya sel **/
            cellStyle.SetFont(font);
            return cellStyle; // Mengembalikan objek gaya sel yang telah dibuat
        }
    }
}