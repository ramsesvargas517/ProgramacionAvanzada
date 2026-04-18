using ProgramacionAvanzadaG8.EntityFramework;
using ProgramacionAvanzadaG8.Models;
using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;

namespace ProgramacionAvanzadaG8.Controllers
{
    public class AccountController : Controller
    {
        private FrijolitoEntities1 db = new FrijolitoEntities1();

        // ===============================================================
        // GET: Login
        // ===============================================================
        public ActionResult Login()
        {
            if (Session["UsuarioId"] != null)
                return RedirectToAction("Index", "Home");
            return View();
        }

        // ===============================================================
        // POST: Login
        // Campos reales BD: username, password_hash, rol_id
        // ===============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(Models.UsuarioModel model, string returnUrl)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.PasswordHash))
            {
                TempData["Error"] = "Por favor complete todos los campos.";
                return View(model);
            }

            var hash = HashSHA256(model.PasswordHash);

            // Buscar por email O username, con password_hash
            var usuario = db.Usuario.FirstOrDefault(u =>
                (u.email == model.Username || u.username == model.Username) &&
                u.password_hash == hash);

            if (usuario == null)
            {
                TempData["Error"] = "Correo o contrase\u00f1a incorrectos.";
                return View(model);
            }

            Session["UsuarioId"] = usuario.usuario_id;
            Session["UsuarioNombre"] = usuario.nombre;
            Session["UsuarioRolId"] = usuario.rol_id;

            // Mantener compatibilidad con el filtro AdminAuthorize usado en AdminController
            if (usuario.rol_id == 1)
                Session["RolAdmin"] = "Administrador";
            else
                Session.Remove("RolAdmin");

            // rol_id 1 = Admin (ajusta el número si es diferente en tu BD)
            if (usuario.rol_id == 1)
                Session["RolAdmin"] = "Administrador";
            else
                Session.Remove("RolAdmin");

            // Si había un returnUrl (p.e. redirección desde admin) y es local, redirigir allí
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            if (usuario.rol_id == 1)
                return RedirectToAction("Index", "Admin");

            return RedirectToAction("Index", "Home");
        }

