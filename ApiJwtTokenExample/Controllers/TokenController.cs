using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ApiJwtTokenExample.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ApiJwtTokenExample.Controllers
{
	[Produces("application/json")]
	[Route("api/[controller]")]
    public class TokenController : Controller
    {
		private IConfiguration config;

		public TokenController(IConfiguration configuration)
		{
			config = configuration;
		}
		
		[AllowAnonymous]
		[HttpPost]
		[ValidateModel]
		public async Task<IActionResult> GetToken([FromBody] AuthEntity data)
		{
			var result = new ResponseBase<string>();
			var userAuth = new ResponseBase<bool>();

			//Escriba aquí la implementación para validar usuario y contraseña de acceso
			userAuth = await new ExampleDao().SimulateLogin(data.UserName, data.Password);

			if (userAuth.Code == 200 && userAuth.Data)
			{
				var claims = new[]
				{
					new Claim(JwtRegisteredClaimNames.Sub, data.UserName),
					new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
				};

				var token = new JwtSecurityToken
				(
					issuer: config["token:issuer"],
					audience: config["token:audience"],
					claims: claims,
					expires: DateTime.UtcNow.AddHours(double.Parse(config["token:expire"])),
					notBefore: DateTime.UtcNow,
					signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["token:signingkey"])), SecurityAlgorithms.HmacSha256)
				);

				result.Code = (int)HttpStatusCode.OK;
				result.Data = new JwtSecurityTokenHandler().WriteToken(token);
				result.Message = userAuth.Message;

				return Ok(result);
			}
			else
			{
				result.Code = (int)HttpStatusCode.Unauthorized;
				result.Message = userAuth.Message;
				result.Data = "";

				return StatusCode(result.Code, result);
			}
		}
	}
}
