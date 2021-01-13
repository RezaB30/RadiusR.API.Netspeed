using RadiusR.API.Netspeed.Enums;
using RadiusR.API.Netspeed.Localization;
using RadiusR.DB.Utilities.Billing;
using RezaB.API.WebService;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace RadiusR.API.Netspeed
{
    public static class CommonResponse
    {
        public static ServiceResponse InternalException(string culture, Exception ex = null)
        {
            return new ServiceResponse()
            {
                ErrorCode = (int)ErrorCodes.InternalServerError,
                ErrorMessage = new RezaB.Data.Localization.LocalizedList<ErrorCodes, ErrorMessages>().GetDisplayText((int)ErrorCodes.InternalServerError, CreateCulture(culture)) + $" - {ex.Message}"
            };
        }
        public static ServiceResponse UnauthorizedResponse(string culture)
        {
            return new ServiceResponse()
            {
                ErrorCode = (int)ErrorCodes.AuthenticationFailed,
                ErrorMessage = new RezaB.Data.Localization.LocalizedList<ErrorCodes, ErrorMessages>().GetDisplayText((int)ErrorCodes.AuthenticationFailed, CreateCulture(culture))
            };
        }
        public static ServiceResponse SubscriberNotFoundErrorResponse(string culture)
        {
            return new ServiceResponse()
            {
                ErrorCode = (int)ErrorCodes.SubscriberNotFound,
                ErrorMessage = new RezaB.Data.Localization.LocalizedList<ErrorCodes, ErrorMessages>().GetDisplayText((int)ErrorCodes.SubscriberNotFound, CreateCulture(culture))
            };
        }
        public static ServiceResponse NullObjectException(string culture)
        {
            return new ServiceResponse()
            {
                ErrorCode = (int)ErrorCodes.NullObjectFound,
                ErrorMessage = new RezaB.Data.Localization.LocalizedList<ErrorCodes, ErrorMessages>().GetDisplayText((int)ErrorCodes.NullObjectFound, CreateCulture(culture))
            };
        }
        public static ServiceResponse BillsNotFoundException(string culture)
        {
            return new ServiceResponse()
            {
                ErrorCode = (int)ErrorCodes.BillsNotFound,
                ErrorMessage = new RezaB.Data.Localization.LocalizedList<ErrorCodes, ErrorMessages>().GetDisplayText((int)ErrorCodes.BillsNotFound, CreateCulture(culture))
            };
        }
        public static ServiceResponse SuccessResponse(string culture)
        {
            return new ServiceResponse()
            {
                ErrorCode = (int)ErrorCodes.Success,
                ErrorMessage = new RezaB.Data.Localization.LocalizedList<ErrorCodes, ErrorMessages>().GetDisplayText((int)ErrorCodes.Success, CreateCulture(culture))
            };
        }
        public static ServiceResponse FailedResponse(string culture, string errorMessage = null)
        {
            return new ServiceResponse()
            {
                ErrorCode = (int)ErrorCodes.Failed,
                ErrorMessage = errorMessage ?? new RezaB.Data.Localization.LocalizedList<ErrorCodes, ErrorMessages>().GetDisplayText((int)ErrorCodes.Failed, CreateCulture(culture))
            };
        }
        public static ServiceResponse HasMoreSubscription(string culture)
        {
            return new ServiceResponse()
            {
                ErrorCode = (int)ErrorCodes.HasMoreSubscription,
                ErrorMessage = new RezaB.Data.Localization.LocalizedList<ErrorCodes, ErrorMessages>().GetDisplayText((int)ErrorCodes.HasMoreSubscription, CreateCulture(culture))
            };
        }
        public static ServiceResponse HaveAlreadyCustomer(string culture)
        {
            return new ServiceResponse()
            {
                ErrorCode = (int)ErrorCodes.AlreadyHaveCustomer,
                ErrorMessage = new RezaB.Data.Localization.LocalizedList<ErrorCodes, ErrorMessages>().GetDisplayText((int)ErrorCodes.AlreadyHaveCustomer, CreateCulture(culture))
            };
        }
        public static ServiceResponse WrongOrInvalidBills(string culture)
        {
            return new ServiceResponse()
            {
                ErrorCode = (int)ErrorCodes.WrongOrInvalidBill,
                ErrorMessage = new RezaB.Data.Localization.LocalizedList<ErrorCodes, ErrorMessages>().GetDisplayText((int)ErrorCodes.WrongOrInvalidBill, CreateCulture(culture))
            };
        }
        public static ServiceResponse SpecialOfferError(string culture)
        {
            return new ServiceResponse()
            {
                ErrorCode = (int)ErrorCodes.SpecialOfferError,
                ErrorMessage = new RezaB.Data.Localization.LocalizedList<ErrorCodes, ErrorMessages>().GetDisplayText((int)ErrorCodes.SpecialOfferError, CreateCulture(culture))
            };
        }
        public static ServiceResponse PaymentResponse(string culture, BillPayment.ResponseType responseType)
        {
            return new ServiceResponse()
            {
                ErrorCode = (int)ErrorCodes.Failed,
                ErrorMessage = new RezaB.Data.Localization.LocalizedList<BillPayment.ResponseType, ErrorMessages>().GetDisplayText((int)responseType, CreateCulture(culture))
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
}