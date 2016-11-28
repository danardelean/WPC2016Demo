using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.ViewModels;

using Xamarin.Forms;

namespace SFA
{
	public partial class LoginPage : ContentPage
	{
		public LoginPage()
		{
			InitializeComponent();

			BindingContext = new LoginViewModel();

		}

		protected async override void OnAppearing()
		{
			base.OnAppearing();

			await Task.Delay(App.AnimationSpeed);

			// Sequentially animate the login buttons. ScaleTo() makes them "grow" from a singularity to the full button size.
			await SignInButton.ScaleTo(1, (uint)App.AnimationSpeed, Easing.SinIn);
			await SkipSignInButton.ScaleTo(1, (uint)App.AnimationSpeed, Easing.SinIn);
		}
	}
}
