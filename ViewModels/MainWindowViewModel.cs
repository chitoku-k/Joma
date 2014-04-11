using Joma.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace Joma.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private RelayCommand _RunCommand;
        public RelayCommand RunCommand 
        {
            get { return _RunCommand ?? (_RunCommand = new RelayCommand(Run)); }
            set { _RunCommand = value; }
        }

        private RelayCommand _DownloadBarcodesCommand;
        public RelayCommand DownloadBarcodesCommand
        {
            get { return _DownloadBarcodesCommand ?? (_DownloadBarcodesCommand = new RelayCommand(DownloadBarcodes)); }
            set { _DownloadBarcodesCommand = value; }
        }

        public int Count { get; set; }

        private ObservableCollection<string> _Logs;
        public ObservableCollection<string> Logs
        {
            get { return _Logs ?? (_Logs = new ObservableCollection<string>()); }
            set { _Logs = value; }
        }

        private ObservableCollection<string> _BarcodesLogs;
        public ObservableCollection<string> BarcodesLogs
        {
            get { return _BarcodesLogs ?? (_BarcodesLogs = new ObservableCollection<string>()); }
            set { _BarcodesLogs = value; }
        }

        private string _Successes;
        public string Successes
        {
            get { return _Successes ?? (_Successes = ""); }
            set
            {
                _Successes = value; 
                PropertyChanged(this, new PropertyChangedEventArgs("Successes"));
            }
        }

        private async void Run(object parameter)
        {
            var sw = new Stopwatch();
            sw.Start();

            for (int i = 1; true; i++)
            {
                try
                {
                    var result = await JomaChallenge.GetGiftPage();
                    if (result == null)
                    {
                        Logs.Insert(0, string.Format("{0} / {1}: ハズレ", i, Count));
                    }
                    else
                    {
                        Successes = Successes.Insert(0, result + "\n");
                        Logs.Insert(0, string.Format("{0} / {1}: アタリ ({2} 個目)", i, Count, Successes.Where(x => x == '\n').Count()));
                    }

                    if (Successes.Where(x => x == '\n').Count() >= Count)
                    {
                        sw.Stop();
                        Logs.Insert(0, string.Format("--- 終了 --- ({0:N0} s)", sw.Elapsed.Seconds));
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Logs.Insert(0, string.Format("{0} / {1}: 失敗 ({2})", i, Count, ex.GetType().FullName));
                }
            }
        }

        private async void DownloadBarcodes(object parameter)
        {
            var urls = Successes.Split('\n');
            for (int i = 0; i < urls.Length; i++)
            {
                try
                {
                    BarcodesLogs.Insert(0, string.Format("{0} / {1}: ダウンロード中...", i + 1, urls.Length));
                    await JomaDownloader.GetBarcodeImage(urls[i]);
                    BarcodesLogs.RemoveAt(0);
                    BarcodesLogs.Insert(0, string.Format("{0} / {1}: ダウンロード完了", i + 1, urls.Length));
                }
                catch (Exception ex)
                {
                    BarcodesLogs.Insert(0, string.Format("{0} / {1}: 失敗 ({2})", i + 1, urls.Length, ex.GetType().FullName));
                }
            }
            BarcodesLogs.Insert(0, "--- 終了 ---");
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
