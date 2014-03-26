﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Gecko;
using Gecko.DOM;
using Palaso.Reporting;
using Palaso.WritingSystems;

namespace WeSay.UI.TextBoxes
{
	public partial class GeckoComboBox : UserControl, IControlThatKnowsWritingSystem
	{
		private GeckoWebBrowser _browser;
		private bool _browserIsReadyToNavigate;
		private bool _browserDocumentLoaded;
		private int _pendingInitialIndex;
		private string _pendingHtmlLoad;
		private IWritingSystemDefinition _writingSystem;
		private bool _keyPressed;
		private GeckoSelectElement _selectElement;
		private GeckoBodyElement _bodyElement;
		private EventHandler _loadHandler;
		private EventHandler<GeckoDomKeyEventArgs> _domKeyDownHandler;
		private EventHandler<GeckoDomKeyEventArgs> _domKeyUpHandler;
		private EventHandler<GeckoDomEventArgs> _domFocusHandler;
		private EventHandler<GeckoDomEventArgs> _domBlurHandler;
		private EventHandler<GeckoDomEventArgs> _domClickHandler;
		private EventHandler _domDocumentChangedHandler;
		private EventHandler _textChangedHandler;
		private readonly string _nameForLogging;
		private bool _inFocus;
		private List<Object> _items;
		private StringBuilder _itemHtml;
		public event EventHandler SelectedValueChanged;

		public GeckoComboBox()
		{
			InitializeComponent();

			if (_nameForLogging == null)
			{
				_nameForLogging = "??";
			}
			Name = _nameForLogging;
			_keyPressed = false;
			ReadOnly = false;
			_inFocus = false;
			_pendingInitialIndex = -1;
			_items = new List<object>();
			_itemHtml = new StringBuilder(); 

			var designMode = (LicenseManager.UsageMode == LicenseUsageMode.Designtime);
			if (designMode)
				return;

			Debug.WriteLine("New GeckoComboBox");
			_browser = new GeckoWebBrowser();
			_browser.Dock = DockStyle.Fill;
			_browser.Parent = this;
			_loadHandler = new EventHandler(GeckoBox_Load);
			this.Load += _loadHandler;
			Controls.Add(_browser);

			_domKeyDownHandler = new EventHandler<GeckoDomKeyEventArgs>(OnDomKeyDown);
			_browser.DomKeyDown += _domKeyDownHandler;
			_domKeyUpHandler = new EventHandler<GeckoDomKeyEventArgs>(OnDomKeyUp);
			_browser.DomKeyUp += _domKeyUpHandler;
			_domFocusHandler = new EventHandler<GeckoDomEventArgs>(_browser_DomFocus);
			_browser.DomFocus += _domFocusHandler;
			_domBlurHandler = new EventHandler<GeckoDomEventArgs>(_browser_DomBlur);
			_browser.DomBlur += _domBlurHandler;
			_domDocumentChangedHandler = new EventHandler(_browser_DomDocumentChanged);
			_browser.DocumentCompleted += _domDocumentChangedHandler;
			_domClickHandler = new EventHandler<GeckoDomEventArgs>(_browser_DomClick);
			_browser.DomClick += _domClickHandler;

			_textChangedHandler = new EventHandler(OnTextChanged);
			this.TextChanged += _textChangedHandler;
			this.ResumeLayout(false);
		}

		public GeckoComboBox(IWritingSystemDefinition ws, string nameForLogging)
			: this()
		{
			_nameForLogging = nameForLogging;
			if (_nameForLogging == null)
			{
				_nameForLogging = "??";
			}
			Name = _nameForLogging;
			WritingSystem = ws;
		}

		public void Closing()
		{
			_items.Clear();
			_itemHtml.Clear();
			this.Load -= _loadHandler;
			_browser.DomKeyDown -= _domKeyDownHandler;
			_browser.DomKeyUp -= _domKeyUpHandler;
			_browser.DomFocus -= _domFocusHandler;
			_browser.DomBlur -= _domBlurHandler;
			_browser.DocumentCompleted -= _domDocumentChangedHandler;
			_browser.DomClick -= _domClickHandler;
			this.TextChanged -= _textChangedHandler;
			_items = null;
			_loadHandler = null;
			_domKeyDownHandler = null;
			_domKeyUpHandler = null;
			_domClickHandler = null;
			_textChangedHandler = null;
			_domFocusHandler = null;
			_domDocumentChangedHandler = null;
			_browser.Stop();
			_browser.Dispose();
			_browser = null;
		}

		public void AddItem(Object item)
		{
			_items.Add(item);
			_itemHtml.AppendFormat("<option value=\"{0}\">{1}</option>", item.ToString(), item.ToString());
		}

