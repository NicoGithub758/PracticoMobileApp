namespace PracticoMobileApp;

[QueryProperty(nameof(Email), "email")]
[QueryProperty(nameof(SitioNombre), "sitioNombre")]
[QueryProperty(nameof(SitioLogo), "sitioLogo")]
public partial class SolicitudPendientePage : ContentPage
{
    public string Email { get; set; } = string.Empty;
    public string SitioNombre { get; set; } = string.Empty;
    public string SitioLogo { get; set; } = string.Empty;

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

        AplicarLogoSitio();
    }

    /// <summary>
    /// Si recibimos un LogoUrl valido, lo usamos. Si no, dejamos el icono PencaUY por defecto.
    /// </summary>
    private void AplicarLogoSitio()
    {
        try
        {
            if (string.IsNullOrEmpty(SitioLogo)) return;

            var logoUrl = Uri.UnescapeDataString(SitioLogo);
            if (string.IsNullOrWhiteSpace(logoUrl)) return;

            if (Uri.TryCreate(logoUrl, UriKind.Absolute, out var uri))
            {
                SitioLogoImage.Source = ImageSource.FromUri(uri);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Logo] Error cargando logo del sitio: {ex.Message}");
        }
    }

    /// <summary>
    /// El usuario quiere volver a intentar loguear (por si ya lo aprobaron).
    /// </summary>
    private async void OnIntentarLoginTapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("//SitiosPage");
    }

    private async void OnVolverSitiosTapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("//SitiosPage");
    }
}
