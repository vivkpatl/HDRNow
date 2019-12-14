using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PictureLoadingApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HDRFrame : Page
    {
        //stores the value of the slider used to alter the HDR image
        float sliderValue;

        public HDRFrame()
        {
            this.InitializeComponent();

            sliderValue = (float)BlenderSlider.Value;
        }

        //stores the LDR images from the previous page to render the HDR image
        LDRImage underExp, regExp, overExp;

        //stores the rendered HDR image
        HDRImage HDRI;
        LDRImage HDRPreview;

        // WE'll use this to show things like color channels
        LDRImage calculated;

        //initially called when this page is loaded, will recieve the 3 LDR images loaded from the previous page in a List as a parameter
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            //loads the LDR images into the properities of this page
            List<LDRImage> LDRImages = (List<LDRImage>)e.Parameter;
            underExp = LDRImages[0];
            regExp = LDRImages[1];
            overExp = LDRImages[2];

            //Renders the HDR image
            Recalculate(this, null);

            RecalculateButton.IsEnabled = false;
        }

        //Displays the menu for saving the image as different file types
        //private void SaveMenu(object sender, TappedRoutedEventArgs e)
        //{
        //    FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        //}

        //saves the image as a .hdr file
        private void SaveHDR(object sender, RoutedEventArgs e)
        {
            HDRI.SaveFile();
        }

        //saves the image as a .png image
        private async void SavePNG(object sender, RoutedEventArgs e)
        {
            LDRImage HDRPreview = new LDRImage();
            await HDRPreview.LoadFromBytes(HDRI.Transform(), HDRI.Width, HDRI.Height);
            HDRPreview.SaveFile();
        }

        //Called when the play button is pressed
        private async void Recalculate(object sender, TappedRoutedEventArgs e)
        {
            //Re-renders the HDR image based on the new slider value
            HDRI = Blending.Blend(sliderValue / 100f, underExp, regExp, overExp);

            //Creates and displays a preview LDR image based off of the HDR image
            HDRPreview = new LDRImage();
            await HDRPreview.LoadFromBytes(HDRI.Transform(), HDRI.Width, HDRI.Height);
            HDRImage.Source = HDRPreview.GetBitmapImage();

            RecalculateButton.IsEnabled = false;
        }

        //When the slider is changed, update the sliderValue property to match it
        private void BlendChange(object sender, RangeBaseValueChangedEventArgs e)
        {
            sliderValue = (float)BlenderSlider.Value;

            RecalculateButton.IsEnabled = true;
        }

        // Show the regular old HDR
        private void ViewHDR(object sender, TappedRoutedEventArgs e)
        {
            SaveButton.IsEnabled = true;
            RecalculateButton.IsEnabled = true;
            BlenderSlider.IsEnabled = true;

            // Loads HDR Preview from cache
            HDRImage.Source = HDRPreview.GetBitmapImage();
        }
        private void ViewAsRed(object sender, TappedRoutedEventArgs e)
        {
            ShowChannel(0);
        }
        private void ViewAsGreen(object sender, TappedRoutedEventArgs e)
        {
            ShowChannel(1);
        }
        private void ViewAsBlue(object sender, TappedRoutedEventArgs e)
        {
            ShowChannel(2);
        }

        /// <summary>
        /// Shows a specified channel in the loaded <see cref="HDRI"/>
        /// </summary>
        /// <param name="channel">I think 0 is red?</param>
        private async void ShowChannel(int channel)
        {
            SaveButton.IsEnabled = false;
            RecalculateButton.IsEnabled = false;
            BlenderSlider.IsEnabled = false;

            if (HDRI == null) return;
            calculated = new LDRImage();
            await calculated.LoadFromBytes(HDRI.TransformChannel(channel), HDRI.Width, HDRI.Height);
            ShowCalculated();
        }

        private void ShowCalculated()
        {
            if (calculated != null)
                HDRImage.Source = calculated.GetBitmapImage();
        }
    }
}
