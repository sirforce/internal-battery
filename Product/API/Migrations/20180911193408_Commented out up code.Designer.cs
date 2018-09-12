﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using UpDiddyApi.Models;

namespace UpDiddyApi.Migrations
{
    [DbContext(typeof(UpDiddyDbContext))]
    [Migration("20180911193408_Commented out up code")]
    partial class Commentedoutupcode
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.2-rtm-30932")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("UpDiddyApi.Models.Enrollment", b =>
                {
                    b.Property<int>("EnrollmentId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("CourseId");

                    b.Property<DateTime>("EnrollDate");

                    b.Property<decimal>("EnrollmentFee");

                    b.Property<int>("IsDeleted");

                    b.Property<int>("SubscriberId");

                    b.HasKey("EnrollmentId");

                    b.ToTable("Enrollment");
                });

            modelBuilder.Entity("UpDiddyApi.Models.Subscriber", b =>
                {
                    b.Property<int>("SubscriberId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("IsDeleted");

                    b.Property<string>("MsalObjectId")
                        .IsRequired();

                    b.HasKey("SubscriberId");

                    b.ToTable("Subscriber");
                });

            modelBuilder.Entity("UpDiddyApi.Models.Topic", b =>
                {
                    b.Property<int>("TopicId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Code");

                    b.Property<int>("IsDeleted");

                    b.Property<string>("Name");

                    b.HasKey("TopicId");

                    b.ToTable("Topic");
                });

            modelBuilder.Entity("UpDiddyApi.Models.Vendor", b =>
                {
                    b.Property<int>("VendorId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("IsDeleted");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.HasKey("VendorId");

                    b.ToTable("Vendor");
                });
#pragma warning restore 612, 618
        }
    }
}
