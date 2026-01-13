using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace property_lease_saas.Migrations
{
    /// <inheritdoc />
    public partial class FixLeaseRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LeaseRequests_Properties_PropertyId",
                table: "LeaseRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_Leases_Properties_PropertyId",
                table: "Leases");

            migrationBuilder.AddColumn<Guid>(
                name: "LeaseRequestId",
                table: "Leases",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LeaseId",
                table: "LeaseRequests",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Properties_IsPublished",
                table: "Properties",
                column: "IsPublished");

            migrationBuilder.CreateIndex(
                name: "IX_Properties_IsTaken",
                table: "Properties",
                column: "IsTaken");

            migrationBuilder.CreateIndex(
                name: "IX_Properties_LandlordId",
                table: "Properties",
                column: "LandlordId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRequests_Status",
                table: "MaintenanceRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Leases_EndDate",
                table: "Leases",
                column: "EndDate");

            migrationBuilder.CreateIndex(
                name: "IX_Leases_LandlordId",
                table: "Leases",
                column: "LandlordId");

            migrationBuilder.CreateIndex(
                name: "IX_Leases_StartDate",
                table: "Leases",
                column: "StartDate");

            migrationBuilder.CreateIndex(
                name: "IX_Leases_Status",
                table: "Leases",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Leases_TenantId",
                table: "Leases",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaseRequests_LandlordId",
                table: "LeaseRequests",
                column: "LandlordId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaseRequests_LeaseId",
                table: "LeaseRequests",
                column: "LeaseId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeaseRequests_RequestedAt",
                table: "LeaseRequests",
                column: "RequestedAt");

            migrationBuilder.CreateIndex(
                name: "IX_LeaseRequests_Status",
                table: "LeaseRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_LeaseRequests_TenantId",
                table: "LeaseRequests",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_LeaseRequests_Leases_LeaseId",
                table: "LeaseRequests",
                column: "LeaseId",
                principalTable: "Leases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LeaseRequests_Properties_PropertyId",
                table: "LeaseRequests",
                column: "PropertyId",
                principalTable: "Properties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Leases_Properties_PropertyId",
                table: "Leases",
                column: "PropertyId",
                principalTable: "Properties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LeaseRequests_Leases_LeaseId",
                table: "LeaseRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_LeaseRequests_Properties_PropertyId",
                table: "LeaseRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_Leases_Properties_PropertyId",
                table: "Leases");

            migrationBuilder.DropIndex(
                name: "IX_Properties_IsPublished",
                table: "Properties");

            migrationBuilder.DropIndex(
                name: "IX_Properties_IsTaken",
                table: "Properties");

            migrationBuilder.DropIndex(
                name: "IX_Properties_LandlordId",
                table: "Properties");

            migrationBuilder.DropIndex(
                name: "IX_MaintenanceRequests_Status",
                table: "MaintenanceRequests");

            migrationBuilder.DropIndex(
                name: "IX_Leases_EndDate",
                table: "Leases");

            migrationBuilder.DropIndex(
                name: "IX_Leases_LandlordId",
                table: "Leases");

            migrationBuilder.DropIndex(
                name: "IX_Leases_StartDate",
                table: "Leases");

            migrationBuilder.DropIndex(
                name: "IX_Leases_Status",
                table: "Leases");

            migrationBuilder.DropIndex(
                name: "IX_Leases_TenantId",
                table: "Leases");

            migrationBuilder.DropIndex(
                name: "IX_LeaseRequests_LandlordId",
                table: "LeaseRequests");

            migrationBuilder.DropIndex(
                name: "IX_LeaseRequests_LeaseId",
                table: "LeaseRequests");

            migrationBuilder.DropIndex(
                name: "IX_LeaseRequests_RequestedAt",
                table: "LeaseRequests");

            migrationBuilder.DropIndex(
                name: "IX_LeaseRequests_Status",
                table: "LeaseRequests");

            migrationBuilder.DropIndex(
                name: "IX_LeaseRequests_TenantId",
                table: "LeaseRequests");

            migrationBuilder.DropColumn(
                name: "LeaseRequestId",
                table: "Leases");

            migrationBuilder.DropColumn(
                name: "LeaseId",
                table: "LeaseRequests");

            migrationBuilder.AddForeignKey(
                name: "FK_LeaseRequests_Properties_PropertyId",
                table: "LeaseRequests",
                column: "PropertyId",
                principalTable: "Properties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Leases_Properties_PropertyId",
                table: "Leases",
                column: "PropertyId",
                principalTable: "Properties",
                principalColumn: "Id");
        }
    }
}
