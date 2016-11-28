using System;
using System.Collections.ObjectModel;
using SFA.Models;
using SFA.ViewModels;
using Xamarin.Forms;

namespace SFA
{
	public class ProductsViewModel:BaseViewModel
	{
		public ProductsViewModel(string categoryId,string title=null)
		{
			CategoryId = categoryId;
			Title = title;
		}



		string _CategoryId;
		public string CategoryId
		{
			get { return _CategoryId; }
			set
			{
				SetProperty(ref _CategoryId, value, "CategoryId");
			}
		}

		ObservableCollection<Product> _Products;
		public ObservableCollection<Product> Products
		{
			get { return _Products; }
			set
			{
				SetProperty(ref _Products, value, "Products");
			}
		}

		Command _LoadProductsCommand;

		/// <summary>
		/// Command to load accounts
		/// </summary>
		public Command LoadProductsCommand
		{
			get { return _LoadProductsCommand ?? (_LoadProductsCommand = new Command(ExecuteLoadProductsCommand)); }
		}

		async void ExecuteLoadProductsCommand()
		{
			if (IsBusy)
				return;

			IsBusy = true;

			Products = new ObservableCollection<Product>((await DataService.GetProductsAsync(_CategoryId)));

			IsBusy = false;
		}
	}
}
