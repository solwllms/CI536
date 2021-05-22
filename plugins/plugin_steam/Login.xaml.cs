using ModernWpf.Controls;
using plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CI536
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Login : ContentDialog
    {
        AUTH_MODE authMode;

        public Login()
        {
            InitializeComponent();
        }

        public void UpdateStage(AUTH_MODE authMode, string error)
        {
            this.authMode = authMode;

            this.lbl_error.Content = error;

            this.lbl_auth.Visibility = authMode != AUTH_MODE.NONE ? Visibility.Visible : Visibility.Hidden;
            this.txt_auth.Visibility = authMode != AUTH_MODE.NONE ? Visibility.Visible : Visibility.Hidden;

            IsEnabled = true;
        }

        private void ButtonLogin(object sender, RoutedEventArgs e)
        {
            PluginDLL.Login(txt_username.Text, txt_password.Password,
                authMode == AUTH_MODE.APP ? txt_auth.Text : null,
                authMode == AUTH_MODE.EMAIL ? txt_auth.Text : null);
            IsEnabled = false;
        }

        private void ButtonCancel(object sender, RoutedEventArgs e)
        {
            Hide();
        }
    }
}
