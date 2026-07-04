namespace PracticoMobileApp.Models
{
    /// <summary>
    /// Estructura que devuelve GET /api/predicciones/partidos.
    /// Cada partido viene con la prediccion del usuario (si tiene) embebida.
    /// </summary>
    public class PartidoConPrediccionMobile
    {
        public int Id { get; set; }  // PartidoId
        public EquipoMobile Local { get; set; } = new();
        public EquipoMobile Visitante { get; set; } = new();
        public DateTime Fecha { get; set; }
        public int? GolesLocal { get; set; }
        public int? GolesVisitante { get; set; }
        public bool Jugado { get; set; }
        public PrediccionMobile? Prediccion { get; set; }

        // --- Propiedades calculadas para binding en la UI ---

        public string FechaTexto => Fecha.ToString("dd/MM/yyyy HH:mm");

        public string EstadoPrediccion => Prediccion != null
            ? $"Tu predicción: {Prediccion.GolesEquipoLocal}-{Prediccion.GolesEquipoVisitante}"
            : "Sin predicción";

        public Color EstadoColor => Prediccion != null
            ? Color.FromArgb("#4CAF50")
            : Color.FromArgb("#FF9800");

        public bool PuedePredecir => !Jugado && Fecha > DateTime.Now;

        // Inputs editables
        public string GolesLocalInput { get; set; } = "";
        public string GolesVisitanteInput { get; set; } = "";
    }

    public class EquipoMobile
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
    }

    public class PrediccionMobile
    {
        public int Id { get; set; }
        public int GolesEquipoLocal { get; set; }
        public int GolesEquipoVisitante { get; set; }
        public int PuntosObtenidos { get; set; }
    }

    /// <summary>
    /// Body que se manda al POST /api/predicciones/create.
    /// Coincide con PrediccionDTO de la API.
    /// </summary>
    public class CrearPrediccionApiRequest
    {
        public int Id { get; set; }  // 0 si es nueva, > 0 si es modificacion
        public int ParticipacionId { get; set; }
        public int PartidoId { get; set; }
        public int GolesEquipoLocal { get; set; }
        public int GolesEquipoVisitante { get; set; }
    }
}
