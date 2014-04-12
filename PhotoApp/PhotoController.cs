using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Drawing;
using Xamarin.Media;
using MonoTouch.CoreImage;

namespace PhotoApp
{
    public class PhotoController : UIViewController
    {
        UIImageView imageView;
        UIButton takePhoto, pickPhoto, applyNoirFilter;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            float w = 200.0f;
            float h = 30.0f;
            float left = View.Center.X - w / 2;

            View.BackgroundColor = UIColor.White;

            imageView = new UIImageView
            {
                Frame = new RectangleF(40, 40, 240, 240),
                ContentMode = UIViewContentMode.ScaleAspectFit
            };

            takePhoto = UIButton.FromType(UIButtonType.System);
            takePhoto.Frame = new RectangleF(left, 300, w, h);
            takePhoto.SetTitle("Take Photo with Camera", UIControlState.Normal);
            takePhoto.TouchUpInside += (sender, e) => TakePhoto();

            pickPhoto = UIButton.FromType(UIButtonType.System);
            pickPhoto.Frame = new RectangleF(left, 340, w, h);
            pickPhoto.SetTitle("Choose Photo from Library", UIControlState.Normal);
            pickPhoto.TouchUpInside += (sender, e) => PickPhoto();

            applyNoirFilter = UIButton.FromType(UIButtonType.System);
            applyNoirFilter.Frame = new RectangleF(left, 380, w, h);
            applyNoirFilter.SetTitle("Apply Noir Filter", UIControlState.Normal);
            applyNoirFilter.TouchUpInside += (sender, e) => ApplyNoirFilter();

            View.AddSubview(imageView);
            View.AddSubview(takePhoto);
            View.AddSubview(pickPhoto);
            View.AddSubview(applyNoirFilter);
        }

        void ApplyNoirFilter()
        {
            var image = imageView.Image;

            if (image != null)
            {
                var noir = new CIPhotoEffectNoir();
                noir.Image = new CIImage(image.CorrectOrientation());

                var ciContext = CIContext.FromOptions(null);
                var output = noir.OutputImage;

                using (image = UIImage.FromImage(ciContext.CreateCGImage(output, output.Extent)))
                {
                    imageView.Image = image;
                }
            }
            else
            {
                new UIAlertView("Image Empty", "Select a photo or take one with the camera", null, "OK").Show();
            }
        }

        async void PickPhoto()
        {
            var picker = new MediaPicker();
            var media = await picker.PickPhotoAsync();
            using (var image = UIImage.FromFile(media.Path))
            {
                imageView.Image = image;
            }
        }

        async void TakePhoto()
        {
            var picker = new MediaPicker();

            if (!picker.IsCameraAvailable || !picker.PhotosSupported)
            {
                new UIAlertView("Camera Unavailable", "This device does not have a camera", null, "OK").Show();
                return;
            }

            var mediaOptions = new StoreCameraMediaOptions
            {
                Name = "temp.png",
                Directory = "temp"
            };

            var media = await picker.TakePhotoAsync(mediaOptions);

            using (var image = UIImage.FromFile(media.Path))
            {
                imageView.Image = image;
            }
        }
    }

    public static class ImageExtensions
    {
        public static UIImage CorrectOrientation(this UIImage image)
        {
            if (image.Orientation == UIImageOrientation.Up)
                return image;

            UIGraphics.BeginImageContextWithOptions(image.Size, false, image.CurrentScale);
            image.Draw(new RectangleF(0, 0, image.Size.Width, image.Size.Height));
            UIImage img = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();
            return img;
        }
    }
}