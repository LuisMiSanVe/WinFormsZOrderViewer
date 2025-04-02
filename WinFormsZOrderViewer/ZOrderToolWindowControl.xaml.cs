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
        static string projectAssembly = "";
        static List<System.Windows.Forms.Control> controls = new List<System.Windows.Forms.Control>();
        public ZOrderToolWindowControl()
        {
            this.InitializeComponent();
            DTE dte = (DTE)Marshal.GetActiveObject("VisualStudio.DTE");
            LoadForms(dte);
        }

        private void LoadForms(DTE dte)
        {
            foreach (Project project in dte.Solution.Projects) {

                projectAssembly = Path.Combine(Path.GetDirectoryName(project.FullName), "bin\\Debug\\net8.0-windows\\" + project.Properties.Item("OutputFilename").Value.ToString());

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
                // Load Assembly's dependencies
                Assembly.Load("System.Windows.Forms");
                Assembly assembly = Assembly.LoadFrom(projectAssembly);

                // Search the class path of the selected Forms
                file = files.Where(s => s.Contains(FormsComboBox.SelectedItem.ToString())).First();


                Type formType = assembly.GetType("WinFormsApp1.Form1", true, true);
                object formClass = Activator.CreateInstance(formType);

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
                MessageBox.Show(ex.Message + ex.GetType().FullName + ex.StackTrace);
            }
        }
    }
}