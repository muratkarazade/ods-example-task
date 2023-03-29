using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using ODS_Example_Task.Models;
using System.Data.SqlClient;

namespace ODS_Example_Task.Controllers
{
    public class CitiesController : Controller
    {
        private readonly IConfiguration _configuration;       
        public CitiesController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            List<City> cityList = await GetAllCitiesAsync();
            return View(cityList);
        }

        public async Task<List<City>> GetAllCitiesAsync()
        {
            List<City> citiesList = new List<City>();
            try
            {
                string connectionStr = _configuration.GetConnectionString("DefaultConnection");
                string commandStr = "SELECT * FROM dbo.City";
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
                                    City city = new City();
                                    city.CityID = await reader.IsDBNullAsync(0) ? 0 : reader.GetInt32(0);
                                    city.CityName = await reader.IsDBNullAsync(1) ? default(string) : reader.GetString(1);                                   
                                    citiesList.Add(city);

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
            return citiesList;
        }


        public IActionResult Create()
        {
            return View();
        }
        public async Task<IActionResult> CreateCity(City city)
        {
            bool success = false;
            try
            {
                if (city == null) return BadRequest();
                string cityNameValue = city.CityName;
                var existRegion = await CheckCityNameExistsAsync(cityNameValue);
                if (existRegion == true) return BadRequest("Belirtilen Şehir Kayıtlı");
                var regionCount = await GetCityCountAsync();
                if (regionCount >= 7)
                {
                    return BadRequest("Şehir Sayısı 81 den fazla olamaz");
                }
                string connectionStr = _configuration.GetConnectionString("DefaultConnection");
                string commandStr = "INSERT INTO dbo.City ( CityName) VALUES ( @CityName)"; ;
                using (SqlConnection con = new SqlConnection(connectionStr))
                {
                    using (SqlCommand cmd = new SqlCommand(commandStr, con))
                    {

                        cmd.Parameters.AddWithValue("@CityName", city.CityName ?? (object)DBNull.Value);
                        await con.OpenAsync();
                        await cmd.ExecuteNonQueryAsync(); success = true;
                    }
                }
                return Redirect("/Cities/Index");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<bool> CheckCityNameExistsAsync(string cityName)
        {
            try
            {
                string connectionStr = _configuration.GetConnectionString("DefaultConnection");
                string commandStr = "SELECT COUNT(*) FROM dbo.City WHERE CityName = @CityName";
                using (SqlConnection con = new SqlConnection(connectionStr))
                {
                    using (SqlCommand cmd = new SqlCommand(commandStr, con))
                    {
                        cmd.Parameters.AddWithValue("@CityName", cityName);
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

        public async Task<int> GetCityCountAsync()
        {
            int count = 0;
            try
            {
                string connectionStr = _configuration.GetConnectionString("DefaultConnection");
                string commandStr = "SELECT COUNT(*) FROM dbo.City";
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


        public async Task<IActionResult> Delete(int id)
        {
            bool success = false;
            try
            {
                if (id == null) return BadRequest();
                string connectionStr = _configuration.GetConnectionString("DefaultConnection");
                string commandStr = "DELETE FROM dbo.City WHERE CityID=@id"; ;
                using (SqlConnection con = new SqlConnection(connectionStr))
                {
                    using (SqlCommand cmd = new SqlCommand(commandStr, con))
                    {

                        cmd.Parameters.AddWithValue("@id", id);
                        await con.OpenAsync();
                        await cmd.ExecuteNonQueryAsync(); success = true;
                    }
                }
                return Redirect("/Cities/Index");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}





















// ***************************************************************

//// GET: Cities
//public async Task<IActionResult> Index()
//{
//    var areas_cities_taskContext = _context.City.Include(c => c.Regions);
//    return View(await areas_cities_taskContext.ToListAsync());
//}

//// GET: Cities/Details/5
//public async Task<IActionResult> Details(int? id)
//{
//    if (id == null || _context.City == null)
//    {
//        return NotFound();
//    }

//    var city = await _context.City
//        .Include(c => c.Regions)
//        .FirstOrDefaultAsync(m => m.CitiesID == id);
//    if (city == null)
//    {
//        return NotFound();
//    }

//    return View(city);
//}

//// GET: Cities/Create
//public IActionResult Create()
//{
//    ViewData["RegionID"] = new SelectList(_context.Set<Region>(), "RegionID", "RegionID");
//    return View();
//}

//// POST: Cities/Create
//// To protect from overposting attacks, enable the specific properties you want to bind to.
//// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
//[HttpPost]
//[ValidateAntiForgeryToken]
//public async Task<IActionResult> Create([Bind("CitiesID,CityName,PlateCode,RegionID")] City city)
//{
//    if (ModelState.IsValid)
//    {
//        _context.Add(city);
//        await _context.SaveChangesAsync();
//        return RedirectToAction(nameof(Index));
//    }
//    ViewData["RegionID"] = new SelectList(_context.Set<Region>(), "RegionID", "RegionID", city.RegionID);
//    return View(city);
//}

//// GET: Cities/Edit/5
//public async Task<IActionResult> Edit(int? id)
//{
//    if (id == null || _context.City == null)
//    {
//        return NotFound();
//    }

//    var city = await _context.City.FindAsync(id);
//    if (city == null)
//    {
//        return NotFound();
//    }
//    ViewData["RegionID"] = new SelectList(_context.Set<Region>(), "RegionID", "RegionID", city.RegionID);
//    return View(city);
//}

//// POST: Cities/Edit/5
//// To protect from overposting attacks, enable the specific properties you want to bind to.
//// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
//[HttpPost]
//[ValidateAntiForgeryToken]
//public async Task<IActionResult> Edit(int id, [Bind("CitiesID,CityName,PlateCode,RegionID")] City city)
//{
//    if (id != city.CitiesID)
//    {
//        return NotFound();
//    }

//    if (ModelState.IsValid)
//    {
//        try
//        {
//            _context.Update(city);
//            await _context.SaveChangesAsync();
//        }
//        catch (DbUpdateConcurrencyException)
//        {
//            if (!CityExists(city.CitiesID))
//            {
//                return NotFound();
//            }
//            else
//            {
//                throw;
//            }
//        }
//        return RedirectToAction(nameof(Index));
//    }
//    ViewData["RegionID"] = new SelectList(_context.Set<Region>(), "RegionID", "RegionID", city.RegionID);
//    return View(city);
//}

//// GET: Cities/Delete/5
//public async Task<IActionResult> Delete(int? id)
//{
//    if (id == null || _context.City == null)
//    {
//        return NotFound();
//    }

//    var city = await _context.City
//        .Include(c => c.Regions)
//        .FirstOrDefaultAsync(m => m.CitiesID == id);
//    if (city == null)
//    {
//        return NotFound();
//    }

//    return View(city);
//}

//// POST: Cities/Delete/5
//[HttpPost, ActionName("Delete")]
//[ValidateAntiForgeryToken]
//public async Task<IActionResult> DeleteConfirmed(int id)
//{
//    if (_context.City == null)
//    {
//        return Problem("Entity set 'areas_cities_taskContext.City'  is null.");
//    }
//    var city = await _context.City.FindAsync(id);
//    if (city != null)
//    {
//        _context.City.Remove(city);
//    }

//    await _context.SaveChangesAsync();
//    return RedirectToAction(nameof(Index));
//}

//private bool CityExists(int id)
//{
//    return (_context.City?.Any(e => e.CitiesID == id)).GetValueOrDefault();
//}