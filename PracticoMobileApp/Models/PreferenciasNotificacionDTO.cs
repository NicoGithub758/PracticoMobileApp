namespace PracticoMobileApp.Models
{
    public class PreferenciasNotificacionDTO
    {
        public bool RecibirResultados { get; set; } = true;
        public bool RecibirPartidos { get; set; } = true;
        public bool RecibirGenerales { get; set; } = true;
        public bool RecibirRanking { get; set; } = true;
    }
}
