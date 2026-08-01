namespace EnergyOptimizer.Core.DTOs.BuildingDTOs
{
    public class BuildingDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public double TotalArea { get; set; }
        public int NumberOfRooms { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateBuildingDto
    {
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public double TotalArea { get; set; }
        public int NumberOfRooms { get; set; }
    }

    public class UpdateBuildingDto
    {
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public double TotalArea { get; set; }
        public int NumberOfRooms { get; set; }
    }
}
