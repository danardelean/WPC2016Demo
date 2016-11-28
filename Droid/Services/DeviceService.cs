using System;
using SFA.Droid;
using Xamarin.Forms;
using System.IO;

[assembly: Dependency(typeof(DeviceService))]

namespace SFA.Droid
{
	public class DeviceService:IDeviceService
	{
		public string DBLocalPath
		{
			get
			{
				var sqliteFilename = "sfa.db3";
				string documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal); // Documents folder
				return Path.Combine(documentsPath, sqliteFilename);
			}
		}
	}
}
