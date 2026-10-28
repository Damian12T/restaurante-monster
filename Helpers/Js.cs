using System.Globalization;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace RestauranteMonster.Helpers;

/// <summary>
/// Valores seguros para insertar en JavaScript dentro de atributos HTML (onclick="...").
/// JavaScriptEncoder escapa comillas, &lt;, &gt; y &amp; como \uXXXX, por lo que el resultado
/// no puede cerrar ni la cadena JS ni el atributo HTML.
/// </summary>
public static class Js
{
    /// <summary>Literal de cadena JS entre comillas simples.</summary>
    public static IHtmlContent Cadena(string? valor)
        => new HtmlString("'" + JavaScriptEncoder.Default.Encode(valor ?? string.Empty) + "'");

    /// <summary>Número con punto decimal, independiente de la cultura del servidor.</summary>
    public static IHtmlContent Numero(decimal valor)
        => new HtmlString(valor.ToString(CultureInfo.InvariantCulture));
}
