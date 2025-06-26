using System.Xml.Serialization;

namespace LightspeedAccess.Models.ShippingInfo
{
	[ XmlType( "ContactAddress" ) ]
	public class LightspeedAddress
	{
		[ XmlElement( "address1" ) ]
		public string Address1{ get; set; }

		[ XmlElement( "address2" ) ]
		public string Address2{ get; set; }

		[ XmlElement( "zip" ) ]
		public string Zip{ get; set; }

		[ XmlElement( "country" ) ]
		public string Country{ get; set; }

		[ XmlElement( "state" ) ]
		public string State{ get; set; }

		[ XmlElement( "city" ) ]
		public string City{ get; set; }
	}
}