using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDTestSerial.Model
{
    public class SettingContext : DbContext
    {
        public DbSet<SWSetting> SWSettings { get; set; }
        public DbSet<ValueSetting> ValueSettings { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<JigModel> JigModels { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=Database.db");
        }
    }

    public class SWSetting
    {
        public uint SWSettingID { get; set; }
        public bool UD3Test { get; set; } = true;
        public bool UO3Test { get; set; } = true;
        public bool UO2Test { get; set; } = true;
        public bool PDCTest { get; set; } = true;
        public bool LOADTest { get; set; } = true;
        public bool VCONNTest { get; set; } = true;
        public bool SBUTest { get; set; } = true;
    }

    public class ValueSetting
    {
        public uint ValueSettingID { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public class Position
    {
        public uint PositionID { get; set; }
        public string PosDescription { get; set; }
        public int PositionValue { get; set; }
    }
}

