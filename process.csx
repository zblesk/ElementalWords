using System.Globalization;

using (var stream = new StreamReader("en_US.dic"))
using (var output = new StreamWriter("outdict_en_US.txt"))
{
	while (!stream.EndOfStream)
	{
		var word = stream.ReadLine().Trim();
		var slashPos = word.IndexOf('/');
		if (slashPos > 0)
		{
			word = word.Substring(0, slashPos);
		}
		output.WriteLine(word.ToString(CultureInfo.InvariantCulture));
	}
}