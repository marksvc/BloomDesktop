﻿using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using Bloom.Api;
using Bloom.Collection;
using Bloom.Registration;
using Bloom.Utils;
using L10NSharp;
using SIL.IO;
using SIL.Reporting;

namespace Bloom.TeamCollection
{
	public interface ITeamCollectionManager
	{
		void RaiseBookStatusChanged(BookStatusChangeEventArgs eventInfo);
	}

	/// <summary>
	/// This class, created by autofac as part of the project context, handles determining
	/// whether the current collection has an associated TeamCollection, and if so, creating it.
	/// Autofac classes needing access to the TeamCollection (if any) should be constructed
	/// with an instance of this.
	/// </summary>
	public class TeamCollectionManager: IDisposable, ITeamCollectionManager
	{
		private readonly BloomWebSocketServer _webSocketServer;
		private readonly BookStatusChangeEvent _bookStatusChangeEvent;
		public TeamCollection CurrentCollection { get; private set; }
		// Normally the same as CurrentCollection, but CurrentCollection is only
		// non-null when we have a fully functional Team Collection operating.
		// Sometimes a TC may be disconnected, that is, we know this is a TC,
		// but we can't currently do TC operations, for example, because we don't
		// find the folder where the repo lives, or it's a dropbox folder but
		// Dropbox is not running or we can't ping dropbox.com.
		// A collection we know is a TC may also be disabled because there is no
		// enterprise subscription. Another possibility is that we can't do TC
		// operations because the user has not registered; I've been calling this
		// disabled also, but it's not just that we choose not to allow it; we
		// actually need the missing information to make things work.
		// In all these situations, most TC operations simply don't happen because
		// CurrentCollection is null, but there are a few operations that still need
		// to be aware of the TC (for example, we still don't allow editing books
		// that are in the Repo and not checked out, and still show the TC status icon)
		// and it is easiest to achieve this by having a (Disconnected)TC object.
		// This property allows us to find the TC whether or not it is disconnected.
		// I can't find a good word that covers both disconnected and disabled,
		// so in places where it is ambiguous I'm just using disconnected.
		public TeamCollection CurrentCollectionEvenIfDisconnected { get; private set; }

		/// <summary>
		/// Raised when the status of the whole collection (this.TeamCollectionStatus) might have changed.
		/// (That is, when a new message or milestone arrives...currently we don't ensure that the status
		/// actually IS different from before.)
		/// </summary>
		public static event EventHandler TeamCollectionStatusChanged;
		private readonly string _localCollectionFolder;
		private static string _overrideCurrentUser;
		private static string _overrideCurrentUserFirstName;
		private static string _overrideCurrentUserSurname;
		private static string _overrideMachineName;

		/// <summary>
		/// Force the startup sync of collection files to be FROM the repo TO local.
		/// </summary>
		public static bool ForceNextSyncToLocal { set; get; }

		internal static void ForceCurrentUserForTests(string user)
		{
			_overrideCurrentUser = user;
		}

		public static void RaiseTeamCollectionStatusChanged()
		{
			TeamCollectionStatusChanged?.Invoke(null, new EventArgs());
		}

		/// <summary>
		/// Return true if the user must check this book out before editing it,
		/// deleting it, etc. This is automatically false if the collection is not
		/// a TC; if it is a TC (even a disconnected one), it's true if the book is
		/// NOT checked out.
		/// </summary>
		/// <param name="bookFolderPath"></param>
		/// <returns></returns>
		public bool NeedCheckoutToEdit(string bookFolderPath)
		{
			if (CurrentCollectionEvenIfDisconnected == null)
				return false;
			return CurrentCollectionEvenIfDisconnected.NeedCheckoutToEdit(bookFolderPath);
		}

		/// <summary>
		/// This is an additional check on delete AFTER we make sure the book is checked out.
		/// Even if it is, we can't delete it while disconnected because we don't have a way
		/// to actually remove it from the TC. Our current Delete mechanism, unlike git etc.,
		/// does not postpone delete until commit.
		/// </summary>
		/// <param name="bookFolderPath"></param>
		/// <returns></returns>
		public bool CannotDeleteBecauseDisconnected(string bookFolderPath)
		{
			if (CurrentCollectionEvenIfDisconnected == null)
				return false;
			return CurrentCollectionEvenIfDisconnected.CannotDeleteBecauseDisconnected(bookFolderPath);
		}

		public TeamCollectionStatus CollectionStatus
		{
			get
			{
				if (CurrentCollectionEvenIfDisconnected != null)
				{
					return CurrentCollectionEvenIfDisconnected.CollectionStatus;
				}

				return TeamCollectionStatus.None;
			}
		}

