namespace Hotel_Management_System.Model
{
    public class Booking
    {
        public int Id { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public string Status { get; set; } = "Confirmed"; // Confirmed, Cancelled
        public DateTime BookedAt { get; set; } = DateTime.UtcNow;

        // Which guest booked
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        // Which room booked
        public int RoomId { get; set; }
        public Room Room { get; set; } = null!;
    }
}
