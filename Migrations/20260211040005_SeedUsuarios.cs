using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace Inventarios.Migrations
{
    /// <inheritdoc />
    public partial class SeedUsuarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // IDs fijos para permitir revertir
            var rolAdminId = "3e1c50a1-8cbe-4c0f-9f2c-a1234567890a";
            var rolUsuarioId = "ae8c60f5-9b6a-4c07-8d90-b2345678901b";

            var adminId = "1d3c6b1a-11e0-4ae2-81c1-c3456789012c";
            var user1Id = "2a2b7c2b-22f1-4b2f-9b12-d4567890123d";
            var user2Id = "3b3c8d3c-33f2-4c3f-9c23-e5678901234e";
            var user3Id = "4c4d9e4d-44f3-4d4f-9d34-f6789012345f";
            var user4Id = "5d5eaf5e-55f4-4e5f-9e45-a78901234560";

            // Crear roles
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "Name", "NormalizedName", "ConcurrencyStamp" },
                values: new object[,]
                {
                    { rolAdminId, "Admin", "ADMIN", Guid.NewGuid().ToString() },
                    { rolUsuarioId, "Usuario", "USUARIO", Guid.NewGuid().ToString() }
                });

            var hasher = new PasswordHasher<IdentityUser>();

            IdentityUser CrearUsuario(string id, string correo, string password)
            {
                var user = new IdentityUser
                {
                    Id = id,
                    UserName = correo,
                    NormalizedUserName = correo.ToUpperInvariant(),
                    Email = correo,
                    NormalizedEmail = correo.ToUpperInvariant(),
                    EmailConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                };
                user.PasswordHash = hasher.HashPassword(user, password);
                return user;
            }

            var admin = CrearUsuario(adminId, "admin@demo.com", "Admin123!");
            var u1 = CrearUsuario(user1Id, "usuario1@demo.com", "Usuario123!");
            var u2 = CrearUsuario(user2Id, "usuario2@demo.com", "Usuario123!");
            var u3 = CrearUsuario(user3Id, "usuario3@demo.com", "Usuario123!");
            var u4 = CrearUsuario(user4Id, "usuario4@demo.com", "Usuario123!");

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[]
                {
                    "Id","UserName","NormalizedUserName","Email","NormalizedEmail","EmailConfirmed",
                    "PasswordHash","SecurityStamp","ConcurrencyStamp","PhoneNumber","PhoneNumberConfirmed",
                    "TwoFactorEnabled","LockoutEnd","LockoutEnabled","AccessFailedCount"
                },
                values: new object[,]
                {
                    { admin.Id, admin.UserName, admin.NormalizedUserName, admin.Email, admin.NormalizedEmail, admin.EmailConfirmed, admin.PasswordHash, admin.SecurityStamp, admin.ConcurrencyStamp, null, false, false, null, false, 0 },
                    { u1.Id, u1.UserName, u1.NormalizedUserName, u1.Email, u1.NormalizedEmail, u1.EmailConfirmed, u1.PasswordHash, u1.SecurityStamp, u1.ConcurrencyStamp, null, false, false, null, false, 0 },
                    { u2.Id, u2.UserName, u2.NormalizedUserName, u2.Email, u2.NormalizedEmail, u2.EmailConfirmed, u2.PasswordHash, u2.SecurityStamp, u2.ConcurrencyStamp, null, false, false, null, false, 0 },
                    { u3.Id, u3.UserName, u3.NormalizedUserName, u3.Email, u3.NormalizedEmail, u3.EmailConfirmed, u3.PasswordHash, u3.SecurityStamp, u3.ConcurrencyStamp, null, false, false, null, false, 0 },
                    { u4.Id, u4.UserName, u4.NormalizedUserName, u4.Email, u4.NormalizedEmail, u4.EmailConfirmed, u4.PasswordHash, u4.SecurityStamp, u4.ConcurrencyStamp, null, false, false, null, false, 0 },
                });

            // Asignar roles
            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "UserId", "RoleId" },
                values: new object[,]
                {
                    { adminId, rolAdminId },
                    { adminId, rolUsuarioId }, // Admin también como usuario
                    { user1Id, rolUsuarioId },
                    { user2Id, rolUsuarioId },
                    { user3Id, rolUsuarioId },
                    { user4Id, rolUsuarioId }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var rolAdminId = "3e1c50a1-8cbe-4c0f-9f2c-a1234567890a";
            var rolUsuarioId = "ae8c60f5-9b6a-4c07-8d90-b2345678901b";

            var adminId = "1d3c6b1a-11e0-4ae2-81c1-c3456789012c";
            var user1Id = "2a2b7c2b-22f1-4b2f-9b12-d4567890123d";
            var user2Id = "3b3c8d3c-33f2-4c3f-9c23-e5678901234e";
            var user3Id = "4c4d9e4d-44f3-4d4f-9d34-f6789012345f";
            var user4Id = "5d5eaf5e-55f4-4e5f-9e45-a78901234560";

            migrationBuilder.DeleteData("AspNetUserRoles", new[] { "UserId", "RoleId" }, new object[] { adminId, rolAdminId });
            migrationBuilder.DeleteData("AspNetUserRoles", new[] { "UserId", "RoleId" }, new object[] { adminId, rolUsuarioId });
            migrationBuilder.DeleteData("AspNetUserRoles", new[] { "UserId", "RoleId" }, new object[] { user1Id, rolUsuarioId });
            migrationBuilder.DeleteData("AspNetUserRoles", new[] { "UserId", "RoleId" }, new object[] { user2Id, rolUsuarioId });
            migrationBuilder.DeleteData("AspNetUserRoles", new[] { "UserId", "RoleId" }, new object[] { user3Id, rolUsuarioId });
            migrationBuilder.DeleteData("AspNetUserRoles", new[] { "UserId", "RoleId" }, new object[] { user4Id, rolUsuarioId });

            migrationBuilder.DeleteData("AspNetUsers", "Id", adminId);
            migrationBuilder.DeleteData("AspNetUsers", "Id", user1Id);
            migrationBuilder.DeleteData("AspNetUsers", "Id", user2Id);
            migrationBuilder.DeleteData("AspNetUsers", "Id", user3Id);
            migrationBuilder.DeleteData("AspNetUsers", "Id", user4Id);

            migrationBuilder.DeleteData("AspNetRoles", "Id", rolAdminId);
            migrationBuilder.DeleteData("AspNetRoles", "Id", rolUsuarioId);
        }
    }
}
