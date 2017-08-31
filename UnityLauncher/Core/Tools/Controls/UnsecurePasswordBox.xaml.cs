using System;
using System.Windows;
using System.Windows.Controls;

namespace UnityLauncher.Core.Tools.Controls
{
    /// <summary>
    /// Interaction logic for UnsecurePasswordBox.xaml
    /// </summary>
    public partial class UnsecurePasswordBox
    {
        public static DependencyProperty passworDependencyProperty = DependencyProperty.Register("Password", typeof(string), typeof(UnsecurePasswordBox), new FrameworkPropertyMetadata("",
            (o, args) =>
            {
                var box = o as UnsecurePasswordBox;
                if (box != null)
                {
                    box.Password = args.NewValue as string;
                }
            })
        {
            BindsTwoWayByDefault = true
        });

        public static DependencyProperty showPasswordDependencyProperty = DependencyProperty.Register("ShowPassword", typeof(bool), typeof(UnsecurePasswordBox), new FrameworkPropertyMetadata(false,
            (o, args) =>
            {
                var box = o as UnsecurePasswordBox;
                if (box != null)
                {
                    box.ShowPassword = (bool)args.NewValue;
                }
            })
        {
            BindsTwoWayByDefault = true
        });

        public static DependencyProperty fontWeightDependencyProperty = DependencyProperty.Register("FontWeight", typeof(FontWeight), typeof(UnsecurePasswordBox), new FrameworkPropertyMetadata(FontWeights.Normal,
            (o, args) =>
            {
                var box = o as UnsecurePasswordBox;
                if (box != null)
                {
                    box.FontWeight = (FontWeight)args.NewValue;
                }
            })
        {
            BindsTwoWayByDefault = true
        });

        public new FontWeight FontWeight
        {
            get => (FontWeight) GetValue(fontWeightDependencyProperty);
            set
            {
                TargetPaswordBox.FontWeight = value;
                TargetTextBox.FontWeight = value;
                SetValue(fontWeightDependencyProperty, value);
            }
        }

        public bool ShowPassword
        {
            get => (bool) GetValue(showPasswordDependencyProperty);
            set
            {
                TargetPaswordBox.Visibility = value ? Visibility.Collapsed : Visibility.Visible;
                TargetTextBox.Visibility = !value ? Visibility.Collapsed : Visibility.Visible;
                SetValue(showPasswordDependencyProperty, value);
            }
        }

        public string Password
        {
            get => (string) GetValue(passworDependencyProperty);
            set
            {
                if (TargetPaswordBox.Password != value)
                {
                    TargetPaswordBox.Password = value;
                }
                if (TargetTextBox.Text != value)
                {
                    TargetTextBox.Text = value;
                }
                SetValue(passworDependencyProperty, value);
            }
        }

        public UnsecurePasswordBox()
        {
            InitializeComponent();
            TargetPaswordBox.PasswordChanged += OnPasswordChanged;
            TargetTextBox.TextChanged += OnTextChanged;
            TargetPaswordBox.Visibility = Visibility.Visible;
            TargetTextBox.Visibility = Visibility.Collapsed;
        }

        private void OnTextChanged(object sender, TextChangedEventArgs textChangedEventArgs)
        {
            Password = TargetTextBox.Text;
        }

        private void OnPasswordChanged(object sender, RoutedEventArgs routedEventArgs)
        {
            Password = TargetPaswordBox.Password;
        }
    }
}
