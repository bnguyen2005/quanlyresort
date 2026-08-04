using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyResort.Migrations
{
    /// <inheritdoc />
    public partial class AddTwoFactorAuthFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_RestaurantOrder_PaymentStatus",
                table: "RestaurantOrders");

            migrationBuilder.AddColumn<bool>(
                name: "TwoFactorEnabled",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "TwoFactorEnabledAt",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TwoFactorRecoveryCodes",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TwoFactorSecret",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_RestaurantOrder_PaymentStatus",
                table: "RestaurantOrders",
                sql: "PaymentStatus IN ('Unpaid', 'Paid', 'Refunded', 'AwaitingConfirmation')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_RestaurantOrder_PaymentStatus",
                table: "RestaurantOrders");

            migrationBuilder.DropColumn(
                name: "TwoFactorEnabled",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TwoFactorEnabledAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TwoFactorRecoveryCodes",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TwoFactorSecret",
                table: "Users");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RestaurantOrder_PaymentStatus",
                table: "RestaurantOrders",
                sql: "PaymentStatus IN ('Unpaid', 'Paid', 'Refunded')");
        }
    }
}
