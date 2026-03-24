using System;

namespace ProgramacionAvanzadaG8.Models
{
    public class CarritoItemModel
    {
        public int ProductoId { get; set; }
        public string SKU { get; set; }
        public string Nombre { get; set; }
        public string Imagen { get; set; }
        public decimal Precio { get; set; }
        public int Cantidad { get; set; }
        public int StockDisponible { get; set; }

        public decimal Subtotal
        {
            get { return Precio * Cantidad; }
        }
    }
}