using System.Text;
using RestauranteMonster.Helpers;

namespace RestauranteMonster.Tests;

public class SubidaImagenTests
{
    [Fact]
    public void Jpeg() => Assert.Equal(".jpg", SubidaImagen.DetectarExtension(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0, 0, 0, 0, 0, 0, 0, 0 }));

    [Fact]
    public void Png() => Assert.Equal(".png", SubidaImagen.DetectarExtension(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 0 }));

    [Fact]
    public void Webp() => Assert.Equal(".webp", SubidaImagen.DetectarExtension(Encoding.ASCII.GetBytes("RIFF\0\0\0\0WEBP")));

    [Theory]
    [InlineData("<script>alert(1)</script>")]
    [InlineData("GIF89a......")]
    [InlineData("RIFF\0\0\0\0WAVE")]
    [InlineData("")]
    public void ContenidoQueNoEsJpegPngNiWebp_SeRechaza(string contenido)
        => Assert.Null(SubidaImagen.DetectarExtension(Encoding.ASCII.GetBytes(contenido)));
}
