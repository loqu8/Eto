using System;
using Eto.Forms;
using sw = System.Windows;
using sd = System.Drawing;
using swm = System.Windows.Media;
using swf = System.Windows.Forms;
using Eto.Drawing;

namespace Eto.Platform.Wpf.Forms
{
	public class ScreenHandler : WidgetHandler<swf.Screen, Screen>, IScreen
	{
		float scale;

		public ScreenHandler (sw.Window window)
		{
			var source = sw.PresentationSource.FromVisual (window);
			Control = GetCurrentScreen (window);
			scale = (float)(source.CompositionTarget.TransformToDevice.M22 * 92.0 / 72.0);
		}

		public ScreenHandler (swf.Screen screen)
		{
			Control = screen;
			var form = new swf.Form ();
			var graphics = form.CreateGraphics ();
			scale = graphics.DpiY / 72f;
		}

		static swf.Screen GetCurrentScreen (sw.Window window)
		{
			var centerPoint = new sd.Point ((int)(window.Left + window.ActualWidth / 2), (int)(window.Top + window.ActualHeight / 2));
			foreach (var s in swf.Screen.AllScreens) {
				if (s.Bounds.Contains (centerPoint))
					return s;
			}
			return swf.Screen.PrimaryScreen;
		}
		public float RealScale
		{
			get { return scale; }
		}

		public float Scale
		{
			get { return scale; }
		}

		public RectangleF Bounds
		{
			get { return Control.Bounds.ToEto (); }
		}

		public RectangleF WorkingArea
		{
			get { return Control.WorkingArea.ToEto (); }
		}

		public int BitsPerPixel
		{
			get { return Control.BitsPerPixel; }
		}

		public bool IsPrimary
		{
			get { return Control.Primary; }
		}
	}
}

