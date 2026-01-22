using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace property_lease_saas.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentFeature8 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MaintenancePayments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    MaintenanceRequestId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MaintenanceApplicationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MechanicId = table.Column<string>(type: "TEXT", nullable: false),
                    MechanicName = table.Column<string>(type: "TEXT", nullable: false),
                    LandlordId = table.Column<string>(type: "TEXT", nullable: false),
                    LandlordName = table.Column<string>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PaymentMethod = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TransactionId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    WorkDescription = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenancePayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaintenancePayments_MaintenanceApplications_MaintenanceApplicationId",
                        column: x => x.MaintenanceApplicationId,
                        principalTable: "MaintenanceApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MaintenancePayments_MaintenanceRequests_MaintenanceRequestId",
                        column: x => x.MaintenanceRequestId,
                        principalTable: "MaintenanceRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaymentReminders",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    LeaseId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TenantId = table.Column<string>(type: "TEXT", nullable: false),
                    DueDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsSent = table.Column<bool>(type: "INTEGER", nullable: false),
                    SentAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsPaid = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentReminders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentReminders_Leases_LeaseId",
                        column: x => x.LeaseId,
                        principalTable: "Leases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RentPayments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    LeaseId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TenantId = table.Column<string>(type: "TEXT", nullable: false),
                    TenantName = table.Column<string>(type: "TEXT", nullable: false),
                    LandlordId = table.Column<string>(type: "TEXT", nullable: false),
                    LandlordName = table.Column<string>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DueDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PaymentMethod = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TransactionId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PaymentFor = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    IsLate = table.Column<bool>(type: "INTEGER", nullable: false),
                    LateFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RentPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RentPayments_Leases_LeaseId",
                        column: x => x.LeaseId,
                        principalTable: "Leases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MaintenancePayments_LandlordId",
                table: "MaintenancePayments",
                column: "LandlordId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenancePayments_MaintenanceApplicationId",
                table: "MaintenancePayments",
                column: "MaintenanceApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenancePayments_MaintenanceRequestId",
                table: "MaintenancePayments",
                column: "MaintenanceRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenancePayments_MechanicId",
                table: "MaintenancePayments",
                column: "MechanicId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenancePayments_Status",
                table: "MaintenancePayments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentReminders_LeaseId",
                table: "PaymentReminders",
                column: "LeaseId");

            migrationBuilder.CreateIndex(
                name: "IX_RentPayments_LandlordId",
                table: "RentPayments",
                column: "LandlordId");

            migrationBuilder.CreateIndex(
                name: "IX_RentPayments_LeaseId",
                table: "RentPayments",
                column: "LeaseId");

            migrationBuilder.CreateIndex(
                name: "IX_RentPayments_Status",
                table: "RentPayments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_RentPayments_TenantId",
                table: "RentPayments",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MaintenancePayments");

            migrationBuilder.DropTable(
                name: "PaymentReminders");

            migrationBuilder.DropTable(
                name: "RentPayments");
        }
    }
}
