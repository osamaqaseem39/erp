using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Abstractions;

public interface IControllerBase
{
    Task<IActionResult> HandleAsync<T>(Func<Task<T>> operation) where T : class?;
    Task<IActionResult> HandleAsync(Func<Task> operation);
}