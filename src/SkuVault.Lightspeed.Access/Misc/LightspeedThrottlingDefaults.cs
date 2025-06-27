namespace SkuVault.Lightspeed.Access.Misc
{
	static class LightspeedThrottlingDefaults
	{
		public const int LightspeedBucketSize = 180;
		public const int LightspeedDripRate = 1;
		public const int ReadRequestCost = 1;
		public const int WriteRequestCost = 10;
	}
}
