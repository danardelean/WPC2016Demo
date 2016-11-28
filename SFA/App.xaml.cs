using Xamarin.Forms;
using SFA.Models;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

namespace SFA
{
	public partial class App : Application
	{
		public App()
		{
			InitializeComponent();

			if (!DependencyService.Get<IAuthenticationService>().IsAuthentified)
				MainPage = new LoginPage();
			else
			{
				if (Device.OS == TargetPlatform.iOS)
					MainPage = new RootTabPage();
				else
					MainPage = new RootPage();
			}
		}

		protected override void OnStart()
		{
			// Handle when your app starts
			Subscribe();
		}

		void Subscribe()
		{
			MessagingCenter.Subscribe<MessagingCenterBroadcast, MessageDialog>(this, MessagingCenterConstants.Dialog, HandleMessage);
			MessagingCenter.Subscribe<MessagingCenterBroadcast>(this, MessagingCenterConstants.Logged_In, HandleLoggedIn);
		}

		protected override void OnSleep()
		{
			// Handle when your app sleeps
			MessagingCenter.Unsubscribe<MessagingCenterBroadcast, MessageDialog>(this,MessagingCenterConstants.Dialog);
			MessagingCenter.Unsubscribe<MessagingCenterBroadcast>(this, MessagingCenterConstants.Logged_In);
		}

		protected override void OnResume()
		{
			// Handle when your app resumes
			Subscribe();
		}

		void HandleMessage(Models.MessagingCenterBroadcast sender, MessageDialog dialog)
		{
			
			Device.BeginInvokeOnMainThread(async () =>
			{
				await Current.MainPage.DisplayAlert(dialog.Title, dialog.Message, dialog.Cancel);
			});
		}

		void HandleLoggedIn(Models.MessagingCenterBroadcast senderg)
		{

			if (Device.OS == TargetPlatform.iOS)
				MainPage = new RootTabPage();
			else
				MainPage = new RootPage();
		}

		public static int AnimationSpeed = 250;
	}
}
