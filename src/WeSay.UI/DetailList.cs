using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Palaso.UI.WindowsForms.i18n;
using Palaso.i18n;
using Palaso.Reporting;
using WeSay.UI.Buttons;
using WeSay.UI.TextBoxes;

namespace WeSay.UI
{
	/// <summary>
	/// This control displays a simple list of {label, control} pairs, one per row.
	/// It supports dynamically removing and inserting new rows, to support
	/// "ghost" fields
	/// </summary>
	public partial class DetailList: TableLayoutPanel, IMessageFilter, ILocalizableControl
	{
		/// <summary>
		/// Can be used to track which data item the user is currently editting, to,
		/// for example, hilight that piece in a preview control
		/// </summary>
		public event EventHandler<CurrentItemEventArgs> ChangeOfWhichItemIsInFocus = delegate { };

		private readonly int _indexOfLabel; // todo to const?
		private readonly int _indexOfWidget = 1; // todo to const?

		private bool _disposed;
		private readonly StackTrace _stackAtConstruction;

		public EventHandler MouseEnteredBounds;
		public EventHandler MouseLeftBounds;
		private bool _mouseIsInBounds;

		public DetailList()
		{
#if DEBUG
			_stackAtConstruction = new StackTrace();
#endif
			InitializeComponent();
			this.SuspendLayout();
			Application.AddMessageFilter(this);

			Name = "DetailList"; //for  debugging
			ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 0));
			ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
			ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 30));
			Dock = DockStyle.Fill;
			AutoSize = true;
			AutoSizeMode = AutoSizeMode.GrowAndShrink;

			MouseClick += OnMouseClick;

			this.ResumeLayout(false);
			//CellPaint += OnCellPaint;
			//var rand = new Random();
			//BackColor = Color.FromArgb(255, rand.Next(0, 255), rand.Next(0, 255), rand.Next(0, 255));
		}

		public float LabelColumnWidth
		{
			get { return ColumnStyles[0].Width; }
			set
			{
				SuspendLayout();
#if __MonoCS__
				if (value > LabelColumnWidth)
					ShrinkEditControlWidths((int)(value - LabelColumnWidth));
#endif
				if (value != LabelColumnWidth)
				{
					ColumnStyles[0].Width = value;
				}
				ResumeLayout();
			}

		}

#if __MonoCS__
		/// <summary>
		/// Shrink the edit control widths to make up for the increased width of the labels.
		/// </summary>
		/// <remarks>
		/// Microsoft .Net TableLayoutPanel code handles nested tables better than Mono -- this
		/// adjustment isn't needed there.  This is needed primarily when converting ghost
		/// entries to real entries in the Dictionary Browse & Edit tool.
		/// </remarks>
		void ShrinkEditControlWidths(int shrinkage)
		{
			for (int row = 0; row < RowCount; row++)
			{
				var c = GetEditControlFromRow(row);
				if (c is MultiTextControl)
				{
					if (c.Width > shrinkage)
					{
						c.Width = c.Width - shrinkage;
						// When the control resizes, its internal boxes resize automatically,
						// so we need to shrink each of them as well.
						foreach (var box in (c as MultiTextControl).TextBoxes)
						{
							if (box.Width > shrinkage)
								box.Width = box.Width - shrinkage;
						}
					}
				}
				else if (c is TextBox)
				{
					if (c.Width > shrinkage)
						c.Width = c.Width - shrinkage;
				}
			}
		}
