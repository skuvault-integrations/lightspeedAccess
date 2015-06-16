﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace lightspeedAccess.Models.Account
{
	[ XmlType( "Account" ) ]
	class LightspeedAccountInfo
	{
		[XmlElement( "accountID" )]
		public int AccountId { get; set; }
	}

	[ XmlRoot( "Accounts", Namespace = "", IsNullable = false ) ]
	class LightspeedAccountList
	{
		[XmlElement( typeof( LightspeedAccountInfo ) )]
		public LightspeedAccountInfo[] Account { get; set; }
	}
}
