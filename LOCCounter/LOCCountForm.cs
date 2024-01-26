/* Copyright (c) 2011, Gary Gocek. */
using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Resources;
using Sloc;

namespace LOCCounter
{
	/// <summary>
	/// A form for starting the LOC counting program.
	/// </summary>
	public class LOCCountForm : System.Windows.Forms.Form
	{
		#region MAIN

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			// Started with no args. Show the GUI.
			System.Windows.Forms.Application.EnableVisualStyles();
			System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
			System.Windows.Forms.Application.Run(new LOCCountForm());
		}

		#endregion

		#region File type icon stuff
		// This stuff gets system information on the file extensions requested by the user,
		// such as a file type description and a file type icon.

		private Hashtable _FileExtensionInfoTable;
		private System.Windows.Forms.ImageList _fileIconImageList;
		// The choices made on the extension form
		private string extensionFormChoices = string.Empty;

		internal const uint SHGFI_ICON = 0x000000100;	// get icon
		internal const uint SHGFI_TYPENAME = 0x000000400;	// get type name
		internal const uint SHGFI_SYSICONINDEX = 0x000004000;	// get system icon index
		internal const uint SHGFI_SMALLICON = 0x000000001;	// get small icon
		internal const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;
		internal const uint SHGFI_LARGEICON = 0x000000000;	// get large icon

		// Use when pszPath is a string
		[DllImport("shell32.dll", CharSet = CharSet.Auto)]
		internal static extern IntPtr SHGetFileInfo(string pszPath, int
			dwFileAttributes, out SHFILEINFO psfi, int cbFileInfo, uint uFlags);

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		internal struct SHFILEINFO
		{
			private const int MAX_PATH = 260;
			public IntPtr hIcon;
			public int iIcon;
			public int dwAttributes;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
			public string szDisplayName;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
			public string szTypeName;
		}
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		internal struct LPSTR
		{
			private const int MAX_PATH = 260;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
			public string lpBuffer;
		}

		/// <summary>
		/// Load file type icons from the operating system.
		/// </summary>
		private void LoadImageList()
		{
			this._fileIconImageList.Images.Clear();
			string[] fileExtensions = extensionFormChoices.Split(new char[] { '|' });

			int nonEmpty = 0;
			for (int i = 0; i < fileExtensions.Length; i++)
			{
				if (fileExtensions[i].Length <= 0)
					continue;
				SHFILEINFO rInfo = new SHFILEINFO();
				string strType;
				if (fileExtensions[i].StartsWith("*."))
					strType = fileExtensions[i].Remove(0, 2);
				else
					strType = fileExtensions[i];
				IntPtr pointer = SHGetFileInfo("foo." + strType, 0, out rInfo, Marshal.SizeOf(typeof(SHFILEINFO)),
					SHGFI_ICON |
					SHGFI_SMALLICON |
					SHGFI_SYSICONINDEX |
					SHGFI_USEFILEATTRIBUTES |
					SHGFI_TYPENAME
					);
				Icon aIcon = System.Drawing.Icon.FromHandle(rInfo.hIcon);
				this._fileIconImageList.Images.Add(aIcon);
				_FileExtensionInfoTable[strType] = new string[] { rInfo.szTypeName, nonEmpty.ToString() };
				nonEmpty++;
			}
		}

		#endregion

		private MenuStrip menuStrip1;
		private ToolStripMenuItem fileToolStripMenuItem;
		private ToolStripMenuItem exitToolStripMenuItem;
		private ToolStripMenuItem helpToolStripMenuItem;
		private ToolStripMenuItem aboutToolStripMenuItem;
		private ToolStripMenuItem contentsToolStripMenuItem;
		private ToolStripMenuItem browseToolStripMenuItem;
		private ToolStripMenuItem countLOCToolStripMenuItem;
		private Label RootFolderLabel;
		private Label BrowseLabel;
		private ToolStripStatusLabel toolStripStatusBrowseLabel;
		private Button extensionsButton;
		private Label extensionsLabel;
		private TextBox extensionsTextBox;
		private Button browseButton;
		private FolderBrowserDialog folderBrowserDialog1;
		private StatusStrip statusStrip1;
		private Button cancelButton;
		private ToolStripProgressBar toolStripProgressBar1;
		private BackgroundWorker backgroundWorkerCountLines;
		private ToolStripStatusLabel toolStripStatusLabel1;
		private ContextMenuStrip itemContextMenu;
		private ToolStripMenuItem openSelectionInNotepadToolStripMenuItem;	// use passed dwFileAttribute
		private ToolStripMenuItem totalsToolStripMenuItem;

		private System.Windows.Forms.TextBox txtProjectDirectory;
		private System.Windows.Forms.Label lblProjectDirectory;
		private System.Windows.Forms.Label lblFileTypes;
		private System.Windows.Forms.Button btnCountNumLines;
		private System.Windows.Forms.ColumnHeader columnHeaderFileName;
		private System.Windows.Forms.ListView listViewFiles;
		private System.Windows.Forms.ColumnHeader columnHeaderLineCount;
		private System.Windows.Forms.ColumnHeader columnHeaderCommentLines;
		private System.Windows.Forms.ColumnHeader columnHeaderEmptyLines;
		private System.Windows.Forms.ColumnHeader columnHeaderResult;
		private System.Windows.Forms.ColumnHeader columnHeaderDirectory;
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.ColumnHeader columnHeaderFileType;

