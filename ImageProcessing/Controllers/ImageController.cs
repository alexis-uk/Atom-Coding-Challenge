using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImageProcessing.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO.Compression;
using Microsoft.Extensions.Configuration;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : ControllerBase
    {

        private readonly IMemoryCache cache;
        private string imagesZipFile;


        public ImageController(IMemoryCache memoryCache, IConfiguration configuration)
        {
            cache = memoryCache;
            imagesZipFile = configuration["ImagesZipFile"]; ;
        }

        [HttpGet]
        public ActionResult<IActionResult> Get([FromBody] ImageProcessing.Models.ImageBody imageBody)
        {
            string jsonString = JsonSerializer.Serialize(imageBody);
            try
            {
                if (!cache.TryGetValue(jsonString, out Image image))
                {
                    image=ProcessRequest(imageBody);
                    cache.Set(jsonString, image, TimeSpan.FromMinutes(60));
                }
                return File(ImageHelper.ImageToByteArray(image), ImageHelper.GetMimeType(image));
            }
            catch (Exception)
            {
                return BadRequest(imageBody);
            }
        }

        private Image ProcessRequest(ImageProcessing.Models.ImageBody imageBody) {
            using (ZipArchive archive = ZipFile.OpenRead(imagesZipFile))
            {
                ZipArchiveEntry entry = archive.GetEntry($"product_images/{imageBody.Name}");

                var imageStream = entry.Open();
                var image = Image.FromStream(imageStream);

                if (!string.IsNullOrEmpty(imageBody.BackgroundColor))
                {
                    image = ImageHelper.ChangeBackgroundColor(image, ColorTranslator.FromHtml(imageBody.BackgroundColor));
                }
                if (!string.IsNullOrEmpty(imageBody.Watermark))
                {
                    image = ImageHelper.AddWatermark(image, imageBody.Watermark);
                }
                if (imageBody.Height != null && imageBody.Width != null)
                {
                    image = ImageHelper.Resize(image, imageBody.Width.Value, imageBody.Height.Value);
                }
                switch (imageBody.Format.ToUpper())
                {
                    case "JPG":
                        image = ImageHelper.ConvertTo(image, ImageFormat.Jpeg);
                        break;
                    case "TIFF":
                        image = ImageHelper.ConvertTo(image, ImageFormat.Tiff);
                        break;
                    case "GIF":
                        image = ImageHelper.ConvertTo(image, ImageFormat.Gif);
                        break;
                    case "BMP":
                        image = ImageHelper.ConvertTo(image, ImageFormat.Bmp);
                        break;
                    default:
                        image = ImageHelper.ConvertTo(image, ImageFormat.Png);
                        break;
                }
                return image; 
            }
        }
    }
}
