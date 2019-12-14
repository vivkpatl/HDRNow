/*
 Jackson Frank
 Blending.cs
 Class for the final blended image
 */ 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Storage;

namespace PictureLoadingApp
{
	/// <summary>
	/// Static class that controls the blending operation
	/// </summary>
    static class Blending
    {
		/// <summary>
		/// A list of images
		/// </summary>
		static List<LDRImage> _imgs;

		/// <summary>
		/// Blends images stored in this <see cref="Blending"/> object.
        /// magicSliderValue: Percentage of range? See whiteboard...[0, 1]
		/// </summary>
		public static HDRImage Blend(float magicSliderValue, params LDRImage[] imgs) {
            // Put that array in a list
            //_imgs = new List<LDRImage>(imgs);

            // Init outputs
            // TODO: Get real values for height and width 

            /* 
			
				So yeah here I guess we do the calculations
				I imagine for max multithread perf. we should
				split the pixels up by rows?

			*/
            int n = imgs.Length;//Number of Images Being Used
            byte[][] imageBytes = new byte[n][];//Row: Image Number Column: byte value
            for(int i = 0; i < n; i++)//Place pixels into 2D Array
            {
                imageBytes[i] = imgs[i].GetBytes();
            }

            float[] raw = new float[imageBytes[0].Length];//Output Array


            float[] xValues = new float[n];//Average Brightness of Image #j
            for (int j = 0; j < xValues.Length; j++)
            {
                xValues[j] = AverageBrightness(imageBytes[j]);
            }

            for (int i = 0; i < imgs[0].GetDecoder().PixelWidth * imgs[0].GetDecoder().PixelHeight; i++)//Loop through each Pixel Position
            {
                float[] yValues = new float[n];//Luminance
                //Terms used in Logarithmic Regression
                float sumYLnX = 0;
                float sumY = 0;
                float sumLnX = 0;
                float sum_LnX2 = 0;
                float sumLnX_2 = 0;
                for (int j = 0; j < n; j++)//Find Pixel Values at Position i for each image
                {
                    //Calculate Y Value (Luminance) for each image at Position i
                    //0.299 * R + 0.587 * G + 0.114 * B = Luminance
                    yValues[j] = (float)(0.299 * imageBytes[j][i * 4 + 0] + 0.587 * imageBytes[j][i * 4 + 1] + 0.114 * imageBytes[j][i * 4 + 2]);

                    sumYLnX += (float)yValues[j] * (float)Math.Log(xValues[j]);
                    sumY += yValues[j];
                    sumLnX += (float)Math.Log(xValues[j]);
                    sum_LnX2 += (float)Math.Log(xValues[j]) * (float)Math.Log(xValues[j]);
                }
                sumLnX_2 = sumLnX * sumLnX;

                float b = (n * sumYLnX - sumY * sumLnX) / (n * sum_LnX2 - sumLnX_2);//Coefficient (Vertical Stretch) of Parent Function ln()
                float a = (sumY - b * sumLnX) / n;//Constant term (Vertical Shift)

                //Bounds for Transformation along Curve
                float alpha = xValues[n / 2];
                float beta = xValues[0];
                //Choose either left or right bound based on size of range of Y values
                if (b * Math.Abs(Math.Log(xValues[n / 2]) - Math.Log(xValues[0])) < b * Math.Abs(Math.Log(xValues[xValues.Length - 1]) - Math.Log(xValues[n / 2])))
                {
                    beta = xValues[xValues.Length - 1];
                }
                float gamma = Math.Abs(alpha - beta) * magicSliderValue;//X Value of New Luminance Value for output pixel
                if (beta < alpha)
                {
                    gamma += beta;
                }
                else
                {
                    gamma += alpha;
                }
                float newLuminance = a + b * (float)Math.Log(gamma);//New Luminance for output pixel
                //Calculate chrominance channels from middle image to recover new RGB values
                float cb = 128 + (float)(-0.168736 * imageBytes[n / 2][i * 4] - 0.331264 * imageBytes[n / 2][i * 4 + 1] + 0.5 * imageBytes[n / 2][i * 4 + 2]);
                float cr = 128 + (float)(0.5 * imageBytes[n / 2][i * 4] - 0.418688 * imageBytes[n / 2][i * 4 + 1] - 0.081312 * imageBytes[n / 2][i * 4 + 2]);

                //Recover RGB values
                float newRed = newLuminance + 1.042f * (cr - 128);
                float newGreen = newLuminance - 0.344136f * (cb - 128) - 0.714136f * (cr - 128);
                float newBlue = newLuminance + 1.772f * (cb - 128);

                //Assign to output array
				// Swapped red and blue because it seemed to be flipping those - Callum
                raw[i * 4] = newBlue;
                raw[i * 4 + 1] = newGreen;
                raw[i * 4 + 2] = newRed;
                raw[i * 4 + 3] = imageBytes[n / 2][i * 4 + 3];
            }
			
			// Construct HDR image for final output
			HDRImage output = new HDRImage(imgs[0].GetWidth(), raw);
			return output;
		}

