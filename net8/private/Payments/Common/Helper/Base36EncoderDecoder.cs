// <copyright file="Base36EncoderDecoder.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Helper
{   
    using System;
    using System.Text;
    using Microsoft.Commerce.Payments.Common.Tracing;

    public static class Base36EncoderDecoder
    {
        private const int MaxStringLength = 12;
        private const int BaseNumber = 36;
        private static char[] alphabet = new char[36] 
        { 
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 
            'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 
            'U', 'V', 'W', 'X', 'Y', 'Z' 
        };
        
        public static string Encode(ulong number, int length)
        {
            StringBuilder builder = new StringBuilder();
            do
            {
                builder.Insert(0, alphabet[number % (ulong)BaseNumber]);
                number /= (ulong)BaseNumber;
            }
            while (number != 0);

            builder.Insert(0, alphabet[0].ToString(), length - builder.Length);
            
            return builder.ToString();
        }

        public static string Encode(ulong number)
        {
            StringBuilder builder = new StringBuilder();
            do
            {
                builder.Insert(0, alphabet[number % (ulong)BaseNumber]);
                number /= (ulong)BaseNumber;
            }
            while (number != 0);

            return builder.ToString();
        }

        public static ulong Decode(string value, EventTraceActivity traceActivityId)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw TraceCore.TraceException(
                    traceActivityId, 
                    new ArgumentException(string.Format("input string is null or just white spaces: '{0}'", value ?? "NULL"), "value"));
            }
            
            if (value.Length > MaxStringLength)
            {
                throw TraceCore.TraceException(
                    traceActivityId,
                    new ArgumentException(string.Format("input string is tool long: '{0}'", value), "value"));
            }
            
            ulong number = 0;
            foreach (char token in value)
            {
                int mod = Array.IndexOf(alphabet, char.ToUpperInvariant(token));
                if (mod < 0)
                {
                    throw TraceCore.TraceException(
                        traceActivityId, 
                        new ArgumentException(string.Format("Invalid character(s) in the input string: '{0}'", value), "value"));
                }

                number = (number * BaseNumber) + ((ulong)mod);
            }

            return number;
        }
    }
}
