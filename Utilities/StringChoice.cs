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
        /// Escape key returns an empty string.
        /// </summary>
        public static string Read(List<string> choices)
        {
            //could support default choice by passing in the index.  
            int idx = -1;
            ConsoleKey ch = ConsoleKey.NoName;
            if (idx > -1)
                Console.Write(choices[idx]);

            int maxLength = 0; foreach (string s in choices) maxLength = maxLength > s.Length ? maxLength : s.Length;
            while (ch != ConsoleKey.Enter && ch != ConsoleKey.Escape)
            {
                ch = Console.ReadKey(true).Key;
                if (idx > -1)
                    Console.Write(new String('\b', choices[idx].Length)); //reset cursor

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
    }
}
