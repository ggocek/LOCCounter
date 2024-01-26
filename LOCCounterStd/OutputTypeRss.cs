/* Copyright (c) 2011, Gary Gocek. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Syndication;
using Sloc;

namespace LOCCounterStd
{
	/// <summary>
	/// A class for producing save-able RSS 2.0 output from
	/// data procduced by the Sloc class.
	/// </summary>
	public class OutputTypeRss : IOutputType
	{
		// Private cache for ShortTypeName
		private string shortTypeName = "RSS";
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
			SyndicationFeed sf =
				new System.ServiceModel.Syndication.SyndicationFeed(
					"Lines of code", "Lines of code", new Uri("http://www.gocek.org"), string.Empty, DateTime.Now);
			SyndicationPerson sp =
				new SyndicationPerson("gary@gocek.org", "LOC Counter", "http://www.gocek.org");
			sf.Authors.Add(sp);

			List<SyndicationItem> feedItems = new List<SyndicationItem>();
			foreach (SlocFileReport sfr in loc)
			{
				SyndicationItem feedItem =
					new SyndicationItem(
						sfr.FullPath,
						sfr.Summary,
						new Uri("http://www.gocek.org"));
				feedItems.Add(feedItem);
			}

			sf.Items = feedItems;
			System.Text.StringBuilder sb = new StringBuilder();
			System.Xml.XmlWriter rssWriter = System.Xml.XmlWriter.Create(sb);
			Rss20FeedFormatter rssFormatter = new Rss20FeedFormatter(sf);
			rssFormatter.WriteTo(rssWriter);
			rssWriter.Close();

			// By default, XmlWriter and Rss20FeedFormatter generate RSS text using encoding="utf-16".
			// But a command prompt using the file redirector ">" generates a utf-8 text file,
			// and if you try to open that XML in a browser, you get:
			// "Switch from current encoding to specified encoding not supported."
			// If the file is opened in notepad and saved as Unicode, the result will open in a browser.
			// So, the encoding value in the XML header should be reset to utf-8, and there is info
			// on the web that a string replacement as below is as good a way as any to change the encoding.
			// In particular, using an XmlWriterSettings object with Encoding set to UTF-8 doesn't work.
			// System.Xml.XmlWriterSettings xSet = new System.Xml.XmlWriterSettings();
			// xSet.Encoding = System.Text.Encoding.UTF8; // doesn't work when sent to the XmlWriter constructor
			// Here is the hokey way to set the encoding:
			return new string[] { sb.ToString().Replace("utf-16", "utf-8") };
		}
	}
}
