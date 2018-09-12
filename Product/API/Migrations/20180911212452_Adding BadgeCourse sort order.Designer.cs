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
    [Migration("20180911212452_Adding BadgeCourse sort order")]
    partial class AddingBadgeCoursesortorder
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.2-rtm-30932")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("UpDiddyApi.Models.Badge", b =>
                {
                    b.Property<int>("BadgeID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<Guid>("BadgeGuid");

                    b.Property<string>("BadgeName")
                        .IsRequired();

                    b.Property<DateTime>("CreateDate");

                    b.Property<Guid>("CreateGuid");

                    b.Property<string>("Description");

                    b.Property<DateTime>("EndDate");

                    b.Property<bool>("Hidden");

                    b.Property<string>("Icon");

                    b.Property<int>("IsDeleted");

                    b.Property<DateTime>("ModifyDate");

                    b.Property<Guid>("ModifyGuid");

                    b.Property<int>("Points");

                    b.Property<string>("Slug");

                    b.Property<string>("SortOrder");

                    b.Property<DateTime>("StartDate");

                    b.HasKey("BadgeID");

                    b.ToTable("Badge");
                });

            modelBuilder.Entity("UpDiddyApi.Models.BadgeCourse", b =>
                {
                    b.Property<string>("BadgeCourseID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("BadgeCourseGuid");

                    b.Property<string>("BadgeID")
                        .IsRequired();

                    b.Property<DateTime>("CreateDate");

                    b.Property<Guid>("CreateGuid");

                    b.Property<int>("IsDeleted");

                    b.Property<DateTime>("ModifyDate");

                    b.Property<Guid>("ModifyGuid");

                    b.Property<string>("Notes");

                    b.Property<int>("SortOrder");

                    b.HasKey("BadgeCourseID");

                    b.ToTable("BadgeCourse");
                });

            modelBuilder.Entity("UpDiddyApi.Models.Course", b =>
                {
                    b.Property<int>("CourseId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("CourseCode")
                        .IsRequired();

                    b.Property<Guid>("CourseGuid");

                    b.Property<string>("CourseName");

                    b.Property<int>("CourseSchedule");

                    b.Property<int>("IsDeleted");

                    b.Property<decimal>("PurchasePrice");

                    b.Property<string>("Tags");

                    b.Property<int>("VendorId");

                    b.HasKey("CourseId");

                    b.ToTable("Course");
                });

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
