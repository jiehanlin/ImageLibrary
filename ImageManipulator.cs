using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;

namespace Dataway.ImageLibrary
{
	/// <summary>
	/// Provides static methods for quickly manipulating images such as scaling
	/// </summary>
	public static class ImageManipulator
	{
		#region Public Methods

		/// <summary>
		/// Get an image which is scaled to the new size keeping the aspect ratio
		/// </summary>
		/// <param name="image">Image to scale</param>
		/// <param name="newSize">Size of new image</param>
		/// <param name="isScaleUp">Should the output image be able to be bigger than the source image</param>
		/// <param name="useResampling">Setting to true will use a custom image sampler to improve quality on older windows servers, note speed is considerably slower</param>
		/// <returns>Image scaled to the new size</returns>
		public static Image GetScaledAspectImage(Image image, Size newSize, bool isScaleUp, bool useResampling)
		{
			return ImageManipulator.GetScaledAspectImage(image, newSize, isScaleUp, useResampling, false, Color.White);
		}

		/// <summary>
		/// Get an image which is scaled to the new size keeping the aspect ratio
		/// </summary>
		/// <param name="image">Source image</param>
		/// <param name="newSize">Size of new image</param>
		/// <param name="isScaleUp">Should the output image be able to be bigger than the source image</param>
		/// <param name="useResampling">Setting to true will use a custom image sampler to improve quality on older windows servers, note speed is considerably slower</param>
		/// <param name="padOut">Add padding around the image to keep size parameters with aspect ratio</param>
		/// <param name="backgroundColor">Background colour to appear in padding area</param>
		/// <returns>Image scaled to the new size</returns>
		public static Image GetScaledAspectImage(Image image, Size newSize, bool isScaleUp, bool useResampling, bool padOut, Color backgroundColor)
		{
			int width = 1;
			int height = 1;

			float imgRatio = (float)image.Height / (float)image.Width;
			float reqRatio = (float)newSize.Height / (float)newSize.Width;

			if (imgRatio < reqRatio)
			{
				width = newSize.Width;
				if (!isScaleUp && width > image.Width)
				{
					width = image.Width;
				}
				height = (int)Math.Round(((float)width * imgRatio));
			}
			else
			{
				height = newSize.Height;
				if (!isScaleUp && height > image.Height)
				{
					height = image.Height;
				}
				width = (int)Math.Round(((float)height / imgRatio));
			}

			if (width < 1)
			{
				width = 1;
			}
			if (height < 1)
			{
				height = 1;
			}

			Image scaled = null;

			if (useResampling)
			{
				scaled = GetScaledImageHiQuality(image, new Size(width, height));
			}
			else
			{
				scaled = GetScaledImage(image, new Size(width, height));
			}

			if (padOut)
			{
				if (width < newSize.Width || height < newSize.Height)
				{
					Bitmap padded = new Bitmap(newSize.Width, newSize.Height);
					Graphics g = Graphics.FromImage(padded);

					g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
					g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
					g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

					g.Clear(backgroundColor);

					int x = newSize.Width / 2 - scaled.Width / 2;
					int y = newSize.Height / 2 - scaled.Height / 2;

					g.DrawImage(scaled, x, y, scaled.Width, scaled.Height);
					scaled = padded;
				}
			}

			return scaled;
		}

		/// <summary>
		/// Get an image resized and cropped to fit the specified size
		/// </summary>
		/// <param name="image">Source image</param>
		/// <param name="fittedSize">Size of the new image</param>
		/// <param name="scaleUp">Should the output image be able to be bigger than the source image</param>
		/// <param name="useResampling">Setting to true will use a custom image sampler to improve quality on older windows servers, note speed is considerably slower</param>
		/// <returns>Image scaled and cropped to the new size</returns>
		public static Image GetFittedImage(Image image, Size fittedSize, bool scaleUp, bool useResampling, CropPosition position)
		{
			return ImageManipulator.GetFittedImage(image, fittedSize, scaleUp, useResampling, Color.White, position);
		}

