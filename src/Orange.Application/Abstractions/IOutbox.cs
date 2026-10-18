using Orange.Domain.Common;

namespace Orange.Application.Abstractions;

public interface IOutbox
{
    void Enqueue(DomainEvent domainEvent);
}
