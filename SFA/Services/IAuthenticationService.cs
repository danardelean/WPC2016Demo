using System;
using System.Threading.Tasks;

namespace SFA
{
	public interface IAuthenticationService
	{
		bool IsAuthentified { get;}
		Task<bool> LoginAsync(string user,string password);
		Task<bool> LogoutAsync();
	}
}
