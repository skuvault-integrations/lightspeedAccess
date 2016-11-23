using System.Xml.Serialization;
using lightspeedAccess.Models.ShippingInfo;
using LightspeedAccess.Models.Order;

namespace LightspeedAccess.Models.ShippingInfo
{
	public class Contact
	{
		[ XmlElement( typeof( LightspeedAddress ) ) ]
		public LightspeedAddress[] Addresses;

		[ XmlElement( typeof( LightspeedPhone ) ) ]
		public LightspeedPhone[] Phones;

		[ XmlElement( typeof( LightspeedEmail ) ) ]
		public LightspeedEmail[] Emails;
	}

	[ XmlType( "Contact" ) ]
	public class Contact2
	{
		[ XmlArray( "Addresses" ) ]
		public LightspeedAddress[] Addresses{ get; set; }

		[ XmlArray( "Phones" ) ]
		public LightspeedPhone[] Phones{ get; set; }

		[ XmlArray( "Emails" ) ]
		public LightspeedEmail[] Emails{ get; set; }

		[ XmlArray( "Websites" ) ]
		public LightspeedWebsite[] Websites{ get; set; }
	}
}