		/// <summary>
		/// Get an image resized and cropped to fit the specified size
		/// </summary>
		/// <param name="image">Source image</param>
		/// <param name="fittedSize">Size of the new image</param>
		/// <param name="isScaleUp">Should the output image be able to be bigger than the source image</param>
		/// <param name="useResampling">Setting to true will use a custom image sampler to improve quality on older windows servers, note speed is considerably slower</param>
		/// <param name="backgroundColor">Background colour only appears when isScaleUp is false and the image is smaller than the fitted size</param>
		/// <returns>Image scaled and cropped to the new size</returns>
		public static Image GetFittedImage(Image image, Size fittedSize, bool isScaleUp, bool useResampling, Color backgroundColor, CropPosition position)
		{
			int width = 1;
			int height = 1;

			float imgRatio = (float)image.Height / (float)image.Width;
			float reqRatio = (float)fittedSize.Height / (float)fittedSize.Width;

			if (imgRatio < reqRatio)
			{
				height = fittedSize.Height;

				if (fittedSize.Height > image.Height && !isScaleUp)
				{
					height = image.Height;
				}

				width = (int)Math.Round((float)height / imgRatio);
			}
			else
			{
				width = fittedSize.Width;
				if (fittedSize.Width > image.Width && !isScaleUp)
				{
					width = image.Width;
				}
				height = (int)Math.Round((float)width * imgRatio);
			}

			if (width < 1)
			{
				width = 1;
			}
			if (height < 1)
			{
				height = 1;
			}

			Image drawImage = null;

			if (width == image.Width && height == image.Height)
			{
				drawImage = image;
			}
			else
			{
				if (useResampling)
				{
					drawImage = GetScaledImageHiQuality(image, new Size(width, height));
				}
				else
				{
					drawImage = GetScaledImage(image, new Size(width, height));
				}
			}

			Bitmap finalImg = new Bitmap(fittedSize.Width, fittedSize.Height);
			Graphics g = Graphics.FromImage(finalImg);

			g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
			g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
			g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

			g.Clear(backgroundColor);

			int x = 0;
			int y = 0;

			switch (position)
			{
				case CropPosition.Centre:
					x = (int)Math.Round((float)finalImg.Width / 2.0 - (float)drawImage.Width / 2.0);
					y = (int)Math.Round((float)finalImg.Height / 2.0 - (float)drawImage.Height / 2.0);
					break;
				case CropPosition.TopCentre:
					x = (int)Math.Round((float)finalImg.Width / 2.0 - (float)drawImage.Width / 2.0);
					y = 0;
					break;
				case CropPosition.TopLeft:
					x = 0;
					y = 0;
					break;
			}

			g.DrawImage(drawImage, x, y, drawImage.Width, drawImage.Height);

			drawImage.Dispose();
			drawImage = null;

			image.Dispose();
			image = null;

			g.Dispose();

			return finalImg;
		}

		/// <summary>
		/// Produces an image fitting into the fittedSize, if the image appears to have a background the image will be scaled and centred with the background colour, otherwise it will be scaled and cropped
		/// </summary>
		/// <param name="image">Source image</param>
		/// <param name="size">Size of the resulted image</param>
		/// <param name="useResampling">Setting to true will use a custom image sampler to improve quality on older windows servers, note speed is considerably slower</param>
		/// <param name="padding">Padding used for images with backgrounds to pad them away from edges</param>
		/// <param name="trimThreashold">Colour difference variation used to determined the trimming on an image</param>
		/// <returns> Produces an image fitting into the fittedSize</returns>
		public static Image GetSmartFit(Image image, Size size, bool useResampling, int padding, int trimThreashold, CropPosition position)
		{
			image = ImageManipulator.GetRepairedSource(image);

			Bitmap newBmp = new Bitmap(size.Width, size.Height);

			int preTrimW = image.Size.Width;
			int preTrimH = image.Size.Height;


			//image = GetMaximumImageSize(image, size);

			var trimResults = GetTrimmedImageResult(image, trimThreashold);

			// Wasn't trimmed therefore should be simple fitted image
			if (!trimResults.HasAllSidesTrimmed())
			{
				newBmp = (Bitmap)GetFittedImage(image, size, false, true, position);
			}
			else
			{
				Size paddedSize = new Size(size.Width - padding * 2, size.Height - padding * 2);

				if (paddedSize.Width <= 0 || paddedSize.Height <= 0)
				{
					paddedSize = size;
				}

				var trimmedImage = GetTrimmedImage(image, 0, trimResults);
				var newImg = GetScaledAspectImage(trimmedImage, paddedSize, false, useResampling);

				Graphics g = Graphics.FromImage(newBmp);

				g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
				g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
				g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

				g.Clear(trimResults.DiscoveredColor);
				g.DrawImage(newImg, newBmp.Width / 2 - newImg.Width / 2, newBmp.Height / 2 - newImg.Height / 2);
			}

			return newBmp;
		}

