using PracticoMobileApp.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace PracticoMobileApp.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        // En emulador Android: 10.0.2.2 apunta a localhost de la PC
        private const string BaseUrl = "https://10.0.2.2:7230";
        //private const string BaseUrl = "https://sincere-delight-production-006f.up.railway.app";

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

        // ====================================================================
        //  ENDPOINTS PUBLICOS (no requieren JWT)
        // ====================================================================

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
        /// Login interno con email + password.
        /// Devuelve (response, mensajeError). Si response != null, login OK.
        /// </summary>
        public async Task<(MobileAuthResponse? response, string? error)> LoginInternoAsync(
            string email, string password, int sitioId)
        {
            try
            {
                var body = new { email, password, sitioId };
                var response = await _httpClient.PostAsJsonAsync("/api/mobile/auth/login", body);

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<MobileAuthResponse>();
                    return (data, null);
                }

                var errorMsg = await LeerMensajeError(response);
                return (null, errorMsg);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API ERROR] LoginInterno: {ex.Message}");
                return (null, "No se pudo conectar con el servidor.");
            }
        }

        /// <summary>
        /// Registra un nuevo usuario.
        /// Si el sitio es Abierta, devuelve JWT y EstadoSolicitud="Activo".
        /// Si el sitio requiere aprobacion/invitacion, devuelve JWT vacio y EstadoSolicitud="Pendiente".
        /// </summary>
        public async Task<(MobileAuthResponse? response, string? error)> RegistrarAsync(
            string nombre, string email, string password, int sitioId, string? tokenInvitacion = null)
        {
            try
            {
                var body = new { nombre, email, password, sitioId, tokenInvitacion };
                var response = await _httpClient.PostAsJsonAsync("/api/mobile/auth/register", body);

                // 200 (Activo) o 202 (Pendiente) son ambos exitosos
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<MobileAuthResponse>();
                    return (data, null);
                }

                var errorMsg = await LeerMensajeError(response);
                return (null, errorMsg);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API ERROR] Registrar: {ex.Message}");
                return (null, "No se pudo conectar con el servidor.");
            }
        }

        /// <summary>
        /// Login social con Google via Auth0. Devuelve el JWT propio de la plataforma.
        /// </summary>
        public async Task<(MobileAuthResponse? response, string? error)> LoginSocialAsync(
            string auth0Token, int sitioId)
        {
            try
            {
                var body = new { auth0Token, sitioId };
                var response = await _httpClient.PostAsJsonAsync("/api/mobile/auth/social", body);

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<MobileAuthResponse>();
                    return (data, null);
                }

                var errorMsg = await LeerMensajeError(response);
                return (null, errorMsg);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API ERROR] LoginSocial: {ex.Message}");
                return (null, "No se pudo conectar con el servidor.");
            }
        }

        // ====================================================================
        //  ENDPOINTS AUTENTICADOS (requieren JWT)
        // ====================================================================

        /// <summary>
        /// Registra el token FCM del dispositivo en la API.
        /// </summary>
        public async Task<bool> GuardarFcmTokenAsync(string fcmToken)
        {
            try
            {
                if (!await AgregarTokenAsync()) return false;

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
        /// Limpia el token FCM del usuario al cerrar sesion.
        /// </summary>
        public async Task<bool> LimpiarFcmTokenAsync()
        {
            try
            {
                if (!await AgregarTokenAsync()) return false;
                var response = await _httpClient.DeleteAsync("/api/mobile/auth/fcm-token");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API ERROR] LimpiarFcmToken: {ex.Message}");
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

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Mi Posición] Excepción: {ex.Message}");
                return null;
            }
        }

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

                return new List<PosicionDTO>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Top Posiciones] Excepción: {ex.Message}");
                return new List<PosicionDTO>();
            }
        }

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

                return new List<PencaInstanciaMobile>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Pencas] Excepcion: {ex.Message}");
                return new List<PencaInstanciaMobile>();
            }
        }

        /// <summary>
        /// Obtiene las preferencias de notificaciones del usuario actual.
        /// Si nunca fueron configuradas, la API devuelve los defaults (todos true).
        /// </summary>
        public async Task<PreferenciasNotificacionDTO?> ObtenerPreferenciasAsync()
        {
            try
            {
                if (!await AgregarTokenAsync()) return null;

                var response = await _httpClient.GetAsync("/api/mobile/preferencias/notificaciones");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<PreferenciasNotificacionDTO>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }

                System.Diagnostics.Debug.WriteLine($"[Preferencias] Error: {response.StatusCode}");
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Preferencias] Excepcion: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Guarda las preferencias de notificaciones del usuario actual.
        /// </summary>
        public async Task<bool> GuardarPreferenciasAsync(PreferenciasNotificacionDTO prefs)
        {
            try
            {
                if (!await AgregarTokenAsync()) return false;

                var response = await _httpClient.PutAsJsonAsync("/api/mobile/preferencias/notificaciones", prefs);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Preferencias] Error al guardar: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Obtiene los partidos de una penca con la prediccion del usuario (si tiene).
        /// Llama al endpoint del compañero. Devuelve TODOS los partidos (jugados y no jugados).
        /// El llamador filtra segun necesite.
        /// </summary>
        public async Task<List<PartidoConPrediccionMobile>> ObtenerPartidosYPrediccionesAsync(int participacionId)
        {
            try
            {
                if (!await AgregarTokenAsync())
                    return new List<PartidoConPrediccionMobile>();

                var response = await _httpClient.GetAsync($"/api/predicciones/partidos?idParticipacion={participacionId}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var partidos = JsonSerializer.Deserialize<List<PartidoConPrediccionMobile>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return partidos ?? new List<PartidoConPrediccionMobile>();
                }

                System.Diagnostics.Debug.WriteLine($"[Predicciones] Error GET: {response.StatusCode}");
                return new List<PartidoConPrediccionMobile>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Predicciones] Excepcion GET: {ex.Message}");
                return new List<PartidoConPrediccionMobile>();
            }
        }

        /// <summary>
        /// Crea o actualiza una prediccion. El endpoint hace upsert.
        /// - Si prediccionId == 0: crea nueva
        /// - Si prediccionId > 0: actualiza la existente
        /// El API devuelve solo 200 OK sin body, asi que devolvemos (true, null) en exito.
        /// </summary>
        public async Task<(bool exito, string? error)> CrearOActualizarPrediccionAsync(
            int prediccionId,  // 0 si es nueva, > 0 si es modificacion
            int participacionId,
            int partidoId,
            int golesLocal,
            int golesVisitante)
        {
            try
            {
                if (!await AgregarTokenAsync())
                    return (false, "No hay sesion activa.");

                var body = new CrearPrediccionApiRequest
                {
                    Id = prediccionId,
                    ParticipacionId = participacionId,
                    PartidoId = partidoId,
                    GolesEquipoLocal = golesLocal,
                    GolesEquipoVisitante = golesVisitante
                };

                var response = await _httpClient.PostAsJsonAsync("/api/predicciones/create", body);

                if (response.IsSuccessStatusCode)
                    return (true, null);

                // El endpoint devuelve BadRequest con un string simple, no un objeto.
                var errorContent = await response.Content.ReadAsStringAsync();
                // Limpiar comillas si vienen
                errorContent = errorContent.Trim('"');
                return (false, string.IsNullOrEmpty(errorContent) ? $"Error {(int)response.StatusCode}" : errorContent);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Predicciones] Excepcion POST: {ex.Message}");
                return (false, "No se pudo conectar con el servidor.");
            }
        }

        /// <summary>
        /// Crea una orden de pago en PayPal a traves del backend.
        /// Devuelve la respuesta con el OrderId, PagoId y la ApprovalUrl que tiene que cargar el WebView.
        /// </summary>
        public async Task<(CrearPagoResponseDto? response, string? error)> CrearOrdenPagoAsync(int pencaInstanciaId)
        {
            try
            {
                if (!await AgregarTokenAsync())
                    return (null, "No hay sesion activa.");

                var body = new CrearPagoRequestDto
                {
                    PencaInstanciaId = pencaInstanciaId
                };

                var response = await _httpClient.PostAsJsonAsync("/api/pagos/crear-orden", body);

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<CrearPagoResponseDto>();
                    if (data == null || string.IsNullOrEmpty(data.ApprovalUrl))
                        return (null, "El servidor no devolvio una URL valida de PayPal.");

                    return (data, null);
                }

                var errorMsg = await LeerMensajeError(response);
                return (null, errorMsg);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Pagos] Excepcion CrearOrden: {ex.Message}");
                return (null, "No se pudo conectar con el servidor.");
            }
        }

        /// <summary>
        /// Confirma un pago que el usuario ya aprobo en PayPal.
        /// Llama a /api/pagos/confirmar para que el backend capture la orden.
        /// </summary>
        public async Task<(bool exito, string? error)> ConfirmarPagoAsync(int pagoId, string orderId)
        {
            try
            {
                if (!await AgregarTokenAsync())
                    return (false, "No hay sesion activa.");

                var body = new ConfirmarPagoRequestDto
                {
                    PagoId = pagoId,
                    OrderId = orderId
                };

                var response = await _httpClient.PostAsJsonAsync("/api/pagos/confirmar", body);

                if (response.IsSuccessStatusCode)
                    return (true, null);

                var errorMsg = await LeerMensajeError(response);
                return (false, errorMsg);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Pagos] Excepcion ConfirmarPago: {ex.Message}");
                return (false, "No se pudo conectar con el servidor.");
            }
        }

        // ====================================================================
        //  HELPERS
        // ====================================================================

        /// <summary>
        /// Agrega el JWT guardado al header Authorization del HttpClient.
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

        /// <summary>
        /// Intenta extraer el campo "mensaje" del body de respuesta de error.
        /// </summary>
        private async Task<string> LeerMensajeError(HttpResponseMessage response)
        {
            try
            {
                var content = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(content);
                if (doc.RootElement.TryGetProperty("mensaje", out var mensaje))
                    return mensaje.GetString() ?? "Error desconocido.";
            }
            catch { }
            return $"Error {(int)response.StatusCode}.";
        }
    }

    // ========================================================================
    //  DTOs del lado mobile
    // ========================================================================

    public class SitioDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string? LogoUrl { get; set; }
        public string? ColorPrincipal { get; set; }
        public string TipoRegistro { get; set; } = string.Empty;
    }

    /// <summary>
    /// Respuesta unificada de los endpoints de autenticacion mobile.
    /// </summary>
    public class MobileAuthResponse
    {
        public string Jwt { get; set; } = string.Empty;
        public int UsuarioSitioId { get; set; }
        public int SitioId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        /// <summary>"Activo" (login OK con JWT) o "Pendiente" (esperando aprobacion).</summary>
        public string EstadoSolicitud { get; set; } = "Activo";

        /// <summary>Mensaje opcional para mostrar al usuario.</summary>
        public string? Mensaje { get; set; }
    }

    // Alias por compatibilidad con codigo viejo
    public class LoginResponse : MobileAuthResponse { }
}

