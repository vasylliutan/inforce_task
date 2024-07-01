using System.ComponentModel.DataAnnotations.Schema;

namespace Inforce_task.Models
{
    public class Url
    {
        public int id { get; set; }
        public string originalUrl { get; set; }
        public string shortUrl { get; set; }
        public string createdBy { get; set; }
        public DateTime createdDate { get; set; }

        [ForeignKey("createdBy")]
        public User user
        {   get; set;   }
    }
}