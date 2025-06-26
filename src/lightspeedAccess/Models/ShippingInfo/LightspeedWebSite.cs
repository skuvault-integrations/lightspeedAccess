using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace lightspeedAccess.Models.ShippingInfo
{
	[ XmlType( "ContactWebsite" ) ]
	public class LightspeedWebsite
	{
		public string url{ get; set; }
	}
}