/*----------------------------------------------------------------------------//
 * BlendCalculator.cs
 * Callum Walker
 * February 2018
 * Does blending operations for a specific part of an image???
 * May be merged into Blending eventually
//----------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//----------------------------------------------------------------------------//
namespace PictureLoadingApp {
	/// <summary>
	/// Does the blend calculations for a portion of an image
	/// </summary>
	class BlendCalculator {

		struct PixelData {
			public byte[] bytes;
			public int width;
			public float exposure;
		}

		public float[] Calculate(params byte[][] imgs) {
			return new float[4];
		}

	}
}
