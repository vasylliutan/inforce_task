using System.ComponentModel.DataAnnotations;

namespace Inforce_task.ViewModels
{


    namespace Inforce_task.ViewModels
    {
        public class UrlViewModel
        {
            [Required]
            [Url]
            [Display(Name = "Original URL")]
            public string OriginalUrl { get; set; }

            //[Required]
            //[Display(Name = "Short URL")]
            //public string ShortUrl { get; set; }
        }
    }

}
