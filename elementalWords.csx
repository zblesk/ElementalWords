using System.Globalization;
using System.Linq;
using System.Collections.Generic;

var elements = new List<string> {"h", "he", "li", "be", "b", "c", "n", "o", "f", "ne", "na", "mg", "al",
	"si", "p", "s", "cl", "ar", "k", "ca", "sc", "ti", "v", "cr", "mn", "fe", "co", "ni", "cu", "zn",
	"ga", "ge", "as", "se", "br", "kr", "rb", "sr", "y", "zr", "nb", "mo", "tc", "ru", "rh", "pd", "ag",
	"cd", "in", "sn", "sb", "te", "i", "xe", "cs", "ba", "lu", "hf", "ta", "w", "re", "os", "ir", "pt",
	"au", "hg", "tl", "pb", "bi", "po", "at", "rn", "fr", "ra", "lr", "rf", "db", "sg", "bh", "hs",
	"mt", "ds", "rg", "cn", "uut", "fl", "uup", "lv", "uus", "uuo", "la", "ce", "pr", "nd", "pm", "sm",
	"eu", "gd", "tb", "dy", "ho", "er", "tm", "yb", "ac", "th", "pa", "u", "np", "pu", "am", "cm", "bk",
	"cf", "es", "fm", "md", "no" };

static string FormatElement(string el)
{
	return Char.ToUpper(el[0]) + el.Substring(1);
}

static string RemoveDiacritics(string text)
{
    var normalizedString = text.Normalize(NormalizationForm.FormD);
    var stringBuilder = new StringBuilder();

    foreach (var c in normalizedString)
    {
        var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
        if (unicodeCategory != UnicodeCategory.NonSpacingMark)
        {
            stringBuilder.Append(c);
        }
    }
    return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
}

class QueueItem
{
	public string OriginalWord { get; private set; }
	public string Remainder { get; private set; }
	private List<string> Elements = new List<string>();

	public QueueItem(
		string word)
	{
		OriginalWord = word;
		Remainder = word;
	}

	public QueueItem AddElement(
		string element)
	{
		if (!Remainder.StartsWith(element, StringComparison.InvariantCultureIgnoreCase))
		{
			throw new ArgumentException(string.Format(
				"Invalid element {0} for string {1}",
				element,
				Remainder));
		}
		var newItem = new QueueItem(OriginalWord);
		newItem.Remainder = Remainder.Substring(element.Length);
		newItem.Elements.AddRange(Elements);
		newItem.Elements.Add(element);
		return newItem;
	}

	public string GetElements()
	{
		return Elements.Aggregate(
				"",
				(acc, nxt) => acc + FormatElement(nxt));
	}

	public void AddElementCounts(Dictionary<string, int> counts)
	{
		foreach (var element in Elements)
		{
		    if (!counts.ContainsKey(element))
		    {
		    	counts[element] = 0;
		    }
		    counts[element]++;
		}
	}

	public override string ToString()
	{
		return string.Format("{0} ({1})",
			OriginalWord,
			GetElements());
	}
}

IEnumerable<QueueItem> FindAllEncodings(string normalWord)
{
	var queue = new Queue<QueueItem>();
	queue.Enqueue(new QueueItem(normalWord));
	while (queue.Any())
	{
		var item = queue.Dequeue();
		if (item.Remainder.Length == 0)
		{
			yield return item;
		}
		for (int len = 1; len <= 3; len++)
		{
			if (item.Remainder.Length >= len)
			{
				var prefix = item.Remainder.Substring(0, len);
				if (elements.Contains(prefix))
				{
					queue.Enqueue(item.AddElement(prefix));
				}
			}
		}
	}
}

const int max_count = 40;

using (var stream = new StreamReader("outdict_en_US.txt"))
{
	var converted = 0;
	var longestLen = 0;
	var longestWords = new List<string>();
	var mostVariationsCount = 0;
	var mostVariedWords = new List<string>();
	var counts = new int[max_count];
	var unconverted = 0;
	var elementCounts = new Dictionary<string, int>();
	while (!stream.EndOfStream)
	{
		var word = stream.ReadLine().Trim().ToLower();
		var encodings =  FindAllEncodings(RemoveDiacritics(word)).ToList();

		if (encodings.Count > mostVariationsCount)
		{
			mostVariationsCount = encodings.Count;
			mostVariedWords = new List<string>();
		}
		if (encodings.Count == mostVariationsCount)
		{
			mostVariedWords.Add(word);
		}
		counts[encodings.Count]++;

		if (encodings.Any())
		{
			if (word.Length > longestLen)
			{
				longestLen = word.Length;
				longestWords = new List<string>();
			}
			if (word.Length == longestLen)
			{
				longestWords.Add(word);
			}

			foreach (var enc in encodings)
			{
			    enc.AddElementCounts(elementCounts);
			}

			Console.WriteLine(
				"{0} - {1}",
				word,
				string.Join(
                    ", ",
                    encodings.Select(e => e.GetElements())));
			converted++;
		}
		else
		{
			unconverted++;
		}
	}
	Console.WriteLine(
		"\nConverted: {0}\nUnconverted: {1}",
		converted,
		unconverted);
	Console.WriteLine(
		"\nLongest: {0} - {1}",
		longestLen,
		longestWords.Aggregate("", (a, w) => a + '\n' + w ));
	Console.WriteLine("\nMost variations: {0} - {1}",
		mostVariationsCount,
		mostVariedWords.Aggregate("", (a, w) => a + '\n' + w ));

	Console.WriteLine("Counts:");
	for (int i = 0; i < max_count; i++)
	{
	    if (counts[i] > 0)
	    {
	    	Console.WriteLine("{0} variations \t{1} times.", i, counts[i]);
	    }
	}
	Console.WriteLine();

	var elementCountList = elementCounts.ToList();
	elementCountList.Sort((firstPair, nextPair) =>
    {
        return firstPair.Value.CompareTo(nextPair.Value) * -1;
    });
	foreach (var kvp in elementCountList)
	{
	    Console.WriteLine("{0}\t{1}", FormatElement(kvp.Key), kvp.Value);
	}
}