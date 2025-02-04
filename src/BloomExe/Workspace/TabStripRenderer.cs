using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Messir.Windows.Forms;

namespace Bloom.Workspace
{
	/// <summary>
	/// Represents a renderer class for TabStrip control
	/// </summary>
	internal class TabStripRenderer : ToolStripRenderer
	{
		private const int selOffset = 2;

		private ToolStripRenderer _currentRenderer = null;
		private ToolStripRenderMode _renderMode = ToolStripRenderMode.Custom;
		private bool _mirrored = false;
		private bool _useVisualStyles = Application.RenderWithVisualStyles;

		/// <summary>
		/// Gets or sets render mode for this renderer
		/// </summary>
		public ToolStripRenderMode RenderMode
		{
			get { return _renderMode; }
			set
			{
				_renderMode = value;
				switch (_renderMode)
				{
					case ToolStripRenderMode.Professional:
						_currentRenderer = new ToolStripProfessionalRenderer();
						break;
					case ToolStripRenderMode.System:
						_currentRenderer = new ToolStripSystemRenderer();
						break;
					default:
						_currentRenderer = null;
						break;
				}
			}
		}

		/// <summary>
		/// Gets or sets whether to mirror background
		/// </summary>
		/// <remarks>Use false for left and top positions, true for right and bottom</remarks>
		public bool Mirrored
		{
			get { return _mirrored; }
			set { _mirrored = value; }
		}

		/// <summary>
		/// Returns if visual styles should be applied for drawing
		/// </summary>
		public bool UseVisualStyles
		{
			get { return _useVisualStyles; }
			set
			{
				if (value && !Application.RenderWithVisualStyles)
					return;
				_useVisualStyles = value;
			}
		}

		protected override void Initialize(ToolStrip ts)
		{
			base.Initialize(ts);
		}

		protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
		{
			return;			
		}

		protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
		{
			if (_currentRenderer != null)
			{
				_currentRenderer.DrawToolStripBackground(e);
				return;
			}

			if (SIL.PlatformUtilities.Platform.IsWindows)
			{
				base.OnRenderToolStripBackground(e);
				return;
			}

			// there is no handler for this event in mono, so we need to handle it here.
			var b = new SolidBrush(e.ToolStrip.BackColor);
			e.Graphics.FillRectangle(b, e.AffectedBounds);
		}

		protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
		{
			Graphics g = e.Graphics;
			TabStrip tabs = e.ToolStrip as TabStrip;
			TabStripButton tab = e.Item as TabStripButton;
			if (tabs == null || tab == null)
			{
				if (_currentRenderer != null)
					_currentRenderer.DrawButtonBackground(e);
				else
					base.OnRenderButtonBackground(e);
				return;
			}

			bool selected = tab.Checked;
			bool hovered = tab.Selected;
			int top = 0;
			int left = 0;
			int width = tab.Bounds.Width - 1;
			int height = tab.Bounds.Height - 1;
			Rectangle drawBorder;

//
//			if (UseVisualStyles)
//			{
//				if (tabs.Orientation == Orientation.Horizontal)
//				{
//					if (!selected)
//					{
//						top = selOffset;
//						height -= (selOffset - 1);
//					}
//					else
//						top = 1;
//					drawBorder = new Rectangle(0, 0, width, height);
//				}
//				else
//				{
//					if (!selected)
//					{
//						left = selOffset;
//						width -= (selOffset - 1);
//					}
//					else
//						left = 1;
//					drawBorder = new Rectangle(0, 0, height, width);
//				}
//				using (Bitmap b = new Bitmap(drawBorder.Width, drawBorder.Height))
//				{
//					VisualStyleElement el = VisualStyleElement.Tab.TabItem.Normal;
//					if (selected)
//						el = VisualStyleElement.Tab.TabItem.Pressed;
//					if (hovered)
//						el = VisualStyleElement.Tab.TabItem.Hot;
//					if (!tab.Enabled)
//						el = VisualStyleElement.Tab.TabItem.Disabled;
//
//					if (!selected || hovered) drawBorder.Width++; else drawBorder.Height++;
//
//					using (Graphics gr = Graphics.FromImage(b))
//					{
//						VisualStyleRenderer rndr = new VisualStyleRenderer(el);
//						rndr.DrawBackground(gr, drawBorder);
//
//						if (tabs.Orientation == Orientation.Vertical)
//						{
//							if (Mirrored)
//								b.RotateFlip(RotateFlipType.Rotate270FlipXY);
//							else
//								b.RotateFlip(RotateFlipType.Rotate270FlipNone);
//						}
//						else
//						{
//							if (Mirrored)
//								b.RotateFlip(RotateFlipType.RotateNoneFlipY);
//						}
//						if (Mirrored)
//						{
//							left = tab.Bounds.Width - b.Width - left;
//							top = tab.Bounds.Height - b.Height - top;
//						}
//						g.DrawImage(b, left, top);
//					}
//				}
//			}
//			else
			{
				if (tabs.Orientation == Orientation.Horizontal)
				{
					if (!selected)
					{
						top = selOffset;
						height -= (selOffset - 1);
					}
					else
						top = 1;
					if (Mirrored)
					{
						left = 1;
						top = 0;
					}
					else
						top++;
					width--;
				}
//				else
//				{
//					if (!selected)
//					{
//						left = selOffset;
//						width--;
//					}
//					else
//						left = 1;
//					if (Mirrored)
//					{
//						left = 0;
//						top = 1;
//					}
//				}
				height--;
				drawBorder = new Rectangle(left, top, width, height);


				using (GraphicsPath gp = new GraphicsPath())
				{
//					if (Mirrored && tabs.Orientation == Orientation.Horizontal)
//					{
//						gp.AddLine(drawBorder.Left, drawBorder.Top, drawBorder.Left, drawBorder.Bottom - 2);
//						gp.AddArc(drawBorder.Left, drawBorder.Bottom - 3, 2, 2, 90, 90);
//						gp.AddLine(drawBorder.Left + 2, drawBorder.Bottom, drawBorder.Right - 2, drawBorder.Bottom);
//						gp.AddArc(drawBorder.Right - 2, drawBorder.Bottom - 3, 2, 2, 0, 90);
//						gp.AddLine(drawBorder.Right, drawBorder.Bottom - 2, drawBorder.Right, drawBorder.Top);
//					}
					//else
					 if (!Mirrored && tabs.Orientation == Orientation.Horizontal)
					{
						gp.AddLine(drawBorder.Left, drawBorder.Bottom, drawBorder.Left, drawBorder.Top + 2);
						gp.AddArc(drawBorder.Left, drawBorder.Top + 1, 2, 2, 180, 90);
						gp.AddLine(drawBorder.Left + 2, drawBorder.Top, drawBorder.Right - 2, drawBorder.Top);
						gp.AddArc(drawBorder.Right - 2, drawBorder.Top + 1, 2, 2, 270, 90);
						gp.AddLine(drawBorder.Right, drawBorder.Top + 2, drawBorder.Right, drawBorder.Bottom);
					}
//					else if (Mirrored && tabs.Orientation == Orientation.Vertical)
//					{
//						gp.AddLine(drawBorder.Left, drawBorder.Top, drawBorder.Right - 2, drawBorder.Top);
//						gp.AddArc(drawBorder.Right - 2, drawBorder.Top + 1, 2, 2, 270, 90);
//						gp.AddLine(drawBorder.Right, drawBorder.Top + 2, drawBorder.Right, drawBorder.Bottom - 2);
//						gp.AddArc(drawBorder.Right - 2, drawBorder.Bottom - 3, 2, 2, 0, 90);
//						gp.AddLine(drawBorder.Right - 2, drawBorder.Bottom, drawBorder.Left, drawBorder.Bottom);
//					}
//					else
//					{
//						gp.AddLine(drawBorder.Right, drawBorder.Top, drawBorder.Left + 2, drawBorder.Top);
//						gp.AddArc(drawBorder.Left, drawBorder.Top + 1, 2, 2, 180, 90);
//						gp.AddLine(drawBorder.Left, drawBorder.Top + 2, drawBorder.Left, drawBorder.Bottom - 2);
//						gp.AddArc(drawBorder.Left, drawBorder.Bottom - 3, 2, 2, 90, 90);
//						gp.AddLine(drawBorder.Left + 2, drawBorder.Bottom, drawBorder.Right, drawBorder.Bottom);
//					}

					if (selected || hovered)
					{
						Color fill = (hovered) ? Color.WhiteSmoke : Palette.SelectedTabBackground;
						if (_renderMode == ToolStripRenderMode.Professional)
						{
							fill = (hovered) ? ProfessionalColors.ButtonCheckedGradientBegin : ProfessionalColors.ButtonCheckedGradientEnd;
							using (LinearGradientBrush br = new LinearGradientBrush(tab.ContentRectangle, fill, ProfessionalColors.ButtonCheckedGradientMiddle, LinearGradientMode.Vertical))
								g.FillPath(br, gp);
						}
						else
							using (SolidBrush br = new SolidBrush(fill))
								g.FillPath(br, gp);
					}
					else
					{
						using (SolidBrush br = new SolidBrush(Palette.UnselectedTabBackground))
							g.FillPath(br, gp);
					}

//					using (Pen p = new Pen((selected) ? ControlPaint.Dark(SystemColors.AppWorkspace) : SystemColors.AppWorkspace))
//						g.DrawPath(p, gp);

					g.DrawPath(Pens.Black, gp);
				}
			}

		}

