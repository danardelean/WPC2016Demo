using System;
using SFA.iOS;
using Xamarin.Forms;
using System.IO;

[assembly: Dependency(typeof(DeviceService))]

namespace SFA.iOS
{
	public class DeviceService : IDeviceService
	{
		public string DBLocalPath
		{
			get
			{
				var sqliteFilename = "sfa.db3";
				string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal); // Documents folder
				string libraryPath = Path.Combine(documentsPath, "..", "Library"); // Library folder
				return Path.Combine(libraryPath, sqliteFilename);

			}
		}
	}
}
