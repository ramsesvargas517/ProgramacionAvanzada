using ProgramacionAvanzadaG8.EntityFramework;
using ProgramacionAvanzadaG8.Models;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;

namespace ProgramacionAvanzadaG8.Controllers
{
    public class AccountController : Controller
    {
        // ----------------------------------------------------------------
        // Helper: genera SHA256 en MAYÚSCULAS sin guiones
        // Igual que SQL: CONVERT(VARCHAR(256), HASHBYTES('SHA2_256', '...'), 2)
        // ----------------------------------------------------------------
        private static string HashSHA256(string texto)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(texto);
                var hash = sha.ComputeHash(bytes);
                // BitConverter → "AB-CD-EF..." → quitar guiones → "ABCDEF..."
                return BitConverter.ToString(hash).Replace("-", "");
            }
        }

        #region Iniciar Sesión

        // GET: /Account/Login
        [HttpGet]
        public ActionResult Login()
        {
            if (Session["Nombre"] != null)
                return RedirectToAction("Index", "Home");

            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(UsuarioModel modelo)
        {
            // PasswordHash en el modelo llega como texto plano desde el form
            // → hay que hashearlo antes de enviarlo al SP
            var hashContrasenna = HashSHA256(modelo.PasswordHash);

            using (var context = new FrijolitoEntities1())
            {
                var result = context.IniciarSesion(modelo.Username, hashContrasenna)
                                    .FirstOrDefault();

                if (result == null)
                {
                    ViewBag.Mensaje = "Su información no se autenticó correctamente.";
                    return View();
                }

                // Sesión general (tienda)
                Session["Nombre"] = result.nombre;
                Session["Apellido"] = result.apellido;
                Session["UsuarioId"] = result.usuario_id;


                var rol = ObtenerRolDeResult(result);
                if (!string.IsNullOrEmpty(rol))
                {
                    Session["RolAdmin"] = rol;
                    Session["NombreAdmin"] = result.nombre;
                }

                // Redirigir según rol
                if (rol == "Administrador")
                    return RedirectToAction("Index", "Admin");

                return RedirectToAction("Index", "Home");
            }
        }

        #endregion

        #region Registrar Usuario

        // GET: /Account/Registro
        [HttpGet]
        public ActionResult Registro()
        {
            return View();
        }

        // POST: /Account/Registro
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registro(UsuarioModel modelo)
        {

            var hashContrasenna = HashSHA256(modelo.PasswordHash);

            using (var context = new FrijolitoEntities1())
            {
                var result = context.RegistrarUsuario(
                    modelo.Username,
                    hashContrasenna,   
                    modelo.Nombre,
                    modelo.Apellido,
                    modelo.Email
                ).FirstOrDefault();

                if (result == null || result <= 0)
                {
                    ViewBag.Mensaje = "No se pudo registrar. Es posible que el usuario ya exista.";
                    return View();
                }

                return RedirectToAction("Login", "Account");
            }
        }

        #endregion

        #region Recuperar Contraseña

        [HttpGet]
        public ActionResult RecuperarContrasenna()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RecuperarContrasenna(UsuarioModel modelo)
        {
            ViewBag.Mensaje = "Si el correo existe en nuestro sistema, recibirá instrucciones en breve.";
            return View();
        }

        #endregion

        #region Cerrar Sesión

        [HttpGet]
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Index", "Home");
        }

        #endregion

        
        private string ObtenerRolDeResult(IniciarSesion_Result result)
        {

            try
            {
                using (var ctx = new FrijolitoEntities1())
                {
                    return ctx.Database
                        .SqlQuery<string>(
                            @"SELECT r.nombre
                              FROM Usuario u
                              INNER JOIN Rol r ON u.rol_id = r.rol_id
                              WHERE u.usuario_id = @id",
                            new System.Data.SqlClient.SqlParameter("@id", result.usuario_id))
                        .FirstOrDefault();
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
