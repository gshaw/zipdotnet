// Copyright (C) 2001 Gerry Shaw
//
// This software is provided 'as-is', without any express or implied
// warranty.  In no event will the authors be held liable for any damages
// arising from the use of this software.
//
// Permission is granted to anyone to use this software for any purpose,
// including commercial applications, and to alter it and redistribute it
// freely, subject to the following restrictions:
//
// 1. The origin of this software must not be misrepresented; you must not
//    claim that you wrote the original software. If you use this software
//    in a product, an acknowledgment in the product documentation would be
//    appreciated but is not required.
// 2. Altered source versions must be plainly marked as such, and must not be
//    misrepresented as being the original software.
// 3. This notice may not be removed or altered from any source distribution.
//
// Gerry Shaw (gerry_shaw@yahoo.com)

using System;
using System.Runtime.Serialization;

namespace OrganicBit.Zip {

    /// <summary>Thrown whenever an error occurs during the build.</summary>
    [Serializable]
    public class ZipException : ApplicationException {

        /// <summary>Constructs an exception with no descriptive information.</summary>
        public ZipException() : base() {
        }

        /// <summary>Constructs an exception with a descriptive message.</summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public ZipException(String message) : base(message) {
        }

        /// <summary>Constructs an exception with a descriptive message and a reference to the instance of the <c>Exception</c> that is the root cause of the this exception.</summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">An instance of <c>Exception</c> that is the cause of the current Exception. If <paramref name="innerException"/> is non-null, then the current Exception is raised in a catch block handling <paramref>innerException</paramref>.</param>
        public ZipException(String message, Exception innerException) : base(message, innerException) {
        }

        /// <summary>Initializes a new instance of the BuildException class with serialized data.</summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        public ZipException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }
    }
}
