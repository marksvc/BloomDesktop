﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Bloom.Properties;
using BookInstance = Bloom.Book.Book;
using Bloom.WebLibraryIntegration;
using SIL.Windows.Forms.ClearShare;
using SIL.Windows.Forms.Progress;
using SIL.Xml;
using BloomTemp;
using System.IO;
using Bloom.Collection;
using Bloom.Book;
using System.Diagnostics;

namespace Bloom.Publish.BloomLibrary
{
	/// <summary>
	/// Puts all of the business logic of whether a book's metadata is complete enough to publish and handling some of the login
	/// process in one place so that the regular single-book Publish tab upload and the command line bulk upload can use the
	/// same verification logic.
	/// </summary>
	public class BloomLibraryPublishModel
	{
		private readonly Metadata _licenseMetadata;
		private readonly LicenseInfo _license;
		private readonly BookUpload _uploader;
		private readonly PublishModel _publishModel;

		public BloomLibraryPublishModel(BookUpload uploader, BookInstance book, PublishModel model)
		{
			Book = book;
			_uploader = uploader;
			_publishModel = model;

			_licenseMetadata = Book.GetLicenseMetadata();
			// This is usually redundant, but might not be on old books where the license was set before the new
			// editing code was written.
			Book.SetMetadata(_licenseMetadata);
			_license = _licenseMetadata.License;

			EnsureBookAndUploaderId();
		}

		internal BookInstance Book { get; }

		internal string LicenseRights => _license.RightsStatement??string.Empty;

		// ReSharper disable once InconsistentNaming
		internal string CCLicenseUrl => (_license as CreativeCommonsLicense)?.Url;

		internal string LicenseToken => _license.Token.ToUpperInvariant();

		internal string Credits => Book.BookInfo.Credits;

		internal string Title => Book.BookInfo.Title;

		internal string Copyright => Book.BookInfo.Copyright;

		internal bool HasOriginalCopyrightInfoInSourceCollection => Book.HasOriginalCopyrightInfoInSourceCollection;

		internal bool IsTemplate => Book.BookInfo.IsSuitableForMakingShells;

		internal string Summary
		{
			get { return Book.BookInfo.Summary; }
			set
			{
				Book.BookInfo.Summary = value;
				Book.BookInfo.Save();
			}
		}

		/// <summary>
		/// This is a difficult concept to implement. The current usage of this is in creating metadata indicating which languages
		/// the book contains. How are we to decide whether it contains enough of a particular language to be useful?
		/// Based on BL-2017, we now return a Dictionary of booleans indicating whether a language should be uploaded by default.
		/// The dictionary contains an entry for every language where the book contains non-x-matter text.
		/// The value is true if every non-x-matter field which contains text in any language contains text in this.
		/// </summary>
		internal Dictionary<string, bool> AllLanguages => Book.AllPublishableLanguages();

		/// <summary>
		/// Gets a user-friendly language name.
		/// </summary>
		internal string PrettyLanguageName(string code)
		{
			return Book.PrettyPrintLanguage(code);
		}

		/// <summary>
		/// Whether the most recent PDF generation succeeded.
		/// </summary>
		public bool PdfGenerationSucceeded
		{
			get { return _publishModel.PdfGenerationSucceeded; }
		}

		private void EnsureBookAndUploaderId()
		{
			if (string.IsNullOrEmpty(Book.BookInfo.Id))
			{
				Book.BookInfo.Id = Guid.NewGuid().ToString();
			}
			Book.BookInfo.Uploader = Uploader;
		}

		internal bool IsBookPublicDomain => _license?.Url != null && _license.Url.StartsWith("http://creativecommons.org/publicdomain/zero/");

		internal bool BookIsAlreadyOnServer => LoggedIn && _uploader.IsBookOnServer(Book.FolderPath);

		internal dynamic ConflictingBookInfo => _uploader.GetBookOnServer(Book.FolderPath);

		private string Uploader => _uploader.UserId;

		/// <summary>
		/// The model alone cannot determine whether a book is OK to upload, because the language requirements
		/// are different for single book upload and bulk upload (which use this same model).
		/// For bulk upload, a book is okay to upload if this property is true AND it has ANY language
		/// with complete data (meaning all non-xmatter fields have something in the language).
		/// For single book upload, a book is okay to upload if this property is true AND it is EITHER
		/// OkToUploadWithNoLanguages OR the user has checked a language checkbox.
		/// This property just determines whether the book's metadata is complete enough to publish
		/// LoggedIn is not part of this, because the two users of the model check for login status in different
		/// parts of the process.
		/// </summary>
		internal bool MetadataIsReadyToPublish =>
		    // Copyright info is not required if the book has been put in the public domain
			// Also, (BL-5563) if there is an original copyright and we're publishing from a source collection,
			// we don't need to have a copyright.
		    (IsBookPublicDomain || !string.IsNullOrWhiteSpace(Copyright) || HasOriginalCopyrightInfoInSourceCollection) &&
		    !string.IsNullOrWhiteSpace(Title);

