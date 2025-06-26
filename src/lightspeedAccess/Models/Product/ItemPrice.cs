using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace lightspeedAccess.Models.Product
{
	[ XmlType( "ItemPrice" ) ]
	[ DataContract ]
	public class ItemPrice
	{
		[ XmlElement( "amount" ) ]
		[ DataMember( Order = 1 ) ]
		public decimal Amount{ get; set; }

		[ XmlElement( "useTypeID" ) ]
		[ DataMember( Order = 2 ) ]
		public int UseTypeId{ get; set; }

		[ XmlElement( "useType" ) ]
		[ DataMember( Order = 3 ) ]
		public string UseType{ get; set; }
	}
}