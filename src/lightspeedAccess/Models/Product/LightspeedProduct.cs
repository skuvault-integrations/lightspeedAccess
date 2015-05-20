using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace lightspeedAccess.Models.Product
{
	[XmlType( "Item" )]
	public class LightspeedProduct
	{
		[XmlElement( "itemID" )]
		public int ItemId { get; set; }

		[XmlElement( "systemSku" )]
		public string Sku { get; set; }
	}

	[XmlRootAttribute( "Items", Namespace = "", IsNullable = false )]
	public class LightspeedProductList
	{
		[XmlElement( typeof( LightspeedProduct ) )]
		public LightspeedProduct[] Item { get; set; }
	}
}