		public Object SelectedItem
		{
			get
			{
				if (_browser.Document == null)
				{
					return null;
				}
				var content = (GeckoSelectElement)_browser.Document.GetElementById("itemList");
				if (content != null)
				{
					return (_items[content.SelectedIndex]);
				}
				return null;
			}

		}
		public int SelectedIndex
		{
			get
			{
				if (_browser.Document == null)
				{
					return -1;
				}
				var content = (GeckoSelectElement)_browser.Document.GetElementById("itemList");
				if (content != null)
				{
					return (content.SelectedIndex);
				}
				return -1;
			}
			set
			{
				if (!_browserDocumentLoaded)
				{
					_pendingInitialIndex = value;
				}
				if (_browser.Document == null)
				{
					return;
				}
				var content = (GeckoSelectElement)_browser.Document.GetElementById("itemList");
				if (content != null)
				{
					content.SelectedIndex = value;
				}
			}
		}
		public int Length
		{
			get
			{
				return _items.Count;
			}
		}

		public List<Object> Items
		{
			get
			{
				return _items;
			}
		}

		public String SelectedText
		{
			get
			{
				var content = (GeckoSelectElement)_browser.Document.GetElementById("itemList");
				if (content != null)
				{
					return (content.Value);
				}
				return null;				
			}
		}

		public void ListCompleted()
		{
			var html = new StringBuilder();
			html.Append("<!DOCTYPE html>");
			html.Append("<html><header><meta charset=\"UTF-8\">");
			html.Append("<script type='text/javascript'>");
			html.Append(" function fireEvent(name, data)");
            html.Append(" {");
            html.Append("   event = document.createEvent('MessageEvent');");
			html.Append("   event.initMessageEvent(name, false, false, data, null, null, null, null);");
			html.Append("   document.dispatchEvent(event);");
			html.Append(" }");
			html.Append("</script>");
			html.Append("</head>");
			html.AppendFormat("<body style='background:{0}' id='mainbody'>", 
				System.Drawing.ColorTranslator.ToHtml(Color.FromArgb(255,203,255,185)));
			html.Append("<select id='itemList' onchange=\"fireEvent('selectChanged','changed');\">");
			html.Append(_itemHtml);
			html.Append("</select></body></html>");
			SetHtml(html.ToString());
		}
		private void OnSelectedValueChanged(String s)
		{
			if (SelectedValueChanged != null)
			{
				SelectedValueChanged.Invoke(this, null);
			}
		}
		/// <summary>
		/// called when the client changes our Control.Text... we need to them move that into the html
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnTextChanged(object sender, EventArgs e)
		{
			SetText(Text);
		}

		private void _browser_DomDocumentChanged(object sender, EventArgs e)
		{
			_browserDocumentLoaded = true;  // Document loaded once
			if (_pendingInitialIndex > -1)
			{
				SelectedIndex = _pendingInitialIndex;
				_pendingInitialIndex = -1;
			}
			var content = _browser.Document.GetElementById("mainbody");
			if (content != null)
			{
				if (content is GeckoBodyElement)
				{
					_bodyElement = (GeckoBodyElement)content;
					Height = _bodyElement.Parent.ScrollHeight;
				}
			}
		}

		private delegate void ChangeFocusDelegate(GeckoSelectElement ctl);
		private void _browser_DomFocus(object sender, GeckoDomEventArgs e)
		{
#if DEBUG
			Debug.WriteLine("Got Focus: " + Text);
#endif
			var content = (GeckoSelectElement)_browser.Document.GetElementById("itemList");
			if (content != null)
			{
				if (!_inFocus)
				{
					_inFocus = true;
#if DEBUG
					Debug.WriteLine("Got Focus2: " + Text);
#endif
					_selectElement = (GeckoSelectElement)content;
					this.BeginInvoke(new ChangeFocusDelegate(changeFocus), _selectElement);
				}
			}
		}
		private void _browser_DomBlur(object sender, GeckoDomEventArgs e)
		{
			_inFocus = false;
#if DEBUG
			Debug.WriteLine("Got Blur: " + Text);
#endif
		}
		private void _browser_DomClick(object sender, GeckoDomEventArgs e)
		{
#if DEBUG
			Debug.WriteLine ("Got Dom Mouse Click " + Text);
#endif
			_browser.Focus ();
		}

		private void changeFocus(GeckoSelectElement ctl)
		{
#if DEBUG
			Debug.WriteLine("Change Focus: " + Text);
#endif
			ctl.Focus();
		}

		private void OnDomKeyUp(object sender, GeckoDomKeyEventArgs e)
		{
			var content = _browser.Document.GetElementById("main");
			_keyPressed = true;

			//			Debug.WriteLine(content.TextContent);
			Text = content.TextContent;
		}

