using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace Studio.Services
{
    public class CustomSnackbarService
    {
        private SnackbarPresenter _presenter;

        private Snackbar _snackbar;

        public TimeSpan DefaultTimeOut { get; set; } = TimeSpan.FromSeconds(5);

        public void SetSnackbarPresenter(SnackbarPresenter contentPresenter)
        {
            _presenter = contentPresenter;
        }

        public SnackbarPresenter GetSnackbarPresenter()
        {
            return _presenter;
        }

        public void Show(bool immediate, Action<Snackbar> configure)
        {
            if (_presenter is null)
                throw new InvalidOperationException($"The SnackbarPresenter was never set");

            _snackbar ??= new Snackbar(_presenter);
            ConfigureDefaults(_snackbar);
            configure(_snackbar);
            _snackbar.Show(immediate);
        }

        private void ConfigureDefaults(Snackbar snackbar)
        {
            snackbar.Opacity = 0.9;
        }
    }
}
