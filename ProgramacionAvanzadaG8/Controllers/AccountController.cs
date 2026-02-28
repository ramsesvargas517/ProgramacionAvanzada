using ProgramacionAvanzadaG8.EntityFramework;
using ProgramacionAvanzadaG8.Models;
using System.Linq;
using System.Web.Mvc;

namespace ProgramacionAvanzadaG8.Controllers
{
    public class AccountController : Controller
    {

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
            using (var context = new FrijolitoEntities1())
            {
                // Llama al stored procedure IniciarSesion
                // Recibe: username y password_hash
                var result = context.IniciarSesion(modelo.Username, modelo.PasswordHash).FirstOrDefault();

                if (result == null)
                {
                    ViewBag.Mensaje = "Su información no se autenticó correctamente.";
                    return View();
                }

                Session["Nombre"]    = result.nombre;
                Session["Apellido"]  = result.apellido;
                Session["UsuarioId"] = result.usuario_id;

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
            using (var context = new FrijolitoEntities1())
            {
                // Llama al stored procedure RegistrarUsuario
                // Recibe: username, password_hash, nombre, apellido, email
                // Retorna: filas afectadas (> 0 = éxito)
                var result = context.RegistrarUsuario(
                    modelo.Username,
                    modelo.PasswordHash,
                    modelo.Nombre,
                    modelo.Apellido,
                    modelo.Email
                ).FirstOrDefault();

                if (result == null || result <= 0)
                {
                    ViewBag.Mensaje = "Su información no se registró correctamente. Es posible que el usuario ya exista.";
                    return View();
                }

                return RedirectToAction("Login", "Account");
            }
        }

        #endregion

        #region Recuperar Contraseña

        // GET: /Account/RecuperarContrasenna
        [HttpGet]
        public ActionResult RecuperarContrasenna()
        {
            return View();
        }

        // POST: /Account/RecuperarContrasenna
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RecuperarContrasenna(UsuarioModel modelo)
        {
            // TODO: Implementar lógica de recuperación (envío de correo, token, etc.)
            ViewBag.Mensaje = "Si el correo existe en nuestro sistema, recibirá instrucciones en breve.";
            return View();
        }

        #endregion

        #region Cerrar Sesión

        // GET: /Account/Logout
        [HttpGet]
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Index", "Home");
        }

        #endregion
    }
}
