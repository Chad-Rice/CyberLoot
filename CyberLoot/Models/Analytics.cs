// Models/Analytics.cs
using System;

namespace CyberLoot.Models
{
    public class Analytics
    {
        public int Id { get; set; }
        public string UserId { get; set; } = "";
        public DateTime VisitDate { get; set; }
        public string PageVisited { get; set; } = "";
    }
}
