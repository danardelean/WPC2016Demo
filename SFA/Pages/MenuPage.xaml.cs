using System;
using System.Collections.Generic;
using SFA.ViewModels;
using Xamarin.Forms;

namespace SFA
{
	public partial class MenuPage : ContentPage
	{
		RootPage root;
		List<HomeMenuItem> menuItems;
		public MenuPage(RootPage root)
		{
			this.root = root;
			InitializeComponent();
			BindingContext = new BaseViewModel(Navigation)
			{
				Title = "SFA",
				Subtitle = "SFA",
				Icon = "slideout.png"
			};

			ListViewMenu.ItemsSource = menuItems = new List<HomeMenuItem>
				{
					new HomeMenuItem { Title = "Clienti", MenuType = MenuType.Customers, Icon = "customers.png" },
					new HomeMenuItem { Title = "Prodotti", MenuType = MenuType.Products, Icon = "products.png" },
				};

			ListViewMenu.SelectedItem = menuItems[0];

			ListViewMenu.ItemSelected += async (sender, e) =>
				{
					if (ListViewMenu.SelectedItem == null)
						return;

					await this.root.NavigateAsync(((HomeMenuItem)e.SelectedItem).MenuType);
				};
		}
	}
}
