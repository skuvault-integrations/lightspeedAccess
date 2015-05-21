using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using lightspeedAccess.Models.Request;

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
	public class LightspeedProductList : IPaginatedResponse
	{
		[XmlElement( typeof( LightspeedProduct ) )]
		public LightspeedProduct[] Item { get; set; }

		public int count { get; set; }

		public int GetCount()
		{
			return count;
		}

		public void Aggregate( IPaginatedResponse other )
		{
			var otherTyped = ( LightspeedProductList ) other;
			if ( otherTyped != null )
			{
				if ( otherTyped.Item != null )
					Item = Item.Concat( otherTyped.Item ).ToArray();
			}
		}
	}
}
