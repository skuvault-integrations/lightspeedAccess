using System.Xml.Serialization;

namespace SkuVault.Lightspeed.Access.Models.ShippingInfo
{
	[ XmlType( "ContactWebsite" ) ]
	public class LightspeedWebsite
	{
		public string url{ get; set; }
	}
}