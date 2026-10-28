// Prueba E2E de Restaurante Monster: funcionalidad (F) y seguridad (S).
// Requisitos: la app en APP_URL, la base restaurante_monster recién creada con
// database/restaurante_monster_sqlserver.sql, Chrome y sqlcmd con autenticación de Windows.
// Uso: node tests/e2e/e2e-restaurante.mjs [carpeta-de-salida]
import puppeteer from 'puppeteer-core';
import { execFileSync } from 'node:child_process';
import fs from 'node:fs';
import path from 'node:path';

const APP = process.env.APP_URL ?? 'http://localhost:5290';
const OUT = process.argv[2] ?? 'e2e-resultados';
const CHROME = process.env.CHROME_PATH ?? 'C:/Program Files/Google/Chrome/Application/chrome.exe';
const SQLCMD = process.env.SQLCMD ?? 'sqlcmd';
const DB_SERVER = process.env.DB_SERVER ?? 'localhost';
const CAPTURAS = process.env.CAPTURAS === '1';
fs.mkdirSync(OUT, { recursive: true });

const ADMIN = { correo: 'admin@monster.com', clave: 'Admin#Monster2025' };
const MESERO = { correo: 'mesero@monster.com', clave: 'Mesero#Monster2025' };
const S = Date.now().toString().slice(-6);

const resultados = [];
function check(id, nombre, ok, detalle = '') {
  resultados.push({ id, ok });
  console.log(`${ok ? 'PASA ' : 'FALLA'}  ${id}  ${nombre}${detalle ? '  -> ' + detalle : ''}`);
}

function db(sql) {
  const out = execFileSync(SQLCMD, ['-S', DB_SERVER, '-E', '-C', '-d', 'restaurante_monster', '-h', '-1', '-W',
    '-Q', `SET NOCOUNT ON; ${sql}`], { encoding: 'utf8' });
  return out.split(/\r?\n/).map((l) => l.trim()).filter(Boolean);
}
const db1 = (sql) => db(sql)[0];

const PNG_1X1 = Buffer.from(
  'iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==', 'base64');
const pngPath = path.resolve(OUT, 'prueba.png');
fs.writeFileSync(pngPath, PNG_1X1);

const browser = await puppeteer.launch({ executablePath: CHROME, headless: true, args: ['--lang=es-EC'] });

async function nuevaPagina(viewport = { width: 1366, height: 900 }) {
  const ctx = await browser.createBrowserContext();
  const page = await ctx.newPage();
  await page.setViewport(viewport);
  page.on('dialog', (d) => d.accept());
  return page;
}
const shot = async (page, nombre) => {
  if (CAPTURAS) await page.screenshot({ path: path.join(OUT, nombre), fullPage: false });
};
const texto = (page) => page.evaluate(() => document.body.innerText);

async function ir(page, ruta) {
  const r = await page.goto(APP + ruta, { waitUntil: 'networkidle0' });
  return r;
}

async function login(page, u) {
  await ir(page, '/Seguridad/Login');
  await page.locator('input[name=correo]').fill(u.correo);
  await page.locator('input[name=contrasena]').fill(u.clave);
  await Promise.all([page.waitForNavigation({ waitUntil: 'networkidle0' }),
    page.evaluate(() => document.querySelector('form[action="/Seguridad/Login"]').requestSubmit())]);
}

// Rellena y envía un formulario de la página por su atributo action (como lo haría el usuario).
async function enviarFormulario(page, action, valores, archivo) {
  await page.evaluate((action, valores) => {
    const f = document.querySelector(`form[action="${action}"]`);
    if (!f) throw new Error('No existe el formulario ' + action);
    for (const [k, v] of Object.entries(valores)) {
      const el = f.querySelector(`[name="${k}"]`);
      if (!el) throw new Error(`No existe el campo ${k} en ${action}`);
      el.value = v;
    }
  }, action, valores);
  if (archivo) {
    const input = await page.$(`form[action="${action}"] input[type=file]`);
    await input.uploadFile(archivo);
  }
  await Promise.all([page.waitForNavigation({ waitUntil: 'networkidle0' }),
    page.evaluate((action) => document.querySelector(`form[action="${action}"]`).requestSubmit(), action)]);
}

