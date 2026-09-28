using ERP.Api.Errors;
using ERP.Shared.DTOs;
using ERP.Shared.Exceptions;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using ErpValidationException = ERP.Shared.Exceptions.ValidationException;

namespace ERP.Tests.Api;

public class ApiErrorMapperTests
{
    [Fact]
    public void FluentValidationException_MapsTo400WithCamelCaseFieldErrors()
    {
        var ex = new FluentValidation.ValidationException(new[]
        {
            new ValidationFailure("CustomerCode", "Customer code is required"),
            new ValidationFailure("Lines[0].UnitPrice", "Must be 0 or greater"),
            new ValidationFailure("CustomerCode", "Customer code is required"),
        });

        var error = ApiErrorMapper.Map(ex, "trace-1");

        Assert.Equal(400, error.Status);
        Assert.Equal(ApiErrorCodes.ValidationError, error.ErrorCode);
        Assert.Equal(new[] { "Customer code is required" }, error.Errors!["customerCode"]);
        Assert.Equal(new[] { "Must be 0 or greater" }, error.Errors!["lines[0].unitPrice"]);
        Assert.Equal("trace-1", error.TraceId);
    }

    [Fact]
    public void ErpValidationException_KeepsMessageAndListsErrorsUnderEmptyKey()
    {
        var error = ApiErrorMapper.Map(new ErpValidationException("Email already registered."));

        Assert.Equal(400, error.Status);
        Assert.Equal("Email already registered.", error.Message);
        Assert.Equal(new[] { "Email already registered." }, error.Errors![""]);
    }

    [Theory]
    [InlineData(typeof(NotFoundException), 404, "NOT_FOUND")]
    [InlineData(typeof(UnauthorizedException), 401, "UNAUTHORIZED")]
    [InlineData(typeof(ForbiddenException), 403, "FORBIDDEN")]
    [InlineData(typeof(ConflictException), 409, "CONFLICT")]
    public void ErpExceptions_UseTheirOwnStatusAndMessage(Type type, int status, string code)
    {
        var ex = (Exception)Activator.CreateInstance(type, "Readable message")!;

        var error = ApiErrorMapper.Map(ex);

        Assert.Equal(status, error.Status);
        Assert.Equal(code, error.ErrorCode);
        Assert.Equal("Readable message", error.Message);
    }

    [Fact]
    public void KeyNotFoundException_MapsTo404()
    {
        var error = ApiErrorMapper.Map(new KeyNotFoundException("Employee with ID 'x' not found."));

        Assert.Equal(404, error.Status);
        Assert.Equal("Employee with ID 'x' not found.", error.Message);
    }

    [Fact]
    public void NotImplementedException_MapsTo501WithoutInternalDetail()
    {
        var error = ApiErrorMapper.Map(new NotImplementedException("BillService.ApproveAsync is not yet implemented"));

        Assert.Equal(501, error.Status);
        Assert.Equal(ApiErrorCodes.NotImplemented, error.ErrorCode);
        Assert.DoesNotContain("BillService", error.Message);
    }

    [Fact]
    public void DbUpdateException_MapsTo409WithGenericMessage()
    {
        var error = ApiErrorMapper.Map(new DbUpdateException("Cannot insert duplicate key row in object 'dbo.Items'"));

        Assert.Equal(409, error.Status);
        Assert.DoesNotContain("dbo.Items", error.Message);
    }

    [Fact]
    public void InvalidOperationException_FromErpCode_IsABusinessRule()
    {
        var error = ApiErrorMapper.Map(Capture(() => throw new InvalidOperationException("System roles cannot be deleted.")));

        Assert.Equal(409, error.Status);
        Assert.Equal(ApiErrorCodes.BusinessRule, error.ErrorCode);
        Assert.Equal("System roles cannot be deleted.", error.Message);
    }

    [Fact]
    public void InvalidOperationException_FromFrameworkCode_IsHidden()
    {
        var error = ApiErrorMapper.Map(Capture(() => new List<int>().First()));

        Assert.Equal(500, error.Status);
        Assert.Equal(ApiErrorMapper.GenericMessage, error.Message);
    }

    [Fact]
    public void UnknownException_IsHidden()
    {
        var error = ApiErrorMapper.Map(new Exception("Login failed for user 'sa'"));

        Assert.Equal(500, error.Status);
        Assert.Equal(ApiErrorMapper.GenericMessage, error.Message);
    }

    [Fact]
    public void ErpExceptionWith5xxStatus_IsHidden()
    {
        var error = ApiErrorMapper.Map(new ERPException("Connection string invalid", "DB", 503));

        Assert.Equal(503, error.Status);
        Assert.Equal(ApiErrorMapper.GenericMessage, error.Message);
    }

    [Theory]
    [InlineData("CustomerCode", "customerCode")]
    [InlineData("Lines[0].UnitPrice", "lines[0].unitPrice")]
    [InlineData("alreadyCamel", "alreadyCamel")]
    [InlineData("", "")]
    public void ToCamelCasePath_ConvertsEachSegment(string input, string expected)
    {
        Assert.Equal(expected, ApiErrorMapper.ToCamelCasePath(input));
    }

    private static Exception Capture(Action action)
    {
        try { action(); }
        catch (Exception ex) { return ex; }
        throw new InvalidOperationException("Expected an exception.");
    }

    private static Exception Capture(Func<object> action) => Capture(() => { action(); });
}
