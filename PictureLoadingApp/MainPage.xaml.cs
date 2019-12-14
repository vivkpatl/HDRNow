/*--------------------------------------------------------//
 MainPage.xaml.cs
 Jackson Frank
 Main C# file for program
 Handles loading images and manipulating pixels
 //-------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Collections;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using Windows.Graphics.Imaging;
using System.Threading;
using Windows.ApplicationModel.Core;
using Windows.UI.ViewManagement;
//using System.Runtime.InteropServices.WindowsRuntime;

namespace PictureLoadingApp
{
    /// <summary>
    /// Use to interact with UI elements
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        //Original images
        LDRImage underExp, overExp, regExp;

		// Hdr Image
		//HDRImage HDRI;
		//LDRImage HDRPreview;

        //Loads underexposed image
        private async void LoadUnderExp(object sender, TappedRoutedEventArgs e)
        {
            underExp = new LDRImage();
            await underExp.Load();
            if (underExp.GetBitmapImage() != null)
            {
                UnderExp.Source = underExp.GetBitmapImage();
                UnderExpText.Opacity = 0.0;

                //activates the HDR rendering button if the other two images are loaded as well
                if (regExp != null && overExp != null)
                {
                    HDRButton.IsEnabled = true;
                }
            }

            

        }
        //Loads overexposed image
        private async void LoadOverExp(object sender, TappedRoutedEventArgs e)
        {
            overExp = new LDRImage();
            await overExp.Load();
            if(overExp.GetBitmapImage() != null)
            {
                OverExp.Source = overExp.GetBitmapImage();
                OverExpText.Opacity = 0.0;

                //activates the HDR rendering button if the other two images are loaded as well
                if (regExp != null && underExp != null)
                {
                    HDRButton.IsEnabled = true;
                }
            }
        }
        //Loads normally exposed image
        private async void LoadRegExp(object sender, TappedRoutedEventArgs e)
        {
            //loads in the normally exposed image
            regExp = new LDRImage();
            await regExp.Load();
            if (regExp.GetBitmapImage() != null)
            {
                RegExp.Source = regExp.GetBitmapImage();
                RegExpText.Opacity = 0.0;

                //activates the HDR rendering button if the other two images are loaded as well
                if (overExp != null && underExp != null)
                {
                    HDRButton.IsEnabled = true;
                }
            }
        }

        //Loads the HDR image, opens a seperate window
        private async void LoadHDR(object sender, TappedRoutedEventArgs e)
        {
            if (underExp != null && overExp != null && regExp != null)
            {
                /*
                // Hope for the best
                HDRI = Blending.Blend(0.5f, underExp, regExp, overExp);

                // How do async
                HDRPreview = new LDRImage();
                await HDRPreview.LoadFromBytes(HDRI.Transform(), HDRI.Width, HDRI.Height);
                Blend.Source = HDRPreview.GetBitmapImage();
                */

                // Delete old RGB channels
                //channels = null;

                LoadingText.Text = "Loading...";
                

                //puts the LDR images into a list to pass as a parameter to the HDR image display window
                List<LDRImage> LDRImages = new List<LDRImage>();
                LDRImages.Add(underExp);
                LDRImages.Add(regExp);
                LDRImages.Add(overExp);

                //creates a new window that will display the HDR image, and allow the user to manipulate and save it
                CoreApplicationView newView = CoreApplication.CreateNewView();
                int newViewID = 0;
                await newView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    Frame frame = new Frame();
                    frame.Navigate(typeof(HDRFrame), LDRImages);
                    Window.Current.Content = frame;
                    Window.Current.Activate();

                    newViewID = ApplicationView.GetForCurrentView().Id;
                });
                bool viewShown = await ApplicationViewSwitcher.TryShowAsStandaloneAsync(newViewID);

                LoadingText.Text = "Loaded";
            }
        }








































































        /*
        ====================================================================================
        ============================CODE GRAVEYARD DON'T DELETE=============================
        ====================================================================================

        // TEST
		float magicSlider;

        private async void TestHDR (object sender, TappedRoutedEventArgs e)
        {
			// Hope for the best
			HDRI = new HDRImage(256, 128);
			HDRI.Randomize();

			// How do async
			HDRPreview = new LDRImage();
			await HDRPreview.LoadFromBytes( HDRI.Transform(), HDRI.Width, HDRI.Height );
			Blend.Source = HDRPreview.GetBitmapImage();
        }

        private void DispImgs(object sender, TappedRoutedEventArgs e)
        {
            // Reload images into frames
            if (underExp != null)
                UnderExp.Source = underExp.GetBitmapImage();
            if (regExp != null)
                RegExp.Source = regExp.GetBitmapImage();
            if (overExp != null)
                OverExp.Source = overExp.GetBitmapImage();
        }


        LDRImage[] channels;
        private async void DispRGB(object sender, TappedRoutedEventArgs e)
        {
            // No HDR? No show
            if (HDRPreview == null || HDRI == null)
                return;

            if (channels == null)
            {
                channels = new LDRImage[3];

                // R G AND B
                channels[0] = new LDRImage();
                channels[1] = new LDRImage();
                channels[2] = new LDRImage();

                await channels[0].LoadFromBytes(HDRI.TransformChannel(0), HDRI.Width, HDRI.Height);
                await channels[1].LoadFromBytes(HDRI.TransformChannel(1), HDRI.Width, HDRI.Height);
                await channels[2].LoadFromBytes(HDRI.TransformChannel(2), HDRI.Width, HDRI.Height);
            }

            UnderExp.Source = channels[0].GetBitmapImage();
            RegExp.Source = channels[1].GetBitmapImage();
            OverExp.Source = channels[2].GetBitmapImage();

        }


        //when the left slider is changed
		private void BlendChange(object sender, RangeBaseValueChangedEventArgs e)
        {
			//magicSlider = (float)BlenderSlider.Value;
            
        }
        //when the right slider is changed
        private void TopChange(object sender, RangeBaseValueChangedEventArgs e)
        {
            
        }

        // Saves the LDR version of the HDR
        private void SaveLDR(object sender, TappedRoutedEventArgs e)
        {
			if (HDRPreview != null) {
				HDRPreview.SaveFile();
			}
        }

		// Saves that HDR
		private void SaveHDR(object sender, TappedRoutedEventArgs e)
        {
			if (HDRI != null) {
				HDRI.SaveFile();
			}
        }






        /*-------------------------------------------------//
        DO NOT DELETE ANYTHING PAST THIS LINE
        //-------------------------------------------------*/




        //MICHAEL NORRIS LINE 171


        //enum Exp { UNDER, OVER, REG };
        //byte[][] bytesHDR = new byte[3][];
        //BitmapDecoder[] HDRDecoder = new BitmapDecoder[3];
        //WriteableBitmap wbBlend;
        //byte[] Manip;
        //private async void HDR(float percent, float top)
        //{


        //    List<float> brightness = new List<float>();


        //    for (int i = 0; i < HDRDecoder[(int)Exp.UNDER].PixelHeight; i++)
        //    {
        //        for (int g = 0; g < HDRDecoder[(int)Exp.UNDER].PixelWidth; g++)
        //        {
        //            int h = (i * (int)HDRDecoder[(int)Exp.UNDER].PixelWidth + g) * 4;
        //            float bright = (float)Math.Sqrt((.299 * (bytesHDR[(int)Exp.UNDER][h] * bytesHDR[(int)Exp.UNDER][h]) + .587 * (bytesHDR[(int)Exp.UNDER][h + 1] * bytesHDR[(int)Exp.UNDER][h + 1]) + .114 * (bytesHDR[(int)Exp.UNDER][h + 2] * bytesHDR[(int)Exp.UNDER][h + 2])));
        //            brightness.Add(bright);


        //        }
        //    }

        //    brightness.Sort();

        //    int pixels = (int)HDRDecoder[(int)Exp.UNDER].PixelWidth * (int)HDRDecoder[(int)Exp.UNDER].PixelHeight;
        //    int partitionIndex = (int)(percent * pixels);
        //    float partition = brightness[partitionIndex];


        //    for (int i = 0; i < HDRDecoder[(int)Exp.OVER].PixelHeight; i++)
        //    {

        //        for (int g = 0; g < HDRDecoder[(int)Exp.OVER].PixelWidth; g++)
        //        {
        //            int h = (i * (int)HDRDecoder[(int)Exp.OVER].PixelWidth + g) * 4;

        //            if (partition < (float)Math.Sqrt((.299 * (bytesHDR[(int)Exp.UNDER][h] * bytesHDR[(int)Exp.UNDER][h]) + .587 * (bytesHDR[(int)Exp.UNDER][h + 1] * bytesHDR[(int)Exp.UNDER][h + 1]) + .114 * (bytesHDR[(int)Exp.UNDER][h + 2] * bytesHDR[(int)Exp.UNDER][h + 2]))))
        //            {
        //                for (int k = 0; k < 4; k++)
        //                {
        //                    Manip[h + k] = bytesHDR[(int)Exp.UNDER][h + k];
        //                }

        //            }
        //            else
        //            {
        //                for (int k = 0; k < 4; k++)
        //                {
        //                    Manip[h + k] = bytesHDR[(int)Exp.OVER][h + k];
        //                }
        //            }
        //        }
        //    }

        //    //float topLayer = .5f;

        //    for (int i = 0; i < HDRDecoder[(int)Exp.REG].PixelHeight; i++)
        //    {
        //        for (int g = 0; g < HDRDecoder[(int)Exp.REG].PixelWidth; g++)
        //        {
        //            int h = (i * (int)HDRDecoder[(int)Exp.REG].PixelWidth + g) * 4;
        //            for (int k = 0; k < 3; k++)
        //            {
        //                float avg = (bytesHDR[(int)Exp.REG][h + k] - Manip[h + k]) * top;
        //                Manip[h + k] += (byte)avg;
        //            }
        //        }
        //    }

        /*------------------------------------------------------//
                MICHAEL NORRIS LOOK HERE
        //------------------------------------------------------*/
        //    using (Stream stream4 = wbBlend.PixelBuffer.AsStream())
        //    {
        //      await stream4.WriteAsync(Manip, 0, Manip.Length);
        //    }

        //    Blend.Source = wbBlend;


        //    _percent = percent;
        //    _top = top;
        //    BlenderSlider.Value = percent;
        //    TopBlenderSlider.Value = top;

        //}

        //private async void LoadHDRImages(int i)
        //{
        //    var picker = new FileOpenPicker();

        //    picker.ViewMode = PickerViewMode.Thumbnail;

        //    picker.SuggestedStartLocation = PickerLocationId.Desktop;

        //    picker.FileTypeFilter.Add(".jpg");
        //    picker.FileTypeFilter.Add(".png");
        //    picker.FileTypeFilter.Add(".jpeg");

        //    StorageFile file = await picker.PickSingleFileAsync();

        //    if (file != null)

        //    {
        //        var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);

        //        BitmapImage image = new BitmapImage();

        //        image.SetSource(stream);

        //        switch (i)
        //        {
        //            case 0:
        //                UnderExp.Source = image;
        //                break;
        //            case 1:
        //                OverExp.Source = image;
        //                break;
        //            case 2:
        //                RegExp.Source = image;
        //                break;
        //        }

        //        var imageStream = await file.OpenStreamForReadAsync();
        //        HDRDecoder[i] = await BitmapDecoder.CreateAsync(imageStream.AsRandomAccessStream());
        //        var imagePixelData = await HDRDecoder[i].GetPixelDataAsync();
        //        bytesHDR[i] = imagePixelData.DetachPixelData();
        //    }
        //}
    }
}
