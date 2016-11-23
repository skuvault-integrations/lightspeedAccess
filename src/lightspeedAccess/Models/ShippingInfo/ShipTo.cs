using System.Xml.Serialization;

namespace LightspeedAccess.Models.ShippingInfo
{
	[ XmlType( "ShipTo" ) ]
	public class ShipTo
	{
		[ XmlElement( "saleID" ) ]
		public int SaleId{ get; set; }

		public string firstName{ get; set; }
		public string lastName{ get; set; }
		public string company{ get; set; }
		public Contact Contact{ get; set; }
	}

	[ XmlType( "ShipTo" ) ]
	public class ShipTo2
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
		public Contact2 Contact{ get; set; }
	}
}