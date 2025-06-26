using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using lightspeedAccess.Models.Order;

namespace lightspeedAccess
{
	public interface ILightspeedOrdersService
	{
		IEnumerable< LightspeedOrder > GetOrders( DateTime dateFrom, DateTime dateTo );
		Task< IEnumerable< LightspeedOrder > > GetOrdersAsync( DateTime dateFrom, DateTime dateTo, CancellationToken ctx );
	}
}