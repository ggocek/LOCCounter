/* Copyright (c) 2011, Gary Gocek. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sloc;

namespace LOCCounterStd
{
	/// <summary>
	/// A class for producing save-able comma separated values output from
	/// data procduced by the Sloc class.
	/// </summary>
	public class OutputTypeCsv : IOutputType
	{
		// Private cache for ShortTypeName
		private string shortTypeName = "CSV";
		/// <summary>
		/// Get/set the name for the type of output to be produced.
		/// </summary>
		public string ShortTypeName
		{
			get { return shortTypeName; }
			set { shortTypeName = value; }
		}

		// Private cache for Loc
		private SlocFileReport[] loc = null;
		/// <summary>
		/// Get/set the information (about the lines of code) to be processed.
		/// </summary>
		public SlocFileReport[] Loc
		{
			get { return loc; }
			set { loc = value; }
		}

		// Private cache for NewLine
		private string newLine = Environment.NewLine;
		/// <summary>
		/// Get/set the newline string to append to each line.
		/// </summary>
		public string NewLine
		{
			get { return newLine; }
			set { newLine = value; }
		}

		/// <summary>
		/// Process the contents of Loc
		/// </summary>
		/// <returns>Text in a form appropriate for ShortTypeName.</returns>
		public string[] Dump()
		{
			List<string> lines = new List<string>();
			lines.Add(SlocFileReport.SummaryDelimitedHeader(',') + newLine);
			foreach (SlocFileReport sfr in loc)
			{
				lines.Add(sfr.SummaryDelimited(',') + newLine);
			}
			return lines.ToArray();
		}
	}
}
