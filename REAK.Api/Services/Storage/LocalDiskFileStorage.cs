namespace REAK.Api.Services.Storage;

public class LocalDiskFileStorage : IFileStorage
{
    private readonly string _root;
    private readonly string _publicUrlBase;

    public LocalDiskFileStorage(IConfiguration configuration, IWebHostEnvironment env)
    {
        _root = ResolveRoot(configuration, env);
        _publicUrlBase = configuration["Storage:PublicUrlBase"] ?? "/media";
        Directory.CreateDirectory(Path.Combine(_root, "listing-media"));
        Directory.CreateDirectory(Path.Combine(_root, "listing-documents"));
    }

    /// <summary>Shared with Program.cs, which needs the same "listing-media" subfolder path to
    /// mount static-file serving scoped to exactly that one public container.</summary>
    public static string ResolveRoot(IConfiguration configuration, IWebHostEnvironment env) =>
        configuration["Storage:LocalRoot"] ?? Path.Combine(env.ContentRootPath, "App_Data", "storage");

    public async Task<StoredFile> SaveAsync(string container, string fileName, Stream content, CancellationToken ct = default)
    {
        var safeName = $"{Guid.NewGuid():N}{Path.GetExtension(fileName)}";
        var relativePath = Path.Combine(container, safeName).Replace('\\', '/');
        var fullPath = Path.Combine(_root, container, safeName);

        await using (var fileStream = File.Create(fullPath))
        {
            await content.CopyToAsync(fileStream, ct);
        }

        return new StoredFile(relativePath, new FileInfo(fullPath).Length);
    }

    public Task<Stream?> OpenReadAsync(string storagePath, CancellationToken ct = default)
    {
        var fullPath = ResolveSafePath(storagePath);
        if (fullPath is null || !File.Exists(fullPath))
        {
            return Task.FromResult<Stream?>(null);
        }

        return Task.FromResult<Stream?>(File.OpenRead(fullPath));
    }

    public Task DeleteAsync(string storagePath, CancellationToken ct = default)
    {
        var fullPath = ResolveSafePath(storagePath);
        if (fullPath is not null && File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    /// <summary>The static-file middleware (wired in Program.cs) is mounted at this same
    /// `_publicUrlBase` but rooted ONLY at the "listing-media" subfolder — never at `_root` itself
    /// — precisely so a private "listing-documents" storagePath could never resolve to a reachable
    /// URL even by construction. Strip the container segment; only the generated file name matters
    /// once the static root is already scoped to the one public container.</summary>
    public string GetPublicUrl(string storagePath) => $"{_publicUrlBase}/{Path.GetFileName(storagePath)}";

    /// <summary>Rejects any storagePath that would resolve outside the storage root — storagePath
    /// values ultimately originate from database rows written by our own SaveAsync, but this stays
    /// defense-in-depth against a future caller passing an unsanitized path.</summary>
    private string? ResolveSafePath(string storagePath)
    {
        var fullPath = Path.GetFullPath(Path.Combine(_root, storagePath));
        return fullPath.StartsWith(Path.GetFullPath(_root), StringComparison.OrdinalIgnoreCase) ? fullPath : null;
    }
}
