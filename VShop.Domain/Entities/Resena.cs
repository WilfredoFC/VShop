using System;
using System.Collections.Generic;
using System.Text;

namespace VShop.Domain.Entities
{
    public class Resena
    {
        public int Id { get; set; }
        public required int ProductoId { get; set; }
        public required string UsuarioId { get; set; }
        public int Calificacion { get; set; } // 1-5
        public string Comentario { get; set; }
        public DateTime FechaResena { get; set; } = DateTime.UtcNow;
        public bool EsAprobado { get; set; } = false;

        // Propiedades de navegación
        public virtual Producto? Producto { get; set; }
    }
}