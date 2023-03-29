using Microsoft.AspNetCore.Mvc;
using ODS_Example_Task.Models;
using System.Data.SqlClient;

namespace ODS_Example_Task.Controllers
{
    public class RegionsController : Controller
    {

        private readonly IConfiguration _configuration;


        public RegionsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            List<Region> regionsList = await GetAllRegionsAsync();
            return View(regionsList);
        }


        /// <summary>
        /// Tüm bölgeleri gösteren metot
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<List<Region>> GetAllRegionsAsync()
        {
            List<Region> regionsList = new List<Region>();
            try
            {
                string connectionStr = _configuration.GetConnectionString("DefaultConnection");
                string commandStr = "SELECT * FROM dbo.Region";
                using (SqlConnection con = new SqlConnection(connectionStr))
                {
                    using (SqlCommand cmd = new SqlCommand(commandStr, con))
                    {
                        await con.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            if (reader.HasRows)
                            {
                                while (await reader.ReadAsync())
                                {
                                    Region regions = new Region();
                                    regions.RegionID = await reader.IsDBNullAsync(0) ? 0 : reader.GetInt32(0);
                                    regions.RegionName = await reader.IsDBNullAsync(1) ? default(string) : reader.GetString(1);
                                    regionsList.Add(regions);

                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
            return regionsList;
        }

       

        public IActionResult Create()
        {
            return View();
        }
        /// <summary>
        /// Yeni bölge ekleme
        /// </summary>
        /// <param name="region"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<IActionResult> CreateRegion(Region region)
        {
            bool success = false;
            try
            {
                if (region == null) return BadRequest();
                string regionNameValue = region.RegionName;
                var existRegion =  await CheckRegionNameExistsAsync(regionNameValue);
                if (existRegion == true) return BadRequest("Belirtilen Bölge Kayıtlı");
                var regionCount = await GetRegionCountAsync();
                if(regionCount >= 7)
                {
                    return BadRequest("Bölge Sayısı 7 den fazla olamaz");
                }
                string connectionStr = _configuration.GetConnectionString("DefaultConnection");
                string commandStr = "INSERT INTO dbo.Region ( RegionName) VALUES ( @RegionName)"; ;
                using(SqlConnection con = new SqlConnection(connectionStr))
                {
                    using(SqlCommand cmd = new SqlCommand(commandStr, con))
                    {
                       
                        cmd.Parameters.AddWithValue("@RegionName", region.RegionName ?? (object)DBNull.Value);
                        await con.OpenAsync();
                        await cmd.ExecuteNonQueryAsync(); success = true;
                    }
                }
                return Redirect("/Regions/Index");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }            
        }

        /// <summary>
        /// Bölge adı kontolü yapan metot
        /// </summary>
        /// <param name="regionName"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> CheckRegionNameExistsAsync(string regionName)
        {
            try
            {
                string connectionStr = _configuration.GetConnectionString("DefaultConnection");
                string commandStr = "SELECT COUNT(*) FROM dbo.Region WHERE RegionName = @RegionName";
                using (SqlConnection con = new SqlConnection(connectionStr))
                {
                    using (SqlCommand cmd = new SqlCommand(commandStr, con))
                    {
                        cmd.Parameters.AddWithValue("@RegionName", regionName);
                        await con.OpenAsync();
                        int count = (int)await cmd.ExecuteScalarAsync();
                        if (count > 0)
                        {
                            return true;
                        }
                        else return false;                       
                    }
                    
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        /// <summary>
        /// Silme İşlemi
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<IActionResult> Delete(int id)
        {
            bool success = false;
            try
            {
                if (id == null) return BadRequest();
                string connectionStr = _configuration.GetConnectionString("DefaultConnection");
                string commandStr = "DELETE FROM dbo.Region WHERE RegionID=@id"; ;
                using (SqlConnection con = new SqlConnection(connectionStr))
                {
                    using (SqlCommand cmd = new SqlCommand(commandStr, con))
                    {

                        cmd.Parameters.AddWithValue("@id", id);
                        await con.OpenAsync();
                        await cmd.ExecuteNonQueryAsync(); success = true;
                    }
                }
                return Redirect("/Regions/Index");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Bölge Sayısı
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<int> GetRegionCountAsync()
        {
            int count = 0;
            try
            {
                string connectionStr = _configuration.GetConnectionString("DefaultConnection");
                string commandStr = "SELECT COUNT(*) FROM dbo.Region";
                using (SqlConnection con = new SqlConnection(connectionStr))
                {
                    using (SqlCommand cmd = new SqlCommand(commandStr, con))
                    {
                        await con.OpenAsync();
                        count = (int)await cmd.ExecuteScalarAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return count;
        }

        /// <summary>
        /// Bölgelere göre şehir sayısı getiren metot
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<IActionResult> GetCityCountByRegionAsync()
        {
            List<CityRegionCount> regionList = new List<CityRegionCount>();
            try
            {
                string connectionStr = _configuration.GetConnectionString("DefaultConnection");
                string commandStr = "SELECT r.RegionID, r.RegionName, COUNT(c.CityID) AS CityCount " +
                                    "FROM dbo.Region r " +
                                    "LEFT JOIN dbo.City c ON r.RegionID = c.RegionID " +
                                    "GROUP BY r.RegionID, r.RegionName";
                using (SqlConnection con = new SqlConnection(connectionStr))
                {
                    using (SqlCommand cmd = new SqlCommand(commandStr, con))
                    {
                        await con.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            if (reader.HasRows)
                            {
                                while (await reader.ReadAsync())
                                {
                                    var region = new CityRegionCount
                                    {
                                        RegionID = reader.GetInt32(0),
                                        RegionName = reader.GetString(1),
                                        CityCount = reader.GetInt32(2)
                                    };
                                    regionList.Add(region);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return View("GetCityCountByRegion", regionList);
        }

        //public async Task<Region> GetRegionByIdAsync(int id)
        //{
        //    Region region = null;
        //    try
        //    {
        //        string connectionStr = _configuration.GetConnectionString("DefaultConnection");
        //        string commandStr = "SELECT * FROM dbo.Region WHERE RegionID = @RegionID";
        //        using (SqlConnection con = new SqlConnection(connectionStr))
        //        {
        //            using (SqlCommand cmd = new SqlCommand(commandStr, con))
        //            {
        //                cmd.Parameters.AddWithValue("@RegionID", id);
        //                await con.OpenAsync();
        //                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
        //                {
        //                    if (reader.HasRows)
        //                    {
        //                        while (await reader.ReadAsync())
        //                        {
        //                            region = new Region();
        //                            region.RegionID = await reader.IsDBNullAsync(0) ? 0 : reader.GetInt32(0);
        //                            region.RegionName = await reader.IsDBNullAsync(1) ? default(string) : reader.GetString(1);
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //    return region;
        //}

        public async Task<IActionResult> GetCitiesByRegion(int regionId)
        {
            List<City> citiesList = new List<City>();
            try
            {
                string connectionStr = _configuration.GetConnectionString("DefaultConnection");
                string commandStr = "SELECT * FROM City WHERE RegionID = @regionId";
                using (SqlConnection con = new SqlConnection(connectionStr))
                {
                    using (SqlCommand cmd = new SqlCommand(commandStr, con))
                    {
                        cmd.Parameters.AddWithValue("@regionId", regionId);
                        await con.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            if (reader.HasRows)
                            {
                                while (await reader.ReadAsync())
                                {
                                    City cities = new City();
                                    cities.CityID = await reader.IsDBNullAsync(0) ? 0 : reader.GetInt32(0);
                                    cities.CityName = await reader.IsDBNullAsync(1) ? default(string) : reader.GetString(1);
                                    cities.PlateCode = await reader.IsDBNullAsync(2) ? 0 : reader.GetInt32(2);
                                    cities.RegionID = await reader.IsDBNullAsync(3) ? 0 : reader.GetInt32(3);
                                    citiesList.Add(cities);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return View("CitiesByRegion", citiesList);
        }



    }
}
