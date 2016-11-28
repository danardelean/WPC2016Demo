using System;
using System.Threading.Tasks;
using SFA.Models;
using Xamarin.Forms;

namespace SFA.ViewModels
{
	public class LoginViewModel:BaseViewModel
	{
		
		string _Username;

		public string Username
		{
			get { return _Username; }
			set
			{
				SetProperty(ref _Username, value, "Username");
			}
		}

		string _Password;

		public string Password
		{
			get { return _Password; }
			set
			{
				SetProperty(ref _Password, value, "Password");
			}
		}


		Command _signInCommand;
		Command _demoCommand;

		/// <summary>
		/// Command to load accounts
		/// </summary>
		public Command SignInCommand
		{
			get { return _signInCommand ?? (_signInCommand = new Command(ExecuteSignCommand,CanExecuteComand)); }
		}

		public Command DemoCommand
		{
			get { return _demoCommand ?? (_demoCommand = new Command(ExecuteDemoCommand, CanExecuteComand)); }
		}

		async void ExecuteSignCommand(object parameter)
		{
			IsBusy = true;
			if (await AuthenticationService.LoginAsync(Username, Password))
			{
				//Navigate
			}
			IsBusy = false;
		}

		public bool IsBusy
		{
			get
			{
				return isBusy;
			}
			set
			{
				SetProperty(ref isBusy, value, "IsBusy", () =>
				{
					SignInCommand.ChangeCanExecute();
					DemoCommand.ChangeCanExecute();
				});
			}
		}

		async void ExecuteDemoCommand(object parameter)
		{
			if (IsBusy)
				return;

			IsBusy = true;
		

			if (!DataService.UsesDemoData)
				await DataService.SeedLocalDataAsync();

			IsBusy = false;
			MessagingCenter.Send(MessagingCenterBroadcast.Instance, MessagingCenterConstants.Logged_In);

		}

		bool CanExecuteComand(object parameter)
		{
			if (IsBusy)
				return false;
			else
				return true;
		}
	}
}
