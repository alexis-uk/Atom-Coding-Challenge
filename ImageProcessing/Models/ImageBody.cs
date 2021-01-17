using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ImageProcessing.Models
{
    public class ImageBody
    {
        [Required]
        public string Name { get; set; }
        public string Format { get; set; }
        public string Watermark { get; set; }
        public string BackgroundColor { get; set; }
        public int? Height { get; set; }
        public int? Width { get; set; }
    }
}
