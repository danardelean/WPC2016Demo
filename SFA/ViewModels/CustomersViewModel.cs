using System;
using System.Collections.ObjectModel;
using SFA.Models;
using Xamarin.Forms;
using System.Threading.Tasks;
using SFA.Extensions;


namespace SFA.ViewModels
{
	public class CustomersViewModel:BaseViewModel
	{
		ObservableCollection<Account> _Accounts;
		public ObservableCollection<Account> Accounts
		{
			get { return _Accounts; }
			set
			{
				SetProperty(ref _Accounts, value, "Accounts");
			}
		}

		public CustomersViewModel()
		{
			this.Title = "Clienti";
			this.Icon = "list.png";
		}

		Command _loadAccountsCommand;

		/// <summary>
		/// Command to load accounts
		/// </summary>
		public Command LoadAccountsCommand
		{
			get { return _loadAccountsCommand ?? (_loadAccountsCommand = new Command(async () => await ExecuteLoadAccountsCommand())); }
		}

		async Task ExecuteLoadAccountsCommand()
		{
			IsBusy = true;
		
			Accounts = (await DataService.GetAccountsAsync(false)).ToObservableCollection();

			IsBusy = false;
		
		}

	}
}
