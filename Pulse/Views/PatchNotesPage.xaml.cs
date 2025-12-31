using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

namespace Studio.Views
{
    /// <summary>
    /// Interaction logic for PatchNotesPage.xaml
    /// </summary>
    public partial class PatchNotesPage : Page
    {
        public PatchNotesPage()
        {
            InitializeComponent();
            InitializeWebView();
        }

        private async void InitializeWebView()
        {
            await WebView.EnsureCoreWebView2Async();

            string htmlFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "PatchNotesWebView", "index.html");
            Debug.WriteLine(htmlFilePath);
            string uri = new Uri(htmlFilePath).AbsoluteUri;

            WebView.Source = new Uri(uri);
        }

        private void WebView_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                WebView.CoreWebView2.WebMessageReceived += (s, args) =>
                {
                    string message = args.WebMessageAsJson;
                    MessageBox.Show($"Received from JS: {message}");
                };
            }
        }
    }
}
