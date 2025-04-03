using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace WinFormsZOrderViewer
{
    /// <summary>
    /// Interaction logic for ZOrderToolWindowControl.
    /// </summary>
    public partial class ZOrderToolWindowControl : UserControl
    {
        static List<string> files = new List<string>();
        static List<System.Windows.Forms.Control> controls = new List<System.Windows.Forms.Control>();
        static Project clientProject = null;
        public ZOrderToolWindowControl()
        {
            this.InitializeComponent();
            DTE dte = (DTE)Marshal.GetActiveObject("VisualStudio.DTE");
            LoadForms(dte);
        }

        private void LoadForms(DTE dte)
        {
            foreach (Project project in dte.Solution.Projects) {

                clientProject = project;

                foreach (ProjectItem item in project.ProjectItems) {
                    if (item.FileNames[0].EndsWith(".cs"))
                        if (item.ProjectItems != null)
                            foreach (ProjectItem subitems in item.ProjectItems) {
                                if (subitems.FileNames[0].EndsWith(".Designer.cs"))
                                    files.Add(subitems.FileNames[0]);
                            }
                }
            }

            for (int i = 0; i < files.Count; i++) {
                 FormsComboBox.Items.Add(files[i].Remove(0, files[i].LastIndexOf("\\")+1));
            }
        }

        private void UpButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Up");
        }

        private void DownButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Down");
        }

        private void FormsComboBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            string file = "";
            try
            {
                // Reflection Aproach - Loads all the requested assemblies -> It's not capable of loading System.Object from the System.Private.CodeLib.dll assembly
                AppDomain.CurrentDomain.AssemblyResolve += ResolverEnsamblados;

                // Dinamicly Load Project's Assembly
                Assembly assembly = Assembly.LoadFrom(Path.Combine(Path.GetDirectoryName(clientProject.FullName), "bin\\Debug\\net8.0-windows\\" + clientProject.Properties.Item("OutputFilename").Value.ToString()));

                // Get the assembly name (with namespaces)
                file = files.Where(s => s.Contains(FormsComboBox.SelectedItem.ToString())).First();

                // Get the Type from the assembly
                Type formType = assembly.GetType(GetAssemblyName(file), true, true);

                // Create an object of the Type
                object formClass = Activator.CreateInstance(formType);

                // Fill the ListView with the project's Form Class Controls
                controls.Clear();
                if (formClass is System.Windows.Forms.Form form)
                {
                    foreach (System.Windows.Forms.Control control in form.Controls)
                    {
                        controls.Add(control);
                    }
                }

                ControlListView.Items.Clear();
                for (int i = 0; i < controls.Count; i++)
                {
                    ControlListView.Items.Add("(" + controls[i].GetType().Name + ") " + controls[i].Name);
                }
            } catch (Exception ex) {
                MessageBox.Show(ex.Message + ex.StackTrace);
            }
        }
        private static Assembly ResolverEnsamblados(object sender, ResolveEventArgs args)
        {
            string assemblyPathFrameWork = Path.Combine(@"C:\Program Files\dotnet\shared\Microsoft.NETCore.App\8.0.14\", new AssemblyName(args.Name).Name + ".dll");
            string assemblyPathForms = Path.Combine(@"C:\Program Files\dotnet\shared\Microsoft.WindowsDesktop.App\8.0.14\", new AssemblyName(args.Name).Name + ".dll");
            try
            {
                if (File.Exists(assemblyPathFrameWork))
                    return Assembly.LoadFile(assemblyPathFrameWork);
                else if (File.Exists(assemblyPathForms))
                    return Assembly.LoadFile(assemblyPathForms);
            }
            catch { return null; }

            return null;
        }
        static string GetAssemblyName(string filePath)
        {
            // Find the project root by looking for the .sln file
            string projectRoot = FindProjectRoot(filePath);

            if (string.IsNullOrEmpty(projectRoot))
            {
                throw new ArgumentException("Project root not found.");
            }

            // Get the relative path by trimming the project root part of the file path
            var fullPath = Path.GetFullPath(filePath);
            string relativePath = fullPath.Substring(projectRoot.Length + 1);

            // Remove the file extension and replace backslashes with dots
            string assemblyName = relativePath.Replace(Path.DirectorySeparatorChar, '.').Replace(".Designer.cs", "");

            // Assuming the project name is the first folder in the path
            string projectName = Path.GetFileName(projectRoot);

            // Combine project name with relative path to form the assembly name
            return $"{projectName}.{assemblyName}";
        }

        static string FindProjectRoot(string filePath)
        {
            // Start from the current file path and walk up the directory tree
            string currentDir = Path.GetDirectoryName(filePath);

            while (currentDir != null)
            {
                // Check if there is a solution file in the current directory
                string[] solutionFiles = Directory.GetFiles(currentDir, "*.csproj");

                if (solutionFiles.Length > 0)
                {
                    // We found the project root (the directory containing the .sln file)
                    return currentDir;
                }

                // Move up one level in the directory tree
                currentDir = Path.GetDirectoryName(currentDir);
            }

            // Return null if no project root is found
            return null;
        }
    }
}