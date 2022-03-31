﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using Bloom.Api;
using Bloom.Book;
using Bloom.Collection;
using Bloom.Properties;
using Bloom.TeamCollection;
using Bloom.ToPalaso;
using Bloom.ToPalaso.Experimental;
using Bloom.Utils;
using Bloom.web.controllers;
using DesktopAnalytics;
using L10NSharp;
using SIL.IO;
using SIL.Progress;
using SIL.Reporting;
using SIL.Windows.Forms;
using SIL.Xml;
using SIL.Windows.Forms.FileSystem;

namespace Bloom.CollectionTab
{
	public class CollectionModel
	{
		private readonly BookSelection _bookSelection;
		private readonly string _pathToLibrary;
		private readonly CollectionSettings _collectionSettings;
		private readonly SourceCollectionsList _sourceCollectionsList;
		private readonly BookCollection.Factory _bookCollectionFactory;
		private readonly EditBookCommand _editBookCommand;
		private readonly BookServer _bookServer;
		private readonly CurrentEditableCollectionSelection _currentEditableCollectionSelection;
		private List<BookCollection> _bookCollections;
		private readonly BookThumbNailer _thumbNailer;
		private TeamCollectionManager _tcManager;
		private readonly BloomWebSocketServer _webSocketServer;
		private LocalizationChangedEvent _localizationChangedEvent;
		private BookCollectionHolder _bookCollectionHolder;

		public CollectionModel(string pathToLibrary, CollectionSettings collectionSettings,
			BookSelection bookSelection,
			SourceCollectionsList sourceCollectionsList,
			BookCollection.Factory bookCollectionFactory,
			EditBookCommand editBookCommand,
			CreateFromSourceBookCommand createFromSourceBookCommand,
			BookServer bookServer,
			CurrentEditableCollectionSelection currentEditableCollectionSelection,
			BookThumbNailer thumbNailer,
			TeamCollectionManager tcManager,
			BloomWebSocketServer webSocketServer,
			BookCollectionHolder bookCollectionHolder,
			LocalizationChangedEvent localizationChangedEvent)
		{
			_bookSelection = bookSelection;
			_pathToLibrary = pathToLibrary;
			_collectionSettings = collectionSettings;
			_sourceCollectionsList = sourceCollectionsList;
			_bookCollectionFactory = bookCollectionFactory;
			_editBookCommand = editBookCommand;
			_bookServer = bookServer;
			_currentEditableCollectionSelection = currentEditableCollectionSelection;
			_thumbNailer = thumbNailer;
			_tcManager = tcManager;
			_webSocketServer = webSocketServer;
			_bookCollectionHolder = bookCollectionHolder;
			_localizationChangedEvent = localizationChangedEvent;

			createFromSourceBookCommand.Subscribe(CreateFromSourceBook);
		}

		/// <summary>
		/// The constructor of BookCommandsApi calls this to work around an Autofac circularity problem.
		/// </summary>
		public BookCommandsApi BookCommands { get; set; }


		public bool CanDeleteSelection
		{
			get { return _bookSelection.CurrentSelection != null && _collectionSettings.AllowDeleteBooks && _bookSelection.CurrentSelection.CanDelete; }

		}

		public bool CanExportSelection
		{
			get { return _bookSelection.CurrentSelection != null && _bookSelection.CurrentSelection.CanExport; }
		}

		public bool CanUpdateSelection
		{
			get { return _bookSelection.CurrentSelection != null && _bookSelection.CurrentSelection.CanUpdate; }

		}

		internal CollectionSettings CollectionSettings
		{
			get { return _collectionSettings; }
		}

		public string LanguageName
		{
			get { return _collectionSettings.Language1.Name; }	// collection tab still uses collection language settings
		}

		private object _bookCollectionLock = new object(); // Locks creation of _bookCollections

