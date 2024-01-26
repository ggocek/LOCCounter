/* Copyright (c) 2011, Gary Gocek. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Resources;

namespace Sloc
{
	/// <summary>
	/// A class library for counting lines of code in selected files under a root folder.
	///	Usage: instantiate the class with the constructor, then set RootFolder and FileExtensions.
	///	Invoke FolderSloc to count the lines in the files inside RootFolder. FolderSloc updates
	///	NumFiles and CurFile while running, so if invoked asynchronously, these can be queried for status.
	/// </summary>
	public class Sloc
	{
		// The CommentDelimiters constructor does a lot of work, so cache it.
		private CommentDelimiters cDelims = null;

		/// <summary>
		/// Constructor
		/// </summary>
		public Sloc()
		{
			cDelims = new CommentDelimiters();
		}

		// Private cache for RootFolder
		private string rootFolder = "C:\\";
		/// <summary>
		/// The folder under which to find files whose lines should be counted.
		/// The default is c:, and the default file extensions list is a long list, so
		/// the default call to FolderSloc could take a while or throw due to locked
		/// Windows folders.
		/// </summary>
		public string RootFolder
		{
			get { return rootFolder; }
			set { rootFolder = value; }
		}

		// Private cache for FileExtensions
		// Make sure to prefix each element with *. so that the default list is normalized as if calling FileExtensions.
		private string fileExtensions =
			"*.cs|*.cpp|*.c|*.h|*.asp|*.aspx|*.ascx|*.ashx|*.asmx|*.asax|*.htm|*.html|*.xml|*.xsl" +
			"|*.ism|*.resx|*.config|*.js|*.sql|*.vb|*.vbs|*.bat|*.cmd|*.java|*.php|*.py" +
			"|*.master|*.proj|*.targets";
		/// <summary>
		/// Get/set the or-bar delimited extensions of files that should be counted.
		/// If an element EXT does not start with *., it will be treated as *.EXT,
		/// so the list can be provided (for example) as cs|aspx|xml . The default is a large set
		/// of common code file extensions.
		/// </summary>
		/// <exception cref="System.ArgumentException">Improperly formatted list of extensions.</exception>
		public string FileExtensions
		{
			get { return fileExtensions; }
			set
			{
				fileExtensions = value;
				List<string> extA = new List<string>();
				// Normalize - make sure the list contains *.ext for each delimited element.
				// Don't convert to lower case here.
				string[] rawExts = fileExtensions.Split(new char[] { '|' });
				foreach (string rawExt in rawExts)
				{
					if (!rawExt.StartsWith("*."))
					{
						if ((rawExt.StartsWith("*") && (rawExt.Length < 2)) ||
							(rawExt.StartsWith(".") && (rawExt.Length < 2)) ||
							(rawExt.Length < 1) ||
							(rawExt.Length >= 40))
							throw new System.ArgumentException("Extension list should contain or-bar delimited extenstions: " + fileExtensions);

						if (rawExt.StartsWith("."))
							extA.Add("*" + rawExt);
						else if (rawExt.StartsWith("*"))
							extA.Add("*." + rawExt.Substring(1));
						else
							extA.Add("*." + rawExt);
					}
					else
					{
						extA.Add(rawExt);
					}
				}

				// Made it this far, reset the list.
				fileExtensions = string.Empty;
				int extCount = 0;
				foreach (string rawExt in extA)
				{
					if (extCount < (extA.Count - 1))
					{
						fileExtensions += (rawExt + "|");
					}
					else
					{
						fileExtensions += rawExt;
					}
					extCount++;
				}
			}
		}

		// Private cache for RawFileNames
		private List<string> rawFileNames = new List<string>();
		/// <summary>
		/// The list of files in the folder and its subfolders
		/// </summary>
		public List<string> RawFileNames
		{
			get { return rawFileNames; }
		}

		// Private cache for NumFiles
		private int numFiles = 0;
		/// <summary>
		/// Get the number of files to be counted.
		/// </summary>
		public int NumFiles
		{
			get { return rawFileNames.Count; }
		}

		// Private cache for CurFile
		private int curFile = 0;
		/// <summary>
		/// Get the number of the file currently being counted, somewhere between zero and NumFiles.
		/// </summary>
		public int CurFile
		{
			get { return curFile; }
		}

		// Private cache for CurExtension
		private string curExtension = string.Empty;
		/// <summary>
		/// Get the extension currently being processed.
		/// </summary>
		public string CurExtension
		{
			get { return curExtension; }
		}

		/// <summary>
		/// Count the LOC in the files under RootFolder.
		/// </summary>
		/// <param name="worker">A background worker (or null), which can request cancelation.</param>
		/// <param name="e">The background worker event args (or null).</param>
		/// <returns>A list of file information objects. The first element in the array will be the total for the
		/// rest of the elements. In case of a cancelation, zero elements will be returned. A folder with no
		/// countable files will return one element for the totals showing zero lines of code.</returns>
		/// <exception cref="System.ArgumentException">Problems with the provided folder.</exception>
		public List<SlocFileReport> FolderSloc(
			System.ComponentModel.BackgroundWorker worker,
			System.ComponentModel.DoWorkEventArgs e)
		{
			List<SlocFileReport> ret = new List<SlocFileReport>();

			if (rootFolder == null)
				rootFolder = string.Empty;

			// Barf on the caller if a bad folder
			if (!Directory.Exists(rootFolder))
			{
				throw new System.ArgumentException("Directory " + rootFolder + " does not exist.");
			}

			// Collect the files. This is slow.
			try
			{
				rawFileNames = CollectFiles(worker);
			}
			catch (Exception ex)
			{
				throw ex;
			}

			if ((worker != null) && worker.CancellationPending)
			{
				rawFileNames = new List<string>();
				numFiles = 0;
				curFile = 0;
				if (e != null)
					e.Cancel = true;
				return new List<SlocFileReport>();
			}

			curFile = 0;
			numFiles = rawFileNames.Count;

			SlocFileReport totalSfr = new SlocFileReport();
			totalSfr.LinesBlank = 0;
			totalSfr.LinesComment = 0;
			totalSfr.LinesTotal = 0;
			ret.Add(totalSfr);

			foreach (string fName in rawFileNames)
			{
				// Count the lines in the next file.
				SlocFileReport sfr = FileSloc(fName);

				// Increment the totals.
				((SlocFileReport)ret[0]).LinesBlank += sfr.LinesBlank;
				((SlocFileReport)ret[0]).LinesComment += sfr.LinesComment;
				((SlocFileReport)ret[0]).LinesTotal += sfr.LinesTotal;

				// Push the file info into the array.
				ret.Add(sfr);

				if ((worker != null) && worker.CancellationPending)
				{
					rawFileNames = new List<string>();
					numFiles = 0;
					curFile = 0;
					if (e != null)
						e.Cancel = true;
					return new List<SlocFileReport>();
				}
			}

			string total = "TOTAL.";
			try
			{
				ResourceManager locRM =
					new ResourceManager("Sloc.Resources.Strings", typeof(Sloc).Assembly);
				total = locRM.GetString("resTotal");
			}
			catch { }
			((SlocFileReport)ret[0]).FileName = total + " - " + numFiles.ToString();
			((SlocFileReport)ret[0]).FullPath = total;
			((SlocFileReport)ret[0]).IsTotal = true;

			return ret;
		}

		/// <summary>
		/// Collect the list of files according to rootFolder and fileExtensions.
		/// </summary>
		/// <param name="worker">The background worker, which can request cancelation.</param>
		/// <returns>An array of string file names (full paths).</returns>
		private List<string> CollectFiles(
			System.ComponentModel.BackgroundWorker worker)
		{
			List<string> ret = new List<string>();

			try
			{
				string[] extA = fileExtensions.Split(new char[] { '|' });
				foreach (string fType in extA)
				{
					// TODO: Start a thread or process to run GetFiles. It can be slow for large folders,
					// and the BackgroundWorker class has no way to get inside it to cancel it.
					// However, in practice, by making a separate call for each extension,
					// the user's wait after requesting cancelation is usually not too bad.
					curExtension = fType;
					string[] typeFiles = Directory.GetFiles(rootFolder, fType, SearchOption.AllDirectories);
					foreach (string fName in typeFiles)
					{
						ret.Add(fName);

						if ((worker != null) && worker.CancellationPending)
						{
							break;
						}
					}

					if ((worker != null) && worker.CancellationPending)
					{
						break;
					}
				}
			}
			catch (Exception ex)
			{
				throw new System.ArgumentException("Directory " + rootFolder + ": " + ex.Message);
			}

			return ret;
		}

		/// <summary>
		/// Count the lines of code for a file.
		/// </summary>
		/// <param name="fName">Full path to a file</param>
		/// <returns>SLOC info</returns>
		private SlocFileReport FileSloc(string fName)
		{
			SlocFileReport ret = new SlocFileReport();
			FileInfo fi = new FileInfo(fName);
			ret.FullPath = fName;
			ret.FileName = fi.Name;
			ret.FolderName = fi.DirectoryName;

			CommentDelimiters.CommentStateInfo prevState = new CommentDelimiters.CommentStateInfo();
			prevState.state = CommentDelimiters.CommentState.None;
			CommentDelimiters.CommentStateInfo thisState = new CommentDelimiters.CommentStateInfo();
			thisState.state = CommentDelimiters.CommentState.None;

			CommentDelimiters.ExtensionDelimiters[] commentDelims = cDelims.CommentDelimitersForFile(fName);

			// Open the next file
			using (StreamReader sr = File.OpenText(fName))
			{
				if (sr != null)
				{
					// File is real, increment the file counter. When this class is used asynchronously,
					// the caller can see how far along the file loop is.
					curFile++;

					// loop until end of file
					string line = string.Empty;
					while ((line = sr.ReadLine()) != null)
					{
						line = line.Trim();

						// Check the comment delimiters.
						thisState = CommentState(line, commentDelims, prevState);

						if (thisState.state == CommentDelimiters.CommentState.None)
						{
							// Not a comment, but check for blank.
							if (line.Length == 0)
							{
								ret.LinesBlank++;
							}
						}
						else
						{
							// A comment
							ret.LinesComment++;
						}

						ret.LinesTotal++;

						prevState = thisState;
					}
				}
			}

			return ret;
		}

		/// <summary>
		/// Examine a line to see how it conforms to its file's comment delimiters.
		/// </summary>
		/// <param name="fileLine">The line of the code file currently being examined.</param>
		/// <param name="commentSpecs">A list of descriptors that tell how a comment can begin
		/// and end for the current file.</param>
		/// <param name="prevCSI">The state of the previous line.</param>
		/// <returns>A new comment state.</returns>
		private CommentDelimiters.CommentStateInfo CommentState(
			string fileLine,
			CommentDelimiters.ExtensionDelimiters[] commentSpecs,
			CommentDelimiters.CommentStateInfo prevCSI)
		{
			CommentDelimiters.CommentStateInfo ret = new CommentDelimiters.CommentStateInfo();
			ret.state = CommentDelimiters.CommentState.None;
			ret.delimEnd = string.Empty;

			if ((prevCSI.state == CommentDelimiters.CommentState.None) ||
				(prevCSI.state == CommentDelimiters.CommentState.StartedAndFinished) ||
				(prevCSI.state == CommentDelimiters.CommentState.Finished))
			{
				// Not in the middle of a comment block. Look for the start of a comment.
				// If length is zero, fall through to return the None state.
				if (fileLine.Length > 0)
				{
					// Look for each possible delimStart at the beginning of the line.
					// If there are no matches, fall through to return the None state.
					// If the line is blank, fall through to return the None state.
					foreach (CommentDelimiters.ExtensionDelimiters commentSpec in commentSpecs)
					{
						if (!commentSpec.isWrapping)
						{
							// Look for a single-line comment, i.e., a line starting with delimStart.
							if (fileLine.StartsWith(commentSpec.delimStart))
							{
								ret.state = CommentDelimiters.CommentState.StartedAndFinished;
								ret.delimEnd = string.Empty;
								break;
							}
						}
						else
						{
							// Look for the start of a multi-line comment, starting with delimStart.
							if (fileLine.StartsWith(commentSpec.delimStart))
							{
								// The start of a possibly multi-line comment blank.
								if (fileLine.Contains(commentSpec.delimEnd))
								{
									// The comment block started and ended.
									ret.state = CommentDelimiters.CommentState.StartedAndFinished;
									ret.delimEnd = string.Empty;
								}
								else
								{
									// The comment block started and has not ended.
									ret.state = CommentDelimiters.CommentState.Started;
									ret.delimEnd = commentSpec.delimEnd;
									break;
								}
							}
						}
					}
				}
			}
			else if ((prevCSI.state == CommentDelimiters.CommentState.Started) ||
				(prevCSI.state == CommentDelimiters.CommentState.Continuing))
			{
				// If the middle of a comment block.
				// Note that a blank line in the middle of a comment block will be marked
				// as a continuation of the comment.
				if (fileLine.Contains(prevCSI.delimEnd))
				{
					// Found the end.
					ret.state = CommentDelimiters.CommentState.Finished;
					ret.delimEnd = string.Empty;
				}
				else
				{
					// The block has not ended, still in the middle of it.
					ret.state = CommentDelimiters.CommentState.Continuing;
					ret.delimEnd = prevCSI.delimEnd;
				}
			}

			return ret;
		}
	}
}
