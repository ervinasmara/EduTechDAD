﻿using Domain.Class;

namespace Persistence.Seed
{
    public class SeedClassRoom
    {
        public static async Task SeedData(DataContext context)
        {
            if (context.ClassRooms.Any()) return;

            var classmajors = new List<ClassRoom>
            {
                new ClassRoom
                {
                    ClassName = "TKJ",
                    LongClassName = "Teknik Komputer Jaringan",
                    UniqueNumberOfClassRoom = "001",
                },
                new ClassRoom
                {
                    ClassName = "TKR",
                    LongClassName = "Teknik Kendaraan Ringan",
                    UniqueNumberOfClassRoom = "002",
                },
                new ClassRoom
                {
                    ClassName = "RPL",
                    LongClassName = "Rekayasa Perangkat Lunak",
                    UniqueNumberOfClassRoom = "003",
                },
            };

            await context.ClassRooms.AddRangeAsync(classmajors);
            await context.SaveChangesAsync();
        }
    }
}