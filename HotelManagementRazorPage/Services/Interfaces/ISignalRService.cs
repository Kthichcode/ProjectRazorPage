namespace Services.Interfaces
{
    public interface ISignalRService
    {
        Task SendRoomStatusUpdate(string message);

        /// Broadcasts a new booking event to all Staff clients
        Task SendNewBooking(int bookingId, string customerName, string phoneNumber, string roomNumbers, decimal totalAmount, DateTime checkIn, DateTime checkOut);
    }
}
