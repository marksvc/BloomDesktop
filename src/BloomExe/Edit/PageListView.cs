using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Bloom.Api;
using Bloom.Book;
using Bloom.web;
using Fleck;
using L10NSharp;
using SIL.Reporting;
using SIL.Windows.Forms.Reporting;

namespace Bloom.Edit
{
	public partial class PageListView : UserControl
	{
		private readonly PageSelection _pageSelection;
		private readonly EditingModel _model;
		private bool _dontForwardSelectionEvent;
		private IPage _pageWeThinkShouldBeSelected;
		private bool _hyperlinkMessageShown = false;

		public PageListView(PageSelection pageSelection, RelocatePageEvent relocatePageEvent, EditingModel model,
			HtmlThumbNailer thumbnailProvider, NavigationIsolator isolator, ControlKeyEvent controlKeyEvent, PageListApi pageListApi, BloomWebSocketServer webSocketServer)
		{
			_pageSelection = pageSelection;
			_model = model;
			this.Font= SystemFonts.MessageBoxFont;
			InitializeComponent();
			_thumbNailList.PageListApi = pageListApi;
			_thumbNailList.WebSocketServer = webSocketServer;
			this.BackColor = Palette.SidePanelBackgroundColor;

			_thumbNailList.Thumbnailer = thumbnailProvider;
			_thumbNailList.RelocatePageEvent = relocatePageEvent;
			_thumbNailList.PageSelectedChanged+=new EventHandler(OnPageSelectedChanged);
			_thumbNailList.ControlKeyEvent = controlKeyEvent;
			_thumbNailList.Model = model;
			_thumbNailList.BringToFront(); // needed to get DockStyle.Fill to work right.
			// First action determines whether the menu item is enabled, second performs it.
			var menuItems = new List<WebThumbNailList.MenuItemSpec>();
			menuItems.Add(
				new WebThumbNailList.MenuItemSpec() {
					Label = LocalizationManager.GetString("EditTab.DuplicatePageButton", "Duplicate Page"),
					EnableFunction = (page) => page != null && _model.CanDuplicatePage,
					ExecuteCommand = (page) => _model.DuplicatePage(page)});
			menuItems.Add(
				new WebThumbNailList.MenuItemSpec()
				{
					Label = LocalizationManager.GetString("EditTab.DuplicatePageMultiple", "Duplicate Page Many Times..."),
					EnableFunction = (page) => page != null && _model.CanDuplicatePage,
					ExecuteCommand = (page) => _model.DuplicateManyPages(page)
				});
			menuItems.Add(
				new WebThumbNailList.MenuItemSpec()
				{
					Label = LocalizationManager.GetString("EditTab.CopyPage", "Copy Page"),
					EnableFunction = (page) => page != null && _model.CanCopyPage,
					ExecuteCommand = (page) => _model.CopyPage(page)
				});
			menuItems.Add(
				new WebThumbNailList.MenuItemSpec()
				{
					Label = LocalizationManager.GetString("EditTab.CopyHyperlink", "Copy Hyperlink"),
					EnableFunction = (page) => page != null && _model.CanCopyHyperlink,
					ExecuteCommand = (page) =>
					{
						_model.CopyHyperlink(page);
						if (!_hyperlinkMessageShown)
						{
							_hyperlinkMessageShown = true;
							var msg = LocalizationManager.GetString("EditTab.HowToUseHyperlink",
								"To use this hyperlink, go to the page where you are making a Table of Contents. Next, select some text and then click on the image of chain link. This will turn the selected text into a hyperlink to this page.");
							var title = LocalizationManager.GetString("EditTab.UsingHyperlink", "Using a hyperlink");
							var dlg = new ProblemNotificationDialog(
								msg, title);
							dlg.Icon = SystemIcons.Information.ToBitmap();
							dlg.ReoccurenceMessage = null;
							dlg.Show();
						}
					}
				});
			menuItems.Add(
				new WebThumbNailList.MenuItemSpec()
				{
					Label = LocalizationManager.GetString("EditTab.PastePage", "Paste Page"),
					EnableFunction = (page) => page != null && _model.CanAddPages && _model.GetClipboardHasPage(),
					ExecuteCommand = (page) => _model.PastePage(page)
				});
			menuItems.Add(
				new WebThumbNailList.MenuItemSpec() {
					Label = LocalizationManager.GetString("EditTab.DeletePageButton", "Remove Page"),
					EnableFunction = (page) => page != null && _model.CanDeletePage,
					ExecuteCommand = (page) =>
					{
						if (ConfirmRemovePageDialog.Confirm())
							_model.DeletePage(page);
					}});
			menuItems.Add(
				new WebThumbNailList.MenuItemSpec() {
					Label = LocalizationManager.GetString("EditTab.ChooseLayoutButton", "Choose Different Layout"),
					EnableFunction = (page) => page != null && !page.Required && !_model.CurrentBook.LockedDown,
					ExecuteCommand = (page) => _model.ChangePageLayout(page)});
			// This adds the desired menu items to the Gecko context menu that happens when we right-click
			_thumbNailList.ContextMenuProvider = args =>
			{
				var page = _thumbNailList.GetPageContaining(args.TargetNode);
				if (page == null)
					return true; // no page-related commands if we didn't click on one.
				if(page != _pageSelection.CurrentSelection)
				{
					return true; //it's too dangerous to let users do thing to a page they aren't seeing
				}
				foreach (var item in menuItems)
				{
					var menuItem = new MenuItem(item.Label, (sender, eventArgs) => item.ExecuteCommand(page));
					args.ContextMenu.MenuItems.Add(menuItem);
					menuItem.Enabled = item.EnableFunction(page);
				}
				return true;
			};
			// This sets up the context menu items that will be shown when the user clicks the
			// arrow in the thumbnail list.
			_thumbNailList.ContextMenuItems = menuItems;
		}

