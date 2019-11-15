using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using arte_7;

namespace MediaStreamer_SL
{
    public partial class MainPage : UserControl
    {
        private ViewModel _viewModel;

        public MainPage()
        {
            InitializeComponent();

            _viewModel = new ViewModel();
            _viewModel.LoadViewList(() => MessageBox.Show("Videos loaded."));


        }
    }
}
