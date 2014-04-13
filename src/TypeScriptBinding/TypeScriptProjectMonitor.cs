﻿// 
// TypeScriptProjectMonitor.cs
// 
// Author:
//   Matt Ward <ward.matt@gmail.com>
// 
// Copyright (C) 2013 Matthew Ward
// 
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using ICSharpCode.TypeScriptBinding.Hosting;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.Projects;

namespace ICSharpCode.TypeScriptBinding
{
	public class TypeScriptProjectMonitor
	{
		TypeScriptContextProvider contextProvider;
		
		public TypeScriptProjectMonitor(TypeScriptContextProvider contextProvider)
		{
			this.contextProvider = contextProvider;
			IdeApp.Workspace.SolutionLoaded += SolutionLoaded;
			IdeApp.Workspace.SolutionUnloaded += SolutionClosed;
			IdeApp.Workspace.FileRemovedFromProject += ProjectItemRemoved;
			IdeApp.Workspace.FileAddedToProject += ProjectItemAdded;
		}
		
		void SolutionLoaded(object sender, SolutionEventArgs e)
		{
			foreach (Project project in e.Solution.GetAllProjects()) {
				CreateTypeScriptContextIfProjectHasTypeScriptFiles(project);
			}
		}
		
		void CreateTypeScriptContextIfProjectHasTypeScriptFiles(Project project)
		{
			var typeScriptProject = new TypeScriptProject(project);
			if (typeScriptProject.HasTypeScriptFiles()) {
				contextProvider.CreateProjectContext(typeScriptProject);
			}
		}
		
		void SolutionClosed(object sender, EventArgs e)
		{
			contextProvider.DisposeAllProjectContexts();
		}
		
		void ProjectItemRemoved(object sender, ProjectFileEventArgs e)
		{
			foreach (ProjectFileEventInfo info in e) {
				RemoveTypeScriptFileFromContext(info.ProjectFile.FilePath);
			}
		}
		
		void RemoveTypeScriptFileFromContext(FilePath fileName)
		{
			if (TypeScriptParser.IsTypeScriptFileName(fileName)) {
				TypeScriptContext context = TypeScriptService.ContextProvider.GetContext(fileName);
				context.RemoveFile(fileName);
			}
		}
		
		void ProjectItemAdded(object sender, ProjectFileEventArgs e)
		{
			foreach (ProjectFileEventInfo info in e) {
				AddTypeScriptFileToContext(info.Project, info.ProjectFile.FilePath);
			}
		}
		
		void AddTypeScriptFileToContext(Project project, FilePath fileName)
		{
			if (TypeScriptParser.IsTypeScriptFileName(fileName)) {
				var typeScriptProject = new TypeScriptProject(project);
				TypeScriptService.ContextProvider.AddFileToProjectContext(typeScriptProject, fileName);
			}
		}
	}
}
