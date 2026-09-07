using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Text.RegularExpressions;

namespace MindSync.Infrastructure.Data;

public static class DatabaseInitializer
{
    private const string SetupScriptResource = "MindSync.Infrastructure.Data.Scripts.SqlServer_Setup.sql";
    private const string SeedScriptResource = "MindSync.Infrastructure.Data.Scripts.AudioTracks_Seed.sql";

    public static async Task InitializeAsync(
        string connectionString,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        var masterConnectionString = BuildConnectionStringForDatabase(connectionString, "master");

        await WaitForServerAsync(masterConnectionString, logger, cancellationToken);

        logger.LogInformation("Aplicando esquema de banco de dados (SqlServer_Setup.sql)...");
        await ExecuteEmbeddedScriptAsync(masterConnectionString, SetupScriptResource, cancellationToken);

        logger.LogInformation("Catalogando faixas de áudio iniciais (AudioTracks_Seed.sql)...");
        await ExecuteEmbeddedScriptAsync(connectionString, SeedScriptResource, cancellationToken);

        logger.LogInformation("O banco de dados está pronto.");
    }

    private static async Task WaitForServerAsync(
        string masterConnectionString,
        ILogger logger, CancellationToken cancellationToken)
    {
        const int maxAttempts = 10;
        var delay = TimeSpan.FromSeconds(3);

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await using var connection = new SqlConnection(masterConnectionString);
                await connection.OpenAsync(cancellationToken);
                return;
            }
            catch (Exception ex) when (attempt < maxAttempts)
            {
                logger.LogWarning(
                    "O SQL Server ainda não está pronto (tentativa {Attempt}/{MaxAttempts}): {Message}. Tentando novamente em {Delay}s...",
                    attempt, maxAttempts, ex.Message, delay.TotalSeconds);
                await Task.Delay(delay, cancellationToken);
            }
        }

        await using var finalConnection = new SqlConnection(masterConnectionString);
        await finalConnection.OpenAsync(cancellationToken);
    }

    private static async Task ExecuteEmbeddedScriptAsync(
        string connectionString,
        string resourceName,
        CancellationToken cancellationToken)
    {
        var script = ReadEmbeddedScript(resourceName);

        var batches = SplitIntoBatches(script);

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        foreach (var batch in batches)
        {
            if (string.IsNullOrWhiteSpace(batch))
                continue;

            await using var command = new SqlCommand(batch, connection)
            {
                CommandTimeout = 60
            };
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private static string ReadEmbeddedScript(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException(
                $"Script incorporado '{resourceName}' não foi encontrado. Verifique a entrada <EmbeddedResource> em MindSync.Infrastructure.csproj.");

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private static string[] SplitIntoBatches(string script) =>
        Regex.Split(script, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);

    private static string BuildConnectionStringForDatabase(string connectionString, string database)
    {
        var builder = new SqlConnectionStringBuilder(connectionString)
        {
            InitialCatalog = database
        };
        return builder.ConnectionString;
    }
}