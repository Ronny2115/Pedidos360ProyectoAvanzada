using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pedidos360Proyecto.Models.Api;
using Pedidos360Proyecto.Services;

namespace Pedidos360Proyecto.Controllers.Api
{
    [Authorize]
    [ApiController]
    [Route("api/pedidos")]
    public class PedidosApiController : ControllerBase
    {
        private readonly PedidoCalculoService _calculoService;

        public PedidosApiController(PedidoCalculoService calculoService)
        {
            _calculoService = calculoService;
        }

        [HttpPost("calcular")]
        public async Task<ActionResult<CalcularPedidoResponse>> Calcular([FromBody] CalcularPedidoRequest request)
        {
            var resultado = await _calculoService.CalcularAsync(request.Lineas);
            return Ok(resultado);
        }
    }
}
