﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml;
using Palaso.IO;
using Palaso.Xml;
using Skybound.Gecko;
using Skybound.Gecko.DOM;
using TempFile = BloomTemp.TempFile;

namespace Bloom
{

	public partial class Browser : UserControl
	{
		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool SetDllDirectory(string lpPathName);


		protected GeckoWebBrowser _browser;
		bool _browserIsReadyToNavigate;
		private string _url;
		private XmlDocument _pageDom;
		private TempFile _tempHtmlFile;
		private PasteCommand _pasteCommand;
		private CopyCommand _copyCommand;
	  private  UndoCommand _undoCommand;
		private  CutCommand _cutCommand;
		public event EventHandler OnBrowserClick;



		public static void SetUpXulRunner()
		{
			string xulRunnerPath = Path.Combine(FileLocator.DirectoryOfApplicationOrSolution, "xulrunner");
			if (!Directory.Exists(xulRunnerPath))
			{

				//if this is a programmer, go look in the lib directory
				xulRunnerPath = Path.Combine(FileLocator.DirectoryOfApplicationOrSolution,
											 Path.Combine("lib", "xulrunner"));

				//on my build machine, I really like to have the dir labelled with the version.
				//but it's a hassle to update all the other parts (installer, build machine) with this number,
				//so we only use it if we don't find the unnumbered alternative.
				if(!Directory.Exists(xulRunnerPath))
					xulRunnerPath = Path.Combine(FileLocator.DirectoryOfApplicationOrSolution,
												 Path.Combine("lib", "xulrunner8"));

				//NB: WHEN CHANGING VERSIONS, ALSO CHANGE IN THESE LOCATIONS:
				// get the new xulrunner, zipped (as it comes from mozilla), onto c:\builddownloads on the palaso teamcity build machine
				//	build/build.win.proj: change the zip file to match the new name


			}
			//Review: and early tester found that wrong xpcom was being loaded. The following solution is from http://www.geckofx.org/viewtopic.php?id=74&action=new
			SetDllDirectory(xulRunnerPath);

			Skybound.Gecko.Xpcom.Initialize(xulRunnerPath);
		}

		public Browser()
		{
			InitializeComponent();
		}

		public void SetEditingCommands( CutCommand cutCommand, CopyCommand copyCommand, PasteCommand pasteCommand, UndoCommand undoCommand)
		{
			_cutCommand = cutCommand;
			_copyCommand = copyCommand;
			_pasteCommand = pasteCommand;
			_undoCommand = undoCommand;

			_cutCommand.Implementer = () => _browser.CutSelection();
			_copyCommand.Implementer = () => _browser.CopySelection();
			_pasteCommand.Implementer = () => _browser.Paste();
			_undoCommand.Implementer = () => _browser.Undo();

			//none of these worked
/*            _browser.DomKeyPress+=new GeckoDomKeyEventHandler((sender, args) => UpdateEditButtons());
			_browser.DomClick += new GeckoDomEventHandler((sender, args) => UpdateEditButtons());
			_browser.DomFocus += new GeckoDomEventHandler((sender, args) => UpdateEditButtons());
  */      }

		public void SaveHTML(string path)
		{
			_browser.SaveDocument(path, "text/html");
		}

		private void UpdateEditButtons()
		{
			if (_copyCommand == null)
				return;

			_cutCommand.Enabled = _browser != null && _browser.CanCutSelection;
			_copyCommand.Enabled = _browser != null && _browser.CanCopySelection;
			_pasteCommand.Enabled = _browser != null && _browser.CanPaste;
			_undoCommand.Enabled = _browser != null && _browser.CanUndo;
		}

		void OnValidating(object sender, CancelEventArgs e)
		{
			UpdateDomWithNewEditsCopiedOver();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (_tempHtmlFile != null)
			{
				_tempHtmlFile.Dispose();
				_tempHtmlFile = null;
			}
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}
		public GeckoWebBrowser WebBrowser { get { return _browser; } }

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if(DesignMode)
			{
				this.BackColor=Color.DarkGray;
				return;
			}

			_browser = new GeckoWebBrowser();



