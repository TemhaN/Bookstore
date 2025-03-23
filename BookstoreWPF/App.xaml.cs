using System.Windows;

namespace BookstoreWPF
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            new LoginRegisterWindow().Show();
        }
    }
}