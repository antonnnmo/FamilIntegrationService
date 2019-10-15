using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilIntegrationCore.Models;

namespace ProcessingIntegrationService.Managers
{
    public class CardManager : BaseManager
    {
        protected override string GetPrimaryQuery(IEnumerable<BaseProcessingModel> models)
        {
            var cards = models.Select(m => (CardProcessingModel)m);
            var sb = new StringBuilder();
            sb.AppendLine(@"INSERT INTO ""public"".""Card"" (""Id"", ""Number"", ""State"", ""IsMain"", ""ContactId"") VALUES ");

            sb.AppendLine(String.Join(",", cards.Select(c => String.Format(@"('{0}', '{1}', '{2}', '{3}', {4})",
                c.CardId != "" ? c.CardId : c.Id.ToString(), (c.Number ?? "").Replace("'", "''"), c.State, c.IsMain,
                c.ContactId != "" ? string.Format("'{0}'", c.ContactId) : "null"))));
            return sb.ToString();
        }

        protected override string GetQuery(BaseProcessingModel model)
        {
            var c = (CardProcessingModel)model;
            return string.Format( 
                @"
                do $$ begin
                if (select 1 from ""Card"" where ""Id""='{0}') then
                    UPDATE ""public"".""Card"" SET ""Number"" = '{1}', ""State"" = '{2}', ""IsMain"" = '{3}', ""ContactId"" = {4} WHERE ""Id"" = '{0}';
                ELSE
                    INSERT INTO ""public"".""Card"" (""Id"", ""Number"", ""State"", ""IsMain"", ""ContactId"") VALUES ('{0}', '{1}', '{2}', '{3}', {4});
                END IF;
                END $$
                ", 
                c.CardId != "" ? c.CardId : c.Id.ToString(), (c.Number ?? "").Replace("'", "''"), c.State, c.IsMain,
                c.ContactId != "" ? string.Format("'{0}'", c.ContactId) : "null"
            );
        }
    }
}
