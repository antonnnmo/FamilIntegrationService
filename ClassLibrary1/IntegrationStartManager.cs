using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.Core;

namespace Terrasoft.Configuration
{
	public class IntegrationStartManager
	{
		private string _uri;

		public IntegrationStartManager(UserConnection uc)
		{
			_uri = Terrasoft.Core.Configuration.SysSettings.GetValue(uc, "GateIntegrationService", String.Empty);
		}

		public void StartContact(bool isPrimary)
		{
			if (isPrimary)
			{
				Request("Main/Primary");
			}
			else
			{
				Request("Main");
			}
		}

		public void StartProductSize(bool isPrimary)
		{
			if (isPrimary)
			{
				Request("Main/PrimaryProductSize");
			}
			else
			{
				Request("Main/ProductSize");
			}
		}

		public void StartProductTag(bool isPrimary)
		{
			if (isPrimary)
			{
				Request("Main/PrimaryProductTag");
			}
			else
			{
				Request("Main/ProductTag");
			}
		}

		public void StartContactTag(bool isPrimary)
		{
			if (isPrimary)
			{
				Request("Main/PrimaryContactTag");
			}
			else
			{
				Request("Main/ContactTag");
			}
		}

		public void StartProductCategory(bool isPrimary)
		{
			if (isPrimary)
			{
				Request("Main/PrimaryProductCategory");
			}
			else
			{
				Request("Main/ProductCategory");
			}
		}

		public void StartProductSubCategory(bool isPrimary)
		{
			if (isPrimary)
			{
				Request("Main/PrimaryProductSubCategory");
			}
			else
			{
				Request("Main/ProductSubCategory");
			}
		}

		public void StartBrandType(bool isPrimary)
		{
			if (isPrimary)
			{
				Request("Main/PrimaryBrandType");
			}
			else
			{
				Request("Main/BrandType");
			}
		}

		public void StartProductGroup(bool isPrimary)
		{
			if (isPrimary)
			{
				Request("Main/PrimaryProductGroup");
			}
			else
			{
				Request("Main/ProductGroup");
			}
		}

		public void StartSMS(bool isPrimary)
		{
			if (isPrimary)
			{
				Request("Main/PrimarySMS");
			}
			else
			{
				Request("Main/SMS");
			}
		}

		private string Request(string method)
		{
			var req = (HttpWebRequest)WebRequest.Create(String.Format("{0}/api/{1}", _uri, method));
			req.Method = "GET";
			req.Timeout = 10 * 1000 * 60;

			try
			{
				using (var response = req.GetResponse())
				{
					using (var responseStream = response.GetResponseStream())
					{
						using (var streamReader = new StreamReader(responseStream))
						{
							return streamReader.ReadToEnd();
						}
					}
				}
			}
			catch (WebException e)
			{
				using (var streamReader = new StreamReader(e.Response.GetResponseStream()))
				{
					var res = streamReader.ReadToEnd();
					throw new Exception(e.Message + " " + res);
				}
			}
		}
	}
}
