using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace DatabaseSchemaExporter;

public class Program
{
    private const string MarkdownOutputFileName = "DATABASE_SCHEMA.md";
    private const string MermaidOutputFileName = "DATABASE_SCHEMA_ERD.mmd";

    public static async Task Main(string[] args)
    {
        Console.WriteLine("================================================================");
        Console.WriteLine("              MySQL Database Schema Exporter                    ");
        Console.WriteLine("================================================================");

        try
        {
            // Read connection string from environment variable, with a fallback for local development.
            var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = "Server=localhost;Database=exuberant_db;Uid=root;Pwd=password;Convert Zero Datetime=True;";
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[INFO] DB_CONNECTION_STRING not set. Using default for local development.");
                Console.ResetColor();
            }

            var exporter = new SchemaExporter(connectionString);

            Console.WriteLine($"Connecting to database...");
            var export = await exporter.GenerateExportAsync();

            string cwd = Directory.GetCurrentDirectory();
            string markdownPath = Path.Combine(cwd, MarkdownOutputFileName);
            string mermaidPath = Path.Combine(cwd, MermaidOutputFileName);

            await Task.WhenAll(
                File.WriteAllTextAsync(markdownPath, export.Markdown, Encoding.UTF8),
                File.WriteAllTextAsync(mermaidPath, export.StandaloneMermaid, Encoding.UTF8)
            );

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n[SUCCESS] Schema exported successfully.");
            Console.ResetColor();
            Console.WriteLine($"Markdown: {markdownPath}");
            Console.WriteLine($"ER Diagram (Mermaid): {mermaidPath}");
        }
        catch (MySqlException sqlEx)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n[SQL ERROR] {sqlEx.Message}");
            Console.WriteLine($"Error Code: {sqlEx.Number}");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n[ERROR] An unexpected error occurred:");
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
            Console.ResetColor();
        }
        finally
        {
            Console.WriteLine("================================================================");
        }
    }
}

/// <summary>
/// Handles the logic for retrieving database metadata and formatting it as Markdown and Mermaid ERD.
/// </summary>
public class SchemaExporter
{
    private readonly string _connectionString;

