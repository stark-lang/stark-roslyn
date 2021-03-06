﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Composition;
using StarkPlatform.CodeAnalysis.Editor.Shared.Utilities;
using StarkPlatform.CodeAnalysis.Host;
using StarkPlatform.CodeAnalysis.Host.Mef;
using StarkPlatform.CodeAnalysis.Navigation;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;

namespace StarkPlatform.VisualStudio.LanguageServices.Implementation
{
    [ExportWorkspaceServiceFactory(typeof(IDocumentNavigationService), ServiceLayer.Host), Shared]
    internal sealed class VisualStudioDocumentNavigationServiceFactory : IWorkspaceServiceFactory
    {
        private readonly IDocumentNavigationService _singleton;

        [ImportingConstructor]
        [Obsolete(MefConstruction.ImportingConstructorMessage, error: true)]
        private VisualStudioDocumentNavigationServiceFactory(
            IThreadingContext threadingContext,
            SVsServiceProvider serviceProvider,
            IVsEditorAdaptersFactoryService editorAdaptersFactoryService)
        {
            _singleton = new VisualStudioDocumentNavigationService(threadingContext, serviceProvider, editorAdaptersFactoryService);
        }

        public IWorkspaceService CreateService(HostWorkspaceServices workspaceServices)
        {
            return _singleton;
        }
    }
}
