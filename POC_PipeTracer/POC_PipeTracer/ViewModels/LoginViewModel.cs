using POC_PipeTracer.Views;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace POC_PipeTracer.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        public Command LoginCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new Command(OnLoginClicked);
        }

        private  void OnLoginClicked(object obj)
        {
            INeoReader callService = DependencyService.Get<INeoReader>();
            callService.ReadDM();
        }
    }
}
