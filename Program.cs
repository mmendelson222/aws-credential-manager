﻿using Amazon;
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
                        Console.WriteLine("S: Set    default credential");
                        Console.WriteLine("P: Push   to named credential");
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
                                ProfileManager.RegisterProfile(profileName, accessKey, secretKey);
                                break;
                            }
                        case 'r':
                            {
                                Console.Write("Select credential to remove (use arrows or type): ");
                                string selected = Utilities.StringChoice.Read(names);
                                if (!string.IsNullOrEmpty(selected))
                                {
                                    Console.WriteLine("\nRemoving " + selected);
                                    ProfileManager.UnregisterProfile(selected);
                                }
                                break;
                            }
                        case 's':
                            {
                                Console.Write("Set credential as default (use arrows or type): ");
                                string selected = Utilities.StringChoice.Read(names);
                                if (!string.IsNullOrEmpty(selected))
                                {
                                    Console.WriteLine("\nSetting default to " + selected);
                                    AWSCredentials creds = ProfileManager.GetAWSCredentials(selected);
                                    SetDefaultCredential(creds);
                                }
                                break;
                            }

                        //push credential to named credential
                        case 'p':
                            {
                                Console.Write("Select credential to push (use arrows or type): ");
                                string selected = Utilities.StringChoice.Read(names);
                                if (!string.IsNullOrEmpty(selected))
                                {
                                    //var region = ReadRegion();
                                    AWSCredentials creds = ProfileManager.GetAWSCredentials(selected);
                                    AddStoredCredential(creds, selected);
                                }

                                break;
                            }

                        //version info
                        case 'v':
                            Console.WriteLine(VersionInfo);
                            break;

                        case 'y':
                            ListCredentials(true); //list with details
                            break;

                        case 'z':
                            {
                                //List<string> choices = null;  //test case 1
                                //List<string> choices = new List<string>(); //test case 2

                                List<string> choices = new List<string>() { "one", "two", "three", "aaa", "bbb", "bbc", "bba", "ccc" };
                                Console.Write("Make your choice: ");
                                string selected = Utilities.StringChoice.Read(choices, 2);
                                Console.WriteLine("You chose: " + selected);
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
            string niceProfile = profileName.Replace(' ', '-');
            Console.WriteLine("Pushing credential " + niceProfile);
            RunConfigure(creds, string.Format("set aws_access_key_id {0} --profile {1}", creds.GetCredentials().AccessKey, niceProfile));
            RunConfigure(creds, string.Format("set aws_secret_access_key {0} --profile {1}", creds.GetCredentials().SecretKey, niceProfile));
        }

        private static void SetDefaultCredential(AWSCredentials creds)
        {
            RunConfigure(creds, string.Format("set aws_access_key_id {0}", creds.GetCredentials().AccessKey));
            RunConfigure(creds, string.Format("set aws_secret_access_key {0}", creds.GetCredentials().SecretKey));
        }


        /// <summary>
        /// Note: faul
        /// </summary>
        /// <param name="creds"></param>
        /// <param name="operation"></param>
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
            if (p.ExitCode != 0)
                Console.WriteLine(string.Format("Command failed.  Exit code was {0}", p.ExitCode));
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
                        Console.WriteLine(string.Format("{0} {1}", profileName, creds.AccessKey));
                }
            }
            return sortedNames;
        }

        private static string ReadRegion()
        {
            Console.Write("Region (use arrows): ");

            List<string> regions = new List<string>();
            foreach (var s in RegionEndpoint.EnumerableAllRegions) regions.Add(s.SystemName);

            return Utilities.StringChoice.Read(regions, 0);
        }

        private static string ReadLine(string prompt)
        {
            Console.Write(prompt);
            return Console.ReadLine();
        }

        public static string VersionInfo
        {
            get
            {
                //add version/build date
                System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
                Version v = asm.GetName().Version;
                DateTime bdt = new DateTime(2000, 1, 1);
                bdt = bdt.AddDays(Convert.ToInt32(v.Build));
                bdt = bdt.AddSeconds(Convert.ToInt32(v.Revision) * 2);
                return string.Format("{2} Version {0}, built {1}", v, bdt.ToString("MM/dd/yy"), asm.GetName().Name);  // h:mm tt
            }
        }
    }
}