		/// <summary>
		/// Trims empty space around an image
		/// </summary>
		/// <param name="image">Source image</param>
		/// <param name="backgroundColor">Color to trim</param>
		/// <param name="threashold">difference allowed for colour trimming 1 to 255</param>
		/// <returns>Image with ouside color trimmed away</returns>
		public static Image GetTrimmedImage(Image image, int repad, int threashold)
		{
			return GetTrimmedImage(image, repad, GetTrimmedImageResult(image, threashold));
		}

		public static Image GetTrimmedImage(Image image, int repad, TrimResult result)
		{
			// This is done to ensure all the missing settings are recreated
			Bitmap img = GetRepairedSource(image) as Bitmap;

			if (!result.HasAllSidesTrimmed())
			{
				repad = 0;
			}

			Bitmap newImage = new Bitmap(img.Width - result.LeftTrim - result.RightTrim + repad * 2, img.Height - result.TopTrim - result.BottomTrim + repad * 2);

			Graphics g = Graphics.FromImage(newImage);

			g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
			g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
			g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

			g.Clear(result.DiscoveredColor);
			g.DrawImage(img, 0 - result.LeftTrim + repad, 0 - result.TopTrim + repad);

			g.Dispose();

			return newImage;
		}

		public static TrimResult GetTrimmedImageResult(Image image, Color backgroundColor, int threashold)
		{
			// Testing performance
			int iterations = 0;

			var img = image as Bitmap;

			TrimResult result = new TrimResult();

			// Top
			for (int y = 0; y < img.Height; y++)
			{
				for (int x = 1; x < img.Width; x++)
				{
					iterations++;
					if (GetColorDifference(img.GetPixel(x, y), backgroundColor) > threashold)
					{
						result.TopTrim = y;
						y = img.Height;
						break;
					}
				}
			}

			// Left
			for (int x = 0; x < img.Width; x++)
			{
				for (int y = result.TopTrim; y < img.Height; y++)
				{
					iterations++;
					if (GetColorDifference(img.GetPixel(x, y), backgroundColor) > threashold)
					{
						result.LeftTrim = x;
						x = img.Width;
						break;
					}
				}
			}

			// Right
			for (int x = img.Width - 1; x > result.LeftTrim; x--)
			{
				for (int y = result.TopTrim; y < img.Height; y++)
				{
					iterations++;
					if (GetColorDifference(img.GetPixel(x, y), backgroundColor) > threashold)
					{
						result.RightTrim = img.Width - 1 - x;
						x = 0;
						break;
					}
				}
			}

			// Bottom
			for (int y = img.Height - 1; y > result.TopTrim; y--)
			{
				for (int x = result.LeftTrim; x < img.Width; x++)
				{
					iterations++;
					if (GetColorDifference(img.GetPixel(x, y), backgroundColor) > threashold)
					{
						result.BottomTrim = img.Height - 1 - y;
						y = 0;
						break;
					}
				}
			}

			var total = iterations;

			return result;
		}

		public static TrimResult GetTrimmedImageResult(Image image, int threashold)
		{
			var backgroundColor = ((Bitmap)image).GetPixel(0, 0);
			var trimResults = GetTrimmedImageResult(image, backgroundColor, threashold);
			trimResults.DiscoveredColor = backgroundColor;
			return trimResults;
		}

		public static Image GetPaddedImage(Image image, int padding, Color backgroundColor)
		{
			Bitmap newBmp = new Bitmap(image.Width + padding * 2, image.Height + padding * 2);
			Graphics g = Graphics.FromImage(newBmp);

			g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
			g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
			g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

			g.Clear(backgroundColor);
			g.DrawImage(image, padding, padding);
			return newBmp;
		}

		/// <summary>
		/// Gets the stream for any image
		/// </summary>
		/// <param name="image">Image source</param>
		/// <returns>The stream for any image</returns>
		public static Stream GetImageStream(Image image)
		{
			ImageConverter ic = new ImageConverter();
			Image i2 = image.Clone() as Image;
			image = null;
			return new MemoryStream((byte[])ic.ConvertTo(i2, typeof(byte[])));
		}

		/// <summary>
		/// Get ImageCodecInfo from mime type
		/// </summary>
		/// <param name="mimeType">Mime type string for example "image/jpeg"</param>
		/// <returns>ImageCodecInfo codec info</returns>
		public static ImageCodecInfo GetCodexInfo(string mimeType)
		{
			ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();
			foreach (var encoder in encoders)
			{
				if (encoder.MimeType == mimeType)
				{
					return encoder;
				}
			}
			return null;
		}

