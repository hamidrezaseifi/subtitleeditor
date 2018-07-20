using Microsoft.Win32;
using System;
using System.Collections.Generic;
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

namespace SubtitleEditor
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SubtitleViewModel _viewModel;

        public MainWindow()
        {
            _viewModel = new SubtitleViewModel();

            InitializeComponent();
        }

        public SubtitleViewModel ViewModel
        {
            get
            {
                return _viewModel;
            }
            set
            {
                _viewModel = value;
            }
        }

        private void btnBrows_Click(object sender, RoutedEventArgs e)
        {
            string initdir = "d:\\media";
            if(System.IO.File.Exists(txtFilePath.Text))
            {
                System.IO.FileInfo fi = new System.IO.FileInfo(txtFilePath.Text);
                initdir = fi.Directory.FullName;
            }

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.FileName = txtFilePath.Text;
            openFileDialog.Filter = "Srt files (*.srt)|*.srt|All files (*.*)|*.*";
            openFileDialog.InitialDirectory = initdir;
            openFileDialog.Multiselect = false;

            if (openFileDialog.ShowDialog() == true)
            {
                txtFilePath.Text = openFileDialog.FileName;
                LoadFile();

                
            }
                
        }

        private void btnReload_Click(object sender, RoutedEventArgs e)
        {
            LoadFile();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if(ViewModel.Items.Count == 0)
            {
                return;
            }
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = txtFilePath.Text;
            saveFileDialog.Filter = "Srt files (*.srt)|*.srt|All files (*.*)|*.*";
            
            if (saveFileDialog.ShowDialog() == true)
            {
                ViewModel.Save(saveFileDialog.FileName);
            }
        }

        private void LoadFile()
        {
            _viewModel.Load(txtFilePath.Text);
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            int seconds;
            if (int.TryParse(txtSeconds.Text, out seconds))
            {
                if (seconds != 0)
                {
                    _viewModel.AddSeconds(seconds);
                }
            }
        }

        private void DelItem_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;

            int index = int.Parse(btn.Tag.ToString());

            ViewModel.RemoveAt(index);

            //lstItems.GetBindingExpression(ListView.ItemsSourceProperty).UpdateTarget();
            lstItems.ItemsSource = null;
            lstItems.ItemsSource = ViewModel.Items;
        }

        private void btnConvert_Click(object sender, RoutedEventArgs e)
        {
            float f = float.Parse(txtFrom.Text.Replace(".", ","));
            float t = float.Parse(txtTo.Text.Replace(".", ","));

            ViewModel.ChangeRate(f, t);
        }
    }
}
