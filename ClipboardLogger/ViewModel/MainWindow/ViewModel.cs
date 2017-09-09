using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using ClipboardManager.Model;
using Microsoft.Win32;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.SaveFile;

namespace ClipboardManager.ViewModel.MainWindow
{
    public class MainWindowViewModel : BaseViewModel
    {
        #region ItemSource binding
        private ObservableCollection<ClipboardElement> _clipboardElements;
        public ObservableCollection<ClipboardElement> ClipboardElements { get => _clipboardElements; set => _clipboardElements = value; }
        #endregion

        #region Public and private properties

        private readonly DispatcherTimer _timer;
        private string LastClipBoardText { get; set; }
        public bool LoggerIsOn { get { if (LoggClipboardIsTrue()){ _timer.Start(); } return LoggClipboardIsTrue(); } set { if (value) { _timer.Start(); SetLoggClipboard(true); } else { _timer.Stop(); SetLoggClipboard(false); } } }
        public bool IsOnStartup { get => ProgramIsInStartup(); set => SetStartup(value); }
        private ClipboardElement _selectedItem { get; set; }
        public ClipboardElement SelectedItem { get => _selectedItem; set { _selectedItem = value; RaisePropertyChanged("SelectedItem"); }  }
        public RelayCommand AboutDialogCommand { get; set; }
        public RelayCommand ClearClipboardCommand { get; set; }
        public RelayCommand ExitEnvironmentCommand { get; set; }
        public RelayCommand ClearClipboardElementsCommand { get; set; }
        public RelayCommand SaveClipboardDataCommand { get; set; }
        public RelayCommand RemoveClipboardElementCommand { get; set; }
        public RelayCommand CopyClipboardElementIdCommand { get; set; }
        public RelayCommand CopyClipboardElementTextContentCommand { get; set; }
        public RelayCommand CopyClipboardElementTimeAndDateCommand { get; set; }
        #endregion

        /// <summary>
        /// The constructor. Initializes all the commands, properties and variables that we will need.
        /// To see these commands, properties and variables, open the "Public and Private properties" region.
        /// </summary>
        public MainWindowViewModel()
        {
            //initialize commands
            AboutDialogCommand = new RelayCommand(AboutDialog);
            ClearClipboardCommand = new RelayCommand(ClearClipboard);
            ExitEnvironmentCommand = new RelayCommand(ExitProgramEnvironment);
            ClearClipboardElementsCommand = new RelayCommand(ClearAllClipboardElements);
            SaveClipboardDataCommand = new RelayCommand(SaveClipboardData);
            RemoveClipboardElementCommand = new RelayCommand(RemoveClipboardElementFromList);
            CopyClipboardElementIdCommand = new RelayCommand(CopyClipboardElementId);
            CopyClipboardElementTextContentCommand = new RelayCommand(CopyClipboardElementContent);
            CopyClipboardElementTimeAndDateCommand = new RelayCommand(CopyClipboardElementTimeAndDate);
            //initialize the ClipBoard elements Collection
            _clipboardElements = new ObservableCollection<ClipboardElement>();
            //initialize timer
            _timer = new DispatcherTimer(DispatcherPriority.Send);
            _timer.Interval = TimeSpan.FromMilliseconds(80);
            _timer.Tick += ClipboardChecker_Tick;
            //initialize LastClipboardText to the clipboard text
            LastClipBoardText = Clipboard.GetText();
        } 

        #region Methods
        public void SaveClipboardData(object obj)
        {
            if (ClipboardElements.Count > 0)
            {
                SaveFileDialogSettings saveFileDialogSettings = new SaveFileDialogSettings();
                saveFileDialogSettings.FileName = "ClipboardData";
                saveFileDialogSettings.Filter = "Log Text File (.log)|*.log|Text File (.txt)|*.txt";
                saveFileDialogSettings.CheckFileExists = false;
                DialogService dialogViewModel = new DialogService();
                bool? result = dialogViewModel.ShowSaveFileDialog(this, saveFileDialogSettings);
                if (result != true) return;
                using (StreamWriter sw = new StreamWriter(saveFileDialogSettings.FileName))
                {
                    foreach (ClipboardElement element in ClipboardElements)
                    {
                        sw.Write("ID: " + element.ID + sw.NewLine + "TextContent: " + element.TextContent + sw.NewLine + "TimAndDate: " + element.TimeAndDate + sw.NewLine + sw.NewLine);
                    }
                    sw.Close();
                }
            }
        }

