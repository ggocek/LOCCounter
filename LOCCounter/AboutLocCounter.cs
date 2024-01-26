using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace LOCCounter
{
	partial class AboutLocCounter : Form
	{
		public AboutLocCounter()
		{
			InitializeComponent();
			VersionInfo vi = new VersionInfo();
			
			this.Text = String.Format(vi.FriendlyVersionInfo);
			this.labelProductName.Text = vi.FriendlyProductInfo;
			this.labelVersion.Text = vi.FriendlyVersionInfo;
			this.labelCopyright.Text = vi.FriendlyCopyrightInfo;
			this.linkCompany.Text = vi.FriendlyCompanyInfo;

			this.textBoxDescription.Text = vi.FriendlyDescriptionInfo + Environment.NewLine + Environment.NewLine +
                "MIT License" + Environment.NewLine +
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
        }

        private void okButton_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		private void linkCompany_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			// Specify that the link was visited.
			this.linkCompany.LinkVisited = true;
			// Navigate to a URL.
			System.Diagnostics.Process.Start(this.linkCompany.Text);
		}
	}
}
