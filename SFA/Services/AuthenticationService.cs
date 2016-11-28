using System;
using System.Threading.Tasks;
using Plugin.Connectivity;
using SFA.Services;
using Xamarin.Forms;
using SFA.Models;

[assembly: Dependency(typeof(AuthenticationService))]
namespace SFA.Services
{
	public class AuthenticationService : IAuthenticationService
	{
		public bool IsAuthentified
		{
			get
			{
				return false;
			}
		}

		public async Task<bool> LoginAsync(string user, string password)
		{
			if (!CrossConnectivity.Current.IsConnected)
				MessagingCenter.Send(MessagingCenterBroadcast.Instance, MessagingCenterConstants.Dialog, new MessageDialog()
				{
					Title = "Errore",
					Message = "Terminale non collegato !",
					Cancel = "Ok"
				});

			MessagingCenter.Send(MessagingCenterBroadcast.Instance, MessagingCenterConstants.Dialog,
								  new MessageDialog()
								  {

									  Title = "Errore",
									  Message = "Utente e password non validi!",
									  Cancel = "Ok"
								  }
								 );
			//DO THE WORK
			return false;
		}

		public async Task<bool> LogoutAsync()
		{
			return true;
		}
	}
}
