using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Color = System.Drawing.Color;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Point = System.Drawing.Point;

namespace TilesheetIndexGenerator;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
// ReSharper disable once UnusedMember.Global
// ReSharper disable once RedundantExtendsListEntry
public partial class MainWindow : Window
{
	internal static int FontWidth = 5;
	internal static int FontHeight = 7;

	internal static Bitmap Font = new("Assets\\numfont.png");

	internal static Bitmap? Input;
	internal static Bitmap? Overlay;

	internal enum IndexModes
	{
		Index,
		XY
	}

	internal static IndexModes IndexMode = IndexModes.Index;

	public MainWindow()
	{
		InitializeComponent();

		indexRadioButton.IsChecked = true;
	}

	private void BtnLoad_OnClick(object sender, RoutedEventArgs e)
	{
		OpenFileDialog op = new()
		{
			Title = "Select a picture",
			Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" +
					"JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
					"Portable Network Graphic (*.png)|*.png"
		};

		if (op.ShowDialog() != true)
			return;

		//InputImage = new BitmapImage(new Uri(op.FileName));
		Input = new Bitmap(op.FileName);
		Debug.WriteLine(Input.Width + " x " + Input.Height);
		DisplayImage.Source = new BitmapImage(new Uri(op.FileName));

		//DisplayImage.Width = DisplayImage.Source.Width;
		//DisplayImage.Height = DisplayImage.Source.Height;
		
		ZoomBorder.Reset();
		DisplayImage.Width = ZoomBorder.Width;
		DisplayImage.Height = ZoomBorder.Height;
	}
	
	private void BtnGenerate_OnClick(object sender, RoutedEventArgs e)
	{
		// show error if either width box input or height box input fails to validate as int
		if (!int.TryParse(TileWidthInputBox.Text, out int tileWidth) || !int.TryParse(TileHeightInputBox.Text, out int tileHeight))
		{
			MessageBox.Show("An integer value must be entered for both the tile width and tile height.", "Invalid Tile Dimensions", MessageBoxButton.OK, MessageBoxImage.Error);
			e.Handled = true;
			return;
		}

		// different error if input image is null
		if (DisplayImage.Source is null || Input is null)
		{
			MessageBox.Show("Image must be loaded first.", "No Input Image", MessageBoxButton.OK, MessageBoxImage.Error);
			e.Handled = true;
			return;
		}

		Overlay = IndexMode == IndexModes.Index ? GenerateIndexOverlay(tileWidth, tileHeight) : GenerateXYOverlay(tileWidth, tileHeight);
		
		Bitmap displayBitmap = new(Input.Width, Input.Height, PixelFormat.Format32bppArgb);
		displayBitmap.MakeTransparent(Color.Transparent);

		using Graphics displayGraphics = Graphics.FromImage(displayBitmap);

		displayGraphics.Clear(Color.Transparent);

		//displayGraphics.DrawImage(Input, new PointF(0, 0));
		//displayGraphics.DrawImage(Overlay, new PointF(0, 0));
		
		//displayGraphics.DrawImageUnscaled(Input, new Point(0, 0));

		// yeah i have no fucking idea why this is what i have to do to get it to draw the input image at the correct size.
		// what the actual fuck.
		displayGraphics.DrawImageUnscaledAndClipped(Input, new Rectangle(0, 0, Input.Width, Input.Height));
		displayGraphics.DrawImageUnscaled(Overlay, new Point(0, 0));

		DisplayImage.Source = displayBitmap.ToWpfBitmap();
	}

