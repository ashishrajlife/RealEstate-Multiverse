using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Multiverse.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddClientProfilePreferencesAndLead : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AssignedEmployeeId",
                table: "Clients",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BHK",
                table: "Clients",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ClientType",
                table: "Clients",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "InitialNotes",
                table: "Clients",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InterestedIn",
                table: "Clients",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LeadSource",
                table: "Clients",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LeadStatus",
                table: "Clients",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LoanRequired",
                table: "Clients",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxBudget",
                table: "Clients",
                type: "decimal(14,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MinBudget",
                table: "Clients",
                type: "decimal(14,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextFollowUpAt",
                table: "Clients",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Occupation",
                table: "Clients",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Possession",
                table: "Clients",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreferredArea",
                table: "Clients",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreferredLocation",
                table: "Clients",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PreferredPropertyType",
                table: "Clients",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "Clients",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Clients_AssignedEmployeeId",
                table: "Clients",
                column: "AssignedEmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Clients_Users_AssignedEmployeeId",
                table: "Clients",
                column: "AssignedEmployeeId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clients_Users_AssignedEmployeeId",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Clients_AssignedEmployeeId",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "AssignedEmployeeId",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "BHK",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "ClientType",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "InitialNotes",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "InterestedIn",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "LeadSource",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "LeadStatus",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "LoanRequired",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "MaxBudget",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "MinBudget",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "NextFollowUpAt",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "Occupation",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "Possession",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "PreferredArea",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "PreferredLocation",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "PreferredPropertyType",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "Clients");
        }
    }
}
