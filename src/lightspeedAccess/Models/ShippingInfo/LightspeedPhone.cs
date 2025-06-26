using System.Xml.Serialization;

namespace lightspeedAccess.Models.ShippingInfo
{
	[ XmlType( "ContactPhone" ) ]
	public class LightspeedPhone
	{
		public string number{ get; set; }

		public string useType{ get; set; }
	}
}