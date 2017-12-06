﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using SIL.IO;
using SIL.Reporting;
using SIL.Windows.Forms.ImageToolbox;

namespace Bloom.ImageProcessing
{
	/// <summary>
	/// Currently the only processing we're doing it to make PNGs with lots of whitespace look good against our colored background pages
	/// Previously, we also shrunk images to improve performance when we were handing out file paths. Now that we are giving images
	/// over http, gecko may do well enough without the shrinking.
	/// </summary>
	public class RuntimeImageProcessor : IDisposable
	{
		private readonly BookRenamedEvent _bookRenamedEvent;
		public int TargetDimension = 500;

		// the ConcurrentDictionary is thread-safe
		private ConcurrentDictionary<string, string> _originalPathToProcessedVersionPath;

		// using a ConcurrentDictionary because there isn't a thread-safe List in .Net 4.0
		private ConcurrentDictionary<string, bool> _imageFilesToReturnUnprocessed;

		private string _cacheFolder;

		private static ImageAttributes _convertWhiteToTransparent;

		public RuntimeImageProcessor(BookRenamedEvent bookRenamedEvent)
		{
			_bookRenamedEvent = bookRenamedEvent;
			_originalPathToProcessedVersionPath = new ConcurrentDictionary<string, string>();
			_imageFilesToReturnUnprocessed = new ConcurrentDictionary<string, bool>();
			_cacheFolder = Path.Combine(Path.GetTempPath(), "Bloom");
			_bookRenamedEvent.Subscribe(OnBookRenamed);
		}

		private static ImageAttributes ConvertWhiteToTransparent
		{
			get
			{
				if (_convertWhiteToTransparent == null)
				{
					_convertWhiteToTransparent = new ImageAttributes();
					_convertWhiteToTransparent.SetColorKey(Color.FromArgb(253, 253, 253), Color.White);
				}
				return _convertWhiteToTransparent;
			}
		}

		private void OnBookRenamed(KeyValuePair<string, string> fromPathAndToPath)
		{
			//Note, we don't pay attention to what the change was, we just purge the whole cache

			TryToDeleteCachedImages();
			_originalPathToProcessedVersionPath = new ConcurrentDictionary<string, string>();
			_imageFilesToReturnUnprocessed = new ConcurrentDictionary<string, bool>();
		}

		public void Dispose()
		{
			if (_originalPathToProcessedVersionPath == null)
				return;

			TryToDeleteCachedImages();
			_originalPathToProcessedVersionPath = null;

			//NB: this turns out to be dangerous. Without it, we still delete all we can, leave some files around
			//each time, and then deleting them on the next run
			//			_cacheFolder.Dispose();

			GC.SuppressFinalize(this);
		}

		private void TryToDeleteCachedImages()
		{
			lock (this)
			{
				foreach(var path in _originalPathToProcessedVersionPath.Values)
				{
					try
					{
						if (RobustFile.Exists(path))
						{
							RobustFile.Delete(path);
							Debug.WriteLine("RuntimeImageProcessor Successfully deleted: " + path);
						}
					}
					catch (Exception e)
					{
						Debug.WriteLine("RuntimeImageProcessor Dispose(): " + e.Message);
					}
				}
				_originalPathToProcessedVersionPath.Clear();
			}
		}

		public string GetPathToResizedImage(string originalPath, bool getThumbnail = false)
		{
			//don't mess with Bloom UI images
			if (new[] {"/img/", "placeHolder", "Button"}.Any(s => originalPath.Contains(s)))
				return originalPath;

			var cacheFileName = originalPath;

			if (getThumbnail)
			{
				cacheFileName = "thumbnail_" + cacheFileName;
			}

			// check if this image is in the do-not-process list
			bool test;
			if (_imageFilesToReturnUnprocessed.TryGetValue(cacheFileName, out test)) return originalPath;

			lock (this)
			{
				// if there is a cached version, return it
				string pathToProcessedVersion;
				if (_originalPathToProcessedVersionPath.TryGetValue(cacheFileName, out pathToProcessedVersion))
				{
					if (RobustFile.Exists(pathToProcessedVersion) &&
						new FileInfo(originalPath).LastWriteTimeUtc <= new FileInfo(pathToProcessedVersion).LastWriteTimeUtc)
					{
						return pathToProcessedVersion;
					}

					// the file has changed, remove from cache
					string valueRemoved;
					_originalPathToProcessedVersionPath.TryRemove(cacheFileName, out valueRemoved);
				}

				// there is not a cached version, try to make one
				var pathToProcessedImage = Path.Combine(_cacheFolder, Path.GetRandomFileName() + Path.GetExtension(originalPath));

				if (!Directory.Exists(Path.GetDirectoryName(pathToProcessedImage)))
					Directory.CreateDirectory(Path.GetDirectoryName(pathToProcessedImage));

				// BL-1112: images not loading in page thumbnails
				bool success;
				if (getThumbnail)
				{
					// The HTML div that contains the thumbnails is 80 pixels wide, so make the thumbnails 80 pixels wide
					success = GenerateThumbnail(originalPath, pathToProcessedImage, 80);
				}
				else
				{
					success = MakePngBackgroundTransparent(originalPath, pathToProcessedImage);
				}

				if (!success)
				{
					// add this image to the do-not-process list so we don't waste time doing this again
					_imageFilesToReturnUnprocessed.TryAdd(cacheFileName, true);
					return originalPath;
				}

				_originalPathToProcessedVersionPath.TryAdd(cacheFileName, pathToProcessedImage); //remember it so we can reuse if they show it again, and later delete

				return pathToProcessedImage;
			}
		}

