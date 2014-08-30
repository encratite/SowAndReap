namespace SowAndReap
{
	class Source
	{
		public readonly string FileName;
		public readonly string Channel;
		public decimal? Position = null;
		public decimal? Length = null;

		public Source(string fileName, string channel)
		{
			FileName = fileName;
			Channel = channel;
		}
	}
}
