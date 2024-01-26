/* Copyright (c) 2011, Gary Gocek. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Xml;
using System.Configuration;

namespace Sloc
{

	/// <summary>
	/// A class for providing the delimiters of comment lines for different types of files.
	/// TODO: make it possible to supply additional extensions and comment delimiters
	/// without having to change the code. Could be args of the constructor, but probably should
	/// be some sort of input file, which probably should be an XML file.
	/// </summary>
	/// <exception cref="System.ArgumentException">If the XML file is badly formed.</exception>
	public class CommentDelimiters
	{
		private IDictionary<string, CommentDescriptors> commentDescs = new Dictionary<string, CommentDescriptors>();
		//private List<CommentDescriptors> commentDescs = new List<CommentDescriptors>();
		private ExtensionDelimiters[] defaultDelims = null;

		/// <summary>
		/// Describes the state of a comment for a line in a file.
		/// </summary>
		public enum CommentState
		{
			/// <summary>
			/// The state was not previously in a multi-line comment,
			/// and this line is not a comment. The line is either blank
			/// or a source line of code.
			/// </summary>
			None,
			/// <summary>
			/// This line is a single comment line.
			/// </summary>
			StartedAndFinished,
			/// <summary>
			/// This line starts and does not end a multi-line comment block.
			/// </summary>
			Started,
			/// <summary>
			/// The previous state was Started or Continuing and this line is within
			/// a multi-line comment and does not end the multi-line comment.
			/// </summary>
			Continuing,
			/// <summary>
			/// The previous state was Started or Continuing, and this line is a comment line,
			/// and this line ends a multi-line comment.
			/// </summary>
			Finished
		}

		/// <summary>
		/// Describes the state of a comment line and the multi-line closing delimiter, if any.
		/// </summary>
		public struct CommentStateInfo
		{
			/// <summary>
			/// The comment state of a line in a file.
			/// </summary>
			public CommentState state;
			/// <summary>
			/// If the comment state is Started or Continuing, this is the delimiter to look for
			/// to close the multi-line comment. Otherwise undefined.
			/// </summary>
			public string delimEnd;
		}

		/// <summary>
		/// For any extension, there will be a list of these to describe ways
		/// in which comments can be delimited.
		/// </summary>
		public struct ExtensionDelimiters
		{
			/// <summary>
			/// True if the delimiters mark the start and end of a comment block.
			/// False if only delimStart is used to mark a short comment.
			/// </summary>
			public bool isWrapping;
			/// <summary>
			/// The start of the comment
			/// </summary>
			public string delimStart;
			/// <summary>
			/// The end of a comment block
			/// </summary>
			public string delimEnd;

			/// <summary>
			/// Construct an instance with the provided values
			/// </summary>
			/// <param name="isWrap">True if the delimiters start and end a comment block,
			/// false if a comment is prefixed by dlimStart</param>
			/// <param name="dlimStart">The start of a comment</param>
			/// <param name="dlimEnd">The end of a comment block, ignored when isWrap is false.</param>
			public ExtensionDelimiters(bool isWrap, string dlimStart, string dlimEnd)
			{
				isWrapping = isWrap;
				delimStart = dlimStart;
				delimEnd = dlimEnd;
			}
		}

		/// <summary>
		/// For all known files types, there is a descriptor.
		/// </summary>
		public struct CommentDescriptors
		{
			/// <summary>
			/// The extension for the file whose comment delimiters are described
			/// </summary>
			public string fileExtension;
			/// <summary>
			/// The list of delimiters
			/// </summary>
			public ExtensionDelimiters[] extDelims;

			/// <summary>
			/// Construct an instance with the provided values
			/// </summary>
			/// <param name="fileExt">A file extension for which the comment delimiters are valid</param>
			/// <param name="dlimsForExt">A list of delimiter objects</param>
			public CommentDescriptors(string fileExt, ExtensionDelimiters[] dlimsForExt)
			{
				fileExtension = fileExt;
				extDelims = dlimsForExt;
			}
		}

		/// <summary>
		/// Constructor
		/// </summary>
		public CommentDelimiters()
		{
			commentDescs = new Dictionary<string, CommentDescriptors>();

			try
			{
				// Read the custom config section.
				LOCCConfigurationSection myLOCCCSection = ConfigurationManager.GetSection("LOCCounterExtensionsSection") as LOCCConfigurationSection;
				if (myLOCCCSection.Extensions.Count > 0)
				{
					for (int iii = 0; iii < myLOCCCSection.Extensions.Count; iii++)
					{
						string myRawExt = string.Empty;
						string myRawDelims = string.Empty;
						try
						{
							// Init the list of delimiters for this extension
							List<ExtensionDelimiters> myDelims = new List<ExtensionDelimiters>();

							myRawExt = myLOCCCSection.Extensions[iii].Name;
							myRawDelims = myLOCCCSection.Extensions[iii].Value;
							string[] myRawDelimsA = myRawDelims.Split(new char[] { ',' });

							// If the list of delimiters is not empty, parse the list.
							if (myRawDelimsA.Length > 0)
							{
								foreach (string myRawDelim in myRawDelimsA)
								{
									// myRawDelim will either be a single string, or two strings separated by a space.
									// Trim and remove multiple spaces.
									string normalizeMyRawDelim = myRawDelim.Trim();
									while (normalizeMyRawDelim.Contains("  "))
										normalizeMyRawDelim = normalizeMyRawDelim.Replace("  ", " ");
									if (normalizeMyRawDelim.Contains(" "))
									{
										// The string contains starting and ending delimiters for multi-line comments.
										string[] normalizeMyRawDelimA = normalizeMyRawDelim.Split(new char[] { ' ' });
										if (normalizeMyRawDelimA.Length == 2)
										{
											ExtensionDelimiters wed = new ExtensionDelimiters(true, normalizeMyRawDelimA[0], normalizeMyRawDelimA[1]);
											myDelims.Add(wed);
										}
									}
									else
									{
										// The string contains the delimiter for a single line comment.
										ExtensionDelimiters ped = new ExtensionDelimiters(false, normalizeMyRawDelim, null);
										myDelims.Add(ped);
									}
								}
								CommentDescriptors cd = new CommentDescriptors("." + myRawExt, myDelims.ToArray());
								commentDescs.Add(myRawExt, cd);
							}
						}
						catch (Exception ex)
						{
							throw new System.ArgumentException(
								"Error in CommentDelimiters.xml near extension " + myRawExt + ": " + ex.Message);
						}
					}
				}
				else
				{
					// No extension keys in app.config, provide the C extension, others will use the default.
					ExtensionDelimiters ed1 = new ExtensionDelimiters(false, "//", null);
					ExtensionDelimiters ed2 = new ExtensionDelimiters(true, "/*", "*/");
					CommentDescriptors cd1 = new CommentDescriptors(".c", new ExtensionDelimiters[] { ed1, ed2 });
					commentDescs.Add("c", cd1);
				}
			}
			catch (Exception ex1)
			{
				throw new System.ArgumentException(
					"Error reading app.config: " + ex1.Message);
			}

			// Use C style for when the user checks an unknown file type.
			ExtensionDelimiters ed3 = new ExtensionDelimiters(false, "//", null);
			ExtensionDelimiters ed4 = new ExtensionDelimiters(true, "/*", "*/");

			defaultDelims = new ExtensionDelimiters[] { ed3, ed4 };
		}

		/// <summary>
		/// Determine the ways in which comment lines are delimited, based on the file name.
		/// </summary>
		/// <param name="fileName">The file name.</param>
		/// <returns>A list of delimiters. If a list value has false for isWrapping, then any code line whose
		/// first non-white space is delimStart is a comment line. If isWrapping is true, then
		/// delimStart starts a comment section and delimEnd ends the comment section,
		/// allowing for multi-line comments.</returns>
		public ExtensionDelimiters[] CommentDelimitersForFile(string fileName)
		{
			string lFile = fileName.ToLower();
			ExtensionDelimiters[] ret = defaultDelims;
			CommentDescriptors cd;

			int lDot = lFile.LastIndexOf('.');
			string lExt = string.Empty;
			if ((lDot >= 0) && (lFile.Length > (lDot + 1)))
			{
				lExt = lFile.Substring(lDot + 1);
				if (commentDescs.TryGetValue(lExt, out cd))
					ret = cd.extDelims;
			}
			return ret;
		}
	}
}
