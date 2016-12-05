using System.Xml.Serialization;

namespace LightspeedAccess.Models.ShippingInfo
{
	[ XmlType( "Customer" ) ]
	public class Customer
	{
		[ XmlElement( "customerID" ) ]
		public int CustomerId{ get; set; }

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