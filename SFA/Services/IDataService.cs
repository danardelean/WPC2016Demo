using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.Models;

namespace SFA
{
	public interface IDataService
	{
		bool UsesDemoData { get;}

		Task SeedLocalDataAsync();

		Task<List<Account>> GetAccountsAsync(bool includeLeads = false);

		Task<List<Product>> GetProductsAsync(string categoryId);

		Task<List<Category>> GetCategoriesAsync(string parentCategoryId = null);
	}
}
