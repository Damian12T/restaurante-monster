// Añade el token antiforgery (generado por el layout) a las peticiones fetch que modifican datos.
(function () {
  const fetchOriginal = window.fetch;
  window.fetch = function (recurso, opciones = {}) {
    const metodo = (opciones.method || 'GET').toUpperCase();
    const url = new URL(typeof recurso === 'string' ? recurso : recurso.url, location.href);
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    if (token && url.origin === location.origin && !['GET', 'HEAD', 'OPTIONS'].includes(metodo)) {
      const cabeceras = new Headers(opciones.headers || {});
      if (!cabeceras.has('RequestVerificationToken')) cabeceras.set('RequestVerificationToken', token);
      opciones = { ...opciones, headers: cabeceras };
    }
    return fetchOriginal.call(this, recurso, opciones);
  };
})();
