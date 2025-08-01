namespace SoleKingECommerce.Dtos.Address
{
    public class AddressDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<DistrictDto> Districts { get; set; }
    }
}
