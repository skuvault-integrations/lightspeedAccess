using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SkuVault.Lightspeed.Access.Models.Order;

namespace SkuVault.Lightspeed.Access
{
	public interface ILightspeedOrdersService
	{
		IEnumerable< LightspeedOrder > GetOrders( DateTime dateFrom, DateTime dateTo );
		Task< IEnumerable< LightspeedOrder > > GetOrdersAsync( DateTime dateFrom, DateTime dateTo, CancellationToken ctx );
	}
}