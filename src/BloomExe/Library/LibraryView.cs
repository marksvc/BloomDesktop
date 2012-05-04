﻿using System;
using System.Windows.Forms;
using Ionic.Zip;
using Palaso.Reporting;

namespace Bloom.Library
{
	public partial class LibraryView :  UserControl, IBloomTabArea
	{
		private readonly LibraryModel _model;


		private LibraryListView _collectionListView;
		private LibraryBookView _bookView;

		public LibraryView(LibraryModel model, LibraryListView.Factory libraryListViewFactory, LibraryBookView.Factory templateBookViewFactory)
		{
			_model = model;
			InitializeComponent();

			_collectionListView = libraryListViewFactory();
			_collectionListView.Dock = DockStyle.Fill;
			splitContainer1.Panel1.Controls.Add(_collectionListView);

			_bookView = templateBookViewFactory();
			_bookView.Dock = DockStyle.Fill;
			splitContainer1.Panel2.Controls.Add(_bookView);

			splitContainer1.SplitterDistance = _collectionListView.PreferredWidth;
			_makeBloomPackButton.Visible = model.IsShellProject;
		}

		public string CollectionTabLabel
		{
			get { return "Collections"; }//_model.IsShellProject ? "Shell Collection" : "Collection"; }

		}

		private void LibraryView_VisibleChanged(object sender, EventArgs e)
		{
			if(Visible)
			{
				UsageReporter.SendNavigationNotice("Library");
			}
		}

		private void OnMakeBloomPackButton_Click(object sender, EventArgs e)
		{
			using(var dlg = new SaveFileDialog())
			{
				dlg.FileName = _model.GetSuggestedBloomPackPath();
				dlg.Filter = "BloomPack|*.BloomPack";
				dlg.RestoreDirectory = true;
				dlg.OverwritePrompt = true;
				if(DialogResult.Cancel == dlg.ShowDialog())
				{
					return;
				}
				_model.MakeBloomPack(dlg.FileName);
			}
		}

		public string HelpTopicUrl
		{
			get { return "/Tasks/Vernacular_Collection_tasks/Vernacular_Collections_tasks_overview.htm"; }
		}

		public Control TopBarControl
		{
			get { return _topBarControl; }
		}
	}
}