		public IReadOnlyList<BookCollection> GetBookCollections()
		{
			lock (_bookCollectionLock)
			{
				if (_bookCollections == null)
				{
					_bookCollections = new List<BookCollection>(GetBookCollectionsOnce());

					//we want the templates to be second (after the vernacular collection) regardless of alphabetical sorting
					var templates = _bookCollections.First(c => c.Name == "Templates");
					_bookCollections.Remove(templates);
					_bookCollections.Insert(1, templates);
				}
				return _bookCollections;
			}
		}

		public BookInfo BookInfoFromCollectionAndId(string collectionPath, string bookId)
		{
			var collection = GetBookCollections().FirstOrDefault(c => c.PathToDirectory == collectionPath);
			if (collection == null)
				return null;
			return collection.GetBookInfos().FirstOrDefault(bi => bi.Id == bookId);
		}

		public void ReloadCollections()
		{
			lock (_bookCollectionLock)
			{
				_bookCollections = null;
				GetBookCollections();
			}

			_webSocketServer.SendEvent("editableCollectionList", "reload:" + _bookCollections[0].PathToDirectory);
		}

		public void DuplicateBook(Book.Book book)
		{
			var newBookDir = book.Storage.Duplicate();

			// Get rid of any TC status we copied from the original, so Bloom treats it correctly as a new book.
			BookStorage.RemoveLocalOnlyFiles(newBookDir);

			ReloadEditableCollection();

			var dupInfo = TheOneEditableCollection.GetBookInfos()
				.FirstOrDefault(info => info.FolderPath == newBookDir);
			if (dupInfo != null)
			{
				var newBook = GetBookFromBookInfo(dupInfo);
				SelectBook(newBook);
				BookHistory.AddEvent(newBook, BookHistoryEventType.Created, $"Duplicated from existing book \"{book.Title}\"");
			}
		}

		/// <summary>
		/// Eventually this might entirely replace ReloadCollections, since we probably never need to reload anything ut the first.
		/// For now it actually reloads them all, but at least allows clients that definitely only need the first reloaded to do so.
		/// </summary>
		/// <param name="collection"></param>
		public void ReloadEditableCollection()
		{
			// I hope we can get rid of this when we retire the old LibraryListView, but for now we need to keep both views up to date.
			// optimize: we only need to reload the first (editable) collection; better yet, we only need to add the one new book to it.
			ReloadCollections();       
		}

		public void UpdateLabelOfBookInEditableCollection(Book.Book book)
		{
			// We can find a more efficient way to do this if necessary.
			//ReloadEditableCollection();
			// This allows it to get resorted. Is that good? It may move dramatically, even disappear from the screen.
			TheOneEditableCollection.UpdateBookInfo(book.BookInfo);
			// This actually changes the label. (One would think that re-rendering the collection would do this,
			// but somehow the way we are caching the book title as state in anticipation of the following
			// message was preventing this. It might be redundant now.)
			BookCommandsApi.UpdateButtonTitle(_webSocketServer, book.BookInfo, book.NameBestForUserDisplay);

			// happens as a side effect
			//_webSocketServer.SendEvent("editableCollectionList", "reload:" + _bookCollections[0].PathToDirectory);
		}

		/// <summary>
		/// Titles of all the books in the vernacular collection.
		/// </summary>
		internal IEnumerable<string> BookTitles
		{
			get { return TheOneEditableCollection.GetBookInfos().Select(book => book.Title); }
		}

		public BookCollection TheOneEditableCollection
		{
			get { return GetBookCollections().First(c => c.Type == BookCollection.CollectionType.TheOneEditableCollection); }
		}

		
		public string VernacularLibraryNamePhrase
		{
			get { return _collectionSettings.VernacularCollectionNamePhrase; }
		}

		public bool IsShellProject
		{
			get { return _collectionSettings.IsSourceCollection; }
		}

		public bool ShowSourceCollections
		{
			get { return _collectionSettings.AllowNewBooks; }

		}

