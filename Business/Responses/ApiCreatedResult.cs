﻿using Domain.Commands;
using Domain.Hateoas;
using Domain.Messages;
using Domain.Responses;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace Business.Responses
{
    [ExcludeFromCodeCoverage]
    public class ApiCreatedResult : CreatedResult, IApiResult
    {
        internal ApiCreatedResult() : this(string.Empty, default) { }
        internal ApiCreatedResult(string location, object data) : base(location, new Result
        {
            Data = data,
            Success = true
        })
        {
            Data = data;
            Errors = new List<DomainMessage>();
        }

        public object Data { get; private set; }
        public bool Success => true;
        public IEnumerable<DomainMessage> Errors { get; }
        public IDictionary<string, Link> Links { get; private set; } = new Dictionary<string, Link>(StringComparer.InvariantCultureIgnoreCase);
        public HttpStatusCode GetStatus() => HttpStatusCode.Created;

        public IApiResult SetCreated(object result, ICreationCommand creationCommand)
        {
            var urlHelper = creationCommand.GetUrlHelper();
            var routeName = creationCommand.GetRouteName();
            var routeValuesFunction = creationCommand.GetRouteValuesFunc();

            Location = urlHelper.Link(routeName, routeValuesFunction(result));
            Data = result;

            return new ApiCreatedResult(Location, Data);
        }

        public IApiResult SetResult(object result, HttpStatusCode status = HttpStatusCode.OK)
        {
            if (status != HttpStatusCode.Created) return new ApiResult().SetResult(result, status);

            Data = result;
            ((Result)Value).Data = Data;

            return this;
        }

        public IApiResult SetExecutionError(HttpStatusCode httpStatusCode = HttpStatusCode.Conflict, params DomainMessage[] errors)
        {
            return new ApiResult().SetExecutionError(httpStatusCode, errors);
        }

        public IApiResult SetValidationError(params ValidationFailure[] validationFailures)
        {
            return new ApiResult().SetValidationError(validationFailures);
        }

        public IApiResult IncludeHateoas(IApiHateoasFactory hateoas)
        {
            if (Success)
            {
                Links = hateoas.Create(Data);
                Value = new Result() { Data = Data, Success = Success, Links = Links };
            }

            return this;
        }
    }
}
