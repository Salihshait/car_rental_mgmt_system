namespace CarRent.Application.DTOs.Crm;

public record FeedbackDto(
    Guid Id,
    Guid CustomerId,
    string? CustomerName,
    Guid? BookingId,
    int Rating,
    string? Comment,
    string Category,
    bool IsPublished,
    DateTime CreatedAt);

public record CreateFeedbackRequest(int Rating, string? Comment, string Category, Guid? BookingId);

public record PublishFeedbackRequest(bool IsPublished);