// Pulsa el control de una fila (enlace o botón dentro de un formulario) cuya ruta contiene `accion`.
async function pulsarEnFila(page, textoFila, accion) {
  const ok = await page.evaluate((textoFila, accion) => {
    const fila = [...document.querySelectorAll('tr')].find((tr) => tr.innerText.includes(textoFila));
    if (!fila) return false;
    const el = fila.querySelector(`a[href*="${accion}"]`) ??
      fila.querySelector(`form[action*="${accion}"] button, form[action*="${accion}"] [type=submit]`);
    if (!el) return false;
    el.setAttribute('data-e2e', '1');
    return true;
  }, textoFila, accion);
  if (!ok) return false;
  await Promise.all([page.waitForNavigation({ waitUntil: 'networkidle0' }), page.click('[data-e2e="1"]')]);
  return true;
}

// Petición POST sin token antiforgery (XMLHttpRequest, para no pasar por ningún envoltorio de fetch).
async function postSinToken(page, url, cuerpo) {
  return page.evaluate((url, cuerpo) => new Promise((res) => {
    const x = new XMLHttpRequest();
    x.open('POST', url);
    x.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');
    x.onload = () => res(x.status);
    x.onerror = () => res(-1);
    x.send(cuerpo);
  }), url, cuerpo);
}

// Sube un archivo arbitrario al formulario de platos, con el token de la página si existe.
async function subirArchivoPlato(page, nombrePlato, nombreArchivo, contenido, tipo, idTipo) {
  await ir(page, '/Admin/Platos');
  return page.evaluate(async (nombrePlato, nombreArchivo, contenido, tipo, idTipo) => {
    const f = document.querySelector('form[action="/Admin/Platos"]');
    const fd = new FormData(f);
    fd.set('idPlato', '0'); fd.set('nombre', nombrePlato); fd.set('descripcion', 'prueba');
    fd.set('precio', '1'); fd.set('idTipo', idTipo);
    fd.set('imagen', new Blob([contenido], { type: tipo }), nombreArchivo);
    const r = await fetch('/Admin/Platos', { method: 'POST', body: fd });
    return r.status;
  }, nombrePlato, nombreArchivo, contenido, tipo, idTipo);
}

const dir = path.resolve(process.env.WWWROOT ?? 'wwwroot', 'resources', 'productos');
const archivosAntes = new Set(fs.existsSync(dir) ? fs.readdirSync(dir) : []);
const hashLegacy = (s) => /^[0-9a-f]{64}$/.test(s ?? '');

// ───────────────────────────── Sin sesión y login
const anon = await nuevaPagina();
await ir(anon, '/');
check('F01', 'La raíz muestra el login', anon.url().includes('/Seguridad/Login') || (await anon.$('form[action="/Seguridad/Login"]')) !== null);
await shot(anon, '01-login.png');
await ir(anon, '/Admin/Platos');
const enLogin = async (p) => (await p.$('form[action="/Seguridad/Login"]')) !== null;
check('F02', 'Sin sesión, /Admin/Platos redirige al login', await enLogin(anon));
await login(anon, { correo: ADMIN.correo, clave: 'incorrecta' });
check('F03', 'Contraseña incorrecta muestra error', (await texto(anon)).includes('Credenciales incorrectas'));

// ───────────────────────────── Administrador
const admin = await nuevaPagina();
await login(admin, ADMIN);
check('F04', 'El administrador inicia sesión (usuario semilla)', admin.url().includes('/Admin/Dashboard'));
await shot(admin, '02-admin-dashboard.png');
check('S01', 'El hash del administrador se migra a un formato con sal tras iniciar sesión',
  !hashLegacy(db1(`SELECT contrasena FROM usuario WHERE correo='${ADMIN.correo}'`)));

