using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExuberantPathfinders.Web.Migrations
{
    /// <inheritdoc />
    public partial class RenameIdentityTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                SET @has_aspnet_users = (
                    SELECT COUNT(*)
                    FROM INFORMATION_SCHEMA.TABLES
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'AspNetUsers'
                );

                SET @rename_sql = IF(
                    @has_aspnet_users = 1,
                    'RENAME TABLE
                        `AspNetRoleClaims` TO `RoleClaims`,
                        `AspNetRoles` TO `Roles`,
                        `AspNetUserClaims` TO `UserClaims`,
                        `AspNetUserLogins` TO `UserLogins`,
                        `AspNetUserRoles` TO `UserRoles`,
                        `AspNetUsers` TO `Users`,
                        `AspNetUserTokens` TO `UserTokens`',
                    'SELECT 1'
                );

                PREPARE stmt FROM @rename_sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                SET @has_users = (
                    SELECT COUNT(*)
                    FROM INFORMATION_SCHEMA.TABLES
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Users'
                );

                SET @has_aspnet_users = (
                    SELECT COUNT(*)
                    FROM INFORMATION_SCHEMA.TABLES
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'AspNetUsers'
                );

                SET @rename_sql = IF(
                    @has_users = 1 AND @has_aspnet_users = 0,
                    'RENAME TABLE
                        `RoleClaims` TO `AspNetRoleClaims`,
                        `Roles` TO `AspNetRoles`,
                        `UserClaims` TO `AspNetUserClaims`,
                        `UserLogins` TO `AspNetUserLogins`,
                        `UserRoles` TO `AspNetUserRoles`,
                        `Users` TO `AspNetUsers`,
                        `UserTokens` TO `AspNetUserTokens`',
                    'SELECT 1'
                );

                PREPARE stmt FROM @rename_sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");
        }
    }
}
