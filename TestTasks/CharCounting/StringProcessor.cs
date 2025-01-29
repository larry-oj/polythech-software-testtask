using System;
using System.Collections.Generic;
using System.Linq;

namespace TestTasks.VowelCounting
{
    public class StringProcessor
    {
        public (char symbol, int count)[] GetCharCount(string veryLongString, char[] countedChars)
        {
            var map = new Dictionary<char, int>();
            
            foreach (var @char in countedChars)
            {
                map[@char] = 0;
            }
            
            foreach (var @char in veryLongString.Where(@char => map.ContainsKey(@char)))
            {
                map[@char]++;
            }
            
            return countedChars.Select(c => (c, map[c])).ToArray();
        }
    }
}
