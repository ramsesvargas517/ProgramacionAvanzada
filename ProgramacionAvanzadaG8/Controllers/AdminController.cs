using ProgramacionAvanzadaG8.EntityFramework;
using ProgramacionAvanzadaG8.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProgramacionAvanzadaG8.Controllers
{
    [AdminAuthorize]
    public class AdminController : Controller
    {
        // ----------------------------------------------------------------
        // Opciones de genero
        // ----------------------------------------------------------------
        private static readonly List<SelectListItem> OpcionesGenero = new List<SelectListItem>
        {
            new SelectListItem { Value = "Unisex", Text = "Unisex (Ninos y Ninas)" },
            new SelectListItem { Value = "Nino",   Text = "Ninos"                  },
            new SelectListItem { Value = "Nina",   Text = "Ninas"                  },
        };

        private FrijolitoEntities1 Db() => new FrijolitoEntities1();

        // ----------------------------------------------------------------
        // GuardarImagen — patrón del profe
        // ----------------------------------------------------------------
        private string GuardarImagen(HttpPostedFileBase file, string subcarpeta)
        {
            if (file == null || file.ContentLength == 0) return null;

            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var permitidas = new[] { ".jpg", ".jpeg", ".png" };

            if (!permitidas.Contains(extension))
                throw new InvalidOperationException("Formato no permitido. Solo JPG y PNG.");

            // Tamaño máximo 2 MB (coherente con la validación en el endpoint AJAX)
            if (file.ContentLength > 2 * 1024 * 1024)
                throw new InvalidOperationException("La imagen no puede superar 2 MB.");

            // Validar magic bytes para mayor seguridad
            if (!EsImagenValida(file))
                throw new InvalidOperationException("El archivo no es una imagen válida.");

            string fileName = Guid.NewGuid().ToString("N") + extension;
            string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Uploads", subcarpeta);

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            file.SaveAs(Path.Combine(folder, fileName));

            return "/Uploads/" + subcarpeta + "/" + fileName;
        }

        private void BorrarImagen(string rutaRelativa)
        {
            if (string.IsNullOrWhiteSpace(rutaRelativa)) return;
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                    rutaRelativa.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);
            }
            catch { }
        }

        // ================================================================
        // DASHBOARD
        // ================================================================

        [HttpGet]
        public ActionResult Index()
        {
            ViewBag.Title = "Dashboard";
            using (var db = Db())
            {
                ViewBag.TotalProductos = db.Producto.Count();
                ViewBag.TotalCategorias = db.Categoria.Count();
                ViewBag.TotalUsuarios = db.Usuario.Count();
                ViewBag.StockBajo = db.Producto.Count(p => p.stock <= 5);
                ViewBag.TotalNinos = ContarPorGenero(db, "Nino");
                ViewBag.TotalNinas = ContarPorGenero(db, "Nina");
                ViewBag.TotalUnisex = ContarPorGenero(db, "Unisex");
            }
            return View("~/Views/Admin/Index.cshtml");
        }

        private int ContarPorGenero(FrijolitoEntities1 db, string genero)
        {
            try
            {
                return db.Database
                    .SqlQuery<int>("SELECT COUNT(*) FROM Producto WHERE genero = @g",
                        new SqlParameter("@g", genero))
                    .FirstOrDefault();
            }
            catch { return 0; }
        }

        // ================================================================
        // PRODUCTOS — Listado
        // ================================================================

        [HttpGet]
        public ActionResult Productos(int? categoriaId, string genero)
        {
            ViewBag.Title = "Gestion de Productos";
            List<ProductoModel> lista;
            using (var db = Db())
            {
                lista = db.Database.SqlQuery<ProductoModel>(
                    @"SELECT p.producto_id  AS ProductoId,
                             p.sku          AS Sku,
                             p.nombre       AS Nombre,
                             p.descripcion  AS Descripcion,
                             p.precio       AS Precio,
                             p.stock        AS Stock,
                             p.categoria_id AS CategoriaId,
                             p.imagen       AS Imagen,
                             p.genero       AS Genero,
                             c.nombre       AS CategoriaNombre
                      FROM   Producto p
                      INNER JOIN Categoria c ON p.categoria_id = c.categoria_id
                      WHERE  (@cat IS NULL OR p.categoria_id = @cat)
                        AND  (@gen IS NULL OR p.genero = @gen)
                      ORDER BY p.nombre",
                    new SqlParameter("@cat", (object)categoriaId ?? DBNull.Value),
                    new SqlParameter("@gen", (object)genero ?? DBNull.Value)
                ).ToList();

                CargarFiltros(categoriaId, genero);
            }
            return View("~/Views/Admin/Productos.cshtml", lista);
        }

        // ================================================================
        // PRODUCTOS — Crear
        // ================================================================

        [HttpGet]
        public ActionResult CrearProducto()
        {
            ViewBag.Title = "Nuevo Producto";
            CargarCategorias();
            CargarGeneros();
            return View("~/Views/Admin/CrearProducto.cshtml", new ProductoModel { Genero = "Unisex" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CrearProducto(ProductoModel model, HttpPostedFileBase ImagenFile)
        {
            ViewBag.Title = "Nuevo Producto";

            if (!ModelState.IsValid)
            {
                CargarCategorias(model.CategoriaId);
                CargarGeneros(model.Genero);
                return View("~/Views/Admin/CrearProducto.cshtml", model);
            }

            string rutaImagen = null;
            try
            {
                if (ImagenFile != null && ImagenFile.ContentLength > 0)
                    rutaImagen = GuardarImagen(ImagenFile, "productos");

                using (var db = Db())
                {
                    var resultado = db.Database.SqlQuery<decimal>(
                        "EXEC InsertarProducto @Sku,@Nombre,@Descripcion,@Precio,@Stock,@CategoriaId,@Imagen,@Genero",
                        new SqlParameter("@Sku", model.Sku),
                        new SqlParameter("@Nombre", model.Nombre),
                        new SqlParameter("@Descripcion", (object)model.Descripcion ?? DBNull.Value),
                        new SqlParameter("@Precio", model.Precio),
                        new SqlParameter("@Stock", model.Stock),
                        new SqlParameter("@CategoriaId", model.CategoriaId),
                        new SqlParameter("@Imagen", (object)rutaImagen ?? DBNull.Value),
                        new SqlParameter("@Genero", model.Genero ?? "Unisex")
                    ).FirstOrDefault();

                    if (resultado == -1m)
                    {
                        ModelState.AddModelError("Sku", "Ya existe un producto con ese SKU.");
                        if (rutaImagen != null) BorrarImagen(rutaImagen);
                        CargarCategorias(model.CategoriaId);
                        CargarGeneros(model.Genero);
                        return View("~/Views/Admin/CrearProducto.cshtml", model);
                    }
                }

                TempData["Success"] = "Producto creado correctamente.";
                return RedirectToAction("Productos");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                CargarCategorias(model.CategoriaId);
                CargarGeneros(model.Genero);
                return View("~/Views/Admin/CrearProducto.cshtml", model);
            }
            catch
            {
                TempData["Error"] = "Ocurrio un error al guardar el producto.";
                CargarCategorias(model.CategoriaId);
                CargarGeneros(model.Genero);
                return View("~/Views/Admin/CrearProducto.cshtml", model);
            }
        }

        // ================================================================
        // PRODUCTOS — Editar
        // ================================================================

        [HttpGet]
        public ActionResult EditarProducto(int id)
        {
            ViewBag.Title = "Editar Producto";
            using (var db = Db())
            {
                var model = db.Database.SqlQuery<ProductoModel>(
                    @"SELECT p.producto_id  AS ProductoId,
                             p.sku          AS Sku,
                             p.nombre       AS Nombre,
                             p.descripcion  AS Descripcion,
                             p.precio       AS Precio,
                             p.stock        AS Stock,
                             p.categoria_id AS CategoriaId,
                             p.imagen       AS Imagen,
                             p.genero       AS Genero,
                             c.nombre       AS CategoriaNombre
                      FROM   Producto p
                      INNER JOIN Categoria c ON p.categoria_id = c.categoria_id
                      WHERE  p.producto_id = @id",
                    new SqlParameter("@id", id)
                ).FirstOrDefault();

                if (model == null)
                {
                    TempData["Error"] = "Producto no encontrado.";
                    return RedirectToAction("Productos");
                }

                CargarCategorias(model.CategoriaId);
                CargarGeneros(model.Genero);
                return View("~/Views/Admin/EditarProducto.cshtml", model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarProducto(ProductoModel model, HttpPostedFileBase ImagenFile)
        {
            ViewBag.Title = "Editar Producto";

            if (!ModelState.IsValid)
            {
                CargarCategorias(model.CategoriaId);
                CargarGeneros(model.Genero);
                return View("~/Views/Admin/EditarProducto.cshtml", model);
            }

            try
            {
                string rutaImagenNueva = null;
                if (ImagenFile != null && ImagenFile.ContentLength > 0)
                {
                    if (!string.IsNullOrEmpty(model.Imagen))
                        BorrarImagen(model.Imagen);
                    rutaImagenNueva = GuardarImagen(ImagenFile, "productos");
                }

                using (var db = Db())
                {
                    db.Database.ExecuteSqlCommand(
                        "EXEC ActualizarProducto @ProductoId,@Sku,@Nombre,@Descripcion,@Precio,@Stock,@CategoriaId,@Imagen,@Genero",
                        new SqlParameter("@ProductoId", model.ProductoId),
                        new SqlParameter("@Sku", model.Sku),
                        new SqlParameter("@Nombre", model.Nombre),
                        new SqlParameter("@Descripcion", (object)model.Descripcion ?? DBNull.Value),
                        new SqlParameter("@Precio", model.Precio),
                        new SqlParameter("@Stock", model.Stock),
                        new SqlParameter("@CategoriaId", model.CategoriaId),
                        new SqlParameter("@Imagen", (object)rutaImagenNueva ?? DBNull.Value),
                        new SqlParameter("@Genero", model.Genero ?? "Unisex")
                    );
                }

                TempData["Success"] = "Producto actualizado correctamente.";
                return RedirectToAction("Productos");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                CargarCategorias(model.CategoriaId);
                CargarGeneros(model.Genero);
                return View("~/Views/Admin/EditarProducto.cshtml", model);
            }
            catch
            {
                TempData["Error"] = "Ocurrio un error al actualizar.";
                CargarCategorias(model.CategoriaId);
                CargarGeneros(model.Genero);
                return View("~/Views/Admin/EditarProducto.cshtml", model);
            }
        }

        // ================================================================
        // PRODUCTOS — Eliminar
        // ================================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EliminarProducto(int id)
        {
            try
            {
                using (var db = Db())
                {
                    var img = ObtenerImagenProducto(db, id);
                    db.Database.ExecuteSqlCommand(
                        "EXEC EliminarProducto @ProductoId",
                        new SqlParameter("@ProductoId", id));
                    if (img != null) BorrarImagen(img);
                }
                TempData["Success"] = "Producto eliminado.";
            }
            catch { TempData["Error"] = "No se pudo eliminar el producto."; }

            return RedirectToAction("Productos");
        }

        // ================================================================
        // SUBIR IMAGEN — AJAX endpoint
        // POST /Admin/SubirImagen
        // Retorna JSON: { ok: true, ruta: "..." } o { ok: false, mensaje: "..." }
        // ================================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubirImagen(HttpPostedFileBase imagen)
        {
            if (imagen == null || imagen.ContentLength == 0)
                return Json(new { ok = false, mensaje = "No se recibi\u00f3 ning\u00fan archivo." });

            // Validar tipo MIME
            var tiposPermitidos = new[] { "image/jpeg", "image/jpg", "image/png" };
            if (!tiposPermitidos.Contains(imagen.ContentType.ToLower()))
                return Json(new { ok = false, mensaje = "Solo se permiten archivos JPG y PNG." });

            // Validar extensi\u00f3n
            var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png" };
            var ext = Path.GetExtension(imagen.FileName).ToLower();
            if (!extensionesPermitidas.Contains(ext))
                return Json(new { ok = false, mensaje = "Extensi\u00f3n no permitida. Use .jpg o .png" });

            // Validar tama\u00f1o m\u00e1ximo 2 MB
            const int maxBytes = 2 * 1024 * 1024;
            if (imagen.ContentLength > maxBytes)
                return Json(new { ok = false, mensaje = "El archivo supera el m\u00e1ximo de 2 MB." });

            // Validar magic bytes (cabecera real del archivo)
            if (!EsImagenValida(imagen))
                return Json(new { ok = false, mensaje = "El archivo no es una imagen v\u00e1lida." });

            try
            {
                // Reusar GuardarImagen que ya existe en este controller
                string ruta = GuardarImagen(imagen, "productos");
                return Json(new { ok = true, ruta = ruta });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { ok = false, mensaje = ex.Message });
            }
            catch
            {
                return Json(new { ok = false, mensaje = "Error al guardar. Int\u00e9ntalo de nuevo." });
            }
        }

        // Helper: verificar magic bytes de JPG y PNG
        private bool EsImagenValida(HttpPostedFileBase archivo)
        {
            var buffer = new byte[8];
            archivo.InputStream.Position = 0;
            int leidos = archivo.InputStream.Read(buffer, 0, buffer.Length);
            archivo.InputStream.Position = 0;

            if (leidos < 4) return false;

            bool esJpg = buffer[0] == 0xFF && buffer[1] == 0xD8 && buffer[2] == 0xFF;
            bool esPng = leidos >= 8 &&
                         buffer[0] == 0x89 && buffer[1] == 0x50 &&
                         buffer[2] == 0x4E && buffer[3] == 0x47 &&
                         buffer[4] == 0x0D && buffer[5] == 0x0A &&
                         buffer[6] == 0x1A && buffer[7] == 0x0A;

            return esJpg || esPng;
        }

        // ================================================================
        // CATEGORIAS
        // ================================================================

        [HttpGet]
        public ActionResult Categorias()
        {
            ViewBag.Title = "Gestion de Categorias";
            using (var db = Db())
            {
                var lista = db.ObtenerCategorias().Select(c => new CategoriaModel
                {
                    CategoriaId = c.categoria_id,
                    Nombre = c.nombre,
                    Descripcion = c.descripcion,
                    Imagen = ObtenerImagenCategoria(db, c.categoria_id)
                }).ToList();
                return View("~/Views/Admin/Categorias.cshtml", lista);
            }
        }

        [HttpGet]
        public ActionResult CrearCategoria()
        {
            ViewBag.Title = "Nueva Categoria";
            return View("~/Views/Admin/CrearCategoria.cshtml", new CategoriaModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CrearCategoria(CategoriaModel model, HttpPostedFileBase ImagenFile)
        {
            ViewBag.Title = "Nueva Categoria";
            if (!ModelState.IsValid)
                return View("~/Views/Admin/CrearCategoria.cshtml", model);
            try
            {
                string ruta = null;
                if (ImagenFile != null && ImagenFile.ContentLength > 0)
                    ruta = GuardarImagen(ImagenFile, "categorias");

                using (var db = Db())
                    db.Database.ExecuteSqlCommand(
                        "EXEC InsertarCategoria @Nombre,@Descripcion,@Imagen",
                        new SqlParameter("@Nombre", model.Nombre),
                        new SqlParameter("@Descripcion", (object)model.Descripcion ?? DBNull.Value),
                        new SqlParameter("@Imagen", (object)ruta ?? DBNull.Value));

                TempData["Success"] = "Categoria creada.";
                return RedirectToAction("Categorias");
            }
            catch (InvalidOperationException ex) { ModelState.AddModelError("", ex.Message); }
            catch { TempData["Error"] = "Error al crear la categoria."; }
            return View("~/Views/Admin/CrearCategoria.cshtml", model);
        }

        [HttpGet]
        public ActionResult EditarCategoria(int id)
        {
            ViewBag.Title = "Editar Categoria";
            using (var db = Db())
            {
                var cat = db.Categoria.Find(id);
                if (cat == null)
                {
                    TempData["Error"] = "Categoria no encontrada.";
                    return RedirectToAction("Categorias");
                }
                var model = new CategoriaModel
                {
                    CategoriaId = cat.categoria_id,
                    Nombre = cat.nombre,
                    Descripcion = cat.descripcion,
                    Imagen = ObtenerImagenCategoria(db, id)
                };
                return View("~/Views/Admin/EditarCategoria.cshtml", model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarCategoria(CategoriaModel model, HttpPostedFileBase ImagenFile)
        {
            ViewBag.Title = "Editar Categoria";
            if (!ModelState.IsValid)
                return View("~/Views/Admin/EditarCategoria.cshtml", model);
            try
            {
                string ruta = null;
                if (ImagenFile != null && ImagenFile.ContentLength > 0)
                {
                    if (!string.IsNullOrEmpty(model.Imagen)) BorrarImagen(model.Imagen);
                    ruta = GuardarImagen(ImagenFile, "categorias");
                }
                using (var db = Db())
                    db.Database.ExecuteSqlCommand(
                        "EXEC ActualizarCategoria @CategoriaId,@Nombre,@Descripcion,@Imagen",
                        new SqlParameter("@CategoriaId", model.CategoriaId),
                        new SqlParameter("@Nombre", model.Nombre),
                        new SqlParameter("@Descripcion", (object)model.Descripcion ?? DBNull.Value),
                        new SqlParameter("@Imagen", (object)ruta ?? DBNull.Value));

                TempData["Success"] = "Categoria actualizada.";
                return RedirectToAction("Categorias");
            }
            catch (InvalidOperationException ex) { ModelState.AddModelError("", ex.Message); }
            catch { TempData["Error"] = "Error al actualizar la categoria."; }
            return View("~/Views/Admin/EditarCategoria.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EliminarCategoria(int id)
        {
            try
            {
                using (var db = Db())
                {
                    var r = db.Database
                        .SqlQuery<EliminarCategoriaResult>("EXEC EliminarCategoria @CategoriaId",
                            new SqlParameter("@CategoriaId", id))
                        .FirstOrDefault();
                    if (r != null && r.FilasAfectadas == -1)
                    {
                        TempData["Error"] = "No se puede eliminar: tiene productos asociados.";
                        return RedirectToAction("Categorias");
                    }
                }
                TempData["Success"] = "Categoria eliminada.";
            }
            catch { TempData["Error"] = "No se pudo eliminar la categoria."; }
            return RedirectToAction("Categorias");
        }

        // ================================================================
        // USUARIOS
        // ================================================================

        [HttpGet]
        public ActionResult Usuarios()
        {
            ViewBag.Title = "Usuarios Registrados";
            List<UsuarioAdminModel> lista;
            using (var db = Db())
                lista = db.Database.SqlQuery<UsuarioAdminModel>("EXEC ObtenerUsuarios").ToList();
            return View("~/Views/Admin/Usuarios.cshtml", lista);
        }

        // ================================================================
        // HELPERS PRIVADOS
        // ================================================================

        private void CargarCategorias(int? sel = null)
        {
            using (var db = Db())
            {
                var cats = db.ObtenerCategorias()
                    .Select(c => new CategoriaModel
                    {
                        CategoriaId = c.categoria_id,
                        Nombre = c.nombre
                    }).ToList();
                ViewBag.Categorias = new SelectList(cats, "CategoriaId", "Nombre", sel);
            }
        }

        private void CargarGeneros(string sel = null)
        {
            ViewBag.Generos = OpcionesGenero.Select(o => new SelectListItem
            {
                Value = o.Value,
                Text = o.Text,
                Selected = o.Value == sel
            }).ToList();
        }

        private void CargarFiltros(int? catSel, string genSel)
        {
            using (var db = Db())
            {
                var cats = db.ObtenerCategorias()
                    .Select(c => new CategoriaModel
                    {
                        CategoriaId = c.categoria_id,
                        Nombre = c.nombre
                    }).ToList();
                ViewBag.Categorias = new SelectList(cats, "CategoriaId", "Nombre", catSel);
            }

            var gensFilter = new List<SelectListItem>
            {
                new SelectListItem { Value = "",       Text = "Todos"  },
                new SelectListItem { Value = "Nino",   Text = "Ninos"  },
                new SelectListItem { Value = "Nina",   Text = "Ninas"  },
                new SelectListItem { Value = "Unisex", Text = "Unisex" },
            };
            ViewBag.GenerosFilter = new SelectList(gensFilter, "Value", "Text", genSel ?? "");
            ViewBag.GeneroFiltro = genSel;
            ViewBag.CategoriaFiltro = catSel;
        }

        private string ObtenerImagenProducto(FrijolitoEntities1 db, int id)
        {
            try
            {
                return db.Database.SqlQuery<string>(
                    "SELECT imagen FROM Producto WHERE producto_id=@id",
                    new SqlParameter("@id", id)).FirstOrDefault();
            }
            catch { return null; }
        }

        private string ObtenerImagenCategoria(FrijolitoEntities1 db, int id)
        {
            try
            {
                return db.Database.SqlQuery<string>(
                    "SELECT imagen FROM Categoria WHERE categoria_id=@id",
                    new SqlParameter("@id", id)).FirstOrDefault();
            }
            catch { return null; }
        }

    } // ← FIN de la clase AdminController

    // ====================================================================
    // Clases auxiliares (fuera de AdminController, dentro del namespace)
    // ====================================================================

    public class EliminarCategoriaResult
    {
        public int FilasAfectadas { get; set; }
        public string Mensaje { get; set; }
    }

    public class UsuarioAdminModel
    {
        public int UsuarioId { get; set; }
        public string Username { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public string Rol { get; set; }
    }

    public class AdminAuthorizeAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var rol = filterContext.HttpContext.Session["RolAdmin"] as string;
            if (string.IsNullOrEmpty(rol) || rol != "Administrador")
                filterContext.Result = new RedirectResult(
                    "/Account/Login?returnUrl=" +
                    Uri.EscapeDataString(filterContext.HttpContext.Request.RawUrl));
            base.OnActionExecuting(filterContext);
        }
    }

} 
