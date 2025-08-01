using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using SoleKingECommerce.Dtos.Address;
using System.Text.Json;

namespace SoleKingECommerce.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class AddressController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public AddressController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpGet("provinces")]
        public IActionResult GetProvinces()
        {
            var addressData = LoadAddressData();
            var provinces = addressData.Select(p => new { Id = p.Id, Name = p.Name }).ToList();
            return Ok(provinces);
        }

        [HttpGet("districts/{provinceId}")]
        public IActionResult GetDistricts(string provinceId)
        {
            var addressData = LoadAddressData();
            var province = addressData.FirstOrDefault(p => p.Id == provinceId);

            if (province == null)
                return NotFound();

            var districts = province.Districts.Select(d => new { Id = d.Id, Name = d.Name }).ToList();
            return Ok(districts);
        }

        [HttpGet("wards/{provinceId}/{districtId}")]
        public IActionResult GetWards(string provinceId, string districtId)
        {
            var addressData = LoadAddressData();
            var province = addressData.FirstOrDefault(p => p.Id == provinceId);

            if (province == null)
                return NotFound();

            var district = province.Districts.FirstOrDefault(d => d.Id == districtId);

            if (district == null)
                return NotFound();

            var wards = district.Wards.Select(w => new { Id = w.Id, Name = $"{w.Level} {w.Name}" }).ToList();
            return Ok(wards);
        }

        private List<AddressDto> LoadAddressData()
        {
            var filePath = Path.Combine(_env.WebRootPath, "json", "vietnamAddress.json");
            var jsonData = System.IO.File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<List<AddressDto>>(jsonData);
        }
    }
}