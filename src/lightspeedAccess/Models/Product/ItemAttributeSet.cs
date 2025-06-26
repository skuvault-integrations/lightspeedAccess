using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace LightspeedAccess.Models.Product
{
	[ XmlType( "ItemAttributeSet" ) ]
	[ DataContract ]
	public class ItemAttributeSet
	{
		[ XmlElement( "name" ) ]
		[ DataMember( Order = 1 ) ]
		public string Name{ get; set; }

		[ XmlElement( "attributeName1" ) ]
		[ DataMember( Order = 2 ) ]
		public string AttributeName1{ get; set; }

		[ XmlElement( "attributeName2" ) ]
		[ DataMember( Order = 3 ) ]
		public string AttributeName2{ get; set; }

		[ XmlElement( "attributeName3" ) ]
		[ DataMember( Order = 4 ) ]
		public string AttributeName3{ get; set; }

		[ XmlElement( "system" ) ]
		[ DataMember( Order = 5 ) ]
		public bool System{ get; set; }

		[ XmlElement( "archived" ) ]
		[ DataMember( Order = 6 ) ]
		public bool Archived{ get; set; }
	}
}