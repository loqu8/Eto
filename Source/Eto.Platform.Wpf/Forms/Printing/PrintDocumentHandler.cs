using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using sw = System.Windows;
using swd = System.Windows.Documents;
using swc = System.Windows.Controls;
using swm = System.Windows.Media;
using Eto.Forms;
using Eto.Drawing;
using Eto.Platform.Wpf.Drawing;

namespace Eto.Platform.Wpf.Forms.Printing
{
	public class PrintDocumentHandler : WidgetHandler<PrintDocumentHandler.Paginator, PrintDocument>, IPrintDocument
	{
		public PrintDocumentHandler ()
		{
			Control = new Paginator { Handler = this };
		}

		class Canvas : swc.UserControl
		{
			public PrintDocumentHandler Handler { get; set; }

			public int PageNumber { get; set; }

			protected override void OnRender (swm.DrawingContext dc)
			{
				base.OnRender (dc);
				var rect = new Rectangle (new Size((int)Width, (int)Height));
				var graphicsHandler = new GraphicsHandler (this, dc, new sw.Rect (0, 0, Width, Height));
				var graphics = new Graphics (Handler.Widget.Generator, graphicsHandler);
				// needed to set size properly for some reason.. ??
				graphics.DrawRectangle (new Pen(Colors.Transparent), rect);

				var args = new PrintPageEventArgs (graphics, rect.Size, PageNumber);
				Handler.Widget.OnPrintPage (args);
			}
		}

		public class Paginator : swd.DocumentPaginator
		{
			public PrintDocumentHandler Handler { get; set; }

			public override swd.DocumentPage GetPage (int pageNumber)
			{
				var page = new Canvas { 
					Handler = this.Handler,
					PageNumber = pageNumber,
					Width = ImageableArea.Width,
					Height = ImageableArea.Height
				};

				page.Measure (ImageableArea.Size);
				page.Arrange (ImageableArea);

				return new swd.DocumentPage(page);
			}

			public override bool IsPageCountValid
			{
				get { return true; }
			}

			public override int PageCount
			{
				get { return Handler.PageCount; }
			}

			public override sw.Size PageSize
			{
				get; set;
			}
			public sw.Rect ImageableArea
			{
				get;
				set;
			}

			public override swd.IDocumentPaginatorSource Source
			{
				get { return null; }
			}
		}

		public void Print ()
		{
			var print = new swc.PrintDialog ();
			print.SetEtoSettings(PrintSettings);

			Control.PageSize = new sw.Size (print.PrintableAreaWidth, print.PrintableAreaHeight);
			var printCapabilities = print.PrintQueue.GetPrintCapabilities(print.PrintTicket);
			var ia = printCapabilities.PageImageableArea;
			Control.ImageableArea = new sw.Rect(ia.OriginWidth, ia.OriginHeight, ia.ExtentWidth, ia.ExtentHeight);
			//printCapabilities.PageImageableArea.OriginWidth, printCapabilities.PageImageableArea.OriginHeight
			print.PrintDocument (Control, this.Name);
		}


		public override void AttachEvent (string id)
		{
			switch (id) {
			case PrintDocument.BeginPrintEvent:
			case PrintDocument.EndPrintEvent:
			case PrintDocument.PrintPageEvent:
				// handled by paginator
				break;
			default:
				base.AttachEvent (id);
				break;
			}
		}

		public string Name
		{
			get; set;
		}

		public int PageCount { get; set; }

		public PrintSettings PrintSettings
		{
			get; set;
		}
	}
}
