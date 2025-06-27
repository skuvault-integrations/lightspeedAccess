using System.Xml.Serialization;

namespace SkuVault.Lightspeed.Access.Models.ShippingInfo
{
	[ XmlType( "Contact" ) ]
	public class Contact
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