		private void SetupChangeNotifications(BookCollection collection)
		{
			collection.CollectionChanged += (sender, args) =>
			{
				_webSocketServer.SendEvent("editableCollectionList", "reload:" + collection.PathToDirectory);
			};

			_localizationChangedEvent?.Subscribe(unused =>
			{
				if (collection.IsFactoryInstalled)
				{
					_webSocketServer.SendEvent("editableCollectionList", "reload:" + collection.PathToDirectory);
				}
				else
				{
					// This is tricky. Reloading the collection won't do it, because nothing has changed that would cause
					// the buttons to re-render. But some of them may be showing a string like "Missing title" that
					// is localizable. This is not very efficient, as we may process updates for many books that
					// don't need it or don't even have buttons due to laziness. But changing UI language is really rare.
					foreach (var info in collection.GetBookInfos())
						BookCommands.RequestButtonLabelUpdate(collection.PathToDirectory, info.Id);
				}
			});
		}

		/// <summary>
		/// This may be called on any thread. Please leave things where the only call is in GetBookCollections,
		/// having claimed the appropriate lock.
		/// </summary>
		/// <returns></returns>
		private IEnumerable<BookCollection> GetBookCollectionsOnce()
		{
			BookCollection editableCollection;
			using (PerformanceMeasurement.Global?.Measure("Creating Primary Collection"))
			{
				editableCollection = _bookCollectionFactory(_pathToLibrary,
					BookCollection.CollectionType.TheOneEditableCollection);
				if (_bookCollectionHolder != null)
					_bookCollectionHolder.TheOneEditableCollection = editableCollection;
				SetupChangeNotifications(editableCollection);
			}

			_currentEditableCollectionSelection.SelectCollection(editableCollection);
			yield return editableCollection;

			foreach (var bookCollection in _sourceCollectionsList.GetSourceCollectionsFolders())
			{
				var collection = _bookCollectionFactory(bookCollection, BookCollection.CollectionType.SourceCollection);
				// Apart from the editable collection, I think only the downloaded books needs this (because books
				// can be deleted from it and possibly added by new downloads); but it seems safest to set up for all.
				SetupChangeNotifications(collection);
				yield return collection;
			}
		}

		private Form MainShell => Application.OpenForms.Cast<Form>().FirstOrDefault(f => f is Shell);

		// Not entirely happy that this method which launches a dialog is in the Model.
		// but with the Collection UI moving to JS I don't see a good alternative
		public void MakeBloomPack(bool forReaderTools)
		{
			using (var dlg = new DialogAdapters.SaveFileDialogAdapter())
			{
				dlg.FileName = GetSuggestedBloomPackPath();
				dlg.Filter = "BloomPack|*.BloomPack";
				dlg.RestoreDirectory = true;
				dlg.OverwritePrompt = true;
				if (DialogResult.Cancel == dlg.ShowDialog(MainShell))
				{
					return;
				}
				MakeBloomPack(dlg.FileName, forReaderTools);
			}
		}

		public void RescueMissingImages()
		{
			using (var dlg = new FolderBrowserDialog())
			{
				dlg.ShowNewFolderButton = false;
				dlg.Description = "Select the folder where replacement images can be found";
				if (DialogResult.OK == dlg.ShowDialog(MainShell))
				{
					AttemptMissingImageReplacements(dlg.SelectedPath);
				}
			}
		}
		public void MakeReaderTemplateBloompack()
		{
			using (var dlg = new MakeReaderTemplateBloomPackDlg())
			{
				dlg.SetLanguage(LanguageName);
				dlg.SetTitles(BookTitles);
				var mainWindow = MainShell;
				if (dlg.ShowDialog(mainWindow) != DialogResult.OK)
					return;
				MakeBloomPack(true);
			}
		}

