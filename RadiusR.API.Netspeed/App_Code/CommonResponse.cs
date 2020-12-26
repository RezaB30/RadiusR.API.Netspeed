using Resources;
using RezaB.API.WebService;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Web;

/// <summary>
/// Summary description for CommonResponse
/// </summary>
public static class CommonResponse
{
    public static ServiceResponse InternalException(string culture , Exception ex = null)
    {
        return new ServiceResponse()
        {
            ErrorCode = (int)ErrorCodes.InternalServerError,
            ErrorMessage = ErrorMessages.ResourceManager.GetString(ErrorCodes.InternalServerError.ToString(), CreateCulture(culture)) + (ex == null ? "" : $"{ex.Message}")
        };
    }
    public static ServiceResponse UnauthorizedResponse(string culture)
    {
        return new ServiceResponse()
        {
            ErrorCode = (int)ErrorCodes.AuthenticationFailed,
            ErrorMessage = ErrorMessages.ResourceManager.GetString(ErrorCodes.AuthenticationFailed.ToString(), CreateCulture(culture))
        };
    }
    public static ServiceResponse SubscriberNotFoundErrorResponse(string culture)
    {
        return new ServiceResponse()
        {
            ErrorCode = (int)ErrorCodes.SubscriberNotFound,
            ErrorMessage = ErrorMessages.ResourceManager.GetString(ErrorCodes.SubscriberNotFound.ToString(), CreateCulture(culture))
        };
    }
    public static ServiceResponse NullObjectException(string culture)
    {
        return new ServiceResponse()
        {
            ErrorCode = (int)ErrorCodes.NullObjectFound,
            ErrorMessage = ErrorMessages.ResourceManager.GetString(ErrorCodes.NullObjectFound.ToString(), CreateCulture(culture))
        };
    }
    public static ServiceResponse BillsNotFoundException(string culture)
    {
        return new ServiceResponse()
        {
            ErrorCode = (int)ErrorCodes.BillsNotFound,
            ErrorMessage = ErrorMessages.ResourceManager.GetString(ErrorCodes.BillsNotFound.ToString(), CreateCulture(culture))
        };
    }
    public static ServiceResponse Failed(string culture)
    {
        return new ServiceResponse()
        {
            ErrorCode = (int)ErrorCodes.Failed,
            ErrorMessage = ErrorMessages.ResourceManager.GetString(ErrorCodes.Failed.ToString(), CreateCulture(culture))
        };
    }
    public static ServiceResponse SuccessResponse(string culture)
    {
        return new ServiceResponse()
        {
            ErrorCode = (int)ErrorCodes.Success,
            ErrorMessage = ErrorMessages.ResourceManager.GetString(ErrorCodes.Success.ToString(), CreateCulture(culture))
        };
    }
    public static ServiceResponse FailedResponse(string culture)
    {
        return new ServiceResponse()
        {
            ErrorCode = (int)ErrorCodes.Failed,
            ErrorMessage = ErrorMessages.ResourceManager.GetString(ErrorCodes.Failed.ToString(), CreateCulture(culture))
        };
    }
    private static CultureInfo CreateCulture(string cultureName)
    {
        var currentCulture = CultureInfo.InvariantCulture;
        try
        {
            currentCulture = CultureInfo.CreateSpecificCulture(cultureName);
        }
        catch { }
        return currentCulture;
    }
}