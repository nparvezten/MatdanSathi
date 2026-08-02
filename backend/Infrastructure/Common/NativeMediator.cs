using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;
using MatdarSathi.API.Application.Common.Interfaces;

namespace MatdarSathi.API.Infrastructure.Common;

public class NativeMediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;

    public NativeMediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var requestType = request.GetType();

        // 1. FluentValidation pre-execution check
        var validatorType = typeof(IValidator<>).MakeGenericType(requestType);
        System.Collections.IEnumerable? validators = null;
        try
        {
            var enumerableType = typeof(IEnumerable<>).MakeGenericType(validatorType);
            validators = _serviceProvider.GetService(enumerableType) as System.Collections.IEnumerable;
        }
        catch
        {
            validators = null;
        }

        if (validators != null)
        {
            var failures = new List<ValidationFailure>();
            foreach (var validator in validators)
            {
                if (validator != null)
                {
                    var contextType = typeof(ValidationContext<>).MakeGenericType(requestType);
                    var context = Activator.CreateInstance(contextType, request);
                    var validateMethod = validatorType.GetMethod("ValidateAsync", new[] { contextType, typeof(CancellationToken) });
                    if (validateMethod != null)
                    {
                        var task = (Task<ValidationResult>)validateMethod.Invoke(validator, new[] { context, cancellationToken })!;
                        var result = await task;
                        if (result != null && result.Errors != null && result.Errors.Count > 0)
                        {
                            failures.AddRange(result.Errors);
                        }
                    }
                }
            }
            if (failures.Count > 0)
            {
                throw new ValidationException(failures);
            }
        }

        // 2. Dispatch to Native IRequestHandler<TRequest, TResponse>
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));
        var handler = _serviceProvider.GetService(handlerType);
        if (handler == null)
        {
            throw new InvalidOperationException($"No handler registered for request type '{requestType.Name}' returning '{typeof(TResponse).Name}'.");
        }

        var method = handlerType.GetMethod(nameof(IRequestHandler<IRequest<TResponse>, TResponse>.Handle));
        if (method == null)
        {
            throw new InvalidOperationException($"Handle method not found on handler for '{requestType.Name}'.");
        }

        try
        {
            var resultTask = (Task<TResponse>)method.Invoke(handler, new object[] { request, cancellationToken })!;
            return await resultTask;
        }
        catch (System.Reflection.TargetInvocationException ex) when (ex.InnerException != null)
        {
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            throw;
        }
    }
}