	private Bitmap GenerateXYOverlay(int tileWidth, int tileHeight)
	{
		//BitmapSource inputSource = (BitmapSource) InputImage.Source;

		int width = Input!.Width;
		int height = Input.Height;

		Bitmap outputBitmap = new(width, height, PixelFormat.Format32bppArgb);
		outputBitmap.MakeTransparent(Color.Transparent);

		using Graphics g = Graphics.FromImage(outputBitmap);

		g.Clear(Color.Transparent);
		
		List<string> rows = new();
		int maxDigitsInCell = (tileWidth / FontWidth) * (tileHeight / FontHeight);
		
		RectangleF srcRectangle = new(0, 0, FontWidth, FontHeight);
		RectangleF destRectangle = new(0, 0, FontWidth, FontHeight);
		g.DrawImage(Font, destRectangle, srcRectangle, GraphicsUnit.Pixel);

		int index = 0;
		for (int rowOffset = 1 * tileHeight; rowOffset < height; rowOffset += tileHeight)
		{
			index++;

			string indexString = index.ToString();
			int numDigits = indexString.Length;

			if (numDigits > maxDigitsInCell)
			{
				if (index == 0)
					return outputBitmap;

				index = 0;
				indexString = index.ToString();
			}

			int numDigitsInRow = tileWidth / FontWidth;

			rows.Clear();

			while (indexString.Length > numDigitsInRow)
			{
				rows.Insert(0, indexString[^numDigitsInRow..]);
				indexString = indexString[..^numDigitsInRow];
			}

			if (indexString.Length != 0)
			{
				rows.Insert(0, indexString);
			}
				
			for (int row = 0; row < rows.Count; row++)
			{
				int rowLength = rows[row].Length;
				string rowString = rows[row];

				for (int i = 0; i < rowLength; i++)
				{
					string digit = rowString.Substring(i, 1);

					if (digit == " ")
						continue;

					int digitOffset = int.Parse(digit) * FontWidth;
						
					srcRectangle = new RectangleF(digitOffset, 0, FontWidth, FontHeight);

					int x = i * (FontWidth - 1);
					int y = rowOffset + row * (FontHeight - 1);

					destRectangle = new RectangleF(x, y, FontWidth, FontHeight);
						
					g.DrawImage(Font, destRectangle, srcRectangle, GraphicsUnit.Pixel);
				}
			}
		}

		index = 0;
		for (int columnOffset = 1 * tileWidth; columnOffset < width; columnOffset += tileWidth)
		{
			index++;

			string indexString = index.ToString();
			int numDigits = indexString.Length;

			if (numDigits > maxDigitsInCell)
			{
				if (index == 0)
					return outputBitmap;

				index = 0;
				indexString = index.ToString();
			}

			int numDigitsInRow = tileWidth / FontWidth;

			rows.Clear();

			while (indexString.Length > numDigitsInRow)
			{
				rows.Insert(0, indexString[^numDigitsInRow..]);
				indexString = indexString[..^numDigitsInRow];
			}

			if (indexString.Length != 0)
			{
				rows.Insert(0, indexString);
			}
				
			for (int row = 0; row < rows.Count; row++)
			{
				int rowLength = rows[row].Length;
				string rowString = rows[row];

				for (int i = 0; i < rowLength; i++)
				{
					string digit = rowString.Substring(i, 1);

					if (digit == " ")
						continue;

					int digitOffset = int.Parse(digit) * FontWidth;
						
					srcRectangle = new RectangleF(digitOffset, 0, FontWidth, FontHeight);
					
					int x = columnOffset + i * (FontWidth - 1);
					int y = row * (FontHeight - 1);

					destRectangle = new RectangleF(x, y, FontWidth, FontHeight);
						
					g.DrawImage(Font, destRectangle, srcRectangle, GraphicsUnit.Pixel);
				}
			}
		}

		return outputBitmap;
	}

