using System;
using Xamarin.Forms;

namespace SFA.Statics
{
	public static class Thicknesses
	{
		public static Thickness Empty
		{
			get { return new Thickness(0, 0, 0, 0); }
		}

		public static Thickness IosStatusBar
		{
			get { return new Thickness(0, 20, 0, 0); }
		}
	}
}
