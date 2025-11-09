using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyResort.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerSupportTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FAQs",
                columns: table => new
                {
                    FAQId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Question = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Answer = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    ViewCount = table.Column<int>(type: "INTEGER", nullable: false),
                    HelpfulCount = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FAQs", x => x.FAQId);
                });

            migrationBuilder.CreateTable(
                name: "SupportTickets",
                columns: table => new
                {
                    TicketId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TicketNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CustomerId = table.Column<int>(type: "INTEGER", nullable: true),
                    Subject = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Priority = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    AssignedTo = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ContactName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ContactEmail = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    ContactPhone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ClosedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ResolutionNotes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportTickets", x => x.TicketId);
                    table.ForeignKey(
                        name: "FK_SupportTickets_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "TicketMessages",
                columns: table => new
                {
                    MessageId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TicketId = table.Column<int>(type: "INTEGER", nullable: false),
                    Content = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    SenderType = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    SenderName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    SenderEmail = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    IsInternal = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsRead = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AttachmentUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    AttachmentName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketMessages", x => x.MessageId);
                    table.ForeignKey(
                        name: "FK_TicketMessages_SupportTickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "SupportTickets",
                        principalColumn: "TicketId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FAQs_Category_IsActive_DisplayOrder",
                table: "FAQs",
                columns: new[] { "Category", "IsActive", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_FAQs_IsActive",
                table: "FAQs",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_ContactEmail",
                table: "SupportTickets",
                column: "ContactEmail");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_CreatedAt",
                table: "SupportTickets",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_CustomerId_CreatedAt",
                table: "SupportTickets",
                columns: new[] { "CustomerId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_Status_Priority",
                table: "SupportTickets",
                columns: new[] { "Status", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_TicketNumber",
                table: "SupportTickets",
                column: "TicketNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TicketMessages_CreatedAt",
                table: "TicketMessages",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_TicketMessages_TicketId_CreatedAt",
                table: "TicketMessages",
                columns: new[] { "TicketId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FAQs");

            migrationBuilder.DropTable(
                name: "TicketMessages");

            migrationBuilder.DropTable(
                name: "SupportTickets");
        }
    }
}