		// Make a thumbnail of the input image. newWidth and newHeight are both limits; the image will not be larger than original,
		// but if necessary will be shrunk to fit within the indicated rectangle.
		public static bool GenerateThumbnail(string originalPath, string pathToProcessedImage, int newWidth)
		{
			using (var originalImage = PalasoImage.FromFileRobustly(originalPath))
			{
				// check if it needs resized
				if (originalImage.Image.Width <= newWidth) return false;

				// calculate dimensions
				var newW = (originalImage.Image.Width > newWidth) ? newWidth : originalImage.Image.Width;
				var newH = newW * originalImage.Image.Height / originalImage.Image.Width;

				var thumbnail = new Bitmap(newW, newH);

				var g = Graphics.FromImage(thumbnail);

				Image imageToDraw = originalImage.Image;
				bool useOriginalImage = ImageUtils.AppearsToBeJpeg(originalImage);
				if (!useOriginalImage)
				{
					imageToDraw = MakePngBackgroundTransparent(originalImage);
				}
				var destRect = new Rectangle(0, 0, newW, newH);

				g.DrawImage(imageToDraw, destRect , new Rectangle(0,0,originalImage.Image.Width, originalImage.Image.Height),GraphicsUnit.Pixel);
				if (!useOriginalImage)
					imageToDraw.Dispose();
				RobustImageIO.SaveImage(thumbnail, pathToProcessedImage);
			}

			return true;
		}

		// Make a thumbnail of the input image. newWidth and newHeight are both limits; the image will not be larger than original,
		// but if necessary will be shrunk to fit within the indicated rectangle.
		public static bool GenerateEBookThumbnail(string coverImagePath, string pathToProcessedImage, int thumbnailWidth, int thumbnailHeight, Color backColor)
		{
			using (var coverImage = PalasoImage.FromFileRobustly(coverImagePath))
			{
				var coverImageWidth = coverImage.Image.Width;
				var coverImageHeight = coverImage.Image.Height;


				// We want to see a small border of background color, even if the image is a photo.
				const int kborder = 1;
				var availableThumbnailWidth = thumbnailWidth - (2 * kborder);
				var availableThumbnailHeight = thumbnailHeight - (2 * kborder);

				// Calculate how big the image can be while keeping its original proportions.
				// First assume the width is the limiting factor
				var targetImageWidth = (coverImageWidth > availableThumbnailWidth) ? availableThumbnailWidth : coverImage.Image.Width;
				var targetImageHeight = targetImageWidth * coverImageHeight / coverImageWidth;

				// if actually the height is the limiting factor, maximize height and re-compute the width
				if (targetImageHeight > availableThumbnailHeight)
				{
					targetImageHeight = availableThumbnailHeight;
					targetImageWidth = targetImageHeight * coverImageWidth / coverImageHeight;
				}

				// pad to center the cover image
				var horizontalPadding = (availableThumbnailWidth - targetImageWidth) / 2;
				var verticalPadding = (availableThumbnailHeight - targetImageHeight) / 2;
				var destRect = new Rectangle(kborder + horizontalPadding, kborder + verticalPadding, targetImageWidth, targetImageHeight);

				// the decision here is just a heuristic based on the observation that line-drawings seem to look better in nice square block of color,
				// while full-color (usually jpeg) books look better with a thin (or no) border. We could put this under user control eventually.

				Rectangle backgroundAndBorderRect;
				var appearsToBeJpeg = ImageUtils.AppearsToBeJpeg(coverImage);
				if(appearsToBeJpeg)
				{
					backgroundAndBorderRect = destRect;
					backgroundAndBorderRect.Inflate(kborder * 2, kborder * 2);
				}
				else
				{
					// or, if we decide to always deliver the full thing:
					backgroundAndBorderRect = new Rectangle(0, 0, thumbnailWidth, thumbnailHeight);
				}

				using (var thumbnail = new Bitmap(thumbnailWidth, thumbnailHeight))
				using (var g = Graphics.FromImage(thumbnail))
				using(var brush = new SolidBrush(backColor))
				{
					g.FillRectangle(brush, backgroundAndBorderRect);

					lock(ConvertWhiteToTransparent)
					{
						var imageAttributes = appearsToBeJpeg ? null : ConvertWhiteToTransparent;
						g.DrawImage(
							coverImage.Image, // finally, draw the cover image
							destRect, // with a scaled and centered destination
							0, 0, coverImageWidth, coverImageHeight, // from the entire cover image,
							GraphicsUnit.Pixel,
							imageAttributes); // changing white to transparent if a png
					}
					RobustImageIO.SaveImage(thumbnail, pathToProcessedImage);
				}
			}

			return true;
		}
		private static Image MakePngBackgroundTransparent(PalasoImage originalImage)
		{
			//impose a maximum size because in BL-2871 "Opposites" had about 6k x 6k and we got an ArgumentException
			//from the new BitMap()
			var destinationWidth = Math.Min(1000, originalImage.Image.Width);
			var destinationHeight = (int)((float)originalImage.Image.Height * ((float)destinationWidth / (float)originalImage.Image.Width));
			var processedBitmap = new Bitmap(destinationWidth, destinationHeight);
			using (var g = Graphics.FromImage(processedBitmap))
			{
				var destRect = new Rectangle(0, 0, destinationWidth, destinationHeight);
				lock (ConvertWhiteToTransparent)
				{
					g.DrawImage(originalImage.Image, destRect, 0, 0, originalImage.Image.Width, originalImage.Image.Height,
						GraphicsUnit.Pixel, ConvertWhiteToTransparent);
				}
			}
			return processedBitmap;
		}


