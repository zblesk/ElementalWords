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

private static string RemoveDiacritics(string text)
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
				(acc, nxt) => acc + Char.ToUpper(nxt[0]) + nxt.Substring(1));
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

using (var stream = new StreamReader("outdict.txt"))
{
	var converted = 0;
	var unconverted = 0;
	while (!stream.EndOfStream)
	{
		var word = stream.ReadLine().Trim().ToLower();
		var encodings =  FindAllEncodings(RemoveDiacritics(word)).ToList();
		if (encodings.Any())
		{
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
	Console.WriteLine("\nConverted: {0}\nUnconverted: {1}", converted, unconverted);
}