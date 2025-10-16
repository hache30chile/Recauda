namespace Recauda.Models
{
    public class VRecaudador
    {
        public int Id { get; set; }
        public bool rec_activo { get; set; }
        public int usu_id { get; set; }
        public int com_id { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string Nombre { get; set; }
        public string com_nombre { get; set; }
    }
}