		public TeamCollectionMessageLog MessageLog
		{
			get
			{
				if (CurrentCollectionEvenIfDisconnected != null)
					return CurrentCollectionEvenIfDisconnected.MessageLog;
				return null;
			}
		}

		public TeamCollectionManager(string localCollectionPath, BloomWebSocketServer webSocketServer, BookRenamedEvent bookRenamedEvent, BookStatusChangeEvent bookStatusChangeEvent)
		{
			_webSocketServer = webSocketServer;
			_bookStatusChangeEvent = bookStatusChangeEvent;
			_localCollectionFolder = Path.GetDirectoryName(localCollectionPath);
			bookRenamedEvent.Subscribe(pair =>
			{
				CurrentCollectionEvenIfDisconnected?.HandleBookRename(Path.GetFileName(pair.Key), Path.GetFileName(pair.Value));
			});
			var impersonatePath = Path.Combine(_localCollectionFolder, "impersonate.txt");
			if (RobustFile.Exists(impersonatePath))
			{
				var lines = RobustFile.ReadAllLines(impersonatePath);
				_overrideCurrentUser = lines.FirstOrDefault();
				if (lines.Length > 1)
					_overrideMachineName = lines[1];
				if (lines.Length > 2)
					_overrideCurrentUserFirstName = lines[2];
				if (lines.Length > 3)
					_overrideCurrentUserSurname = lines[3];
			}

			var localSettingsPath = Path.Combine(_localCollectionFolder, TeamCollectionSettingsFileName);
			if (RobustFile.Exists(localSettingsPath))
			{
				try
				{
					var doc = new XmlDocument();
					doc.Load(localSettingsPath);
					var repoFolderPath = doc.DocumentElement.GetElementsByTagName("TeamCollectionFolder").Cast<XmlElement>()
						.First().InnerText;
					if (Directory.Exists(repoFolderPath))
					{
						if (DropboxUtils.IsPathInDropboxFolder(repoFolderPath))
						{
							if (!DropboxUtils.IsDropboxProcessRunning)
							{
								MakeDisconnected(repoFolderPath, "TeamCollection.NeedDropboxRunning",
									"Dropbox does not appear to be running.",
									null,null);
								return;
							}

							if (!DropboxUtils.CanAccessDropbox())
							{
								MakeDisconnected(repoFolderPath, "TeamCollection.NeedDropboxAccess",
									"Bloom cannot reach Dropbox.com.",
									null, null);
								return;
							}
						}
						CurrentCollection = new FolderTeamCollection(this, _localCollectionFolder, repoFolderPath);
						CurrentCollectionEvenIfDisconnected = CurrentCollection;
						CurrentCollection.SocketServer = SocketServer;
						// Later, we will sync everything else, but we want the current collection settings before
						// we create the CollectionSettings object.
						if (ForceNextSyncToLocal)
						{
							ForceNextSyncToLocal = false;
							CurrentCollection.CopyRepoCollectionFilesToLocal(_localCollectionFolder);
						}
						else
						{
							CurrentCollection.SyncLocalAndRepoCollectionFiles();
						}
					}
					else
					{
						MakeDisconnected( repoFolderPath, "TeamCollection.MissingRepo",
							"Bloom could not find the Team Collection folder at '{0}'. If that drive or network is disconnected, re-connect it. If you have moved where that folder is located, 1) quit Bloom 2) go to the Team Collection folder and double-click “Join this Team Collection”.", repoFolderPath, Environment.NewLine);
					}
				}
				catch (Exception ex)
				{
					NonFatalProblem.Report(ModalIf.All, PassiveIf.All, "Bloom found Team Collection settings but could not process them", null, ex, true);
					CurrentCollection = null;
					CurrentCollectionEvenIfDisconnected = null;
				}
			}
		}

		public void MakeDisconnected(string repoFolderPath, string messageId, string message, string param0, string param1)
		{
			CurrentCollection = null;
			// This will show the TC icon in error state, and if the dialog is shown it will have this one message.
			CurrentCollectionEvenIfDisconnected = new DisconnectedTeamCollection(this, _localCollectionFolder, repoFolderPath);
			CurrentCollectionEvenIfDisconnected.SocketServer = SocketServer;
			CurrentCollectionEvenIfDisconnected.MessageLog.WriteMessage(MessageAndMilestoneType.Error, messageId, message,
				param0, param1);
			CurrentCollectionEvenIfDisconnected.MessageLog.WriteMessage(MessageAndMilestoneType.Error, "TeamCollection.OperatingDisconnected", "When you have resolved this problem, please click \"Reload Collection\". Until then, your Team Collection will operate in \"Disconnected\" mode.",
				param0, param1);
			// This is normally ensured by pushing an Error message into the log. But in this case,
			// before the user gets a chance to open the dialog, we will run SyncAtStartup, push a Reloaded
			// milestone into the log, and thus suppress it. If we're disconnected, whatever gets in the
			// message log, we want to offer Reload...after all, the message says to use it.
			MessageLog.NextTeamCollectionDialogShouldForceReloadButton = true;
		}