		protected override void OnRenderItemImage(ToolStripItemImageRenderEventArgs e)
		{
			TabStripButton btn = e.Item as TabStripButton;

			var rect = e.ImageRectangle;

			if (btn != null)
			{
				// adjust the image position up for Linux
				if (SIL.PlatformUtilities.Platform.IsLinux)
				{
					if (e.ToolStrip.Orientation == Orientation.Horizontal)
						rect.Offset(0, -4);
				}
				else
				{
					var delta = ((Mirrored) ? -1 : 1) * ((btn.Checked) ? 1 : selOffset);
					if (e.ToolStrip.Orientation == Orientation.Horizontal)
						rect.Offset((Mirrored) ? 2 : 1, delta + ((Mirrored) ? 1 : 0));
					else
						rect.Offset(delta + 2, 0);
				}
			}

			ToolStripItemImageRenderEventArgs x =
				new ToolStripItemImageRenderEventArgs(e.Graphics, e.Item, e.Image, rect);

			if (_currentRenderer != null)
				_currentRenderer.DrawItemImage(x);
			else
				base.OnRenderItemImage(x);
		}


		protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
		{
			// set the font before calculating the size because bold text is being cut off in Linux.
			TabStripButton btn = e.Item as TabStripButton;
			if ((btn != null) && btn.Checked)
				e.TextFont = btn.SelectedFont;

			e.SizeTextRectangleToText();

			// adjust the text position up for Linux
			var rect = e.TextRectangle;
			if (SIL.PlatformUtilities.Platform.IsLinux)
				rect.Offset(0, -2);
			else
				rect.Offset(0, 8); // hatton for bloom lower is better

			if (btn != null)
			{
				var delta = ((Mirrored) ? -1 : 1) * ((btn.Checked) ? 1 : selOffset);
				if (e.ToolStrip.Orientation == Orientation.Horizontal)
					rect.Offset((Mirrored) ? 2 : 1, delta + ((Mirrored) ? 1 : -1));
				else
					rect.Offset(delta + 2, 0);

				if (btn.Selected)
					e.TextColor = btn.HotTextColor;
				else if (btn.Checked)
					e.TextColor = btn.SelectedTextColor;
				else
					e.TextColor = Palette.LightTextAgainstDarkBackground;
			}

			e.TextRectangle = rect;

			if (_currentRenderer != null)
				_currentRenderer.DrawItemText(e);
			else
				base.OnRenderItemText(e);
		}

		protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
		{
			if (_currentRenderer != null)
				_currentRenderer.DrawArrow(e);
			else
				base.OnRenderArrow(e);
		}

		protected override void OnRenderDropDownButtonBackground(ToolStripItemRenderEventArgs e)
		{
			if (_currentRenderer != null)
				_currentRenderer.DrawDropDownButtonBackground(e);
			else
				base.OnRenderDropDownButtonBackground(e);
		}

		protected override void OnRenderGrip(ToolStripGripRenderEventArgs e)
		{
			if (_currentRenderer != null)
				_currentRenderer.DrawGrip(e);
			else
				base.OnRenderGrip(e);
		}

		protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
		{
			if (_currentRenderer != null)
				_currentRenderer.DrawImageMargin(e);
			else
				base.OnRenderImageMargin(e);
		}

		protected override void OnRenderItemBackground(ToolStripItemRenderEventArgs e)
		{
			if (_currentRenderer != null)
				_currentRenderer.DrawItemBackground(e);
			else
			{
				//base.OnRenderItemBackground(e);
				e.Graphics.FillRectangle(Brushes.BlueViolet, e.Item.ContentRectangle);
			}
		}

		protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
		{
			if (_currentRenderer != null)
				_currentRenderer.DrawItemCheck(e);
			else
				base.OnRenderItemCheck(e);
		}

		protected override void OnRenderLabelBackground(ToolStripItemRenderEventArgs e)
		{
			if (_currentRenderer != null)
				_currentRenderer.DrawLabelBackground(e);
			else
				base.OnRenderLabelBackground(e);
		}

		protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
		{
			if (_currentRenderer != null)
				_currentRenderer.DrawMenuItemBackground(e);
			else
				base.OnRenderMenuItemBackground(e);
		}

		protected override void OnRenderOverflowButtonBackground(ToolStripItemRenderEventArgs e)
		{
			if (_currentRenderer != null)
				_currentRenderer.DrawOverflowButtonBackground(e);
			else
				base.OnRenderOverflowButtonBackground(e);
		}

		protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
		{
			if (_currentRenderer != null)
				_currentRenderer.DrawSeparator(e);
			else
				base.OnRenderSeparator(e);
		}

		protected override void OnRenderSplitButtonBackground(ToolStripItemRenderEventArgs e)
		{
			if (_currentRenderer != null)
				_currentRenderer.DrawSplitButton(e);
			else
				base.OnRenderSplitButtonBackground(e);
		}

		protected override void OnRenderStatusStripSizingGrip(ToolStripRenderEventArgs e)
		{
			if (_currentRenderer != null)
				_currentRenderer.DrawStatusStripSizingGrip(e);
			else
				base.OnRenderStatusStripSizingGrip(e);
		}

		protected override void OnRenderToolStripContentPanelBackground(ToolStripContentPanelRenderEventArgs e)
		{
			if (_currentRenderer != null)
				_currentRenderer.DrawToolStripContentPanelBackground(e);
			else
				base.OnRenderToolStripContentPanelBackground(e);
		}

		protected override void OnRenderToolStripPanelBackground(ToolStripPanelRenderEventArgs e)
		{
			if (_currentRenderer != null)
				_currentRenderer.DrawToolStripPanelBackground(e);
			else
				base.OnRenderToolStripPanelBackground(e);
		}

		protected override void OnRenderToolStripStatusLabelBackground(ToolStripItemRenderEventArgs e)
		{
			if (_currentRenderer != null)
				_currentRenderer.DrawToolStripStatusLabelBackground(e);
			else
				base.OnRenderToolStripStatusLabelBackground(e);
		}
	}
}
