using System;
using System.Collections.ObjectModel;
using SFA.Models;
using Xamarin.Forms;

namespace SFA.ViewModels
{
	public class CategoriesViewModel:BaseViewModel
	{


		string _CategoryId;

		public string CategoryId
		{
			get { return _CategoryId; }
			set
			{
				SetProperty(ref _CategoryId, value, "CategoryId");
			}
		}



		ObservableCollection<Category> _SubCategories;

		public ObservableCollection<Category> SubCategories
		{
			get { return _SubCategories; }
			set
			{
				SetProperty(ref _SubCategories, value, "SubCategories");
			}
		}

		public CategoriesViewModel(string categoryId = null,string title=null)
		{
			CategoryId = categoryId;

			Title = title;
			SubCategories = new ObservableCollection<Category>();

		}

		Command _LoadCategoriesCommand;

		/// <summary>
		/// Command to load accounts
		/// </summary>
		public Command LoadCategoriesCommand
		{
			get { return _LoadCategoriesCommand ?? (_LoadCategoriesCommand = new Command(ExecuteLoadCategoriesCommand)); }
		}

		async void ExecuteLoadCategoriesCommand()
		{
			if (IsBusy)
				return;

			IsBusy = true;

			SubCategories = new ObservableCollection<Category>((await DataService.GetCategoriesAsync(CategoryId)));


			IsBusy = false;
		}

		public bool NeedsRefresh { get; set; }
	}
}
