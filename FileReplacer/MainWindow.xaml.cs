using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using Path = System.IO.Path;
using Serilog;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace FileReplacer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDisposable
    {
        private ObservableCollection<Source> _sourcePaths = new();
        private ObservableCollection<Destination> _destinationPaths = new();
        private bool _disposedValue;

        public MainWindow()
        {
            InitializeComponent();

            ViewModel vmObj = this.DataContext as ViewModel;
            vmObj.IsPopupVisible = Visibility.Hidden;

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File("filereplacer.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog? openFileDlg;
            try
            {
                // Create OpenFileDialog
                openFileDlg = new OpenFileDialog
                {

                    // Configure OpenFileDialog
                    Multiselect = true,
                    Filter = "All files (*.*)|*.*"
                };

                // Launch OpenFileDialog by calling ShowDialog method
                bool? result = openFileDlg.ShowDialog();

                // Get the selected file name and display in a TextBox.
                // Load content of file in a TextBlock
                if (!result.HasValue || !result.Value) return;

                foreach (string name in openFileDlg.FileNames)
                {
                    _sourcePaths.Add(new Source() { Path = name });
                }

                _sourcePaths = (ObservableCollection<Source>)_sourcePaths.Distinct(new SourceComparer());
                gridSource.ItemsSource = _sourcePaths;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to read files");
                Log.Error(ex, "Error occurred while clicking browse button");
            }
            finally
            {
                openFileDlg = null;
            }
        }

        private void btnReplace_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // reads input files in an array
                string[] filesToCopy = ((ObservableCollection<Source>)gridSource.ItemsSource).Where(a => !string.IsNullOrEmpty(a.Path)).Select(c => c.Path).ToArray();
                string[] destLocations = ((ObservableCollection<Destination>)gridDestinations.ItemsSource).Where(a => !string.IsNullOrEmpty(a.Path)).Select(c => c.Path).ToArray();

                // calculating the progress percentage
                int total = destLocations.Length * filesToCopy.Length;
                int i = 1;

                // loops through destination folders
                foreach (string dest in destLocations)
                {
                    foreach (string file in filesToCopy)
                    {
                        if (!Directory.Exists(dest))
                        {
                            Log.Information($"The file path is {dest}");
                            MessageBox.Show("The directory path is either invalid or is not present");
                            return;
                        }
                        // separating the filename from path
                        string fileName = file.Split("\\", StringSplitOptions.RemoveEmptyEntries)?.LastOrDefault();

                        if (txtBackup.Visibility == Visibility.Visible && !string.IsNullOrEmpty(txtBackup.Text))
                        {
                            if (!Directory.Exists(txtBackup.Text))
                            {
                                Log.Information($"The backup path is {txtBackup.Text}");
                                MessageBox.Show("The backup path is either invalid or is not present");
                                return;
                            }

                            string destFileName = Path.Combine(dest, fileName);
                            if (File.Exists(destFileName))
                            {
                                File.Copy(destFileName, Path.Combine(txtBackup.Text, fileName), true);
                            }
                        }

                        File.Copy(file, Path.Combine(dest, fileName), true);

                        // incrementing the progress bar status
                        pbStatus.Value = ((double)i / total) * 100;
                        i++;
                    }
                }
                MessageBox.Show("All files copied successfully!!!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to copy files");
                Log.Error(ex, "Error occurred while copying files");
            }
        }

        private void chkBackup_Checked(object sender, RoutedEventArgs e)
        {
            txtBackup.Visibility = txtBackup.Visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
        }

        private void btnLoadProfile_Click(object sender, RoutedEventArgs e)
        {
            string? val;
            OpenFileDialog? openFileDlg;
            try
            {
                // Create OpenFileDialog
                openFileDlg = new OpenFileDialog
                {

                    // Configure OpenFileDialog
                    Multiselect = false,
                    Filter = "Json files (*.json)|*.json"
                };

                // Launch OpenFileDialog by calling ShowDialog method
                bool? result = openFileDlg.ShowDialog();

                // Get the selected file name and display in a TextBox.
                // Load content of file in a TextBlock
                if (!result.HasValue || !result.Value) return;

                using (JsonTextReader rdr =
                   new JsonTextReader(
                       new StreamReader(new FileStream(openFileDlg.FileName, FileMode.Open))))
                {
                    val = rdr.ReadAsString();
                }

                if (!string.IsNullOrEmpty(val))
                {
                    Profile? prf = JsonConvert.DeserializeObject<Profile>(val);

                    if (prf != null)
                    {
                        if (prf.SourceLocations != null && prf.SourceLocations.Count > 0)
                        {
                            foreach (var item in prf.SourceLocations)
                            {
                                _sourcePaths.Add(new Source()
                                {
                                    Path = item
                                });
                            }
                        }

                        if (prf.DestinationLocations != null && prf.DestinationLocations.Count > 0)
                        {
                            foreach (var item in prf.DestinationLocations)
                            {
                                _destinationPaths.Add(new Destination()
                                {
                                    Path = item
                                });
                            }
                        }

                        _sourcePaths = new ObservableCollection<Source>(_sourcePaths.Distinct(new SourceComparer()));
                        _destinationPaths =
                            new ObservableCollection<Destination>(
                                _destinationPaths.Distinct(new DestinationComparer()));

                        gridSource.ItemsSource = _sourcePaths;
                        gridDestinations.ItemsSource = _destinationPaths;

                        chkBackup.IsChecked = prf.IsBackupEnabled ?? false;
                        txtBackup.Text = prf.BackupLocation ?? "";
                        txtBackup.Visibility = (prf.IsBackupEnabled != null && prf.IsBackupEnabled.Value)
                            ? Visibility.Visible
                            : Visibility.Hidden;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load profile");
                Log.Error(ex, "Error occurred while loading profile");
            }
        }

        private void btnSaveProfile_Click(object sender, RoutedEventArgs e)
        {
            puSave.IsOpen = true;
        }

        private void btnProfileSave_Click(object sender, RoutedEventArgs e)
        {
            List<string>? lstSource = (chkSaveSourceProfile.IsChecked != null && chkSaveSourceProfile.IsChecked.Value) ? ((ObservableCollection<Source>)gridSource.ItemsSource).Where(a => !string.IsNullOrEmpty(a.Path))
                .Select(c => c.Path).ToList() : null;
            List<string>? lstDestinations = (chkSaveDestinationProfile.IsChecked != null && chkSaveDestinationProfile.IsChecked.Value) ? ((ObservableCollection<Destination>)gridDestinations.ItemsSource).Where(a => !string.IsNullOrEmpty(a.Path))
                .Select(c => c.Path).ToList() : null;
            bool? isChecked = (chkSaveBackupProfile.IsChecked != null && chkSaveBackupProfile.IsChecked.Value) ? chkBackup.IsChecked : null;
            string? locBackup = (chkSaveBackupProfile.IsChecked != null && chkSaveBackupProfile.IsChecked.Value) ? txtBackup.Text : null;

            Profile prf = new Profile(isChecked, locBackup, lstSource, lstDestinations);
            string val = JsonConvert.SerializeObject(prf, new JsonSerializerSettings() { Formatting = Formatting.Indented, NullValueHandling = NullValueHandling.Ignore });

            // Create SaveFileDialog
            SaveFileDialog saveFileDlg = new SaveFileDialog
            {
                // Configure SaveFileDialog
                Filter = "Json files (*.json)|*.json"
            };

            puSave.IsOpen = false;
            // Launch SaveFileDialog by calling ShowDialog method
            bool? result = saveFileDlg.ShowDialog();
            // Get the selected file name and display in a TextBox.
            // Load content of file in a TextBlock
            if (!result.HasValue || !result.Value) return;

            using (JsonTextWriter wrtr = new JsonTextWriter(new StreamWriter(new FileStream(saveFileDlg.FileName, FileMode.Create))))
            {
                wrtr.WriteValue(val);
                Console.WriteLine();
            }
        }

        private void btnSaveClose_Click(object sender, RoutedEventArgs e)
        {
            puSave.IsOpen = false;
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            gridSource.ItemsSource = null;
            gridDestinations.ItemsSource = null;
            txtBackup.Text = null;
            chkBackup.IsChecked = false;
            chkSaveBackupProfile.IsChecked = false;
            chkSaveDestinationProfile.IsChecked = false;
            chkSaveSourceProfile.IsChecked = false;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    Log.CloseAndFlush();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~MainWindow()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
