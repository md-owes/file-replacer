﻿using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using Path = System.IO.Path;
using Serilog;
using System.Collections.Generic;

namespace FileReplacer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDisposable
    {
        ObservableCollection<Source> sourcePaths = new ObservableCollection<Source>();
        private bool disposedValue;

        public MainWindow()
        {
            InitializeComponent();

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File("filereplacer.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDlg;
            try
            {
                // Create OpenFileDialog
                openFileDlg = new OpenFileDialog();

                // Configure OpenFileDialog
                openFileDlg.Multiselect = true;
                openFileDlg.Filter = "All files (*.*)|*.*";

                // Launch OpenFileDialog by calling ShowDialog method
                bool? result = openFileDlg.ShowDialog();

                // Get the selected file name and display in a TextBox.
                // Load content of file in a TextBlock
                if (result.HasValue && result.Value)
                {
                    foreach (string name in openFileDlg.FileNames)
                    {
                        sourcePaths.Add(new Source() { Path = name });
                    }
                    gridSource.ItemsSource = sourcePaths;
                }
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
                            if(!Directory.Exists(txtBackup.Text))
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

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    Log.CloseAndFlush();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
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
