namespace Terrasoft.Configuration
{
	using System.ServiceModel;
	using System.ServiceModel.Web;
	using System.ServiceModel.Activation;
	using Terrasoft.Web.Common;
	using FamilIntegrationService.Models;
	using System.Collections.Generic;
	using FamilIntegrationCore.Models;

	[ServiceContract]
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
	public class GateIntegrationService : BaseService
	{
		[OperationContract]
		[WebInvoke(Method = "POST", UriTemplate = "PrimaryIntegratePack", BodyStyle = WebMessageBodyStyle.Wrapped,
			RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
		public PackResult PrimaryIntegratePack(IntegrationObjectRequest request)
		{
			if (request.TableName == "ContactGate")
				return new ContactIntegrationManager(UserConnection).PrimaryIntegrateContactPack(request.Objects);
			if (request.TableName == "ProductSizeGate")
				return new ProductSizeIntegrationManager(UserConnection).PrimaryIntegrateProductSizePack(request.Objects);
			if (request.TableName == "ProductTagGate")
				return new ProductTagIntegrationManager(UserConnection).PrimaryIntegrateProductTagPack(request.Objects);
			if (request.TableName == "ContactTagGate")
				return new ContactTagIntegrationManager(UserConnection).PrimaryIntegrateContactTagPack(request.Objects);
			if (request.TableName == "ProductCategoryGate")
				return new ProductCategoryIntegrationManager(UserConnection).PrimaryIntegrateProductCategoryPack(request.Objects);
			if (request.TableName == "ProductSubCategoryGate")
				return new ProductSubCategoryIntegrationManager(UserConnection).PrimaryIntegrateProductSubCategoryPack(request.Objects);
			if (request.TableName == "BrandTypeGate")
				return new BrandTypeIntegrationManager(UserConnection).PrimaryIntegrateBrandTypePack(request.Objects);
			if (request.TableName == "ShopGate")
				return new ShopIntegrationManager(UserConnection).PrimaryIntegrateShopPack(request.Objects);
			if (request.TableName == "ProductGroupGate")
				return new ProductGroupIntegrationManager(UserConnection).PrimaryIntegrateProductGroupPack(request.Objects);
			if (request.TableName == "SMSGate")
				return new SMSIntegrationManager(UserConnection).PrimaryIntegratePack(request.Objects);
			if (request.TableName == "ProductGate")
				return new ProductIntegrationManager(UserConnection).PrimaryIntegratePack(request.Objects);
			return null;
		}


		[OperationContract]
		[WebInvoke(Method = "POST", UriTemplate = "IntegratePack", BodyStyle = WebMessageBodyStyle.Wrapped,
			RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
		public List<PackResult> IntegratePack(IntegrationObjectRequest request)
		{
			if (request.TableName == "ContactGate")
				return new ContactIntegrationManager(UserConnection).IntegrateContactPack(request.Objects);
			if (request.TableName == "ProductSizeGate")
				return new ProductSizeIntegrationManager(UserConnection).IntegrateProductSizePack(request.Objects);
			if (request.TableName == "ProductTagGate")
				return new ProductTagIntegrationManager(UserConnection).IntegrateProductTagPack(request.Objects);
			if (request.TableName == "ContactTagGate")
				return new ContactTagIntegrationManager(UserConnection).IntegrateContactTagPack(request.Objects);
			if (request.TableName == "ProductCategoryGate")
				return new ProductCategoryIntegrationManager(UserConnection).IntegrateProductCategoryPack(request.Objects);
			if (request.TableName == "ProductSubCategoryGate")
				return new ProductSubCategoryIntegrationManager(UserConnection).IntegrateProductSubCategoryPack(request.Objects);
			if (request.TableName == "BrandTypeGate")
				return new BrandTypeIntegrationManager(UserConnection).IntegrateBrandTypePack(request.Objects);
			if (request.TableName == "ShopGate")
				return new ShopIntegrationManager(UserConnection).IntegrateShopPack(request.Objects);
			if (request.TableName == "ProductGroupGate")
				return new ProductGroupIntegrationManager(UserConnection).IntegrateProductGroupPack(request.Objects);
			if (request.TableName == "SMSGate")
				return new SMSIntegrationManager(UserConnection).IntegratePack(request.Objects);
			if (request.TableName == "ProductGate")
				return new ProductIntegrationManager(UserConnection).IntegratePack(request.Objects);
			else return null;
		}


	}

}