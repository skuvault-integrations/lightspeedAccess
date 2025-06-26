using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace lightspeedAccess.Models.ShippingInfo
{
	[ XmlType( "ContactEmail" ) ]
	public class LightspeedEmail
	{
		public string address{ get; set; }
		public string useType{ get; set; }
	}
}