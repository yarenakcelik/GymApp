using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymApp.Migrations
{
    /// <inheritdoc />
    public partial class AddDomainModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MemberProfiles_AspNetUsers_UserId",
                table: "MemberProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_TrainerSpecializations_Trainers_TrainerId",
                table: "TrainerSpecializations");

            migrationBuilder.DropIndex(
                name: "IX_TrainerSpecializations_TrainerId",
                table: "TrainerSpecializations");

            migrationBuilder.DropColumn(
                name: "TrainerId",
                table: "TrainerSpecializations");

            migrationBuilder.DropColumn(
                name: "Rating",
                table: "Trainers");

            migrationBuilder.DropColumn(
                name: "ReviewCount",
                table: "Trainers");

            migrationBuilder.DropColumn(
                name: "Goal",
                table: "MemberProfiles");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Trainers",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "MemberProfiles",
                newName: "ApplicationUserId");

            migrationBuilder.RenameIndex(
                name: "IX_MemberProfiles_UserId",
                table: "MemberProfiles",
                newName: "IX_MemberProfiles_ApplicationUserId");

            migrationBuilder.RenameColumn(
                name: "EndTime",
                table: "Appointments",
                newName: "CreatedAt");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "TrainerSpecializations",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "AvailableFrom",
                table: "Trainers",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "AvailableTo",
                table: "Trainers",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Bio",
                table: "Trainers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Trainers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TrainerSpecializationId",
                table: "Trainers",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "Services",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Services",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Services",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "HeightCm",
                table: "MemberProfiles",
                type: "float",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "MemberProfiles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "MemberProfiles",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "FitnessGoal",
                table: "MemberProfiles",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "MemberProfiles",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetUserTokens",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserTokens",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "ProviderKey",
                table: "AspNetUserLogins",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserLogins",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Appointments",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "Appointments",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)");

            migrationBuilder.AddColumn<int>(
                name: "DurationMinutes",
                table: "Appointments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Trainers_TrainerSpecializationId",
                table: "Trainers",
                column: "TrainerSpecializationId");

            migrationBuilder.AddForeignKey(
                name: "FK_MemberProfiles_AspNetUsers_ApplicationUserId",
                table: "MemberProfiles",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Trainers_TrainerSpecializations_TrainerSpecializationId",
                table: "Trainers",
                column: "TrainerSpecializationId",
                principalTable: "TrainerSpecializations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MemberProfiles_AspNetUsers_ApplicationUserId",
                table: "MemberProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_Trainers_TrainerSpecializations_TrainerSpecializationId",
                table: "Trainers");

            migrationBuilder.DropIndex(
                name: "IX_Trainers_TrainerSpecializationId",
                table: "Trainers");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "TrainerSpecializations");

            migrationBuilder.DropColumn(
                name: "AvailableFrom",
                table: "Trainers");

            migrationBuilder.DropColumn(
                name: "AvailableTo",
                table: "Trainers");

            migrationBuilder.DropColumn(
                name: "Bio",
                table: "Trainers");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Trainers");

            migrationBuilder.DropColumn(
                name: "TrainerSpecializationId",
                table: "Trainers");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "MemberProfiles");

            migrationBuilder.DropColumn(
                name: "FitnessGoal",
                table: "MemberProfiles");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "MemberProfiles");

            migrationBuilder.DropColumn(
                name: "DurationMinutes",
                table: "Appointments");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Trainers",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "ApplicationUserId",
                table: "MemberProfiles",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_MemberProfiles_ApplicationUserId",
                table: "MemberProfiles",
                newName: "IX_MemberProfiles_UserId");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Appointments",
                newName: "EndTime");

            migrationBuilder.AddColumn<int>(
                name: "TrainerId",
                table: "TrainerSpecializations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "Rating",
                table: "Trainers",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReviewCount",
                table: "Trainers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "Services",
                type: "decimal(10,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<int>(
                name: "HeightCm",
                table: "MemberProfiles",
                type: "int",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "MemberProfiles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Goal",
                table: "MemberProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetUserTokens",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserTokens",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "ProviderKey",
                table: "AspNetUserLogins",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserLogins",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Appointments",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "Appointments",
                type: "decimal(10,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.CreateIndex(
                name: "IX_TrainerSpecializations_TrainerId",
                table: "TrainerSpecializations",
                column: "TrainerId");

            migrationBuilder.AddForeignKey(
                name: "FK_MemberProfiles_AspNetUsers_UserId",
                table: "MemberProfiles",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TrainerSpecializations_Trainers_TrainerId",
                table: "TrainerSpecializations",
                column: "TrainerId",
                principalTable: "Trainers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
