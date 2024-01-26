/* Copyright (c) 2011, Gary Gocek. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sloc;

namespace LOCCounterStd
{
	/// <summary>
	/// An inheritable class for producing save-able output from
	/// data procduced by the Sloc class.
	/// </summary>
	public interface IOutputType
	{
		/// <summary>
		/// Get/set the name for the type of output to be produced.
		/// </summary>
		string ShortTypeName { get; set; }

		/// <summary>
		/// Get/set the information (about the lines of code) to be processed.
		/// </summary>
		SlocFileReport[] Loc { get; set; }

		/// <summary>
		/// Get/set the newline string to append to each line.
		/// </summary>
		string NewLine { get; set; }

		/// <summary>
		/// Process the contents of Loc
		/// </summary>
		/// <returns>Text in a form appropriate for ShortTypeName.</returns>
		string[] Dump();
	}
}
