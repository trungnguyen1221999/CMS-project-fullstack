using Application.Constants;
using Application.Contracts.Common;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    public abstract class ApiControllerBase : ControllerBase
    {
        protected ActionResult ToActionResult(WriteResponse result)
        {
            if (result.IsSuccess)
                return Ok(result);

            return IsNotFoundError(result.ErrorCode) ? NotFound(result) : BadRequest(result);
        }

        protected ActionResult ToActionResult<T>(ReadResponse<T> result)
        {
            if (result.IsSuccess)
                return Ok(result);

            return IsNotFoundError(result.ErrorCode) ? NotFound(result) : BadRequest(result);
        }

        private static bool IsNotFoundError(string? errorCode) =>
            errorCode is not null && errorCode.EndsWith(ErrorMessages.Common.NotFoundSuffix);
    }
}
