using Amazon;
using Amazon.Runtime;
using Amazon.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;


namespace credential_manager
{
    class Program
    {
        static void Main(string[] args)
        {
            ReportDefault();

            try
            {
                char input;
                bool showPrompt = true;
                List<string> names = null;

                do
                {
                    if (showPrompt)
                    {
                        Console.WriteLine();
                        names = ListCredentials(false);

                        Console.WriteLine();
                        Console.WriteLine("A: Add    stored credential");
                        Console.WriteLine("R: Remove stored credential");
                        Console.WriteLine("D: Set Default Credential");
                        Console.WriteLine("X: Exit\n");
                    }

                    input = Console.ReadKey(true).KeyChar.ToString().ToLower()[0];
                    showPrompt = true;

                    switch (input)
                    {
                        case 'a':
                            {
                                var profileName = ReadLine("Profile name: "); if (profileName.Length == 0) break;
                                var accessKey = ReadLine("Access key: "); if (accessKey.Length == 0) break;
                                var secretKey = ReadLine("Secret key: "); if (secretKey.Length == 0) break;
                                var region = ReadRegion();
                                ProfileManager.RegisterProfile(profileName, accessKey, secretKey);
                                break;
                            }
                        case 'r':
                            {
                                Console.Write("Select credential to remove (use arrows): ");
                                string selected = StringChoice(names);
                                if (!string.IsNullOrEmpty(selected))
                                {
                                    Console.WriteLine("\nRemoving " + selected);
                                    ProfileManager.UnregisterProfile(selected);
                                }
                                break;
                            }
                        case 'd':
                            {
                                Console.Write("Set credential as default (use arrows): ");
                                string selected = StringChoice(names);
                                if (!string.IsNullOrEmpty(selected))
                                {
                                    Console.WriteLine("\nSetting default to " + selected);
                                    AWSCredentials creds = ProfileManager.GetAWSCredentials(selected);
                                    SetDefaultCredential(creds);
                                }
                                break;
                            }

                        case 'y':
                            {
                                ListCredentials(true); //list with details
                                break;
                            }
                        case 'z':
                            {
                                string region = ReadRegion();
                                Console.WriteLine("You chose: " + region);
                                break;
                            }
                        default:
                            showPrompt = false;
                            break;
                    }
                } while (input != 'x' && input != 'q');
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);

                if (System.Diagnostics.Debugger.IsAttached)
                {
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                }
            }
        }

        /// <summary>
        /// note: this can only be done once per session. 
        /// </summary>
        private static void ReportDefault()
        {
            try
            {
                Console.WriteLine("Current default account:");
                Console.WriteLine(Operations.IamOperations.UserInfo());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void AddStoredCredential(AWSCredentials creds, string profileName)
        {
            RunConfigure(creds, string.Format("set aws_access_key_id {0} --profile {1}", creds.GetCredentials().AccessKey, profileName));
            RunConfigure(creds, string.Format("set aws_secret_access_key {0} --profile {1}", creds.GetCredentials().SecretKey, profileName));
        }

        private static void SetDefaultCredential(AWSCredentials creds)
        {
            RunConfigure(creds, string.Format("set aws_access_key_id {0}", creds.GetCredentials().AccessKey));
            RunConfigure(creds, string.Format("set aws_secret_access_key {0}", creds.GetCredentials().SecretKey));
        }

        private static void RunConfigure(AWSCredentials creds, string operation)
        {
            Process p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = "aws";
            p.StartInfo.Arguments = "configure " + operation;
            p.Start();
            string strOutput = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            if (!string.IsNullOrEmpty(strOutput)) Console.WriteLine(strOutput);
        }

        private static List<string> ListCredentials(bool showall)
        {
            List<string> sortedNames;
            sortedNames = ProfileManager.ListProfileNames().OrderBy(p => p).ToList();

            if (sortedNames.Count == 0)
            {
                Console.WriteLine("No credentials are currently stored.  Type A to add one.");
            }
            else
            {
                Console.WriteLine("=== Stored Credetials ===");
                foreach (var profileName in sortedNames)
                {
                    var creds = ProfileManager.GetAWSCredentials(profileName).GetCredentials();
                    if (showall)
                        Console.WriteLine(string.Format("{0} {1} {2}", profileName, creds.AccessKey, creds.SecretKey));
                    else
                        Console.WriteLine(string.Format("{0} {1}",profileName, creds.AccessKey));
                }
            }
            return sortedNames;
        }

        private static string ReadRegion()
        {
            Console.Write("Region (use arrows): ");

            List<string> regions = new List<string>();
            foreach (var s in RegionEndpoint.EnumerableAllRegions) regions.Add(s.SystemName);

            return StringChoice(regions);
        }

        /// <summary>
        /// Allow the user to choose between the given choices, using arrow keys only.
        /// Doesn't support a default option at the moment.
        /// Escape key returns an empty string.
        /// </summary>
        private static string StringChoice(List<string> choices)
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

        private static string ReadLine(string prompt)
        {
            Console.Write(prompt);
            return Console.ReadLine();
        }
    }
}
