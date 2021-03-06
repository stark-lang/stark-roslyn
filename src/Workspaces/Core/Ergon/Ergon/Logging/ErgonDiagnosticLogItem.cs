﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

namespace StarkPlatform.CodeAnalysis.Ergon.Logging
{
    internal class ErgonDiagnosticLogItem : DiagnosticLogItem
    {
        public string FileName { get; }
        public int LineNumber { get; }
        public int ColumnNumber { get; }

        public ErgonDiagnosticLogItem(WorkspaceDiagnosticKind kind, string projectFilePath, string message, string fileName, int lineNumber, int columnNumber)
            : base(kind, message, projectFilePath)
        {
            FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
            LineNumber = lineNumber;
            ColumnNumber = columnNumber;
        }

        public override string ToString()
            => $"{FileName}: ({LineNumber}, {ColumnNumber}): {Message}";
    }
}
