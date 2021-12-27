using Microsoft.AspNetCore.Mvc;
using QRBee.Core;
using QRBee.Core.Data;

namespace QRBee.Api.Services
{
    public interface IQRBeeAPI
    {
        Task<RegistrationResponse> Register(RegistrationRequest value);

    }
}
