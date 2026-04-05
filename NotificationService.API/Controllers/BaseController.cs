using Microsoft.AspNetCore.Mvc;
using SmartShip.Shared.DTOs;
using SmartShip.Shared.Exceptions;

namespace NotificationService.API.Controllers;

[ApiController]
public abstract class BaseController : ControllerBase
{
    protected IActionResult HandleException(Exception ex)
    {
        var traceId = HttpContext.TraceIdentifier;

        return ex switch
        {
            ValidationException validationEx => BadRequest(ApiErrorResponse.Create(
                validationEx.StatusCode, validationEx.Message, validationEx.Code,
                validationEx.Errors, traceId)),

            NotFoundException notFoundEx => NotFound(ApiErrorResponse.Create(
                notFoundEx.StatusCode, notFoundEx.Message, notFoundEx.Code,
                traceId: traceId)),

            UnauthorizedException unauthorizedEx => Unauthorized(ApiErrorResponse.Create(
                unauthorizedEx.StatusCode, unauthorizedEx.Message, unauthorizedEx.Code,
                traceId: traceId)),

            ForbiddenException forbiddenEx => StatusCode(403, ApiErrorResponse.Create(
                forbiddenEx.StatusCode, forbiddenEx.Message, forbiddenEx.Code,
                traceId: traceId)),

            ConflictException conflictEx => Conflict(ApiErrorResponse.Create(
                conflictEx.StatusCode, conflictEx.Message, conflictEx.Code,
                traceId: traceId)),

            BusinessException businessEx => UnprocessableEntity(ApiErrorResponse.Create(
                businessEx.StatusCode, businessEx.Message, businessEx.Code,
                traceId: traceId)),

            _ => StatusCode(500, ApiErrorResponse.Create(
                500, "An unexpected error occurred.", "INTERNAL_SERVER_ERROR",
                traceId: traceId))
        };
    }

    protected IActionResult OkResponse<T>(T data, string message = "Success")
    {
        return Ok(ApiResponse<T>.SuccessResponse(data, message));
    }

    protected IActionResult CreatedResponse<T>(string actionName, object routeValues, T data, string message = "Created successfully")
    {
        return CreatedAtAction(actionName, routeValues, ApiResponse<T>.SuccessResponse(data, message));
    }

    protected IActionResult NotFoundResponse(string message)
    {
        return NotFound(ApiResponse<object>.FailureResponse(message));
    }

    protected IActionResult BadRequestResponse(string message)
    {
        return BadRequest(ApiResponse<object>.FailureResponse(message));
    }

    protected IActionResult UnauthorizedResponse(string message = "Unauthorized access")
    {
        return Unauthorized(ApiResponse<object>.FailureResponse(message));
    }
}