        // ===============================================================
        // Logout
        // ===============================================================
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }

        // ===============================================================
        // GET: Registro
        // ===============================================================
        public ActionResult Registro()
        {
            return View(new Models.UsuarioModel());
        }

        // ===============================================================
        // POST: Registro
        // ===============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registro(Models.UsuarioModel model)
        {
            if (model == null)
            {
                ModelState.AddModelError("", "Datos inválidos.");
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Verificar que no exista el email ni el username
            if (db.Usuario.Any(u => u.email == model.Email))
            {
                ModelState.AddModelError("Email", "Ya existe una cuenta con ese correo.");
                return View(model);
            }
            if (db.Usuario.Any(u => u.username == model.Username))
            {
                ModelState.AddModelError("Username", "Ese nombre de usuario ya est\u00e1 en uso.");
                return View(model);
            }

            var nuevo = new Usuario
            {
                username = model.Username,
                nombre = model.Nombre,
                apellido = model.Apellido,
                email = model.Email,
                password_hash = HashSHA256(model.PasswordHash),
                rol_id = 2   // 2 = Cliente (ajusta si es diferente)
            };

            db.Usuario.Add(nuevo);
            db.SaveChanges();

            TempData["Success"] = "Cuenta creada exitosamente. Ya puedes iniciar sesi\u00f3n.";
            return RedirectToAction("Login");
        }

        // ===============================================================
        // GET: RecuperarContrasenna
        // ===============================================================
        public ActionResult RecuperarContrasenna()
        {
            return View();
        }

        // Endpoint pequeño para validar si el usuario está autenticado (usado por AJAX)
        public ActionResult VerificarSesion()
        {
            return Json(new { autenticado = Session["UsuarioId"] != null }, JsonRequestBehavior.AllowGet);
        }

        // ===============================================================
        // POST: RecuperarContrasenna
        // ===============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RecuperarContrasenna(string CorreoElectronico)
        {
            TempData["CorreoIngresado"] = CorreoElectronico;

            // Buscar por email (campo real en la BD)
            var usuario = db.Usuario.FirstOrDefault(u =>
                u.email == CorreoElectronico);

            // Siempre el mismo mensaje por seguridad
            if (usuario == null)
            {
                TempData["Success"] = "Si el correo existe en nuestro sistema, recibir\u00e1s un enlace en breve.";
                return RedirectToAction("RecuperarContrasenna");
            }

            // Las columnas token_recuperacion y token_expiracion
            // ya existen en la BD según la consulta SELECT que enviaste ✓
            string token = GenerarToken();
            DateTime expira = DateTime.Now.AddHours(2);

            usuario.token_recuperacion = token;
            usuario.token_expiracion = expira;
            db.SaveChanges();

            string enlace = Url.Action(
                "RestablecerContrasenna", "Account",
                new { token = token },
                Request.Url.Scheme
            );

            // Usar el campo email para enviar
            bool enviado = EnviarCorreoRecuperacion(
                usuario.email,
                usuario.nombre,
                enlace
            );

            if (enviado)
            {
                TempData["Success"] = "Si el correo existe en nuestro sistema, recibir\u00e1s un enlace en breve.";
            }
            else
            {
                TempData["Error"] = "Error al enviar el correo. Int\u00e9ntalo de nuevo.";
                // si hay detalle lo añadimos para debugging en la vista (solo en dev)
                if (TempData["ErrorDetalleCorreo"] != null)
                    TempData["Error"] += " Detalle: " + TempData["ErrorDetalleCorreo"];
            }

            return RedirectToAction("RecuperarContrasenna");
        }

        // ===============================================================
        // GET: RestablecerContrasenna?token=xxx
        // ===============================================================
        public ActionResult RestablecerContrasenna(string token)
        {
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login");

            var usuario = db.Usuario.FirstOrDefault(u =>
                u.token_recuperacion == token &&
                u.token_expiracion > DateTime.Now);

            if (usuario == null)
            {
                TempData["Error"] = "El enlace es inv\u00e1lido o ya expir\u00f3. Solicita uno nuevo.";
                return RedirectToAction("RecuperarContrasenna");
            }

            ViewBag.Token = token;
            return View();
        }

        // ===============================================================
        // POST: RestablecerContrasenna
        // ===============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RestablecerContrasenna(string token,
            string NuevaContrasenna, string ConfirmarContrasenna)
        {
            if (NuevaContrasenna != ConfirmarContrasenna)
            {
                TempData["Error"] = "Las contrase\u00f1as no coinciden.";
                ViewBag.Token = token;
                return View();
            }

            var usuario = db.Usuario.FirstOrDefault(u =>
                u.token_recuperacion == token &&
                u.token_expiracion > DateTime.Now);

            if (usuario == null)
            {
                TempData["Error"] = "El enlace es inv\u00e1lido o ya expir\u00f3.";
                return RedirectToAction("RecuperarContrasenna");
            }

            // Actualizar con el campo real: password_hash
            usuario.password_hash = HashSHA256(NuevaContrasenna);
            usuario.token_recuperacion = null;
            usuario.token_expiracion = null;
            db.SaveChanges();

            TempData["Success"] = "\u00a1Contrase\u00f1a actualizada! Ya puedes iniciar sesi\u00f3n.";
            return RedirectToAction("Login");
        }

        // ===============================================================
        // HELPERS PRIVADOS
        // ===============================================================

        private string HashSHA256(string input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input), "Password value was null when attempting to hash. Ensure the form field name matches the action parameter and the value is provided.");

            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                var hash = sha.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        private string GenerarToken()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                var bytes = new byte[48];
                rng.GetBytes(bytes);
                return Convert.ToBase64String(bytes)
                    .Replace("+", "-").Replace("/", "_").Replace("=", "");
            }
        }

        private bool EnviarCorreoRecuperacion(string destinatario,
            string nombre, string enlace)
        {
            try
            {
                // Leer configuración con nombres posibles (compatibilidad con distintos web.config)
                var cfg = System.Configuration.ConfigurationManager.AppSettings;
                string smtpUsuario = cfg["SmtpUsuario"] ?? cfg["CuentaCorreo"];
                string smtpPassword = cfg["SmtpPassword"] ?? cfg["contrasennaCorreo"];
                string smtpHost = cfg["SmtpHost"] ?? cfg["SmtpServer"] ?? "smtp.gmail.com";
                int smtpPort = 587;
                int.TryParse(cfg["SmtpPort"], out smtpPort);

                if (string.IsNullOrEmpty(smtpUsuario) || string.IsNullOrEmpty(smtpPassword))
                    return false;

                var mensaje = new MailMessage
                {
                    From = new MailAddress(smtpUsuario, "Frijolito Jugueter\u00eda"),
                    Subject = "Recuperaci\u00f3n de contrase\u00f1a - Frijolito",
                    IsBodyHtml = true,
                    Body = CuerpoCorreo(nombre, enlace)
                };
                mensaje.To.Add(destinatario);

                var cliente = new SmtpClient(smtpHost, smtpPort)
                {
                    Credentials = new NetworkCredential(smtpUsuario, smtpPassword),
                    EnableSsl = true
                };
                cliente.Send(mensaje);
                return true;
            }
            catch (Exception ex)
            {
                // Guardar detalle en TempData para diagnóstico (se muestra en la vista de recuperación)
                try { TempData["ErrorDetalleCorreo"] = ex.Message + " - " + ex.StackTrace; } catch { }
                // También escribir un log sencillo en App_Data para revisión del servidor
                try
                {
                    var folder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
                    if (!System.IO.Directory.Exists(folder)) System.IO.Directory.CreateDirectory(folder);
                    var logFile = System.IO.Path.Combine(folder, "smtp_errors.log");
                    var text = DateTime.Now.ToString("s") + " - " + ex.ToString() + Environment.NewLine;
                    System.IO.File.AppendAllText(logFile, text);
                }
                catch { }
                return false;
            }
        }

        private string CuerpoCorreo(string nombre, string enlace)
        {
            return $@"
<!DOCTYPE html>
<html lang='es'>
<head><meta charset='utf-8'></head>
<body style='margin:0;padding:0;font-family:Arial,sans-serif;background:#f9f9f9;'>
  <table width='100%' cellpadding='0' cellspacing='0'
         style='background:#f9f9f9;padding:40px 0;'>
    <tr><td align='center'>
      <table width='520' cellpadding='0' cellspacing='0'
             style='background:#fff;border-radius:16px;overflow:hidden;
                    box-shadow:0 4px 20px rgba(0,0,0,.08);'>

        <tr>
          <td style='background:#111;padding:28px 40px;text-align:center;'>
            <h1 style='color:#fff;margin:0;font-size:22px;font-weight:800;'>
              &#127873; Frijolito Jugueter&#237;a
            </h1>
          </td>
        </tr>

        <tr>
          <td style='padding:40px;'>
            <h2 style='font-size:20px;font-weight:800;color:#111;margin:0 0 12px;'>
              &#128274; Recupera tu contrase&#241;a
            </h2>
            <p style='color:#555;font-size:15px;line-height:1.7;margin:0 0 24px;'>
              Hola <strong>{nombre}</strong>,<br/>
              Recibimos una solicitud para restablecer la contrase&#241;a de tu cuenta.
              Haz clic en el bot&#243;n para crear una nueva:
            </p>
            <div style='text-align:center;margin:32px 0;'>
              <a href='{enlace}'
                 style='background:#111;color:#fff;text-decoration:none;
                        font-weight:700;font-size:15px;padding:14px 36px;
                        border-radius:50px;display:inline-block;'>
                Restablecer contrase&#241;a
              </a>
            </div>
            <p style='color:#999;font-size:13px;line-height:1.6;margin:0;'>
              Enlace v&#225;lido por <strong>2 horas</strong>.<br/>
              Si no solicitaste esto, ignora este mensaje.
            </p>
          </td>
        </tr>

        <tr>
          <td style='background:#f5f5f5;padding:20px 40px;
                     text-align:center;border-top:1px solid #eee;'>
            <p style='color:#aaa;font-size:12px;margin:0;'>
              &copy; {DateTime.Now.Year} Frijolito Jugueter&#237;a &mdash; Costa Rica
            </p>
          </td>
        </tr>

      </table>
    </td></tr>
  </table>
</body>
</html>";
        }
    }
}