let vistasOk = 0;
for (const v of ['Dashboard', 'Platos', 'Bebidas', 'Categorias', 'Clientes', 'Empleados', 'Stock', 'Facturas', 'Reportes']) {
  const r = await ir(admin, '/Admin/' + v);
  if (r.status() === 200 && !(await texto(admin)).includes('Error:')) vistasOk++;
}
check('F05', 'Las 9 vistas administrativas cargan sin error', vistasOk === 9, `${vistasOk}/9`);

// Categorías: crear, editar
const cat = `Cat E2E ${S}`;
await ir(admin, '/Admin/Categorias');
await enviarFormulario(admin, '/Admin/Categorias', { idTipo: '0', nombre: cat });
const idEntrada = db1("SELECT id_tipo FROM tipo_plato WHERE nombre=N'Entrada'");
const idCat = db1(`SELECT id_tipo FROM tipo_plato WHERE nombre=N'${cat}'`);
check('F06', 'Crear categoría', !!idCat);
await ir(admin, `/Admin/Categorias?editar=${idCat}`);
await enviarFormulario(admin, '/Admin/Categorias', { idTipo: idCat, nombre: cat + ' ed' });
check('F07', 'Editar categoría', db1(`SELECT nombre FROM tipo_plato WHERE id_tipo=${idCat}`) === cat + ' ed');

// Platos: crear con imagen PNG válida, editar, cambiar disponibilidad
const plato = `Plato E2E ${S}`;
await ir(admin, '/Admin/Platos');
await enviarFormulario(admin, '/Admin/Platos', { idPlato: '0', nombre: plato, descripcion: 'Creado por la E2E', precio: '7.50', idTipo: idEntrada }, pngPath);
const idPlato = db1(`SELECT id_plato FROM plato WHERE nombre=N'${plato}'`);
const img = db1(`SELECT imagen FROM plato WHERE id_plato=${idPlato}`);
const rImg = img ? await admin.evaluate((u) => fetch(u).then((r) => r.status), '/' + img) : null;
check('F08', 'Crear plato con imagen PNG', !!idPlato && rImg === 200, `${img ?? 'sin imagen'} HTTP ${rImg}`);
await ir(admin, `/Admin/Platos?editar=${idPlato}`);
await enviarFormulario(admin, '/Admin/Platos', { idPlato, nombre: plato, descripcion: 'Editado', precio: '8.25', idTipo: idEntrada });
check('F09', 'Editar plato', db1(`SELECT CAST(precio AS VARCHAR) FROM plato WHERE id_plato=${idPlato}`) === '8.25');
await ir(admin, '/Admin/Platos');
const pulsado = await pulsarEnFila(admin, plato, 'TogglePlato');
check('F10', 'Desactivar plato desde la vista', pulsado && db1(`SELECT disponible FROM plato WHERE id_plato=${idPlato}`) === '0');
await ir(admin, '/Admin/Platos');
await pulsarEnFila(admin, plato, 'TogglePlato');
check('F11', 'Reactivar plato desde la vista', db1(`SELECT disponible FROM plato WHERE id_plato=${idPlato}`) === '1');
await admin.evaluate((t) => {
  const fila = [...document.querySelectorAll('tr')].find((tr) => tr.innerText.includes(t));
  [...fila.querySelectorAll('button')].find((b) => (b.getAttribute('onclick') ?? '').startsWith('editarPlato'))?.click();
}, plato);
const precioForm = await admin.evaluate(() => document.querySelector('form[action="/Admin/Platos"] [name=precio]').value);
check('F26', 'El botón Editar de un plato carga el precio decimal correctamente', precioForm === '8.25', precioForm);
await shot(admin, '03-admin-platos.png');

