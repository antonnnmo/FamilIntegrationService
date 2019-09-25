namespace Terrasoft.Configuration
{
	using System.ServiceModel;
	using System.ServiceModel.Web;
	using System.ServiceModel.Activation;
	using Terrasoft.Web.Common;
	using System.Collections.Generic;
	using System.Runtime.Serialization;
	using Terrasoft.Core.DB;
	using System.Linq;

	[DataContract]
	public class CalcProductRetailPriceRequest
	{
		[DataMember]
		public List<string> ProductCodes { get; set; }
	}

	[ServiceContract]
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
	public class GateIntegrationService : BaseService
	{
		[OperationContract]
		[WebInvoke(Method = "POST", UriTemplate = "CalcProductRetailPrice", BodyStyle = WebMessageBodyStyle.Wrapped,
			RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
		public decimal CalcProductRetailPrice(CalcProductRetailPriceRequest request)
		{
			if (request == null || request.ProductCodes == null || request.ProductCodes.Count == 0) return 0;
			return (new Select(UserConnection)
				.Column(Func.Sum("SmrRecommendedRetailPrice"))
				.From("Product")
				.Where("Code").In(request.ProductCodes.Select(c => Column.Parameter(c))) as Select).ExecuteScalar<decimal>();
		}
	}

}