			_browser.Parent = this;
			_browser.Dock = DockStyle.Fill;
			Controls.Add(_browser);
			_browser.Navigating += new GeckoNavigatingEventHandler(_browser_Navigating);
		   // NB: registering for domclicks seems to stop normal hyperlinking (which we don't
			//necessarily need).  When I comment this out, I get an error if the href had, for example,
			//"bloom" for the protocol.  We could probably install that as a protocol, rather than
			//using the click to just get a target and go from there, if we wanted.
			_browser.DomClick += new GeckoDomEventHandler(OnBrowser_DomClick);

			_browserIsReadyToNavigate = true;

			UpdateDisplay();
			_browser.Validating += new CancelEventHandler(OnValidating);
			_browser.Navigated += CleanupAfterNavigation;//there's also a "document completed"
			_browser.DocumentCompleted += new EventHandler(_browser_DocumentCompleted);

			_updateCommandsTimer.Enabled = true;//hack
			RaiseGeckoReady();
	   }

		void _browser_DocumentCompleted(object sender, EventArgs e)
		{

		}

		void OnBrowser_DomClick(object sender, GeckoDomEventArgs e)
		{
			EventHandler handler = OnBrowserClick;
			if (handler != null)
				handler(this, e);
		}


		void _browser_Navigating(object sender, GeckoNavigatingEventArgs e)
		{
			Debug.WriteLine("Navigating " + e.Uri);
		}

		private void CleanupAfterNavigation(object sender, GeckoNavigatedEventArgs e)
		{
		   //NO. We want to leave it around for debugging purposes. It will be deleted when the next page comes along, or when this class is disposed of
//    		if(_tempHtmlFile!=null)
//    		{
//				_tempHtmlFile.Dispose();
//    			_tempHtmlFile = null;
//    		}
			//didn't seem to do anything:  _browser.WebBrowserFocus.SetFocusAtFirstElement();
		}

		public void Navigate(string url, bool cleanupFileAfterNavigating)
		{
			_url=url; //TODO: fix up this hack. We found that deleting the pdf while we're still showing it is a bad idea.
			if(cleanupFileAfterNavigating && !_url.EndsWith(".pdf"))
			{
				SetNewTempFile(TempFile.TrackExisting(url));
			}
			UpdateDisplay();
		}

		//NB: make sure the <base> is set correctly, 'cause you don't know where this method will
		//save the file before navigating to it.
		public void Navigate(XmlDocument dom)
		{
			//application/xhtml+xml
			_pageDom = dom;
			//now done in InitScript.js AddJavaScriptForEditing(_pageDom);
			XmlHtmlConverter.MakeXmlishTagsSafeForInterpretationAsHtml(dom);
			SetNewTempFile(TempFile.CreateHtm5FromXml(dom));
			_url = _tempHtmlFile.Path;
			UpdateDisplay();
		}

		private void SetNewTempFile(TempFile tempFile)
		{
			if(_tempHtmlFile!=null)
			{
				_tempHtmlFile.Dispose();
			}
			_tempHtmlFile = tempFile;
		}



		private void UpdateDisplay()
		{
			if (!_browserIsReadyToNavigate)
				return;

			if (_url!=null)
			{
				_browser.Visible = true;
				_browser.Navigate(_url);
			}
		}



		/// <summary>
		/// What's going on here: the browser is just /editting displaying a copy of one page of the document.
		/// So we need to copy any changes back to the real DOM.
		/// </summary>
		private void UpdateDomWithNewEditsCopiedOver()
		{
			if (_pageDom == null)
				return;

			//this is to force an onblur so that we can get at the actual user-edited value
			_browser.WebBrowserFocus.Deactivate();

			var body = _browser.Document.GetElementsByTagName("body");
			if (body.Count ==0)	//review: this does happen... onValidating comes along, but there is no body. Assuming it is a timing issue.
				return;

			var content = body[0].InnerHtml;
			XmlDocument dom;

			//todo: deal with exception that can come out of this
			try
			{
				dom = XmlHtmlConverter.GetXmlDomFromHtml(content);
				var bodyDom = dom.SelectSingleNode("//body");

				if (_pageDom == null)
					return;

				//track a regression
				var destinationDomPage = _pageDom.SelectSingleNode("//body/div[contains(@class,'-bloom-page')]");
				if (destinationDomPage == null)
					return;
				var expectedPageId = destinationDomPage["id"];

				var browserPageId = bodyDom.SelectSingleNode("//body/div[contains(@class,'-bloom-page')]");
				if (browserPageId == null)
					return;//why? but I've seen it happen

				var thisPageId = browserPageId["id"];
				if(expectedPageId != thisPageId)
				{
					Palaso.Reporting.ErrorReport.NotifyUserOfProblem("Bloom encountered an error saving that page (unexpected page id)");
					return;
				}
				_pageDom.GetElementsByTagName("body")[0].InnerXml = bodyDom.InnerXml;

				//enchance: would be better to do this in the InitScripts.js, and call a function in there.
				foreach (XmlElement j in _pageDom.SafeSelectNodes("//div[contains(@class, 'ui-tooltip')]"))
				{
					j.ParentNode.RemoveChild(j);
				}
			}
			catch(Exception e)
			{
				Palaso.Reporting.ErrorReport.NotifyUserOfProblem(e, "Sorry, Bloom choked on something on this page (invalid incoming html).\r\n\r\n+{0}", e);
				return;
			}



			try
			{
				XmlHtmlConverter.ThrowIfHtmlHasErrors(_pageDom.OuterXml);
			}
			catch (Exception e)
			{
				var exceptionWithHtmlContents = new Exception(content);
				Palaso.Reporting.ErrorReport.NotifyUserOfProblem(e, "Sorry, Bloom choked on something on this page (validating page).\r\n\r\n+{0}", e.Message);
			}

		}

