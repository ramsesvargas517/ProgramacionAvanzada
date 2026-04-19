using System.Web.Mvc;

namespace ProgramacionAvanzadaG8.Controllers
{
    public class PaginasController : Controller
    {
        public ActionResult NuestroBlog()
        {
            ViewBag.Titulo = "Nuestro Blog";
            return View();
        }

        public ActionResult PoliticaPrivacidad()
        {
            ViewBag.Titulo = "Política de Privacidad";
            return View();
        }

        public ActionResult Contactenos()
        {
            ViewBag.Titulo = "Contáctenos";
            return View();
        }

        public ActionResult Ayuda()
        {
            ViewBag.Titulo = "Ayuda";
            return View();
        }

        public ActionResult Comunidad()
        {
            ViewBag.Titulo = "Comunidad";
            return View();
        }

        public ActionResult Historia()
        {
            ViewBag.Titulo = "Historia";
            ViewBag.Encabezado = "Historia";
            ViewBag.Contenido = "Aquí puedes conocer más sobre la historia de la empresa, sus inicios y su evolución.";
            return View("PaginaBase");
        }

        public ActionResult NuestroEquipo()
        {
            ViewBag.Titulo = "Nuestro Equipo";
            ViewBag.Encabezado = "Nuestro Equipo";
            ViewBag.Contenido = "Conoce a las personas que forman parte del equipo y hacen posible el funcionamiento de la empresa.";
            return View("PaginaBase");
        }

        public ActionResult Servicios()
        {
            ViewBag.Titulo = "Servicios";
            ViewBag.Encabezado = "Servicios";
            ViewBag.Contenido = "En esta vista se muestran los principales servicios que ofrecemos a nuestros clientes.";
            return View("PaginaBase");
        }

        public ActionResult Empresa()
        {
            ViewBag.Titulo = "Empresa";
            ViewBag.Encabezado = "Empresa";
            ViewBag.Contenido = "Información general sobre la empresa, su misión, visión y enfoque comercial.";
            return View("PaginaBase");
        }

        public ActionResult Mayoreo()
        {
            ViewBag.Titulo = "Mayoreo";
            ViewBag.Encabezado = "Mayoreo";
            ViewBag.Contenido = "Sección orientada a compras al por mayor y relaciones comerciales con distribuidores.";
            return View("PaginaBase");
        }

        public ActionResult Menudeo()
        {
            ViewBag.Titulo = "Menudeo";
            ViewBag.Encabezado = "Menudeo";
            ViewBag.Contenido = "Vista enfocada en ventas al detalle para clientes individuales.";
            return View("PaginaBase");
        }

        public ActionResult SobreNosotros()
        {
            ViewBag.Titulo = "Sobre Nosotros";
            ViewBag.Encabezado = "Sobre Nosotros";
            ViewBag.Contenido = "Conoce más sobre nosotros, nuestra identidad y lo que ofrecemos.";
            return View("PaginaBase");
        }

        public ActionResult Vehiculos()
        {
            ViewBag.Titulo = "Vehículos";
            ViewBag.Encabezado = "Vehículos";
            ViewBag.Contenido = "En esta página se muestran los productos o categorías relacionadas con vehículos.";
            return View("PaginaBase");
        }

        public ActionResult Superheroes()
        {
            ViewBag.Titulo = "Superhéroes";
            ViewBag.Encabezado = "Superhéroes";
            ViewBag.Contenido = "Página dedicada a productos y categorías inspiradas en superhéroes.";
            return View("PaginaBase");
        }
    }
}