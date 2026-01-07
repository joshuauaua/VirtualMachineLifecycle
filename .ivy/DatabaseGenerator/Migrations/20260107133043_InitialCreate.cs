using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace VirtualMachineLifecycle.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_actions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    DescriptionText = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_actions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "providers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    DescriptionText = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_providers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "user_types",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    DescriptionText = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_types", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "vm_statuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    DescriptionText = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vm_statuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    UserTypeId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_users_user_types_UserTypeId",
                        column: x => x.UserTypeId,
                        principalTable: "user_types",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "vms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    VmStatusId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProviderId = table.Column<int>(type: "INTEGER", nullable: false),
                    Tags = table.Column<string>(type: "TEXT", nullable: true),
                    LastAction = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_vms_providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_vms_vm_statuses_VmStatusId",
                        column: x => x.VmStatusId,
                        principalTable: "vm_statuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    VmId = table.Column<int>(type: "INTEGER", nullable: true),
                    ActionId = table.Column<int>(type: "INTEGER", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Details = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_audit_logs_audit_actions_ActionId",
                        column: x => x.ActionId,
                        principalTable: "audit_actions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_audit_logs_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_audit_logs_vms_VmId",
                        column: x => x.VmId,
                        principalTable: "vms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "snapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VmId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedBy = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_snapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_snapshots_users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_snapshots_vms_VmId",
                        column: x => x.VmId,
                        principalTable: "vms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "audit_actions",
                columns: new[] { "Id", "DescriptionText" },
                values: new object[,]
                {
                    { 1, "Login" },
                    { 2, "Logout" },
                    { 3, "Snapshot Created" },
                    { 4, "Snapshot Deleted" },
                    { 5, "Snapshot Restored" },
                    { 6, "VM Start" },
                    { 7, "VM Stop" },
                    { 8, "Reboot" },
                    { 9, "Delete" },
                    { 10, "View" }
                });

            migrationBuilder.InsertData(
                table: "providers",
                columns: new[] { "Id", "DescriptionText" },
                values: new object[,]
                {
                    { 1, "AWS" },
                    { 2, "Azure" }
                });

            migrationBuilder.InsertData(
                table: "user_types",
                columns: new[] { "Id", "DescriptionText" },
                values: new object[,]
                {
                    { 1, "Viewer" },
                    { 2, "Operator" }
                });

            migrationBuilder.InsertData(
                table: "vm_statuses",
                columns: new[] { "Id", "DescriptionText" },
                values: new object[,]
                {
                    { 1, "Running" },
                    { 2, "Stopped" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_ActionId",
                table: "audit_logs",
                column: "ActionId");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_UserId",
                table: "audit_logs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_VmId",
                table: "audit_logs",
                column: "VmId");

            migrationBuilder.CreateIndex(
                name: "IX_snapshots_CreatedBy",
                table: "snapshots",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_snapshots_VmId",
                table: "snapshots",
                column: "VmId");

            migrationBuilder.CreateIndex(
                name: "IX_users_UserTypeId",
                table: "users",
                column: "UserTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_vms_ProviderId",
                table: "vms",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_vms_VmStatusId",
                table: "vms",
                column: "VmStatusId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropTable(
                name: "snapshots");

            migrationBuilder.DropTable(
                name: "audit_actions");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "vms");

            migrationBuilder.DropTable(
                name: "user_types");

            migrationBuilder.DropTable(
                name: "providers");

            migrationBuilder.DropTable(
                name: "vm_statuses");
        }
    }
}
