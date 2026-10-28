using System.Security.Cryptography;
using System.Text;
using RestauranteMonster.Helpers;

namespace RestauranteMonster.Tests;

public class EncriptadorTests
{
    private static string Sha256Hex(string s) => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(s)));

    [Fact]
    public void HashNuevo_UsaFormatoPbkdf2ConSal()
    {
        var hash = Encriptador.HashContrasena("Clave#Segura1");
        var partes = hash.Split('$');
        Assert.Equal(4, partes.Length);
        Assert.Equal("pbkdf2-sha256", partes[0]);
        Assert.Equal(16, Convert.FromBase64String(partes[2]).Length);
    }

    [Fact]
    public void MismaContrasena_ProduceHashesDistintos()
        => Assert.NotEqual(Encriptador.HashContrasena("igual"), Encriptador.HashContrasena("igual"));

    [Fact]
    public void Verificar_AceptaLaCorrectaYRechazaOtra()
    {
        var hash = Encriptador.HashContrasena("Clave#Segura1");
        Assert.True(Encriptador.Verificar("Clave#Segura1", hash, out var migrar));
        Assert.False(migrar);
        Assert.False(Encriptador.Verificar("clave#segura1", hash, out _));
    }

    [Fact]
    public void HashAntiguoSha256_SeAceptaYSeMarcaParaMigrar()
    {
        var antiguo = Sha256Hex("Admin#Monster2025");
        Assert.True(Encriptador.Verificar("Admin#Monster2025", antiguo, out var migrar));
        Assert.True(migrar);
        Assert.False(Encriptador.Verificar("otra", antiguo, out _));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("pbkdf2-sha256$600000$no-es-base64$!!")]
    public void ValoresAlmacenadosInvalidos_NoSeAceptan(string? almacenado)
        => Assert.False(Encriptador.Verificar("x", almacenado, out _));
}
