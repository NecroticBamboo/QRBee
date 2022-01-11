using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace QRBee.Services
{
    /// <summary>
    /// Interface for QR scanning
    /// </summary>
    public interface IQRScanner
    {
        /// <summary>
        /// Scan QR Code
        /// </summary>
        /// <returns></returns>
        Task<string> ScanQR();
    }
}
