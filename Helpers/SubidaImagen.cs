namespace RestauranteMonster.Helpers;

public static class SubidaImagen
{
    private const long MaxTamano = 2 * 1024 * 1024;

    public static async Task<string?> GuardarAsync(IFormFile? archivo, IWebHostEnvironment env)
    {
        if (archivo == null || archivo.Length == 0) return null;
        if (archivo.Length > MaxTamano)
            throw new InvalidOperationException("La imagen no puede superar 2 MB.");

        // El tipo se decide por el contenido real del archivo, no por el nombre ni por
        // el Content-Type que envía el navegador (ambos los controla el cliente).
        var cabecera = new byte[12];
        await using (var lectura = archivo.OpenReadStream())
        {
            var leidos = await lectura.ReadAtLeastAsync(cabecera, cabecera.Length, throwOnEndOfStream: false);
            if (leidos < cabecera.Length) Array.Clear(cabecera, leidos, cabecera.Length - leidos);
        }
        var extension = DetectarExtension(cabecera)
            ?? throw new InvalidOperationException("Solo se permiten imágenes JPEG, PNG o WEBP.");

        var nombreArchivo = $"{Guid.NewGuid():N}{extension}";
        var carpeta       = Path.Combine(env.WebRootPath, "resources", "productos");
        Directory.CreateDirectory(carpeta);

        await using var stream = new FileStream(Path.Combine(carpeta, nombreArchivo), FileMode.CreateNew);
        await archivo.CopyToAsync(stream);

        return $"resources/productos/{nombreArchivo}";
    }

    /// <summary>Devuelve la extensión según la firma del archivo, o null si no es JPEG, PNG ni WEBP.</summary>
    public static string? DetectarExtension(ReadOnlySpan<byte> c)
    {
        if (c.Length >= 3 && c[0] == 0xFF && c[1] == 0xD8 && c[2] == 0xFF)
            return ".jpg";
        if (c.Length >= 8 && c[..8].SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }))
            return ".png";
        if (c.Length >= 12 && c[..4].SequenceEqual("RIFF"u8) && c[8..12].SequenceEqual("WEBP"u8))
            return ".webp";
        return null;
    }
}
