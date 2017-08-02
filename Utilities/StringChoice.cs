using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities
{
    /// <summary>
    /// 3/9/17 Changed filtering behavior
    /// 4/20/14 Added integer parsing, YesNo etc., internal class
    /// </summary>
    public static class StringChoice
    {
        static StringChoice() { Audible = true; }
        public static bool Audible { get; set; }

        public static object Read(Dictionary<string, object> choices)
        {
            var selected = Read(choices.Keys.ToList());
            if (string.IsNullOrEmpty(selected)) return null;
            return choices[selected];
        }

        public static string Read(List<string> choices)
        {
            return (new StringChoiceInternal(choices).AcceptUserInput());
        }

        public static string Read(List<string> choices, string defaultChoice)
        {
            if (defaultChoice == null) defaultChoice = string.Empty;
            int selected = choices.FindIndex(s => string.Compare(s, defaultChoice, true, System.Globalization.CultureInfo.CurrentCulture) == 0);
            if (selected < 0)
                return Read(choices);
            else
                return (new StringChoiceInternal(choices, selected)).AcceptUserInput();
        }

        public static string Read(List<string> choices, int defaultChoice)
        {
            return (new StringChoiceInternal(choices, defaultChoice)).AcceptUserInput();
        }

        public static int ReadInteger()
        {
            var b = new StringBuilder();
            var key = new ConsoleKeyInfo();
            while (key.Key != ConsoleKey.Enter && key.Key != ConsoleKey.Escape)
            {
                key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter)
                {
                }
                else if (key.Key == ConsoleKey.Escape)
                {
                    Console.Write(new string('\b', b.Length));
                    Console.Write(new string(' ', b.Length));
                    Console.Write(new string('\b', b.Length));
                    b.Length = 0;
                }
                else
                {
                    if (key.Key == ConsoleKey.Backspace && b.Length > 0)
                    {
                        b.Length--;
                        Console.Write("\b \b");
                    }
                    else if ("0123456789".Contains(key.KeyChar))
                    {
                        b.Append(key.KeyChar);
                        Console.Write(key.KeyChar);
                    }
                    else
                    {
                        CharRejected();
                    }
                }
            }

            int i;
            if (!int.TryParse(b.ToString(), out i))
                return -1;
            else
                return i;
        }

        /// <summary>
        /// just tell us if this is a quit character.
        /// </summary>
        internal static bool IsQuit(char c)
        {
            return ((new char[] { 'x', 'q' }).Contains(c.ToString().ToLower()[0]));
        }

        /// <summary>
        /// Prompt the user and accept y/n.  Return boolean. 
        /// </summary>
        internal static bool YesNo(string prompt)
        {
            bool doPrompt = true;  //only re-prompt if audible signal turned off. 
            do
            {
                if (doPrompt) Console.Write(prompt + " (y/n) ");
                var keyInfo = Console.ReadKey(true);
                char entered = keyInfo.KeyChar.ToString().ToLower()[0];
                switch (entered)
                {
                    case 'y': Console.WriteLine(entered); return true;
                    case 'n': Console.WriteLine(entered); return false;
                    default: doPrompt = !CharRejected(); break;
                }
            } while (true);
        }

        /// <summary>
        /// check KeyAvailable for a character.  Continue, cancel (if x/q) or pause with option to cancel.
        /// </summary>
        internal static bool CheckForCancel()
        {
            bool canceled = false;
            if (Console.KeyAvailable)
            {
                canceled = StringChoice.IsQuit(Console.ReadKey(true).KeyChar); //if user pressed x or q, just cancel.
                if (!canceled)
                    canceled = StringChoice.YesNo("Pausing operation.  Cancel? ");       //or ask poliely.
            }
            return canceled;
        }

        #region private static methods
        internal static void WriteWithHighlighting(string s)
        {
            WriteWithHighlighting(s, s.Length);
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

        /// <summary>
        /// Action to take if a character is rejected (e.g. beep).  Returns true if some action was taken.
        /// </summary>
        private static bool CharRejected()
        {
            if (Audible)
            {
                System.Media.SystemSounds.Beep.Play();
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        class StringChoiceInternal
        {
            internal StringChoiceInternal(List<string> choices)
            {
                this.choices = choices;
                if (choices != null)
                    //determine max string length for display purposes.
                    foreach (string s in choices) maxLength = maxLength > s.Length ? maxLength : s.Length;
            }

            internal StringChoiceInternal(List<string> choices, int selectedIndex) : this(choices)
            {
                idx = selectedIndex;
            }

            /// <summary>
            /// The GUTS of StringChoice.
            /// Allow the user to choose between the given choices, using arrow keys or by typing the first few characters.  
            /// Set idx for default option. 
            /// Return key:  Select current option (or empty string if no option shown).
            /// Escape key:  Returns an empty string.
            /// </summary>
            internal string AcceptUserInput()
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
                                if (idx == choices.Count - 1)   //end of overall list.  special case.
                                    idx = choices.FindIndex(s => s.StartsWith(sMatch, true, System.Globalization.CultureInfo.CurrentCulture));
                                else if (choices[idx + 1].StartsWith(sMatch, true, System.Globalization.CultureInfo.CurrentCulture))
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
                                if (idx == 0)   //start of overall list.  special case.
                                    idx = choices.FindLastIndex(s => s.StartsWith(sMatch, true, System.Globalization.CultureInfo.CurrentCulture));
                                else if (choices[idx - 1].StartsWith(sMatch, true, System.Globalization.CultureInfo.CurrentCulture))
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

            #region private methods (stringchoice)
            private List<string> choices;
            private int maxLength = 0;

            private int idx = -1;                       //currently selected index
            private string sMatch = string.Empty;       //last successful match, or empty.

            private bool Matching { get { return !string.IsNullOrEmpty(sMatch); } }

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
                else
                {
                    CharRejected();
                }
            }
            #endregion
        }
    }
}