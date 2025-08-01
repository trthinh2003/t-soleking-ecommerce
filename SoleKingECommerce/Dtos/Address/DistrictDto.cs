namespace SoleKingECommerce.Dtos.Address
{
    public class DistrictDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<WardDto> Wards { get; set; }
    }
}
