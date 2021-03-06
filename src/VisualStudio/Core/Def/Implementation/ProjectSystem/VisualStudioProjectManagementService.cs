﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using EnvDTE;
using StarkPlatform.CodeAnalysis;
using StarkPlatform.CodeAnalysis.Editor.Shared.Utilities;
using StarkPlatform.CodeAnalysis.Host.Mef;
using StarkPlatform.CodeAnalysis.ProjectManagement;
using StarkPlatform.VisualStudio.LanguageServices.Implementation.ProjectSystem;
using StarkPlatform.VisualStudio.LanguageServices.Implementation.ProjectSystem.Extensions;
using Roslyn.Utilities;

namespace Roslyn.VisualStudio.Services.Implementation.ProjectSystem
{
    [ExportWorkspaceService(typeof(IProjectManagementService), ServiceLayer.Host), Shared]
    internal class VisualStudioProjectManagementService : ForegroundThreadAffinitizedObject, IProjectManagementService
    {
        [ImportingConstructor]
        [Obsolete(MefConstruction.ImportingConstructorMessage, error: true)]
        public VisualStudioProjectManagementService(IThreadingContext threadingContext)
            : base(threadingContext)
        {
        }

        public string GetDefaultNamespace(StarkPlatform.CodeAnalysis.Project project, Workspace workspace)
        {
            this.AssertIsForeground();

            var folders = new List<string>();
            var defaultNamespace = "";

            if (workspace is VisualStudioWorkspaceImpl vsWorkspace)
            {
                vsWorkspace.GetProjectData(project.Id,
                    out var hierarchy, out var envDTEProject);

                try
                {
                    defaultNamespace = (string)envDTEProject.ProjectItems.ContainingProject.Properties.Item("DefaultNamespace").Value; // Do not Localize
                }
                catch (ArgumentException)
                {
                    // DefaultNamespace does not exist for this project.
                }
            }

            return defaultNamespace;
        }

        public IList<string> GetFolders(ProjectId projectId, Workspace workspace)
        {
            var folders = new List<string>();

            if (workspace is VisualStudioWorkspaceImpl vsWorkspace)
            {
                vsWorkspace.GetProjectData(projectId,
                    out var hierarchy, out var envDTEProject);

                var projectItems = envDTEProject.ProjectItems;

                var projectItemsStack = new Stack<Tuple<ProjectItem, string>>();

                // Populate the stack
                projectItems.OfType<ProjectItem>().Where(n => n.IsFolder()).Do(n => projectItemsStack.Push(Tuple.Create(n, "\\")));
                while (projectItemsStack.Count != 0)
                {
                    var projectItemTuple = projectItemsStack.Pop();
                    var projectItem = projectItemTuple.Item1;
                    var currentFolderPath = projectItemTuple.Item2;

                    var folderPath = currentFolderPath + projectItem.Name + "\\";

                    folders.Add(folderPath);
                    projectItem.ProjectItems.OfType<ProjectItem>().Where(n => n.IsFolder()).Do(n => projectItemsStack.Push(Tuple.Create(n, folderPath)));
                }
            }

            return folders;
        }
    }
}