		public  void SelectBook(Book.Book book)
		{
			 _bookSelection.SelectBook(book);
		}
		public Book.Book GetSelectedBookOrNull()
		{
			return _bookSelection.CurrentSelection;
		}
		public bool DeleteBook(Book.Book book, BookCollection collection = null)
		{
			if (collection == null)
				collection = TheOneEditableCollection;
			Debug.Assert(book.FolderPath == _bookSelection.CurrentSelection?.FolderPath);

			if (_bookSelection.CurrentSelection != null && _bookSelection.CurrentSelection.CanDelete)
			{
				if (IsCurrentBookInCollection())
				{
					if (!_bookSelection.CurrentSelection.IsSaveable)
					{
						var msg = LocalizationManager.GetString("TeamCollection.CheckOutForDelete",
							"Please check out the book before deleting it.");
						ErrorReport.NotifyUserOfProblem(msg);
						return false;
					}

					if (_tcManager.CannotDeleteBecauseDisconnected(_bookSelection.CurrentSelection.FolderPath))
					{
						var msg = LocalizationManager.GetString("TeamCollection.ConnectForDelete",
							"Please connect to the Team Collection before deleting books that are part of it.");
						ErrorReport.NotifyUserOfProblem(msg);
						return false;
					}
				}
				var bookName = _bookSelection.CurrentSelection.NameBestForUserDisplay;
				var confirmRecycleDescription = L10NSharp.LocalizationManager.GetString("CollectionTab.ConfirmRecycleDescription", "The book '{0}'");
				if (ConfirmRecycleDialog.JustConfirm(string.Format(confirmRecycleDescription, bookName), false, "Palaso"))
				{
					// The sequence of these is a bit arbitrary. We'd like to delete the book in both places.
					// Either could conceivably fail. If something goes wrong with removing the selection
					// from it (very unlikely), we may as well leave nothing changed. If we delete it from
					// the local collection but fail to delete it from the repo, it will come back at the
					// next startup. If we delete it from the repo but fail to delete it locally,
					// it will just stick around, and at least the desired team collection result has
					// been achieved and the local result won't be a surprise later. So it seems marginally
					// better to do them in this order.
					_bookSelection.SelectBook(null);
					if (collection == TheOneEditableCollection)
						_tcManager.CurrentCollection?.DeleteBookFromRepo(book.FolderPath);
					collection.DeleteBook(book.BookInfo);
					return true;
				}
			}
			return false;
		}

		private bool IsCurrentBookInCollection()
		{
			var currentFolder = Path.GetDirectoryName(_bookSelection.CurrentSelection.FolderPath);
			return (currentFolder == _collectionSettings.FolderPath);
		}

		public void DoubleClickedBook()
		{
			// If we need the book to be checked out for editing, make sure it is. Do not allow double click
			// to check it out. 
			if (_bookSelection.CurrentSelection?.IsSaveable ?? false)
			{
				_editBookCommand.Raise(_bookSelection.CurrentSelection);
			}
		}

		public void OpenFolderOnDisk()
		{
			try
			{
				PathUtilities.SelectFileInExplorer(_bookSelection.CurrentSelection.FolderPath);
			}
			catch (System.Runtime.InteropServices.COMException e)
			{
				SIL.Reporting.ErrorReport.NotifyUserOfProblem(e,
					"Bloom had a problem asking your operating system to show that folder. Sorry!");
			}
		}

		public void BringBookUpToDate()
		{
			var b = _bookSelection.CurrentSelection;
			_bookSelection.SelectBook(null);

			using (var dlg = new ProgressDialogForeground()) //REVIEW: this foreground dialog has known problems in other contexts... it was used here because of its ability to handle exceptions well. TODO: make the background one handle exceptions well
			{
				dlg.ShowAndDoWork(progress=>b.BringBookUpToDate(progress));
			}

			_bookSelection.SelectBook(b);
		}


		public void ExportInDesignXml(string path)
		{
			var pathToXnDesignXslt = FileLocationUtilities.GetFileDistributedWithApplication("xslts", "BloomXhtmlToDataForMergingIntoInDesign.xsl");

#if DEBUG
			 _bookSelection.CurrentSelection.OurHtmlDom.RawDom.Save(path.Replace(".xml",".xhtml"));
#endif

			var dom = _bookSelection.CurrentSelection.OurHtmlDom.ApplyXSLT(pathToXnDesignXslt);

			using (var writer = XmlWriter.Create(path, CanonicalXmlSettings.CreateXmlWriterSettings()))
			{
				dom.Save(writer);
			}
		}