		internal bool LoggedIn => _uploader.LoggedIn;

		/// <summary>
		/// Stored Web user Id
		/// </summary>
		///  Best not to store its own value, because the username/password can be changed if the user logs into a different account.
		internal string WebUserId { get { return Settings.Default.WebUserId; } }

		/// <summary>
		/// We would like users to be able to publish picture books that don't have any text.  Historically, we've required
		/// non-empty books with text unless the book is marked as being a template.  This restriction is too severe, so for
		/// now, we require either a template or a pure picture book.  (No text boxes apart from image description boxes on
		/// content pages.)  (See https://issues.bloomlibrary.org/youtrack/issue/BL-7514 for the initial user request, and
		/// https://issues.bloomlibrary.org/youtrack/issue/BL-7799 for why we made this property non-trivial.)
		/// </summary>
		internal bool OkToUploadWithNoLanguages => Book.BookInfo.IsSuitableForMakingShells || Book.HasOnlyPictureOnlyPages();

		internal bool IsThisVersionAllowedToUpload => _uploader.IsThisVersionAllowedToUpload();

		internal string UploadOneBook(BookInstance book, LogBox progressBox, PublishView publishView, bool excludeMusic, out string parseId)
		{
			using (var tempFolder = new TemporaryFolder(Path.Combine("BloomUpload", Path.GetFileName(book.FolderPath))))
			{
				BookUpload.PrepareBookForUpload(ref book, _publishModel.BookServer, tempFolder.FolderPath, progressBox);
				var bookParams = new BookUploadParameters
				{
					ExcludeMusic = excludeMusic,
					PreserveThumbnails = false,
				};
				return _uploader.FullUpload(book, progressBox, publishView, bookParams, out parseId);
			}
		}

		/// <summary>
		/// Try to login using stored userid and password
		/// Test LoggedIn property to verify.
		/// </summary>
		/// <returns></returns>
		internal void LogIn()
		{
			FirebaseLoginDialog.FirebaseUpdateToken();
		}

		internal void Logout()
		{
			_uploader.Logout();
		}

		internal LicenseState LicenseType
		{
			get
			{
				if (_license is CreativeCommonsLicense)
				{
					return LicenseState.CreativeCommons;
				}
				if (_license is NullLicense)
				{
					return LicenseState.Null;
				}
				return LicenseState.Custom;
			}
		}

		/// <summary>
		/// Used by bulk uploader to tell the user why we aren't uploading their book.
		/// </summary>
		/// <returns></returns>
		public string GetReasonForNotUploadingBook()
		{
			const string couldNotUpload = "Could not upload book. ";
			// It might be because we're missing required metadata.
			if (!MetadataIsReadyToPublish)
			{
				if (string.IsNullOrWhiteSpace(Title))
				{
					return couldNotUpload + "Required book Title is empty.";
				}
				if (string.IsNullOrWhiteSpace(Copyright))
				{
					return couldNotUpload + "Required book Copyright is empty.";
				}
			}
			// Or it might be because a non-template book doesn't have any 'complete' languages.
			// every non-x - matter field which contains text in any language contains text in this
			return couldNotUpload + "A non-template book needs at least one language where every non-xmatter field contains text in that language.";
		}

		public void UpdateBookMetadataFeatures(bool isBlind, bool isTalkingBook, bool isSignLanguage)
		{
			var allowedLanguages = Book.BookInfo.PublishSettings.BloomLibrary.TextLangs.IncludedLanguages()
				.Union(Book.BookInfo.PublishSettings.BloomLibrary.SignLangs.IncludedLanguages());

			Book.UpdateMetadataFeatures(
				isBlindEnabled: isBlind,
				isTalkingBookEnabled: isTalkingBook,
				isSignLanguageEnabled: isSignLanguage,
				allowedLanguages);
		}

		public void SaveTextLanguageSelection(string langCode, bool include)
		{
			Book.BookInfo.PublishSettings.BloomLibrary.TextLangs[langCode] = include ? InclusionSetting.Include : InclusionSetting.Exclude;
			Book.BookInfo.Save();   // We updated the BookInfo, so need to persist the changes. (but only the bookInfo is necessary, not the whole book)
		}

		public void SaveAudioLanguageSelection(bool include)
		{
			// Currently, audio language selection is all or nothing for Bloom Library publish
			foreach (var langCode in Book.BookInfo.PublishSettings.BloomLibrary.AudioLangs.Keys.ToList())
			{
				Book.BookInfo.PublishSettings.BloomLibrary.AudioLangs[langCode] = include ? InclusionSetting.Include : InclusionSetting.Exclude;
			}
			Book.BookInfo.Save();   // We updated the BookInfo, so need to persist the changes. (but only the bookInfo is necessary, not the whole book)
		}

		public void InitializeLanguages()
		{
			InitializeLanguages(Book);
		}