		/// <summary>
		/// Get ImageCodecInfo from file extension
		/// </summary>
		/// <param name="extension">Filename extensions such as ".jpg"</param>
		/// <returns>ImageCodecInfo codec info</returns>
		public static ImageCodecInfo GetCodexInfoFromExt(string extension)
		{
			extension = extension.Replace(".", string.Empty).ToLower();
			ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();

			foreach (var encoder in encoders)
			{
				var encoderExtensions = encoder.FilenameExtension.Split(';');
				foreach (string encoderExt in encoderExtensions)
				{
					string cleanedExtension = encoderExt.Replace("*.", string.Empty).ToLower();
					if (cleanedExtension == extension)
						return encoder;
				}
			}

			return null;
		}

		/// <summary>
		/// Save an image, determines the image code from the filename extension using passed quality
		/// </summary>
		/// <param name="image">Image object to save</param>
		/// <param name="filename">Path to save image</param>
		/// <param name="quality">Quality, int between 0 and 100</param>
		public static void SaveImage(Image image, string filename, int quality)
		{
			if (quality < 0 || quality > 100)
			{
				throw new ArgumentOutOfRangeException("quality must be between 0 and 100.");
			}

			EncoderParameters myEncoderParameters = new EncoderParameters(1);
			myEncoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)quality);

			string ext = Path.GetExtension(filename);

			var codec = ImageManipulator.GetCodexInfoFromExt(ext);

