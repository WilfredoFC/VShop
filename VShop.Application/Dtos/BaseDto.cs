namespace VShop.Application.Dtos
{
    public abstract class BaseDto
    {
        public int Id { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;
        public bool EsActivo { get; set; } = true;
    }
}
