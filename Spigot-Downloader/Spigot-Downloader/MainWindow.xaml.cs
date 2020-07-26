using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.WindowsAPICodePack.Shell.Interop;
using NSoup;
using NSoup.Nodes;
using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Spigot_Downloader;

namespace Spigot_Downloader
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        string[] delfiles = new string[8] { "apache-maven-3.6.0", "BuildData", "Bukkit", "CraftBukkit", "PortableGit-2.24.1.2-32-bit", "PortableGit-2.24.1.2-64-bit", "Spigot", "work" };
        BackgroundWorker dummyfiledel;
        BackgroundWorker startdownload;
        string language_code = "en-US";
        string title = "Spigot Downloader";
        string newver;
        bool workresult;
        RegistryKey reg;

        public static String GetWebText(string Url)
        {

            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(Url);
            myRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.116 Safari/537.36";
            myRequest.Method = "GET";
            WebResponse myResponse = myRequest.GetResponse();
            StreamReader sr = new StreamReader(myResponse.GetResponseStream(), System.Text.Encoding.UTF8);
            string result = sr.ReadToEnd();
            sr.Close();
            myResponse.Close();

            return result;
        }

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += OnLoaded;
            try
            {
                reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\JavaSoft\Java Update\Policy");
            }
            catch
            {
                MessageBox.Show("자바를 설치후 실행해 주세요.\n자바 미설치시 프로그램이 작동하지 않습니다.\n\nPlease install Java and run it.\nIf Java is not installed, the program does not work.", title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                this.Close();
            }
            dummyfiledel = new BackgroundWorker()//더미파일 삭제
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            dummyfiledel.DoWork += dummyfiledel_Dowork;
            dummyfiledel.ProgressChanged += dummyfiledel_ProgressChanged;
            dummyfiledel.RunWorkerCompleted += dummyfiledel_RunWorkerCompleted;

            startdownload = new BackgroundWorker()//스피갓 다운 시작
            {
                WorkerSupportsCancellation = true
            };
            startdownload.DoWork += startdownload_Dowork;
            startdownload.RunWorkerCompleted += startdownload_RunWorkerCompleted;
            try
            {
                Document doc = NSoupClient.Parse(GetWebText("https://hub.spigotmc.org/jenkins/job/BuildTools/"));
                var versionlist = doc.Select("a.tip.model-link.inside.build-link.display-name");
                newver = NSoupClient.Parse(versionlist.ToArray()[0].ToString()).Text();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to retrieve BuildTools version list.\nIf the phenomenon persists, please press the \"Bug Report\"\nbutton in the program to report the bug.\n\nLearn more:\n" + ex, title, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            reg = Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Spigot Downloader");
            string savever = reg.GetValue("BuildTools version", "").ToString();
            if (!new FileInfo("BuildTools.jar").Exists || savever != newver)
            {
                progressonly window1 = new progressonly(language_code);
                window1.Show();
                reg.SetValue("BuildTools version", newver);
            }
            else
            {

            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            CultureInfo ci = CultureInfo.InstalledUICulture;
            if (ci.Name == "ko-KR")
            {
                language_code = ci.Name;
                language.SelectedIndex = 0;
            }
            else
            {
                language_code = "en-US";
                language.SelectedIndex = 1;
            }
            path.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        }

        public string run_command(string runfile, string command)
        {
            System.Diagnostics.ProcessStartInfo pri = new System.Diagnostics.ProcessStartInfo();
            System.Diagnostics.Process pro = new System.Diagnostics.Process();

            //실행할 파일 명 입력하기
            pri.FileName = runfile;

            //cmd 창 띄우기
            pri.CreateNoWindow = true; //flase가 띄우기, true가 안 띄우기
            pri.UseShellExecute = false;

            pri.RedirectStandardInput = true;
            pri.RedirectStandardOutput = true;
            pri.RedirectStandardError = true;

            pro.StartInfo = pri;
            pro.Start();

            //명령어 실행
            pro.StandardInput.Write(command + Environment.NewLine);
            pro.StandardInput.Close();
            string resultValue = pro.StandardOutput.ReadToEnd();
            pro.WaitForExit();
            pro.Close();

            return resultValue;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (language.SelectedIndex == 0)
            {
                Change_Language("ko-KR");
                language_code = "ko-KR";
            }
            if (language.SelectedIndex == 1)
            {
                Change_Language("en-US");
                language_code = "en-US";
            }
        }

        private void Change_Language(string Language)
        {
            if (Language == "ko-KR")
            {
                ver_box.Header = "버전 선택";
                ver_sel_info.Content = "만약 버진이 1.8 같이 버전의 맨 뒷자리가 비어 있는 경우 공백으로 두기";
                del_dummyfile_text.Text = "빌드툴 더미 파일 삭제\n(모든 설정이 완벽하지만 오류가 있는 경우,\n이 버튼을 누르십시오.)";
                is_latest_ver.Content = "최신버전";
                sel_save_path_box.Header = "저장 위치 선택";
                downlaod.Content = "다운 시작";
                buildtoolsdownload.Content = "BuildTools.jar 다시 다운로드";
                reportbug.Content = "버그제보";
                return;
            }
            else if (Language == "en-US")
            {
                ver_box.Header = "Version select";
                ver_sel_info.Content = "Leave blank if the last digit of the version is empty, such as Virgin 1.8";
                del_dummyfile_text.Text = "Delete BuildTools dummy file\n(If all settings are perfect but there is an error,\nplease press the button.)";
                is_latest_ver.Content = "Latest Version";
                sel_save_path_box.Header = "Select a Path to save file";
                downlaod.Content = "Start Download";
                buildtoolsdownload.Content = "Re-downlaod BuildTools.jar";
                reportbug.Content = "Report Bug";
                return;
            }
            else
            {
                throw new Exception();
            }
        }

        private void sel_path_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            dialog.InitialDirectory = @"C:\";
            var result = dialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                path.Text = dialog.FileName;
            }
        }

        private void del_dummyfile_button_Click(object sender, RoutedEventArgs e)
        {
            process.Maximum = 8;
            dummyfiledel.RunWorkerAsync();
        }

        private void dummyfiledel_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (language_code == "ko-KR")
                MessageBox.Show("더미파일 삭제가 완료되었습니다.\n만약 더미파일 삭제 후에도 오류가 계속된다면 프로그램을 재설치 해주세요.", title, MessageBoxButton.OK, MessageBoxImage.Information);
            else
                MessageBox.Show("Dummy file deletion is complete.\nIf the error continues even after deleting the dummy file, please reinstall the program.", title, MessageBoxButton.OK, MessageBoxImage.Information);
            comment.Content = "";
        }

        private void dummyfiledel_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            process.Value = e.ProgressPercentage;
        }

        private void dummyfiledel_Dowork(object sender, DoWorkEventArgs e)
        {
            int progress = 0;
            foreach (string i in delfiles)
            {
                progress++;
                dummyfiledel.ReportProgress(progress);
                try
                {
                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        if (language_code == "ko-KR")
                        {
                            comment.Content = "삭제중... " + i;
                        }
                        else
                        {
                            comment.Content = "Deleteing... " + i;
                        }
                    }));

                    DirectoryInfo dir = new DirectoryInfo(@"./" + i);

                    System.IO.FileInfo[] files = dir.GetFiles("*.*",
                    SearchOption.AllDirectories);

                    foreach (System.IO.FileInfo file in files)
                        file.Attributes = FileAttributes.Normal;
                    Directory.Delete(@"./" + i, true);
                }
                catch
                {
                    progress++;
                    continue;
                }
            }
        }

        private void startdownload_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            comment.Content = "";
            process.IsIndeterminate = false;
            if (workresult)
            {
                if (language_code == "ko-KR")
                {
                    if (MessageBox.Show("다운로드가 완료되었습니다.\n저장폴더를 여시겠습니까?", title, MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                        run_command(@"cmd.exe", "explorer.exe " + path.Text);
                }
                else
                {
                    if (MessageBox.Show("Download completed successfully.\nDo you want to open the save folder ? ", title, MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                        run_command(@"cmd.exe", "explorer.exe " + path.Text);
                }
            }
        }


        private void startdownload_Dowork(object sender, DoWorkEventArgs e)
        {
            string command = @"java -Xmx1G -jar BuildTools.jar ";
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                if (is_latest_ver.IsChecked.Value)
                {
                    command += "--rev latest";
                }
                else
                {
                    if (ver3.Text != "")
                    {
                        command += "--rev " + ver1.Text + '.' + ver2.Text + '.' + ver3.Text;
                    }
                    else
                    {
                        command += "--rev " + ver1.Text + '.' + ver2.Text;
                    }
                }
                command += " -o " + path.Text;
                if (language_code == "ko-KR")
                    comment.Content = "다운로드중...";
                else
                    comment.Content = "Downloading...";
                process.IsIndeterminate = true;
            }));
            try
            {
                string result = run_command(@"cmd.exe", command);
                if(result.IndexOf("Saved as") == -1)
                {
                    throw new Exception();
                }
                workresult = true;
            }
            catch (Exception ex)
            {
                workresult = false;
                if(language_code == "ko-KR")
                {
                    MessageBox.Show("스피갓 다운로드에 실패하였습니다.\n" + 
                                    "아래 사항을 따라도 오류가 지속된다면 버그 제보를 눌러서 이 메시지를 캡처해 올려주세요.\n" + 
                                    "1. \"BuildTools.jar 다시 다운로드\"버튼을 눌러 BuildTools.jar를 다시 다운로드 해주세요.\n" +
                                    "2. \"명령 프롬프트\"를 열어서 \"java -version\"를 입력해 \"java version \"~~~\"...(중략)\"이 나오는지 확인하세요.\n" +
                                    "만약 안나온다면 인터넷에 \"java은(는) 내부 또는...(중략)\"를 검색하여 해결방법을 따라합니다.\n" +
                                    "3. 프로그램을 지웠다가 재설치합니다.\n" +
                                    "만약 위의 사항을 전부 수행 했음에도 지속해서 이 오류가 뜬다면 버그제보를 눌러 이 메시지를 캡처해 올려주세요.\n\n" +
                                    ex.ToString(), title, MessageBoxButton.OK, MessageBoxImage.Error);

                }
                else
                {
                    MessageBox.Show("Failed to download spigot.\n" +
                                    "If the error persists after following the instructions below, please click the bug report to capture and upload this message\n" +
                                    "1.Press the \"Re-download BuildTools.jar\" button to download BuildTools.jar again.\n" +
                                    "2.Open \"command prompt\" and enter \"java -version\" to see if \"java version \"~~~\"...(medium)\"" +
                                    "If not, search for the message on the Internet and follow the solution." +
                                    "3.Remove and reinstall the program.\n" +
                                    "If you continue to get this error after performing all of the above, please click the bug report and upload this message.\n\n" +
                                    ex.ToString(), title, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void is_latest_ver_Checked(object sender, RoutedEventArgs e)
        {
            ver1.IsEnabled = false;
            ver2.IsEnabled = false;
            ver3.IsEnabled = false;
            dot1.IsEnabled = false;
            dot2.IsEnabled = false;
        }

        private void is_latest_ver_Unchecked(object sender, RoutedEventArgs e)
        {
            ver1.IsEnabled = true;
            ver2.IsEnabled = true;
            ver3.IsEnabled = true;
            dot1.IsEnabled = true;
            dot2.IsEnabled = true;
        }

        private void downlaod_Click(object sender, RoutedEventArgs e)
        {
            process.Maximum = 100;
            startdownload.RunWorkerAsync();
        }

        private void reportbug_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/bass9030/Spigot-Downloader/issues/new");
        }

        private void buildtoolsdownload_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                Document doc = NSoupClient.Parse(GetWebText("https://hub.spigotmc.org/jenkins/job/BuildTools/"));
                var versionlist = doc.Select("a.tip.model-link.inside.build-link.display-name");
                newver = NSoupClient.Parse(versionlist.ToArray()[0].ToString()).Text();
            }
            catch
            {
            }
            RegistryKey reg;
            reg = Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Spigot Downloader");

            progressonly window1 = new progressonly(language_code);
            window1.Show();
        }
    }

}