		/// <summary>
		/// When editting using a browser (at least, gecko), we can't actually
		/// just grab the new value of, say, a textarea.  Gecko will always return the
		/// original value to us, even after being editted.
		/// But from *within* the browser, javascript can get at the new values.
		/// So here, we inject some javascript which
		/// copies the editted values back into the dom.
		/// </summary>
		private void AddJavaScriptForEditing(XmlDocument dom)
		{
			//ref: http://dev-answers.blogspot.com/2007/08/firefox-does-not-reflect-input-form.html
			foreach (XmlElement node in dom.SafeSelectNodes("//input"))
			{
				node.SetAttribute("onblur", "", "this.setAttribute('Value',this.value);");
			}
			foreach (XmlElement node in dom.SafeSelectNodes("//textarea"))
			{
				node.SetAttribute("onblur", "","this.innerHTML = this.value;");
			}
		}



		private void OnUpdateDisplayTick(object sender, EventArgs e)
		{
			UpdateEditButtons();
		}

		/// <summary>
		/// This is needed if we want to save before getting a natural Validating event.
		/// </summary>
		public void ReadEditableAreasNow()
		{
			UpdateDomWithNewEditsCopiedOver();
		}

		public void Copy()
		{
			_browser.CopySelection();
		}

		/// <summary>
		/// add a jscript source file
		/// </summary>
		/// <param name="filename"></param>
		public void AddScriptSource(string filename)
		{
			if (!File.Exists(Path.Combine(Path.GetDirectoryName(_url), filename)))
				throw new FileNotFoundException(filename);

			GeckoDocument doc = WebBrowser.Document;
			var head = doc.GetElementsByTagName("head").First();
			GeckoScriptElement script = doc.CreateElement("script") as GeckoScriptElement;
			script.Type = "text/javascript";
			script.Src = filename;
			head.AppendChild(script);
		}

		public void AddScriptContent(string content)
		{
			GeckoDocument doc = WebBrowser.Document;
			var head = doc.GetElementsByTagName("head").First();
			GeckoScriptElement script = doc.CreateElement("script") as GeckoScriptElement;
			script.Type = "text/javascript";
			script.Text = content;
			head.AppendChild(script);
		}

		public void RunJavaScript(string script)
		{
			WebBrowser.Navigate("javascript:void("+script+")");
			Application.DoEvents(); //review... is there a better way?  it seems that NavigationFinished isn't raised.
		}


		/* snippets
		 *
		 * //           _browser.WebBrowser.Navigate("javascript:void(document.getElementById('output').innerHTML = 'test')");
//            _browser.WebBrowser.Navigate("javascript:void(alert($.fn.jquery))");
//            _browser.WebBrowser.Navigate("javascript:void(alert($(':input').serialize()))");
			//_browser.WebBrowser.Navigate("javascript:void(document.getElementById('output').innerHTML = form2js('form','.',false,null))");
			//_browser.WebBrowser.Navigate("javascript:void(alert($(\"form\").serialize()))");

			*/
		public event EventHandler GeckoReady;

		public void RaiseGeckoReady()
		{
			EventHandler handler = GeckoReady;
			if (handler != null) handler(this, null);
		}
	}
}