		private bool bookHasClass(string className)
		{
			return _bookSelection.CurrentSelection?.OurHtmlDom.Body.GetAttribute("class").Contains(className) ?? false;
		}

		public bool IsBookLeveled => bookHasClass("leveled-reader");
		public bool IsBookDecodable => bookHasClass("decodable-reader");

		public void SetIsBookLeveled(bool leveled)
		{
			if (leveled && IsBookDecodable)
				SetIsBookDecodable(false);
			SetBookHasClass(leveled, "leveled-reader");
		}
		public void SetIsBookDecodable(bool decodable)
		{
			if (decodable && IsBookLeveled)
				SetIsBookLeveled(false);
			SetBookHasClass(decodable, "decodable-reader");
		}

		private void SetBookHasClass(bool shouldHaveClass, string className)
		{
			var body = _bookSelection.CurrentSelection?.OurHtmlDom.Body;
			if (body == null)
				return;
			var classVal = body.GetAttribute("class");
			if (shouldHaveClass && !classVal.Contains(className))
			{
				body.SetAttribute("class", (classVal + " " + className).Trim());
				_bookSelection.CurrentSelection.Save();
			} else if (!shouldHaveClass && classVal.Contains(className))
			{
				body.SetAttribute("class", classVal.Replace(className, "").Trim());
				_bookSelection.CurrentSelection.Save();
			}
		}

		/// <summary>
		/// All we do at this point is make a file with a ".doc" extension and open it.
		/// </summary>
		/// <remarks>
		/// The .doc extension allows the operating system to recognize which program
		/// should open the file, and the program (whether Microsoft Word or LibreOffice
		/// or OpenOffice) seems to handle HTML content just fine.
		/// </remarks>
		public void ExportDocFormat(string path)
		{
			var sourcePath = _bookSelection.CurrentSelection.GetPathHtmlFile();
			if (RobustFile.Exists(path))
			{
				RobustFile.Delete(path);
			}
			// Linux (Trusty) LibreOffice requires slightly different metadata at the beginning
			// of the file in order to recognize it as HTML.  Otherwise it opens the file as raw
			// HTML (See https://silbloom.myjetbrains.com/youtrack/issue/BL-2276 if you don't
			// believe me.)  I don't know any perfect way to add this information to the file,
			// but a simple string replace should be safe.  This change works okay for both
			// Windows and Linux and for all three programs (Word, OpenOffice and Libre Office).
			var content = RobustFile.ReadAllText(sourcePath);
			var fixedContent = content.Replace("<meta charset=\"UTF-8\">",
				"<meta http-equiv=\"content-type\" content=\"text/html; charset=utf-8\">");
			var xmlDoc = RepairWordVisibility(fixedContent);
			XmlHtmlConverter.SaveDOMAsHtml5(xmlDoc, path); // writes file and returns path
		}

		// BL-5998 Apparently Word doesn't read our CSS rules for bloom-visibility correctly.
		// So we're forced to control visibility more directly with inline styles.
		private static XmlDocument RepairWordVisibility(string content)
		{
			var xmlDoc = XmlHtmlConverter.GetXmlDomFromHtml(content);
			var dom = new HtmlDom(xmlDoc);
			var bloomEditableDivs = dom.RawDom.SafeSelectNodes("//div[contains(@class, 'bloom-editable')]");
			foreach (XmlElement editableDiv in bloomEditableDivs)
			{
				HtmlDom.SetInlineStyle(editableDiv,
					HtmlDom.HasClass(editableDiv, "bloom-visibility-code-on") ? "display: block;" : "display: none;");
			}

			return dom.RawDom;
		}

