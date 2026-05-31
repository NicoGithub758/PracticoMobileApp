namespace PracticoMobileApp.Models
{
    public class PencaInstanciaMobile
    {
        public int PencaInstanciaId { get; set; }
        public int PencaId { get; set; }
        public string NombrePenca { get; set; }
        public string Deporte { get; set; }
        public int CantidadEquipos { get; set; }
        public decimal Costo { get; set; }
        public bool Finalizada { get; set; }

        public bool Participa { get; set; }
        public int? ParticipacionId { get; set; }
        public bool EstaPagado { get; set; }
        public int PuntajeTotal { get; set; }

        // --- Propiedades calculadas para la UI ---

        // Texto del estado para mostrar en la lista
        public string EstadoTexto
        {
            get
            {
                if (Finalizada) return "Finalizada";
                if (Participa && EstaPagado) return "Participando";
                if (Participa && !EstaPagado) return "Pago pendiente";
                return "Disponible";
            }
        }

        // Color del badge segun el estado
        public Color EstadoColor
        {
            get
            {
                if (Finalizada) return Colors.Gray;
                if (Participa && EstaPagado) return Color.FromArgb("#4CAF50");   // Verde
                if (Participa && !EstaPagado) return Color.FromArgb("#FF9800");  // Naranja
                return Color.FromArgb("#2196F3");                                // Azul
            }
        }

        // Muestra el boton de posiciones solo si participa y pago
        public bool PuedeVerPosiciones => Participa && EstaPagado;

        // Costo formateado
        public string CostoTexto => Costo > 0 ? $"USD {Costo:F2}" : "Gratis";
    }
}
