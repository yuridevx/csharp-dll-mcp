using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace McpNetDll.Helpers;

public static class IdentifierMeaningFilter
{
	// Precompiled regex to split identifiers into tokens: handles camelCase, PascalCase, underscores, digits
	private static readonly Regex SplitRegex = new(
		@"(?<!^)(?=[A-Z])|[_\-\s]+|(?<=[a-zA-Z])(?=\d)|(?<=\d)(?=[a-zA-Z])",
		RegexOptions.Compiled);

	// Rejects if name appears base64-like or random: many consonants in a row, no vowels, etc.
	private static readonly Regex LooksRandomRegex = new("^[A-Za-z0-9]{6,}$", RegexOptions.Compiled);

	public static bool HasMeaningfulName(string? identifier)
	{
		if (string.IsNullOrWhiteSpace(identifier)) return false;

		// Keep common .NET special names
		if (identifier.StartsWith("get_", StringComparison.Ordinal) ||
			identifier.StartsWith("set_", StringComparison.Ordinal))
		{
			return true;
		}

		var raw = identifier!.Trim('_');
		if (raw.Length <= 2) return true; // short names like x, y, id — keep

		// Quick heuristic: if it looks purely random alphanumeric and not split into words, scrutinize more
		var tokens = Tokenize(raw);
		if (tokens.Count == 0) return false;

		// Score tokens: count how many are valid English words
		int englishHits = 0;
		int dictionaryChecked = 0;

		foreach (var token in tokens)
		{
			if (string.IsNullOrWhiteSpace(token)) continue;
			var t = token.Trim('_').ToLowerInvariant();
			if (t.Length == 0) continue;
			// Ignore very short tokens except well-known ones
			if (t.Length <= 2 && t is not ("id" or "io" or "ui" or "db")) continue;
			dictionaryChecked++;
			if (EnglishWordIndex.Contains(t)) englishHits++;
		}

		// Accept if we have at least one dictionary word and it forms at least 40% of checked tokens
		if (englishHits > 0 && englishHits * 100 >= Math.Max(1, dictionaryChecked) * 40) return true;

		// If no hits but the original contains vowels and common dev words, be lenient
		if (ContainsVowel(raw) && ContainsCommonDevSubstrings(raw)) return true;

		// Otherwise, likely obfuscated
		return false;
	}

	public static List<string> Tokenize(string identifier)
	{
		var parts = SplitRegex.Split(identifier)
			.Where(p => !string.IsNullOrWhiteSpace(p))
			.SelectMany(SplitAllCaps)
			.ToList();
		return parts;
	}

	private static IEnumerable<string> SplitAllCaps(string token)
	{
		// Split ALLCAPS by vowels boundaries to attempt partial tokens (e.g., HTTPServer -> HTTP, Server)
		if (token.Length >= 4 && token.All(char.IsUpper))
		{
			// Keep as-is and also try splitting into smaller chunks of length 2-4
			yield return token;
			for (int size = 2; size <= 4; size++)
			{
				for (int i = 0; i + size <= token.Length; i += size)
				{
					yield return token.Substring(i, size);
				}
			}
		}
		else
		{
			yield return token;
		}
	}

	private static bool ContainsVowel(string s)
	{
		foreach (var ch in s)
		{
			var c = char.ToLowerInvariant(ch);
			if (c is 'a' or 'e' or 'i' or 'o' or 'u' or 'y') return true;
		}
		return false;
	}

	private static bool ContainsCommonDevSubstrings(string s)
	{
		var lower = s.ToLowerInvariant();
		return lower.Contains("get") || lower.Contains("set") || lower.Contains("add") || lower.Contains("remove") || lower.Contains("init") || lower.Contains("load") || lower.Contains("save") || lower.Contains("read") || lower.Contains("write") || lower.Contains("name") || lower.Contains("value");
	}
}