        private void AboutDialog(object obj)
        {
            DialogService dialogViewModel = new DialogService();
            dialogViewModel.ShowMessageBox(this, "Created by Michelangelo Sarafis", "About", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ClearClipboard(object obj)
        {
            Clipboard.Clear();
            DialogService dialogViewModel = new DialogService();
            dialogViewModel.ShowMessageBox(this, "Clipboard Cleared !!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExitProgramEnvironment(object obj)
        {
            Application.Current.Shutdown();
        }

        private void ClearAllClipboardElements(object obj)
        {
            ClipboardElements.Clear();
            LastClipBoardText = Clipboard.GetText();
        }

        private void RemoveClipboardElementFromList(object obj)
        {
            if (SelectedItem == null) return;
            for(int i = int.Parse(SelectedItem.ID); i < ClipboardElements.Count; i++)
            {
                // we create a new (ModifiedElement)object rather than modify it directly, because when we modify
                // a property of an object... the GUI doesn't update. We need to call the ListView.Items.Refresh method...
                // but since we use MVVM we can't. But when we change an object completely the GUI updates.
                ClipboardElement modifiedElement = ClipboardElements[i];
                modifiedElement.ID = (int.Parse(modifiedElement.ID) - 1).ToString();
                ClipboardElements.Remove(ClipboardElements[i]);
                ClipboardElements.Insert((int.Parse(modifiedElement.ID) - 1), modifiedElement);
            }
            ClipboardElements.Remove(SelectedItem);
        }

        private void CopyClipboardElementId(object obj)
        {
            LoggerIsOn = false;
            if (SelectedItem != null)
            {
                Clipboard.SetText(SelectedItem.ID);
            }
            LastClipBoardText = Clipboard.GetText(); //we put that, so that the timer will not add it to the clipboardList
            LoggerIsOn = true;
        }

        private void CopyClipboardElementContent(object obj)
        {
            LoggerIsOn = false;
            if (SelectedItem != null)
            {
                Clipboard.SetText(SelectedItem.TextContent);
            }
            LastClipBoardText = Clipboard.GetText();
            LoggerIsOn = true;
        }

        private void CopyClipboardElementTimeAndDate(object obj)
        {
            LoggerIsOn = false;
            if(SelectedItem != null)
            {
                Clipboard.SetText(SelectedItem.TimeAndDate);
            }
            LastClipBoardText = Clipboard.GetText();
            LoggerIsOn = true;
        }

        private void ClipboardChecker_Tick(object sender, EventArgs e)
        {
            if (LastClipBoardText != Clipboard.GetText() && ClipboardElements.Count == 0 && Clipboard.GetText() != "")
            {
                ClipboardElements.Add(new ClipboardElement { ID = (ClipboardElements.Count + 1).ToString(), TextContent = Clipboard.GetText(), TimeAndDate = DateTime.Now.ToString() });
                LastClipBoardText = Clipboard.GetText();
            }
            else if (LastClipBoardText != Clipboard.GetText() && Clipboard.GetText() != "")
            {
                ClipboardElements.Add(new ClipboardElement { ID = (ClipboardElements.Count + 1).ToString(), TextContent = Clipboard.GetText(), TimeAndDate = DateTime.Now.ToString() });
                LastClipBoardText = Clipboard.GetText();
            }
        }

        private void SetStartup(bool enable)
        {
            string runKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
            string AppName = "ClipboardLogger";

            RegistryKey startupKey = Registry.CurrentUser.OpenSubKey(runKey);

            if (enable)
            {
                // enable startup
                if (startupKey.GetValue(AppName) != null) return;
                startupKey.Close();
                startupKey = Registry.CurrentUser.OpenSubKey(runKey, true);
                startupKey.SetValue(AppName, Assembly.GetExecutingAssembly().Location);
                startupKey.Close();
            }
            else
            {
                // remove startup
                startupKey = Registry.CurrentUser.OpenSubKey(runKey, true);
                startupKey.DeleteValue(AppName, false);
                startupKey.Close();
            }
        }

        private bool ProgramIsInStartup()
        {
            string runKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
            string AppName = "ClipboardLogger";

            RegistryKey startupKey = Registry.CurrentUser.OpenSubKey(runKey);

            if (startupKey.GetValue(AppName) != null && startupKey.GetValue(AppName).ToString() == Assembly.GetExecutingAssembly().Location)
                return true;
            return false;
        }

        private void SetLoggClipboard(bool enable)
        {
            RegistryKey currentUser = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64);
            var reg = currentUser.OpenSubKey("Software\\ClipboardLogger\\Configuration", true);

            if (enable)
            {
                if (reg == null)
                {
                    reg = currentUser.CreateSubKey("Software\\ClipboardLogger\\Configuration");
                }
                reg.SetValue("LoggTheClipboard", "Yes");
            }
            else
            {
                reg.SetValue("LoggTheClipboard", "No");
            }
        }

        private bool LoggClipboardIsTrue()
        {
            RegistryKey currentUser = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64); //The key entry
            var reg = currentUser.OpenSubKey("Software\\ClipboardLogger\\Configuration", true);

            if (reg.GetValue("LoggTheClipboard") != null && reg.GetValue("LoggTheClipboard").ToString() == "Yes")
                return true;
            return false;
        }
        #endregion
    }
}
