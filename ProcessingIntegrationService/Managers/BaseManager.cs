using FamilIntegrationCore.Models;
using FamilIntegrationService.Models;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessingIntegrationService.Managers
{
    public abstract class BaseManager : ControllerBase
    {
        public ActionResult LoadPrimaryCardPack(IEnumerable<BaseProcessingModel> models)
        {
            if (models == null) return BadRequest("Ошибка передачи аргументов");
            try
            {
                using (var conn = new NpgsqlConnection(DBProvider.GetConnectionString()))
                {
                    conn.Open();

                    // Insert some data
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = GetPrimaryQuery(models);
                        cmd.ExecuteNonQuery();
                    }
                }

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        protected abstract string GetPrimaryQuery(IEnumerable<BaseProcessingModel> models);
        protected abstract string GetQuery(BaseProcessingModel model);

        public ActionResult LoadCardPack(IEnumerable<BaseProcessingModel> models)
        {
            if (models == null) return BadRequest("Ошибка передачи аргументов");
            var result = new List<PackResult>();

            using (var conn = new NpgsqlConnection(DBProvider.GetConnectionString()))
            {
                conn.Open();

                foreach (var m in models)
                {
                    try
                    {
                        using (var cmd = new NpgsqlCommand())
                        {
                            cmd.Connection = conn;
                            cmd.CommandText = GetQuery(m);
                            cmd.ExecuteNonQuery();
                        }

                        result.Add(new PackResult() { IsSuccess = true, Id = m.ERPId });
                    }
                    catch (Exception e)
                    {
                        result.Add(new PackResult() { IsSuccess = false, ErrorMessage = e.Message, Id = m.ERPId });
                    }
                }
            }

            return Ok(result);
        }
    }
}
