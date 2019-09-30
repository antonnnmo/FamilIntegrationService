using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Npgsql;

namespace ProcessingIntegrationService.Controllers
{
    [Route("api/Identity")]
    [ApiController]
    public class IdentityController : ControllerBase
    {
		[Route("token")]
		[HttpPost]
		public async Task<IActionResult> Token([FromBody]IdentityViewModel request)
		{
			//Добавить проверку срока жизни PIN-кода
			if (!ValidateUser(request))
			{
				return Unauthorized();
			}

			var claims = new List<Claim>
			{
				new Claim(ClaimsIdentity.DefaultNameClaimType, request.Login),
			};
			var identity = new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);

			var now = DateTime.UtcNow;
			var jwt = new JwtSecurityToken(
					issuer: "FamilIntegrationService",
					audience: "IntegrationUser",
					notBefore: now,
					claims: identity.Claims,
					expires: now.Add(TimeSpan.FromHours(24)),
					signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes("gfdiog40-]kgf-043uo")), SecurityAlgorithms.HmacSha256));
			var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

			return Ok(encodedJwt);
		}

		private bool ValidateUser(IdentityViewModel request)
		{
			var passwordHash = GetPasswordHash(request.Password);
			using (var conn = new NpgsqlConnection(DBProvider.GetConnectionString()))
			{
				conn.Open();
				return (Int64)new NpgsqlCommand(String.Format(@"Select COUNT(1) from ""public"".""User"" Where ""Login"" = '{0}' and ""Password"" = '{1}'", request.Login, passwordHash), conn).ExecuteScalar() > 0;
			}
		}

		private string GetPasswordHash(string password)
		{
			using (SHA256 mySHA256 = SHA256.Create())
			{
				var hash = mySHA256.ComputeHash(Encoding.UTF8.GetBytes(password));
				return Convert.ToBase64String(hash);
			}
		}
	}
}