			if (codec != null)
			{
				image.Save(filename, codec, myEncoderParameters);
			}
			else
			{
				image.Save(filename);
			}
		}

		/// <summary>
		/// Grey scale an image.
		/// </summary>
		/// <param name="image">Grey scaled image.</param>
		public static void GreyScale(Image image)
		{
			Bitmap bmp = image as Bitmap;

			int height = bmp.Height;
			int width = bmp.Width;

			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					Color c = bmp.GetPixel(x, y);
					int luma = (int)(c.R * 0.3 + c.G * 0.59 + c.B * 0.11);
					bmp.SetPixel(x, y, Color.FromArgb(luma, luma, luma));
				}
			}
		}

		/// <summary>
		/// Invert the image.
		/// </summary>
		/// <param name="image">Image to invert.</param>
		public static void Invert(Image image)
		{
			Bitmap bmp = image as Bitmap;

			int height = bmp.Height;
			int width = bmp.Width;

			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					Color s = bmp.GetPixel(x, y);
					bmp.SetPixel(x, y, Color.FromArgb(255 - s.R, 255 - s.G, 255 - s.B));
				}
			}
		}

		/// <summary>
		/// Changes the gamma on an image.
		/// </summary>
		/// <param name="image">Image to change.</param>
		/// <param name="red">Change gamma on red 1 is normal.</param>
		/// <param name="green">Change gamm1 on green 1 is normal.</param>
		/// <param name="blue">Change gamma on blue 1 is normal.</param>
		public static void SetGamma(Image image, double red, double green, double blue)
		{
			Bitmap bmap = (Bitmap)image;
			Color c;
			byte[] redGamma = CreateGammaArray(red);
			byte[] greenGamma = CreateGammaArray(green);
			byte[] blueGamma = CreateGammaArray(blue);

			int width = bmap.Width;
			int height = bmap.Height;

			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					c = bmap.GetPixel(x, y);
					bmap.SetPixel(x, y, Color.FromArgb(redGamma[c.R],
					   greenGamma[c.G], blueGamma[c.B]));
				}
			}
		}

		/// <summary>
		/// Set the brightness of an image.
		/// </summary>
		/// <param name="image">Image to change the brightness of.</param>
		/// <param name="brightness">Brightness amount ranging from -255 (darkest) to 255 (brightest).</param>
		public static void SetBrightness(Image image, int brightness)
		{
			Bitmap bmap = (Bitmap)image;
			if (brightness < -255) brightness = -255;
			if (brightness > 255) brightness = 255;
			Color c;

			int width = bmap.Width;
			int height = bmap.Height;

			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					c = bmap.GetPixel(x, y);
					int cR = c.R + brightness;
					int cG = c.G + brightness;
					int cB = c.B + brightness;

					if (cR < 0) cR = 1;
					if (cR > 255) cR = 255;

					if (cG < 0) cG = 1;
					if (cG > 255) cG = 255;

					if (cB < 0) cB = 1;
					if (cB > 255) cB = 255;

					bmap.SetPixel(x, y, Color.FromArgb((byte)cR, (byte)cG, (byte)cB));
				}
			}
		}

		/// <summary>
		/// Sets the contrast of an image.
		/// </summary>
		/// <param name="image">Image to change the contrast of.</param>
		/// <param name="contrast">Contrast value ranging from -100 (least contrast) to 100 (most contrast).</param>
		public static void SetContrast(Image image, double contrast)
		{
			Bitmap bmap = (Bitmap)image;
			if (contrast < -100) contrast = -100;
			if (contrast > 100) contrast = 100;
			contrast = (100.0 + contrast) / 100.0;
			contrast *= contrast;
			Color c;

			int width = bmap.Width;
			int height = bmap.Height;

			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					c = bmap.GetPixel(x, y);
					double pR = c.R / 255.0;
					pR -= 0.5;
					pR *= contrast;
					pR += 0.5;
					pR *= 255;
					if (pR < 0) pR = 0;
					if (pR > 255) pR = 255;

					double pG = c.G / 255.0;
					pG -= 0.5;
					pG *= contrast;
					pG += 0.5;
					pG *= 255;
					if (pG < 0) pG = 0;
					if (pG > 255) pG = 255;

					double pB = c.B / 255.0;
					pB -= 0.5;
					pB *= contrast;
					pB += 0.5;
					pB *= 255;
					if (pB < 0) pB = 0;
					if (pB > 255) pB = 255;

					bmap.SetPixel(x, y, Color.FromArgb((byte)pR, (byte)pG, (byte)pB));
				}
			}
		}

		/// <summary>
		/// Flip and rotate and image.
		/// </summary>
		/// <param name="image">Image to flip and rotate.</param>
		/// <param name="rotateFlipType">Type of flip and rotation to use.</param>
		public static void RotateFlip(Image image, RotateFlipType rotateFlipType)
		{
			image.RotateFlip(rotateFlipType);
		}

		/// <summary>
		/// Mask an image using an image with transparency
		/// </summary>
		/// <param name="image">Original image</param>
		/// <param name="mask">Mask image</param>
		/// <param name="invert">Invert the effect of the mask</param>
		public static void ApplyMask(Image image, Image mask, bool invert)
		{
			Bitmap bmp = image as Bitmap;
			Bitmap maskBmp = mask as Bitmap;

			int height = image.Height;
			int width = image.Width;

			int maskHeight = mask.Height;
			int maskWidth = mask.Width;

			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					var originalColour = bmp.GetPixel(x, y);
					if (x < maskWidth && y < maskHeight)
					{
						var trans = maskBmp.GetPixel(x, y).A;
						if (invert)
						{
							trans = (byte)(255 - (int)trans);
						}
						Color newColour = Color.FromArgb(trans, originalColour);
						bmp.SetPixel(x, y, newColour);
					}
					else
					{
						if (!invert)
						{
							bmp.SetPixel(x, y, Color.FromArgb(0, originalColour));
						}
					}
				}
			}
			maskBmp.Dispose();
		}

		/// <summary>
		/// Mask an image using a greyscale image, where lighter shades represent more visible areas
		/// </summary>
		/// <param name="image">Original image</param>
		/// <param name="mask">Mask image</param>
		/// <param name="invert">Invert the effect of the mask</param>
		public static void ApplyGreyScaleMask(Image image, Image mask, bool invert)
		{
			Bitmap bmp = image as Bitmap;
			Bitmap maskBmp = mask as Bitmap;

			int height = image.Height;
			int width = image.Width;

			int maskHeight = mask.Height;
			int maskWidth = mask.Width;

			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					var originalColour = bmp.GetPixel(x, y);
					if (x < maskWidth && y < maskHeight)
					{
						var trans = maskBmp.GetPixel(x, y).R;
						if (invert)
						{
							trans = (byte)(255 - (int)trans);
						}
						Color newColour = Color.FromArgb(trans, originalColour);
						bmp.SetPixel(x, y, newColour);
					}
					else
					{
						if (!invert)
						{
							bmp.SetPixel(x, y, Color.FromArgb(0, originalColour));
						}
					}
				}
			}
			maskBmp.Dispose();
		}

		public static Image GetCropped(Image image, Rectangle cropRect)
		{
			Bitmap bmp = image as Bitmap;
			var cropped = bmp.Clone(cropRect, bmp.PixelFormat);
			return cropped;
		}

		private static Color GetColorBlend(Color underlyingColor, Color overlayColor, BlendMode blend, int amount)
		{
			int newR = 0;
			int newG = 0;
			int newB = 0;

			switch (blend)
			{
				case BlendMode.Alpha:
					newR = overlayColor.R;
					newG = overlayColor.G;
					newB = overlayColor.B;
					break;
				case BlendMode.Multiply:
					newR = (int)(underlyingColor.R * overlayColor.R / 255);
					newG = (int)(underlyingColor.G * overlayColor.G / 255);
					newB = (int)(underlyingColor.B * overlayColor.B / 255);
					break;
				case BlendMode.Additive:
					newR = (int)(underlyingColor.R + overlayColor.R);
					newG = (int)(underlyingColor.G + overlayColor.G);
					newB = (int)(underlyingColor.B + overlayColor.B);
					break;
				case BlendMode.Darken:
					newR = (int)(underlyingColor.R - overlayColor.R);
					newG = (int)(underlyingColor.G - overlayColor.G);
					newB = (int)(underlyingColor.B - overlayColor.B);
					break;
				case BlendMode.RootMultiply:
					newR = (int)(Math.Sqrt(underlyingColor.R) * Math.Sqrt(overlayColor.R));
					newG = (int)(Math.Sqrt(underlyingColor.G) * Math.Sqrt(overlayColor.G));
					newB = (int)(Math.Sqrt(underlyingColor.B) * Math.Sqrt(overlayColor.B));
					break;
				case BlendMode.Divide:
					newR = (int)(underlyingColor.R ^ 2 * overlayColor.R ^ 2 * 2);
					newG = (int)(underlyingColor.G ^ 2 * overlayColor.G ^ 2 * 2);
					newB = (int)(underlyingColor.B ^ 2 * overlayColor.B ^ 2 * 2);
					break;
				case BlendMode.ReverseDivide:
					newR = (int)(underlyingColor.R / 255f * overlayColor.R);
					newG = (int)(underlyingColor.G / 255f * overlayColor.G);
					newB = (int)(underlyingColor.B / 255f * overlayColor.B);
					break;
				case BlendMode.Difference:
					newR = (int)(overlayColor.R - underlyingColor.R);
					newG = (int)(overlayColor.G - underlyingColor.G);
					newB = (int)(overlayColor.B - underlyingColor.B);
					break;
				case BlendMode.Distance:
					newR = (int)(overlayColor.R * 2 - underlyingColor.R * 2);
					newG = (int)(overlayColor.G * 2 - underlyingColor.G * 2);
					newB = (int)(overlayColor.B * 2 - underlyingColor.B * 2);
					break;
				case BlendMode.Saturate:
					int diffR = underlyingColor.R - overlayColor.R;
					int diffG = underlyingColor.G - overlayColor.G;
					int diffB = underlyingColor.B - overlayColor.B;
					newR = (int)(overlayColor.R * diffR / 16f);
					newG = (int)(overlayColor.G * diffG / 16f);
					newB = (int)(overlayColor.B * diffB / 16f);
					break;
			}

			//apply amount
			newR = underlyingColor.R + (int)((newR - underlyingColor.R) * amount / 100f * overlayColor.A / 255f);
			newG = underlyingColor.G + (int)((newG - underlyingColor.G) * amount / 100f * overlayColor.A / 255f);
			newB = underlyingColor.B + (int)((newB - underlyingColor.B) * amount / 100f * overlayColor.A / 255f);

			// Ensuring now colours are outside bounds.
			if (newR < 0)
			{
				newR = 0;
			}
			if (newR > 255)
			{
				newR = 255;
			}
			if (newG < 0)
			{
				newG = 0;
			}
			if (newG > 255)
			{
				newG = 255;
			}
			if (newB < 0)
			{
				newB = 0;
			}
			if (newB > 255)
			{
				newB = 255;
			}

			return Color.FromArgb(underlyingColor.A, newR, newG, newB);
		}

		public static Image OverlayImage(Image underlyingImage, Image overlayImage, BlendMode blend, int amount)
		{
			Bitmap newImage = underlyingImage.Clone() as Bitmap;

			Bitmap overlayBitmap = overlayImage as Bitmap;

			for (int y = 0; y < overlayBitmap.Height; y++)
			{
				for (int x = 0; x < overlayBitmap.Width; x++)
				{
					newImage.SetPixel(x, y, GetColorBlend(newImage.GetPixel(x, y), overlayBitmap.GetPixel(x, y), blend, amount));
				}
			}

			return newImage;
		}

		public delegate SafeColor BlendColor(Color c1, Color c2);

		public static Image ColorImage(Image image, Color color, BlendColor blendColors)
		{
			Bitmap bmpImage = image as Bitmap;
			Bitmap newImg = new Bitmap(image.Width, image.Height);

			for (int y = 0; y < image.Height; y++)
			{
				for (int x = 0; x < image.Width; x++)
				{
					newImg.SetPixel(x, y, blendColors(bmpImage.GetPixel(x, y), color).GetColor());
				}
			}

			bmpImage.Dispose();

			return newImg;
		}

		public static Image ManipluatePixel(Image image, BlendColorHandler blender)
		{
			Bitmap bmp = image as Bitmap;
			for (int y = 0; y < bmp.Height; y++)
			{
				for (int x = 0; x < bmp.Width; x++)
				{
					bmp.SetPixel(x, y, blender(bmp.GetPixel(x, y)).GetColor());
				}
			}
			return bmp;
		}

		public static Image BlendImages(Image img1, Image img2, BlendColor blendProcess)
		{
			int width = img1.Width;
			int height = img1.Height;
			if (img2.Width < width)
			{
				width = img2.Width;
			}
			if (img2.Height < height)
			{
				height = img2.Height;
			}

			Bitmap newBmp = new Bitmap(width, height);

			Bitmap img1Bmp = img1 as Bitmap;
			Bitmap img2Bmp = img2 as Bitmap;

			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					newBmp.SetPixel(x, y, blendProcess(img1Bmp.GetPixel(x, y), img2Bmp.GetPixel(x, y)).GetColor());
				}
			}

			img1Bmp.Dispose();
			img2Bmp.Dispose();

			return newBmp;
		}

		#endregion

		public delegate SafeColor BlendColorHandler(Color c1);

		#region Private Methods

		private static Image GetScaledImageHiQuality(Image image, Size size)
		{
			ResamplingService resamplingService = new ResamplingService();
			resamplingService.Filter = ResamplingFilters.Mitchell;
			ushort[][,] input = ConvertBitmapToArray((Bitmap)image);
			ushort[][,] output = resamplingService.Resample(input, size.Width, size.Height);
			Image drawImage = (Image)ConvertArrayToBitmap(output);

			image.Dispose();
			image = null;

			return drawImage;
		}

		private static Image GetScaledImage(Image image, Size size)
		{
			Bitmap bmp = new Bitmap(image, size);
			image.Dispose();
			image = null;
			return bmp;
		}

		private static Image GetRepairedSource(Image image)
		{
			return GetScaledAspectImage(image, image.Size, false, true);
		}

		/// <summary>
		/// Converts Bitmap to array. Supports only Format32bppArgb pixel format.
		/// </summary>
		/// <param name="bmp">Bitmap to convert.</param>
		/// <returns>Output array.</returns>
		private static ushort[][,] ConvertBitmapToArray(Bitmap bmp)
		{
			ushort[][,] array = new ushort[4][,];

			for (int i = 0; i < 4; i++)
				array[i] = new ushort[bmp.Width, bmp.Height];

			BitmapData bd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
			int nOffset = (bd.Stride - bd.Width * 4);
			unsafe
			{
				byte* p = (byte*)bd.Scan0;
				for (int y = 0; y < bd.Height; y++)
				{
					for (int x = 0; x < bd.Width; x++)
					{
						array[3][x, y] = (ushort)p[3];
						array[2][x, y] = (ushort)p[2];
						array[1][x, y] = (ushort)p[1];
						array[0][x, y] = (ushort)p[0];
						p += 4;
					}
					p += nOffset;
				}
			}
			bmp.UnlockBits(bd);
			return array;
		}

		/// <summary>
		/// Converts array to Bitmap. Supports only Format32bppArgb pixel format.
		/// </summary>
		/// <param name="array">Array to convert.</param>
		/// <returns>Output Bitmap.</returns>
		private static Bitmap ConvertArrayToBitmap(ushort[][,] array)
		{

			int width = array[0].GetLength(0);
			int height = array[0].GetLength(1);
			Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
			BitmapData bd = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
			int nOffset = (bd.Stride - bd.Width * 4);
			unsafe
			{
				byte* p = (byte*)bd.Scan0;
				for (int y = 0; y < height; y++)
				{
					for (int x = 0; x < width; x++)
					{
						p[3] = (byte)Math.Min(Math.Max(array[3][x, y], Byte.MinValue), Byte.MaxValue);
						p[2] = (byte)Math.Min(Math.Max(array[2][x, y], Byte.MinValue), Byte.MaxValue);
						p[1] = (byte)Math.Min(Math.Max(array[1][x, y], Byte.MinValue), Byte.MaxValue);
						p[0] = (byte)Math.Min(Math.Max(array[0][x, y], Byte.MinValue), Byte.MaxValue);
						p += 4;
					}
					p += nOffset;
				}
			}
			bmp.UnlockBits(bd);
			return bmp;
		}

		private static byte[] CreateGammaArray(double color)
		{
			byte[] gammaArray = new byte[256];
			for (int i = 0; i < 256; ++i)
			{
				gammaArray[i] = (byte)Math.Min(255, (int)((255.0 * Math.Pow(i / 255.0, 1.0 / color)) + 0.5));
			}
			return gammaArray;
		}

		//private static int GetColorDifference(Color a, Color b)
		//{
		//    List<int> differeneces = new List<int>();

		//    if (a.A == 0 || b.A == 0)
		//    {
		//        return 0;
		//    }
		//    else
		//    {
		//        differeneces.Add(Math.Abs(a.R - b.R));
		//        differeneces.Add(Math.Abs(a.G - b.G));
		//        differeneces.Add(Math.Abs(a.B - b.B));
		//    }
		//    differeneces.Add(Math.Abs(a.A - b.A));

		//    int res = differeneces.Max();
		//    return differeneces.Max();
		//}

		private static int GetColorDifference(Color a, Color b)
		{
			int diffference = 0;
			if (a.A == 0 || b.A == 0)
			{
				return 0;
			}
			else
			{
				diffference += Convert.ToInt32(Math.Abs(a.R - b.R));
				diffference += Convert.ToInt32(Math.Abs(a.G - b.G));
				diffference += Convert.ToInt32(Math.Abs(a.B - b.B));
			}
			return diffference / 3;
		}


		private static Image GetMaximumImageSize(Image image, Size maxSize)
		{
			return GetScaledAspectImage(image, maxSize, true, false);
		}

		#endregion
	}

	public class SafeColor
	{
		public SafeColor()
		{
			this.Alpha = 255;
		}

		public SafeColor(int alpha, int r, int g, int b)
		{
			this.Alpha = alpha;
			this.R = r;
			this.G = g;
			this.B = b;
		}

		public SafeColor(int r, int g, int b)
			: this(255, r, g, b)
		{
		}

		public int R { get; set; }

		public int G { get; set; }

		public int B { get; set; }

		public int Alpha { get; set; }

		public Color GetColor()
		{
			int r = this.R;
			int g = this.G;
			int b = this.B;
			int a = this.Alpha;

			if (r > 255)
			{
				r = 255;
			}
			if (r < 0)
			{
				r = 0;
			}
			if (g > 255)
			{
				g = 255;
			}
			if (g < 0)
			{
				g = 0;
			}
			if (b > 255)
			{
				b = 255;
			}
			if (b < 0)
			{
				b = 0;
			}

			return Color.FromArgb(a, r, g, b);
		}
	}

	public class TrimResult
	{
		public TrimResult() { }

		public TrimResult(int top, int left, int right, int bottom)
		{
			this.TopTrim = top;
			this.LeftTrim = left;
			this.RightTrim = right;
			this.BottomTrim = bottom;
		}

		#region Public Properties

		public int TopTrim { get; set; }

		public int LeftTrim { get; set; }

		public int RightTrim { get; set; }

		public int BottomTrim { get; set; }

		public Color DiscoveredColor { get; set; }

		#endregion

		#region Public Methods

		public bool HasAnyTrimming()
		{
			return (TopTrim != 0 || LeftTrim != 0 || RightTrim != 0 || BottomTrim != 0);
		}

		public bool HasAllSidesTrimmed()
		{
			return (TopTrim != 0 && LeftTrim != 0 && RightTrim != 0 && BottomTrim != 0);
		}

		#endregion
	}

	public enum CropPosition
	{
		Centre,
		TopLeft,
		TopCentre
	}

	public enum BlendMode
	{
		Alpha,
		Additive,
		Multiply,
		RootMultiply,
		Darken,
		Divide,
		ReverseDivide,
		Difference,
		Distance,
		Saturate
	}
}
