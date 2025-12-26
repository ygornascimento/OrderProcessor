using Orders.Domain.Entities;
using Xunit;

namespace Orders.UnitTests;

public class OrderTests
{
    [Fact]
    public void Ctor_Com_customerName_vazio_deve_falhar()
    {
        Assert.ThrowsAny<Exception>(() =>
            new Order(Guid.NewGuid(), "", 10m, DateTime.UtcNow)
        );
    }

    [Fact]
    public void Ctor_Com_amount_menor_ou_igual_zero_deve_falhar()
    {
        Assert.ThrowsAny<Exception>(() =>
            new Order(Guid.NewGuid(), "Ygor", 0m, DateTime.UtcNow)
        );

        Assert.ThrowsAny<Exception>(() =>
            new Order(Guid.NewGuid(), "Ygor", -1m, DateTime.UtcNow)
        );
    }
}
