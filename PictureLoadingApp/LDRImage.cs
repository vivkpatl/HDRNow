/*
 Jackson Frank
 LDRImageFile.cs
 Class for seperate images used in the HDR blending
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using System.IO;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;

namespace PictureLoadingApp
{
    class LDRImage
    {
        //Properties
        private BitmapImage image;
        private SoftwareBitmap softMap;
        private byte[] bytes;
        private BitmapDecoder decoder;
        private IRandomAccessStream stream;
        private int width;
        private int height;

        //Constructor
        public LDRImage() {}
 
        public async Task Load()
        {
            //opens the windows file picking window
            var picker = new FileOpenPicker();

            //sets the view to thumbnails in the picking window
            picker.ViewMode = PickerViewMode.Thumbnail;

            picker.SuggestedStartLocation = PickerLocationId.Desktop;

            //filters the files so only these three types of files are shown
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".jpeg");

            
            //awaiting a file to be picked
            StorageFile file = await picker.PickSingleFileAsync();

            //error checking
            if(file == null)
            {
                return;
            }

            stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);

            //creats the bitmapImage from the stream
            image = new BitmapImage();
            image.SetSource(stream);

            //read only stream to initialize the decoder and softwareBitmap
            decoder = await BitmapDecoder.CreateAsync(stream);
            softMap = await decoder.GetSoftwareBitmapAsync();

            //gets the pixel width and height of the image
            width = (int)decoder.PixelWidth;
            height = (int)decoder.PixelHeight;

            //gets the pixel data from the decoder and puts it into a byte array
            var imagePixelData = await decoder.GetPixelDataAsync();
            bytes = imagePixelData.DetachPixelData();
        }

        

        public async Task LoadFromBytes(byte[] bytes, int width, int height)
        {
            this.bytes = bytes;
            this.width = width;
            this.height = height;

            //error checking
            if ((width * height) != (bytes.Length / 4))
            {
                throw new ArgumentException();
            }

			//creates the stream from the byte array

			/*

			stream = new InMemoryRandomAccessStream();
            await stream.WriteAsync(bytes.AsBuffer());
            stream.Seek(0);
            
            //creats the bitmapImage from the stream
            image = new BitmapImage();
            image.SetSource(stream);
			
			*/

			// Uh yeah sure
			stream = new InMemoryRandomAccessStream();
			stream.Size = 0;
			BitmapEncoder encode = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);

			// Set the byte array
			encode.SetPixelData(BitmapPixelFormat.Rgba8, BitmapAlphaMode.Straight,
				(uint)width, (uint)height, 96.0, 96.0,
				bytes);

			// Go into the stream plz
			await encode.FlushAsync();

			image = new BitmapImage();
            image.SetSource(stream);

			//read only stream to initialize the decoder and softwareBitmap
			decoder = await BitmapDecoder.CreateAsync(stream);

			// There's probably a way to do this better...
			softMap = await decoder.GetSoftwareBitmapAsync();
			
        }

        //gets the byte array from the BitmapDecoder
        //
        public async Task LoadFromDecoder(BitmapDecoder decoder)
        {
            this.decoder = decoder;

            //creates the byte array from the decoder
            var imagePixelData = await decoder.GetPixelDataAsync();
            bytes = imagePixelData.DetachPixelData();

            //creates the stream from the byte array
            stream = new InMemoryRandomAccessStream();
            await stream.WriteAsync(bytes.AsBuffer());
            stream.Seek(0);

            //creats the bitmapImage from the stream
            image = new BitmapImage();
            image.SetSource(stream);

            //decoder initializes SoftwareBitmap
            softMap = await decoder.GetSoftwareBitmapAsync();

            //gets the pixel width and height of the image
            width = (int)decoder.PixelWidth;
            height = (int)decoder.PixelHeight;
 
        }
        

        //returns byte array of all RGBA values of the image
        public byte[] GetBytes()
        {
            return bytes;
        }

        //returns the deocoder of the image to check for pixel height, width
        public BitmapDecoder GetDecoder()
        {
            return decoder;
        }

        //returns the BitmapImage of the image for displaying in the UI
        public BitmapImage GetBitmapImage()
        {
            return image;
        }

        //returns the pixel width and height of the image
        public int GetWidth()
        {
            return width;
        }
        public int GetHeight()
        {
            return height;
        }

        

        

        //allows the user to save the LDRImage file to a location of their choosing
        public async void SaveFile()
        {
            //gets the user to pick the location in which they want to save the file in
            FileSavePicker savePicker = new FileSavePicker();
            savePicker.SuggestedStartLocation = PickerLocationId.Desktop;
            savePicker.FileTypeChoices.Add("PNG image", new List<String>() { ".png" });
            savePicker.SuggestedFileName = "LDRImage";
            var outputFile = await savePicker.PickSaveFileAsync();

            if(outputFile == null)
            {
                return;
            }

            using (IRandomAccessStream stream = await outputFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                //creates a bitmapEncoder from the stream from the outputFile chosen
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);

                //sets the softwareBitmap of the encoder to the softwareBitmap of the image
                encoder.SetSoftwareBitmap(softMap);

                //error checking
                try
                {
                    await encoder.FlushAsync();
                }
                catch (Exception err)
                {
                    switch (err.HResult)
                    {
                        case unchecked((int)0x88982F81): //WINCODEC_ERR_UNSUPPORTEDOPERATION
                                                         // If the encoder does not support writing a thumbnail, then try again
                                                         // but disable thumbnail generation.
                            encoder.IsThumbnailGenerated = false;
                            break;
                        default:
                            throw err;
                    }
                }

            }
        }








        ////lets the user pick an image to use and loads it in
        //private async Task LoadImage()
        //{
        //    //opens the windows file picking window
        //    var picker = new FileOpenPicker();

        //    //sets the view to thumbnails in the picking window
        //    picker.ViewMode = PickerViewMode.Thumbnail;

        //    picker.SuggestedStartLocation = PickerLocationId.Desktop;

        //    //filters the files so only these three types of files are shown
        //    picker.FileTypeFilter.Add(".jpg");
        //    picker.FileTypeFilter.Add(".png");
        //    picker.FileTypeFilter.Add(".jpeg");

        //    //awaiting a file to be picked
        //    StorageFile file = await picker.PickSingleFileAsync();
        //    stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);

        //    //creats the bitmapImage from the stream
        //    image = new BitmapImage();
        //    image.SetSource(stream);


        //}
        ////extracts the pixel data from the image for manipulation
        //private async Task ExtractPixelData()
        //{
        //    //read only stream to initialize the decoder and softwareBitmap
        //    decoder = await BitmapDecoder.CreateAsync(stream);
        //    softMap = await decoder.GetSoftwareBitmapAsync();

        //    //gets the pixel width and height of the image
        //    width = (int)decoder.PixelWidth;
        //    height = (int)decoder.PixelHeight;

        //    //gets the pixel data from the decoder and puts it into a byte array
        //    var imagePixelData = await decoder.GetPixelDataAsync();
        //    bytes = imagePixelData.DetachPixelData();

        //}
    }
}
