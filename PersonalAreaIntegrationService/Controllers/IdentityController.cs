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
					issuer: "FamilPAIntegrationService",
					audience: "PAIntegrationUser",
					notBefore: now,
					claims: identity.Claims,
					expires: now.Add(TimeSpan.FromHours(24)),
					signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes("nr490jf390353hj9")), SecurityAlgorithms.HmacSha256));
			var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

			return Ok(encodedJwt);
		}

		private bool ValidateUser(IdentityViewModel request)
		{
            //CreateTableIfNotExists();
            var passwordHash = GetPasswordHash(request.Password);
			using (var conn = new NpgsqlConnection(DBProvider.GetConnectionString()))
			{
				conn.Open();
				return (Int64)new NpgsqlCommand(String.Format(@"Select COUNT(1) from ""public"".""User"" Where ""Login"" = '{0}' and ""Password"" = '{1}'", request.Login, passwordHash), conn).ExecuteScalar() > 0;
			}
		}

        private void CreateTableIfNotExists()
        {
            using (var conn = new NpgsqlConnection(DBProvider.GetConnectionString()))
            {
                conn.Open();
                var query =
                    @"CREATE TABLE IF NOT EXISTS public.""User"" (
                    ""Login"" text NULL,
                    ""Password"" text NULL
                    );

                    do $$ begin
                    if not exists(select 1 from ""User"") then
                        Insert into public.""User""(""Login"", ""Password"")
	                    VALUES('FamilGateService','5u5eGqJgifJL5mPA4RHGoyRMHKc0YkZBIkkKyjiWOoU=');
                    END IF;
                    END $$

                    ";
                new NpgsqlCommand(query, conn).ExecuteNonQuery();
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