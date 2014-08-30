using System.Text.RegularExpressions;

namespace SowAndReap
{
	class ReaperAttribute
	{
		public readonly string Value;
		public readonly ReaperNode Node;

		public string Name
		{
			get
			{
				if (IsValue)
					return ReaperAttribute.ExtractName(Value);
				else
					return null;
			}
		}

		public bool IsValue
		{
			get
			{
				return Value != null;
			}
		}

		public ReaperAttribute(string value)
		{
			Value = value;
			Node = null;
		}

		public ReaperAttribute(ReaperNode node)
		{
			Value = null;
			Node = node;
		}

		public static string ExtractName(string value)
		{
			var pattern = new Regex("^[^ ]+");
			var match = pattern.Match(value);
			string output = match.Groups[0].Value;
			return output;
		}

		public override string ToString()
		{
			if (IsValue)
				return Value;
			else
				return Node.ToString();
		}
	}
}
