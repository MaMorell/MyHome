namespace MyHome.Core.Models.Entities;

public record SensorData : IEntity
{
    public Guid Id { get; set; }
    public string? DeviceName { get; set; }
    public DateTime Timestamp { get; set; }
    public double Humidity { get; set; }
    public double Temperature { get; set; }

    //public int Battery { get; set; }
    //public int Linkquality { get; set; }
    //public int Voltage { get; set; }
}