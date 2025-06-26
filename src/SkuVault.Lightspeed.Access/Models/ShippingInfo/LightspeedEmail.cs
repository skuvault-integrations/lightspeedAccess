using System.Xml.Serialization;

namespace SkuVault.Lightspeed.Access.Models.ShippingInfo
{
	[ XmlType( "ContactEmail" ) ]
	public class LightspeedEmail
	{
		public string address{ get; set; }
		public string useType{ get; set; }
	}
}