    public SchemaExporter(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<SchemaExportResult> GenerateExportAsync()
    {
        var markdown = new StringBuilder();

        markdown.AppendLine("# Project Database Schema");
        markdown.AppendLine();
        markdown.AppendLine($"Generated on: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        markdown.AppendLine();
        markdown.AppendLine("---");
        markdown.AppendLine();

        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        var tables = await GetTablesAsync(connection);
        var views = await GetViewsAsync(connection);
        var procedures = await GetProceduresAsync(connection);
        var triggers = await GetTriggersAsync(connection);
        var allColumns = await GetAllColumnsAsync(connection);
        var allForeignKeys = await GetAllForeignKeysAsync(connection);
        var allProcParams = await GetAllProcedureParametersAsync(connection);

        Console.WriteLine($"Found {tables.Count} tables, {views.Count} views, {procedures.Count} stored procedures, and {triggers.Count} triggers. Generating documentation...");

        string standaloneMermaid = BuildStandaloneMermaid(tables, allColumns, allForeignKeys);

        markdown.AppendLine("## Entity Relationship Diagram");
        markdown.AppendLine();
        markdown.AppendLine("```mermaid");
        markdown.Append(standaloneMermaid);
        markdown.AppendLine("```");
        markdown.AppendLine();

        foreach (var table in tables)
        {
            markdown.AppendLine($"## {table}");
            markdown.AppendLine();
            markdown.AppendLine("| Column | Type | Nullable | Key |");
            markdown.AppendLine("|--------|------|----------|-----|");

            var columns = allColumns
                .Where(c => c.TableName.Equals(table, StringComparison.OrdinalIgnoreCase))
                .OrderBy(c => c.OrdinalPosition)
                .ToList();

            if (columns.Count == 0)
            {
                markdown.AppendLine("| *No columns found* | | | |");
            }
            else
            {
                foreach (var col in columns)
                {
                    string name = col.ColumnName.Replace("|", "\\|");
                    string type = col.ColumnType.Replace("|", "\\|");
                    string nullable = col.IsNullable;
                    string key = col.ColumnKey;

                    markdown.AppendLine($"| {name} | {type} | {nullable} | {key} |");
                }
            }

            var foreignKeys = allForeignKeys
                .Where(fk => fk.TableName.Equals(table, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (foreignKeys.Count > 0)
            {
                markdown.AppendLine();
                markdown.AppendLine("### Foreign Keys");
                markdown.AppendLine();
                markdown.AppendLine("| Column | Constraint | Referenced Table | Referenced Column |");
                markdown.AppendLine("|--------|------------|------------------|-------------------|");
                foreach (var fk in foreignKeys)
                {
                    markdown.AppendLine($"| {fk.ColumnName} | {fk.ConstraintName} | {fk.ReferencedTableName} | {fk.ReferencedColumnName} |");
                }
            }

            markdown.AppendLine();
        }

        if (views.Count > 0)
        {
            markdown.AppendLine("# Views");
            markdown.AppendLine();
            foreach (var view in views)
            {
                markdown.AppendLine($"## {view}");
                markdown.AppendLine();
                markdown.AppendLine("| Column | Type | Nullable |");
                markdown.AppendLine("|--------|------|----------|");

                var columns = allColumns
                    .Where(c => c.TableName.Equals(view, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(c => c.OrdinalPosition)
                    .ToList();

                if (columns.Count == 0)
                {
                    markdown.AppendLine("| *No columns found* | | |");
                }
                else
                {
                    foreach (var col in columns)
                    {
                        string name = col.ColumnName.Replace("|", "\\|");
                        string type = col.ColumnType.Replace("|", "\\|");
                        string nullable = col.IsNullable;

                        markdown.AppendLine($"| {name} | {type} | {nullable} |");
                    }
                }

                markdown.AppendLine();
            }
        }

        if (procedures.Count > 0)
        {
            markdown.AppendLine("# Stored Procedures");
            markdown.AppendLine();
            foreach (var proc in procedures)
            {
                markdown.AppendLine($"## {proc}");
                markdown.AppendLine();

                var parameters = allProcParams
                    .Where(p => p.ProcedureName.Equals(proc, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (parameters.Count > 0)
                {
                    markdown.AppendLine("| Mode | Parameter | Type |");
                    markdown.AppendLine("|------|-----------|------|");
                    foreach (var param in parameters)
                    {
                        markdown.AppendLine($"| {param.Mode} | {param.Name} | {param.DataType} |");
                    }
                    markdown.AppendLine();
                }
                else
                {
                    markdown.AppendLine("*No parameters*");
                    markdown.AppendLine();
                }
            }
        }

        if (triggers.Count > 0)
        {
            markdown.AppendLine("# Triggers");
            markdown.AppendLine();
            markdown.AppendLine("| Trigger | Event | Timing | Table |");
            markdown.AppendLine("|---------|-------|--------|-------|");
            foreach (var trigger in triggers)
            {
                markdown.AppendLine($"| {trigger.Name} | {trigger.Event} | {trigger.Timing} | {trigger.Table} |");
            }
            markdown.AppendLine();
        }

        return new SchemaExportResult(markdown.ToString(), standaloneMermaid);
    }

    private static string BuildStandaloneMermaid(
        IEnumerable<string> tables,
        IReadOnlyCollection<ColumnInfo> allColumns,
        IReadOnlyCollection<ForeignKeyInfo> allForeignKeys)
    {
        var sb = new StringBuilder();
        sb.AppendLine("erDiagram");

        foreach (var table in tables.OrderBy(t => t))
        {
            sb.AppendLine($"    {EscapeMermaidIdentifier(table)} {{");

            var columns = allColumns
                .Where(c => c.TableName.Equals(table, StringComparison.OrdinalIgnoreCase))
                .OrderBy(c => c.OrdinalPosition);

            foreach (var col in columns)
            {
                var keySuffix = col.ColumnKey switch
                {
                    "PRI" => " PK",
                    "UNI" => " UK",
                    _ => string.Empty
                };

                sb.AppendLine($"        {SanitizeType(col.ColumnType)} {EscapeMermaidIdentifier(col.ColumnName)}{keySuffix}");
            }

            sb.AppendLine("    }");
        }

        foreach (var rel in allForeignKeys
                     .GroupBy(fk => fk.ConstraintName)
                     .Select(g => g.First())
                     .OrderBy(fk => fk.ReferencedTableName)
                     .ThenBy(fk => fk.TableName))
        {
            sb.AppendLine(
                $"    {EscapeMermaidIdentifier(rel.ReferencedTableName)} ||--o{{ {EscapeMermaidIdentifier(rel.TableName)} : \"{EscapeMermaidLabel(rel.ConstraintName)}\"");
        }

        return sb.ToString();
    }

    private static string SanitizeType(string columnType)
    {
        var compact = columnType.Replace(" ", "_").Replace(",", "_");
        return EscapeMermaidIdentifier(compact);
    }

    private static string EscapeMermaidIdentifier(string value)
    {
        return value.Replace(" ", "_").Replace("-", "_");
    }

    private static string EscapeMermaidLabel(string value)
    {
        return value.Replace("\"", "\\\"");
    }

    private async Task<List<string>> GetTablesAsync(MySqlConnection connection)
    {
        var tables = new List<string>();

        var query = @"
            SELECT TABLE_NAME
            FROM INFORMATION_SCHEMA.TABLES
            WHERE TABLE_SCHEMA = DATABASE()
              AND TABLE_TYPE = 'BASE TABLE'
              AND TABLE_NAME != '__EFMigrationsHistory'
            ORDER BY TABLE_NAME;";

        using var cmd = new MySqlCommand(query, connection);
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            tables.Add(reader.GetString(0));
        }

        return tables;
    }

    private async Task<List<string>> GetViewsAsync(MySqlConnection connection)
    {
        var views = new List<string>();

        var query = @"
            SELECT TABLE_NAME
            FROM INFORMATION_SCHEMA.TABLES
            WHERE TABLE_SCHEMA = DATABASE()
              AND TABLE_TYPE = 'VIEW'
            ORDER BY TABLE_NAME;";

        using var cmd = new MySqlCommand(query, connection);
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            views.Add(reader.GetString(0));
        }

        return views;
    }

    private async Task<List<string>> GetProceduresAsync(MySqlConnection connection)
    {
        var procs = new List<string>();

        var query = @"
            SELECT ROUTINE_NAME
            FROM INFORMATION_SCHEMA.ROUTINES
            WHERE ROUTINE_SCHEMA = DATABASE()
              AND ROUTINE_TYPE = 'PROCEDURE'
            ORDER BY ROUTINE_NAME;";

        using var cmd = new MySqlCommand(query, connection);
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            procs.Add(reader.GetString(0));
        }

        return procs;
    }

    private async Task<List<TriggerInfo>> GetTriggersAsync(MySqlConnection connection)
    {
        var triggers = new List<TriggerInfo>();

        var query = @"
            SELECT TRIGGER_NAME, EVENT_MANIPULATION, ACTION_TIMING, EVENT_OBJECT_TABLE
            FROM INFORMATION_SCHEMA.TRIGGERS
            WHERE TRIGGER_SCHEMA = DATABASE()
            ORDER BY EVENT_OBJECT_TABLE, ACTION_TIMING, EVENT_MANIPULATION;";

        using var cmd = new MySqlCommand(query, connection);
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            triggers.Add(new TriggerInfo
            {
                Name = reader.GetString(0),
                Event = reader.GetString(1),
                Timing = reader.GetString(2),
                Table = reader.GetString(3)
            });
        }

        return triggers;
    }

    private async Task<List<ColumnInfo>> GetAllColumnsAsync(MySqlConnection connection)
    {
        var columns = new List<ColumnInfo>();

        var query = @"
            SELECT TABLE_NAME, COLUMN_NAME, COLUMN_TYPE, IS_NULLABLE, COLUMN_KEY, ORDINAL_POSITION
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_SCHEMA = DATABASE()
            ORDER BY TABLE_NAME, ORDINAL_POSITION;";

        using var cmd = new MySqlCommand(query, connection);
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            columns.Add(new ColumnInfo
            {
                TableName = reader.GetString(0),
                ColumnName = reader.GetString(1),
                ColumnType = reader.GetString(2),
                IsNullable = reader.GetString(3),
                ColumnKey = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                OrdinalPosition = reader.GetInt32(5)
            });
        }

        return columns;
    }

    private async Task<List<ForeignKeyInfo>> GetAllForeignKeysAsync(MySqlConnection connection)
    {
        var fks = new List<ForeignKeyInfo>();

        var query = @"
            SELECT
                TABLE_NAME,
                COLUMN_NAME,
                CONSTRAINT_NAME,
                REFERENCED_TABLE_NAME,
                REFERENCED_COLUMN_NAME
            FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
            WHERE TABLE_SCHEMA = DATABASE()
              AND REFERENCED_TABLE_NAME IS NOT NULL
            ORDER BY TABLE_NAME, CONSTRAINT_NAME;";

        using var cmd = new MySqlCommand(query, connection);
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            fks.Add(new ForeignKeyInfo
            {
                TableName = reader.GetString(0),
                ColumnName = reader.GetString(1),
                ConstraintName = reader.GetString(2),
                ReferencedTableName = reader.GetString(3),
                ReferencedColumnName = reader.GetString(4)
            });
        }

        return fks;
    }

    private async Task<List<ProcedureParameterInfo>> GetAllProcedureParametersAsync(MySqlConnection connection)
    {
        var paramsList = new List<ProcedureParameterInfo>();

        var query = @"
            SELECT SPECIFIC_NAME, PARAMETER_MODE, PARAMETER_NAME, DATA_TYPE
            FROM INFORMATION_SCHEMA.PARAMETERS
            WHERE SPECIFIC_SCHEMA = DATABASE()
            ORDER BY SPECIFIC_NAME, ORDINAL_POSITION;";

        using var cmd = new MySqlCommand(query, connection);
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            paramsList.Add(new ProcedureParameterInfo
            {
                ProcedureName = reader.GetString(0),
                Mode = reader.IsDBNull(1) ? "IN" : reader.GetString(1),
                Name = reader.GetString(2),
                DataType = reader.GetString(3)
            });
        }

        return paramsList;
    }
}

public sealed record SchemaExportResult(string Markdown, string StandaloneMermaid);

public class ColumnInfo
{
    public string TableName { get; set; } = string.Empty;
    public string ColumnName { get; set; } = string.Empty;
    public string ColumnType { get; set; } = string.Empty;
    public string IsNullable { get; set; } = string.Empty;
    public string ColumnKey { get; set; } = string.Empty;
    public int OrdinalPosition { get; set; }
}

public class ForeignKeyInfo
{
    public string TableName { get; set; } = string.Empty;
    public string ColumnName { get; set; } = string.Empty;
    public string ConstraintName { get; set; } = string.Empty;
    public string ReferencedTableName { get; set; } = string.Empty;
    public string ReferencedColumnName { get; set; } = string.Empty;
}

public class ProcedureParameterInfo
{
    public string ProcedureName { get; set; } = string.Empty;
    public string Mode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
}

public class TriggerInfo
{
    public string Name { get; set; } = string.Empty;
    public string Event { get; set; } = string.Empty;
    public string Timing { get; set; } = string.Empty;
    public string Table { get; set; } = string.Empty;
}
