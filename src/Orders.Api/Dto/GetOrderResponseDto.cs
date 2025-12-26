namespace Orders.Api.Dto
{
    public record GetOrderResponseDto(Guid Id,
        string CustomerName,
        decimal Amount,
        DateTime OrderDate);
}
