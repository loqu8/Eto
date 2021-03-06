using Eto.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using swm = System.Windows.Media;

namespace Eto.Platform.Wpf.Drawing
{
	public class FontsHandler : WidgetHandler<Widget>, IFonts
	{
		HashSet<string> availableFontFamilies;

		public IEnumerable<FontFamily> AvailableFontFamilies
		{
			get { return swm.Fonts.SystemFontFamilies.Select (r => new FontFamily (Generator, new FontFamilyHandler (r))); ; }
		}

		public bool FontFamilyAvailable (string fontFamily)
		{
			if (availableFontFamilies == null) {
				availableFontFamilies = new HashSet<string> (StringComparer.InvariantCultureIgnoreCase);
				foreach (var family in swm.Fonts.SystemFontFamilies) {
					availableFontFamilies.Add (family.Source);
				}
			}
			return availableFontFamilies.Contains (fontFamily);
		}
	}
}
