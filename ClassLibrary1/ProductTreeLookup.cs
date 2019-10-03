using System;
using System.Collections.Generic;
using System.Linq;
using Terrasoft.Core;
using Terrasoft.Core.DB;

namespace Terrasoft.Configuration
{
	public class ProductTreeLookupBranch
	{
		public Guid GroupId { get; set; }
		public Guid SubCategoryId { get; set; }
		public Guid CategoryId { get; set; }
		public Guid DirectionId { get; set; }
	}

	public class ProductTreeLookupCollection
	{
		private UserConnection _uc;

		public ProductTreeLookupCollection(UserConnection uc)
		{
			_uc = uc;
		}

		public List<ProductTreeLookup> Groups { get; set; }
		public List<ProductTreeLookup> SubCategories { get; set; }
		public List<ProductTreeLookup> Categories { get; set; }
		public List<ProductTreeLookup> Directions { get; set; }

		public List<ProductTreeLookup> Brands { get; set; }
		public List<ProductTreeLookup> BrandsTypes { get; set; }

		public ProductTreeLookup FindDirection(string name)
		{
			if (Directions == null)
			{
				LoadDirections();
			}

			return Directions.FirstOrDefault(d => d.Name == name);
		}

		public ProductTreeLookup FindCategory(string name)
		{
			if (Categories == null)
			{
				LoadCategories();
			}

			return Categories.FirstOrDefault(d => d.Name == name);
		}

		public ProductTreeLookup FindSubCategory(string name)
		{
			if (SubCategories == null)
			{
				LoadSubCategories();
			}

			return SubCategories.FirstOrDefault(d => d.Name == name);
		}

		public ProductTreeLookup FindGroup(string name)
		{
			if (SubCategories == null)
			{
				LoadSubCategories();
			}

			return SubCategories.FirstOrDefault(d => d.Name == name);
		}

		private void LoadDirections()
		{
			Directions = (new Select(_uc).Column("Id").Column("Name").Column("SmrERPId").From("SmrProductDirection").GetList(_uc, Guid.Empty, String.Empty, String.Empty) as List<Tuple<Guid, String, String>>)
				.Select(d => new ProductTreeLookup() {
					ERPId = d.Item3,
					Id = d.Item1,
					Name = d.Item2
				}).ToList();
		}

		private void LoadCategories()
		{
			if (Directions == null) LoadDirections();
			Categories = (new Select(_uc).Column("Id").Column("Name").Column("SmrERPId").Column("SmrDirectionId").From("ProductCategory").GetList(_uc, Guid.Empty, String.Empty, String.Empty, Guid.Empty) as List<Tuple<Guid, String, String, Guid>>)
				.Select(c => new ProductTreeLookup()
				{
					ERPId = c.Item3,
					Id = c.Item1,
					Name = c.Item2,
					Parent = Directions.FirstOrDefault(d => d.Id == c.Item4)
				}).ToList();
		}

		private void LoadSubCategories()
		{
			if (Categories == null) LoadCategories();
			SubCategories = (new Select(_uc).Column("Id").Column("Name").Column("SmrERPId").Column("CategoryId").From("ProductType").GetList(_uc, Guid.Empty, String.Empty, String.Empty, Guid.Empty) as List<Tuple<Guid, String, String, Guid>>)
				.Select(sc => new ProductTreeLookup()
				{
					ERPId = sc.Item3,
					Id = sc.Item1,
					Name = sc.Item2,
					Parent = Categories.FirstOrDefault(c => c.Id == sc.Item4)
				}).ToList();
		}

		private void LoadGroups()
		{
			if (SubCategories == null) LoadSubCategories();
			Groups = (new Select(_uc).Column("Id").Column("Name").Column("SmrERPId").Column("SmrSubCategoryId").From("SmrProductGroup").GetList(_uc, Guid.Empty, String.Empty, String.Empty, Guid.Empty) as List<Tuple<Guid, String, String, Guid>>)
				.Select(g => new ProductTreeLookup()
				{
					ERPId = g.Item3,
					Id = g.Item1,
					Name = g.Item2,
					Parent = SubCategories.FirstOrDefault(sc => sc.Id == g.Item4)
				}).ToList();
		}
	}

	public class ProductTreeLookup
	{
		public Guid Id { get; set; }
		public string ERPId { get; set; }
		public ProductTreeLookup Parent { get; set; }
		public string Name { get; set; }
	}
}
