using System.Collections;
using System.Windows;
using System.Windows.Controls;
using UnityLauncher.Interfaces;

namespace UnityLauncher.Core.Commands.RenderMode
{
    /// <summary>
    /// Interaction logic for RenderModeSelect.xaml
    /// </summary>
    public partial class RenderModeSelect : ICommand
    {
        private RenderMode selected;
     
        public RenderModeSelect()
        {
            InitializeComponent();
            FillComboBox();
        }

        private void FillComboBox()
        {
            var comboBox = FindName("ComboBox") as ComboBox;

            if (comboBox != null)
            {
                comboBox.Items.Add(selected = new RenderMode("---", ""));
                comboBox.Items.Add(new RenderMode("DirectX 9", "-force-d3d9"));
                comboBox.Items.Add(new RenderMode("DirectX 11", "-force-d3d11"));
                comboBox.Items.Add(new RenderMode("OpenGL (last availible)", "-force-glcore"));
                comboBox.Items.Add(new RenderMode("OpenGL 3.2", "-force-glcore32"));
                comboBox.Items.Add(new RenderMode("OpenGL 3.3", "-force-glcore33"));
                comboBox.Items.Add(new RenderMode("OpenGL 4.0", "-force-glcore40"));
                comboBox.Items.Add(new RenderMode("OpenGL 4.1", "-force-glcore41"));
                comboBox.Items.Add(new RenderMode("OpenGL 4.2", "-force-glcore42"));
                comboBox.Items.Add(new RenderMode("OpenGL 4.3", "-force-glcore43"));
                comboBox.Items.Add(new RenderMode("OpenGL 4.4", "-force-glcore44"));
                comboBox.Items.Add(new RenderMode("OpenGL 4.5", "-force-glcore45"));
                comboBox.Items.Add(new RenderMode("OpenGL ES (last availible)", "-force-gles"));
                comboBox.Items.Add(new RenderMode("OpenGL ES 3.0", "-force-gles30"));
                comboBox.Items.Add(new RenderMode("OpenGL ES 3.1", "-force-gles31"));
                comboBox.Items.Add(new RenderMode("OpenGL ES 3.2", "-force-gles32"));

                var lastSelected = Settings.GetSetting(this, "LastSelected");

                var idx = 0;

                for (int i = 1; i <= ComboBox.Items.Count - 1; i++)
                {
                    if (((RenderMode) ComboBox.Items[i]).Value == lastSelected)
                    {
                        idx = i;
                    }
                }

                if (idx == 0)
                {
                    Settings.RemoveSetting(this, "LastSelected");
                }

                comboBox.SelectedIndex = idx;
            }
        }

        public FrameworkElement GetControl()
        {
            return this;
        }

        public void OnSelectedEditorUpdate(EditorInfo targetInfo)
        {
        }

        public bool IsValid => true;

        public string GetCommandLineArguments()
        {
            return selected == null?"":selected.Command;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selected = ((ComboBox)sender).Items[((ComboBox)sender).SelectedIndex] as RenderMode;
            if (selected == null || selected.Value == "---")
            {
                Settings.RemoveSetting(this, "LastSelected");
            }
            else
            {
                Settings.SaveSetting(this, "LastSelected", selected.Value);
            }
        }
    }
}
