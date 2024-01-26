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
	public partial class Help : Form
	{
		public Help()
		{
			InitializeComponent();
			VersionInfo vi = new VersionInfo();
			this.Text = String.Format(vi.FriendlyVersionInfo);
			string helpText = string.Empty;
			ResourceManager locRM =
				new ResourceManager("LOCCounter.Resources.Strings", typeof(LOCCountForm).Assembly);

			helpText += locRM.GetString("resHelp00") + Environment.NewLine + Environment.NewLine;

			helpText += locRM.GetString("resHelp01") + Environment.NewLine;
			helpText += locRM.GetString("resHelp02") + Environment.NewLine;
			helpText += locRM.GetString("resHelp03") + Environment.NewLine;
			helpText += locRM.GetString("resHelp04") + Environment.NewLine;
			helpText += locRM.GetString("resHelp05") + Environment.NewLine + Environment.NewLine;

			helpText += locRM.GetString("resHelp06") + Environment.NewLine + Environment.NewLine;

			helpText += locRM.GetString("resHelp07") + Environment.NewLine + Environment.NewLine;

			helpText += locRM.GetString("resHelp08") + Environment.NewLine + Environment.NewLine;

			helpText += locRM.GetString("resHelp09") + Environment.NewLine + Environment.NewLine;

			helpText += locRM.GetString("resHelp10") + Environment.NewLine + Environment.NewLine;

			helpText += locRM.GetString("resHelp11") + Environment.NewLine + Environment.NewLine;

			helpText += locRM.GetString("resHelp12");

			textBoxHelp.Text = helpText;
			textBoxHelp.Select(0, 0);
		}
	}
}
