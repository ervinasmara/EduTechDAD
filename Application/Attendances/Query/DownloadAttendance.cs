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
            // Validasi input
            if (string.IsNullOrEmpty(request.UniqueNumberOfClassRoom) || string.IsNullOrEmpty(request.Year) || string.IsNullOrEmpty(request.Month))
            {
                return Result<(byte[], string)>.Failure("Parameter Unik Nomor Ruang Kelas, Tahun, dan Bulan diperlukan.");
            }

            // Cari kelas yang sesuai dengan UniqueNumberOfClassRoom
            var classRoom = await _context.ClassRooms.FirstOrDefaultAsync(cr => cr.UniqueNumberOfClassRoom == request.UniqueNumberOfClassRoom);

            // Cek apakah kelas ditemukan
            if (classRoom == null)
            {
                return Result<(byte[], string)>.Failure("Kelas tidak ditemukan.");
            }

            // Tentukan rentang tanggal
            var startDate = new DateOnly(int.Parse(request.Year), int.Parse(request.Month), 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            // Ambil data kehadiran
            var attendances = await _context.Attendances
                .Include(a => a.Student)
                .ThenInclude(s => s.ClassRoom)
                .Where(a => a.Date >= startDate && a.Date <= endDate && a.Student.ClassRoom.UniqueNumberOfClassRoom == request.UniqueNumberOfClassRoom)
                .ToListAsync(cancellationToken);

            // Cek apakah ada data
            if (!attendances.Any())
            {
                return Result<(byte[], string)>.Failure("Data kehadiran tidak ditemukan.");
            }

            // Buat workbook Excel
            var workbook = new XSSFWorkbook();
            var sheet = workbook.CreateSheet("Attendance");

            // Buat style untuk sel
            var styles = CreateStyles(workbook);

            // Tulis header
            WriteHeader(sheet, startDate, endDate, styles);

            // Tulis data kehadiran
            WriteAttendanceData(sheet, attendances, startDate, styles);

            // Konversi workbook ke byte array
            using (var ms = new MemoryStream())
            {
                workbook.Write(ms);
                var fileName = $"{classRoom.LongClassName}, {CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(startDate.Month)} {startDate.Year}";
                return Result<(byte[], string)>.Success((ms.ToArray(), fileName));
            }
        }

        private Dictionary<string, ICellStyle> CreateStyles(XSSFWorkbook workbook)
        {
            // Dictionary untuk menyimpan berbagai styles
            var styles = new Dictionary<string, ICellStyle>();

            // Style umum untuk sel dengan border
            var borderedStyle = CreateCellStyle(workbook);
            styles.Add("bordered", borderedStyle);

            // Styles khusus untuk status kehadiran
            styles.Add("present", GetCellStyle(workbook, 1));
            styles.Add("sick", GetCellStyle(workbook, 2));
            styles.Add("absent", GetCellStyle(workbook, 3));

            return styles;
        }

        private void WriteHeader(ISheet sheet, DateOnly startDate, DateOnly endDate, Dictionary<string, ICellStyle> styles)
        {
            var headerRow = sheet.CreateRow(0);
            var headerRow2 = sheet.CreateRow(1);

            // Nama Siswa
            var nameCell = headerRow.CreateCell(0);
            nameCell.SetCellValue("Nama Siswa");
            nameCell.CellStyle = GetCellStyle(sheet.Workbook, 0); // Memanggil metode GetCellStyle dengan kode status 0
            sheet.AddMergedRegion(new CellRangeAddress(0, 1, 0, 0));

            // NIS (Nomor Induk Siswa)
            var nisCell = headerRow.CreateCell(1);
            nisCell.SetCellValue("NIS");
            nisCell.CellStyle = GetCellStyle(sheet.Workbook, 0); // Memanggil metode GetCellStyle dengan kode status 0
            sheet.AddMergedRegion(new CellRangeAddress(0, 1, 1, 1));

            // Bulan dan Tahun
            var monthYearCell = headerRow.CreateCell(2);
            monthYearCell.SetCellValue(startDate.ToString("MMMM yyyy"));
            monthYearCell.CellStyle = GetCellStyle(sheet.Workbook, 0); // Memanggil metode GetCellStyle dengan kode status 0
            var daysInMonth = endDate.Day - startDate.Day + 1;
            sheet.AddMergedRegion(new CellRangeAddress(0, 0, 2, 1 + daysInMonth));

            // Tanggal dalam bulan
            for (int i = 0; i < daysInMonth; i++)
            {
                var date = startDate.AddDays(i);
                var dateCell = headerRow2.CreateCell(2 + i);
                dateCell.SetCellValue(date.Day);

                // Periksa apakah hari Sabtu atau Minggu
                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                {
                    dateCell.CellStyle = GetCellStyle(sheet.Workbook, 3); // Warna merah untuk hari Sabtu dan Minggu
                }
                else
                {
                    dateCell.CellStyle = GetCellStyle(sheet.Workbook, 0); // Warna default
                }
            }


            // Keterangan
            var keteranganCell = headerRow.CreateCell(2 + daysInMonth);
            keteranganCell.SetCellValue("Keterangan");
            keteranganCell.CellStyle = GetCellStyle(sheet.Workbook, 0); // Memanggil metode GetCellStyle dengan kode status 0
            sheet.AddMergedRegion(new CellRangeAddress(0, 0, 2 + daysInMonth, 4 + daysInMonth));

            // H, I, T
            var hadirCell = headerRow2.CreateCell(2 + daysInMonth);
            hadirCell.SetCellValue("H");
            hadirCell.CellStyle = styles["present"];

            var sakitCell = headerRow2.CreateCell(3 + daysInMonth);
            sakitCell.SetCellValue("I");
            sakitCell.CellStyle = styles["sick"];

            var tidakHadirCell = headerRow2.CreateCell(4 + daysInMonth);
            tidakHadirCell.SetCellValue("T");
            tidakHadirCell.CellStyle = styles["absent"];
        }

        private void MergeWeekendColumns(ISheet sheet, DateOnly startDate, int daysInMonth, int lastRow)
        {
            // Loop untuk setiap hari dalam bulan
            for (int i = 0; i < daysInMonth; i++)
            {
                var date = startDate.AddDays(i);
                int columnIndex = 2 + i; // Kolom dimulai dari 2 karena 0 dan 1 digunakan untuk data lain

                // Jika hari adalah Sabtu atau Minggu, merge kolom tersebut
                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                {
                    // Merge kolom dari baris ke-3 hingga baris terakhir data
                    if (lastRow > 2) // Pastikan ada lebih dari satu baris untuk di-merge
                    {
                        sheet.AddMergedRegion(new CellRangeAddress(2, lastRow, columnIndex, columnIndex));
                    }
                }
            }
        }

        private void LabelAndRotateWeekendColumns(ISheet sheet, DateOnly startDate, int daysInMonth)
        {
            ICellStyle verticalTextStyle = sheet.Workbook.CreateCellStyle();
            verticalTextStyle.Rotation = 90; // Putar teks 90 derajat

            // Loop untuk setiap hari dalam bulan
            for (int i = 0; i < daysInMonth; i++)
            {
                var date = startDate.AddDays(i);
                int columnIndex = 2 + i; // Kolom dimulai dari 2 karena 0 dan 1 digunakan untuk data lain

                // Jika hari adalah Sabtu, tulis "Sabtu" dan putar teks
                if (date.DayOfWeek == DayOfWeek.Saturday)
                {
                    ICell cell = sheet.GetRow(2).CreateCell(columnIndex); // Asumsikan baris 1 digunakan untuk label hari
                    cell.SetCellValue("Sabtu");
                    cell.CellStyle = verticalTextStyle;
                }
            }
        }

        // Di dalam metode WriteAttendanceData, setelah loop foreach



        private void WriteAttendanceData(ISheet sheet, List<Attendance> attendances, DateOnly startDate, Dictionary<string, ICellStyle> styles)
        {
            int rowIndex = 2;
            int lastRow = rowIndex + attendances.Count - 1; // Baris terakhir setelah menulis semua data
            var daysInMonth = startDate.AddMonths(1).AddDays(-1).Day;

            foreach (var attendanceGroup in attendances.GroupBy(a => a.StudentId))
            {
                var row = sheet.CreateRow(rowIndex++);
                var student = attendanceGroup.First().Student;

                // Nama Siswa
                var nameCell = row.CreateCell(0);
                nameCell.SetCellValue(student.NameStudent);
                nameCell.CellStyle = styles["bordered"];
                // Tambahkan border di sebelah kanan
                //nameCell.CellStyle.BorderRight = BorderStyle.Thin;

                // NIS (Nomor Induk Siswa)
                var nisCell = row.CreateCell(1);
                nisCell.SetCellValue(student.Nis); // Ambil NIS dari objek student
                nisCell.CellStyle = styles["bordered"];

                // Status Kehadiran
                for (int i = 0; i < daysInMonth; i++)
                {
                    var date = startDate.AddDays(i);
                    var attendance = attendanceGroup.FirstOrDefault(a => a.Date == date);
                    var statusCell = row.CreateCell(2 + i);

                    // Periksa apakah hari Sabtu atau Minggu
                    if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                    {
                        statusCell.CellStyle = GetCellStyle(sheet.Workbook, 3); // Warna merah untuk hari Sabtu dan Minggu
                    }
                    else
                    {
                        statusCell.CellStyle = styles["bordered"]; // Gaya sel default
                    }

                    if (attendance != null)
                    {
                        statusCell.SetCellValue(GetStatusString(attendance.Status));
                        statusCell.CellStyle = styles[GetStatusKey(attendance.Status)]; // Terapkan gaya sel sesuai status kehadiran
                    }
                    else
                    {
                        statusCell.SetCellValue(""); // Jika tidak ada data kehadiran, set nilai sel menjadi kosong
                    }
                }


                // Jumlah Hadir, Sakit, Tidak Hadir
                var attendanceCounts = attendanceGroup.GroupBy(a => a.Status).ToDictionary(g => g.Key, g => g.Count());
                row.CreateCell(2 + daysInMonth).SetCellValue(attendanceCounts.GetValueOrDefault(1, 0));
                row.GetCell(2 + daysInMonth).CellStyle = styles["bordered"];

                row.CreateCell(3 + daysInMonth).SetCellValue(attendanceCounts.GetValueOrDefault(2, 0));
                row.GetCell(3 + daysInMonth).CellStyle = styles["bordered"];

                row.CreateCell(4 + daysInMonth).SetCellValue(attendanceCounts.GetValueOrDefault(3, 0));
                row.GetCell(4 + daysInMonth).CellStyle = styles["bordered"];
            }

            MergeWeekendColumns(sheet, startDate, daysInMonth, lastRow); // Panggil di luar loop foreach
        LabelAndRotateWeekendColumns(sheet, startDate, daysInMonth);
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
                    return "T";
                default:
                    return "";
            }
        }

        private ICellStyle CreateCellStyle(XSSFWorkbook workbook, short? fillColor = null)
        {
            var cellStyle = workbook.CreateCellStyle();
            var font = workbook.CreateFont();
            font.FontHeightInPoints = 10; // Set font size

            // Set alignment
            cellStyle.Alignment = HorizontalAlignment.Center;
            cellStyle.VerticalAlignment = VerticalAlignment.Center;

            // Apply border style to all sides
            cellStyle.BorderTop = BorderStyle.Thin;
            cellStyle.BorderBottom = BorderStyle.Thin;
            cellStyle.BorderLeft = BorderStyle.Thin;
            cellStyle.BorderRight = BorderStyle.Thin;

            if (fillColor != null)
            {
                cellStyle.FillForegroundColor = fillColor.Value;
                cellStyle.FillPattern = FillPattern.SolidForeground;
            }

            cellStyle.SetFont(font); // Apply font
            return cellStyle;
        }

        // Method to get cell style based on status
        private ICellStyle GetCellStyle(IWorkbook workbook, int statusCode)
        {
            var cellStyle = workbook.CreateCellStyle();
            var font = workbook.CreateFont();
            font.FontHeightInPoints = 10; // Set font size

            // Set alignment
            cellStyle.Alignment = HorizontalAlignment.Center;
            cellStyle.VerticalAlignment = VerticalAlignment.Center;

            // Apply border style to all sides
            cellStyle.BorderTop = BorderStyle.Thin;
            cellStyle.BorderBottom = BorderStyle.Thin;
            cellStyle.BorderLeft = BorderStyle.Thin;
            cellStyle.BorderRight = BorderStyle.Thin;

            // Set font bold
            if (statusCode == 0) // Jika kode status adalah 0 (untuk warna biru)
            {
                font.IsBold = true; // Set font bold
            }

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
                    cellStyle.FillForegroundColor = IndexedColors.Red.Index;
                    cellStyle.FillPattern = FillPattern.SolidForeground;
                    break;
            }

            cellStyle.SetFont(font); // Apply font
            return cellStyle;
        }
    }
}
