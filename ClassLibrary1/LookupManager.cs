using FamilIntegrationService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Terrasoft.Configuration;
using Terrasoft.Core;
using Terrasoft.Core.DB;

public class BaseIntegrationLookup
{
	public string Name { get; set; }
	public Guid Id { get; set; }
}

public class IntegrationLookup : BaseIntegrationLookup
{
	public string ERPId { get; set; }
}

public class IntegrationCodeLookup : IntegrationLookup
{
	public string Code { get; set; }
}

public class LookupManager
{
	static object _lock = new object();
	private UserConnection _userConnection;
	private static Dictionary<String, List<BaseIntegrationLookup>> _lookups;
	private static Dictionary<String, List<BaseIntegrationLookup>> Lookups
	{
		get { if (_lookups == null) _lookups = new Dictionary<string, List<BaseIntegrationLookup>>(); return _lookups; }
	}

	public static void ClearCache()
	{
		_lookups = null;
	}

	public LookupManager(UserConnection uc)
	{
		_userConnection = uc;
	}

	public void LoadCache()
	{
		/*if (Shops == null)
		{
			Shops = LoadCodeLookup("SmrShop");
		}*/
	}

	internal Guid? FindLookupIdByName(string name, string lookupName)
	{
		if (string.IsNullOrEmpty(name)) return null;
		List<BaseIntegrationLookup> values;
		lock (_lock)
		{
			if (Lookups.Keys.Contains(lookupName))
			{
				values = Lookups[lookupName];
			}
			else
			{
				values = LoadBaseLookup(lookupName);
				Lookups.Add(lookupName, values);
			}
		}

		return values.FirstOrDefault(s => s.Name == name)?.Id;
	}

	public List<BaseIntegrationLookup> LoadBaseLookup(string name)
	{
		var result = new List<BaseIntegrationLookup>();

		var select = new Select(_userConnection)
						.Column("Id")
						.Column("Name")
						.From(name);

		using (var dbExecutor = _userConnection.EnsureDBConnection())
		{
			using (var reader = select.ExecuteReader(dbExecutor))
			{
				while (reader.Read())
				{
					result.Add(new BaseIntegrationLookup()
					{
						Id = reader.GetValue("Id", Guid.Empty),
						Name = reader.GetValue("Name", String.Empty)
					});
				}
			}
		}

		return result;
	}

	public List<BaseIntegrationLookup> LoadLookup(string name)
	{
		var result = new List<BaseIntegrationLookup>();

		var select = new Select(_userConnection)
						.Column("Id")
						.Column("Name")
						.Column("SmrERPId")
						.From(name);

		using (var dbExecutor = _userConnection.EnsureDBConnection())
		{
			using (var reader = select.ExecuteReader(dbExecutor))
			{
				while (reader.Read())
				{
					result.Add(new IntegrationLookup()
					{
						Id = reader.GetValue("Id", Guid.Empty),
						Name = reader.GetValue("Name", String.Empty),
						ERPId = reader.GetValue("SmrERPId", String.Empty),
					});
				}
			}
		}

		return result;
	}

	internal void AddToLookup(SimpleLookupGateInfo info, string lookupName)
	{
		List<BaseIntegrationLookup> values;
		lock (_lock)
		{
			if (Lookups.Keys.Contains(lookupName))
			{
				values = Lookups[lookupName];
			}
			else
			{
				values = LoadLookup(lookupName);
				Lookups.Add(lookupName, values);
			}
		}

		values.Add(new IntegrationLookup() { ERPId = info.ERPId, Id = info.Id, Name = info.Name });
	}

	internal void UpdateLookup(SimpleLookupGateInfo info, string lookupName)
	{
		List<BaseIntegrationLookup> values;
		lock (_lock)
		{
			if (Lookups.Keys.Contains(lookupName))
			{
				values = Lookups[lookupName];
			}
			else
			{
				values = LoadLookup(lookupName);
				Lookups.Add(lookupName, values);
			}
		}

		var lookupObj = values.FirstOrDefault(v => ((IntegrationLookup)v).ERPId == info.ERPId);
		if (lookupObj != null)
			lookupObj.Name = info.Name;
	}

	public List<BaseIntegrationLookup> LoadCodeLookup(string name)
	{
		var result = new List<BaseIntegrationLookup>();

		var select = new Select(_userConnection)
						.Column("Id")
						.Column("Name")
						.Column("Code")
						.From(name);

		using (var dbExecutor = _userConnection.EnsureDBConnection())
		{
			using (var reader = select.ExecuteReader(dbExecutor))
			{
				while (reader.Read())
				{
					result.Add(new IntegrationCodeLookup()
					{
						Id = reader.GetValue("Id", Guid.Empty),
						Name = reader.GetValue("Name", String.Empty),
						Code = reader.GetValue("Code", String.Empty),
					});
				}
			}
		}

		return result;
	}

	public List<BaseIntegrationLookup> LoadSmrCodeLookup(string name)
	{
		var result = new List<BaseIntegrationLookup>();

		var select = new Select(_userConnection)
						.Column("Id")
						.Column("Name")
						.Column("SmrCode")
						.From(name);

		using (var dbExecutor = _userConnection.EnsureDBConnection())
		{
			using (var reader = select.ExecuteReader(dbExecutor))
			{
				while (reader.Read())
				{
					result.Add(new IntegrationCodeLookup()
					{
						Id = reader.GetValue("Id", Guid.Empty),
						Name = reader.GetValue("Name", String.Empty),
						Code = reader.GetValue("SmrCode", String.Empty),
					});
				}
			}
		}

		return result;
	}

	internal Guid? FindLookupId(string erpId, string lookupName)
	{
		List<BaseIntegrationLookup> values;
		lock (_lock)
		{
			if (Lookups.Keys.Contains(lookupName))
			{
				values = Lookups[lookupName];
			}
			else
			{
				values = LoadLookup(lookupName);
				Lookups.Add(lookupName, values);
			}
		}

		return values.FirstOrDefault(s => ((IntegrationLookup)s).ERPId == erpId)?.Id;
	}

	internal Guid? FindLookupIdByCode(string code, string lookupName)
	{
		List<BaseIntegrationLookup> values;
		lock (_lock)
		{
			if (Lookups.Keys.Contains(lookupName))
			{
				values = Lookups[lookupName];
			}
			else
			{
				values = LoadCodeLookup(lookupName);
				Lookups.Add(lookupName, values);
			}
		}

		return values.FirstOrDefault(s => ((IntegrationCodeLookup)s).Code == code)?.Id;
	}

	internal Guid? FindLookupIdBySmrCode(string code, string lookupName)
	{
		List<BaseIntegrationLookup> values;
		lock (_lock)
		{
			if (Lookups.Keys.Contains(lookupName))
			{
				values = Lookups[lookupName];
			}
			else
			{
				values = LoadSmrCodeLookup(lookupName);
				Lookups.Add(lookupName, values);
			}
		}

		return values.FirstOrDefault(s => ((IntegrationCodeLookup)s).Code == code)?.Id;
	}
}