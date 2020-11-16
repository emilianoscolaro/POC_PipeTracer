using POC_PipeTracer.ViewModels;
using System.ComponentModel;
using Xamarin.Forms;

namespace POC_PipeTracer.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}