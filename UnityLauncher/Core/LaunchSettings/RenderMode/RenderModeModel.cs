using System;
using System.Collections.ObjectModel;
using System.Linq;
using UnityLauncher.Interfaces;

namespace UnityLauncher.Core.LaunchSettings
{
    public class RenderModeModel : EnableableModel, ILaunchCommandSource
    {
        public event Action OnSelectedChange;

        public ObservableCollection<RenderModeInfo> modes = new ObservableCollection<RenderModeInfo>();

        private RenderModeInfo selected;
        public RenderModeInfo Selected
        {
            get => selected;
            set
            {
                if (selected == value) return;
                selected = value;
                Settings.Instance.SaveSetting(Context, "LastSelected", selected?.Value);
                OnSelectedChange?.Invoke();
            }
        }

        public override void Init(IContext context)
        {
            base.Init(context);
            
            modes.Add(new RenderModeInfo("DirectX 9", "-force-d3d9", context));
            modes.Add(new RenderModeInfo("DirectX 11", "-force-d3d11", context));
            modes.Add(new RenderModeInfo("OpenGL (last availible)", "-force-glcore", context));
            modes.Add(new RenderModeInfo("OpenGL 3.2", "-force-glcore32", context));
            modes.Add(new RenderModeInfo("OpenGL 3.3", "-force-glcore33", context));
            modes.Add(new RenderModeInfo("OpenGL 4.0", "-force-glcore40", context));
            modes.Add(new RenderModeInfo("OpenGL 4.1", "-force-glcore41", context));
            modes.Add(new RenderModeInfo("OpenGL 4.2", "-force-glcore42", context));
            modes.Add(new RenderModeInfo("OpenGL 4.3", "-force-glcore43", context));
            modes.Add(new RenderModeInfo("OpenGL 4.4", "-force-glcore44", context));
            modes.Add(new RenderModeInfo("OpenGL 4.5", "-force-glcore45", context));
            modes.Add(new RenderModeInfo("OpenGL ES (last availible)", "-force-gles", context));
            modes.Add(new RenderModeInfo("OpenGL ES 3.0", "-force-gles30", context));
            modes.Add(new RenderModeInfo("OpenGL ES 3.1", "-force-gles31", context));
            modes.Add(new RenderModeInfo("OpenGL ES 3.2", "-force-gles32", context));

            var lastSelected = Settings.Instance.GetSetting<string>(Context, "LastSelected");
            var tmp = modes.FirstOrDefault(info => info.Value == lastSelected);

            Selected = tmp;
        }

        public string CommandLineValue => string.IsNullOrEmpty(Selected?.Command) || !IsEnabled ? "" : Selected.Command;
    }
}