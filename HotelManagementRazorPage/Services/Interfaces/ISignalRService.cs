namespace Services.Interfaces
{
    public interface ISignalRService
    {
        Task SendRoomStatusUpdate(string message);

        /// Broadcasts a new booking event to all Staff clients
        Task SendNewBooking(int bookingId, string customerName, string phoneNumber, string roomNumbers, decimal totalAmount, DateTime checkIn, DateTime checkOut);

        /// Broadcasts room CRUD events to all user clients (action = "created" | "updated" | "deleted")
        Task SendRoomUpdate(string action, string roomNumber);

        /// Broadcasts a payment success event to all Staff clients
        Task SendPaymentSuccess(int bookingId, string customerName, decimal amount);
    }
}
