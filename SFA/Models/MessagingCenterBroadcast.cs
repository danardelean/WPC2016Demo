using System;
namespace SFA.Models
{
	public class MessagingCenterBroadcast
	{
		static MessagingCenterBroadcast _instance;

		public static MessagingCenterBroadcast Instance
		{
			get
			{
				if (_instance == null)
					_instance = new MessagingCenterBroadcast();
				return _instance;
			}

		}

		private MessagingCenterBroadcast()
		{
		}
	}
}
