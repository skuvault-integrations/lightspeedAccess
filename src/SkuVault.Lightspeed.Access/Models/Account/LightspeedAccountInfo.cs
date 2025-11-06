using System.Xml.Serialization;

namespace SkuVault.Lightspeed.Access.Models.Account
{
	[ XmlRoot( "Account" ) ]
	public class LightspeedAccountInfo
	{
		[ XmlAttribute( "systemCustomerID" ) ]
		public int AccountId{ get; set; }
	}
}