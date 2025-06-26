using System.Xml.Serialization;

namespace lightspeedAccess.Models.ShippingInfo
{
	[ XmlType( "ContactEmail" ) ]
	public class LightspeedEmail
	{
		public string address{ get; set; }
		public string useType{ get; set; }
	}
}