using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace TilesheetIndexGenerator;


/*
 *
 * credit for this code goes to Daniel Wolf et al. (https://stackoverflow.com/a/6775114)
 *
 */


public static class Extensions {

	public static Bitmap ToWinFormsBitmap(this BitmapSource bitmapsource)
	{
		using MemoryStream stream = new();
		BitmapEncoder enc = new PngBitmapEncoder();
		enc.Frames.Add(BitmapFrame.Create(bitmapsource));
		enc.Save(stream);

		using Bitmap tempBitmap = new(stream);

		// According to MSDN, one "must keep the stream open for the lifetime of the Bitmap."
		// So we return a copy of the new bitmap, allowing us to dispose both the bitmap and the stream.
		return new Bitmap(tempBitmap);
	}

	public static BitmapSource ToWpfBitmap(this Bitmap bitmap)
	{
		using MemoryStream stream = new();
		bitmap.Save(stream, ImageFormat.Png);

		stream.Position = 0;
		BitmapImage result = new();
		result.BeginInit();
		// According to MSDN, "The default OnDemand cache option retains access to the stream until the image is needed."
		// Force the bitmap to load right now so we can dispose the stream.
		result.CacheOption = BitmapCacheOption.OnLoad;
		result.StreamSource = stream;
		result.EndInit();
		result.Freeze();
		return result;
	}
}