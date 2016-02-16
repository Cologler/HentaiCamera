using System;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace HentaiCamera
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        private void CameraImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var sc = e.NewSize.Width / 360;
            var rect = new Rect(
                -28 * sc,
                142 * sc,
                160 * sc,
                100 * sc);
            this.CameraContentClip.Rect = rect;
            this.Render();
        }

        private void Render()
        {
            var winWidth = Window.Current.Bounds.Width;
            var winHeight = Window.Current.Bounds.Height;
            var widthOffset = (winWidth - this.BackgroundImage.ActualWidth) / 2;
            var heigthOffset = (winHeight - this.BackgroundImage.ActualHeight) / 2;
            var xOffset = this.CameraImageCompositeTransform.TranslateX - widthOffset;
            var yOffset = this.CameraImageCompositeTransform.TranslateY - heigthOffset;
            this.CameraContentImageCompositeTransform.TranslateX = xOffset / 10;
            this.CameraContentImageCompositeTransform.TranslateY = yOffset / 10;
        }

        private void CameraImage_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            this.CameraImageCompositeTransform.TranslateX += e.Delta.Translation.X;
            this.CameraImageCompositeTransform.TranslateY += e.Delta.Translation.Y;
            this.CameraContentClipCompositeTransform.TranslateX += e.Delta.Translation.X;
            this.CameraContentClipCompositeTransform.TranslateY += e.Delta.Translation.Y;
            this.Render();
            e.Handled = true;
        }

        private void CameraImage_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            var p = e.GetCurrentPoint(this);
            var scale = p.Properties.MouseWheelDelta > 0
                ? this.CameraImageCompositeTransform.ScaleX *= 0.9
                : this.CameraImageCompositeTransform.ScaleX *= 1.1;
            scale = Math.Max(0.3, scale);
            scale = Math.Min(1.5, scale);
            this.CameraImageCompositeTransform.ScaleX = scale;
            this.CameraImageCompositeTransform.ScaleY = scale;
            this.CameraContentClipCompositeTransform.ScaleX = scale;
            this.CameraContentClipCompositeTransform.ScaleY = scale;
            e.Handled = true;
        }

        private async void BuildButton_OnClick(object sender, RoutedEventArgs e)
        {
            var bitmap = new RenderTargetBitmap();
            await bitmap.RenderAsync(this.ContentGrid);
            var pixelBuffer = await bitmap.GetPixelsAsync();
            var name = DateTime.Now.ToString("yyyy-MM-dd hh-mm-ss") + ".jpg";
            var file = await KnownFolders.SavedPictures.CreateFileAsync(name, CreationCollisionOption.GenerateUniqueName);
            using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                var dpi = DisplayInformation.GetForCurrentView().LogicalDpi;
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied,
                    (uint)bitmap.PixelWidth, (uint)bitmap.PixelHeight, dpi, dpi,
                    pixelBuffer.ToArray());
                await encoder.FlushAsync();
            }
        }

        private async void OpenButton_OnClick(object sender, RoutedEventArgs e)
        {
            var filePicker = new FileOpenPicker()
            {
                FileTypeFilter = { ".jpg", ".jpeg", ".png" }
            };
            var file = await filePicker.PickSingleFileAsync();
            if (file == null) return;
            var bitmap = new BitmapImage();
            await bitmap.SetSourceAsync(await file.OpenReadAsync());
            this.BackgroundImage.Source = bitmap;
            this.CameraContentImage.Source = bitmap;
            this.Render();
        }
    }
}
