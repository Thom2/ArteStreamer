using System;
using System.Windows;
using System.Reflection;
using System.ComponentModel;
using System.Windows.Data;
using System.IO;


namespace arte_7
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        private ViewModel _viewModel;

        public Window1()
        {
            InitializeComponent();

            listBox1.Items.SortDescriptions.Clear();
            listBox1.Items.SortDescriptions.Add(new SortDescription("InternalDateTime", ListSortDirection.Descending));

            _viewModel = new ViewModel();
            _viewModel.Error += new EventHandler<VideoStreamerEventHandlerArgs>(OnVideoStreamerError);


            _viewModel.DestinationPath = External.GetSpecialFolderPath(SpecialFolderType.MyVideo) + "\\Arte+7";
            if (!Directory.Exists(_viewModel.DestinationPath))
            {
                Directory.CreateDirectory(_viewModel.DestinationPath);
            }

            // Set view model
            this.DataContext = _viewModel;

            UpdateButtonState();

            // Load first video page
            LoadVideoList();
        }

        private void LoadVideoList()
        {
            SetUpdateStarted();
            _viewModel.LoadInitialVideoList(() => SetUpdateFinished());
        }

        private void LoadNextVideoPage()
        {
            SetUpdateStarted();
            _viewModel.LoadNextVideoPage(() => SetUpdateFinished());
        }

        private void LoadAllVideoPages()
        {
            SetUpdateStarted();
            _viewModel.LoadAllVideoPages(() => SetUpdateFinished());
        }

        private void SetUpdateStarted()
        {
            labelStatus.Content = "Updating, please wait...";
            buttonReload.IsEnabled = false;
            buttonDownload.IsEnabled = false;
            buttonLoadNextPage.IsEnabled = false;
            buttonLoadAll.IsEnabled = false;
        }

        private void SetUpdateFinished()
        {
            labelStatus.Content = "Arte Video List Updated";
            buttonReload.IsEnabled = true;
            buttonDownload.IsEnabled = true;

            UpdateButtonState();
        }

        private void UpdateButtonState()
        {
            if (_viewModel.VideoList.LoadedVideoPages >= _viewModel.VideoList.NumberOfVideoPages)
            {
                buttonLoadNextPage.IsEnabled = false;
                buttonLoadAll.IsEnabled = false;
            }
            else
            {
                buttonLoadNextPage.IsEnabled = true;
                buttonLoadAll.IsEnabled = true;
            }
        }

        void OnVideoStreamerError(object sender, VideoStreamerEventHandlerArgs e)
        {
            DisplayError(e.Error.Message);
        }

        private void buttonReload_Click(object sender, RoutedEventArgs e)
        {
            LoadVideoList();
        }

        private void DisplayError(string error)
        {
            labelStatus.Content = error;
        }

        private void buttonDownload_Click(object sender, RoutedEventArgs e)
        {
            if (listBox1.SelectedIndex == -1)
            {
                return;
            }

            _viewModel.DownloadVideo((Video)listBox1.SelectedItem);
        }


        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void buttonOptions_Click(object sender, RoutedEventArgs e)
        {
            OptionsView view = new OptionsView();
            view.ShowDialog();
        }

        private void listBox1_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            //if (listBox1.SelectedIndex != -1)
            //    label2.Content = ((Video)listBox1.SelectedItem).TeaserText;
        }

        private void buttonLoadNextPage_Click(object sender, RoutedEventArgs e)
        {
            LoadNextVideoPage();
        }

        private void buttonLoadAll_Click(object sender, RoutedEventArgs e)
        {
            LoadAllVideoPages();
        }

        private void comboBoxFilterDate_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            _viewModel.FilterVideoList((string)comboBoxFilterDate.SelectedValue);
        }
    }
}
