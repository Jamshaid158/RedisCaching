using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedisCaching.Data;
using RedisCaching.Models;
using RedisCaching.Services;

namespace RedisCaching.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DriverController : ControllerBase
    {
        private readonly ILogger<DriverController> _logger;
        private readonly ICacheService _cacheService;
        private readonly AppDbContext _dbContext;

        public DriverController(
            ILogger<DriverController> logger,
            ICacheService cacheService,
            AppDbContext dbContext)
        {
            _logger = logger;
            _cacheService = cacheService;
            _dbContext = dbContext;
        }

        [HttpGet("drivers")]
        public async Task<IActionResult> Get()
        {
            //check cache data
            var cacheData = _cacheService.GetData<IEnumerable<Driver>>("drivers");

            if(cacheData != null && cacheData.Count() > 0)
            {
                return Ok(cacheData);
            }

            cacheData = await _dbContext.Drivers.ToListAsync();

            //Set expiry time
            var expiryTime = DateTimeOffset.Now.AddMinutes(5);
            _cacheService.SetData<IEnumerable<Driver>>("drivers", cacheData, expiryTime);

            return Ok(cacheData);
        }

        [HttpPost("AddDriver")]
        public async Task<IActionResult> Post(Driver driver)
        {
            var addedObj = await _dbContext.Drivers.AddAsync(driver);

            var expiryTime = DateTimeOffset.Now.AddMinutes(5);
            _cacheService.SetData<Driver>($"driver{driver.Id}", addedObj.Entity, expiryTime);
            
            await _dbContext.SaveChangesAsync();

            return Ok(addedObj.Entity);
        }

        [HttpDelete("DeleteDriver")]
        public async Task<IActionResult> Delete(int id)
        {
            var exist = await _dbContext.Drivers.FirstOrDefaultAsync(x => x.Id == id);

            if(exist != null)
            {
                _dbContext.Remove(exist);
                _cacheService.RemoveData($"driver{id}");
                await _dbContext.SaveChangesAsync();

                return NoContent();
            }

            return NotFound();
        }
    }
}
