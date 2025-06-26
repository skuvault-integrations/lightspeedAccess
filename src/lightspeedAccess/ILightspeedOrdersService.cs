using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LightspeedAccess.Models.Order;

namespace LightspeedAccess
{
	public interface ILightspeedOrdersService
	{
		IEnumerable< LightspeedOrder > GetOrders( DateTime dateFrom, DateTime dateTo );
		Task< IEnumerable< LightspeedOrder > > GetOrdersAsync( DateTime dateFrom, DateTime dateTo, CancellationToken ctx );
	}
}