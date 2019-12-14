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
//----------------------------------------------------------------------------//
namespace PictureLoadingApp {
	/// <summary>
	/// Represents an individual pixel.
	/// Uses a lot of RAM
	/// </summary>
	struct HDRPixel {

		// The HDR pixel has four channels,
		// although we probably won't use alpha much.
		private float _r, _g, _b, _a;

		/// <summary> Red value of this <see cref="HDRPixel"/>. </summary>
		public float Red { get { return _r; } }
		
		/// <summary> Green value of this <see cref="HDRPixel"/>. </summary>
		public float Green { get { return _g; } }
		
		/// <summary> Blue value of this <see cref="HDRPixel"/>. </summary>
		public float Blue { get { return _b; } }
		
		/// <summary> Alpha value of this <see cref="HDRPixel"/>. </summary>
		public float Alpha { get { return _a; } }
		
		/// <summary>
		/// Create an <see cref="HDRPixel"/>.
		/// </summary>
		/// <param name="r">Red.</param>
		/// <param name="g">Green.</param>
		/// <param name="b">Blue.</param>
		/// <param name="a">Alpha.</param>
		public HDRPixel(float r, float g, float b, float a = 1.0f) {
			// Some extremely complicated stuff
			_r = r;
			_g = g;
			_b = b;
			_a = a;
		}

		/// <summary>
		/// Returns a representation of this <see cref="HDRPixel"/>
		/// using the 32-bit "RGB + exponent" format.
		/// (Discards alpha channel)
		/// </summary>
		public byte[] ToRGBE() {
			// RGBA bytes
			byte[] rgbe = new byte[4];

			// From: https://www.graphics.cornell.edu/~bjw/rgbe.html
			int exponent = 0;
			float v = Red; // Normalizing factor

			// I actually have no idea what this does
			if (Green > v) v = Green;
			if (Blue > v) v = Blue;
			
			if (v < 1E-32) {
				// Exponent is very small
				rgbe[0] = rgbe[1] = rgbe[2] = rgbe[3] = 0;
				
			} else {
				// Exponent calculation
				// https://en.cppreference.com/w/cpp/numeric/math/frexp
				exponent = (v == 0) ? 0 : (int)(1 + Math.Log(v, 2.0));
				v = v * (float)Math.Pow(2.0, -exponent) * (256f / v);

				// Here are the BYTES
				rgbe[0] = (byte)(v * Red);
				rgbe[1] = (byte)(v * Green);
				rgbe[2] = (byte)(v * Blue);
				rgbe[3] = (byte)(exponent + 128);
			}

			// Byte array to int
			//return (rgbe[0] << 24) + (rgbe[1] << 16) + (rgbe[2] << 8) + (rgbe[3]);

			// LOL JK
			return rgbe;
		}

	}
}
