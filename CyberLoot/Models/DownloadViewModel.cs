namespace CyberLoot.Models
{
    // ViewModel to pass data to the Downloads view
    public class DownloadViewModel
    {
        public Product CurrentDownload { get; set; }
        public List<Product> Queue { get; set; }
    }
}
