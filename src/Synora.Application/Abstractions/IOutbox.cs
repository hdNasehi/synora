using Synora.Domain.Common;

namespace Synora.Application.Abstractions;

public interface IOutbox
{
    void Enqueue(DomainEvent domainEvent);
}
