using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Shell;
using Wpf.Ui.Controls;

namespace Studio.Controls
{
    /// <summary>
    /// Interaction logic for AddAccountPrompt.xaml
    /// </summary>
    public partial class AddAccountPrompt : ContentDialog
    {
        private bool isInputValid = false;
        public AddAccountPrompt(ContentPresenter contentPresenter)
        {
            InitializeComponent();
            DialogHost = contentPresenter;
            IsPrimaryButtonEnabled = true;
            PrimaryButtonText = "Continue";
        }

        private void OnBattleTagInputTextChanged(object sender, TextChangedEventArgs e)
        {
            string text = BattletagInputBox.Text;
            if (text != "")
            {
                string[] parts = text.Split("#");
                if (parts.Length == 2)
                {
                    if (int.TryParse(parts[1], out _))
                    {
                        isInputValid = true;
                        Debug.WriteLine(parts.Length);
                        return;
                    }

                }
            }

            isInputValid = false;
        }

        protected override void OnButtonClick(ContentDialogButton button)
        {
            if (isInputValid || button == ContentDialogButton.Secondary)
            {
                base.OnButtonClick(button);
                return;
            }

            ErrorText.SetCurrentValue(VisibilityProperty, Visibility.Visible);
            _ = BattletagInputBox.Focus();
        }
    }
}
