﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TaxBeacon.DAL;

#nullable disable

namespace TaxBeacon.DAL.Migrations
{
    [DbContext(typeof(TaxBeaconDbContext))]
    partial class TaxBeaconDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("TaxBeacon.DAL.Entities.Department", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedDateTimeUtc")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DeletedDateTimeUtc")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar");

                    b.Property<Guid>("DivisionId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool?>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("LastModifiedDateTimeUtc")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar");

                    b.Property<Guid>("TenantId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("Id"), false);

                    b.HasIndex("DivisionId");

                    b.HasIndex("TenantId", "Name")
                        .IsUnique();

                    b.HasIndex("TenantId", "DivisionId", "Id");

                    SqlServerIndexBuilderExtensions.IsClustered(b.HasIndex("TenantId", "DivisionId", "Id"));

                    b.ToTable("Departments");
                });

            modelBuilder.Entity("TaxBeacon.DAL.Entities.Division", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedDateTimeUtc")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DeletedDateTimeUtc")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar");

                    b.Property<bool?>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("LastModifiedDateTimeUtc")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar");

                    b.Property<Guid>("TenantId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("Id"), false);

                    b.HasIndex("TenantId", "Id");

                    SqlServerIndexBuilderExtensions.IsClustered(b.HasIndex("TenantId", "Id"));

                    b.HasIndex("TenantId", "Name")
                        .IsUnique();

                    b.ToTable("Divisions");
                });

            modelBuilder.Entity("TaxBeacon.DAL.Entities.JobTitle", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedDateTimeUtc")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DeletedDateTimeUtc")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("DepartmentId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool?>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("LastModifiedDateTimeUtc")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar");

                    b.Property<Guid>("TenantId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("Id"), false);

                    b.HasIndex("DepartmentId");

                    b.HasIndex("TenantId", "Name")
                        .IsUnique();

                    b.HasIndex("TenantId", "DepartmentId", "Id");

                    SqlServerIndexBuilderExtensions.IsClustered(b.HasIndex("TenantId", "DepartmentId", "Id"));

                    b.ToTable("JobTitles");
                });

            modelBuilder.Entity("TaxBeacon.DAL.Entities.Permission", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasDefaultValueSql("NEWID()");

                    b.Property<DateTime>("CreatedDateTimeUtc")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("GETUTCDATE()");

                    b.Property<DateTime?>("DeletedDateTimeUtc")
                        .HasColumnType("datetime2");

                    b.Property<bool?>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("LastModifiedDateTimeUtc")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Permissions");
                });

            modelBuilder.Entity("TaxBeacon.DAL.Entities.Role", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasDefaultValueSql("NEWID()");

                    b.Property<DateTime>("CreatedDateTimeUtc")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("GETUTCDATE()");

                    b.Property<DateTime?>("DeletedDateTimeUtc")
                        .HasColumnType("datetime2");

                    b.Property<bool?>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("LastModifiedDateTimeUtc")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar");

                    b.Property<int>("Type")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(2);

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Roles");
                });

            modelBuilder.Entity("TaxBeacon.DAL.Entities.RolePermission", b =>
                {
                    b.Property<Guid>("RoleId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("PermissionId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("RoleId", "PermissionId");

                    b.HasIndex("PermissionId");

                    b.ToTable("RolePermissions");
                });

            modelBuilder.Entity("TaxBeacon.DAL.Entities.ServiceArea", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedDateTimeUtc")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DeletedDateTimeUtc")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("DepartmentId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Description")
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar");

                    b.Property<bool?>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("LastModifiedDateTimeUtc")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar");

                    b.Property<Guid>("TenantId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("Id"), false);

                    b.HasIndex("DepartmentId");

                    b.HasIndex("TenantId", "Name")
                        .IsUnique();

                    b.HasIndex("TenantId", "DepartmentId", "Id");

                    SqlServerIndexBuilderExtensions.IsClustered(b.HasIndex("TenantId", "DepartmentId", "Id"));

                    b.ToTable("ServiceAreas");
                });

            modelBuilder.Entity("TaxBeacon.DAL.Entities.TableFilter", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasDefaultValueSql("NEWID()");

                    b.Property<string>("Configuration")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar");

                    b.Property<int>("TableType")
                        .HasColumnType("int");

                    b.Property<Guid?>("TenantId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("Id"), false);

                    b.HasIndex("UserId");

                    b.HasIndex("TenantId", "TableType", "UserId");

                    SqlServerIndexBuilderExtensions.IsClustered(b.HasIndex("TenantId", "TableType", "UserId"));

                    b.ToTable("TableFilters");
                });

            modelBuilder.Entity("TaxBeacon.DAL.Entities.Team", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedDateTimeUtc")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DeletedDateTimeUtc")
                        .HasColumnType("datetime2");

                    b.Property<bool?>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("LastModifiedDateTimeUtc")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar");

                    b.Property<Guid>("TenantId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("Id"), false);

                    b.HasIndex("TenantId", "Id");

                    SqlServerIndexBuilderExtensions.IsClustered(b.HasIndex("TenantId", "Id"));

                    b.HasIndex("TenantId", "Name")
                        .IsUnique();

                    b.ToTable("Teams");
                });

            modelBuilder.Entity("TaxBeacon.DAL.Entities.Tenant", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasDefaultValueSql("NEWID()");

                    b.Property<DateTime>("CreatedDateTimeUtc")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("GETUTCDATE()");

                    b.Property<DateTime?>("DeletedDateTimeUtc")
                        .HasColumnType("datetime2");

                    b.Property<bool?>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("LastModifiedDateTimeUtc")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Tenants");
                });

            modelBuilder.Entity("TaxBeacon.DAL.Entities.TenantPermission", b =>
                {
                    b.Property<Guid>("TenantId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("PermissionId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("TenantId", "PermissionId");

                    b.HasIndex("PermissionId");

                    b.ToTable("TenantPermissions");
                });

            modelBuilder.Entity("TaxBeacon.DAL.Entities.TenantRole", b =>
                {
                    b.Property<Guid>("TenantId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("RoleId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("TenantId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("TenantRoles");
                });

            modelBuilder.Entity("TaxBeacon.DAL.Entities.TenantRolePermission", b =>
                {
                    b.Property<Guid>("TenantId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("RoleId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("PermissionId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("TenantId", "RoleId", "PermissionId");

                    b.HasIndex("TenantId", "PermissionId");

                    b.ToTable("TenantRolePermissions");
                });

            modelBuilder.Entity("TaxBeacon.DAL.Entities.TenantUser", b =>
                {
                    b.Property<Guid>("TenantId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("TenantId", "UserId");

                    b.HasIndex("UserId");

                    b.ToTable("TenantUsers");
                });

            modelBuilder.Entity("TaxBeacon.DAL.Entities.TenantUserRole", b =>
                {
                    b.Property<Guid>("TenantId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("RoleId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("TenantId", "RoleId", "UserId");

                    b.HasIndex("TenantId", "UserId");

                    b.ToTable("TenantUserRoles");
                });

            modelBuilder.Entity("TaxBeacon.DAL.Entities.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasDefaultValueSql("NEWID()");

                    b.Property<DateTime>("CreatedDateTimeUtc")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("GETUTCDATE()");

                    b.Property<DateTime?>("DeactivationDateTimeUtc")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DeletedDateTimeUtc")
                        .HasColumnType("datetime2");

                    b.Property<Guid?>("DepartmentId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("DivisionId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar");

                    b.Property<string>("FullName")
                        .IsRequired()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasMaxLength(202)
                        .HasColumnType("nvarchar")
                        .HasComputedColumnSql("TRIM(CONCAT([FirstName], ' ', [LastName]))", true);

                    b.Property<bool?>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<Guid?>("JobTitleId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime?>("LastLoginDateTimeUtc")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("LastModifiedDateTimeUtc")
                        .HasColumnType("datetime2");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar");

                    b.Property<string>("LegalName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar");

                    b.Property<DateTime?>("ReactivationDateTimeUtc")
                        .HasColumnType("datetime2");

                    b.Property<Guid?>("ServiceAreaId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar");

                    b.Property<Guid?>("TeamId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("DepartmentId");

                    b.HasIndex("DivisionId");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.HasIndex("JobTitleId");

                    b.HasIndex("ServiceAreaId");

                    b.HasIndex("TeamId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("TaxBeacon.DAL.Entities.UserActivityLog", b =>
                {
                    b.Property<Guid>("TenantId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<string>("Event")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("EventType")
                        .HasColumnType("int");

                    b.Property<long>("Revision")
                        .HasColumnType("bigint");

                    b.HasKey("TenantId", "UserId", "Date");

                    b.HasIndex("UserId");

                    b.ToTable("UserActivityLogs");
                });

            modelBuilder.Entity("TaxBeacon.DAL.Entities.UserRole", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("RoleId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("UserRoles");
                });

            modelBuilder.Entity("TaxBeacon.DAL.Entities.Department", b =>
                {
                    b.HasOne("TaxBeacon.DAL.Entities.Division", "Division")
                        .WithMany("Departments")
                        .HasForeignKey("DivisionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TaxBeacon.DAL.Entities.Tenant", "Tenant")
                        .WithMany("Departments")
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("Division");

                    b.Navigation("Tenant");
                });

            modelBuilder.Entity("TaxBeacon.DAL.Entities.Division", b =>
                {
                    b.HasOne("TaxBeacon.DAL.Entities.Tenant", "Tenant")
                        .WithMany("Divisions")
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Tenant");
                });

            modelBuilder.Entity("TaxBeacon.DAL.Entities.JobTitle", b =>
                {
                    b.HasOne("TaxBeacon.DAL.Entities.Department", "Department")
                        .WithMany("JobTitles")
                        .HasForeignKey("DepartmentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Department");
                });

            modelBuilder.Entity("TaxBeacon.DAL.Entities.RolePermission", b =>
                {
                    b.HasOne("TaxBeacon.DAL.Entities.Permission", "Permission")
                        .WithMany("RolePermissions")
                        .HasForeignKey("PermissionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TaxBeacon.DAL.Entities.Role", "Role")
                        .WithMany("RolePermissions")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Permission");

                    b.Navigation("Role");
                });

            modelBuilder.Entity("TaxBeacon.DAL.Entities.ServiceArea", b =>
                {
                    b.HasOne("TaxBeacon.DAL.Entities.Department", "Department")
                        .WithMany("ServiceAreas")
                        .HasForeignKey("DepartmentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TaxBeacon.DAL.Entities.Tenant", "Tenant")
                        .WithMany("ServiceAreas")
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("Department");

                    b.Navigation("Tenant");
                });

            modelBuilder.Entity("TaxBeacon.DAL.Entities.TableFilter", b =>
                {
                    b.HasOne("TaxBeacon.DAL.Entities.Tenant", "Tenant")
                        .WithMany("TableFilters")
                        .HasForeignKey("TenantId");

                    b.HasOne("TaxBeacon.DAL.Entities.User", "User")
                        .WithMany("TableFilters")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Tenant");

                    b.Navigation("User");
                });

            modelBuilder.Entity("TaxBeacon.DAL.Entities.Team", b =>
                {
                    b.HasOne("TaxBeacon.DAL.Entities.Tenant", "Tenant")
                        .WithMany("Teams")
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Tenant");
                });

            modelBuilder.Entity("TaxBeacon.DAL.Entities.TenantPermission", b =>
                {
                    b.HasOne("TaxBeacon.DAL.Entities.Permission", "Permission")
                        .WithMany("TenantPermissions")
                        .HasForeignKey("PermissionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TaxBeacon.DAL.Entities.Tenant", "Tenant")
                        .WithMany("TenantPermissions")
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Permission");

                    b.Navigation("Tenant");
                });

            modelBuilder.Entity("TaxBeacon.DAL.Entities.TenantRole", b =>
                {
                    b.HasOne("TaxBeacon.DAL.Entities.Role", "Role")
                        .WithMany("TenantRoles")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TaxBeacon.DAL.Entities.Tenant", "Tenant")
                        .WithMany("TenantRoles")
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Role");

                    b.Navigation("Tenant");
                });

            modelBuilder.Entity("TaxBeacon.DAL.Entities.TenantRolePermission", b =>
                {
                    b.HasOne("TaxBeacon.DAL.Entities.TenantPermission", "TenantPermission")
                        .WithMany("TenantRolePermissions")
                        .HasForeignKey("TenantId", "PermissionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TaxBeacon.DAL.Entities.TenantRole", "TenantRole")
                        .WithMany("TenantRolePermissions")
                        .HasForeignKey("TenantId", "RoleId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("TenantPermission");

                    b.Navigation("TenantRole");
                });

            modelBuilder.Entity("TaxBeacon.DAL.Entities.TenantUser", b =>
                {
                    b.HasOne("TaxBeacon.DAL.Entities.Tenant", "Tenant")
                        .WithMany("TenantUsers")
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TaxBeacon.DAL.Entities.User", "User")
                        .WithMany("TenantUsers")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Tenant");

                    b.Navigation("User");
                });

            modelBuilder.Entity("TaxBeacon.DAL.Entities.TenantUserRole", b =>
                {
                    b.HasOne("TaxBeacon.DAL.Entities.TenantRole", "TenantRole")
                        .WithMany("TenantUserRoles")
                        .HasForeignKey("TenantId", "RoleId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("TaxBeacon.DAL.Entities.TenantUser", "TenantUser")
                        .WithMany("TenantUserRoles")
                        .HasForeignKey("TenantId", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("TenantRole");

                    b.Navigation("TenantUser");
                });

            modelBuilder.Entity("TaxBeacon.DAL.Entities.User", b =>
                {
                    b.HasOne("TaxBeacon.DAL.Entities.Department", "Department")
                        .WithMany("Users")
                        .HasForeignKey("DepartmentId");

                    b.HasOne("TaxBeacon.DAL.Entities.Division", "Division")
                        .WithMany("Users")
                        .HasForeignKey("DivisionId");

                    b.HasOne("TaxBeacon.DAL.Entities.JobTitle", "JobTitle")
                        .WithMany("Users")
                        .HasForeignKey("JobTitleId");

                    b.HasOne("TaxBeacon.DAL.Entities.ServiceArea", "ServiceArea")
                        .WithMany("Users")
                        .HasForeignKey("ServiceAreaId");

                    b.HasOne("TaxBeacon.DAL.Entities.Team", "Team")
                        .WithMany("Users")
                        .HasForeignKey("TeamId");

                    b.Navigation("Department");

                    b.Navigation("Division");

                    b.Navigation("JobTitle");

                    b.Navigation("ServiceArea");

                    b.Navigation("Team");
                });

            modelBuilder.Entity("TaxBeacon.DAL.Entities.UserActivityLog", b =>
                {
                    b.HasOne("TaxBeacon.DAL.Entities.User", "User")
                        .WithMany("UserActivityLogs")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("TaxBeacon.DAL.Entities.UserRole", b =>
                {
                    b.HasOne("TaxBeacon.DAL.Entities.Role", "Role")
                        .WithMany("UserRoles")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TaxBeacon.DAL.Entities.User", "User")
                        .WithMany("UserRoles")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Role");

                    b.Navigation("User");
                });

            modelBuilder.Entity("TaxBeacon.DAL.Entities.Department", b =>
                {
                    b.Navigation("JobTitles");

                    b.Navigation("ServiceAreas");

                    b.Navigation("Users");
                });

            modelBuilder.Entity("TaxBeacon.DAL.Entities.Division", b =>
                {
                    b.Navigation("Departments");

                    b.Navigation("Users");
                });

            modelBuilder.Entity("TaxBeacon.DAL.Entities.JobTitle", b =>
                {
                    b.Navigation("Users");
                });

            modelBuilder.Entity("TaxBeacon.DAL.Entities.Permission", b =>
                {
                    b.Navigation("RolePermissions");

                    b.Navigation("TenantPermissions");
                });

            modelBuilder.Entity("TaxBeacon.DAL.Entities.Role", b =>
                {
                    b.Navigation("RolePermissions");

                    b.Navigation("TenantRoles");

                    b.Navigation("UserRoles");
                });

            modelBuilder.Entity("TaxBeacon.DAL.Entities.ServiceArea", b =>
                {
                    b.Navigation("Users");
                });

            modelBuilder.Entity("TaxBeacon.DAL.Entities.Team", b =>
                {
                    b.Navigation("Users");
                });

            modelBuilder.Entity("TaxBeacon.DAL.Entities.Tenant", b =>
                {
                    b.Navigation("Departments");

                    b.Navigation("Divisions");

                    b.Navigation("ServiceAreas");

                    b.Navigation("TableFilters");

                    b.Navigation("Teams");

                    b.Navigation("TenantPermissions");

                    b.Navigation("TenantRoles");

                    b.Navigation("TenantUsers");
                });

            modelBuilder.Entity("TaxBeacon.DAL.Entities.TenantPermission", b =>
                {
                    b.Navigation("TenantRolePermissions");
                });

            modelBuilder.Entity("TaxBeacon.DAL.Entities.TenantRole", b =>
                {
                    b.Navigation("TenantRolePermissions");

                    b.Navigation("TenantUserRoles");
                });

            modelBuilder.Entity("TaxBeacon.DAL.Entities.TenantUser", b =>
                {
                    b.Navigation("TenantUserRoles");
                });

            modelBuilder.Entity("TaxBeacon.DAL.Entities.User", b =>
                {
                    b.Navigation("TableFilters");

                    b.Navigation("TenantUsers");

                    b.Navigation("UserActivityLogs");

                    b.Navigation("UserRoles");
                });
#pragma warning restore 612, 618
        }
    }
}
