using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace SowAndReap
{
	class Parser
	{
		const char NodePrefix = '<';
		const char NodeSuffix = '>';

		Dictionary<int, Source> _Sources = new Dictionary<int, Source>();
		int _ItemId = 10000;

		public void ReadArdourSession(string path)
		{
			var document = new XmlDocument();
			document.Load(path);
			var sessionNode = document.SelectSingleNode("//Session");
			int sampleRate = int.Parse(sessionNode.Attributes["sample-rate"].Value);
			var sourceNodes = sessionNode.SelectNodes("//Sources/Source[@name and @id and @captured-for]");
			foreach (XmlNode sourceNode in sourceNodes)
			{
				var attributes = sourceNode.Attributes;
                string fileName = attributes["name"].Value;
				int id = int.Parse(attributes["id"].Value);
				string channel = attributes["captured-for"].Value;
				var ardourSource = new Source(fileName, channel);
				_Sources[id] = ardourSource;
			}
			var regionNodes = sessionNode.SelectNodes("//Playlist/Region");
			foreach (XmlNode regionNode in regionNodes)
			{
				var attributes = regionNode.Attributes;
				int id = int.Parse(attributes["source-0"].Value);
				int position = int.Parse(attributes["position"].Value);
				int length = int.Parse(attributes["length"].Value);
				Source source;
				if (!_Sources.TryGetValue(id, out source))
					continue;
				source.Position = (decimal)position / sampleRate;
				source.Length = (decimal)length / sampleRate;
			}
		}

		public void WriteReaperProject(string inputPath, string outputPath)
		{
			var lines = File.ReadLines(inputPath).GetEnumerator();
			lines.MoveNext();
			var rootNode = ReadReaperNode(ref lines);
			var tracks = rootNode.Attributes.Where(attribute => !attribute.IsValue && attribute.Node.Name == "TRACK").Select(attribute => attribute.Node).ToList();
			foreach (var pair in _Sources)
			{
				var source = pair.Value;
				var track = tracks.Where(x => x.Attributes.Count > 0 && NameAttributeMatches(x.Attributes[0], source.Channel)).FirstOrDefault();
				if (track == null)
					throw new ApplicationException(string.Format("Unable to find channel \"{0}\"", source.Channel));
				AddItemToTrack(source, track);
			}
			using (var stream = new FileStream(outputPath, FileMode.Create))
			{
				stream.SetLength(0);
				using (var writer = new StreamWriter(stream))
				{
					WriteReaperNode(rootNode, writer);
	            }
			}
		}

		void AddItemToTrack(Source source, ReaperNode track)
		{
			var item = new ReaperNode("ITEM");
			Action<string> addAttribute = (value) => item.Attributes.Add(new ReaperAttribute(value));
			addAttribute(string.Format("POSITION {0}", source.Position));
			addAttribute("SNAPOFFS 0.00000000000000");
			addAttribute(string.Format("LENGTH {0}", source.Length));
			addAttribute("LOOP 1");
			addAttribute("ALLTAKES 0");
			addAttribute("SEL 0");
			addAttribute("FADEIN 1 0.010000 0.000000 1 0 0.000000");
			addAttribute("FADEOUT 1 0.010000 0.000000 1 0 0.000000");
			addAttribute("MUTE 0");
			addAttribute(string.Format("IGUID {{{0}}}", Guid.NewGuid()));
			addAttribute(string.Format("IID {0}", _ItemId));
			addAttribute(string.Format("NAME \"{0}\"", source.FileName));
			addAttribute("VOLPAN 1.000000 0.000000 1.000000 -1.000000");
			addAttribute("SOFFS 0.00000000000000");
			addAttribute("PLAYRATE 1.00000000000000 1 0.00000000000000 -1 0 0.002500");
			addAttribute("CHANMODE 0");
			addAttribute(string.Format("GUID {{{0}}}", Guid.NewGuid()));
			addAttribute("RECPASS 1500");
			var sourceWave = new ReaperNode("SOURCE WAVE");
			var sourceAttribute = new ReaperAttribute(string.Format("FILE \"{0}\"", source.FileName));
            sourceWave.Attributes.Add(sourceAttribute);
			var sourceWaveAttribute = new ReaperAttribute(sourceWave);
			item.Attributes.Add(sourceWaveAttribute);
			var itemAttribute = new ReaperAttribute(item);
			track.Attributes.Add(itemAttribute);
			_ItemId++;
        }

		bool NameAttributeMatches(ReaperAttribute attribute, string channel)
		{
			string target = string.Format("NAME \"{0}\"", channel);
			bool isMatch = attribute.IsValue && attribute.Value == target;
			return isMatch;
		}

		ReaperNode ReadReaperNode(ref IEnumerator<string> lines)
		{
			string header = lines.Current.Trim().Substring(1);
			var output = new ReaperNode(header);
			while (lines.MoveNext())
			{
				string line = lines.Current.Trim();
				if (line.Length == 0)
					continue;
				switch (line[0])
				{
					case NodePrefix:
						var node = ReadReaperNode(ref lines);
						var nodeAttribute = new ReaperAttribute(node);
						output.Attributes.Add(nodeAttribute);
						break;

					case NodeSuffix:
						return output;

					default:
						var valueAttribute = new ReaperAttribute(line);
						output.Attributes.Add(valueAttribute);
						break;
				}
			}
			throw new ApplicationException("Unable to find terminator");
		}

		void WriteReaperNode(ReaperNode node, StreamWriter writer)
		{
			writer.WriteLine(NodePrefix + node.Header);
			foreach (var attribute in node.Attributes)
			{
				if (attribute.IsValue)
					writer.WriteLine(attribute.Value);
				else
					WriteReaperNode(attribute.Node, writer);
			}
			writer.WriteLine(NodeSuffix);
		}
	}
}
