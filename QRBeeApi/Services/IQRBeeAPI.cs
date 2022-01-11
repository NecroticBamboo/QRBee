using Microsoft.AspNetCore.Mvc;
using QRBee.Core;
using QRBee.Core.Data;

namespace QRBee.Api.Services
{
    /// <summary>
    /// QRBeeAPI interface
    /// </summary>
    public interface IQRBeeAPI
    {
        /// <summary>
        /// Handles Registration request
        /// </summary>
        /// <param name="value">Registration request</param>
        /// <returns>Registration response</returns>
        Task<RegistrationResponse> Register(RegistrationRequest value);

    }
}
