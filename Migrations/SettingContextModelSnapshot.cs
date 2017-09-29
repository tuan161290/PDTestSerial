using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using PDTestSerial.Model;

namespace PDTestSerial.Migrations
{
    [DbContext(typeof(SettingContext))]
    partial class SettingContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.2");

            modelBuilder.Entity("PDTestSerial.Model.JigModel", b =>
                {
                    b.Property<int>("JigModelID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Channel");

                    b.Property<bool>("IsSetInJig");

                    b.Property<string>("JigDesciption");

                    b.Property<int>("JigID");

                    b.Property<int>("JigPos");

                    b.Property<int>("JigTestResult");

                    b.HasKey("JigModelID");

                    b.ToTable("JigModels");
                });

            modelBuilder.Entity("PDTestSerial.Model.Position", b =>
                {
                    b.Property<uint>("PositionID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("PosDescription");

                    b.Property<int>("PositionValue");

                    b.HasKey("PositionID");

                    b.ToTable("Positions");
                });

            modelBuilder.Entity("PDTestSerial.Model.SWSetting", b =>
                {
                    b.Property<uint>("SWSettingID")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("LOADTest");

                    b.Property<bool>("PDCTest");

                    b.Property<bool>("SBUTest");

                    b.Property<bool>("UD3Test");

                    b.Property<bool>("UO2Test");

                    b.Property<bool>("UO3Test");

                    b.Property<bool>("VCONNTest");

                    b.HasKey("SWSettingID");

                    b.ToTable("SWSettings");
                });

            modelBuilder.Entity("PDTestSerial.Model.ValueSetting", b =>
                {
                    b.Property<uint>("ValueSettingID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Key");

                    b.Property<string>("Value");

                    b.HasKey("ValueSettingID");

                    b.ToTable("ValueSettings");
                });
        }
    }
}