		public void UpdateThumbnailAsync(Book.Book book, HtmlThumbNailer.ThumbnailOptions thumbnailOptions, Action<Book.BookInfo, Image> callback, Action<Book.BookInfo, Exception> errorCallback)
		{
			if (!(book is ErrorBook))
			{
				_thumbNailer.RebuildThumbNailAsync(book, thumbnailOptions, callback, errorCallback);
			}

		}

		public void UpdateThumbnailAsync(Book.Book book)
		{
			UpdateThumbnailAsync(book, new HtmlThumbNailer.ThumbnailOptions(), RefreshOneThumbnail, HandleThumbnailerErrror);
		}

		private void RefreshOneThumbnail(Book.BookInfo bookInfo, Image image)
		{
			// The arguments here are not currently used (method signature is legacy),
			// but may be useful if we optimize.
			// optimize: I think this will reload all of them
			_webSocketServer.SendString("bookImage", "reload", bookInfo.Id);
		}

		private void HandleThumbnailerErrror(Book.BookInfo bookInfo, Exception error)
		{
			string path = Path.Combine(bookInfo.FolderPath, "thumbnail.png");
			try
			{
				Resources.Error70x70.Save(@path, ImageFormat.Png);
			}
			catch (Exception e)
			{
				Logger.WriteError("Could not save error icon for book", e);
			}

			RefreshOneThumbnail(bookInfo, Resources.Error70x70);
		}

		internal (string dirName, string dirPrefix) GetDirNameAndPrefixForCollectionBloomPack()
		{
			var dir = TheOneEditableCollection.PathToDirectory;
			return (dir, "");
		}

		public void MakeBloomPack(string path, bool forReaderTools = false)
		{
			var (dirName, dirPrefix) = GetDirNameAndPrefixForCollectionBloomPack();
			var rootName = Path.GetFileName(dirName);
			if (rootName == null) return;
			Logger.WriteEvent($"Making BloomPack at {path} forReaderTools={forReaderTools}");
			MakeBloomPackWithUI(path, dirName, dirPrefix, forReaderTools, isCollection: true);
		}

		internal (string dirName, string dirPrefix) GetDirNameAndPrefixForSingleBookBloomPack(string inputBookFolder)
		{
			var rootName = Path.GetFileName(inputBookFolder);
			if (rootName != null)
				rootName += Path.DirectorySeparatorChar;

			return (inputBookFolder, rootName);
		}

		public void MakeSingleBookBloomPack(string path, string inputBookFolder)
		{
			var (dirName, dirPrefix) = GetDirNameAndPrefixForSingleBookBloomPack(inputBookFolder);
			if (dirPrefix == null) return;
			Logger.WriteEvent($"Making single book BloomPack at {path} bookFolderPath={inputBookFolder}");
			MakeBloomPackWithUI(path, dirName, dirPrefix, false, isCollection: false);
		}

		private void MakeBloomPackWithUI(string path, string dir, string dirNamePrefix, bool forReaderTools, bool isCollection)
		{
			try
			{
				if (RobustFile.Exists(path))
				{
					// UI already got permission for this
					RobustFile.Delete(path);
				}
				using (var pleaseWait = new SimpleMessageDialog("Creating BloomPack...", "Bloom"))
				{
					try
					{
						pleaseWait.Show();
						pleaseWait.BringToFront();
						Application.DoEvents(); // actually show it
						Cursor.Current = Cursors.WaitCursor;

						Logger.WriteEvent("BloomPack path will be " + path + ", made from " + dir + " with rootName " + Path.GetFileName(dir));
						MakeBloomPackInternal(path, dir, dirNamePrefix, forReaderTools, isCollection);

						// show it
						Logger.WriteEvent("Showing BloomPack on disk");
						PathUtilities.SelectFileInExplorer(path);
						Analytics.Track("Create BloomPack");
					}
					finally
					{
						Cursor.Current = Cursors.Default;
						pleaseWait.Close();
					}
				}
			}
			catch (Exception e)
			{
				ErrorReport.NotifyUserOfProblem(e, "Could not make the BloomPack at " + path);
			}
		}

