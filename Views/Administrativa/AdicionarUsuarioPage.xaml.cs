using MauiApp1.ViewModels.Administrativa;
using MauiApp1.Services.Admin;

namespace MauiApp1.Views.Administrativa 
{ 
    public partial class AdicionarUsuarioPage : ContentPage 
    { 
        private readonly AdicionarUsuarioViewModel _viewModel; 
        public AdicionarUsuarioPage(AdminService adminService) 
        { 
            InitializeComponent(); 
            _viewModel = new AdicionarUsuarioViewModel(adminService); 
            BindingContext = _viewModel; 
        } 
        private async void OnBackClicked(object sender, EventArgs e) 
        { 
            if (_viewModel.VoltarCommand.CanExecute(null)) 
            { 
                _viewModel.VoltarCommand.Execute(null); 
                return; 
            } 
            
            await Shell.Current.GoToAsync(".."); 
        } 
    } 
}