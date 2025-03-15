using BackWork;
using Microsoft.Graphics.Display;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace App3
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private TasksRecord _currentTask;
        private bool _isEditingExistingTask = false;
        private List<TasksRecord> _tasks = new List<TasksRecord>();
        private void TextBlock_SelectionChanged(object sender, RoutedEventArgs e)
        {
            LoadTasks();
        }
        private void LoadTasks()
        {
            bool filterEnabled = cbFilterTasks.IsChecked ?? true;
            _tasks = new List<TasksRecord>(BackWork.Workspace.GetTasks(filterEnabled));
            UpdateTaskComboBox();
            this.DispatcherQueue.TryEnqueue(() =>
            {
                UpdateTaskComboBox();
            });
        }
        private void OnFilterChecked(object sender, RoutedEventArgs e)
        {
            LoadTasks();
        }
        private void LoadTaskData(TasksRecord task)
        {
            name.Text = task._name;
            filePathTextBox.Text = task._execPaths?.FirstOrDefault();

            // ����ʱ����ʾ
            if (task._triggerTypes?.Contains("Time") == true)
            {
                setTimeType.SelectedIndex = 0;

                if (task._nextRunTime.HasValue)
                {
                    TimeSpan remaining = task._nextRunTime.Value - DateTime.Now;
                    if (remaining.TotalHours >= 1)
                    {
                        value.Text = ((int)remaining.TotalHours).ToString();
                        timeType.SelectedIndex = 2;
                    }
                    else if (remaining.TotalMinutes >= 1)
                    {
                        value.Text = ((int)remaining.TotalMinutes).ToString();
                        timeType.SelectedIndex = 1;
                    }
                    else
                    {
                        value.Text = ((int)remaining.TotalSeconds).ToString();
                        timeType.SelectedIndex = 0;
                    }
                }
            }
            else if (task._startTime?.Length > 0)
            {
                setTimeType.SelectedIndex = 1;
                DateTime taskTime = task._startTime[0];

                if (taskTime > DateTime.MinValue)
                {
                    datePicker.SelectedDate = new DateTimeOffset(taskTime);
                    timePicker.Time = taskTime.TimeOfDay;
                }
                else
                {
                    datePicker.SelectedDate = null;
                    timePicker.Time = TimeSpan.Zero;
                }
            }
        }
        public MainWindow()
        {
            InitializeComponent();
            LoadTasks();
            task_.Items.Add(new ComboBoxItem { Content = "����¼ƻ�" });
            task_.SelectedIndex = 0;
            task_.SelectionChanged += Task_SelectionChanged;
            App.WindowHandle = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var windowId = Win32Interop.GetWindowIdFromWindow(App.WindowHandle);
            var appWindow = AppWindow.GetFromWindowId(windowId);

            // ���ô��ڳ�ʼ��С
            [DllImport("user32.dll")]
            static extern IntPtr GetDC(IntPtr ptr);
            [DllImport("gdi32.dll")]
            static extern int GetDeviceCaps(
            IntPtr hdc, // handle to DC
            int nIndex // index of capability
            );
            IntPtr hdc = GetDC(IntPtr.Zero);
            int Dpi = GetDeviceCaps(hdc, 88);
            var displayInfo = DisplayInformation.CreateForWindowId(windowId);

            int width = 370*Dpi/96;
            int height = 580*Dpi/96;
            appWindow.Resize(new Windows.Graphics.SizeInt32 { Width = width, Height = height });

            if (appWindow.Presenter is OverlappedPresenter presenter)
            {
                presenter.IsResizable = true;
                presenter.IsMaximizable = true;
            }
            var displayArea = DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Primary);
            if (displayArea != null)
            {
                int centerX = (displayArea.WorkArea.Width - width) / 2;
                int centerY = (displayArea.WorkArea.Height - height) / 2;
                appWindow.Move(new Windows.Graphics.PointInt32(centerX, centerY));
            }
        }


        private void Task_SelectionChanged(object sender, SelectionChangedEventArgs e)//����������ѡ���ʱ��
        {
            if (task_.SelectedIndex == 0)
            {
                _currentTask = default;
                _isEditingExistingTask = false;
                name.IsEnabled = true;
                ClearInputs();
                return;
            }

            if (task_.SelectedItem is ComboBoxItem item && item.Tag is TasksRecord task)
            {
                _currentTask = task;
                _isEditingExistingTask = true;
                name.IsEnabled = false;
                LoadTaskData(task);
            }
        }
        private void ClearInputs()//�������
        {
            name.Text = string.Empty;
            value.Text = string.Empty;
            filePathTextBox.Text = string.Empty;
            datePicker.SelectedDate = null;
            timePicker.Time = TimeSpan.Zero;
        }

        private async Task ShowMessage(string message)//���Ҳ����˵
        {
            var dialog = new ContentDialog
            {
                Title = "��ʾ",
                Content = message,
                CloseButtonText = "ȷ��",
                XamlRoot = this.Content.XamlRoot
            };
            await dialog.ShowAsync();
        }

        private void UpdateTaskComboBox()
            //ˢ��
        {
            task_.Items.Clear();
            task_.Items.Add(new ComboBoxItem { Content = "����¼ƻ�" });

            foreach (var task in _tasks.OrderBy(t => t._nextRunTime))
            {
                var item = new ComboBoxItem
                {
                    Content = $"{task._name} - {task.GetNextRunTime()}",
                    Tag = task,
                    Foreground = task._description == "CreatedByKdenplasmaviaTaskOrganiser"
                        ? new SolidColorBrush(Colors.Black)
                        : new SolidColorBrush(Colors.Gray)
                };

                task_.Items.Add(item);
            }

            task_.SelectedIndex = 0;
        }
        private void setTimeType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (value == null || timeType == null || datePicker == null || timePicker == null)
                return;

            bool isCountdownMode = setTimeType.SelectedIndex == 0;

            value.Visibility = isCountdownMode ? Visibility.Visible : Visibility.Collapsed;
            timeType.Visibility = isCountdownMode ? Visibility.Visible : Visibility.Collapsed;
            datePicker.Visibility = isCountdownMode ? Visibility.Collapsed : Visibility.Visible;
            timePicker.Visibility = isCountdownMode ? Visibility.Collapsed : Visibility.Visible;

            //Ĭ��ʱ��
            /*if (!isCountdownMode)
            {
                datePicker.Date = DateTimeOffset.Now.AddDays(1);
                timePicker.Time = TimeSpan.FromHours(9);
            }*/
        }
        private async void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var openPicker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.Desktop
            };
            openPicker.FileTypeFilter.Add("*");
            openPicker.FileTypeFilter.Add(".exe");
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, App.WindowHandle);

            StorageFile file = await openPicker.PickSingleFileAsync();
            filePathTextBox.Text = file?.Path ?? "δѡ���ļ�";
        }

        private static FileOpenPicker InitializeWithWindow(FileOpenPicker obj, IntPtr windowHandle)
        {
            WinRT.Interop.InitializeWithWindow.Initialize(obj, windowHandle);
            return obj;
        }

        private void addTask(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateInputs()) return;

                var newTask = CreateTaskFromInputs();

                // ɾ��������
                if (_isEditingExistingTask)
                {
                    BackWork.Workspace.DeleteTask(_currentTask._name);
                }

                BackWork.Workspace.SetTasks(newTask);
                LoadTasks();
                ClearInputs();
                task_.SelectedIndex = 0;
                _isEditingExistingTask = false;
            }
            catch (Exception ex)
            {
                _ = ShowMessage($"����ʧ�ܣ�{ex.Message}");
            }
        }
        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(name.Text))
            {
                _= ShowMessage("������ƻ�����");
                return false;
            }

            if (string.IsNullOrWhiteSpace(filePathTextBox.Text))
            {
                _ = ShowMessage("��ѡ���ִ���ļ�");
                return false;
            }

            if (setTimeType.SelectedIndex == 0) // ����ʱģʽ?
            {
                if (!int.TryParse(value.Text, out int delay) || delay <= 0)
                {
                    _ = ShowMessage("��������Ч�ĵ���ʱʱ��");
                    return false;
                }
            }
            else // ָ��ʱ��ģʽ
            {
                if (!datePicker.SelectedDate.HasValue)
                {
                    _ = ShowMessage("��ѡ������");
                    return false;
                }

                DateTime selectedTime = datePicker.SelectedDate.Value.DateTime + timePicker.Time;
                if (selectedTime <= DateTime.Now)
                {
                    _ = ShowMessage("��ѡ��δ����ʱ��");
                    return false;
                }
            }
            return true;
        }
        private async void deleteTask_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //ѡ����Ч����?
                if (task_.SelectedIndex <= 0)
                {
                    await ShowMessage("����ѡ��һ��Ҫɾ��������");
                    return;
                }

                //��ȡ��ǰѡ�е�����
                var selectedItem = task_.SelectedItem as ComboBoxItem;
                if (selectedItem?.Tag is TasksRecord selectedTask)
                {
                    //ȷ��
                    var confirmDialog = new ContentDialog
                    {
                        Title = "ȷ��ɾ��",
                        Content = $"ȷ��Ҫɾ������ '{selectedTask._name}' ��",
                        PrimaryButtonText = "ɾ��",
                        CloseButtonText = "ȡ��",
                        XamlRoot = this.Content.XamlRoot
                    };

                    var result = await confirmDialog.ShowAsync();

                    if (result == ContentDialogResult.Primary)
                    {
                        //ɾ��
                        BackWork.Workspace.DeleteTask(selectedTask._name);
                        LoadTasks();
                        // ����
                        ClearInputs();
                        task_.SelectedIndex = 0;

                        await ShowMessage("����ɾ���ɹ�");
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                await ShowMessage("Ȩ�޲��㣬�޷�ɾ������");
            }
            catch (Exception ex)
            {
                await ShowMessage($"ɾ��ʧ�ܣ�{ex.Message}");
            }
        }
        private TasksRecord CreateTaskFromInputs()
        {
            DateTime? nextRunTime = null;
            DateTime[] startTime = Array.Empty<DateTime>();
            string[] triggerTypes = Array.Empty<string>();

            if (setTimeType.SelectedIndex == 0) // ����ʱ
            {
                int delay = int.Parse(value.Text);
                TimeSpan delaySpan = timeType.SelectedIndex switch
                {
                    0 => TimeSpan.FromSeconds(delay),
                    1 => TimeSpan.FromMinutes(delay),
                    2 => TimeSpan.FromHours(delay),
                    _ => throw new ArgumentException("��Ч��ʱ�䵥λ")
                };

                nextRunTime = DateTime.Now.Add(delaySpan);
                startTime = new[] { nextRunTime.Value };
                triggerTypes = new[] { "Time" };
            }
            else // ָ��ʱ��
            {
                if (datePicker.SelectedDate.HasValue)
                {
                    DateTimeOffset selectedDate = datePicker.SelectedDate.Value;
                    TimeSpan selectedTime = timePicker.Time;
                    DateTime combinedDateTime = selectedDate.DateTime.Date + selectedTime;

                    nextRunTime = combinedDateTime;
                    startTime = new[] { combinedDateTime };
                    triggerTypes = new[] { "DateTime" };
                }
                else
                {
                    throw new ArgumentException("��ѡ����Ч���ں�ʱ��");
                }
            }

            return new TasksRecord(
                name: name.Text,
                state: "Ready",
                nextRunTime: nextRunTime,
                startTime: startTime,
                triggerType: triggerTypes,
                execPath: new[] { filePathTextBox.Text },
                execArguments: Array.Empty<string>(),
                description: "CreatedByKdenplasmaviaTaskOrganiser"
            );
        }
    }
}
