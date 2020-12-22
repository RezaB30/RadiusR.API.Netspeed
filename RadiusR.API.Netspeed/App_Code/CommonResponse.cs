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
public static class CommonResponse<T, HAT> where HAT : HashAlgorithm
{
    public static BaseResponse<T, HAT> InternalException(string passwordHash, BaseRequest<HAT> baseRequest)
    {
        return new BaseResponse<T, HAT>(passwordHash, baseRequest)
        {
            Culture = baseRequest.Culture,
            ResponseMessage = new ServiceResponse()
            {
                ErrorCode = (int)ErrorCodes.InternalServerError,
                ErrorMessage = ErrorMessages.ResourceManager.GetString(ErrorCodes.InternalServerError.ToString(), CreateCulture(baseRequest.Culture))
            }
        };
    }
    public static BaseResponse<T, HAT> UnauthorizedResponse(string passwordHash, BaseRequest<HAT> baseRequest)
    {
        return new BaseResponse<T, HAT>(passwordHash, baseRequest)
        {
            Culture = baseRequest.Culture,
            ResponseMessage = new ServiceResponse()
            {
                ErrorCode = (int)ErrorCodes.AuthenticationFailed,
                ErrorMessage = ErrorMessages.ResourceManager.GetString(ErrorCodes.AuthenticationFailed.ToString(), CreateCulture(baseRequest.Culture))
            }
        };
    }
    public static BaseResponse<T, HAT> SubscriberNotFoundErrorResponse(string passwordHash, BaseRequest<HAT> baseRequest)
    {
        return new BaseResponse<T, HAT>(passwordHash, baseRequest)
        {
            Culture = baseRequest.Culture,
            ResponseMessage = new ServiceResponse()
            {
                ErrorCode = (int)ErrorCodes.SubscriberNotFound,
                ErrorMessage = ErrorMessages.ResourceManager.GetString(ErrorCodes.SubscriberNotFound.ToString(), CreateCulture(baseRequest.Culture))
            }
        };
    }
    public static BaseResponse<T, HAT> Failed(string passwordHash, BaseRequest<HAT> baseRequest)
    {
        return new BaseResponse<T, HAT>(passwordHash, baseRequest)
        {
            Culture = baseRequest.Culture,
            ResponseMessage = new ServiceResponse()
            {
                ErrorCode = (int)ErrorCodes.Failed,
                ErrorMessage = ErrorMessages.ResourceManager.GetString(ErrorCodes.Failed.ToString(), CreateCulture(baseRequest.Culture))
            }
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