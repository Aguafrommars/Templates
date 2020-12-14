// Copyright (c) 2020 @Olivier Lefebvre. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;
using System.Runtime.Serialization;

namespace TIS.Quickstart.Account
{
    /// <summary>
    /// Trowed when user click on malicious link
    /// </summary>
    /// <seealso cref="System.Exception" />
    [Serializable]
    public class InvalidReturnUrlException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidReturnUrlException"/> class.
        /// </summary>
        public InvalidReturnUrlException() : this("invalid return URL")
        {
        }

        public InvalidReturnUrlException(string message) : this(message, null)
        {
        }

        public InvalidReturnUrlException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidReturnUrlException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }
    }
}
