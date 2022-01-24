﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace QRBee.Services
{
    public class Settings
    {
        public string PIN            { get; set; }
        public bool IsRegistered     { get; set; }

        public string Name           { get; set; }
        public string Email          { get; set; }
        public string DateOfBirth    { get; set; }
        public string CardNumber     { get; set; }
        public string ValidFrom      { get; set; }
        public string ExpirationDate { get; set; }
        public string CardHolderName { get; set; }
        public string CVC            { get; set; }
        public string IssueNo        { get; set; }
        public string Password       { get; set; }
    }

    public interface ILocalSettings
    {
        string QRBeeApiUrl { get; }
        Task SaveSettings(Settings settings);
        Task<Settings> LoadSettings();


    }
}
