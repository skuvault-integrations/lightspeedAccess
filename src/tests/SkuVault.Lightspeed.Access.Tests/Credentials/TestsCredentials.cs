using System.IO;

namespace SkuVault.Lightspeed.Access.Tests.Credentials
{
	internal class TestsCredentials
	{
		public string ClientId{ get; private set; }
		public string ClientSecret{ get; private set; }
		public int AccountId{ get; private set; }
		public string AccessToken{ get; private set; }
		public string RefreshToken{ get; private set; }

		public TestsCredentials( string fName )
		{
			this.ReadCredentials( fName );
		}

		private void ReadCredentials( string fName )
		{
			using( StreamReader sr = new StreamReader( fName ) )
			{
				var line = sr.ReadLine();
				var data = line.Split( ',' );
				this.ClientId = data[ 0 ];
				this.ClientSecret = data[ 1 ];
				this.AccountId = int.Parse( data[ 2 ] );
				this.AccessToken = data[ 3 ];
				this.RefreshToken = data[ 4 ];
			}
		}
	}
}
