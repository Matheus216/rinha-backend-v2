namespace payment_backend.Models;

public record Summary(SummaryInformation Default, SummaryInformation Fallback);
public record SummaryInformation(int TotalRequests, double TotalAmount);