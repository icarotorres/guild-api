using Application.Common.Abstractions;
using AutoMapper;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Common.PipelineBehaviors
{
    public class MapperBehavior<TCommand, TResult> : IPipelineBehavior<TCommand, TResult>
        where TCommand : IMappedResultCommand
        where TResult : IApiResult
    {
        private readonly IMapper _mapper;

        public MapperBehavior(IMapper mapper)
        {
            _mapper = mapper;
        }

        public async Task<TResult> Handle(TCommand command, CancellationToken cancellationToken, RequestHandlerDelegate<TResult> next)
        {
            var result = await next();
            if (result.Success)
            {
                var mappedData = command.MapResult(result.Data, _mapper);
                result.SetData(mappedData);
            }
            return result;
        }
    }
}