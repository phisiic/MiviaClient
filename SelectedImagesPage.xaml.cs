using MiviaMaui.Bus;
using MiviaMaui.Dtos;
using MiviaMaui.Interfaces;
using MiviaMaui.Services;
using MiviaMaui.ViewModels;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;

namespace MiviaMaui.Views
{
    public partial class SelectedImagesPage : ContentPage
    {
        private readonly SelectedImagesViewModel _viewModel;

        public SelectedImagesPage(ModelService modelService, List<ImageDto> selectedImages)
        {
            InitializeComponent();

            _viewModel = new SelectedImagesViewModel(modelService, selectedImages,
                App.ServiceProvider.GetRequiredService<ICommandBus>(),
                App.ServiceProvider.GetRequiredService<IQueryBus>(),
                App.ServiceProvider.GetRequiredService<IImagePathService>(),
                App.ServiceProvider.GetRequiredService<INotificationService>());
            BindingContext = _viewModel;

#if ANDROID
            selectedImagesGrid.Span = 1;
#endif
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadModelsAsync();
        }

        private void OnImagesSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedImage = e.CurrentSelection.FirstOrDefault() as ImageDto;
            _viewModel.CurrentImage = selectedImage;
        }

        private void OnModelSelectionChanged(object sender, CheckedChangedEventArgs e)
        {
            if (sender is CheckBox checkBox &&
                checkBox.BindingContext is ModelDto model)
            {
                model.IsSelected = e.Value;
                _viewModel.HandleModelSelection(model);
            }
        }

        private async void OnFolderTapped(object sender, EventArgs e)
        {
            if (sender is Image image && image.GestureRecognizers.First() is TapGestureRecognizer tapGesture)
            {
                string folderPath = tapGesture.CommandParameter as string;

                var directoryPath = Path.GetDirectoryName(folderPath);

                if (Directory.Exists(directoryPath))
                {
                    var uri = new Uri($"file:///{directoryPath.Replace("\\", "/")}");
                    await Launcher.OpenAsync(uri);
                }
                else
                {
                    await DisplayAlert("Error", "Folder does not exist.", "OK");
                }
            }
        }

        private void OnFrameTapped(object sender, TappedEventArgs e)
        {
            if (sender is Frame frame && frame.BindingContext is ModelDto model)
            {
                model.IsSelected = !model.IsSelected;
                _viewModel.HandleModelSelection(model);

                if (frame.FindByName<CheckBox>("checkbox") is CheckBox checkbox)
                {
                    checkbox.IsChecked = model.IsSelected;
                }
            }
        }

        private async void OnProcessImagesClicked(object sender, EventArgs e)
        {
            try
            {
                await _viewModel.ProcessImagesAsync();
                await DisplayAlert("Success", "Images processed successfully", "OK");
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "An error occurred while processing images", "OK");
            }
        }

    }
}