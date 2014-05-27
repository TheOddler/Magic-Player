using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnidecodeSharpFork;

public delegate void SimpleEventHandler();

public static class Helpers {
	
	public static void Shuffle<T>(this IList<T> list)  
	{  
		int n = list.Count;  
		while (n > 1) {  
			n--;  
			int k = Random.Range(0, n + 1);  
			T value = list[k];  
			list[k] = list[n];  
			list[n] = value;  
		}  
	}
	
	public static string Wrap(this string text, int maxLettersWidth) {
		string[] words = text.Split(' ');
		var wrapped = new StringBuilder();
		
		int lastLineLength = words[0].Length;
		wrapped.Append(words[0]);
		
		for (int i = 1; i < words.Length; ++i) {
			string word = words[i];
			int wordLength = word.Length;
			if (lastLineLength + wordLength > maxLettersWidth) {
				wrapped.AppendLine();
				lastLineLength = 0;
			}
			
			lastLineLength += wordLength;
			wrapped.Append(word);
		}
		
		return wrapped.ToString();
	}
	
	
	public static string Simplify(this string name) {
		return name.Unidecode().ToLowerInvariant();
	}
	
}
