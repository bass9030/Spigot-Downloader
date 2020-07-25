using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Net;
using System.ComponentModel;
using Microsoft.WindowsAPICodePack.Shell.Interop;
using System.IO;

namespace Spigot_Downloader
{
    /// <summary>
    /// Window1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class progressonly : Window
    {
        string language_code;
        public progressonly(string language)
        {
            InitializeComponent();
            language_code = language;
            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            File.Delete("./BuildTools.jar");
            WebClient web = new WebClient();
            web.Headers.Add("User-Agent", "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.3; WOW64; Trident/7.0)");
            web.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
            web.DownloadProgressChanged += new DownloadProgressChangedEventHandler(Progresschange);
            web.DownloadFileAsync(new Uri("https://hub.spigotmc.org/jenkins/job/BuildTools/lastSuccessfulBuild/artifact/target/BuildTools.jar"), "BuildTools.jar");
        }

        private void Progresschange(object sender, DownloadProgressChangedEventArgs e)
        {
            pro.Value = e.ProgressPercentage;
        }

        private void Completed(object sender, AsyncCompletedEventArgs e)
        {
            var file = new FileInfo("BuildTools.jar");
            if (file.Length != 0)
            {
                if (language_code == "ko-KR")
                    MessageBox.Show("다운로드 완료!", "Spigot Downloader", MessageBoxButton.OK, MessageBoxImage.Information);
                else
                    MessageBox.Show("Downlaod Completed!", "Spigot Downloader", MessageBoxButton.OK, MessageBoxImage.Information);

            }
            else
            {
                if(language_code == "ko-KR")
                    MessageBox.Show("다운로드 실패.\n만약 이 메시지가 계속 뜬다면, 인터넷 연결 상태를 확인하세요.", "Spigot Downlaoder", MessageBoxButton.OK, MessageBoxImage.Error);
                else
                    MessageBox.Show("Download failed\nIf the message appears continuously, check your Internet connection.", "Spigot Downlaoder", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            Window.GetWindow(this).Close();
        }
    }
}
