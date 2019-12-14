/*----------------------------------------------------------------------------//
 * HDRImage.cs
 * Callum Walker
 * February 2018
 * Class for manipulating and storing HDR images
//----------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using System.IO;
//----------------------------------------------------------------------------//
namespace PictureLoadingApp {
	class HDRImage {

		// Two-dimensional array of pixels
		//private HDRPixel[,] _pixels;
		private float[] _data;

		// Here are some dimensions!!!
		private int _width = -1, _height = -1;
		public int Width { get { return _width; } }
		public int Height { get { return _height; } }

		/// <summary>
		/// <see cref="LDRImage"/> representation of this <see cref="HDRImage"/>
		/// </summary>
		private LDRImage _ldr;

		/// <summary>
		/// Constructor working
		/// </summary>
		/// <param name="width">The width of the <see cref="HDRImage"/></param>
		/// <param name="height">The height of the <see cref="HDRImage"/></param>
		public HDRImage(int width, int height) {
			// Wow look at that
			_Init(width, height);
		}

		/// <summary>
		/// Creates a new <see cref="HDRImage"/> from an array of floats.
		/// </summary>
		/// <param name="width">Width of the image, in pixels</param>
		/// <param name="pixels">Array of float values in RGBA order.</param>
		public HDRImage(int width, float[] pixels) {
			// Init: calculate height
			int height = (pixels.Length / 4) / width;
			_Init(width, (height < 1) ? 1 : height);

			// Welp.
			_data = pixels;
		}

		// Here we go!!!
		void _Init(int width, int height) {
			// Actually create the array
			_data = new float[width * height * 4];

			// Set the values to the, uhh... correct values?
			_width = width;
			_height = height;
		}

		/// <summary>
		/// Set the color of a pixel in this <see cref="HDRImage"/>
		/// </summary>
		/// <param name="x">X coordinate of pixel</param>
		/// <param name="y">Y coordinate of pixel</param>
		/// <param name="pix">The color of the pixel</param>
		public void SetPixel(int x, int y, HDRPixel pix) {
			if (x >= 0 && y >= 0 && x < _width && y < _height) { 
				_data[4 * (x + y * _width) + 0] = pix.Red;
				_data[4 * (x + y * _width) + 1] = pix.Green;
				_data[4 * (x + y * _width) + 2] = pix.Blue;
				_data[4 * (x + y * _width) + 3] = pix.Alpha;
			}
		}

		/// <summary>
		/// Gets a pixel from this <see cref="HDRImage"/>
		/// as a <see cref="HDRPixel"/>
		/// </summary>
		/// <param name="x">X coordinate of pixel</param>
		/// <param name="y">Y coordinate of pixel</param>
		/// <returns>An <see cref="HDRPixel"/> object,</returns>
		public HDRPixel GetPixel (int x, int y) {
			return new HDRPixel(
					_data[4 * (x + y * _width) + 0],
					_data[4 * (x + y * _width) + 1],
					_data[4 * (x + y * _width) + 2],
					_data[4 * (x + y * _width) + 3]
				);
		}
		
		/// <summary>
		/// FOR TESTING ONLY: 
		/// Fills the image full of "random" pixels
		/// </summary>
		public void Randomize() {
			for (int x = 0; x < _width; ++x) {
				for (int y = 0; y < _height; ++y) {
					SetPixel(x, y, new HDRPixel(
						(float)Math.Abs(Math.Cos(x / 3.5)),
						y / 100f, 
						1f + (float)Math.Sin(y / 5.0)));
				}
			}
		}

		//--------------------------------------------------------------------//

		/// <summary>
		/// Generates a representation of this <see cref="HDRImage"/> as an
		/// <see cref="LDRImage"/>
		/// </summary>
		public byte[] Transform() {
			byte[] bytes = new byte[_data.Length];

			// TODO: ACTUAL COLOR TRANSFORMATION

			// Slow: find min and max bounds
			float min = float.PositiveInfinity, max = float.NegativeInfinity;
			for (int i = 0; i < _data.Length; ++i) {
				if (_data[i] < min) min = _data[i];
				if (_data[i] > max) max = _data[i];

				// Skip alpha values
				if (i % 4 == 2)
					i++;
			}
            
			// AND AWAY WE GO
			for (int i = 0; i < bytes.Length; i ++) {
				// alpha is always 1 for now
				if (i % 4 == 3) {
					bytes[i] = 255;
				} else {
					// Map the value with the min and max from earlier
					bytes[i] = (byte)(255f * (_data[i] - min) / max);
				}
			}
			
			// Return LDR
			return bytes;

			/*
			_ldr = new LDRImage();
			await _ldr.LoadFromBytes(bytes, _width, _height);
			return _ldr;
			*/	
		}

		// Returns byte array of channel R (0), G (1), or B (2)
		public byte[] TransformChannel(int channel) {
			byte[] bytes = new byte[_data.Length];

			// all bytes zero
			for (int i = 0; i < bytes.Length; ++i) {
				bytes[i] = (byte)((i % 4 == 3) ? 255 : 0);
			}

			// Slow: find min and max bounds
			float min = float.PositiveInfinity, max = float.NegativeInfinity;
			for (int i = channel; i < _data.Length; i += 4) {
				if (_data[i] < min) min = _data[i];
				if (_data[i] > max) max = _data[i];
			}
			
			// AND AWAY WE GO
			for (int i = channel; i < bytes.Length; i += 4) {
				// Map the value with the min and max from earlier
				bytes[i] = (byte)(255f * (_data[i] - min) / max);
			}

			// GREEN ONLY: FOR TESTING PURPOSES
			/*for (int i = 0;  i < bytes.Length; i += 4) {
				bytes[i + 0] = bytes[i + 2] = bytes[i + 1];	
			}*/
			
			// Return LDR
			return bytes;
		}

		/// <summary>
		/// Attempts to turn the HDR pixels into  RGBE (*.hdr) color data.
		/// </summary>
		/// <returns>Returns a byte array.</returns>
		private byte[] ToRGBEBytes() {
			// Let's byte
			byte[] bytes = new byte[_width * _height * 4];

			// FILL IT UP
			for (int i = 0; i < bytes.Length; i += 4) {
				byte[] pix = GetPixel((i / 4) % _width, (i / 4) / _width).ToRGBE();
				bytes[i + 0] = pix[0];
				bytes[i + 1] = pix[1];
				bytes[i + 2] = pix[2];
				bytes[i + 3] = pix[3];
			}

			// Done.
			return bytes;
		}

		/// <summary>
		/// Attempts to turn the HDR pixels into  RGBE (*.hdr) color data.
		/// Uses run-length encoding to do so.
		/// </summary>
		private byte[] ToRGBEBytesRLE() {
			// From: https://www.graphics.cornell.edu/~bjw/rgbe.html
			// Don't bother with this if width is too high or low?
			if (_width < 8 || _width > 0x7fff)
				return ToRGBEBytes();

			// Output stream
			Stream output = new MemoryStream();
			
			// Minimum run length
			int minlen = 4;

			// RGBE values
			byte[] pix = new byte[4];

			// First in each line:
			byte[] init = new byte[] { 2, 2, (byte)(_width >> 8), (byte)(_width & 0xFF) };

			// scanlines
			for (int y = 0; y < _height; ++y) {

				// One channel each scanline
				output.Write(init, 0, 4); // Init for each
				
				// Read scanline into buffer
				byte[] bufferLine = new byte[_width * 4];
				for (int col = 0; col < _width; col++) {
					pix = GetPixel(col, y).ToRGBE();
					bufferLine[col + 0 * _width] = pix[0];
					bufferLine[col + 1 * _width] = pix[1];
					bufferLine[col + 2 * _width] = pix[2];
					bufferLine[col + 3 * _width] = pix[3];
				}

				//Seperate channels each scanline
				for (int c = 0; c < 4; ++c) {

					// Stuff for runs (consecutive bytes identical)
					int runStart = 0, runLength = 0;
					int runLengthOld = 0, nonrunLength = 0;
					int x = 0;
					
					while (x < _width) {
						// Find the next run (identical consecutive byte) if one exists
						runStart = x;
						runLength = runLengthOld = 0;
						while (runLength < minlen && runStart < _width) {
							runStart += runLength;
							runLengthOld = runLength;
							runLength = 1;
							while ((runStart + runLength < _width) && (runLength < 127) &&
								(bufferLine[runStart + c * _width] == bufferLine[runStart + runLength + c * _width]))
								runLength++;
						}

						// "If data before next big run is a short run then write it as such"
						if (runLengthOld > 1 && (runLengthOld == runStart - x)) {
							output.WriteByte((byte)(128 + runLengthOld));
							output.WriteByte(bufferLine[x + c * _width]);
							x = runStart;
						}

						// Write bytes until next run 
						while (x < runStart) {
							nonrunLength = runStart - x;
							// Clamp to 128 plz
							if (nonrunLength > 128)
								nonrunLength = 128;
							// Write nonrun length
							output.WriteByte((byte)nonrunLength);
							// Write data
							output.Write(bufferLine, x + c * _width, nonrunLength);
							// Increment counter
							x += nonrunLength;
						}

						// Write next run if one was found
						if (runLength >= minlen) {
							// Write run length followed by single byte of repeat val
							output.WriteByte((byte)(128 + runLength));
							output.WriteByte(bufferLine[runStart + c * _width]);
							x += runLength;
						}
					}	
				}
			}

			// convert stream to buffer.
			byte[] bufferOut = new byte [output.Length];
			output.Position = 0; // Otherwise just reads all zeroes
			output.Read(bufferOut, 0, bufferOut.Length);
			return bufferOut;
		}

		//allows the user to save the LDRImage file to a location of their choosing
        public async void SaveFile() {
			// Gets the user to pick the location in which they want to save the file in
            FileSavePicker savePicker = new FileSavePicker();
            savePicker.SuggestedStartLocation = PickerLocationId.Desktop;
            savePicker.FileTypeChoices.Add("Radiance HDR", new List<String>() { ".hdr" });
            savePicker.SuggestedFileName = "HDRImage";
            var outputFile = await savePicker.PickSaveFileAsync();

			if (outputFile == null)
				return;

			// Here's our bytes
			byte[] exporty = ToRGBEBytesRLE(); 
			string header = "#?RADIANCE\n" +
							"#Created with the JacksonTastic Image Processing Algorithm ©2018\n" +
							"EXPOSURE=1.000\nFORMAT=32-bit_rle_rgbe\n\n" +
							"-Y " + _height.ToString() + " +X " + _width.ToString() + "\n";
			byte[] headerBytes = Encoding.ASCII.GetBytes(header.ToCharArray());

			// This is memory intensive
			byte[] buf = new byte[exporty.Length + headerBytes.Length];
			headerBytes.CopyTo(buf, 0);
			exporty.CopyTo(buf, headerBytes.Length);
			
			// Write some bytes
			await FileIO.WriteBytesAsync(outputFile, buf);
        }
	}
}
//----------------------------------------------------------------------------//