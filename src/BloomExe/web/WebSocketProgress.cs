﻿using System;
using System.Diagnostics;
using Bloom.Api;
using L10NSharp;

namespace Bloom.web
{
	/// <summary>
	/// Sends localized messages to a websocket, intended for html display
	/// </summary>
	public class WebSocketProgress
	{
		private readonly IBloomWebSocketServer _bloomWebSocketServer;
		private readonly string _clientContext;
		private string _l10IdPrefix;

		private string GetL10nId(string idSuffix, bool useL10nIdPrefix)
		{
			if (useL10nIdPrefix)
				return _l10IdPrefix + idSuffix;
			else
				return idSuffix;
		}

		/// <summary>
		/// Get a new WebSocketProgress that will prefix each localization id with the given string
		/// </summary>
		/// <param name="localizationIdPrefix"></param>
		public virtual WebSocketProgress WithL10NPrefix(string localizationIdPrefix)
		{
			return new WebSocketProgress(_bloomWebSocketServer, _clientContext)
			{
				_l10IdPrefix = localizationIdPrefix
			};
		}

		protected WebSocketProgress()
		{
		}

		/// <summary>
		/// Get an object that you can use to send messages to a given clientContext
		/// </summary>
		/// <param name="bloomWebSocketServer"></param>
		/// <param name="clientContext">This goes out with our messages and, on the client side (typescript), messages are filtered
		/// down to the context (usualy a screen) that requested them.</param>
		public WebSocketProgress(IBloomWebSocketServer bloomWebSocketServer, string clientContext)
		{
			_bloomWebSocketServer = bloomWebSocketServer;
			_clientContext = clientContext;
		}

		public virtual void ErrorWithoutLocalizing(string message, params object[] args)
		{
			MessageWithoutLocalizing($"<span style='color:red'>{message}</span>", args);
			SIL.Reporting.Logger.WriteEvent($"Error: {message}", args);
		}
		public void Error(string idSuffix, string message, bool useL10nIdPrefix=true)
		{
			var localizedMessage = LocalizationManager.GetDynamicString(appId: "Bloom", id: GetL10nId(idSuffix, useL10nIdPrefix), englishText: message);
			ErrorWithoutLocalizing(localizedMessage);
			if (localizedMessage != message)
				SIL.Reporting.Logger.WriteEvent($"Error: {message}");	// repeat message in the log unlocalized.
		}

		public virtual void MessageWithoutLocalizing(string message, params object[] args)
		{
			_bloomWebSocketServer.SendString(_clientContext, "progress", String.Format(message, args));
		}

		public virtual void MessageWithStyleWithoutLocalizing(string message, string cssStyleRules)
		{
			dynamic messageBundle = new DynamicJson();
			messageBundle.message = message;
			messageBundle.cssStyleRule = cssStyleRules;
			_bloomWebSocketServer.SendBundle(_clientContext, "progress", messageBundle);
		}

		public virtual void Message(string idSuffix, string comment, string message, bool useL10nIdPrefix=true)
		{
			MessageWithoutLocalizing(LocalizationManager.GetDynamicString(appId: "Bloom", id: GetL10nId(idSuffix, useL10nIdPrefix), englishText: message, comment: comment));
		}
		public void Message(string idSuffix, string message, bool useL10nIdPrefix = true)
		{
			MessageWithoutLocalizing(LocalizationManager.GetDynamicString(appId: "Bloom", id: GetL10nId(idSuffix, useL10nIdPrefix), englishText: message));
		}

		// Use with care: if the first parameter is a string, you can leave out one of the earlier arguments with no compiler warning.
		public virtual void MessageWithParams(string idSuffix, string comment, string message, params object[] parameters)
		{
			var formatted = GetMessageWithParams(idSuffix, comment, message, parameters);
			MessageWithoutLocalizing(formatted);
		}

		// Use with care: if the first parameter is a string, you can leave out one of the earlier arguments with no compiler warning.
		public virtual void ErrorWithParams(string idSuffix, string comment, string message, params object[] parameters)
		{
			var formatted = GetMessageWithParams(idSuffix, comment, message, parameters);
			ErrorWithoutLocalizing(formatted);
			var formattedEnglish = String.Format(message, parameters);
			if (formatted != formattedEnglish)
				SIL.Reporting.Logger.WriteEvent($"Error: {formattedEnglish}");	// repeat message in the log unlocalized.
		}

		// Use with care: if the first parameter is a string, you can leave out one of the earlier arguments with no compiler warning.
		public virtual void MessageWithColorAndParams(string idSuffix, string comment, string color, string message, params object[] parameters)
		{
			var formatted = GetMessageWithParams(idSuffix, comment, message, parameters);
			MessageWithoutLocalizing("<span style='color:" + color + "'>" + formatted + "</span>");
		}

		// Use with care: if the first parameter is a string, you can leave out one of the earlier arguments with no compiler warning.
		public string GetMessageWithParams(string idSuffix, string comment, string message, params object[] parameters)
		{
			Debug.Assert(message.Contains("{0}"));
			var localized = LocalizationManager.GetDynamicString(appId: "Bloom", id: GetL10nId(idSuffix, true), englishText: message,
				comment: comment);
			var formatted = String.Format(localized, parameters);
			return formatted;
		}

		public void MessageUsingTitle(string idSuffix, string message, string bookTitle, bool useL10nIdPrefix = true)
		{
			var formatted = GetTitleMessage(idSuffix, message, bookTitle, useL10nIdPrefix);
			MessageWithoutLocalizing(formatted);
		}

		public string GetTitleMessage(string idSuffix, string message, string bookTitle, bool useL10nIdPrefix = true)
		{
			Debug.Assert(message.Contains("{0}"));
			Debug.Assert(!message.Contains("{1}"));
			var localized = LocalizationManager.GetDynamicString(appId: "Bloom", id: GetL10nId(idSuffix, useL10nIdPrefix), englishText: message,
				comment: "{0} is a book title");
			var formatted = String.Format(localized, bookTitle);
			return formatted;
		}

		public void Exception(Exception exception)
		{
			ErrorWithoutLocalizing(exception.Message);
			ErrorWithoutLocalizing(exception.StackTrace);
		}
	}

	// Passing one of these where we don't need the progress report saves recipients handling nulls
	public class NullWebSocketProgress : WebSocketProgress
	{
		public override void MessageWithoutLocalizing(string message, params object[] args)
		{
		}

		public override void MessageWithStyleWithoutLocalizing(string message, string cssStyleRules)
		{
		}

		public override WebSocketProgress WithL10NPrefix(string localizationIdPrefix)
		{
			return this;
		}
	}
}