		public static void InitializeLanguages(BookInstance book)
		{
			var allLanguages = book.AllPublishableLanguages();

			var bookInfo = book.BookInfo;
			Debug.Assert(bookInfo?.MetaData != null, "Precondition: MetaData must not be null");

			if (bookInfo.PublishSettings.BloomLibrary.TextLangs == null)
			{
				bookInfo.PublishSettings.BloomLibrary.TextLangs = new Dictionary<string, InclusionSetting>();
			}

			// reinitialize our list of which languages to publish, defaulting to the ones that are complete.
			foreach (var kvp in allLanguages)
			{
				var langCode = kvp.Key;

				// First, check if the user has already explicitly set the value. If so, we'll just use that value and be done.
				if (bookInfo.PublishSettings.BloomLibrary.TextLangs.TryGetValue(langCode, out InclusionSetting checkboxValFromSettings))
				{
					if (checkboxValFromSettings.IsSpecified())
					{
						continue;
					}
				}

				// Nope, either no value exists or the value was some kind of default value.
				// Compute (or recompute) what the value should default to.
				bool isChecked = kvp.Value || IsRequiredLanguageForBook(langCode, book);

				var newInitialValue = isChecked ? InclusionSetting.IncludeByDefault : InclusionSetting.ExcludeByDefault;
				bookInfo.PublishSettings.BloomLibrary.TextLangs[langCode] = newInitialValue;
			}

			// Initialize the Talking Book Languages settings
			if (bookInfo.PublishSettings.BloomLibrary.AudioLangs == null)
			{
				bookInfo.PublishSettings.BloomLibrary.AudioLangs = new Dictionary<string, InclusionSetting>();
				var allLangCodes = allLanguages.Select(x => x.Key);
				foreach (var langCode in allLangCodes)
				{
					bookInfo.PublishSettings.BloomLibrary.AudioLangs[langCode] = InclusionSetting.IncludeByDefault;
				}
			}

			if (bookInfo.PublishSettings.BloomLibrary.SignLangs == null)
				bookInfo.PublishSettings.BloomLibrary.SignLangs = new Dictionary<string, InclusionSetting>();
			var collectionSignLangCode = book.CollectionSettings.SignLanguageIso639Code;
			// User may have unset or modified the sign language for the collection in which case we need to exclude the old one it if it was previously included.
			foreach (var includedSignLangCode in bookInfo.PublishSettings.BloomLibrary.SignLangs.IncludedLanguages().ToList())
			{
				if (includedSignLangCode != collectionSignLangCode)
				{
					bookInfo.PublishSettings.BloomLibrary.SignLangs[includedSignLangCode] = InclusionSetting.ExcludeByDefault;
				}
			}
			// Include the collection sign language by default unless the user excluded it.
			if (!string.IsNullOrEmpty(collectionSignLangCode))
			{
				if (!bookInfo.PublishSettings.BloomLibrary.SignLangs.ContainsKey(collectionSignLangCode) ||
					bookInfo.PublishSettings.BloomLibrary.SignLangs[collectionSignLangCode] != InclusionSetting.Exclude)
				{
					bookInfo.PublishSettings.BloomLibrary.SignLangs[collectionSignLangCode] = InclusionSetting.IncludeByDefault;
				}
			}

			// The metadata may have been changed, so save it.
			bookInfo.Save();
		}

		public static bool IsRequiredLanguageForBook(string langCode, Book.Book book)
		{
			// Languages which have been selected for display in this book need to be selected
			return
				langCode == book.BookData.Language1.Iso639Code ||
				langCode == book.Language2IsoCode ||
				langCode == book.Language3IsoCode;
		}

		public void ClearSignLanguageToPublish()
		{
			foreach (var includedSignLangCode in Book.BookInfo.PublishSettings.BloomLibrary.SignLangs.IncludedLanguages().ToList()) {
				Book.BookInfo.PublishSettings.BloomLibrary.SignLangs[includedSignLangCode] = InclusionSetting.Exclude;
			}
			Book.BookInfo.Save();
		}

		public void SetOnlySignLanguageToPublish(string langCode)
		{
			Book.BookInfo.PublishSettings.BloomLibrary.SignLangs = new Dictionary<string, InclusionSetting>
			{
				[langCode] = InclusionSetting.Include
			};
			Book.BookInfo.Save();
		}

		public bool IsPublishSignLanguage()
		{
			return Book.BookInfo.PublishSettings.BloomLibrary.SignLangs.IncludedLanguages().Any();
		}

		public void ClearBlindAccessibleToPublish()
		{
			Book.UpdateBlindFeature(false, null);
			Book.BookInfo.Save();
		}

		public void SetOnlyBlindAccessibleToPublish(string langCode)
		{
			Book.UpdateBlindFeature(true, new List<string> { langCode });
			Book.BookInfo.Save();
		}
	}

	internal enum LicenseState
	{
		Null,
		CreativeCommons,
		Custom
	}
}
