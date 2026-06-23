using System.Text.Json.Serialization;

namespace PracticoMobileApp.Models
{
    /// <summary>
    /// Request para crear una orden de pago.
    /// </summary>
    public class CrearPagoRequestDto
    {
        [JsonPropertyName("pencaInstanciaId")]
        public int PencaInstanciaId { get; set; }
    }

    /// <summary>
    /// Response de la API cuando se crea una orden.
    /// Contiene la URL que el WebView va a cargar.
    /// </summary>
    public class CrearPagoResponseDto
    {
        [JsonPropertyName("orderId")]
        public string OrderId { get; set; } = string.Empty;

        [JsonPropertyName("pagoId")]
        public int PagoId { get; set; }

        [JsonPropertyName("approvalUrl")]
        public string ApprovalUrl { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request para confirmar el pago una vez aprobado en PayPal.
    /// </summary>
    public class ConfirmarPagoRequestDto
    {
        [JsonPropertyName("pagoId")]
        public int PagoId { get; set; }

        [JsonPropertyName("orderId")]
        public string OrderId { get; set; } = string.Empty;
    }
}
