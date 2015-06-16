using System.Xml.Serialization;
using lightspeedAccess.Models.ShippingInfo;
using LightspeedAccess.Models.Order;

namespace LightspeedAccess.Models.ShippingInfo
{
	public class Contact
	{
		[XmlElement( typeof( LightspeedAddress ) )]
		public LightspeedAddress[] Addresses;

		[XmlElement( typeof( LightspeedPhone ) )]
		public LightspeedPhone[] Phones;

		[XmlElement( typeof( LightspeedEmail ) )]
		public LightspeedEmail[] Emails;
	}
}