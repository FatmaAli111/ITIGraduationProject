using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Identitiy.Commands.Validators
{
    namespace ITIGraduationProject.Application.Behaviors
    {
        public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
            where TRequest : IRequest<TResponse>
        {
            private readonly IEnumerable<IValidator<TRequest>> _validators;

            public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
            {
                _validators = validators;
            }

            public async Task<TResponse> Handle(
                TRequest request,
                RequestHandlerDelegate<TResponse> next,
                CancellationToken cancellationToken)
            {
                if (_validators.Any())
                {
                    var context = new ValidationContext<TRequest>(request);

                    var failures = _validators
                        .Select(v => v.Validate(context))
                        .SelectMany(result => result.Errors)
                        .Where(f => f != null)
                        .ToList();

                    if (failures.Any())
                        throw new ValidationException(failures);
                }

                return await next();
            }
        }
    }
}
