using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lightspeedAccess.Models.Configuration
{
	public class LightspeedConfig
	{
		public string Endpoint { get; private set; }
		public string Username { get; private set; }
		public string Password { get; private set; }

		public LightspeedConfig()
		{
			Endpoint = "https://api.merchantos.com/API/Account/797/";
			Username = "imademo";
			Password = "thisismypass";
		}
	}

}