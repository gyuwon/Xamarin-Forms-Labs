using System;
using System.Linq;
using Xamarin.Forms.Labs.Sample.ViewModel;

namespace Xamarin.Forms.Labs.Sample.Pages.Controls
{
    public partial class InfiniteScrollPage
    {
        public InfiniteScrollPage()
        {
            InitializeComponent();

            var viewModel = new InfiniteScrollViewModel();
            BindingContext = viewModel;
            Appearing += (s, e) => viewModel.Initialize.Execute(null);
        }
    }
}