		/// <summary>
		/// Makes a BloomPack of the specified dir.
		/// </summary>
		/// <param name="path">The path to write to. Precondition: Must not exist.</param>
		internal void MakeBloomPackInternal(string path, string dir, string dirNamePrefix, bool forReaderTools, bool isCollection)
		{
			var excludeAudio = true; // don't want audio in bloompack
			if (isCollection)
			{
				BookCompressor.CompressCollectionDirectory(path, dir, dirNamePrefix, forReaderTools, excludeAudio);
			}
			else
			{
				BookCompressor.CompressBookDirectory(path, dir, dirNamePrefix, forReaderTools, excludeAudio);
			}
		}

		public string GetSuggestedBloomPackPath()
		{
			return TheOneEditableCollection.Name+".BloomPack";
		}

		public void DoUpdatesOfAllBooks()
		{
			using (var dlg = new ProgressDialogBackground())
			{
				dlg.ShowAndDoWork((progress, args) => DoUpdatesOfAllBooks(progress));
			}
		}

		public void DoUpdatesOfAllBooks(IProgress progress)
		{
			int i = 0;
			foreach (var bookInfo in TheOneEditableCollection.GetBookInfos())
			{
				i++;
				var book = _bookServer.GetBookFromBookInfo(bookInfo);
				//gets overwritten: progress.WriteStatus(book.NameBestForUserDisplay);
				progress.WriteMessage("Processing " + book.NameBestForUserDisplay+ " " + i + "/" + TheOneEditableCollection.GetBookInfos().Count());
				book.BringBookUpToDate(progress);
			}
		}

		public void DoChecksOfAllBooks()
		{
			using (var dlg = new ProgressDialogBackground())
			{
				dlg.ShowAndDoWork((progress, args) => DoChecksOfAllBooksBackgroundWork(dlg,null));
				if (dlg.Progress.ErrorEncountered || dlg.Progress.WarningsEncountered)
				{
					MessageBox.Show("Bloom will now open a list of problems it found.");
					var path = Path.GetTempFileName() + ".txt";
					RobustFile.WriteAllText(path, dlg.ProgressString.Text);
					PathUtilities.OpenFileInApplication(path);
				}
				else
				{
					MessageBox.Show("Bloom didn't find any problems.");
				}
			}
		}

		public void AttemptMissingImageReplacements(string pathToFolderOfReplacementImages=null)
		{
			using (var dlg = new ProgressDialogBackground())
			{
				dlg.ShowAndDoWork((progress, args) => DoChecksOfAllBooksBackgroundWork(dlg, pathToFolderOfReplacementImages));
				if (dlg.Progress.ErrorEncountered || dlg.Progress.WarningsEncountered)
				{
					MessageBox.Show("There were some problems. Bloom will now open a log of the attempt to replace missing images.");
				}
				else
				{
					MessageBox.Show("There are no more missing images. Bloom will now open a log of what it did.");
				}

				var path = Path.GetTempFileName() + ".txt";
				RobustFile.WriteAllText(path, dlg.ProgressString.Text);
				try
				{
					PathUtilities.OpenFileInApplication(path);
				}
				catch (System.OutOfMemoryException)
				{
					// This has happened at least once.  See https://silbloom.myjetbrains.com/youtrack/issue/BL-3431.
					MessageBox.Show("Bloom ran out of memory trying to open the log.  You should quit and restart the program.  (Your books should all be okay.)");
				}
			}

		}