#endif

		public int WidestLabelWidthWithMargin
		{
			get
			{
				var labels = new List<Control>();
				AppendControlsFromEachFieldRow(0, labels);
				return labels.Select(label => ((Label)label).GetPreferredSize(new Size(1000, 1000)).Width + label.Margin.Left + label.Margin.Right).Concat(new[] { 0 }).Max();
			}
		}

		private void OnCellPaint(object sender, TableLayoutCellPaintEventArgs e)
		{
			var bounds = new Rectangle(e.CellBounds.Location, new Size(e.CellBounds.Width, e.CellBounds.Height - 10));
			e.Graphics.DrawRectangle(new Pen(Color.FromArgb(BackColor.ToArgb() ^ 0x808080)), bounds);
		}

		private void OnMouseClick(object sender, MouseEventArgs e)
		{
			Select();
			_clicked = true;
		}
		public void AddDetailList(DetailList detailList, int insertAtRow)
		{
			SuspendLayout();
			if (insertAtRow >= RowCount)
			{
				RowCount = insertAtRow + 1;
			}
			SetColumnSpan(detailList, 3);
			RowStyles.Add(new RowStyle(SizeType.AutoSize));
			detailList.MouseWheel += OnChildWidget_MouseWheel;
			detailList.Margin = new Padding(0, detailList.Margin.Top, 0, detailList.Margin.Bottom);
			Controls.Add(detailList, _indexOfLabel, insertAtRow);
			OnLabelsChanged(this, new EventArgs());
			detailList.LabelsChanged += OnLabelsChanged;
			ResumeLayout(false);
		}

		protected override void OnGotFocus(EventArgs e)
		{
			base.OnGotFocus(e);
			//we don't want to have focus, ourselves
			if (RowCount >0)
				MoveInsertionPoint(0);
		}
		/// <summary>
		/// Forces scroll bar to only have vertical scroll bar and not horizontal scroll bar by
		/// allowing enough space for the scroll bar to be added in (even though it then
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPaddingChanged(EventArgs e)
		{
			VerifyNotDisposed();
			base.OnPaddingChanged(e);
			Padding = new Padding(Padding.Left,
								  Padding.Top,
								  Math.Max(Padding.Right, 20),
								  Padding.Bottom);
		}

		public Control AddWidgetRow(string label, bool isHeader, Control control)
		{
			return AddWidgetRow(label, isHeader, control, RowCount, false);
		}

		public Control FocussedImmediateChild
		{
			get
			{
				foreach (Control child in Controls)
				{
					if (child.ContainsFocus)
					{
						return child;
					}
				}
				return null;
			} //set
			//{
			//    //Keep track of the active control ourselves by storing it in a private member, note that
			//    //that we only allow the active control to be set if it is actually a child of ours.
			//    if (Contains(value))
			//        _focussedImmediateChild = value;
			//}
		}

		public void Clear()
		{
			//Debug.WriteLine("VBox " + Name + "   Clearing");

			RowCount = 0;
			RowStyles.Clear();
			while (Controls.Count > 0)
			{
				Control myControl = Controls[0];
				//  Debug.WriteLine("  VBoxClear() calling dispose on " + base.Controls[0].Name);
				if (Controls[0] is DetailList)
				{
					((DetailList)Controls[0]).LabelsChanged -= OnLabelsChanged;
					((DetailList)Controls[0]).Clear();
				}
				Controls[0].Dispose();
			}
			Controls.Clear();
			// Debug.WriteLine("VBox " + Name + "   Clearing DONE");
		}

		public event EventHandler LabelsChanged;

		private void OnLabelSizeChanged(object sender, EventArgs e)
		{
			OnLabelsChanged(this, e);
		}

		public Control AddWidgetRow(string fieldLabel,
									bool isHeader,
									Control editWidget,
									int insertAtRow,
									bool isGhostField)
		{
			RowStyles.Add(new RowStyle(SizeType.AutoSize));

			if (insertAtRow >= RowCount)
			{
				RowCount = insertAtRow + 1;
			}
			else
			{
				if (insertAtRow == -1)
				{
					insertAtRow = RowCount;
				}
				else
				{
					// move down to make space for new row
					for (int row = RowCount;row > insertAtRow;row--)
					{
						for (int col = 0;col < ColumnCount;col++)
						{
							Control c = GetControlFromPosition(col, row - 1);
							if (c != null)
							{
								SetCellPosition(c, new TableLayoutPanelCellPosition(col, row));
								c.TabIndex = row;
							}
						}
					}
				}
				RowCount++;
			}

			Label label = new Label();
			if (isHeader)
			{
				label.Font = new Font(StringCatalog.LabelFont /* label.Font*/, FontStyle.Bold);
			}
			//label.Font =StringCatalog.ModifyFontForLocalization(label.Font);
			label.Text = fieldLabel;
			label.AutoSize = true;

			int beforeHeadingPadding = (isHeader && insertAtRow != 0) ? 18 : 0;
			//        label.Top = 3 + beforeHeadingPadding;
			label.Margin = new Padding(label.Margin.Left,
									   beforeHeadingPadding,
									   label.Margin.Right,
									   label.Margin.Bottom);
			label.Anchor = AnchorStyles.Left | AnchorStyles.Top;
			if (isGhostField)
			{
				label.ForeColor = Color.Gray;
			}

			Controls.Add(label, _indexOfLabel, insertAtRow);
			OnLabelsChanged(this, new EventArgs());
			label.SizeChanged += OnLabelSizeChanged;

			// AnchorStyle overrides DockStyle and on Linux must anchor to Top.
			editWidget.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;

			editWidget.KeyDown += OnEditWidget_KeyDown;
			editWidget.MouseWheel += OnChildWidget_MouseWheel;

			Debug.Assert(GetControlFromPosition(_indexOfWidget, insertAtRow) == null);

			//test
			editWidget.TabIndex = insertAtRow;

			editWidget.Margin = new Padding(editWidget.Margin.Left,
											beforeHeadingPadding,
											editWidget.Margin.Right,
											editWidget.Margin.Bottom);

			// At this point, multitext controls were being displayed on the screen.
			// We weren't able to get around this by simply using SuspendLayout and ResumeLayout
			//
			// http://msdn.microsoft.com/msdnmag/issues/06/03/WindowsFormsPerformance/
			// tells why:
			// "If the handles are created you will notice the difference, even if you use
			// SuspendLayout and ResumeLayout calls. SuspendLayout only prevents Windows
			// Forms OnLayout from being called. It will not prevent messages about size
			// changes from being sent and processed."
			//
			// we eventually get around this by making control invisible while it lays out
			// and then making it visible again. (See EntryViewControl.cs:RefreshEntryDetail)
			Controls.Add(editWidget, _indexOfWidget, insertAtRow);
			int tabIndex = 0;
			foreach (Control control in Controls)
			{
				control.TabIndex = tabIndex++;
			}
			return editWidget;
		}

		private void OnLabelsChanged(object sender, EventArgs e)
		{
			if (LabelsChanged != null)
			{
				LabelsChanged(this, new EventArgs());
			}
		}

		private void OnChildWidget_MouseWheel(object sender, MouseEventArgs e)
		{
			OnMouseWheel(e);
		}

		private void OnEditWidget_KeyDown(object sender, KeyEventArgs e)
		{
			VerifyNotDisposed();
			OnKeyDown(e);
		}

		private bool SetFocusOnControl(Control c)
		{
			bool focusSucceeded;
			if (c is MultiTextControl)
			{
				var tb = ((MultiTextControl)c).TextBoxes[0];
				focusSucceeded = tb.Focus();
				if (tb is WeSayTextBox)
					((WeSayTextBox)tb).Select(1000, 0); //go to end
			}
			else if (c is WeSayTextBox)
			{
				focusSucceeded = c.Focus();
				((WeSayTextBox)c).Select(1000, 0); //go to end
			}
			else
			{
				focusSucceeded = c.Focus();
			}
			return focusSucceeded;
		}


		/// <summary>
		/// Used to set the focus on the first editable field, which may not be at the index of the starting row field.  This method recurses through nested DetailLists to find an editable field on which to set the focus
		/// </summary>
		/// <returns>True if an editable control was found, otherwise false</returns>
		public void MoveInsertionPoint(int row)
		{
#if (!DEBUG) // not worth crashing over
			try
			{
#endif
			if (0 > row || row >= RowCount)
			{
				throw new ArgumentOutOfRangeException("row",
													  row,
													  "row must be between 0 and Count-1 inclusive");
			}
			var c = GetEditControlFromRow(row);
			SetFocusOnControl(c);
#if (!DEBUG) // not worth crashing over
			}
			catch (Exception)
			{
			}
#endif
		}

		public void OnBinding_ChangeOfWhichItemIsInFocus(object sender, CurrentItemEventArgs e)
		{
			VerifyNotDisposed();
			ChangeOfWhichItemIsInFocus(sender, e);
		}

		public Control GetEditControlFromRow(int fieldIndex)
		{
			return GetControlFromPosition(1, fieldIndex);
		}

		/// <summary>
		/// for tests
		/// </summary>
		public Label GetLabelControlFromRow(int fieldIndex)
		{
			return GetControlFromPosition(0, fieldIndex) as Label;
		}

		/// <summary>
		/// for tests
		/// </summary>
		public DeleteButton GetDeleteButton(int fieldIndex)
		{
			return GetControlFromPosition(2, fieldIndex) as DeleteButton;
		}

		private void AppendControlsFromEachFieldRow(int columnIndex, List<Control> controls)
		{
			for (int row = 0; row < RowCount; row++)
			{
				var c = GetControlFromPosition(columnIndex, row);
				if (c != null)
					controls.Add(c);
			}
		}

		private Control GetFirstControlInRow(int row)
		{
			return GetControlFromPosition(0, row);
		}

		~DetailList()
		{
			if (!_disposed)
			{
				string trace = "Was not recorded.";
				if (_stackAtConstruction != null)
				{
					trace = _stackAtConstruction.ToString();
				}
				throw new InvalidOperationException("Disposed not explicitly called on " +
													GetType().FullName + ".  Stack at creation was " +
													trace);
			}
		}

		protected void VerifyNotDisposed()
		{
			if (_disposed)
			{
				throw new ObjectDisposedException(GetType().FullName);
			}
		}

		bool _clicked = false;
		private int _mouseOverRow = -1;

		//This message filter is used to determine wether the mouse in hovering over the DetailList or one
		//of its children. MouseLeave unfortunately fires when the mouse moves over a child control. This
		//is not behavior that we want which is why we are adding the MouseEnteredBounds and MouseLeftBounds
		//events
		public bool PreFilterMessage(ref Message m)
		{
			if (m.Msg != 0x0200) return false;
			if (_disposed) return false;
			var controlGettingMessage = FromHandle(m.HWnd);
			if (controlGettingMessage == null) return false;
			//if (!_clicked) return false;
			int x = m.LParam.ToInt32() & 0x0000FFFF;
			int y = (int)((m.LParam.ToInt32() & 0xFFFF0000) >> 16);
			var posRelativeToControl = new Point(x, y);
			var screenPos = controlGettingMessage.PointToScreen(posRelativeToControl);
			var posRelativeToThis = PointToClient(screenPos);

			//Console.WriteLine("MouseCoords: {0} {1} BoundsUpperLeft: {2}, {3}, {4}, {5}", posRelativeToThis.X, posRelativeToThis.Y, Bounds.X, Bounds.Y, Bounds.Width, Bounds.Height);

			var mouseInBounds = ClientRectangle.Contains(posRelativeToThis);
			var oldMouseRow = _mouseOverRow;
			if (_mouseIsInBounds)
			{
				var child = GetChildAtPoint(posRelativeToThis);
				// ignore the space between rows that returns a null control.
				if (child != null)
				{
					var cellPosition = GetCellPosition(child);
					_mouseOverRow = cellPosition.Row;
				}
			}
			else
			{
				_mouseOverRow = -1;
			}
			if (MouseIsInBounds != mouseInBounds || oldMouseRow != _mouseOverRow)
			{
				if (mouseInBounds)
				{
					if (MouseEnteredBounds != null)
					{
						MouseEnteredBounds(this, new EventArgs());
					}
					_mouseIsInBounds = true;
				}
				else
				{
					if (MouseLeftBounds != null)
					{
						MouseLeftBounds(this, new EventArgs());
					}
					_mouseIsInBounds = false;
				}
			}
			return false;
		}

		public void BeginWiring()
		{
			//do nothing
		}

		public void EndWiring()
		{
			//do nothing
		}

		public bool ShouldModifyFont { get; private set; }

		public bool MouseIsInBounds
		{
			get { return _mouseIsInBounds; }
		}

		public int MouseOverRow
		{
			get { return _mouseOverRow; }
		}
	}
}
