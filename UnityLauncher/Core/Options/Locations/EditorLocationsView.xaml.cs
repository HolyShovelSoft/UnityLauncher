using UnityLauncher.Interfaces;

namespace UnityLauncher.Core.Options
{
    public partial class EditorLocationsView : IInitializableView
    {
        public EditorLocationsView() { }

        public void Init(object dataContext)
        {
            DataContext = dataContext;
            InitializeComponent();
        }
    }
}
