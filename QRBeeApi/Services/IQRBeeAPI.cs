using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Mvc;
using QRBee.Core;
using QRBee.Core.Data;

namespace QRBee.Api.Services
{
    /// <summary>
    /// QRBeeAPIService interface
    /// </summary>
    public interface IQRBeeAPI
    {
        /// <summary>
        /// Handles Registration request
        /// </summary>
        /// <param name="value">Registration request</param>
        /// <returns>Registration response</returns>
        Task<RegistrationResponse> Register(RegistrationRequest value);

        /// <summary>
        /// Handles Update request
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="value">Update request</param>
        Task Update(string clientId, RegistrationRequest value);

        /// <summary>
        /// Handles InsertTransaction request
        /// </summary>
        /// <param name="value">Payment request</param>
        Task<PaymentResponse> Pay(PaymentRequest value);

    }
}
