using UnityEngine;
using System;
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
			int k = UnityEngine.Random.Range(0, n + 1);  
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
	
	public static string BypassCrosdomain(string url, string format) {
		return
			"http://query.yahooapis.com/v1/public/yql?q=select%20*%20from%20html%20where%20url%3D'" +
				Uri.EscapeUriString(Uri.EscapeUriString(url)) + //Double encoding fixes the 400 error you get when having spaces
				"'%0A&format=" + format;
	}
	public static string RemoveBypassPadding(string reply) {
		int start = reply.LastIndexOf("<p>");
		if (start < 0) return "[]";
		reply = reply.Remove(0, start+3); //+3 for the <p> itself
		
		int end = reply.LastIndexOf("</p>");
		if (end < 0) return "[]";
		reply = reply.Remove(end); //removes the last xml padding, now it's just a json list
		
		return reply;
	}
	
	public static T RandomElement<T>(this List<T> list) {
		return list[UnityEngine.Random.Range(0, list.Count)];
	}
	
	public static bool ContainsLayer(this LayerMask mask, int layer) {
		return (mask.value & (1 << layer)) > 0;
	}
}
