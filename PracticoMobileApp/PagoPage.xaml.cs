using PracticoMobileApp.Models;
using PracticoMobileApp.Services;

namespace PracticoMobileApp;

public partial class PagoPage : ContentPage
{
    private readonly ApiService _apiService;
    private readonly int _pencaInstanciaId;
    private readonly string _nombrePenca;
    private readonly decimal _costo;

    // Datos del pago en curso
    private string _orderId = string.Empty;
    private int _pagoId = 0;

    // URLs que indican que el flujo de pago termino.
    // PayPal redirige a estas URLs (configuradas en PayPalService.cs / ApplicationContext).
    // Son URLs "fantasma" que no existen realmente, el WebView las intercepta antes de cargarlas.
    private const string URL_EXITO = "https://pencauy.com/pago-exitoso";
    private const string URL_CANCELADO = "https://pencauy.com/pago-cancelado";

    // Bandera para evitar doble confirmacion
    private bool _yaConfirmado = false;

    public PagoPage(int pencaInstanciaId, string nombrePenca, decimal costo)
    {
        InitializeComponent();
        _apiService = new ApiService();
        _pencaInstanciaId = pencaInstanciaId;
        _nombrePenca = nombrePenca;
        _costo = costo;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CrearOrdenYAbrirWebView();
    }

    /// <summary>
    /// Llama al API para crear la orden de PayPal y abre el WebView con la ApprovalUrl.
    /// </summary>
    private async Task CrearOrdenYAbrirWebView()
    {
        LoadingLabel.Text = $"Preparando pago de USD {_costo:F2}...";

        var (response, error) = await _apiService.CrearOrdenPagoAsync(_pencaInstanciaId);

        if (response == null)
        {
            MostrarError("No se pudo crear el pago", error ?? "Error desconocido.");
            return;
        }

        // Guardar OrderId y PagoId para confirmar despues
        _orderId = response.OrderId;
        _pagoId = response.PagoId;

        // Cargar la URL de PayPal en el WebView
        PayPalWebView.Source = response.ApprovalUrl;
        PayPalWebView.IsVisible = true;
        LoadingStack.IsVisible = false;
    }

    /// <summary>
    /// Se ejecuta CADA vez que el WebView navega a una nueva URL.
    /// Detectamos las URLs de redireccion de PayPal para saber el resultado.
    /// </summary>
    private async void OnNavigating(object? sender, WebNavigatingEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"[PagoPage] Navigating to: {e.Url}");

        // CASO 1: PayPal redirige a la URL de exito
        if (e.Url.StartsWith(URL_EXITO, StringComparison.OrdinalIgnoreCase))
        {
            // Cancelamos la navegacion (no vamos a cargar una URL falsa)
            e.Cancel = true;

            if (_yaConfirmado) return;
            _yaConfirmado = true;

            // Ocultar WebView y mostrar overlay
            PayPalWebView.IsVisible = false;
            ConfirmandoOverlay.IsVisible = true;

            await ConfirmarPagoConApi();
            return;
        }

        // CASO 2: Usuario cancelo el pago en PayPal
        if (e.Url.StartsWith(URL_CANCELADO, StringComparison.OrdinalIgnoreCase))
        {
            e.Cancel = true;

            PayPalWebView.IsVisible = false;
            MostrarError("Pago cancelado",
                "Cancelaste el pago. Podes volver a intentarlo cuando quieras.");
        }
    }

    /// <summary>
    /// Llama al endpoint /api/pagos/confirmar para que el backend capture el pago.
    /// </summary>
    private async Task ConfirmarPagoConApi()
    {
        var (exito, error) = await _apiService.ConfirmarPagoAsync(_pagoId, _orderId);

        if (!exito)
        {
            ConfirmandoOverlay.IsVisible = false;
            MostrarError("Error confirmando el pago",
                $"El pago se aprobo en PayPal pero no se pudo confirmar.\nContactá al admin.\n\nDetalle: {error}");
            return;
        }

        // ¡Exito!
        ConfirmandoOverlay.IsVisible = false;

        await DisplayAlert(
            "✅ ¡Pago exitoso!",
            $"Ya formás parte de la penca \"{_nombrePenca}\".\n\nAhora podés hacer tus predicciones.",
            "Continuar"
        );

        // Volver a la pagina anterior (DetallePencaPage)
        await Navigation.PopAsync();
    }

    /// <summary>
    /// Muestra la pantalla de error.
    /// </summary>
    private void MostrarError(string titulo, string mensaje)
    {
        LoadingStack.IsVisible = false;
        PayPalWebView.IsVisible = false;
        ConfirmandoOverlay.IsVisible = false;

        ErrorTituloLabel.Text = titulo;
        ErrorMensajeLabel.Text = mensaje;
        ErrorStack.IsVisible = true;
    }

    /// <summary>
    /// Boton "Volver" en la pantalla de error.
    /// </summary>
    private async void OnVolverClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
