using System.Threading;
using System.Threading.Tasks;

namespace MatdarSathi.API.Application.Common.Interfaces;

public interface IRequest<out TResponse>
{
}

public interface IRequestHandler<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}

public interface IMediator
{
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
}
