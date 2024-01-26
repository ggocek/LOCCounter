using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Sloc
{
	/// <summary>
	/// A class to report the results of counting the lines in a file
	/// </summary>
	public class SlocFileReport
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public SlocFileReport()
		{
		}

		/// <summary>
		/// Reconstruct an instance given a full path and a summary value that was previously
		/// produced by the Summary property. Call this constructor when a program called Sloc
		/// and dumped a summary into a text file, and now that file has been read back in and
		/// the summary needs to be parsed.
		/// </summary>
		/// <param name="fullFilePath">A full path name, parsed to get the folder and name.</param>
		/// <param name="summary">A summary as produced by the Summary property.</param>
		public SlocFileReport(string fullFilePath, string summary)
		{
			FileInfo fi = new FileInfo(fullFilePath);
			folderName = fi.DirectoryName;
			fileName = fi.Name;
			fullPath = fullFilePath;
			linesTotal = 0;
			linesBlank = 0;
			linesComment = 0;

			try
			{
				string[] summParts = summary.Split(new char[] { '|' });
				if (summParts.Length == 4)
				{
					foreach (string summPart in summParts)
					{
						string[] numParts = summPart.Split(new char[] { ':' });
						if (numParts.Length == 2)
						{
							if (numParts[0] == "Total")
								linesTotal = Convert.ToInt32(numParts[1]);
							else if (numParts[0] == "Comment")
								linesComment = Convert.ToInt32(numParts[1]);
							else if (numParts[0] == "Blank")
								linesBlank = Convert.ToInt32(numParts[1]);
						}
					}
				}
			}
			catch
			{
				// We did the best we could, but the caller probably didn't really provide
				// a summary as originally produced by Summary.
			}
		}

		/// <summary>
		/// Construct an instance by reading a record from the provided stream
		/// and splitting the string according to the provided delimiter.
		/// The record must be of the form provided by SummaryDelimited.
		/// Before calling this constructor, call Peek() on the stream, and if
		/// that returns less than zero, then the stream has no more data.
		/// </summary>
		/// <param name="sr">A stream from which to read a record.</param>
		/// <param name="delimiter">Separate character for the fields, such as a comma.</param>
		public SlocFileReport(StreamReader sr, char delimiter)
		{
			string nextLine = sr.ReadLine();
			if (nextLine == null)
				return;

			string[] lineParts = nextLine.Split(new char[] { delimiter });
			if (lineParts.Length != 5)
				return;

			try
			{
				FileInfo fi = new FileInfo(lineParts[4]);
				folderName = fi.DirectoryName;
				fileName = fi.Name;
				fullPath = lineParts[4];
				linesTotal = Convert.ToInt32(lineParts[0]);
				linesComment = Convert.ToInt32(lineParts[1]);
				linesBlank = Convert.ToInt32(lineParts[2]);
			}
			catch
			{
				// We did the best we could, but the caller probably didn't really provide
				// a summary as originally produced by SummaryDelimited.
			}
		}

		// Private cache for FolderName
		private string folderName = string.Empty;
		/// <summary>
		/// The folder under which the file whose lines are counted is located.
		/// </summary>
		public string FolderName
		{
			get { return folderName; }
			set { folderName = value; }
		}

		// Private cache for FileName
		private string fileName = string.Empty;
		/// <summary>
		/// The file name.
		/// </summary>
		public string FileName
		{
			get { return fileName; }
			set { fileName = value; }
		}

		// Private cache for FullPath
		private string fullPath = string.Empty;
		/// <summary>
		/// FolderName plus backslash plus FileName
		/// </summary>
		public string FullPath
		{
			get { return fullPath; }
			set { fullPath = value; }
		}

		// Private cache for LinesTotal
		private int linesTotal = 0;
		/// <summary>
		/// Total number of lines in the file. Subtract LinesBlank and LinesComment to get
		/// the number of source lines of code.
		/// </summary>
		public int LinesTotal
		{
			get { return linesTotal; }
			set { linesTotal = value; }
		}

		// Private cache for LinesBlank
		private int linesBlank = 0;
		/// <summary>
		/// Number of blank lines in the file.
		/// </summary>
		public int LinesBlank
		{
			get { return linesBlank; }
			set { linesBlank = value; }
		}

		// Private cache for LinesComment
		private int linesComment = 0;
		/// <summary>
		/// Number of comment lines in the file.
		/// </summary>
		public int LinesComment
		{
			get { return linesComment; }
			set { linesComment = value; }
		}

		// Private cache for IsTotal
		private bool isTotal = false;
		/// <summary>
		/// True if this instance represents a total of other instances.
		/// </summary>
		public bool IsTotal
		{
			get { return isTotal; }
			set { isTotal = value; }
		}

		/// <summary>
		/// Get a single string that summarizes the results for the file.
		/// Generally not useful until the file has been processed and all the
		/// counts are real.
		/// </summary>
		public string Summary
		{
			get
			{
				// If you change this, be sure to change the constructor that takes
				// a summary string as input.
				return
					"Total:" + linesTotal.ToString() + "|" +
					"Comment:" + linesComment.ToString() + "|" +
					"Blank:" + linesBlank.ToString() + "|" +
					"SLOC:" + (linesTotal - linesBlank - linesComment).ToString();
			}
		}

		/// <summary>
		/// Prepare a single string that summarizes the results. Each field is delimited
		/// by the provided string, so if you want to send the output to a comma separated
		/// values (CSV) file, provide a comma as the delimiter.
		/// Generally not useful until the code file has been processed and all the counts are real.
		/// </summary>
		/// <param name="delimiter">The char to place between values, such as a comma.</param>
		/// <returns>total, comments, blanks, sloc, fullPath (delimited).</returns>
		public string SummaryDelimited(char delimiter)
		{
			// If you change this, be sure to change the constructor that takes
			// a CSV summary string as input.
			return
				linesTotal.ToString() + delimiter +
				linesComment.ToString() + delimiter +
				linesBlank.ToString() + delimiter +
				(linesTotal - linesBlank - linesComment).ToString() + delimiter +
				fullPath;
		}

		/// <summary>
		/// Prepare a string of column names, delimited by the provided string,
		/// corresponding to the columns returned by SummaryDelimited().
		/// </summary>
		/// <param name="delimiter">The char to place between values, such as a comma.</param>
		/// <returns>Total,Comment,Blank,SLOC,Name (using the provided delimiter char).</returns>
		public static string SummaryDelimitedHeader(char delimiter)
		{
			// Be sure this corresponds to SummaryDelimited().
			return "Total" + delimiter + "Comment" + delimiter + "Blank" + delimiter + "SLOC" + delimiter + "Name";
		}
	}
}