        private static float AverageBrightness(byte[] imagePixels)//Calculate Average Brightness of Image
        {
            float total = 0;
            for(int i = 0; i < imagePixels.Length; i++)
            {
                total += imagePixels[i];
            }
            return total /= imagePixels.Length;
        }
		
		//------------------------------------------------------------------------//

        //blends the 3 different exposures
        //percent: percentage of underexposed pixels in the blend (float from 0 to 1)
        //top: how much to blend in the top layer (float from 0 to 1)
        public static void BlendOld
			(float percent, float top, LDRImage underExp, LDRImage regExp, LDRImage overExp, out byte[] bytes)
        {
			bytes = new Byte[regExp.GetDecoder().PixelHeight * regExp.GetDecoder().PixelWidth * 4];

            //will contain the calculated brightness of all pixels in the underexposed image
            List<float> brightness = new List<float>();

            // calcuates brightness for all pixels in the under exposed image,
            // then stores these brightness values in the brightness list
            for (int i = 0; i < underExp.GetDecoder().PixelHeight; i++)
            {
                for (int g = 0; g < underExp.GetDecoder().PixelWidth; g++)
                {
                    int h = (i * (int)underExp.GetDecoder().PixelWidth + g) * 4;
                    float bright = (float)Math.Sqrt((.299 * (underExp.GetBytes()[h] * underExp.GetBytes()[h]) + .587 * (underExp.GetBytes()[h + 1] * underExp.GetBytes()[h + 1]) + .114 * (underExp.GetBytes()[h + 2] * underExp.GetBytes()[h + 2])));
                    brightness.Add(bright);
					
                }
            }

            //sorts the brightness list from least to greatest
            brightness.Sort();

            // Using the percent argument, find the appropriate value within the
            // sorted brightness array in order to come up with a partitioning 
            // value
            int pixels = (int)underExp.GetDecoder().PixelWidth * (int)underExp.GetDecoder().PixelHeight;
            int partitionIndex = (int)(percent * pixels);
            float partition = brightness[partitionIndex];

            // For each pixel in the overexposed image, find the corresponding underexposed
            // image pixel and use the partitioning value in order to determine whether or 
            // not to replace it with the corresponding overexposed image pixel
            for (int i = 0; i < overExp.GetDecoder().PixelHeight; i++)
            {
                for (int g = 0; g < overExp.GetDecoder().PixelWidth; g++)
                {
                    int h = (i * (int)overExp.GetDecoder().PixelWidth + g) * 4;

                    if (partition < (float)Math.Sqrt((.299 * (underExp.GetBytes()[h] * underExp.GetBytes()[h]) + .587 * (underExp.GetBytes()[h + 1] * underExp.GetBytes()[h + 1]) + .114 * (underExp.GetBytes()[h + 2] * underExp.GetBytes()[h + 2]))))
                    {
                        for (int k = 0; k < 4; k++)
                        {
                            bytes[h + k] = underExp.GetBytes()[h + k];
                        }

                    }
                    else
                    {
                        for (int k = 0; k < 4; k++)
                        {
                            bytes[h + k] = overExp.GetBytes()[h + k];
                        }
                    }
                }
            }

            // Blend the regular exposed image on top of the blended image
            // using the top argument
            for (int i = 0; i < regExp.GetDecoder().PixelHeight; i++)
            {
                for (int g = 0; g < regExp.GetDecoder().PixelWidth; g++)
                {
                    int h = (i * (int)regExp.GetDecoder().PixelWidth + g) * 4;
                    for (int k = 0; k < 3; k++)
                    {
                        float avg = (regExp.GetBytes()[h + k] - bytes[h + k]) * top;
                        bytes[h + k] += (byte)avg;
                    }
                }
            }
        }
		
		/*
			OLDE CODE!!!

        //Properties
        private LDRImage underExp, overExp, regExp;
        private byte[] bytes;

        //Contructor
        public Blending(LDRImage und, LDRImage over, LDRImage reg)
        {
            underExp = und;
            overExp = over;
            regExp = reg;

            bytes = new Byte[regExp.GetDecoder().PixelHeight * regExp.GetDecoder().PixelWidth * 4];

            Blend(.5f, .5f);
        }

        //returns byte array that contains each pixels' RGBA values
        public byte[] GetBytes()
        {
            return bytes;
        }
		*/

		/*
			BAD MULTITHREADING CODE

		// Initialize the tasks
		int threads = 8;
		Task<float[]>[] portions = new Task<float[]>[threads];

		// TODO: ASSIGN PARTS OF IMAGE TO "portions"



		// BUILD THE IMAGE FROM THE RESULTING CALCULATIONS
			
		foreach (Task<float[]> portion in portions) {
			portion.Wait();
			float[] result = portion.Result;
		}

		*/

    }
}
