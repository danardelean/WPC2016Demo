using System;
using System.Collections.Generic;
using SFA.Models;
using SFA.ViewModels;

using Xamarin.Forms;

namespace SFA
{
	public partial class CategoryListPage : ContentPage
	{
		public CategoryListPage(string categoryId=null,string title=null)
		{
			InitializeComponent();
			BindingContext = new CategoriesViewModel(categoryId,title);

			NavigationPage.SetBackButtonTitle(this, "");
		}

		CategoriesViewModel ViewModel
		{
			get
			{
				return BindingContext as CategoriesViewModel;
			}

		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			if (ViewModel.IsInitialized)
				return;

			ViewModel.LoadCategoriesCommand.Execute(null);
			ViewModel.IsInitialized = true;
		}

		async void CategoryItemTapped(object sender, ItemTappedEventArgs e)
		{
			Category catalogCategory = ((Category)e.Item);
			((ListView)sender).SelectedItem = null;

			if (catalogCategory.HasSubCategories)
				await Navigation.PushAsync(new CategoryListPage(catalogCategory.Id,catalogCategory.Name)); 
			else
				await Navigation.PushAsync(new ProductListPage(catalogCategory.Id, catalogCategory.Name));
		}
	}
}
