// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n;

/// <summary>
/// Represents errors that occur during the execution of the localization builder.
/// </summary>
public class LocalizationBuilderException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizationBuilderException"/> class.
    /// </summary>
    /// <param name="message">The message describing what could not be loaded.</param>
    /// <param name="innerException">The underlying failure, such as a parser error.</param>
    public LocalizationBuilderException(string message, Exception innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizationBuilderException"/> class.
    /// </summary>
    /// <param name="message">The message describing what could not be loaded.</param>
    public LocalizationBuilderException(string message)
        : base(message) { }
};
