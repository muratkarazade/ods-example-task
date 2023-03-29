using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ODS_Example_Task.Models
{
    public class City
    {
        [Key]
        public int CityID { get; set; }
        public string? CityName { get; set; }
        public int? PlateCode { get; set; }
        public Region Regions { get; set; }
        [ForeignKey("RegionID")]
        public int? RegionID { get; set; }
    }
}
