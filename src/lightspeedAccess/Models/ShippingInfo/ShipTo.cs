using System.Xml.Serialization;

namespace lightspeedAccess.Models.ShippingInfo
{
	[ XmlType( "ShipTo" ) ]
	public class ShipTo
	{
		[ XmlElement( "saleID" ) ]
		public int SaleId{ get; set; }

		[ XmlElement( "firstName" ) ]
		public string FirstName{ get; set; }

		[ XmlElement( "lastName" ) ]
		public string LastName{ get; set; }

		[ XmlElement( "company" ) ]
		public string Company{ get; set; }

		[ XmlElement( "Contact" ) ]
		public Contact Contact{ get; set; }
	}
}