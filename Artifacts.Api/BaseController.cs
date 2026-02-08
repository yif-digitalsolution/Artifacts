using Microsoft.AspNetCore.Mvc;
using Utils;
namespace Artifacts.Api;

[ApiController]
[Route("api/[controller]")]
public class BaseController : ControllerBase {


    protected IActionResult FromResult(Result result)
    {
        return result.ErrorType switch
        {
            ResultErrorType.Validation => BadRequest(result.Error),
            ResultErrorType.NotFound => NotFound(result.Error),
            ResultErrorType.Forbidden => Forbid(),
            _ => result.IsSuccess ? Ok() : BadRequest(result.Error)
        };
    }

    protected IActionResult FromResult<T>(Result<T> result)
    {
        return result.ErrorType switch
        {
            ResultErrorType.Validation => BadRequest(result.Error),
            ResultErrorType.NotFound => NotFound(result.Error),
            ResultErrorType.Forbidden => Forbid(),
            _ => result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error)
        };
    }
}

