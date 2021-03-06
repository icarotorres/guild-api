using Application.Common.Abstractions;
using Application.Common.Results;
using FluentValidation;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Common.PipelineBehaviors
{
    public class RequestValidationBehavior<TCommand, TResult> : IPipelineBehavior<TCommand, TResult>
        where TCommand : IRequest<TResult>
        where TResult : IApiResult
    {
        private readonly IEnumerable<IValidator<TCommand>> _requestValidators;

        public RequestValidationBehavior(IEnumerable<IValidator<TCommand>> requestValidators)
        {
            _requestValidators = requestValidators;
        }

        public async Task<TResult> Handle(TCommand command, CancellationToken cancellationToken, RequestHandlerDelegate<TResult> next)
        {
            var requestContext = new ValidationContext<TCommand>(command);

            var validationTasks = _requestValidators.Select(x => x.ValidateAsync(requestContext, cancellationToken));
            var validations = await Task.WhenAll(validationTasks);
            var failures = validations.SelectMany(x => x.Errors).Where(x => x != null).ToList();

            return failures.Count > 0
                ? (TResult)(new FailValidationResult(failures.ToArray()) as IApiResult)
                : await next();
        }
    }
}