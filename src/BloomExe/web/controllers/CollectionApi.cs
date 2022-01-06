﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Bloom.Api;
using Bloom.Book;
using Bloom.Collection;
using Bloom.CollectionTab;
using Bloom.MiscUI;
using Bloom.Properties;
using Bloom.Spreadsheet;
using Bloom.TeamCollection;
using Bloom.ToPalaso;
using Bloom.Workspace;
using DesktopAnalytics;
using Gecko.WebIDL;
using L10NSharp;
using Newtonsoft.Json;
using SIL.IO;
using SIL.Linq;
using SIL.PlatformUtilities;
using SIL.Reporting;

namespace Bloom.web.controllers
{

	public class CollectionApi
	{
		private readonly CollectionSettings _settings;
		private readonly LibraryModel _libraryModel;
		public const string kApiUrlPart = "collections/";
		private readonly BookSelection _bookSelection;
		// public so that WorkspaceView can set it in constructor.
		// We'd prefer to just let the WorkspaceView be a constructor arg passed to this by Autofac,
		// but that throws an exception, probably there is some circularity.
		public WorkspaceView WorkspaceView;
		public 	 CollectionApi(CollectionSettings settings, LibraryModel libraryModel, BookSelection bookSelection)
		{
			_settings = settings;
			_libraryModel = libraryModel;
			_bookSelection = bookSelection;
		}

		public void RegisterWithApiHandler(BloomApiHandler apiHandler)
		{
			apiHandler.RegisterEndpointHandler(kApiUrlPart + "list", HandleListRequest, true);

			apiHandler.RegisterEndpointHandler(kApiUrlPart + "name", request =>
			{
				// always null? request.ReplyWithText(_collection.Name);
				request.ReplyWithText(_settings.CollectionName);
			}, true);

			apiHandler.RegisterEndpointHandler(kApiUrlPart + "books", HandleBooksRequest, true);
			apiHandler.RegisterEndpointHandler(kApiUrlPart + "book/thumbnail", HandleThumbnailRequest, true);

			apiHandler.RegisterEndpointHandler(kApiUrlPart + "selected-book-id", request =>
			{
				switch (request.HttpMethod)
				{
					case HttpMethods.Get:
						request.ReplyWithText("" + _libraryModel.GetSelectedBookOrNull()?.ID);
						break;
					case HttpMethods.Post:
						var book = GetBookObjectFromPost(request);
						if (book.FolderPath != _bookSelection?.CurrentSelection?.FolderPath)
						{
							_libraryModel.SelectBook(book);
						}

						request.PostSucceeded();
						break;
				}
			}, true);

			apiHandler.RegisterEndpointHandler(kApiUrlPart + "duplicateBook/", (request) =>
			{
				_libraryModel.DuplicateBook(GetBookObjectFromPost(request));
				request.PostSucceeded();
			}, true);
			apiHandler.RegisterEndpointHandler(kApiUrlPart + "deleteBook/", (request) =>
			{
				var collection = GetCollectionOfRequest(request);
				_libraryModel.DeleteBook(GetBookObjectFromPost(request), collection);
				request.PostSucceeded();
			}, true);
			apiHandler.RegisterEndpointHandler(kApiUrlPart + "collectionProps/", HandleCollectionProps, true);

			apiHandler.RegisterEndpointHandler(kApiUrlPart + "makeBloompack/", (request) =>
				{
					_libraryModel.MakeReaderTemplateBloompack();
					request.PostSucceeded();
				},
				true);

			apiHandler.RegisterEndpointHandler(kApiUrlPart + "doChecksOfAllBooks/", (request) =>
				{
					_libraryModel.DoChecksOfAllBooks();
					request.PostSucceeded();
				},
				true);

			apiHandler.RegisterEndpointHandler(kApiUrlPart + "rescueMissingImages/", (request) =>
				{
					_libraryModel.RescueMissingImages();
					request.PostSucceeded();
				},
				true);

			apiHandler.RegisterEndpointHandler(kApiUrlPart + "doUpdatesOfAllBooks/", (request) =>
				{
					_libraryModel.DoUpdatesOfAllBooks();
					request.PostSucceeded();
				},
				true);

		}

		private void HandleCollectionProps(ApiRequest request)
		{
			var collection = GetCollectionOfRequest(request);
			dynamic props = new ExpandoObject();
			props.isFactoryInstalled = collection.IsFactoryInstalled;
			props.containsDownloadedBooks = collection.ContainsDownloadedBooks;
			request.ReplyWithJson(JsonConvert.SerializeObject(props));
		}

		// List out all the collections we have loaded
		public void HandleListRequest(ApiRequest request)
		{
			dynamic output = new List<dynamic>();
			_libraryModel.GetBookCollections().ForEach(c =>
			{
				Debug.WriteLine($"collection: {c.Name}-->{c.PathToDirectory}");
				output.Add(
					new
					{
						id = c.PathToDirectory,
						name = c.Name
					});
			});
			request.ReplyWithJson(JsonConvert.SerializeObject(output));
		}
		public void HandleBooksRequest(ApiRequest request)
		{
			var collection = GetCollectionOfRequest(request);
			if (collection == null)
			{
				return; // have already called request failed at this point
			}

			// Note: the winforms version used ImproveAndRefreshBookButtons(), which may load the whole book.

			var infos = collection.GetBookInfos()
				.Select(info =>
				{
					//var book = _libraryModel.GetBookFromBookInfo(info);
					return new
						{id = info.Id, title = info.QuickTitleUserDisplay, collectionId = collection.PathToDirectory, folderName= Path.GetFileName(info.FolderPath) };
				});
			var json = DynamicJson.Serialize(infos);
			request.ReplyWithJson(json);
		}

		private BookCollection GetCollectionOfRequest(ApiRequest request)
		{
			var id = request.RequiredParam("collection-id").Trim();
			var collection = _libraryModel.GetBookCollections().Find(c => c.PathToDirectory == id);
			if (collection == null)
			{
				request.Failed($"Collection named '{id}' was not found.");
			}

			return collection;
		}

		public void HandleThumbnailRequest(ApiRequest request)
		{
			var bookInfo = GetBookInfoFromRequestParam(request);

			// TODO: This is just a hack to get something showing. It can't make new thumbnails
			string path = Path.Combine(bookInfo.FolderPath, "thumbnail.png");
			if (RobustFile.Exists(path))
				request.ReplyWithImage(path);
			else request.Failed("Thumbnail doesn't exist, and making a new thumbnail is not yet implemented.");
		}

		private BookInfo GetBookInfoFromRequestParam(ApiRequest request)
		{
			var bookId = request.RequiredParam("book-id");
			return GetCollectionOfRequest(request).GetBookInfos().FirstOrDefault(info => info.Id == bookId);
		}
		private BookInfo GetBookInfoFromPost(ApiRequest request)
		{
			var bookId = request.RequiredPostString();
			return GetCollectionOfRequest(request).GetBookInfos().FirstOrDefault(info => info.Id == bookId);
		}

		private Book.Book GetBookObjectFromPost(ApiRequest request)
		{
			var info = GetBookInfoFromPost(request);
			return _libraryModel.GetBookFromBookInfo(info);

		}
	}
}
