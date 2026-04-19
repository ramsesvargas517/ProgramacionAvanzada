using ProgramacionAvanzadaG8.EntityFramework;
using ProgramacionAvanzadaG8.Models;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

namespace ProgramacionAvanzadaG8.Controllers
{
    public class HomeController : Controller
    {
        private FrijolitoEntities1 Db() => new FrijolitoEntities1();

        // GET: /  o  /Home/Index
        [HttpGet]
        public ActionResult Index(string q)
        {
            using (var db = Db())
            {
                // Categorias para el menu desplegable del nav (una vez)
                var categoriasList = db.ObtenerCategorias()
                    .Select(c => new CategoriaModel
                    {
                        CategoriaId = c.categoria_id,
                        Nombre = c.nombre
                    }).ToList();

                if (!string.IsNullOrEmpty(q))
                {
                    // Buscar en nombre, descripcion o categoria (usar comodines en el parámetro)
                    var qParam = new SqlParameter("@q", "%" + q + "%");
                    var productos = db.Database.SqlQuery<ProductoModel>(
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
                          WHERE  p.stock > 0
                            AND  (p.nombre LIKE @q OR p.descripcion LIKE @q OR c.nombre LIKE @q)
                          ORDER BY p.nombre",
                        qParam
                    ).ToList();

                    ViewBag.Productos = productos;
                    ViewBag.Categorias = categoriasList;
                    ViewBag.Title = "Resultados de búsqueda";
                    ViewBag.Query = q;

                    return View("Tienda");
                }

                // Productos destacados para el home (todos, max 8)
                var destacados = db.Database.SqlQuery<ProductoModel>(
                    @"SELECT TOP 8
                             p.producto_id  AS ProductoId,
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
                      WHERE  p.stock > 0
                      ORDER BY p.producto_id DESC"
                ).ToList();

                ViewBag.Productos = destacados;
                ViewBag.Categorias = categoriasList;
            }

            return View();
        }

        // GET: /Home/Ninos — productos para ninos + unisex
        [HttpGet]
        public ActionResult Ninos(int? categoriaId)
        {
            return VistaGenero("Nino", categoriaId);
        }

        // GET: /Home/Ninas — productos para ninas + unisex
        [HttpGet]
        public ActionResult Ninas(int? categoriaId)
        {
            return VistaGenero("Nina", categoriaId);
        }

        // GET: /Home/Tienda — todos los productos con filtros
        [HttpGet]
        public ActionResult Tienda(int? categoriaId, string genero)
        {
            using (var db = Db())
            {
                var productos = db.Database.SqlQuery<ProductoModel>(
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
                        AND  (@gen IS NULL OR p.genero = @gen OR p.genero = 'Unisex')
                        AND  p.stock > 0
                      ORDER BY p.nombre",
                    new SqlParameter("@cat", (object)categoriaId ?? System.DBNull.Value),
                    new SqlParameter("@gen", (object)genero ?? System.DBNull.Value)
                ).ToList();

                var categorias = db.ObtenerCategorias()
                    .Select(c => new CategoriaModel { CategoriaId = c.categoria_id, Nombre = c.nombre })
                    .ToList();

                ViewBag.Productos = productos;
                ViewBag.Categorias = categorias;
                ViewBag.GeneroActivo = genero;
                ViewBag.CatActiva = categoriaId;
                ViewBag.Title = "Tienda";
            }

            return View("Tienda");
        }

        // Accion compartida para Ninos/Ninas
        private ActionResult VistaGenero(string genero, int? categoriaId)
        {
            using (var db = Db())
            {
                var productos = db.Database.SqlQuery<ProductoModel>(
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
                      WHERE  (p.genero = @gen OR p.genero = 'Unisex')
                        AND  (@cat IS NULL OR p.categoria_id = @cat)
                        AND  p.stock > 0
                      ORDER BY p.nombre",
                    new SqlParameter("@gen", genero),
                    new SqlParameter("@cat", (object)categoriaId ?? System.DBNull.Value)
                ).ToList();

                var categorias = db.ObtenerCategorias()
                    .Select(c => new CategoriaModel { CategoriaId = c.categoria_id, Nombre = c.nombre })
                    .ToList();

                ViewBag.Productos = productos;
                ViewBag.Categorias = categorias;
                ViewBag.GeneroActivo = genero;
                ViewBag.CatActiva = categoriaId;
                ViewBag.Title = genero == "Nino" ? "Juguetes para Ninos" : "Juguetes para Ninas";
            }

            return View("Tienda");
        }
    }
}
