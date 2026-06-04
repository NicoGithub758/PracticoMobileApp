namespace PracticoMobileApp;

[QueryProperty(nameof(Email), "email")]
[QueryProperty(nameof(SitioNombre), "sitioNombre")]
public partial class SolicitudPendientePage : ContentPage
{
    public string Email { get; set; } = string.Empty;
    public string SitioNombre { get; set; } = string.Empty;

    public SolicitudPendientePage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        var emailDecodificado = string.IsNullOrEmpty(Email) ? "" : Uri.UnescapeDataString(Email);
        var sitioDecodificado = string.IsNullOrEmpty(SitioNombre) ? "" : Uri.UnescapeDataString(SitioNombre);

        if (!string.IsNullOrEmpty(emailDecodificado))
        {
            MensajeLabel.Text = $"Enviamos tu solicitud para {emailDecodificado}" +
                              (string.IsNullOrEmpty(sitioDecodificado) ? "." : $" en {sitioDecodificado}.");
        }
    }

    /// <summary>
    /// El usuario quiere volver a intentar loguear (por si ya lo aprobaron).
    /// </summary>
    private async void OnIntentarLoginTapped(object sender, TappedEventArgs e)
    {
        // Volver a SitiosPage. El usuario elige sitio y prueba login interno.
        await Shell.Current.GoToAsync("//SitiosPage");
    }

    private async void OnVolverSitiosTapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("//SitiosPage");
    }
}
