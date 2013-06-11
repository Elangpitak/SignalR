﻿using System;
using System.Text;
using Microsoft.Owin.Security.DataProtection;

namespace Microsoft.AspNet.SignalR.Infrastructure
{
    public class DataProtectionProviderProtectedData : IProtectedData
    {
        private static readonly UTF8Encoding _encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

        private readonly Func<string[], IDataProtector> _provider;

        // Known protected data providers
        private readonly IDataProtector _connectionTokenProtector;
        private readonly IDataProtector _groupsProtector;

        public DataProtectionProviderProtectedData(Func<string[], IDataProtector> provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }

            _provider = provider;
            _connectionTokenProtector = provider(new[] { Purposes.ConnectionToken });
            _groupsProtector = provider(new[] { Purposes.Groups });
        }

        public string Protect(string data, string purpose)
        {
            IDataProtector protector = GetDataProtector(purpose);

            byte[] unprotectedBytes = _encoding.GetBytes(data);

            byte[] protectedBytes = protector.Protect(unprotectedBytes);

            return Convert.ToBase64String(protectedBytes);
        }

        public string Unprotect(string protectedValue, string purpose)
        {
            IDataProtector protector = GetDataProtector(purpose);

            byte[] protectedBytes = Convert.FromBase64String(protectedValue);

            byte[] unprotectedBytes = protector.Unprotect(protectedBytes);

            return _encoding.GetString(unprotectedBytes);
        }

        private IDataProtector GetDataProtector(string purpose)
        {
            switch (purpose)
            {
                case Purposes.ConnectionToken:
                    return _connectionTokenProtector;
                case Purposes.Groups:
                    return _groupsProtector;
            }

            return _provider(new[] { purpose });
        }
    }
}
