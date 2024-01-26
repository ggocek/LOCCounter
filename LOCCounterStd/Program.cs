/* Copyright (c) 2011, Gary Gocek. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace LOCCounterStd
{
	// No GUI - console app
	class Program
	{
		static void Main(string[] args)
		{
			// The counter instance.
			Sloc.Sloc counter = null;

			string[] adjustedArgs = new string[args.Length];
			string myRoot = "Unspecified";
			string myType = "TXT";
			string myExts = ((Sloc.Sloc)(new Sloc.Sloc())).FileExtensions;

			// Convert leading hyphens to slashes.
			int adjustedIndex = 0;
			foreach (string arg in args)
			{
				if (arg.StartsWith("-"))
					adjustedArgs[adjustedIndex] = "/" + arg.Substring(1);
				else
					adjustedArgs[adjustedIndex] = arg;
				adjustedIndex++;
			}

			foreach (string arg in adjustedArgs)
			{
				// If any arg asks for help, only show the help and then abort.
				if ((arg == "/?") || (arg == "/h") || (arg == "/H"))
				{
					CommandLineHelp clh = new CommandLineHelp();
					Console.WriteLine(clh.FriendlyHelp);
					return;
				}
				// If any arg asks for licensing, only show the license and then abort.
				if ((arg == "/l") || (arg == "/L"))
				{
					CommandLineLicense cll = new CommandLineLicense();
					Console.WriteLine(cll.FriendlyLicense);
					return;
				}
				else if (arg.StartsWith("/r") || arg.StartsWith("/R"))
				{
					if (arg.Length > 2)
					{
						myRoot = arg.Substring(2);
					}
				}
				else if (arg.StartsWith("/e") || arg.StartsWith("/E"))
				{
					if (arg.Length > 2)
					{
						myExts = arg.Substring(2);
					}
				}
				else if (arg.StartsWith("/t") || arg.StartsWith("/T"))
				{
					if (arg.Length > 2)
					{
						myType = arg.Substring(2);
					}
				}
			}

			// TODO - use reflection to find all classes inherited from IOutputType.
			// Use the class whose shortTypeName matches myType. That way, a new class could
			// be added and this code here would not need to change.

			// Get the output class according to the requested type.
			IOutputType ot = null;
			if ((myType == "TXT") || (myType == "txt"))
			{
				ot = new OutputTypeTxt();
			}
			else if ((myType == "CSV") || (myType == "csv"))
			{
				ot = new OutputTypeCsv();
			}
			else if ((myType == "RSS") || (myType == "rss"))
			{
				ot = new OutputTypeRss();
			}

			if (ot == null)
				return;

			try
			{
				Console.BackgroundColor = ConsoleColor.Blue;

				counter = new Sloc.Sloc();
				counter.RootFolder = myRoot;
				counter.FileExtensions = myExts;

				// Write the text
				ot.Loc = (counter.FolderSloc(null, null)).ToArray();
				foreach (string aLine in ot.Dump())
				{
					Console.Write(aLine);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
			finally
			{
				Console.ResetColor();
			}
		}
	}

	/// <summary>
	/// A class for providing help at the console.
	/// </summary>
	public class CommandLineHelp
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public CommandLineHelp()
		{
		}

		/// <summary>
		/// Display help info when the app was started with the command line.
		/// </summary>
		public string FriendlyHelp
		{
			get
			{
				string helpText = string.Empty;
				VersionInfo vi = new VersionInfo();

				helpText += Environment.NewLine + vi.FriendlyVersionInfo + Environment.NewLine;
				helpText += "LOCCounterStd.exe, Sloc.dll and LOCCounterStd.exe.config should be in the same" + Environment.NewLine;
				helpText += "folder. LOCCounterStd.exe.config may be modified to handle more file types." + Environment.NewLine + Environment.NewLine;
				helpText += "LOCCounterStd.exe /h" + Environment.NewLine;
				helpText += "   Display help information and ignore all other options." + Environment.NewLine;
				helpText += "LOCCounterStd.exe /l" + Environment.NewLine;
				helpText += "   Display licensing information and ignore all other options." + Environment.NewLine + Environment.NewLine;
				helpText += "LOCCounterStd.exe /r{rootFolder} /t{TXT|RSS|CSV} /e{ext1[|ext2[|extN...]]]}" + Environment.NewLine;
				helpText += "   /r names the top level folder containing files to be counted." + Environment.NewLine;
				helpText += "   /t describes the output file type. Text (TXT), RSS 2.0 (RSS)," + Environment.NewLine;
				helpText += "      or comma separated values (CSV). Default is TXT." + Environment.NewLine;
				helpText += "   /e lists bar-delimited extensions for the files to be counted." + Environment.NewLine;
				helpText += "      No blanks. For example, \"/ecs|aspx|htm\"" + Environment.NewLine;
				helpText += "      If or-bars are used, double-quotes are required." + Environment.NewLine;
				helpText += "      Default: Many extensions, the ones listed in the original config file." + Environment.NewLine;
				helpText += "   Output is written to stdout. Redirect as usual, e.g.," + Environment.NewLine;
				helpText += "      LocCounterStd.exe /rc:\\ /tTXT /ecs > foo.txt" + Environment.NewLine + Environment.NewLine;
				helpText += "Put no spaces after an argument. For example," + Environment.NewLine;
				helpText += "   LOCCounterStd.exe /rc:\\ /tRSS \"/ecs|aspx|htm|html\" > bar.xml" + Environment.NewLine + Environment.NewLine;
				helpText += "Processing can take a while for large source trees. Be patient." + Environment.NewLine + Environment.NewLine;
				helpText += "Files with extensions longer than 3 characters have a short 8.3 name" + Environment.NewLine;
				helpText += "with an extension that has three characters. For example, when processing" + Environment.NewLine;
				helpText += "HTM files, HTML files may also be processed." + Environment.NewLine;

				return helpText;
			}
		}
	}

	/// <summary>
	/// A class for providing licensing at the console.
	/// </summary>
	public class CommandLineLicense
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public CommandLineLicense()
		{
		}

		/// <summary>
		/// Display license info when the app was started with the command line.
		/// </summary>
		public string FriendlyLicense
		{
			get
			{
				string licenseText = string.Empty;

				licenseText += "MIT License" + Environment.NewLine +
                "LOC Counter, copyright (c) 2024, Gary Gocek" + Environment.NewLine +
				"All rights reserved." + Environment.NewLine +
                "Permission is hereby granted, free of charge, to any person obtaining a copy" + Environment.NewLine +
                "of this software and associated documentation files(the \"Software\"), to deal" + Environment.NewLine +
                "in the Software without restriction, including without limitation the rights" + Environment.NewLine +
                "copies of the Software, and to permit persons to whom the Software is" + Environment.NewLine +
                "furnished to do so, subject to the following conditions:" + Environment.NewLine +
                "The above copyright notice and this permission notice shall be included in all" + Environment.NewLine +
                "copies or substantial portions of the Software." + Environment.NewLine +
                "THE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR" + Environment.NewLine +
                "FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE" + Environment.NewLine +
                "AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER" + Environment.NewLine +
                "LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM," + Environment.NewLine +
                "OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE" + Environment.NewLine +
                "SOFTWARE.";

                return licenseText;
			}
		}
	}

	/// <summary>
	/// A class for providing version info.
	/// </summary>
	public class VersionInfo
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public VersionInfo()
		{
		}

		/// <summary>
		/// Geta friendly string indicating the version.
		/// </summary>
		public string FriendlyVersionInfo
		{
			get
			{
				string ret = "LOC Counter";

				try
				{
					Assembly assm = Assembly.GetExecutingAssembly();
					AssemblyName an = AssemblyName.GetAssemblyName(assm.Location);
					object[] o = assm.GetCustomAttributes(typeof(AssemblyTitleAttribute), true);
					string myTitle = ((System.Reflection.AssemblyTitleAttribute)o[0]).Title;

					// Major
					ret =
						string.Format("{0} v{1}.{2}.{3}.{4}",
						myTitle,
						an.Version.Major.ToString(),
						an.Version.Minor.ToString(),
						an.Version.Build.ToString(),
						an.Version.Revision.ToString());
				}
				catch
				{
					//
				}

				return ret;
			}
		}
	}
}
