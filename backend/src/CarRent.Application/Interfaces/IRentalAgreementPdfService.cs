using CarRent.Domain.Entities;

namespace CarRent.Application.Interfaces;

public interface IRentalAgreementPdfService
{
    byte[] Generate(Rental rental, Booking booking, Vehicle vehicle, User customer, byte[] signatureImageBytes);
}
