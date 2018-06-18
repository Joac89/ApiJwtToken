using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiJwtTokenExample.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ApiJwtTokenExample.Controllers
{
	[Produces("application/json")]
	[Route("api/v1/[controller]")]
	[EnableCors("*")]
	public class IndexController : Controller
    {
		private IConfiguration config;
		public IndexController(IConfiguration configuration)
		{
			config = configuration;
		}

		// GET: /<controller>/
		[Authorize]
		[HttpGet]
		[Route("validate")]
		[ValidateModel]
		public IActionResult Index()
        {
            var result = new ResponseBase<bool>() {
				Code = 200,
				Message = "Example API jwt Token"
			};

			return Ok(result);
        }
    }
}
