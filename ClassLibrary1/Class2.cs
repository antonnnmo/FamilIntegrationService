namespace Terrasoft.Configuration
{
	using System.ServiceModel;
	using System.ServiceModel.Web;
	using System.ServiceModel.Activation;
	using Terrasoft.Web.Common;
	using Terrasoft.Core.DB;
	using System;
	using System.Runtime.Serialization;

	[DataContract]
	public class DeleteTagRequest
	{
		[DataMember]
		public Guid Id { get; set; }
		[DataMember]
		public string SchemaName { get; set; }
	}

	[ServiceContract]
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
	public class TagService : BaseService
	{
		[OperationContract]
		[WebInvoke(Method = "POST", UriTemplate = "DeleteTag", BodyStyle = WebMessageBodyStyle.Wrapped,
			RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
		public object DeleteTag(DeleteTagRequest request)
		{
			new Delete(UserConnection).From($"{request.SchemaName}InTag").Where("TagId").IsEqual(Column.Parameter(request.Id));

			return new { IsSuccess = true };
		}
	}
}