		private bool MakePngBackgroundTransparent(string originalPath, string pathToProcessedImage)
		{
			try
			{
				using (var originalImage = PalasoImage.FromFileRobustly(originalPath))
				{
					//if it's a jpeg, we don't resize, we don't mess with transparency, nothing. These things
					//are scary in .net. Just send the original back and wash our hands of it.
					if (ImageUtils.AppearsToBeJpeg(originalImage))
					{
						return false;
					}

					using (var processedBitmap = MakePngBackgroundTransparent(originalImage))
					{
						//Hatton July 2012:
						//Once or twice I saw a GDI+ error on the Save below, when the app 1st launched.
						//I verified that if there is an IO error, that's what you get (a GDI+ error).
						//I looked once, and the %temp%/Bloom directory wasn't there, so that's what I think caused the error.
						//It's not clear why the temp/bloom directory isn't there... possibly it was there a moment ago
						//but then some startup thread cleared and deleted it? (we are now running on a thread responding to the http request)

						Exception error = null;
						for (var i = 0; i < 3; i++) //try three times
						{
							try
							{
								error = null;
								RobustImageIO.SaveImage(processedBitmap, pathToProcessedImage, originalImage.Image.RawFormat);
								break;
							}
							catch (Exception e)
							{
								Logger.WriteEvent("***Error in RuntimeImageProcessor while trying to write image.");
								Logger.WriteEvent(e.Message);
								error = e;
								//in setting the sleep time, keep in mind that this may be one of 20 images
								//so if the problem happens to all of them, then you're looking 20*retries*sleep-time,
								//which will look like hung program.
								//Meanwhile, this transparency thing is actually just a nice-to-have. If we give
								//up, it's ok.
								Thread.Sleep(100); //wait a 1/5 second before trying again
							}
						}

						if (error != null)
						{
							throw error;//will be caught below
						}
					}
				}

				return true;

			}
			//we want to gracefully degrade if this fails (as it did once, see comment in bl-2871)
			catch (TagLib.CorruptFileException e)
			{
				NonFatalProblem.Report(ModalIf.Beta, PassiveIf.All, "Problem with image metadata", originalPath, e);
				return false;
			}
			catch (Exception e)
			{
				//while beta might make sense, this is actually
				//a common failure at the moment, with the license.png
				//so I'm setting to alpha.
				NonFatalProblem.Report(ModalIf.Alpha, PassiveIf.All,"Problem making image transparent.", originalPath,e);
				return false;
			}
		}
	}
}
