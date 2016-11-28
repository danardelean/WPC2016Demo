using System;
using SFA.Models;
using SFA.ViewModels;

namespace SFA
{
	public class CustomerDetailViewModel:BaseViewModel
	{
		public Account Account { get; set; }
		public CustomerDetailViewModel(Account account)
		{
			if (account == null)
			{
				Account = new Account();
				Account.Industry = Account.IndustryTypes[0];
				Account.OpportunityStage = Account.OpportunityStages[0];

				this.Title = "New Account";
			}
			else
			{
				Account = account;
				this.Title = "Account";
			}
		}
	}
}
