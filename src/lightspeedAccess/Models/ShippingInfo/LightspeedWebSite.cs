using System.Xml.Serialization;

namespace lightspeedAccess.Models.ShippingInfo
{
	[ XmlType( "ContactWebsite" ) ]
	public class LightspeedWebsite
	{
		public string url{ get; set; }
	}
}