		private void OnDomKeyDown(object sender, GeckoDomKeyEventArgs e)
		{
			if (_inFocus)
			{
				if (!MultiParagraph && e.KeyCode == 13) // carriage return
				{
					e.Handled = true;
				}
				else if ((e.KeyCode == 9) && !e.CtrlKey && !e.AltKey)
				{
					int a = ParentForm.Controls.Count;
#if DEBUG
					Debug.WriteLine ("Got a Tab Key " + Text + " Count " + a.ToString() );
#endif
					if (e.ShiftKey)
					{
						if (!ParentForm.SelectNextControl(this, false, true, true, true))
						{
#if DEBUG
							Debug.WriteLine("Failed to advance");
#endif
						}
					}
					else
					{
						if (!ParentForm.SelectNextControl(this, true, true, true, true))
						{
#if DEBUG
							Debug.WriteLine("Failed to advance");
#endif
						}
					}
				}
				else
				{
					this.RaiseKeyEvent(Keys.A, new KeyEventArgs(Keys.A));
				}
			}
		}

		private void GeckoBox_Load(object sender, EventArgs e)
		{
			_browserIsReadyToNavigate = true;
			_browser.AddMessageEventListener("selectChanged", ((string s) => this.OnSelectedValueChanged(s)));
			if (_pendingHtmlLoad != null)
			{
#if DEBUG
				Debug.WriteLine("Load: " + _pendingHtmlLoad);
#endif
				_browser.LoadHtml(_pendingHtmlLoad);
				_pendingHtmlLoad = null;
			}
			else
			{
#if DEBUG
				Debug.WriteLine ("Load: Empty Line");
#endif
				SetText(""); //make an empty, editable box
			}
		}

		private void SetText(string s)
		{
			String justification = "left";
			if (_writingSystem != null && WritingSystem.RightToLeftScript)
			{
				justification = "right";
			}

			String editable = "true";
			if (ReadOnly)
			{
				editable = "false";
			}
			var html =
				string.Format(
					"<html><header><meta charset=\"UTF-8\"></head><body style='background:#FFFFFF' id='mainbody'><div style='min-height:15px; font-family:{0}; font-size:{1}pt; text-align:{3}' id='main' name='textArea' contentEditable='{4}'>{2}</div></body></html>",
					WritingSystem.DefaultFontName, WritingSystem.DefaultFontSize.ToString(), s, justification, editable);
			if (!_browserIsReadyToNavigate)
			{
				_pendingHtmlLoad = html;
			}
			else
			{
				if (!_keyPressed)
				{
#if DEBUG
					Debug.WriteLine ("SetText: " + html);
#endif
					_browser.LoadHtml(html);
				}
				_keyPressed = false;
			}
		}

		public void SetHtml(string html)
		{
			if (!_browserIsReadyToNavigate)
			{
				_pendingHtmlLoad = html;
			}
			else
			{
				Debug.WriteLine("SetHTML: " + html);
				_browser.LoadHtml(html);
			}
				
		}

		public IWritingSystemDefinition WritingSystem
		{
			get
			{
				if (_writingSystem == null)
				{
					throw new InvalidOperationException(
						"Input system must be initialized prior to use.");
				}
				return _writingSystem;
			}

			set
			{
				if (value == null)
				{
					throw new ArgumentNullException();
				}
				_writingSystem = value;
			}
		}

		public bool MultiParagraph { get; set; }

		public bool IsSpellCheckingEnabled { get; set; }


		public int SelectionStart
		{
			get
			{
				//TODO
				return 0;
			}
			set
			{
				//TODO
			}
		}

		public void AssignKeyboardFromWritingSystem()
		{
			if (_writingSystem == null)
			{
				throw new InvalidOperationException(
					"Input system must be initialized prior to use.");
			}

			_writingSystem.LocalKeyboard.Activate();
		}

		public void ClearKeyboard()
		{
			if (_writingSystem == null)
			{
				throw new InvalidOperationException(
					"Input system must be initialized prior to use.");
			}

			Keyboard.Controller.ActivateDefaultKeyboard();
		}

		public bool ReadOnly { get; set; }

		public bool Multiline
		{
			get
			{
				//todo
				return false;
			}
			set
			{
				//todo
			}
		}

		public bool WordWrap
		{
			get
			{
				//todo
				return false;
			}
			set
			{
				//todo
			}
		}

		public GeckoWebBrowser Browser
		{
			get
			{
				return _browser;
			}
			set
			{
				//todo
			}
		}
		/// <summary>
		/// for automated tests
		/// </summary>
		public void PretendLostFocus()
		{
			OnLostFocus(new EventArgs());
		}

		/// <summary>
		/// for automated tests
		/// </summary>
		public void PretendSetFocus()
		{
			Debug.Assert(_browser != null, "_browser != null");
			_browser.Focus();
		}

		protected override void OnLeave(EventArgs e)
		{
			base.OnLeave(e);

			// this.BackColor = System.Drawing.Color.White;
//			ClearKeyboard();
		 }
		protected override void OnEnter(EventArgs e)
		{
			base.OnEnter(e);
//			AssignKeyboardFromWritingSystem();
		}

	}
}
