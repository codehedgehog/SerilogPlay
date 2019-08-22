namespace SerilogPlay.SimpleMvcClient.Models
{
	using System.Collections.Generic;
	public class UserInfo
	{
		public string Name { get; set; }
		public Dictionary<string, string> Claims { get; set; }
	}
}
