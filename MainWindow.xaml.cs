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

            // 处理时间显示
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
            task_.Items.Add(new ComboBoxItem { Content = "添加新计划" });
            task_.SelectedIndex = 0;
            task_.SelectionChanged += Task_SelectionChanged;
            App.WindowHandle = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var windowId = Win32Interop.GetWindowIdFromWindow(App.WindowHandle);
            var appWindow = AppWindow.GetFromWindowId(windowId);

            // 设置窗口初始大小
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


        private void Task_SelectionChanged(object sender, SelectionChangedEventArgs e)//当更改任务选择框时：
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
        private void ClearInputs()//无需多言
        {
            name.Text = string.Empty;
            value.Text = string.Empty;
            filePathTextBox.Text = string.Empty;
            datePicker.SelectedDate = null;
            timePicker.Time = TimeSpan.Zero;
        }

        private async Task ShowMessage(string message)//这个也不用说
        {
            var dialog = new ContentDialog
            {
                Title = "提示",
                Content = message,
                CloseButtonText = "确定",
                XamlRoot = this.Content.XamlRoot
            };
            await dialog.ShowAsync();
        }

        private void UpdateTaskComboBox()
            //刷新
        {
            task_.Items.Clear();
            task_.Items.Add(new ComboBoxItem { Content = "添加新计划" });

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

            //默认时间
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
            filePathTextBox.Text = file?.Path ?? "未选择文件";
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

                // 删除旧任务）
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
                _ = ShowMessage($"操作失败：{ex.Message}");
            }
        }
        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(name.Text))
            {
                _= ShowMessage("请输入计划名称");
                return false;
            }

            if (string.IsNullOrWhiteSpace(filePathTextBox.Text))
            {
                _ = ShowMessage("请选择可执行文件");
                return false;
            }

            if (setTimeType.SelectedIndex == 0) // 倒计时模式?
            {
                if (!int.TryParse(value.Text, out int delay) || delay <= 0)
                {
                    _ = ShowMessage("请输入有效的倒计时时间");
                    return false;
                }
            }
            else // 指定时间模式
            {
                if (!datePicker.SelectedDate.HasValue)
                {
                    _ = ShowMessage("请选择日期");
                    return false;
                }

                DateTime selectedTime = datePicker.SelectedDate.Value.DateTime + timePicker.Time;
                if (selectedTime <= DateTime.Now)
                {
                    _ = ShowMessage("请选择未来的时间");
                    return false;
                }
            }
            return true;
        }
        private async void deleteTask_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //选中有效任务?
                if (task_.SelectedIndex <= 0)
                {
                    await ShowMessage("请先选择一个要删除的任务");
                    return;
                }

                //获取当前选中的任务
                var selectedItem = task_.SelectedItem as ComboBoxItem;
                if (selectedItem?.Tag is TasksRecord selectedTask)
                {
                    //确认
                    var confirmDialog = new ContentDialog
                    {
                        Title = "确认删除",
                        Content = $"确定要删除任务 '{selectedTask._name}' 吗？",
                        PrimaryButtonText = "删除",
                        CloseButtonText = "取消",
                        XamlRoot = this.Content.XamlRoot
                    };

                    var result = await confirmDialog.ShowAsync();

                    if (result == ContentDialogResult.Primary)
                    {
                        //删除
                        BackWork.Workspace.DeleteTask(selectedTask._name);
                        LoadTasks();
                        // 重置
                        ClearInputs();
                        task_.SelectedIndex = 0;

                        await ShowMessage("任务删除成功");
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                await ShowMessage("权限不足，无法删除任务");
            }
            catch (Exception ex)
            {
                await ShowMessage($"删除失败：{ex.Message}");
            }
        }
        private TasksRecord CreateTaskFromInputs()
        {
            DateTime? nextRunTime = null;
            DateTime[] startTime = Array.Empty<DateTime>();
            string[] triggerTypes = Array.Empty<string>();

            if (setTimeType.SelectedIndex == 0) // 倒计时
            {
                int delay = int.Parse(value.Text);
                TimeSpan delaySpan = timeType.SelectedIndex switch
                {
                    0 => TimeSpan.FromSeconds(delay),
                    1 => TimeSpan.FromMinutes(delay),
                    2 => TimeSpan.FromHours(delay),
                    _ => throw new ArgumentException("无效的时间单位")
                };

                nextRunTime = DateTime.Now.Add(delaySpan);
                startTime = new[] { nextRunTime.Value };
                triggerTypes = new[] { "Time" };
            }
            else // 指定时间
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
                    throw new ArgumentException("请选择有效日期和时间");
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