		// The counter instance.
		private Sloc.Sloc counter = null;

		/// <summary>
		/// Set the window location and size when opening
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void LOCCountForm_Load(object sender, EventArgs e)
		{
			// Set window location
			if (LOCCounter.Properties.Settings.Default.WindowLocation != null)
			{
				this.Location = LOCCounter.Properties.Settings.Default.WindowLocation;
			}
			
			// Set window size
			if (LOCCounter.Properties.Settings.Default.WindowSize != null)
			{
				this.Size = LOCCounter.Properties.Settings.Default.WindowSize;
			}
		}

		/// <summary>
		/// Save the window location and size when closing
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void LOCCountForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			// Copy window location to app settings
			LOCCounter.Properties.Settings.Default.WindowLocation = this.Location;
			
			// Copy window size to app settings
			if (this.WindowState == FormWindowState.Normal)
			{
				LOCCounter.Properties.Settings.Default.WindowSize = this.Size;
			}
			else
			{
				LOCCounter.Properties.Settings.Default.WindowSize = this.RestoreBounds.Size;
			}
			
			// Save settings
			LOCCounter.Properties.Settings.Default.Save();
		}

		/// <summary>
		/// Constructor
		/// </summary>
		public LOCCountForm()
		{
			// Required for Windows Form Designer support
			InitializeComponent();

			// TODO: remember the window size and location. But the easiest way to do that is with app.config,
			// but that would be yet another file that the user has to co-locate with the EXE. Or, CommentDelimiters.xml
			// could be overloaded with that info, but that would be messy for the user.
			LoadStrings();
			_FileExtensionInfoTable = new Hashtable();
			extensionFormChoices = ((Sloc.Sloc)(new Sloc.Sloc())).FileExtensions;
			LoadExtensionsTextBox();

			VersionInfo vi = new VersionInfo();
			this.Text = vi.FriendlyVersionInfo;
			toolStripStatusLabel1.Text = string.Empty;

			if (listViewFiles.Items.Count > 0)
			{
				listViewFiles.ContextMenuStrip = itemContextMenu;
			}
			else
			{
				listViewFiles.ContextMenuStrip = null;
			}

			// Init the starting location
			if (folderBrowserDialog1.SelectedPath == string.Empty)
			{
				if (txtProjectDirectory.Text == string.Empty)
				{
					folderBrowserDialog1.SelectedPath = "c:\\";
					txtProjectDirectory.Text = folderBrowserDialog1.SelectedPath;
				}
				else
				{
					folderBrowserDialog1.SelectedPath = txtProjectDirectory.Text;
				}
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
				fileToolStripMenuItem.Text = locRM.GetString("resFile");
				browseToolStripMenuItem.Text = locRM.GetString("resBrowseElipsis");
				countLOCToolStripMenuItem.Text = locRM.GetString("resCountLOC");
				exitToolStripMenuItem.Text = locRM.GetString("resExit");
				helpToolStripMenuItem.Text = locRM.GetString("resHelp");
				contentsToolStripMenuItem.Text = locRM.GetString("resContents");
				aboutToolStripMenuItem.Text = locRM.GetString("resAbout");
				RootFolderLabel.Text = locRM.GetString("resCountFolder");
				BrowseLabel.Text = locRM.GetString("resBrowse");
				btnCountNumLines.Text = locRM.GetString("resCountLines");
				cancelButton.Text = locRM.GetString("resCancel");
				toolStripStatusBrowseLabel.Text = locRM.GetString("resUseStd");
				extensionsButton.Text = locRM.GetString("resAddRemoveExt");
				extensionsLabel.Text = locRM.GetString("resExtensions");

				listViewFiles.Columns[0].Text = locRM.GetString("resFileName"); // "columnHeaderFileName"
				listViewFiles.Columns[1].Text = locRM.GetString("resFileType"); // "columnHeaderFileType"
				listViewFiles.Columns[2].Text = locRM.GetString("resLines"); // "columnHeaderLineCount"
				listViewFiles.Columns[3].Text = locRM.GetString("resComments"); // "columnHeaderCommentLines"
				listViewFiles.Columns[4].Text = locRM.GetString("resBlank"); // "columnHeaderEmptyLines"
				listViewFiles.Columns[5].Text = locRM.GetString("resSourceLOC"); // "columnHeaderResult"
				listViewFiles.Columns[6].Text = locRM.GetString("resDirectory"); // "columnHeaderDirectory"
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Localization error", MessageBoxButtons.OK);
			}
		}

		/// <summary>
		/// Put the extensions form choices (or a default) into the textbox on the main form.
		/// </summary>
		private void LoadExtensionsTextBox()
		{
			extensionsTextBox.Text = string.Empty;
			string[] fileExtensions = extensionFormChoices.Split(new char[] { '|' });
			int e = 0;
			while (e < fileExtensions.Length)
			{
				for (int e1 = 0; e1 < 4; e1++)
				{
					if (e < fileExtensions.Length)
					{
						string eT = fileExtensions[e];
						if (!eT.StartsWith("*."))
							eT = "*." + eT;
						extensionsTextBox.Text += eT + " ";
					}
					e++;
				}
				extensionsTextBox.Text += Environment.NewLine;
			}
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LOCCountForm));
			this.txtProjectDirectory = new System.Windows.Forms.TextBox();
			this.btnCountNumLines = new System.Windows.Forms.Button();
			this.lblProjectDirectory = new System.Windows.Forms.Label();
			this.lblFileTypes = new System.Windows.Forms.Label();
			this.listViewFiles = new System.Windows.Forms.ListView();
			this.columnHeaderFileName = new System.Windows.Forms.ColumnHeader();
			this.columnHeaderFileType = new System.Windows.Forms.ColumnHeader();
			this.columnHeaderLineCount = new System.Windows.Forms.ColumnHeader();
			this.columnHeaderCommentLines = new System.Windows.Forms.ColumnHeader();
			this.columnHeaderEmptyLines = new System.Windows.Forms.ColumnHeader();
			this.columnHeaderResult = new System.Windows.Forms.ColumnHeader();
			this.columnHeaderDirectory = new System.Windows.Forms.ColumnHeader();
			this.itemContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.openSelectionInNotepadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.totalsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this._fileIconImageList = new System.Windows.Forms.ImageList(this.components);
			this.browseButton = new System.Windows.Forms.Button();
			this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
			this.statusStrip1 = new System.Windows.Forms.StatusStrip();
			this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
			this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
			this.toolStripStatusBrowseLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.cancelButton = new System.Windows.Forms.Button();
			this.backgroundWorkerCountLines = new System.ComponentModel.BackgroundWorker();
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.browseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.countLOCToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.contentsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.RootFolderLabel = new System.Windows.Forms.Label();
			this.BrowseLabel = new System.Windows.Forms.Label();
			this.extensionsButton = new System.Windows.Forms.Button();
			this.extensionsLabel = new System.Windows.Forms.Label();
			this.extensionsTextBox = new System.Windows.Forms.TextBox();
			this.itemContextMenu.SuspendLayout();
			this.statusStrip1.SuspendLayout();
			this.menuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// txtProjectDirectory
			// 
			resources.ApplyResources(this.txtProjectDirectory, "txtProjectDirectory");
			this.txtProjectDirectory.Name = "txtProjectDirectory";
			// 
			// btnCountNumLines
			// 
			this.btnCountNumLines.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
			resources.ApplyResources(this.btnCountNumLines, "btnCountNumLines");
			this.btnCountNumLines.Name = "btnCountNumLines";
			this.btnCountNumLines.UseVisualStyleBackColor = false;
			this.btnCountNumLines.Click += new System.EventHandler(this.btnCountNumLines_Click);
			// 
			// lblProjectDirectory
			// 
			resources.ApplyResources(this.lblProjectDirectory, "lblProjectDirectory");
			this.lblProjectDirectory.Name = "lblProjectDirectory";
			// 
			// lblFileTypes
			// 
			resources.ApplyResources(this.lblFileTypes, "lblFileTypes");
			this.lblFileTypes.Name = "lblFileTypes";
			// 
			// listViewFiles
			// 
			this.listViewFiles.AllowColumnReorder = true;
			resources.ApplyResources(this.listViewFiles, "listViewFiles");
			this.listViewFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderFileName,
            this.columnHeaderFileType,
            this.columnHeaderLineCount,
            this.columnHeaderCommentLines,
            this.columnHeaderEmptyLines,
            this.columnHeaderResult,
            this.columnHeaderDirectory});
			this.listViewFiles.ContextMenuStrip = this.itemContextMenu;
			this.listViewFiles.FullRowSelect = true;
			this.listViewFiles.Name = "listViewFiles";
			this.listViewFiles.SmallImageList = this._fileIconImageList;
			this.listViewFiles.UseCompatibleStateImageBehavior = false;
			this.listViewFiles.View = System.Windows.Forms.View.Details;
			this.listViewFiles.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listViewFiles_ColumnClick);
			// 
			// columnHeaderFileName
			// 
			resources.ApplyResources(this.columnHeaderFileName, "columnHeaderFileName");
			// 
			// columnHeaderFileType
			// 
			resources.ApplyResources(this.columnHeaderFileType, "columnHeaderFileType");
			// 
			// columnHeaderLineCount
			// 
			resources.ApplyResources(this.columnHeaderLineCount, "columnHeaderLineCount");
			// 
			// columnHeaderCommentLines
			// 
			resources.ApplyResources(this.columnHeaderCommentLines, "columnHeaderCommentLines");
			// 
			// columnHeaderEmptyLines
			// 
			resources.ApplyResources(this.columnHeaderEmptyLines, "columnHeaderEmptyLines");
			// 
			// columnHeaderResult
			// 
			resources.ApplyResources(this.columnHeaderResult, "columnHeaderResult");
			// 
			// columnHeaderDirectory
			// 
			resources.ApplyResources(this.columnHeaderDirectory, "columnHeaderDirectory");
			// 
			// itemContextMenu
			// 
			this.itemContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openSelectionInNotepadToolStripMenuItem,
            this.totalsToolStripMenuItem});
			this.itemContextMenu.Name = "contextMenuStrip1";
			resources.ApplyResources(this.itemContextMenu, "itemContextMenu");
			// 
			// openSelectionInNotepadToolStripMenuItem
			// 
			this.openSelectionInNotepadToolStripMenuItem.Name = "openSelectionInNotepadToolStripMenuItem";
			resources.ApplyResources(this.openSelectionInNotepadToolStripMenuItem, "openSelectionInNotepadToolStripMenuItem");
			this.openSelectionInNotepadToolStripMenuItem.Click += new System.EventHandler(this.openSelectionInNotepadToolStripMenuItem_Click);
			// 
			// totalsToolStripMenuItem
			// 
			this.totalsToolStripMenuItem.Name = "totalsToolStripMenuItem";
			resources.ApplyResources(this.totalsToolStripMenuItem, "totalsToolStripMenuItem");
			this.totalsToolStripMenuItem.Click += new System.EventHandler(this.totalSLOCToolStripMenuItem_Click);
			// 
			// _fileIconImageList
			// 
			this._fileIconImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
			resources.ApplyResources(this._fileIconImageList, "_fileIconImageList");
			this._fileIconImageList.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// browseButton
			// 
			resources.ApplyResources(this.browseButton, "browseButton");
			this.browseButton.Name = "browseButton";
			this.browseButton.UseVisualStyleBackColor = true;
			this.browseButton.Click += new System.EventHandler(this.browseButton_Click);
			// 
			// folderBrowserDialog1
			// 
			resources.ApplyResources(this.folderBrowserDialog1, "folderBrowserDialog1");
			this.folderBrowserDialog1.RootFolder = System.Environment.SpecialFolder.MyComputer;
			this.folderBrowserDialog1.ShowNewFolderButton = false;
			// 
			// statusStrip1
			// 
			this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripProgressBar1,
            this.toolStripStatusLabel1,
            this.toolStripStatusBrowseLabel});
			resources.ApplyResources(this.statusStrip1, "statusStrip1");
			this.statusStrip1.Name = "statusStrip1";
			// 
			// toolStripProgressBar1
			// 
			this.toolStripProgressBar1.Name = "toolStripProgressBar1";
			resources.ApplyResources(this.toolStripProgressBar1, "toolStripProgressBar1");
			// 
			// toolStripStatusLabel1
			// 
			resources.ApplyResources(this.toolStripStatusLabel1, "toolStripStatusLabel1");
			this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
			// 
			// toolStripStatusBrowseLabel
			// 
			this.toolStripStatusBrowseLabel.Name = "toolStripStatusBrowseLabel";
			resources.ApplyResources(this.toolStripStatusBrowseLabel, "toolStripStatusBrowseLabel");
			// 
			// cancelButton
			// 
			this.cancelButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
			resources.ApplyResources(this.cancelButton, "cancelButton");
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.UseVisualStyleBackColor = false;
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// backgroundWorkerCountLines
			// 
			this.backgroundWorkerCountLines.WorkerSupportsCancellation = true;
			this.backgroundWorkerCountLines.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorkerCountLines_DoWork);
			// 
			// menuStrip1
			// 
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.helpToolStripMenuItem});
			resources.ApplyResources(this.menuStrip1, "menuStrip1");
			this.menuStrip1.Name = "menuStrip1";
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.browseToolStripMenuItem,
            this.countLOCToolStripMenuItem,
            this.exitToolStripMenuItem});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			resources.ApplyResources(this.fileToolStripMenuItem, "fileToolStripMenuItem");
			// 
			// browseToolStripMenuItem
			// 
			this.browseToolStripMenuItem.Name = "browseToolStripMenuItem";
			resources.ApplyResources(this.browseToolStripMenuItem, "browseToolStripMenuItem");
			this.browseToolStripMenuItem.Click += new System.EventHandler(this.browseButton_Click);
			// 
			// countLOCToolStripMenuItem
			// 
			this.countLOCToolStripMenuItem.Name = "countLOCToolStripMenuItem";
			resources.ApplyResources(this.countLOCToolStripMenuItem, "countLOCToolStripMenuItem");
			this.countLOCToolStripMenuItem.Click += new System.EventHandler(this.btnCountNumLines_Click);
			// 
			// exitToolStripMenuItem
			// 
			this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			resources.ApplyResources(this.exitToolStripMenuItem, "exitToolStripMenuItem");
			this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
			// 
			// helpToolStripMenuItem
			// 
			this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.contentsToolStripMenuItem,
            this.aboutToolStripMenuItem});
			this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
			resources.ApplyResources(this.helpToolStripMenuItem, "helpToolStripMenuItem");
			// 
			// contentsToolStripMenuItem
			// 
			this.contentsToolStripMenuItem.Name = "contentsToolStripMenuItem";
			resources.ApplyResources(this.contentsToolStripMenuItem, "contentsToolStripMenuItem");
			this.contentsToolStripMenuItem.Click += new System.EventHandler(this.contentsToolStripMenuItem_Click);
			// 
			// aboutToolStripMenuItem
			// 
			this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
			resources.ApplyResources(this.aboutToolStripMenuItem, "aboutToolStripMenuItem");
			this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
			// 
			// RootFolderLabel
			// 
			resources.ApplyResources(this.RootFolderLabel, "RootFolderLabel");
			this.RootFolderLabel.Name = "RootFolderLabel";
			// 
			// BrowseLabel
			// 
			resources.ApplyResources(this.BrowseLabel, "BrowseLabel");
			this.BrowseLabel.Name = "BrowseLabel";
			// 
			// extensionsButton
			// 
			resources.ApplyResources(this.extensionsButton, "extensionsButton");
			this.extensionsButton.Name = "extensionsButton";
			this.extensionsButton.UseVisualStyleBackColor = true;
			this.extensionsButton.Click += new System.EventHandler(this.extensionsButton_Click);
			// 
			// extensionsLabel
			// 
			resources.ApplyResources(this.extensionsLabel, "extensionsLabel");
			this.extensionsLabel.Name = "extensionsLabel";
			// 
			// extensionsTextBox
			// 
			resources.ApplyResources(this.extensionsTextBox, "extensionsTextBox");
			this.extensionsTextBox.Name = "extensionsTextBox";
			this.extensionsTextBox.ReadOnly = true;
			this.extensionsTextBox.TabStop = false;
			// 
			// LOCCountForm
			// 
			resources.ApplyResources(this, "$this");
			this.Controls.Add(this.extensionsTextBox);
			this.Controls.Add(this.extensionsLabel);
			this.Controls.Add(this.extensionsButton);
			this.Controls.Add(this.BrowseLabel);
			this.Controls.Add(this.RootFolderLabel);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.statusStrip1);
			this.Controls.Add(this.menuStrip1);
			this.Controls.Add(this.listViewFiles);
			this.Controls.Add(this.browseButton);
			this.Controls.Add(this.lblFileTypes);
			this.Controls.Add(this.lblProjectDirectory);
			this.Controls.Add(this.btnCountNumLines);
			this.Controls.Add(this.txtProjectDirectory);
			this.MainMenuStrip = this.menuStrip1;
			this.Name = "LOCCountForm";
			this.Load += new System.EventHandler(this.LOCCountForm_Load);
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.LOCCountForm_FormClosing);
			this.itemContextMenu.ResumeLayout(false);
			this.statusStrip1.ResumeLayout(false);
			this.statusStrip1.PerformLayout();
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		/// <summary>
		/// Create an item to be inserted as a row of the list view.
		/// </summary>
		/// <param name="fileR">File info</param>
		/// <returns>The item to insert</returns>
		private ListViewItem CreateListViewItem(SlocFileReport fileR)
		{
			int result = fileR.LinesTotal - fileR.LinesComment - fileR.LinesBlank;
			string fileName = fileR.FileName;
			int extIndex = fileName.LastIndexOf('.');
			string[] fileExtensionInfo = null;
			if (extIndex < 0)
			{
				// Usually, the TOTAL row. No extension, just fake the file type.
				fileExtensionInfo = new string[] { string.Empty, "99999" };
			}
			else
			{
				string ext = fileName.Substring(extIndex + 1).ToLower();
				fileExtensionInfo = (string[])_FileExtensionInfoTable[ext];
				if (fileExtensionInfo == null)
				{
					// The file has an extension requested by the user, but we couldn't get info on that extension.
					// Fake it.
					fileExtensionInfo = new string[] { ext, "99999" };
				}
			}
			ListViewItem item = null;
			item = new ListViewItem(
				new string[]
				{
					fileR.FileName,
					fileExtensionInfo[0],
					fileR.LinesTotal.ToString(),
					fileR.LinesComment.ToString(),
					fileR.LinesBlank.ToString(),
					result.ToString(),
					fileR.FolderName
				},
				Convert.ToInt32(fileExtensionInfo[1]));

			if (fileR.IsTotal)
			{
				item.ForeColor = Color.Red;
			}

			return item;
		}

		// Put the file info here
		List<SlocFileReport> sfrs = new List<SlocFileReport>();

		/// <summary>
		/// Click handler for the button to count the lines in the files.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnCountNumLines_Click(object sender, System.EventArgs e)
		{
			try
			{
				this.Cursor = Cursors.WaitCursor;
				btnCountNumLines.Enabled = false;
				fileToolStripMenuItem.Enabled = false;
				exitToolStripMenuItem.Enabled = false;
				helpToolStripMenuItem.Enabled = false;
				aboutToolStripMenuItem.Enabled = false;
				browseButton.Enabled = false;
				extensionsButton.Enabled = false;
				txtProjectDirectory.Enabled = false;
				cancelButton.Visible = true;
				cancelButton.Enabled = true;
				cancelButton.Cursor = Cursors.Default;

				toolStripProgressBar1.Minimum = 0;
				toolStripProgressBar1.Maximum = 100;
				toolStripProgressBar1.Value = 1;
				toolStripStatusLabel1.Text = "Collecting files...";

				listViewFiles.Items.Clear();
				listViewFiles.ContextMenuStrip = null;

				// Show the status message
				Application.DoEvents();

				// Init the SLOC counter object
				counter = new Sloc.Sloc();

				// Collect the extenstions to be counted this run.
				if (!string.IsNullOrEmpty(extensionFormChoices))
					counter.FileExtensions = extensionFormChoices;

				counter.RootFolder = this.txtProjectDirectory.Text;
				// The number of files isn't known until later,
				// so reset the progress bar in the async caller.
				toolStripProgressBar1.Maximum = 1000000;

				bool countOk = AsyncWithProgressBar();
				toolStripProgressBar1.Value = toolStripProgressBar1.Maximum;

				// If there was an error or cancelation, abort.
				if (!countOk)
					return;

				int comment = sfrs[0].LinesComment;
				int empty = sfrs[0].LinesBlank;
				int total = sfrs[0].LinesTotal;

				this.LoadImageList();

				foreach (SlocFileReport fileR in sfrs)
				{
					this.listViewFiles.Items.Add(this.CreateListViewItem(fileR));
				}
				if (listViewFiles.Items.Count > 0)
				{
					listViewFiles.ContextMenuStrip = itemContextMenu;
					AlternatingItemBackColor();
				}
			}
			catch (System.Exception error)
			{
				MessageBox.Show(error.Message);
			}
			finally
			{
				this.Cursor = Cursors.Default;
				btnCountNumLines.Enabled = true;
				fileToolStripMenuItem.Enabled = true;
				exitToolStripMenuItem.Enabled = true;
				helpToolStripMenuItem.Enabled = true;
				aboutToolStripMenuItem.Enabled = true;
				browseButton.Enabled = true;
				extensionsButton.Enabled = true;
				txtProjectDirectory.Enabled = true;
				cancelButton.Visible = false;
				cancelButton.Enabled = false;
				toolStripStatusLabel1.Text = string.Empty;
			}
		}

		/// <summary>
		/// Invoke the asynchronous worker method to count the lines and diddle the progress bar while waiting.
		/// </summary>
		private bool AsyncWithProgressBar()
		{
			try
			{
				string counterNumfiles = string.Empty;
				// Start the background operation that will do the work
				if (!backgroundWorkerCountLines.IsBusy)
					backgroundWorkerCountLines.RunWorkerAsync();
				// Wait until the number of files is available
				while (backgroundWorkerCountLines.IsBusy)
				{
					if (counter.NumFiles > 0)
						break;
					toolStripStatusLabel1.Text = "Collecting files (" + counter.CurExtension + ")";
					Application.DoEvents();
				}
				// If any files were found, wait until the lines are counted
				if (counter.NumFiles > 0)
				{
					counterNumfiles = counter.NumFiles.ToString();
					toolStripProgressBar1.Maximum = counter.NumFiles;
					while (backgroundWorkerCountLines.IsBusy)
					{
						if (toolStripProgressBar1.Value != counter.CurFile)
							toolStripProgressBar1.Value = counter.CurFile;

						if ((counter.CurFile % 100) == 0)
						{
							toolStripStatusLabel1.Text = counter.CurFile.ToString() + " / " + counterNumfiles;
						}

						Application.DoEvents();
					}
				}
				// A cancelation request successfully terminates the while loop, but we don't
				// know here if it was canceled or completed normally. The worker set 'sfrs',
				// and that will have zero elements if it was canceled. A valid folder with no
				// countable files will return one element (the total line showing zero files).
				return (sfrs.Count > 0);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK);
				return false;
			}
		}

		/// <summary>
		/// Asynchronously count the lines in all the files.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void backgroundWorkerCountLines_DoWork(object sender, DoWorkEventArgs e)
		{
			try
			{
				sfrs = counter.FolderSloc(backgroundWorkerCountLines, e);
			}
			catch (Exception ex)
			{
				sfrs = new List<SlocFileReport>();
				SlocFileReport s1 = new SlocFileReport();
				s1.FileName = ex.Message;
				sfrs.Add(s1);
				e.Cancel = true;
			}
		}

		/// <summary>
		/// Attempt to stop the counting process.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void cancelButton_Click(object sender, EventArgs e)
		{
			backgroundWorkerCountLines.CancelAsync();
		}

		/// <summary>
		/// Click handler for when the user clicks on a listview column, to sort by that column.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void listViewFiles_ColumnClick(object sender, System.Windows.Forms.ColumnClickEventArgs e)
		{
			try
			{
				ListViewColumnSort sorter = null;
				SortOrder sortOrder = SortOrder.Ascending;
				if (this.listViewFiles.Sorting == SortOrder.Ascending)
				{
					sortOrder = SortOrder.Descending;
					this.listViewFiles.Sorting = SortOrder.Ascending;
				}
				else
					this.listViewFiles.Sorting = SortOrder.Descending;

				if (e.Column == 0 || e.Column == 1 || e.Column == 6)
					sorter = new ListViewColumnSort(e.Column, sortOrder, ListViewColumnSort.CompareType.Text);
				else
					sorter = new ListViewColumnSort(e.Column, sortOrder, ListViewColumnSort.CompareType.Numeric);

				this.listViewFiles.ListViewItemSorter = sorter;

				if (this.listViewFiles.Sorting == SortOrder.Ascending)
					this.listViewFiles.Sorting = SortOrder.Descending;
				else
					this.listViewFiles.Sorting = SortOrder.Ascending;

				AlternatingItemBackColor();

				this.listViewFiles.ListViewItemSorter = null;
			}
			catch (System.Exception error)
			{
				String Error = error.Message;
				MessageBox.Show(Error);

			}
		}

		/// <summary>
		/// Recolor the background of the items
		/// </summary>
		private void AlternatingItemBackColor()
		{
			for (int iii = 0; iii < listViewFiles.Items.Count; iii += 2)
			{
				if (iii < listViewFiles.Items.Count)
				{
					listViewFiles.Items[iii].BackColor = Color.White;
				}
				if ((iii + 1) < listViewFiles.Items.Count)
				{
					listViewFiles.Items[iii + 1].BackColor = Color.LightYellow;
				}
			}
		}

		/// <summary>
		/// Determine the first selected file and open it in Notepad.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void openSelectionInNotepadToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				if (listViewFiles.SelectedItems.Count <= 0)
				{
					MessageBox.Show("Select a file.", "Nothing selected", MessageBoxButtons.OK);
					return;
				}

				string fileText = listViewFiles.SelectedItems[0].SubItems[0].Text;
				string dirText = listViewFiles.SelectedItems[0].SubItems[6].Text;
				if (System.IO.File.Exists(dirText + "\\" + fileText))
					System.Diagnostics.Process.Start("notepad.exe", dirText + "\\" + fileText);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK);
			}
		}

		/// <summary>
		/// Sum the SLOC for the selected items and show it in a popup.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void totalSLOCToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				if (listViewFiles.SelectedItems.Count <= 0)
				{
					MessageBox.Show("Select one or more files.", "Nothing selected", MessageBoxButtons.OK);
					return;
				}

				int fileCount = 0;
				int linesTot = 0;
				int commentsTot = 0;
				int blankTot = 0;
				int slocTot = 0;
				foreach (ListViewItem lvi in listViewFiles.SelectedItems)
				{
					int thisLines= 0;
					int thisComments = 0;
					int thisBlank = 0;
					int thisSloc = 0;
					try
					{
						fileCount++;
						thisLines = Convert.ToInt32(lvi.SubItems[2].Text);
						thisComments = Convert.ToInt32(lvi.SubItems[3].Text);
						thisBlank = Convert.ToInt32(lvi.SubItems[4].Text);
						thisSloc = Convert.ToInt32(lvi.SubItems[5].Text);
					}
					catch { }
					linesTot += thisLines;
					commentsTot += thisComments;
					blankTot += thisBlank;
					slocTot += thisSloc;
				}

				try
				{
					// Declare a Resource Manager instance.
					ResourceManager locRM =
						new ResourceManager("LOCCounter.Resources.Strings", typeof(LOCCountForm).Assembly);
					string locLines = locRM.GetString("resLines");
					string locComments = locRM.GetString("resComments");
					string locBlank = locRM.GetString("resBlank");
					string locSloc = locRM.GetString("resSourceLOC");

					MessageBox.Show(
						"For " + fileCount.ToString() + " selected files:" + Environment.NewLine +
						locLines + " = " + linesTot.ToString() + Environment.NewLine +
						locComments + " = " + commentsTot.ToString() + Environment.NewLine +
						locBlank + " = " + blankTot.ToString() + Environment.NewLine +
						locSloc + " = " + slocTot.ToString(),
						"Selection totals", MessageBoxButtons.OK);
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.Message, "Localization error", MessageBoxButtons.OK);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK);
			}
		}

		/// <summary>
		/// Browse for a folder
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void browseButton_Click(object sender, EventArgs e)
		{
			DialogResult dr = folderBrowserDialog1.ShowDialog();
			if (dr == DialogResult.OK)
			{
				txtProjectDirectory.Text = folderBrowserDialog1.SelectedPath;
			}
		}

		/// <summary>
		/// Exit the app
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Application.Exit();
		}

		/// <summary>
		/// Show an About box
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			AboutLocCounter a = new AboutLocCounter();
			a.ShowDialog(this);
		}

		/// <summary>
		/// Show the form to manage extensions.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void extensionsButton_Click(object sender, EventArgs e)
		{
			ExtensionsForm extForm =
				new ExtensionsForm(PointToScreen(extensionsButton.Location), extensionFormChoices);
			try
			{
				extForm.ShowDialog();
				if (!string.IsNullOrEmpty(extForm.ExtList))
					extensionFormChoices = extForm.ExtList;
				LoadExtensionsTextBox();
			}
			catch
			{
				if (extForm != null)
					extForm.Close();
			}
		}

		/// <summary>
		/// Show help info
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void contentsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Help h = new Help();
			h.MinimizeBox = false;
			h.ShowDialog(this);
		}

		/// <summary>
		/// A class for sorting the list of files
		/// </summary>
		private class ListViewColumnSort : IComparer
		{
			public enum CompareType
			{
				Text,
				Numeric,
				DateTime
			}
			private int m_SubItemIndex = 0;
			private SortOrder m_SortOrder;
			private CompareType m_compareType;
			public ListViewColumnSort(int SubItemIndex, SortOrder sortOrder, CompareType compareType)
			{
				this.m_SubItemIndex = SubItemIndex;
				this.m_SortOrder = sortOrder;
				this.m_compareType = compareType;
			}
			public int Compare(object objCompare1, object objCompare2)
			{
				//Implements IComparer.Compare
				int rc = 0;
				ListViewItem lviItem1;
				ListViewItem lviItem2;
				lviItem1 = (ListViewItem)objCompare1;
				lviItem2 = (ListViewItem)objCompare2;


				//Sort in Ascending/Descending order
				if (m_SortOrder == SortOrder.Ascending)
				{
					//Execute sort based on Compare Method
					switch (this.m_compareType)
					{
						case CompareType.Text:
							rc = String.Compare(lviItem1.SubItems[m_SubItemIndex].Text, lviItem2.SubItems[m_SubItemIndex].Text);
							break;
						case CompareType.Numeric:
							rc = Decimal.Compare(Convert.ToDecimal(lviItem1.SubItems[m_SubItemIndex].Text), Convert.ToDecimal(lviItem2.SubItems[m_SubItemIndex].Text));
							break;
						case CompareType.DateTime:
							rc = DateTime.Compare(Convert.ToDateTime(lviItem1.SubItems[m_SubItemIndex].Text), Convert.ToDateTime(lviItem2.SubItems[m_SubItemIndex].Text));
							break;
					}
				}
				else
				{
					//Execute sort based on Compare Method
					switch (m_compareType)
					{
						case CompareType.Text:
							rc = String.Compare(lviItem2.SubItems[m_SubItemIndex].Text, lviItem1.SubItems[m_SubItemIndex].Text);
							break;
						case CompareType.Numeric:
							rc = Decimal.Compare(Convert.ToDecimal(lviItem2.SubItems[m_SubItemIndex].Text), Convert.ToDecimal(lviItem1.SubItems[m_SubItemIndex].Text));
							break;
						case CompareType.DateTime:
							rc = DateTime.Compare(Convert.ToDateTime(lviItem2.SubItems[m_SubItemIndex].Text), Convert.ToDateTime(lviItem1.SubItems[m_SubItemIndex].Text));
							break;
					}
				}

				//Destroy local objects
				lviItem1 = null;
				lviItem2 = null;
				return rc;
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
					object[] oTitle = assm.GetCustomAttributes(typeof(AssemblyTitleAttribute), true);
					string myTitle = ((System.Reflection.AssemblyTitleAttribute)oTitle[0]).Title;

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

		/// <summary>
		/// Geta friendly string indicating the version.
		/// </summary>
		public string FriendlyCopyrightInfo
		{
			get
			{
				string ret = string.Empty;

				try
				{
					Assembly assm = Assembly.GetExecutingAssembly();
					AssemblyName an = AssemblyName.GetAssemblyName(assm.Location);
					object[] oCopy = assm.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), true);
					string myCopyright = ((System.Reflection.AssemblyCopyrightAttribute)oCopy[0]).Copyright;

					ret = string.Format("{0}", myCopyright);
				}
				catch
				{
					//
				}

				return ret;
			}
		}

		/// <summary>
		/// Geta friendly string indicating the version.
		/// </summary>
		public string FriendlyCompanyInfo
		{
			get
			{
				string ret = string.Empty;

				try
				{
					Assembly assm = Assembly.GetExecutingAssembly();
					AssemblyName an = AssemblyName.GetAssemblyName(assm.Location);
					object[] oCompany = assm.GetCustomAttributes(typeof(AssemblyCompanyAttribute), true);
					string myCompany = ((System.Reflection.AssemblyCompanyAttribute)oCompany[0]).Company;

					ret = string.Format("{0}", myCompany);
				}
				catch
				{
					//
				}

				return ret;
			}
		}

		/// <summary>
		/// Geta friendly string indicating the title.
		/// </summary>
		public string FriendlyTitleInfo
		{
			get
			{
				string ret = string.Empty;

				try
				{
					Assembly assm = Assembly.GetExecutingAssembly();
					AssemblyName an = AssemblyName.GetAssemblyName(assm.Location);
					object[] oTitle = assm.GetCustomAttributes(typeof(AssemblyTitleAttribute), true);
					string myTitle = ((System.Reflection.AssemblyTitleAttribute)oTitle[0]).Title;

					ret = string.Format("{0}", myTitle);
				}
				catch
				{
					//
				}

				return ret;
			}
		}

		/// <summary>
		/// Geta friendly string indicating the product.
		/// </summary>
		public string FriendlyProductInfo
		{
			get
			{
				string ret = string.Empty;

				try
				{
					Assembly assm = Assembly.GetExecutingAssembly();
					AssemblyName an = AssemblyName.GetAssemblyName(assm.Location);
					object[] oProduct = assm.GetCustomAttributes(typeof(AssemblyProductAttribute), true);
					string myProduct = ((System.Reflection.AssemblyProductAttribute)oProduct[0]).Product;

					ret = string.Format("{0}", myProduct);
				}
				catch
				{
					//
				}

				return ret;
			}
		}

		/// <summary>
		/// Geta friendly string indicating the description.
		/// </summary>
		public string FriendlyDescriptionInfo
		{
			get
			{
				string ret = string.Empty;

				try
				{
					Assembly assm = Assembly.GetExecutingAssembly();
					AssemblyName an = AssemblyName.GetAssemblyName(assm.Location);
					object[] oDescription = assm.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), true);
					string myDescription = ((System.Reflection.AssemblyDescriptionAttribute)oDescription[0]).Description;

					ret = string.Format("{0}", myDescription);
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
