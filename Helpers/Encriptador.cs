using System.Security.Cryptography;
using System.Text;

namespace RestauranteMonster.Helpers;

/// <summary>
/// Hash de contraseñas con PBKDF2-HMAC-SHA256 y sal aleatoria por usuario.
/// Formato almacenado: pbkdf2-sha256$iteraciones$sal(base64)$hash(base64).
/// Acepta además el formato anterior (SHA-256 hexadecimal sin sal) para no
/// bloquear cuentas existentes; ese formato se migra al iniciar sesión.
/// </summary>
public static class Encriptador
{
    private const string Prefijo = "pbkdf2-sha256";
    private const int Iteraciones = 600_000;   // recomendación de OWASP para PBKDF2-HMAC-SHA256
    private const int BytesSal = 16;
    private const int BytesHash = 32;

    public static string HashContrasena(string contrasena)
    {
        var sal = RandomNumberGenerator.GetBytes(BytesSal);
        var hash = Rfc2898DeriveBytes.Pbkdf2(contrasena, sal, Iteraciones, HashAlgorithmName.SHA256, BytesHash);
        return $"{Prefijo}${Iteraciones}${Convert.ToBase64String(sal)}${Convert.ToBase64String(hash)}";
    }

    /// <param name="requiereMigrar">true si la contraseña es correcta pero está en un formato antiguo o más débil.</param>
    public static bool Verificar(string contrasena, string? almacenado, out bool requiereMigrar)
    {
        requiereMigrar = false;
        if (string.IsNullOrEmpty(almacenado)) return false;

        var partes = almacenado.Split('$');
        if (partes.Length == 4 && partes[0] == Prefijo && int.TryParse(partes[1], out var iteraciones))
        {
            byte[] sal, esperado;
            try
            {
                sal = Convert.FromBase64String(partes[2]);
                esperado = Convert.FromBase64String(partes[3]);
            }
            catch (FormatException) { return false; }

            var hash = Rfc2898DeriveBytes.Pbkdf2(contrasena, sal, iteraciones, HashAlgorithmName.SHA256, esperado.Length);
            var correcta = CryptographicOperations.FixedTimeEquals(hash, esperado);
            requiereMigrar = correcta && iteraciones < Iteraciones;
            return correcta;
        }

        // Formato anterior: SHA-256 en hexadecimal, sin sal.
        var legado = Encoding.ASCII.GetBytes(Sha256Hex(contrasena));
        var correctaLegado = CryptographicOperations.FixedTimeEquals(legado, Encoding.ASCII.GetBytes(almacenado.ToLowerInvariant()));
        requiereMigrar = correctaLegado;
        return correctaLegado;
    }

    private static string Sha256Hex(string texto)
        => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(texto)));
}
