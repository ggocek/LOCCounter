using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Resources;

namespace LOCCounter
{
	/// <summary>
	/// A form for choosing the files extensions to consider.
	/// </summary>
	public partial class ExtensionsForm : Form
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public ExtensionsForm() : this(new Point(100, 100), string.Empty)
		{
			InitializeComponent();
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="callerLocation">The top-left corner of the button that opened this form
		/// in scsreen coordinates.</param>
		/// <param name="exts">Or-bar delimited extensions</param>
		public ExtensionsForm(Point callerLocation, string exts)
		{
			InitializeComponent();
			LoadStrings();

			int sw = Screen.PrimaryScreen.Bounds.Width;
			int sh = Screen.PrimaryScreen.Bounds.Height;

			int fw = Width;
			int fh = Height;

			ownerLoc = new Point(callerLocation.X - 100, callerLocation.Y - 25);

			if ((ownerLoc.X + fw) >= sw)
			{
				ownerLoc.X -= ((ownerLoc.X + fw) - sw);
			}
			if ((ownerLoc.Y + fh) >= sh)
			{
				ownerLoc.Y -= ((ownerLoc.Y + fh) - sh);
			}
			if (ownerLoc.X < 0)
			{
				ownerLoc.X = 0;
			}
			if (ownerLoc.Y < 0)
			{
				ownerLoc.Y = 0;
			}

			if (string.IsNullOrEmpty(exts))
				return;

			// Check the boxes for the specified extensions
			string[] fileExtensions = exts.Split(new char[] { '|' });
			List<string> notFound = new List<string>();
			foreach (string e in fileExtensions)
			{
				string e1;
				if (e.StartsWith("*."))
					e1 = e.Substring(2);
				else
					e1 = e;
				bool found = false;
				// Look through the default extensions for the associated checkbox, if any.
				foreach (CheckBox cb in DefaultCheckboxes)
				{
					if (cb.Tag.ToString() == e1)
					{
						cb.Checked = true;
						found = true;
						break;
					}
				}

				// If e was not found in any of the default checkboxes, then it is a custom extension.
				// Collect the custom extensions for later.
				if (!found)
				{
					notFound.Add(e);
				}
			}

			// Insert the custom extensions into empty slots.
			int nfCount = 0;
			foreach (Panel p in CustomPanels)
			{
				if (nfCount < notFound.Count)
				{
					// The panel has a checkbox control and a textbox control.
					// Each one must be set.
					foreach (Control c in p.Controls)
					{
						if (c is CheckBox)
						{
							((CheckBox)c).Checked = true;
							((CheckBox)c).Tag = notFound[nfCount];
						}
						else if (c is TextBox)
						{
							((TextBox)c).Text = notFound[nfCount];
						}
					}
				}
				nfCount++;
			}
		}

		/// <summary>
		/// Load the strings for the current culture
		/// </summary>
		private void LoadStrings()
		{
			try
			{
				// Declare a Resource Manager instance.
				ResourceManager locRM =
					new ResourceManager("LOCCounter.Resources.Strings", typeof(LOCCountForm).Assembly);
				Text = locRM.GetString("resExtensions");
				headerLabel.Text = locRM.GetString("resExtFormHeader");
				selectAllButton.Text = locRM.GetString("resExtFormSelectAll");
				deselectAllButton.Text = locRM.GetString("resExtFormDeselectAll");
				applyButton.Text = locRM.GetString("resApply");
				cancelButton.Text = locRM.GetString("resCancel");
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Localization error", MessageBoxButtons.OK);
			}
		}

		// Private cache for location of owner window,
		// provided to constructor.
		private Point ownerLoc = new Point(0, 0);

		// Private cache for ExtList
		private string extList = string.Empty;
		/// <summary>
		/// Get the list of extensions chosen by the user. Open the form as a modal dialog,
		/// and this property will be set when the dialog closes.
		/// Empty if the user canceled the extensions dialog.
		/// </summary>
		public string ExtList
		{
			get { return extList; }
		}

		/// <summary>
		/// Check the state of each checkbox and construct a string representing the chosen extensions.
		/// </summary>
		private void Apply()
		{
			extList = string.Empty;
			foreach (CheckBox c in DefaultCheckboxes)
			{
				if (c.Checked)
					extList += c.Tag.ToString() + "|";
			}
			foreach (CheckBox c in CustomCheckboxes)
			{
				if ((c.Checked) && (c.Tag.ToString().Length > 0))
					extList += c.Tag.ToString() + "|";
			}

			if (extList.Length > 0)
			{
				// Make sure nothing empty was inserted.
				while (extList.Contains("||"))
					extList.Replace("||", "|");

				// Strip off leading and trailing or-bars.
				if (extList.StartsWith("|"))
				{
					extList = extList.Substring(1);
				}
				if (extList.EndsWith("|"))
				{
					extList = extList.Substring(0, extList.Length - 1);
				}
			}
		}

		/// <summary>
		/// Get the list of default extension checkboxes, not including the custom checkboxes.
		/// The tag of each checkbox contains the associated extension.
		/// </summary>
		private List<Control> DefaultCheckboxes
		{
			get
			{
				// These checkboxes are at the top level of the form
				List<Control> lc = new List<Control>();
				foreach (Control c in this.Controls)
				{
					if (c is CheckBox)
					{
						lc.Add(c);
					}
				}
				return lc;
			}
		}

		/// <summary>
		/// Get the list of customizable extension checkboxes, not including the default checkboxes.
		/// The tag of each checkbox contains the associated extension.
		/// </summary>
		private List<Control> CustomCheckboxes
		{
			get
			{
				// Panels at the top level of the form contain a checkbox and textbox.
				List<Control> lc = new List<Control>();
				foreach (Panel p in CustomPanels)
				{
					CheckBox cfound = null;
					TextBox tfound = null;
					foreach (Control c in p.Controls)
					{
						if (c is CheckBox)
						{
							cfound = (CheckBox)c;
							if ((tfound != null) && (tfound is TextBox))
							{
								c.Tag = tfound.Text;
								lc.Add(c);
								break;
							}
						}
						else if (c is TextBox)
						{
							tfound = (TextBox)c;
							if ((cfound != null) && (cfound is CheckBox))
							{
								cfound.Tag = tfound.Text;
								lc.Add(cfound);
								break;
							}
						}
					}
				}
				return lc;
			}
		}

		/// <summary>
		/// Get the list of panels contaning customizable extension checkboxes.
		/// </summary>
		private List<Control> CustomPanels
		{
			get
			{
				// Panels at the top level of the form contain a checkbox and textbox.
				List<Control> lc = new List<Control>();
				foreach (Control p in this.Controls)
				{
					if (p is Panel)
					{
						lc.Add(p);
					}
				}
				return lc;
			}
		}

		/// <summary>
		/// Move the form to a spot relative to the owner.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ExtensionsForm_Shown(object sender, EventArgs e)
		{
			Location = ownerLoc;
		}

		/// <summary>
		/// Save the checked boxes and return.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void applyButton_Click(object sender, EventArgs e)
		{
			Apply();
			Close();
		}

		/// <summary>
		/// Return with an empty selection list.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void cancelButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		/// <summary>
		/// Check all the default checkboxes and also the custom checkboxes that have text.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void selectAllButton_Click(object sender, EventArgs e)
		{
			foreach (CheckBox c in DefaultCheckboxes)
				c.Checked = true;
			foreach (CheckBox c in CustomCheckboxes)
				if (c.Tag.ToString().Length > 0)
					c.Checked = true;
		}

		/// <summary>
		/// Uncheck all the checkboxes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void deselectAllButton_Click(object sender, EventArgs e)
		{
			foreach (CheckBox c in DefaultCheckboxes)
				c.Checked = false;
			foreach (CheckBox c in CustomCheckboxes)
				c.Checked = false;
		}
	}
}