	private Bitmap GenerateIndexOverlay(int tileWidth, int tileHeight)
	{
		int width = Input!.Width;
		int height = Input.Height;

		Bitmap outputBitmap = new(width, height, PixelFormat.Format32bppArgb);
		outputBitmap.MakeTransparent(Color.Transparent);

		using Graphics g = Graphics.FromImage(outputBitmap);

		g.Clear(Color.Transparent);

		int rowOffset = 0;
		int index = 0;

		int maxDigitsInCell = (tileWidth / FontWidth) * (tileHeight / FontHeight);
		
		List<string> rows = new();

		while (rowOffset < height)
		{
			for (int columnOffset = 0; columnOffset < width; columnOffset += tileWidth)
			{
				string indexString = index.ToString();
				int numDigits = indexString.Length;

				if (numDigits > maxDigitsInCell)
				{
					if (index == 0)
						return outputBitmap;

					index = 0;
					indexString = index.ToString();
				}

				int numDigitsInRow = tileWidth / FontWidth;

				rows.Clear();

				while (indexString.Length > numDigitsInRow)
				{
					rows.Insert(0, indexString[^numDigitsInRow..]);
					indexString = indexString[..^numDigitsInRow];
				}

				if (indexString.Length != 0)
				{
					rows.Insert(0, indexString);
				}
				
				for (int row = 0; row < rows.Count; row++)
				{
					int rowLength = rows[row].Length;
					string rowString = rows[row];

					for (int i = 0; i < rowLength; i++)
					{
						string digit = rowString.Substring(i, 1);

						if (digit == " ")
							continue;

						int digitOffset = int.Parse(digit) * FontWidth;
						RectangleF srcRectangle = new(digitOffset, 0, FontWidth, FontHeight);
						
						int x = columnOffset + i * (FontWidth - 1);
						int y = rowOffset + row * (FontHeight - 1);

						RectangleF destRectangle = new(x, y, FontWidth, FontHeight);
						
						g.DrawImage(Font, destRectangle, srcRectangle, GraphicsUnit.Pixel);
					}
				}

				index++;
			}
		
			rowOffset += tileHeight;
		}

		return outputBitmap;
	}
	
	private void BtnSaveTilesheetWithIndices_OnClick(object sender, RoutedEventArgs e)
	{
		if (Overlay is null)
		{
			MessageBox.Show("Output must be generated first.", "No Output to Save", MessageBoxButton.OK, MessageBoxImage.Error);
			e.Handled = true;
			return;
		}
		
		int width = Input!.Width;
		int height = Input.Height;

		Image saveImg = new Bitmap(width, height, PixelFormat.Format32bppArgb);

		using Graphics gr = Graphics.FromImage(saveImg);
		gr.DrawImageUnscaled(Input, new Point(0, 0));
		gr.DrawImageUnscaled(Overlay, new Point(0, 0));
		
		SaveFileDialog save = new()
		{
			Title = "Save output",
			DefaultExt = ".png",
			Filter = "Portable Network Graphic (*.png)|*.png"
		};

		if (save.ShowDialog() != true)
			return;

		try
		{
			saveImg.Save(save.FileName, ImageFormat.Png);
		}
		catch (Exception ex)
		{
			MessageBox.Show($"Failed to save output. Details: {ex}", "Unable to Save", MessageBoxButton.OK, MessageBoxImage.Error);
		}
	}

	private void BtnSaveIndicesOnly_OnClick(object sender, RoutedEventArgs e)
	{
		if (Overlay is null)
		{
			MessageBox.Show("Output must be generated first.", "No Output to Save", MessageBoxButton.OK, MessageBoxImage.Error);
			e.Handled = true;
			return;
		}

		SaveFileDialog save = new()
		{
			Title = "Save output",
			DefaultExt = ".png",
			Filter = "Portable Network Graphic (*.png)|*.png"
		};

		if (save.ShowDialog() != true)
			return;

		try
		{
			Overlay.Save(save.FileName, ImageFormat.Png);
		}
		catch (Exception ex)
		{
			MessageBox.Show($"Failed to save output. Details: {ex}", "Unable to Save", MessageBoxButton.OK, MessageBoxImage.Error);
		}
	}

	private void CheckIfValidInput(object sender, TextCompositionEventArgs e)
	{
		e.Handled = !int.TryParse(e.Text, out _);
	}

	//private void HideOutputCheckBox_Changed(object sender, RoutedEventArgs e)
	//{
	//	OverlayImage.Visibility = HideOutputCheckbox.IsChecked ?? false ? Visibility.Hidden : Visibility.Visible;
	//}
	//
	//private void HideInputCheckBox_Changed(object sender, RoutedEventArgs e)
	//{
	//	InputImage.Visibility = HideInputCheckbox.IsChecked ?? false ? Visibility.Hidden : Visibility.Visible;
	//}

	private void xyRadioButton_Click(object sender, RoutedEventArgs e)
	{
		indexRadioButton.IsChecked = false;
		IndexMode = IndexModes.XY;
	}

	private void indexRadioButton_Click(object sender, RoutedEventArgs e)
	{
		xyRadioButton.IsChecked = false;
		IndexMode = IndexModes.Index;
	}
}
