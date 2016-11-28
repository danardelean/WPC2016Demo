using System;
using System.Collections.Generic;

using Xamarin.Forms;
using SFA.ViewModels;

namespace SFA
{
	public partial class ProductListPage : ContentPage
	{
		public ProductListPage(string categoryId,string title=null)
		{
			InitializeComponent();
			BindingContext = new ProductsViewModel(categoryId,title);
		}

		ProductsViewModel ViewModel
		{
			get
			{
				return BindingContext as ProductsViewModel;
			}

		}


		protected override void OnAppearing()
		{
			base.OnAppearing();

			if (ViewModel.IsInitialized)
				return;

			ViewModel.LoadProductsCommand.Execute(ViewModel.CategoryId);

			ViewModel.IsInitialized = true;
		}
	}
}
