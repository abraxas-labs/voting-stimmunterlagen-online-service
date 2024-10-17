// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Schema;
using AutoMapper;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Voting.Lib.Iam.Exceptions;
using Voting.Stimmunterlagen.Core.Exceptions;

namespace Voting.Stimmunterlagen.Exceptions;

public readonly struct ExceptionMapping
{
    private const string PostgresDuplicateSqlState = "23505";
    private const string EnumMappingErrorSource = "AutoMapper.Extensions.EnumMapping";

    private readonly StatusCode _grpcStatusCode;
    private readonly int _httpStatusCode;
    private readonly bool _exposeExceptionType;

    public ExceptionMapping(StatusCode grpcStatusCode, int httpStatusCode, bool exposeExceptionType = false)
    {
        _grpcStatusCode = grpcStatusCode;
        _httpStatusCode = httpStatusCode;
        _exposeExceptionType = exposeExceptionType;
    }

    public static int MapToHttpStatusCode(Exception ex)
        => Map(ex)._httpStatusCode;

    public static StatusCode MapToGrpcStatusCode(Exception ex)
        => Map(ex)._grpcStatusCode;

    public static bool ExposeExceptionType(Exception ex)
        => Map(ex)._exposeExceptionType;

    private static ExceptionMapping Map(Exception ex)
        => ex switch
        {
            NotAuthenticatedException => new(StatusCode.Unauthenticated, StatusCodes.Status401Unauthorized),
            ForbiddenException => new(StatusCode.PermissionDenied, StatusCodes.Status403Forbidden),
            ValidationException => new(StatusCode.InvalidArgument, StatusCodes.Status400BadRequest),
            FluentValidation.ValidationException => new(StatusCode.InvalidArgument, StatusCodes.Status400BadRequest),
            EntityNotFoundException => new(StatusCode.NotFound, StatusCodes.Status404NotFound),
            DbUpdateException { InnerException: PostgresException { SqlState: PostgresDuplicateSqlState } } => new(StatusCode.AlreadyExists, StatusCodes.Status409Conflict),
            AutoMapperMappingException autoMapperException when autoMapperException.InnerException is not null => Map(autoMapperException.InnerException),
            AutoMapperMappingException autoMapperException when string.Equals(autoMapperException.Source, EnumMappingErrorSource) => new(StatusCode.InvalidArgument, StatusCodes.Status400BadRequest),
            XmlSchemaValidationException => new(StatusCode.InvalidArgument, StatusCodes.Status400BadRequest),
            RpcException { StatusCode: StatusCode.NotFound } => new(StatusCode.NotFound, StatusCodes.Status404NotFound),
            _ => new(StatusCode.Internal, StatusCodes.Status500InternalServerError),
        };
}
