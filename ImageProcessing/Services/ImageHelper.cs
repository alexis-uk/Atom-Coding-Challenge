using ImageMagick;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace ImageProcessing.Services
{
    public class ImageHelper
    {
        public static Bitmap WatermarkImage(Bitmap image, Bitmap watermark)
        {
            using (Graphics imageGraphics = Graphics.FromImage(image))
            {
                watermark.SetResolution(imageGraphics.DpiX, imageGraphics.DpiY);

                int x = (image.Width - watermark.Width) / 2;
                int y = (image.Height - watermark.Height) / 2;

                imageGraphics.DrawImage(watermark, x, y, watermark.Width, watermark.Height);
            }

            return image;
        }

        public static Image AddWatermark(Image img, string text)
        {
            Image image;
            var watermarkedStream = new MemoryStream();
            using (var graphic = Graphics.FromImage(img))
            {
                FontFamily fontFamily = new FontFamily("Arial");
                Font font = new Font(
                   fontFamily,
                   20,
                   FontStyle.Regular,
                   GraphicsUnit.Pixel);
                var color = Color.FromArgb(200, 255, 255, 255);
                var brush = new SolidBrush(color);
                var point = new Point(img.Width/2, img.Height/2);
                StringFormat sf = new StringFormat();
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;
                graphic.DrawString(text, font, brush, point,sf);
                img.Save(watermarkedStream, ImageFormat.Png);
                image = Image.FromStream(watermarkedStream);
            }

            return image;
        }

        public static Stream ConvertTo(Stream imgStream, ImageFormat imageFormat)
        {
            var image = Image.FromStream(imgStream);
            var stream = new System.IO.MemoryStream();
            image.Save(stream, imageFormat);
            stream.Position = 0;
            return stream;
        }

        public static Image ConvertTo(Image image, ImageFormat imageFormat)
        {
            var stream = new System.IO.MemoryStream();
            image.Save(stream, imageFormat);
            return Image.FromStream(stream);
        }

        public static string GetMimeType(Image i)
        {
            var imgguid = i.RawFormat.Guid;
            foreach (ImageCodecInfo codec in ImageCodecInfo.GetImageDecoders())
            {
                if (codec.FormatID == imgguid)
                    return codec.MimeType;
            }
            return "image/unknown";
        }

        public static Image Resize(Image image, int newWidth, int maxHeight, bool onlyResizeIfWider)
        {

            if (onlyResizeIfWider && image.Width <= newWidth) newWidth = image.Width;

            var newHeight = image.Height * newWidth / image.Width;
            if (newHeight > maxHeight)
            {
                // Resize with height instead  
                newWidth = image.Width * maxHeight / image.Height;
                newHeight = maxHeight;
            }

            var res = new Bitmap(newWidth, newHeight);

            using (var graphic = Graphics.FromImage(res))
            {
                graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphic.SmoothingMode = SmoothingMode.HighQuality;
                graphic.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphic.CompositingQuality = CompositingQuality.HighQuality;
                graphic.DrawImage(image, 0, 0, newWidth, newHeight);
            }

            return res;
        }

        public static Image Resize(Image image, Size size, bool preserveAspectRatio = true)
        {
            int newWidth;
            int newHeight;
            if (preserveAspectRatio)
            {
                int originalWidth = image.Width;
                int originalHeight = image.Height;
                float percentWidth = (float)size.Width / (float)originalWidth;
                float percentHeight = (float)size.Height / (float)originalHeight;
                float percent = percentHeight < percentWidth ? percentHeight : percentWidth;
                newWidth = (int)(originalWidth * percent);
                newHeight = (int)(originalHeight * percent);
            }
            else
            {
                newWidth = size.Width;
                newHeight = size.Height;
            }
            Image newImage = new Bitmap(newWidth, newHeight);
            using (Graphics graphicsHandle = Graphics.FromImage(newImage))
            {
                graphicsHandle.SmoothingMode = SmoothingMode.AntiAlias;
                graphicsHandle.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphicsHandle.DrawImage(image, 0, 0, newWidth, newHeight);
            }
            return newImage;
        }

        public static Image Resize(Image image, int newWidth, int newHeight)
        {
            using (var img = new MagickImage(ImageToByteArray(image)))
            {
                img.Format = MagickFormat.Png;
                img.Resize(newWidth, newHeight);
                img.Strip();
                img.Quality = 85;
                img.BackgroundColor = new MagickColor("#000000");
                return byteArrayToImage(img.ToByteArray());
            }
        }

        public static Image byteArrayToImage(byte[] byteArrayIn)
        {
            using (var ms = new MemoryStream(byteArrayIn))
            {
                return Image.FromStream(ms);
            }
        }

        public static byte[] ImageToByteArray(System.Drawing.Image imageIn)
        {
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, ImageFormat.Jpeg);
            return ms.ToArray();
        }

        public static Image ChangeColor(Image image, Color color)
        {
            
            Bitmap bmp = new Bitmap(image.Width, image.Height);

            using (Graphics grap = Graphics.FromImage(bmp))
            {
                grap.Clear(color);
                grap.InterpolationMode = InterpolationMode.NearestNeighbor;
                grap.PixelOffsetMode = PixelOffsetMode.None;
                grap.DrawImage(image, Point.Empty);
            }

            return image;
        }

        public static Image ChangeBackgroundColor(Image bmp1, Color target)
        {
            Bitmap bmp2 = new Bitmap(bmp1.Width, bmp1.Height);
            Rectangle rect = new Rectangle(Point.Empty, bmp1.Size);
            using (Graphics G = Graphics.FromImage(bmp2))
            {
                G.Clear(target);
                G.DrawImageUnscaledAndClipped(bmp1, rect);
            }
            return bmp2;
        }

    }
}
