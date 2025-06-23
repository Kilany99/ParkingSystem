namespace ParkingSystem.Enums
{
    public enum SpotStatus
    {
        Available,         // 0 - Spot is free
        Occupied,          // 1 - Reservation made but not yet entered
        Reserved,          // 2 - Vehicle has entered and is currently parked
        Maintenance,       // 3 - Under maintenance
        OutOfService       // 4 - Permanently unavailable
    }
}
