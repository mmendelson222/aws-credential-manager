using System;
using System.Collections.Generic;
using System.Linq;

namespace credential_manager.Utilities
{
    /// <summary>
    /// 3/9/17 Changed filtering behavior
    /// </summary>
    public class StringChoice
    {
        public static object Read(Dictionary<string, object> choices)
        {
            var selected = Read(choices.Keys.ToList());
            if (string.IsNullOrEmpty(selected)) return null;
            return choices[selected];
        }

        public static string Read(List<string> choices)
        {
            return (new Utilities.StringChoice(choices).ReadInternal());
        }

        public static string Read(List<string> choices, string defaultChoice)
        {
            int selected = choices.FindIndex(s => string.Compare(s, defaultChoice, true, System.Globalization.CultureInfo.CurrentCulture) == 0);
            if (selected < 0)
                return Read(choices);
            else
                return (new Utilities.StringChoice(choices) { idx = selected }).ReadInternal();
        }

        public static string Read(List<string> choices, int defaultChoice)
        {
            return (new Utilities.StringChoice(choices) { idx = defaultChoice }).ReadInternal();
        }

        List<string> choices;
        int maxLength = 0;

        int idx = -1;                       //currently selected index
        string sMatch = string.Empty;       //last successful match, or empty.

        bool Matching { get { return !string.IsNullOrEmpty(sMatch); } }

        public StringChoice(List<string> choices)
        {
            this.choices = choices;
            if (choices != null)
                //determine max string length for display purposes.
                foreach (string s in choices) maxLength = maxLength > s.Length ? maxLength : s.Length;
        }

        /// <summary>
        /// Allow the user to choose between the given choices, using arrow keys or by typing the first few characters.  
        /// Set idx for default option. 
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

            string currentChoice;
            ConsoleKey ch = ConsoleKey.NoName;

            while (ch != ConsoleKey.Enter && ch != ConsoleKey.Escape)
            {
                currentChoice = idx == -1 ? string.Empty : choices[idx];
                WriteString(currentChoice);

                var keyInfo = Console.ReadKey(true);
                ch = keyInfo.Key;
                Console.Write(new String('\b', currentChoice.Length));  //note: cursor at start of string. 

                switch (ch)
                {
                    case ConsoleKey.DownArrow:
                    case ConsoleKey.RightArrow:
                        if (Matching)
                        {
                            if (choices[idx + 1].StartsWith(sMatch, true, System.Globalization.CultureInfo.CurrentCulture))
                                idx++;
                            else
                                idx = choices.FindIndex(s => s.StartsWith(sMatch, true, System.Globalization.CultureInfo.CurrentCulture));
                        }
                        else
                        {
                            idx++;
                            if (idx >= choices.Count) idx = 0;
                        }
                        break;

                    case ConsoleKey.UpArrow:
                    case ConsoleKey.LeftArrow:
                        if (Matching)
                        {
                            if (choices[idx - 1].StartsWith(sMatch, true, System.Globalization.CultureInfo.CurrentCulture))
                                idx--;
                            else
                                idx = choices.FindLastIndex(s => s.StartsWith(sMatch, true, System.Globalization.CultureInfo.CurrentCulture));
                        }
                        else
                        {
                            idx--;
                            if (idx < 0) idx = choices.Count - 1;
                        }
                        break;

                    case ConsoleKey.Escape:
                        if (Matching)
                        {
                            sMatch = string.Empty;  //stop matching
                            ch = 0;                 //override escape
                        }
                        else
                        {
                            if (idx > -1)
                                Console.Write(new String(' ', choices[idx].Length)); //reset cursor
                            idx = -1;
                        }
                        break;

                    case ConsoleKey.Backspace:
                    case ConsoleKey.Delete:
                        if (sMatch.Length > 0)
                        {
                            TryMatch(sMatch.Substring(0, sMatch.Length - 1));
                            if (sMatch.Length == 0) idx = -1;  //expected behavior: nothing selected
                        }
                        break;

                    default:
                        if (IsPrintable(keyInfo.KeyChar))
                        {
                            TryMatch(sMatch + keyInfo.KeyChar);
                        }
                        break;
                }
            }

            string selected = idx == -1 ? null : choices[idx];
            Console.WriteLine(selected);
            return selected;
        }

        /// <summary>
        /// write the current selection, with padding if necessary to overwrite previous selection.
        /// </summary>
        private void WriteString(string currentChoice)
        {
            WriteWithHighlighting(currentChoice, sMatch.Length);
            int trailingBlanks = maxLength - currentChoice.Length;  //pad with spaces and reset cursor location.
            Console.Write(new String(' ', trailingBlanks));
            Console.Write(new String('\b', trailingBlanks));  //reset cursor to end of current choice. 
        }

        /// <summary>
        /// Attempt a match with the given string.
        /// If successful, adjust current match string and selected index.
        /// </summary>
        private void TryMatch(string tryMatch)
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
        private static void WriteWithHighlighting(string s, int length)
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
        private static bool IsPrintable(char c)
        {
            return c >= ' ' && c <= '~';
        }
    }
}