// Bebidas
const bebida = `Bebida E2E ${S}`;
await ir(admin, '/Admin/Bebidas');
await enviarFormulario(admin, '/Admin/Bebidas', { idBebida: '0', nombre: bebida, descripcion: 'E2E', precio: '2.00' });
const idBebida = db1(`SELECT id_bebida FROM bebida WHERE nombre=N'${bebida}'`);
await ir(admin, '/Admin/Bebidas');
await pulsarEnFila(admin, bebida, 'ToggleBebida');
check('F12', 'Crear bebida y cambiar su disponibilidad', !!idBebida && db1(`SELECT disponible FROM bebida WHERE id_bebida=${idBebida}`) === '0');

// Empleados: crear, desactivar, reactivar
const correoEmp = `mesero${S}@e2e.test`;
await ir(admin, '/Admin/Empleados');
await enviarFormulario(admin, '/Admin/Empleados', { idUsuario: '0', nombre: 'Elena', apellido: 'Prueba', correo: correoEmp, telefono: '0990000000', contrasena: 'Clave#Segura1' });
const idEmp = db1(`SELECT id_usuario FROM usuario WHERE correo='${correoEmp}'`);
check('F13', 'Crear empleado', !!idEmp);
check('S02', 'La contraseña de un usuario nuevo no se guarda como SHA-256 sin sal',
  !!idEmp && !hashLegacy(db1(`SELECT contrasena FROM usuario WHERE id_usuario=${idEmp}`)));
await ir(admin, '/Admin/Empleados');
await pulsarEnFila(admin, correoEmp, 'ToggleEmpleado');
const desact = db1(`SELECT estado FROM usuario WHERE id_usuario=${idEmp}`) === '0';
await ir(admin, '/Admin/Empleados');
await pulsarEnFila(admin, correoEmp, 'ToggleEmpleado');
check('F14', 'Desactivar y reactivar empleado', desact && db1(`SELECT estado FROM usuario WHERE id_usuario=${idEmp}`) === '1');

// Clientes (alta desde el panel)
const correoCliAdm = `cliadm${S}@e2e.test`;
await ir(admin, '/Admin/Clientes');
await enviarFormulario(admin, '/Admin/Clientes', { idCliente: '0', idUsuario: '0', nombre: 'Carla', apellido: 'Panel', correo: correoCliAdm, telefono: '', direccion: 'Quito', fechaNacimiento: '2000-01-01', contrasena: 'Clave#Segura1' });
check('F15', 'Registrar cliente desde el panel', !!db1(`SELECT id_usuario FROM usuario WHERE correo='${correoCliAdm}'`));

// Stock (guardado por fila con fetch)
const idStock = db1('SELECT TOP 1 id_stock FROM stock ORDER BY id_stock');
await ir(admin, '/Admin/Stock');
await admin.evaluate((id) => { document.getElementById('cant-' + id).value = '77'; guardarFila(id); }, idStock);
await new Promise((r) => setTimeout(r, 1500));
check('F16', 'Actualizar stock de una fila', db1(`SELECT cantidad FROM stock WHERE id_stock=${idStock}`) === '77');
await ir(admin, '/Admin/Stock');
await admin.evaluate((id) => { document.getElementById('cant-' + id).value = '55'; }, idStock);
await Promise.all([admin.waitForNavigation({ waitUntil: 'networkidle0' }), admin.evaluate(() => actualizarTodo())]);
check('F27', 'Actualizar todo el stock (formulario)', db1(`SELECT cantidad FROM stock WHERE id_stock=${idStock}`) === '55');
await shot(admin, '10-admin-stock.png');

// ───────────────────────────── Sondas de seguridad del administrador
const estadoAntes = db1(`SELECT disponible FROM plato WHERE id_plato=${idPlato}`);
await ir(admin, `/Admin/TogglePlato?id=${idPlato}&disponible=false`);
check('S03', 'Un GET no cambia la disponibilidad de un plato', db1(`SELECT disponible FROM plato WHERE id_plato=${idPlato}`) === estadoAntes);
await ir(admin, `/Admin/EliminarCategoria?id=${idCat}`);
check('S04', 'Un GET no elimina una categoría', !!db1(`SELECT id_tipo FROM tipo_plato WHERE id_tipo=${idCat}`));

