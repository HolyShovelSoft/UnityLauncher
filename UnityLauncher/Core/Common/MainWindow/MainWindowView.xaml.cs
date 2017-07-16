namespace UnityLauncher
{
    /// <summary>
    /// Interaction logic for MainWindowView.xaml
    /// </summary>
    public partial class MainWindowView
    {
        private static MainWindowView _instance;
        public static MainWindowView Instance => _instance;

        public MainWindowView()
        {
            _instance = this;
            InitializeComponent();            
        }
        
    }
}
