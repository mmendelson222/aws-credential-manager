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
        static string defaultCredName;
        const string MY_URL = "http://bit.ly/2kJ2J42";
        enum eListingType { standard, concise, showall };

        static void Main(string[] args)
        {
            try
            {
                char input;
                bool showPrompt = true;
                List<string> names = null;
                bool showCredentialSecret = false;
                eListingType listingType = eListingType.standard;

                do
                {
                    if (showPrompt)
                    {
                        Console.WriteLine();
                        names = ListCredentials(listingType);

                        //Console.WriteLine();
                        //Console.WriteLine("Stored credentials: Add, Remove, Update, rEname");
                        //Console.WriteLine("Named profiles:     Set Default, Push");
                        //Console.WriteLine("Whois");
                        //Console.WriteLine("X: Exit\n");

                        Console.WriteLine("A: Add    stored credential");
                        Console.WriteLine("R: Remove stored credential");
                        Console.WriteLine("U: Update stored credential");
                        Console.WriteLine("S: Set    default credential");
                        Console.WriteLine("P: Push   to a Named Profile");
                        Console.WriteLine("W: Whois  the associated IAM user");
                        Console.WriteLine("X: Exit\n");
                    }

                    var keyInfo = Console.ReadKey(true);
                    string sKey = keyInfo.Key.ToString();
                    if (sKey.Length > 1)
                        input = char.MinValue;  //omit non-characters (e.g. F1, Escape)
                    else
                        input = sKey.ToLower()[0];

                    showPrompt = true;

                    switch (input)
                    {
                        case 'a':
                            {
                                var profileName = ReadLine("New profile name: "); if (profileName.Length == 0) break;
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
                        case 'e':
                            {
                                Console.Write("Rename credential (use arrows or type): ");
                                string selected = Utilities.StringChoice.Read(names);
                                if (string.IsNullOrEmpty(selected)) break;

                                var newName = ReadLine("Rename to: ");
                                if (string.IsNullOrEmpty(newName)) break;

                                //create new one credential, delete old one.
                                var olcCredential = ProfileManager.GetAWSCredentials(selected).GetCredentials();
                                ProfileManager.RegisterProfile(newName, olcCredential.AccessKey, olcCredential.SecretKey);
                                ProfileManager.UnregisterProfile(selected);
                                break;
                            }
                        case 'u':
                            {
                                Console.Write("Update credential (use arrows or type): ");
                                string selected = Utilities.StringChoice.Read(names); if (string.IsNullOrEmpty(selected)) break;

                                //Determine if the cred being changed  is the default
                                var isDefault = ProfileManager.GetAWSCredentials(selected).GetCredentials().AccessKey == GetDefaultCredential();

                                var accessKey = ReadLine("Access key: "); if (accessKey.Length == 0) break;
                                var secretKey = ReadLine("Secret key: "); if (secretKey.Length == 0) break;
                                ProfileManager.UnregisterProfile(selected);
                                ProfileManager.RegisterProfile(selected, accessKey, secretKey);

                                //if it was a default credential, automatically reset it. 
                                if (isDefault)
                                {
                                    Console.WriteLine("\nResetting default credential.");
                                    SetDefaultCredential(ProfileManager.GetAWSCredentials(selected));
                                }

                                break;
                            }
                        case 's':
                        case 'd':
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

                        //whois information on a credential (select)
                        //should default to current default
                        case 'w':
                            {
                                Console.Write("Choose credential (use arrows or type): ");
                                string selected = Utilities.StringChoice.Read(names, defaultCredName);
                                if (!string.IsNullOrEmpty(selected))
                                {
                                    AWSCredentials creds = ProfileManager.GetAWSCredentials(selected);
                                    ReportWhois(creds);
                                    showPrompt = false;
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

                        case 'c':
                        //concise listing


                        case 'l':
                            listingType++;
                            listingType = (eListingType)((int)listingType % Enum.GetValues(typeof(eListingType)).Length);
                            if (keyInfo.Modifiers == ConsoleModifiers.Control)
                                showCredentialSecret = !showCredentialSecret;
                            break;

                        //version info
                        case 'v':
                            Console.WriteLine(VersionInfo);
                            showPrompt = false;
                            break;

                        //test
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

        private static void ReportWhois(AWSCredentials creds)
        {
            try
            {
                Console.WriteLine(Operations.IamOperations.UserInfo(creds));
                Console.WriteLine();
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
            RunConfigure(string.Format("set aws_access_key_id {0} --profile {1}", creds.GetCredentials().AccessKey, niceProfile), true, false);
            RunConfigure(string.Format("set aws_secret_access_key {0} --profile {1}", creds.GetCredentials().SecretKey, niceProfile), true, false);
        }

        private static void SetDefaultCredential(AWSCredentials creds)
        {
            RunConfigure(string.Format("set aws_access_key_id {0}", creds.GetCredentials().AccessKey), true, false);
            RunConfigure(string.Format("set aws_secret_access_key {0}", creds.GetCredentials().SecretKey), true, false);
        }

        /// <summary>
        /// get the default credential, or empty string if none
        /// </summary>
        private static string GetDefaultCredential()
        {
            string cmd = "get aws_access_key_id";
            return RunConfigure(cmd, false, true).Trim();
        }

        /// <summary>
        /// Run an aws configure operation. 
        /// </summary>
        private static string RunConfigure(string operation, bool showStdOut, bool bailOnfail)
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
            {
                if (bailOnfail)
                    return string.Empty;
                else
                    Console.WriteLine(string.Format("AWS configure command failed.  Exit code was {0}", p.ExitCode));
            }
            if (!string.IsNullOrEmpty(strOutput) && showStdOut)
                Console.WriteLine(strOutput);
            return strOutput;
        }

        private static List<string> ListCredentials(eListingType listingType)
        {
            Console.WriteLine();
            List<string> sortedNames = ProfileManager.ListProfileNames().OrderBy(p => p).ToList();

            if (sortedNames.Count == 0)
            {
                Console.WriteLine("No credentials are currently stored.  Type A to add one.");
            }
            else
            {
                int maxLength = sortedNames.Max(n => n.Length);
                string fmtShowAll = string.Format("{{0,-{0}}} {{1}}\n{1} {{2}} {{3}}\r\n", maxLength, new string(' ', maxLength));
                string fmtStandard = string.Format("{{0,-{0}}} {{1}} {{2}}\r\n", maxLength);
                string fmtConcise = string.Format("{{0,-{0}}}", maxLength);

                int columns = Console.WindowWidth / (maxLength + 1);
                int row = 0;
                int rows = (int)Math.Ceiling((double)sortedNames.Count / (double)columns);
                StringBuilder[] sbRows = new StringBuilder[rows];

                StringBuilder sbOut = new StringBuilder();

                var currentDefaultCredential = GetDefaultCredential();

                Console.WriteLine("=== Stored Credentials ===");


                foreach (var profileName in sortedNames)
                {
                    var creds = ProfileManager.GetAWSCredentials(profileName).GetCredentials();
                    string defaultIndicator = string.Empty;
                    if (string.Compare(profileName, "default", true) == 0)
                    {
                        defaultIndicator = string.Format("Careful!\n{0} See note on defaults: {1}", new string(' ', maxLength), MY_URL);
                    }
                    else if (creds.AccessKey == currentDefaultCredential)
                    {
                        defaultCredName = profileName;
                        defaultIndicator = listingType == eListingType.concise ? "*" : "(default)";
                    }

                    switch (listingType)
                    {
                        case eListingType.concise:
                            if (row == 0) sbOut.AppendLine();
                            if (sbRows[row] == null) sbRows[row] = new StringBuilder();
                            sbRows[row].AppendFormat(fmtConcise, profileName + defaultIndicator);
                            row = ++row % rows;
                            break;

                        case eListingType.standard:
                            sbOut.AppendFormat(fmtStandard, profileName, creds.AccessKey, defaultIndicator);
                            break;
                        case eListingType.showall:
                            sbOut.AppendFormat(string.Format(fmtShowAll, profileName, creds.AccessKey, creds.SecretKey, defaultIndicator));
                            break;
                    }
                }

                switch (listingType)
                {
                    case eListingType.concise:
                        foreach (var sb in sbRows)
                            Console.WriteLine(sb);
                        Console.WriteLine();
                        break;

                    default:
                        Console.WriteLine(sbOut);
                        break;
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
            var s = Console.ReadLine();
            return s.Trim();
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
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("{2} Version {0}, built {1}\n", v, bdt.ToString("MM/dd/yy"), asm.GetName().Name);  // h:mm tt
                sb.AppendFormat("Written by Michael Mendelson.  Find the code at at {0}\n", MY_URL);
                return sb.ToString();
            }
        }
    }
}