await ir(admin, '/Admin/Categorias');
const st = await postSinToken(admin, '/Admin/Categorias', `idTipo=0&nombre=CSRF${S}`);
check('S05', 'Un POST sin token antiforgery se rechaza', !db1(`SELECT id_tipo FROM tipo_plato WHERE nombre=N'CSRF${S}'`), `HTTP ${st}`);

await subirArchivoPlato(admin, `HTML ${S}`, 'ataque.html', '<script>alert(1)</script>', 'image/png', idEntrada);
const nuevos = (fs.existsSync(dir) ? fs.readdirSync(dir) : []).filter((f) => !archivosAntes.has(f));
check('S06', 'Se rechaza un .html declarado como image/png',
  !db1(`SELECT id_plato FROM plato WHERE nombre=N'HTML ${S}'`) && !nuevos.some((f) => !/\.(jpg|png|webp)$/i.test(f)));
await subirArchivoPlato(admin, `Falso ${S}`, 'falso.png', 'esto no es una imagen', 'image/png', idEntrada);
check('S07', 'Se rechaza un .png cuyo contenido no es una imagen', !db1(`SELECT id_plato FROM plato WHERE nombre=N'Falso ${S}'`));

// Eliminar categoría desde la vista (con confirmación)
await ir(admin, '/Admin/Categorias');
await pulsarEnFila(admin, cat + ' ed', 'EliminarCategoria');
check('F17', 'Eliminar categoría desde la vista', !db1(`SELECT id_tipo FROM tipo_plato WHERE id_tipo=${idCat}`));

// ───────────────────────────── Mesero
const mesero = await nuevaPagina();
await login(mesero, MESERO);
check('F18', 'El mesero inicia sesión', mesero.url().includes('/Mesero/Dashboard'));
await ir(mesero, '/Admin/Platos');
check('F19', 'El mesero no accede a la administración', (await mesero.$('form[action="/Admin/Platos"]')) === null);
const correoCliMes = `climes${S}@e2e.test`;
await ir(mesero, '/Mesero/RegistrarCliente');
await enviarFormulario(mesero, '/Mesero/RegistrarCliente', { nombre: 'Mario', apellido: 'Mesa', correo: correoCliMes, telefono: '', direccion: '', fechaNacimiento: '', contrasena: 'Clave#Segura1' });
check('F28', 'El mesero registra un cliente', !!db1(`SELECT id_usuario FROM usuario WHERE correo='${correoCliMes}'`));
await ir(mesero, '/Mesero/TomarPedido');
await Promise.all([mesero.waitForNavigation({ waitUntil: 'networkidle0' }), mesero.evaluate((id) => agregarItem('PLATO', id), Number(idPlato))]);
await shot(mesero, '04-mesero-pedido.png');
await Promise.all([mesero.waitForNavigation({ waitUntil: 'networkidle0' }),
  mesero.evaluate(() => document.querySelector('form[action="/Mesero/TomarPedido"]').requestSubmit())]);
check('F20', 'El mesero registra un pedido y obtiene la factura', mesero.url().includes('/Mesero/Factura'));
await shot(mesero, '05-mesero-factura.png');

const meseroNuevo = await nuevaPagina();
await login(meseroNuevo, { correo: correoEmp, clave: 'Clave#Segura1' });
check('F21', 'Un empleado creado desde el panel inicia sesión', meseroNuevo.url().includes('/Mesero/Dashboard'));

