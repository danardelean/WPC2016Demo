using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.Statics;
using SFA.ViewModels;
using Xamarin.Forms;

namespace SFA
{
	public class RootPage : MasterDetailPage
	{
		Dictionary<MenuType, NavigationPage> Pages { get; set; }

		public RootPage()
		{
			Pages = new Dictionary<MenuType, NavigationPage>();
			Master = new MenuPage(this);
			BindingContext = new BaseViewModel(Navigation)
			{
				Title = "S.F.A.",
				Icon = "slideout.png"
			};
			//setup home page
			NavigateAsync(MenuType.Customers);
		}

		void SetDetailIfNull(Page page)
		{
			if (Detail == null && page != null)
				Detail = page;
		}

		public async Task NavigateAsync(MenuType id)
		{
			Page newPage;
			if (!Pages.ContainsKey(id))
			{
				switch (id)
				{

					case MenuType.Customers:
						var page = new SFANavigationPage(new CustomersPage
						{
							BindingContext = new CustomersViewModel(),
							Title = "Clienti",
							Icon = new FileImageSource { File = "customers.png" }
						});
						SetDetailIfNull(page);
						Pages.Add(id, page);
						break;
					case MenuType.Products:
						page = new SFANavigationPage(new CategoryListPage
						{
							BindingContext = new CategoriesViewModel(),
							Title = "Prodotti",
							Icon = new FileImageSource { File = "products.png" }
						});
						SetDetailIfNull(page);
						Pages.Add(id, page);
						break;
				
				}
			}

			newPage = Pages[id];
			if (newPage == null)
				return;

			//pop to root for Windows Phone
			if (Detail != null && Device.OS == TargetPlatform.WinPhone)
			{
				await Detail.Navigation.PopToRootAsync();
			}

			Detail = newPage;

			if (Device.Idiom != TargetIdiom.Tablet)
				IsPresented = false;
		}
	}

	public class RootTabPage : TabbedPage
	{
		public RootTabPage()
		{
			
			Children.Add(new SFANavigationPage(new CustomersPage
			{
				BindingContext = new CustomersViewModel() { Navigation = this.Navigation },
				Title = "Clienti",
				Icon = new FileImageSource { File = "customers.png" }
			})
			{
				Title = "Clienti",
				Icon = new FileImageSource { File = "customers.png" }
			});
			Children.Add(new SFANavigationPage(new CategoryListPage
			{
				BindingContext = new CategoriesViewModel() { Navigation = this.Navigation },
				Title = "Prodotti",
				Icon = new FileImageSource { File = "products.png" }
			})
			{
				Title = "Prodotti",
				Icon = new FileImageSource { File = "products.png" },
			});

		}

		protected override void OnCurrentPageChanged()
		{
			base.OnCurrentPageChanged();
			this.Title = this.CurrentPage.Title;
		}
	}

	public class SFANavigationPage : NavigationPage
	{
		public SFANavigationPage(Page root)
			: base(root)
		{
			Init();
		}

		public SFANavigationPage()
		{
			Init();
		}

		void Init()
		{

			BarBackgroundColor = Palette._001;
			BarTextColor = Color.White;
		}
	}

	public enum MenuType
	{
		Customers,
		Products
	}

	public class HomeMenuItem
	{
		public HomeMenuItem()
		{
			MenuType = MenuType.Customers;
		}

		public string Icon { get; set; }

		public MenuType MenuType { get; set; }

		public string Title { get; set; }

		public string Details { get; set; }

		public int Id { get; set; }
	}
}
