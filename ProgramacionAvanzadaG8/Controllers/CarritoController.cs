using ProgramacionAvanzadaG8.EntityFramework;
using ProgramacionAvanzadaG8.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace ProgramacionAvanzadaG8.Controllers
{
    public class CarritoController : Controller
    {
        private FrijolitoEntities1 db = new FrijolitoEntities1();

        private List<CarritoItemModel> ObtenerCarrito()
        {
            if (Session["Carrito"] == null)
            {
                Session["Carrito"] = new List<CarritoItemModel>();
            }

            return (List<CarritoItemModel>)Session["Carrito"];
        }

        public ActionResult Index()
        {
            var carrito = ObtenerCarrito();
            return View(carrito);
        }

        [HttpPost]
        public ActionResult Agregar(int productoId, int cantidad = 1)
        {
            var carrito = ObtenerCarrito();

            var producto = db.Producto.FirstOrDefault(p => p.producto_id == productoId);

            if (producto == null)
            {
                TempData["Error"] = "El producto no existe.";
                return RedirectToAction("Tienda", "Home");
            }

            if (producto.stock <= 0)
            {
                TempData["Error"] = "El producto no tiene stock disponible.";
                return RedirectToAction("Tienda", "Home");
            }

            var itemExistente = carrito.FirstOrDefault(x => x.ProductoId == productoId);

            if (itemExistente != null)
            {
                if (itemExistente.Cantidad + cantidad > producto.stock)
                {
                    TempData["Error"] = "No puedes agregar más unidades que el stock disponible.";
                    return RedirectToAction("Tienda", "Home");
                }

                itemExistente.Cantidad += cantidad;
            }
            else
            {
                if (cantidad > producto.stock)
                {
                    TempData["Error"] = "La cantidad solicitada supera el stock disponible.";
                    return RedirectToAction("Tienda", "Home");
                }

                carrito.Add(new CarritoItemModel
                {
                    ProductoId = producto.producto_id,
                    SKU = producto.sku,
                    Nombre = producto.nombre,
                    Imagen = producto.imagen,
                    Precio = producto.precio,
                    Cantidad = cantidad,
                    StockDisponible = producto.stock
                });
            }

            Session["Carrito"] = carrito;
            TempData["Success"] = "Producto agregado al carrito.";

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Eliminar(int productoId)
        {
            var carrito = ObtenerCarrito();
            var item = carrito.FirstOrDefault(x => x.ProductoId == productoId);

            if (item != null)
            {
                carrito.Remove(item);
                Session["Carrito"] = carrito;
                TempData["Success"] = "Producto eliminado del carrito.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult ActualizarCantidad(int productoId, int cantidad)
        {
            var carrito = ObtenerCarrito();
            var item = carrito.FirstOrDefault(x => x.ProductoId == productoId);

            if (item == null)
            {
                TempData["Error"] = "Producto no encontrado en el carrito.";
                return RedirectToAction("Index");
            }

            var producto = db.Producto.FirstOrDefault(p => p.producto_id == productoId);

            if (producto == null)
            {
                TempData["Error"] = "El producto ya no existe.";
                return RedirectToAction("Index");
            }

            if (cantidad <= 0)
            {
                carrito.Remove(item);
                Session["Carrito"] = carrito;
                TempData["Success"] = "Producto eliminado del carrito.";
                return RedirectToAction("Index");
            }

            if (cantidad > producto.stock)
            {
                TempData["Error"] = "La cantidad supera el stock disponible.";
                return RedirectToAction("Index");
            }

            item.Cantidad = cantidad;
            item.StockDisponible = producto.stock;

            Session["Carrito"] = carrito;
            TempData["Success"] = "Cantidad actualizada correctamente.";

            return RedirectToAction("Index");
        }

        public ActionResult Checkout()
        {
            var carrito = ObtenerCarrito();

            if (carrito == null || !carrito.Any())
            {
                TempData["Error"] = "Tu carrito esta vacio.";
                return RedirectToAction("Index");
            }

            ViewBag.MetodosPago = db.Metodo_Pago
                .Select(m => new SelectListItem
                {
                    Value = m.metodo_pago_id.ToString(),
                    Text = m.nombre
                })
                .ToList();

            var model = new CheckoutViewModel
            {
                ItemsCarrito = carrito
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Checkout(CheckoutViewModel model)
        {
            var carrito = ObtenerCarrito();

            if (carrito == null || !carrito.Any())
            {
                TempData["Error"] = "Tu carrito esta vacio.";
                return RedirectToAction("Index");
            }

            ViewBag.MetodosPago = db.Metodo_Pago
                .Select(m => new SelectListItem
                {
                    Value = m.metodo_pago_id.ToString(),
                    Text = m.nombre
                })
                .ToList();

            model.ItemsCarrito = carrito;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            foreach (var item in carrito)
            {
                var productoDb = db.Producto.FirstOrDefault(p => p.producto_id == item.ProductoId);

                if (productoDb == null)
                {
                    TempData["Error"] = "Uno de los productos ya no existe.";
                    return RedirectToAction("Index");
                }

                if (item.Cantidad > productoDb.stock)
                {
                    TempData["Error"] = "No hay suficiente stock para el producto: " + productoDb.nombre;
                    return RedirectToAction("Index");
                }
            }

            decimal subtotal = carrito.Sum(x => x.Subtotal);
            decimal impuesto = 0;
            decimal descuento = 0;
            decimal total = subtotal + impuesto - descuento;

            using (var transaccion = db.Database.BeginTransaction())
            {
                try
                {
                    var cliente = new Cliente
                    {
                        identificacion = model.Identificacion,
                        nombre = model.NombreCliente,
                        apellido = model.ApellidoCliente,
                        telefono = model.Telefono,
                        email = model.Email,
                        direccion = model.Direccion,
                        fecha_registro = DateTime.Now
                    };

                    db.Cliente.Add(cliente);
                    db.SaveChanges();

                    int usuarioId = 1;
                    if (Session["UsuarioId"] != null)
                    {
                        usuarioId = (int)Session["UsuarioId"];
                    }

                    var venta = new Venta
                    {
                        fecha = DateTime.Now,
                        cliente_id = cliente.cliente_id,
                        usuario_id = usuarioId,
                        subtotal = subtotal,
                        impuesto = impuesto,
                        descuento = descuento,
                        total = total,
                        estado = "Pagada"
                    };

                    db.Venta.Add(venta);
                    db.SaveChanges();

                    foreach (var item in carrito)
                    {
                        var productoDb = db.Producto.FirstOrDefault(p => p.producto_id == item.ProductoId);

                        var detalle = new Detalle_Venta
                        {
                            venta_id = venta.venta_id,
                            producto_id = item.ProductoId,
                            cantidad = item.Cantidad,
                            precio_unitario = item.Precio,
                            descuento_linea = 0,
                            total_linea = item.Subtotal
                        };

                        db.Detalle_Venta.Add(detalle);

                        productoDb.stock -= item.Cantidad;
                    }

                    db.SaveChanges();

                    var pago = new Pago
                    {
                        venta_id = venta.venta_id,
                        metodo_pago_id = model.MetodoPagoId,
                        fecha_pago = DateTime.Now,
                        monto = total,
                        referencia = model.ReferenciaPago
                    };

                    db.Pago.Add(pago);
                    db.SaveChanges();

                    transaccion.Commit();

                    Session["Carrito"] = new List<CarritoItemModel>();
                    TempData["Success"] = "Compra realizada con exito. Numero de venta: " + venta.venta_id;

                    return RedirectToAction("Confirmacion", new { id = venta.venta_id });
                }
                catch
                {
                    transaccion.Rollback();
                    TempData["Error"] = "Ocurrio un error al procesar la compra.";
                    return View(model);
                }
            }
        }

        public ActionResult Confirmacion(int id)
        {
            ViewBag.VentaId = id;
            return View();
        }
    }
}