		public static string GetTcLogPathFromLcPath(string localCollectionFolder)
		{
			return Path.Combine(localCollectionFolder, "log.txt");
		}

		/// <summary>
		/// This gets set when we join a new TeamCollection so that the merge we do
		/// later as we open it gets the special behavior for this case.
		/// </summary>
		public static bool NextMergeIsJoinCollection { get; set; }

		public BloomWebSocketServer SocketServer => _webSocketServer;

		public void ConnectToTeamCollection(string repoFolderParentPath, string collectionId)
		{
			var repoFolderPath = PlannedRepoFolderPath(repoFolderParentPath);
			Directory.CreateDirectory(repoFolderPath);
			var newTc = new FolderTeamCollection(this, _localCollectionFolder, repoFolderPath);
			newTc.CollectionId = collectionId;
			newTc.SocketServer = SocketServer;
			newTc.SetupTeamCollectionWithProgressDialog(repoFolderPath);
			CurrentCollection = newTc;
			CurrentCollectionEvenIfDisconnected = newTc;
		}

		public string PlannedRepoFolderPath(string repoFolderParentPath)
		{
			return Path.Combine(repoFolderParentPath, Path.GetFileName(_localCollectionFolder)+ " - TC");
		}

		public const string TeamCollectionSettingsFileName = "TeamCollectionSettings.xml";

		// This is the value the book must be locked to for a local checkout.
		// For all the Team Collection code, this should be the one place we know how to find that user.
		public static string CurrentUser => _overrideCurrentUser ?? SIL.Windows.Forms.Registration.Registration.Default.Email;

		// CurrentUser is the email address and is used as the key, but this is
		// used to display a more friendly name and avatar initials.
		// For all the Team Collection code, this should be the one place we know how to find the current user's first name.
		public static string CurrentUserFirstName => _overrideCurrentUserFirstName ?? SIL.Windows.Forms.Registration.Registration.Default.FirstName;

		// CurrentUser is the email address and is used as the key, but this is
		// used to display a more friendly name and avatar initials.
		// For all the Team Collection code, this should be the one place we know how to find the current user's surname.
		public static string CurrentUserSurname => _overrideCurrentUserSurname ?? SIL.Windows.Forms.Registration.Registration.Default.Surname;

		/// <summary>
		/// This is what the BookStatus.lockedWhere must be for a book to be considered
		/// checked out locally. For all sharing code, this should be the one place to get this.
		/// </summary>
		public static string CurrentMachine => _overrideMachineName ?? Environment.MachineName;

		public void Dispose()
		{
			CurrentCollection?.Dispose();
		}

		public void RaiseBookStatusChanged(BookStatusChangeEventArgs eventInfo)
		{
			_bookStatusChangeEvent.Raise(eventInfo);
		}

		/// <summary>
		/// Disable most TC functionality under various conditions. Put a warning in
		/// the log.
		/// </summary>
		public void CheckDisablingTeamCollections(CollectionSettings settings)
		{
			if (CurrentCollection == null)
				return; // already disabled, or not a TC
			string msg = null;
			string l10nId = null;
			if (!settings.HaveEnterpriseFeatures)
			{
				l10nId = "TeamCollection.DisabledForEnterprise";
				msg = "Bloom Enterprise is not enabled.";
			}

			if (!IsRegistrationSufficient())
			{
				l10nId = "TeamCollection.DisabledForRegistration";
				msg = "You have not registered Bloom with at least an email address to identify who is making changes.";
			}

			if (msg != null)
			{
				MakeDisconnected(CurrentCollection.RepoDescription, l10nId, msg,
					null, null);
			}
		}

		/// <summary>
		/// Returns true if registration is sufficient to use Team Collections; false otherwise
		/// </summary>
		public static bool IsRegistrationSufficient()
		{
			// We're normally checking SIL.Windows.Forms.Registration.Registration.Default.Email,
			// but getting it via TCM.CurrentUser allows overriding for testing.
			return !String.IsNullOrWhiteSpace(CurrentUser);
		}

		public void SetCollectionId(string collectionSettingsCollectionId)
		{
			if (CurrentCollectionEvenIfDisconnected != null)
				CurrentCollectionEvenIfDisconnected.CollectionId = collectionSettingsCollectionId;
		}
	}
}