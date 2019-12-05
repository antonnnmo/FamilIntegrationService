namespace Terrasoft.Configuration.Utils
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Terrasoft.Core.DB;
	using Terrasoft.Core.Entities;

	#region Class: LoyaltyMacrosWorker

	/// <summary>
	/// Describes campaign macros worker to get macros values for entity schema macros.
	/// </summary>
	[MacrosWorker("BB368E0B-6879-41E9-9982-90C948BCA494")] // идентификатор корневого элемента вашей новой группы макросов
	public class LoyaltyMacrosWorker : BaseEntityMacrosWorker, IMacrosWorker
	{
		private static string _promocodePoolNameMacros = "Название пула";
		private static string _promocodeBonusNameMacros = "Бонус";

		/// <summary>
		/// Retrives macros values from database.
		/// </summary>
		/// <param name="macrosInfoCollection">Collection of macros.</param>
		/// <param name="arguments">Arguments.</param>
		/// <returns>Collection of type <see cref="Dictionary{MacrosAlias, MacrosValue}"/>.</returns>
		// Данный метод используется при нажатии на "Тестовое письмо" в рассылке.
		//arguments содержит параметр с идентификатором контакта, которому выполняется рассылка

		public new Dictionary<string, string> Proceed(IEnumerable<MacrosInfo> macrosInfoCollection, object arguments)
		{
			//var result = InternalProceed(macrosInfoCollection, arguments);
			return macrosInfoCollection?.ToDictionary(
				x => x.Name,
				n => GetMacrosValue(arguments as Guid?, macrosInfoCollection, n.Alias));
		}

		//Здесь мы можем вписать любой необходимый нам код. Возвращаемое значение будет вставлено в письмо вместо макроса.
		private string GetMacrosValue(Guid? contactId, IEnumerable<MacrosInfo> macrosInfoCollection, string macrosName)
		{
			//var macrosName = macrosInfoCollection.FirstOrDefault(m => m.Alias == n.Key)?.Name;
			if (macrosName == _promocodePoolNameMacros)
			{
				var select = new Select(UserConnection)
					.Top(1)
					.Column("pool", "Name")
					.From("SmrPromocode").As("p")
					.LeftOuterJoin("SmrPromocodePool").As("pool").On("pool", "Id").IsEqual("p", "PoolId")
					.LeftOuterJoin("Contact").As("c").On("c", "Id").IsEqual("p", "ContactId").And("p", "Code").IsEqual("c", "PromoCode")
					.Where("c", "Id").IsEqual(Column.Parameter(contactId)) as Select;
				return select.ExecuteScalar<string>();
			}
			else if (macrosName.Contains(_promocodeBonusNameMacros))
			{
				var bonusTypeCode = macrosName.Substring(6, macrosName.Length - 7);
				var select = new Select(UserConnection)
					.Top(1)
					.Column("balance", "CurrentAmount")
					.Column("t", "SmrNominative")
					.Column("t", "SmrGenetive")
					.Column("t", "SmrPlural")
					.From("SmrBonusBalance").As("balance")
					.LeftOuterJoin("SmrBonusType").As("t").On("t", "Id").IsEqual("balance", "TypeId")
					.Where("balance", "ContactId").IsEqual(Column.Parameter(contactId)).And("t", "Code").IsEqual(Column.Parameter(bonusTypeCode)) as Select;

				using (var dbExecutor = UserConnection.EnsureDBConnection())
				{
					using (var reader = select.ExecuteReader(dbExecutor))
					{
						if (reader.Read())
						{
							var bonusCount = Convert.ToInt32(Math.Floor(reader.GetValue("CurrentAmount", 0m)));
							var nominative = reader.GetValue("SmrNominative", String.Empty);
							var genetive = reader.GetValue("SmrGenetive", String.Empty);
							var plural = reader.GetValue("SmrPlural", String.Empty);
							return $"{bonusCount} {GetDeclension(bonusCount, nominative, genetive, plural)}";
						}
					}
				}
			}

			return String.Empty;
		}

		public static string GetDeclension(int number, string nominativ, string genetiv, string plural)
		{
			number = number % 100;
			if (number >= 11 && number <= 19)
			{
				return plural;
			}

			var i = number % 10;
			switch (i)
			{
				case 1:
					return nominativ;
				case 2:
				case 3:
				case 4:
					return genetiv;
				default:
					return plural;
			}

		}

		/// <summary>
		/// Retrives batch macros values from database.
		/// </summary>
		/// <param name="macrosInfoCollection">Collection of macros.</param>
		/// <param name="arguments">Arguments.</param>
		/// <returns>Collection of type <see cref="Dictionary{EntityId, Dictionary{MacrosAlias, MacrosValue}}"/>.</returns>
		// Данный метод используется при непосредственно рассылке писем. arguments содержит Select-запрос на выборку данных контакта.
		public new Dictionary<object, Dictionary<string, string>> ProcceedCollection(
				IEnumerable<MacrosInfo> macrosInfoCollection, object arguments)
		{
			var result = InternalProceedCollection(macrosInfoCollection, arguments);

			return result?.ToDictionary(
				x => x.Key,
				y => macrosInfoCollection.ToDictionary(
					m => m.Name,
					n => GetMacrosValue(y.Key as Guid?, macrosInfoCollection, n.Alias)));
		}
	}

	#endregion
}