		private void OnPageSelectedChanged(object page, EventArgs e)
		{
			if (page == null)
				return;
			if (!_dontForwardSelectionEvent)
			{
				_pageSelection.SelectPage(page as Page);
			}
		}

		private void PageListView_BackColorChanged(object sender, EventArgs e)
		{
			_thumbNailList.BackColor = BackColor;
		}

		public void SetBook(Book.Book book)//review: could do this instead by giving this class the bookselection object
		{
			if (book == null)
			{
				_thumbNailList.SetItems(new Page[] { });
			}
			else
			{
				_thumbNailList.SetItems(new IPage[] { new PlaceHolderPage() }.Concat(book.GetPages()));
			  //  _thumbNailList.SetItems(book.GetPages());

				if(_pageWeThinkShouldBeSelected !=null)
				{
					//this var will be set previously when someone told us the page we're to select,
					//but had not yet given us leave to do the time-consuming process of actually
					//making the thumbnails and showing them.
					SelectThumbnailWithoutSendingEvent(_pageWeThinkShouldBeSelected);
				}
			}
		}

		public void UpdateThumbnailAsync(IPage page)
		{
			Logger.WriteMinorEvent("Updating thumbnail for page");

			// This might be redundant, we no longer use premade images of pages in this view.
			// However, it's just possible that when the cover page is modified we need this
			// to get an updated cover image in the Collections view.
			_thumbNailList.Thumbnailer.PageChanged(page.Id);
			_thumbNailList.UpdateThumbnailAsync(page);
		}

		public void UpdateAllThumbnails()
		{
			_thumbNailList.UpdateAllThumbnails();
		}

		public void Clear()
		{
			_thumbNailList.SetItems(new IPage[]{});
		}

		public void SelectThumbnailWithoutSendingEvent(IPage page)
		{
            _pageWeThinkShouldBeSelected = page;
			try
			{
				_dontForwardSelectionEvent = true;
				_thumbNailList.SelectPage(page);
			}
			finally
			{
				_dontForwardSelectionEvent = false;
			}
		}

		public void EmptyThumbnailCache()
		{
			_thumbNailList.EmptyThumbnailCache();
		}

		public new bool Enabled
		{
			set { _thumbNailList.Enabled = value; }
		}
	}
}
