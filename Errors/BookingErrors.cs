namespace MedAI.Errors;

public record BookingErrors
{
    public static readonly Error AlreadyBooked =
        new("Booking.AlreadyBooked", "You have already booked this slot", StatusCodes.Status400BadRequest);
    public static readonly Error FullyBooked =
        new("Booking.FullyBooked", "This slot is fully booked", StatusCodes.Status400BadRequest);

    public static readonly Error NotFound = 
        new("Booking.NotFound", "Booking not found", StatusCodes.Status404NotFound);

    public static readonly Error HasBookings =
        new("Slot.HasBookings", "Cannot delete slot with bookings", StatusCodes.Status400BadRequest);

    public static readonly Error Unauthorized =
        new("Booking.Unauthorized", "You are not authorized to perform this action", StatusCodes.Status401Unauthorized);
}
