using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace LightspeedAccess.Models.Product
{
	[ XmlType( "ItemAttributes" ) ]
	[ DataContract ]
	public class ItemAttributes
	{
		[ XmlElement( "attribute1" ) ]
		[ DataMember( Order = 1 ) ]
		public string Attribute1{ get; set; }

		[ XmlElement( "attribute2" ) ]
		[ DataMember( Order = 2 ) ]
		public string Attribute2{ get; set; }

		[ XmlElement( "attribute3" ) ]
		[ DataMember( Order = 3 ) ]
		public string Attribute3{ get; set; }
	}
}