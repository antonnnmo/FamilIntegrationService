﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace FamilIntegrationService.Models
{
	public class PackResults
	{
		[JsonProperty]
		public List<PackResult> IntegratePackResult { get; set; }
	}

	public class PrimaryIntegratePackResponse
	{
		[JsonProperty]
		public PackResult PrimaryIntegratePackResult { get; set; }
	}

	public class PackResult
	{
		[JsonProperty]
		public string Id { get; set; }
		[JsonProperty]
		public bool IsSuccess { get; set; }
		[JsonProperty]
		public string ErrorMessage { get; set; }
		[JsonProperty]
		public string ContactId { get; set; }
	}
}