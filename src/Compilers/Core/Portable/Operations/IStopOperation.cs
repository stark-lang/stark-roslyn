﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace StarkPlatform.CodeAnalysis.Operations
{
    /// <summary>
    /// Represents an operation to stop or suspend execution of code.
    /// <para>
    /// Current usage:
    ///  (1) VB Stop statement.
    /// </para>
    /// </summary>
    /// <remarks>
    /// This interface is reserved for implementation by its associated APIs. We reserve the right to
    /// change it in the future.
    /// </remarks>
    public interface IStopOperation : IOperation
    {
    }
}

