using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SowAndReap
{
	class ReaperNode
	{
		public readonly string Header;
		public readonly List<ReaperAttribute> Attributes = new List<ReaperAttribute>();

		public string Name
		{
			get
			{
				return ReaperAttribute.ExtractName(Header);
            }
		}

		public ReaperNode(string header)
		{
			Header = header;
		}

		public override string ToString()
		{
			return Header;
		}
	}
}
