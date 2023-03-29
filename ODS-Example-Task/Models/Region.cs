using System.ComponentModel.DataAnnotations;

namespace ODS_Example_Task.Models
{
    public class Region
    {
        [Key]
        public int RegionID { get; set; }
        public string? RegionName { get; set; }
        public bool? SafeDelete { get; set; }
        public bool? HardDelete { get; set; }
        public int? CityCount { get; set; }
        public DateTime? DeletedDate { get; set; }
        public IEnumerable<Region> RegionsList { get; set; }
    }
}
