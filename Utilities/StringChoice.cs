using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace credential_manager.Utilities
{
    public class StringChoice
    {
        /// <summary>
        /// Allow the user to choose between the given choices, using arrow keys only.  
        /// Doesn't support a default option at the moment.
        /// Return key:  Select current option (or empty string if no option shown).
        /// Escape key:  Returns an empty string.
        /// </summary>
        public static string Read(List<string> choices)
        {
            if (choices == null || choices.Count == 0 ){
                Console.WriteLine("[no options]");
                return string.Empty;
            }

            //could support default choice by passing in the index.  
            int idx = -1;
            ConsoleKey ch = ConsoleKey.NoName;
            if (idx > -1)
                Console.Write(choices[idx]);

            //determine max string length for display purposes.
            int maxLength = 0;
            foreach (string s in choices) maxLength = maxLength > s.Length ? maxLength : s.Length;

            string sMatch = string.Empty;

            while (ch != ConsoleKey.Enter && ch != ConsoleKey.Escape)
            {
                var keyInfo = Console.ReadKey(true);
                ch = keyInfo.Key;
                if (idx > -1)
                    Console.Write(new String('\b', choices[idx].Length)); //reset cursor

                //non-printable characters reset search
                if (!IsPrintable(keyInfo.KeyChar)) sMatch = string.Empty;

                switch (ch)
                {
                    case ConsoleKey.UpArrow:
                    case ConsoleKey.RightArrow:
                        idx++;
                        if (idx >= choices.Count) idx = 0;
                        break;

                    case ConsoleKey.DownArrow:
                    case ConsoleKey.LeftArrow:
                        idx--;
                        if (idx < 0) idx = choices.Count - 1;
                        break;

                    case ConsoleKey.Escape:
                        if (idx > -1)
                            Console.Write(new String(' ', choices[idx].Length)); //reset cursor
                        idx = -1;
                        break;

                    default:
                        if (IsPrintable(keyInfo.KeyChar))
                        {
                           sMatch += ch;
                           int matchIdx = choices.FindIndex(s => s.StartsWith(sMatch, true, System.Globalization.CultureInfo.CurrentCulture));
                           if (matchIdx > -1) idx = matchIdx;
                        }
                        break;
                }

                if (idx > -1)
                {
                    Console.Write(choices[idx]);
                    int trailingBlanks = maxLength - choices[idx].Length;  //pad with spaces and reset cursor location.
                    Console.Write(new String(' ', trailingBlanks));
                    Console.Write(new String('\b', trailingBlanks));
                }
                //Console.WriteLine(idx);
            }

            Console.WriteLine();
            if (idx == -1)
                return string.Empty;
            return choices[idx];
        }

        /// <summary>
        /// Exclude backspace, CR, LF etc.  
        /// If choices include those, all bets are off.
        /// https://en.wikipedia.org/wiki/ASCII#ASCII_printable_characters
        /// </summary>
        static bool IsPrintable(char c)
        {
            return c >= ' ' && c <= '~'; 
        }
    }
}
