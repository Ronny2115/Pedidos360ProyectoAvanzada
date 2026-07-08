using Microsoft.AspNetCore.Mvc;

namespace Pedidos360Proyecto.Controllers
{
    [Route("Errors")]
    public class ErrorsController : Controller
    {
        private readonly ILogger<ErrorsController> _logger;

        public ErrorsController(ILogger<ErrorsController> logger)
        {
            _logger = logger;
        }

        [Route("{statusCode:int}")]
        public IActionResult Index(int statusCode)
        {
            Response.StatusCode = statusCode;

            if (statusCode == 404)
            {
                _logger.LogWarning("Pagina no encontrada: {Path}", HttpContext.Request.Path);
                return View("Error404");
            }

            _logger.LogError("Error {StatusCode} al acceder a {Path}", statusCode, HttpContext.Request.Path);
            return View("Error500");
        }

        [Route("AccesoDenegado")]
        public IActionResult AccesoDenegado()
        {
            Response.StatusCode = 403;
            return View();
        }
    }
}
