﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternTracking.Migrations
{
    /// <inheritdoc />
    public partial class AddDepartmentToIntern : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "Interns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "JoinDate",
                table: "Interns",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Department",
                table: "Interns");

            migrationBuilder.DropColumn(
                name: "JoinDate",
                table: "Interns");
        }
    }
}