// ───────────────────────────── Clientes (registro público, pedido, factura)
async function clienteNuevo(nombre, correo) {
  const p = await nuevaPagina();
  await ir(p, '/Seguridad/Registro');
  await enviarFormulario(p, '/Seguridad/Registro', { nombre, apellido: 'Cliente', correo, telefono: '', contrasena: 'Clave#Segura1' });
  await login(p, { correo, clave: 'Clave#Segura1' });
  return p;
}
const cliA = await clienteNuevo('Ana', `ana${S}@e2e.test`);
check('F22', 'Registro público y login de cliente', cliA.url().includes('/Cliente/Menu'));
await shot(cliA, '06-cliente-menu.png');
await cliA.evaluate((id) => ajustar('PLATO', id, 1), Number(idPlato));
await new Promise((r) => setTimeout(r, 800));
await ir(cliA, '/Cliente/Carrito');
await Promise.all([cliA.waitForNavigation({ waitUntil: 'networkidle0' }), cliA.evaluate(() => cambiarCantidad(0, 3))]);
await shot(cliA, '07-cliente-carrito.png');
await Promise.all([cliA.waitForNavigation({ waitUntil: 'networkidle0' }),
  cliA.evaluate(() => document.querySelector('form[action="/Cliente/Pedido"]').requestSubmit())]);
const facturaA = new URL(cliA.url()).searchParams.get('id') ?? cliA.url().split('/').pop();
check('F23', 'El cliente confirma el pedido y ve su factura', cliA.url().includes('/Cliente/Factura') && (await texto(cliA)).includes('Ana'));
check('F29', 'La cantidad modificada en el carrito llega al pedido', db1(`SELECT d.cantidad FROM detalle_pedido d JOIN factura f ON f.id_pedido=d.id_pedido WHERE f.id_factura=${facturaA}`) === '3');
await shot(cliA, '08-cliente-factura.png');

const cliB = await clienteNuevo('Beto', `beto${S}@e2e.test`);
await ir(cliB, `/Cliente/Factura?id=${facturaA}`);
check('S08', 'Un cliente no puede ver la factura de otro', !(await texto(cliB)).includes('Ana Cliente'));

// XSS: nombres maliciosos registrados por el público y vistos por el administrador
const nomXss1 = `Eve"><img src=x onerror="window.__xss=1">`;
const nomXss2 = `O'Brien');window.__xss=2;//`;
await clienteNuevo(nomXss1, `eve${S}@e2e.test`);
await clienteNuevo(nomXss2, `obrien${S}@e2e.test`);
await ir(admin, '/Admin/Clientes');
await new Promise((r) => setTimeout(r, 500));
const xssCarga = await admin.evaluate(() => window.__xss);
await admin.evaluate((t) => {
  const fila = [...document.querySelectorAll('tr')].find((tr) => tr.innerText.includes(t));
  fila?.querySelector('button')?.click();
}, `obrien${S}@e2e.test`);
const xssClic = await admin.evaluate(() => window.__xss);
const valorEditado = await admin.evaluate(() => document.querySelector('form[action="/Admin/Clientes"] [name=nombre]').value);
check('S09', 'Un nombre con HTML no ejecuta código en /Admin/Clientes', xssCarga === undefined && xssClic === undefined, `__xss=${xssClic ?? xssCarga}`);
check('F24', 'Editar un cliente con comillas en el nombre rellena el formulario correctamente', valorEditado === nomXss2, JSON.stringify(valorEditado));
await shot(admin, '09-admin-clientes.png');

// Cierre de sesión
await ir(admin, '/Seguridad/Logout');
await ir(admin, '/Admin/Dashboard');
check('F25', 'Cerrar sesión', await enLogin(admin));

await browser.close();
const f = resultados.filter((r) => r.id.startsWith('F'));
const s = resultados.filter((r) => r.id.startsWith('S'));
const linea = `Funcionales: ${f.filter((r) => r.ok).length}/${f.length} · Seguridad: ${s.filter((r) => r.ok).length}/${s.length}`;
console.log('\n' + linea);
fs.writeFileSync(path.join(OUT, 'resultado.txt'), linea + '\n');
process.exit(resultados.every((r) => r.ok) ? 0 : 1);
