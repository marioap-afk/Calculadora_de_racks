using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using NJsonSchema;

namespace RackCad.Catalogs.RackFrames;

public sealed class JsonRackFrameCatalogService
{
    private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };

    public async Task<RackFrameCatalog> LoadAsync(string catalogPath, CancellationToken cancellationToken = default)
    {
        using var stream = File.OpenRead(catalogPath);
        var catalog = await JsonSerializer.DeserializeAsync<RackFrameCatalog>(stream, SerializerOptions, cancellationToken)
            .ConfigureAwait(false);

        return catalog ?? new RackFrameCatalog();
    }

    public async Task SaveAsync(
        RackFrameCatalog catalog,
        string catalogPath,
        CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(catalogPath);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var stream = File.Create(catalogPath);
        await JsonSerializer.SerializeAsync(stream, catalog, SerializerOptions, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<RackFrameCatalogValidationResult> ValidateAsync(
        string catalogPath,
        string schemaPath,
        CancellationToken cancellationToken = default)
    {
        var catalogJson = await File.ReadAllTextAsync(catalogPath, cancellationToken).ConfigureAwait(false);
        var schemaJson = await File.ReadAllTextAsync(schemaPath, cancellationToken).ConfigureAwait(false);
        var schema = await JsonSchema.FromJsonAsync(schemaJson, cancellationToken).ConfigureAwait(false);
        var validationErrors = schema.Validate(catalogJson);
        var errors = new List<string>();

        foreach (var error in validationErrors)
        {
            errors.Add(error.Path + ": " + error.Kind);
        }

        return new RackFrameCatalogValidationResult(errors);
    }
}
