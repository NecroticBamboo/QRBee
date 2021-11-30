using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace QRBee.Services
{
    public interface IQRScanner
    {
        Task<string> ScanQR();
    }
}
