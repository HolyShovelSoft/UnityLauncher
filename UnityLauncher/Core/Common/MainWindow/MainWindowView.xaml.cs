using System;
using System.Windows;
using UnityLauncher.Interfaces;

namespace UnityLauncher
{
    /// <summary>
    /// Interaction logic for MainWindowView.xaml
    /// </summary>
    public partial class MainWindowView : IInitializableView
    {
        private static MainWindowView _instance;
        public static MainWindowView Instance => _instance;
        public event Action OnCloseEvent; 

        public MainWindowView()
        {
            _instance = this;
        }
        
        public void Init(object dataContext)
        {
            DataContext = dataContext;
            InitializeComponent();
        }

        protected override void OnClosed(EventArgs e)
        {
            OnCloseEvent?.Invoke();

            base.OnClosed(e);

            Application.Current.Shutdown();
        }
    }
}
