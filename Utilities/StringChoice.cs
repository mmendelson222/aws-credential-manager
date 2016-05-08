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
        /// shortcut method
        /// </summary>
        public static string Read(List<string> choices)
        {
            return (new Utilities.StringChoice(choices).ReadInternal());
        }


        List<string> choices;
        int maxLength = 0;

        int idx = -1;                       //currently selected index
        ConsoleKey ch = ConsoleKey.NoName;  //most recent keystroke
        ConsoleKeyInfo keyInfo;             //most recent keystroke
        string sMatch = string.Empty;       //last successful match, or empty.


        public StringChoice(List<string> choices)
        {
            this.choices = choices;
            if (choices != null)
                //determine max string length for display purposes.
                foreach (string s in choices) maxLength = maxLength > s.Length ? maxLength : s.Length;
        }


        /// <summary>
        /// Allow the user to choose between the given choices, using arrow keys only.  
        /// Doesn't support a default option at the moment.
        /// Return key:  Select current option (or empty string if no option shown).
        /// Escape key:  Returns an empty string.
        /// </summary>
        public string ReadInternal()
        {
            if (maxLength == 0)
            {
                Console.WriteLine("[no options]");
                return string.Empty;
            }

            //could support default choice by passing in the index.  
            if (idx > -1)
                Console.Write(choices[idx]);

            while (ch != ConsoleKey.Enter && ch != ConsoleKey.Escape)
            {
                var keyInfo = Console.ReadKey(true);
                ch = keyInfo.Key;
                if (idx > -1)
                    Console.Write(new String('\b', choices[idx].Length)); //reset cursor

                switch (ch)
                {
                    case ConsoleKey.UpArrow:
                    case ConsoleKey.RightArrow:
                        idx++;
                        if (idx >= choices.Count) idx = 0;
                        sMatch = string.Empty;  //reset match
                        break;

                    case ConsoleKey.DownArrow:
                    case ConsoleKey.LeftArrow:
                        idx--;
                        if (idx < 0) idx = choices.Count - 1;
                        sMatch = string.Empty;  //reset match
                        break;

                    case ConsoleKey.Escape:
                        if (idx > -1)
                            Console.Write(new String(' ', choices[idx].Length)); //reset cursor
                        idx = -1;
                        break;

                    case ConsoleKey.Backspace:
                    case ConsoleKey.Delete:
                        if (sMatch.Length > 0)
                            AttemptMatch(sMatch.Substring(0, sMatch.Length - 1));
                        break;

                    default:
                        if (IsPrintable(keyInfo.KeyChar))
                            AttemptMatch(sMatch + keyInfo.KeyChar);
                        break;
                }

                if (idx > -1)
                {
                    WriteWithHighlighting(choices[idx], sMatch.Length);
                    int trailingBlanks = maxLength - choices[idx].Length;  //pad with spaces and reset cursor location.
                    Console.Write(new String(' ', trailingBlanks));
                    Console.Write(new String('\b', trailingBlanks));
                }
            }

            Console.WriteLine();
            if (idx == -1)
                return string.Empty;
            return choices[idx];
        }

        /// <summary>
        /// Attempt a match with the given string.
        /// If successful, adjust current match string and selected index.
        /// </summary>
        private void AttemptMatch(string tryMatch)
        {
            int matchIdx = choices.FindIndex(s => s.StartsWith(tryMatch, true, System.Globalization.CultureInfo.CurrentCulture));
            if (matchIdx > -1)
            {
                sMatch = tryMatch;
                idx = matchIdx;
            }
        }

        /// <summary>
        /// write the first "length" characters reversed.
        /// </summary>
        static void WriteWithHighlighting(string s, int length)
        {
            var fg = Console.ForegroundColor;
            var bg = Console.BackgroundColor;
            Console.ForegroundColor = bg;
            Console.BackgroundColor = fg;
            Console.Write(s.Substring(0, length));
            Console.ResetColor();
            Console.Write(s.Substring(length));
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
