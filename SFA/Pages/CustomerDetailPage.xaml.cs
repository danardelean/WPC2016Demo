using System;
using System.Collections.Generic;
using SFA.Models;
using Xamarin.Forms;
using SFA.ViewModels;

namespace SFA
{
	public partial class CustomerDetailPage : ContentPage
	{
		public CustomerDetailPage(Account account)
		{
			InitializeComponent();
			this.BindingContext = new CustomerDetailViewModel(account);
		}
	}
}