		public void DoChecksOfAllBooksBackgroundWork(ProgressDialogBackground dialog, string pathToFolderOfReplacementImages)
		{
			var bookInfos = TheOneEditableCollection.GetBookInfos();
			var count = bookInfos.Count();
			if (count == 0)
				return;

			foreach (var bookInfo in bookInfos)
			{
				//not allowed in this thread: dialog.ProgressBar.Value++;
				dialog.Progress.ProgressIndicator.PercentCompleted += 100/count;

				var book = _bookServer.GetBookFromBookInfo(bookInfo);

				dialog.Progress.WriteMessage("Checking " + book.NameBestForUserDisplay);
				book.CheckBook(dialog.Progress, pathToFolderOfReplacementImages);
				dialog.ProgressString.WriteMessage("");
			}
			dialog.Progress.ProgressIndicator.PercentCompleted = 100;
		}


		private void CreateFromSourceBook(Book.Book sourceBook)
		{
			try
			{
				var newBook = _bookServer.CreateFromSourceBook(sourceBook, TheOneEditableCollection.PathToDirectory);
				if (newBook == null)
					return; //This can happen if there is a configuration dialog and the user clicks Cancel

				TheOneEditableCollection.AddBookInfo(newBook.BookInfo);

				if (_bookSelection != null)
				{
					Book.Book.SourceToReportForNextRename = Path.GetFileName(sourceBook.FolderPath);
					_bookSelection.SelectBook(newBook, aboutToEdit: true);
				}
				//enhance: would be nice to know if this is a new shell
				if (sourceBook.IsShellOrTemplate)
				{
					Analytics.Track("Create Book",
						new Dictionary<string, string>() {
							{ "Category", sourceBook.CategoryForUsageReporting},
							{ "BookId", newBook.ID},
							{ "Country", _collectionSettings.Country}
						});
				}
				// Better reported in Book_BookTitleChanged as a side effect of selecting it.
				// BookHistory.AddEvent(newBook, BookHistoryEventType.Created, "New book created");
				_editBookCommand.Raise(newBook);
			}
			catch (Exception e)
			{
				SIL.Reporting.ErrorReport.NotifyUserOfProblem(e,
					"Bloom ran into an error while creating that book. (Sorry!)");
			}

		}

		public Book.Book GetBookFromBookInfo(BookInfo bookInfo, bool fullyUpdateBookFiles = false)
		{
			// If we're looking for the current book it's important to return the actual book object,
			// because it could end up modified in ways that make our one out-of-date if we modify another
			// instance based on the same folder. For example, Rename could make our FolderPath wrong.
			if (bookInfo.FolderPath == _bookSelection.CurrentSelection?.FolderPath)
				return _bookSelection.CurrentSelection;
			return _bookServer.GetBookFromBookInfo(bookInfo, fullyUpdateBookFiles);
		}

		/// <summary>
		/// Zip up the book folder, excluding .pdf, .bloombookorder, .map, .bloompack, .db files.
		/// The resulting file should have a .bloomSource extension, but this depends on properly
		/// setting the value of destFileName before calling this method.  TeamCollection still
		/// uses the .bloom extension until we can figure out how to safely migrate teams as a
		/// whole to using .bloomSource.
		/// </summary>
		/// <param name="exception">any exception which occurs when trying to save the file</param>
		/// <returns>true if file was saved successfully; false otherwise</returns>
		/// <remarks>if return value is false, exception is non-null and vice versa</remarks>
		public static bool SaveAsBloomSourceFile(string srcFolderName, string destFileName, out Exception exception)
		{
			exception = null;
			try
			{
				var excludedExtensions = new[] { ".pdf", ".bloombookorder", ".map", ".bloompack", ".db" };

				Logger.WriteEvent("Zipping up {0} ...", destFileName);
				var zipFile = new BloomZipFile(destFileName);
				zipFile.AddDirectoryContents(srcFolderName, excludedExtensions);

				Logger.WriteEvent("Saving {0} ...", destFileName);
				zipFile.Save();

				if (destFileName.EndsWith(".bloom"))
					Logger.WriteEvent("Finished writing .bloom file.");
				else
					Logger.WriteEvent("Finished writing .bloomSource file.");
			}
			catch (Exception e)
			{
				exception = e;
				return false;
			}
			return true;
		}
	}
}