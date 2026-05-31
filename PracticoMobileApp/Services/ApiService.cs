using PracticoMobileApp.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace PracticoMobileApp.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        // En emulador Android: 10.0.2.2 apunta a localhost de la PC
        //private const string BaseUrl = "https://10.0.2.2:7230";
        private const string BaseUrl = "https://sincere-delight-production-006f.up.railway.app";

        public ApiService()
        {
#if DEBUG
            // En debug ignoramos el certificado SSL local
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (msg, cert, chain, errors) => true
            };
            _httpClient = new HttpClient(handler);
#else
            _httpClient = new HttpClient();
#endif
            _httpClient.BaseAddress = new Uri(BaseUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(15);
        }

        /// <summary>
        /// Obtiene la lista de sitios activos desde la API.
        /// </summary>
        public async Task<List<SitioDto>> GetSitiosAsync()
        {
            try
            {
                var sitios = await _httpClient.GetFromJsonAsync<List<SitioDto>>("/api/mobile/sitios");
                return sitios ?? new List<SitioDto>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API ERROR] GetSitios: {ex.Message}");
                return new List<SitioDto>();
            }
        }

        /// <summary>
        /// Login social con Google via Auth0. Devuelve el JWT propio de la plataforma.
        /// </summary>
        public async Task<LoginResponse?> LoginSocialAsync(string auth0Token, int sitioId)
        {
            try
            {
                var body = new { auth0Token, sitioId };
                var response = await _httpClient.PostAsJsonAsync("/api/mobile/auth/social", body);

                if (!response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"[API ERROR] LoginSocial: {response.StatusCode}");
                    return null;
                }

                return await response.Content.ReadFromJsonAsync<LoginResponse>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API ERROR] LoginSocial: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Registra el token FCM del dispositivo en la API.
        /// </summary>
        public async Task<bool> GuardarFcmTokenAsync(string fcmToken)
        {
            try
            {
                var jwt = await SecureStorage.GetAsync("jwt_token");
                if (string.IsNullOrEmpty(jwt)) return false;

                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);

                var body = new { fcmToken };
                var response = await _httpClient.PostAsJsonAsync("/api/mobile/auth/fcm-token", body);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API ERROR] GuardarFcmToken: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Obtiene la tabla de posiciones completa de una penca.
        /// </summary>
        public async Task<List<PosicionDTO>> ObtenerTablaPosicionesAsync(int pencaInstanciaId)
        {
            try
            {
                if (!await AgregarTokenAsync())
                    return new List<PosicionDTO>();

                var response = await _httpClient.GetAsync($"/api/posiciones/{pencaInstanciaId}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var posiciones = JsonSerializer.Deserialize<List<PosicionDTO>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return posiciones ?? new List<PosicionDTO>();
                }

                System.Diagnostics.Debug.WriteLine($"[Posiciones] Error: {response.StatusCode}");
                return new List<PosicionDTO>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Posiciones] Excepción: {ex.Message}");
                return new List<PosicionDTO>();
            }
        }

        /// <summary>
        /// Obtiene la posición y puntos del usuario actual.
        /// </summary>
        public async Task<MiPosicionDTO?> ObtenerMiPosicionAsync(int pencaInstanciaId)
        {
            try
            {
                if (!await AgregarTokenAsync())
                    return null;

                var response = await _httpClient.GetAsync($"/api/posiciones/{pencaInstanciaId}/mi-posicion");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var miPosicion = JsonSerializer.Deserialize<MiPosicionDTO>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return miPosicion;
                }

                System.Diagnostics.Debug.WriteLine($"[Mi Posición] Error: {response.StatusCode}");
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Mi Posición] Excepción: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Obtiene el top N de la tabla de posiciones.
        /// </summary>
        public async Task<List<PosicionDTO>> ObtenerTopPosicionesAsync(int pencaInstanciaId, int cantidad = 10)
        {
            try
            {
                if (!await AgregarTokenAsync())
                    return new List<PosicionDTO>();

                var response = await _httpClient.GetAsync($"/api/posiciones/{pencaInstanciaId}/top/{cantidad}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var posiciones = JsonSerializer.Deserialize<List<PosicionDTO>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return posiciones ?? new List<PosicionDTO>();
                }

                System.Diagnostics.Debug.WriteLine($"[Top Posiciones] Error: {response.StatusCode}");
                return new List<PosicionDTO>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Top Posiciones] Excepción: {ex.Message}");
                return new List<PosicionDTO>();
            }
        }

        /// <summary>
        /// Obtiene todas las pencas del sitio del usuario.
        /// El sitio se determina automaticamente por el JWT en la API.
        /// </summary>
        public async Task<List<PencaInstanciaMobile>> ObtenerPencasDelSitioAsync()
        {
            try
            {
                if (!await AgregarTokenAsync())
                    return new List<PencaInstanciaMobile>();

                var response = await _httpClient.GetAsync("/api/mobile/pencas");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var pencas = JsonSerializer.Deserialize<List<PencaInstanciaMobile>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return pencas ?? new List<PencaInstanciaMobile>();
                }

                System.Diagnostics.Debug.WriteLine($"[Pencas] Error: {response.StatusCode}");
                return new List<PencaInstanciaMobile>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Pencas] Excepcion: {ex.Message}");
                return new List<PencaInstanciaMobile>();
            }
        }

        /// <summary>
        /// Agrega el JWT guardado al header Authorization del HttpClient.
        /// Llamar antes de cualquier request que requiera autenticacion.
        /// </summary>
        private async Task<bool> AgregarTokenAsync()
        {
            var jwt = await SecureStorage.GetAsync("jwt_token");
            if (string.IsNullOrEmpty(jwt))
            {
                System.Diagnostics.Debug.WriteLine("[AUTH] No hay JWT guardado");
                return false;
            }

            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);
            return true;
        }

    }

    // DTOs del lado mobile
    public class SitioDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string? LogoUrl { get; set; }
        public string? ColorPrincipal { get; set; }
        public string TipoRegistro { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public string Jwt { get; set; } = string.Empty;
        public int UsuarioSitioId { get; set; }
        public int SitioId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
