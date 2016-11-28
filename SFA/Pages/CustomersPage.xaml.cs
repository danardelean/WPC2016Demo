using System;
using System.Collections.Generic;

using Xamarin.Forms;
using SFA.ViewModels;
using SFA.Models;

namespace SFA
{
	public partial class CustomersPage : ContentPage
	{
		public CustomersPage()
		{
			InitializeComponent();
			BindingContext = new CustomersViewModel();

			NavigationPage.SetBackButtonTitle(this, "");
		}

		CustomersViewModel ViewModel
		{
			get
			{
				return BindingContext as CustomersViewModel;
			}
				
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			if (ViewModel.IsInitialized)
				return;
			
			ViewModel.LoadAccountsCommand.Execute(null);
			ViewModel.IsInitialized = true;
		}

		async void CustomerItemTapped(object sender, ItemTappedEventArgs e)
		{
			Account account = (Account)e.Item;
			((ListView)sender).SelectedItem = null;
			await Navigation.PushAsync (new CustomerDetailPage( account));
		}
	}
}
