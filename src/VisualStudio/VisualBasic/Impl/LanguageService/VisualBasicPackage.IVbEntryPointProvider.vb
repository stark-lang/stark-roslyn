﻿' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Runtime.InteropServices
Imports System.Threading
Imports Microsoft.VisualStudio.LanguageServices.VisualBasic.ProjectSystemShim
Imports Microsoft.VisualStudio.LanguageServices.VisualBasic.ProjectSystemShim.Interop
Imports Microsoft.VisualStudio.Shell.Interop

Namespace Microsoft.VisualStudio.LanguageServices.VisualBasic
    Partial Friend Class VisualBasicPackage
        Implements IVBEntryPointProvider

        Public Function GetFormEntryPointsList(<[In]> pHierarchy As Object,
                                               cItems As Integer,
                                               <Out> bstrList() As String,
                                               <Out> ByVal pcActualItems As IntPtr) As Integer Implements IVBEntryPointProvider.GetFormEntryPointsList

            Dim workspace = ComponentModel.GetService(Of VisualStudioWorkspace)()
            Dim hierarchy = CType(pHierarchy, IVsHierarchy)

            For Each projectId In workspace.CurrentSolution.ProjectIds
                Dim projectHierarchy = workspace.GetHierarchy(projectId)
                If hierarchy Is projectHierarchy Then
                    Dim compilation = workspace.CurrentSolution.GetProject(projectId).GetCompilationAsync(CancellationToken.None).WaitAndGetResult(CancellationToken.None)

                    VisualBasicProject.GetEntryPointsWorker(compilation, cItems, bstrList, pcActualItems, findFormsOnly:=True)
                End If
            Next

            Return VSConstants.S_OK
        End Function
    End Class
End Namespace
