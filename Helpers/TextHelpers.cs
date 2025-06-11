using System.Text;

using Snap.Assets.Fonts;

namespace Snap.Helpers;

public static class TextHelpers
{
	public static string FormatText(Font font, string text, int width)
	{
		if (text.IsEmpty() || width == 0)
			return string.Empty;

		var sb = new StringBuilder();
		var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

		foreach (var line in lines)
		{
			if (line.IsEmpty())
			{
				sb.AppendLine(); // Preserve empty lines
				continue;
			}

			var words = line.Split(' ');
			var current = new StringBuilder();

			foreach (var word in words)
			{
				string testLine = current.Length > 0 ? $"{current} {word}" : word;

				// Check if adding the word exceeds width
				if (font.Measure(testLine).X >= width)
				{
					sb.AppendLine(current.ToString().Trim());
					current.Clear();
					current.Append(word);
				}
				else
				{
					if (current.Length > 0) current.Append(' ');
					current.Append(word);
				}
			}

			// Append last formatted line within this section
			if (current.Length > 0)
				sb.AppendLine(current.ToString().Trim());
		}

		return sb.ToString().TrimEnd(); // Trim only trailing newline
	}
}
