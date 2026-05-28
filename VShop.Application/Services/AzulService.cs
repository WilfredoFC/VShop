using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using VShop.Application.Dtos.Pago;
using VShop.Application.Dtos.Pedido;
using VShop.Application.Interfaces;

namespace VShop.Application.Services
{
    public class AzulService : IAzulService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AzulService> _logger;

        public AzulService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<AzulService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<AzulSolicitudResponse> SolicitarTokenAsync(PedidoDto pedido)
        {
            var client = _httpClientFactory.CreateClient("AzulClient");
            // Configurar autenticación (certificados o headers)
            // Ejemplo con headers:
            client.DefaultRequestHeaders.Add("Auth1", _configuration["Azul:Auth1"]);
            client.DefaultRequestHeaders.Add("Auth2", _configuration["Azul:Auth2"]);

            var request = new
            {
                Channel = "EC",
                Store = _configuration["Azul:Store"],
                PosInputMode = "E-Commerce",
                TrxType = "Sale",
                Amount = pedido.Total, // Asegurar que sea en centavos si Azul lo requiere
                Itbis = pedido.Impuestos,
                CurrencyPosCode = "DOP",
                Payments = "1",
                Plan = "0",
                OrderNumber = pedido.NumeroPedido,
                CustomOrderId = pedido.NumeroPedido,
                ECommerceUrl = _configuration["App:BaseUrl"],
                UrlPost = _configuration["Azul:UrlPost"] // Ej: https://tusitio.com/Pago/Notificacion
            };

            try
            {
                var response = await client.PostAsJsonAsync("Default.aspx", request);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadFromJsonAsync<AzulApiResponse>();
                    // Suponiendo que Azul devuelve algo como { Token: "...", Url: "..." }
                    if (content != null && !string.IsNullOrEmpty(content.Token))
                    {
                        return new AzulSolicitudResponse
                        {
                            Success = true,
                            Token = content.Token,
                            Url = content.Url
                        };
                    }
                    else
                    {
                        _logger.LogError("Respuesta de Azul sin token: {0}", content);
                        return new AzulSolicitudResponse { Success = false, ErrorMessage = "Respuesta inválida de Azul" };
                    }
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error al llamar a Azul: {0} - {1}", response.StatusCode, error);
                    return new AzulSolicitudResponse { Success = false, ErrorMessage = "Error de comunicación con Azul" };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción en SolicitarTokenAsync");
                return new AzulSolicitudResponse { Success = false, ErrorMessage = ex.Message };
            }
        }

        public AzulNotificacionResult ProcesarNotificacion(IFormCollection formData)
        {
            // Extraer campos que Azul envía
            var orderNumber = formData["OrderNumber"].ToString();
            var authCode = formData["AuthorizationCode"].ToString();
            var responseCode = formData["ResponseCode"].ToString(); // ej. "ISO8583"
            var isoCode = formData["IsoCode"].ToString(); // "00" éxito
            var errorDesc = formData["ErrorDescription"].ToString();

            return new AzulNotificacionResult
            {
                OrderNumber = orderNumber,
                AuthorizationCode = authCode,
                ResponseCode = responseCode,
                IsoCode = isoCode,
                ErrorDescription = errorDesc
            };
        }

        public Task<bool> ValidarNotificacionAsync(IFormCollection formData)
        {
            // Aquí implementar validación de firma si Azul la provee
            // Por ahora retornamos true
            return Task.FromResult(true);
        }
    }
}
