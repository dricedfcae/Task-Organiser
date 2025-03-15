using Microsoft.Win32.TaskScheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using Task = Microsoft.Win32.TaskScheduler.Task;
//这个BackWork命名空间里的东西是原本我在控制台程序构建的东西
//后端构造得很复杂完善但加上winui3前端以后发现我前端处理不过来这么多参数
//于是你可以看到TasksRecord结构十分复杂，但是前端能点的就这么多
//因为添加任务那部分方法的很多参数其实已经被默认提供了
namespace BackWork

{
    public struct TasksRecord
    {
        public string? _description;
        public string? _name, _state;
        public DateTime? _nextRunTime;
        public DateTime[]? _startTime;
        public string[]? _triggerTypes, _execPaths, execArguments;
        public TasksRecord(string? name = "未知", string? state = null, DateTime? nextRunTime = null,//这是名字，状态，下一次运行时间
            //这是触发器(启动时间，类型)
            DateTime[]? startTime = null, string[]? triggerType = null,
            //这是Actions（执行路径，可选参数，描述）
            string[]? execPath = null, string[]? execArguments = null, string? description = null)
        {
            this._name = name;
            this._startTime = startTime;
            this._triggerTypes = triggerType;
            this._state = state;
            this._nextRunTime = nextRunTime;
            this._execPaths = execPath;
            this.execArguments = execArguments;
            this._description = description;
        }
        public readonly string? GetDescription() => _description;
        public readonly string? GetStartTime()
        {
            try
            {
                if (this._startTime is not null)
                {
                    if (this._startTime.Min() == DateTime.MinValue || this._startTime == null)
                    {
                        return "未知";
                    }
                    else
                    {
                        return this._startTime.Max().ToString();
                    }
                }
                else
                {
                    return "未知";
                }
            }
            catch (System.InvalidOperationException ex)
            {
                //Console.WriteLine(ex);
                return "这项任务还没有触发器";
            }
        }
        public readonly string? GetTriggerType()
        {
            if (this._triggerTypes is null)
                return "无触发器";
            else
            {
                if (this._triggerTypes.Length == 1)
                    return this._triggerTypes[0];
                else if (this._triggerTypes.Length == 0)
                    return "无触发器";
                else return "多个触发器";
            }
        }
        public readonly string? GetName()
        {
            return this._name;
        }
        public readonly string? GetState()
        {
            return this._state switch
            {
                "Disabled" => "禁用",
                "Queued" => "正在排队",
                "Ready" => "准备就绪",
                "Running" => "正在运行",
                _ => "未知"
            };
        }
        public readonly string? GetNextRunTime()
        {
            return this._nextRunTime switch
            {
                null => "未知",
                _ when this._nextRunTime == DateTime.MinValue => "未知",
                _ => this._nextRunTime.ToString()
            };
        }
        public readonly string? GetExecPath()
        {
            if (_execPaths is not null)
            {
                if (_execPaths.Length == 1)
                    return _execPaths[0];
                else if (_execPaths.Length == 0)
                    return "无执行路径";
                else return "多个执行路径";
            }
            else
            {
                return "无执行路径";
            }
        }
        public readonly string? GetExecArguments()
        {
            if (execArguments is not null)
            {
                if (execArguments.Length == 1)
                    return execArguments[0];
                else if (execArguments.Length == 0)
                    return "无执行参数";
                else return "多个执行参数";
            }
            else
            {
                return "无执行参数";
            }
        }

    }
    public class Workspace
    {
        private static int tasksCount;
        public static TasksRecord[] GetTasks(bool onlyMyAppTasks = true)
        {
            List<TasksRecord> filteredTasks = new List<TasksRecord>();
            using (TaskService ts = new())
            {
                foreach (Task task in ts.RootFolder.GetTasks())
                {
                    bool isMyAppTask = false;
                    try
                    {
                        isMyAppTask = task.Definition.RegistrationInfo.Description?
                            .Contains("CreatedByKdenplasmaviaTaskOrganiser") ?? false;
                    }
                    catch
                    {
                        isMyAppTask = false;
                    }

                    if (onlyMyAppTasks && !isMyAppTask)
                        continue;

                    //解析触发器信息
                    int triggersNumber = task.Definition.Triggers.Count;
                    DateTime[] startBoundary = new DateTime[triggersNumber];
                    string[] triggerType = new string[triggersNumber];

                    int i = 0;
                    foreach (Trigger trigger in task.Definition.Triggers)
                    {
                        try
                        {
                            startBoundary[i] = trigger.StartBoundary;
                            triggerType[i] = trigger.TriggerType switch
                            {
                                TaskTriggerType.Time => "定时触发",
                                TaskTriggerType.Daily => "每日触发",
                                TaskTriggerType.Weekly => "每周触发",
                                TaskTriggerType.Monthly => "每月触发",
                                TaskTriggerType.Idle => "空闲时触发",
                                TaskTriggerType.Logon => "登录时触发",
                                TaskTriggerType.Boot => "启动时触发",
                                _ => "其他触发类型"
                            };
                        }
                        catch
                        {
                            startBoundary[i] = DateTime.MinValue;
                            triggerType[i] = "未知触发器";
                        }
                        i++;
                    }

                    //解析操作信息
                    int actionsNumber = task.Definition.Actions.Count;
                    string[] execActionPath = new string[actionsNumber];
                    string[] execActionArguments = new string[actionsNumber];
                    int t = 0;
                    foreach (var action in task.Definition.Actions)
                    {
                        if (action is ExecAction execAction)
                        {
                            try
                            {
                                execActionPath[t] = execAction.Path ?? string.Empty;
                                execActionArguments[t] = execAction.Arguments ?? string.Empty;
                            }
                            catch
                            {
                                execActionPath[t] = "无效路径";
                                execActionArguments[t] = "无效参数";
                            }
                            t++;
                        }
                    }
                    //构建任务记录
                    var taskRecord = new TasksRecord(
                        name: task.Name,
                        state: task.State.ToString(),
                        nextRunTime: task.NextRunTime,
                        startTime: startBoundary,
                        triggerType: triggerType,
                        execPath: execActionPath,
                        execArguments: execActionArguments,
                        description: task.Definition.RegistrationInfo.Description
                    );

                    //添加到结果列表
                    filteredTasks.Add(taskRecord);
                }
            }
            return [.. filteredTasks];
        }
        public static void SetTasks()/*
                                      * 
                                      * 这个是由ai创建的一个示范，作为示例方法在调试时调用，实际程序不会使用*/
        {
            // 创建TaskService实例
            TaskService ts = new TaskService();

            // 创建任务定义
            TaskDefinition td = ts.NewTask();
            td.RegistrationInfo.Description = "My Sample Task";

            // 添加触发器 - 每天运行一次
            td.Triggers.Add(new DailyTrigger { StartBoundary = DateTime.Today.AddHours(8) });

            // 添加操作 - 启动记事本
            td.Actions.Add(new ExecAction("notepad.exe"));

            // 注册任务到任务计划程序
            ts.RootFolder.RegisterTaskDefinition(@"MySampleTask", td, TaskCreation.CreateOrUpdate, null, null, TaskLogonType.InteractiveToken, null);

            Console.WriteLine("任务计划已成功创建。");
        }
        public static void SetTasks(TasksRecord tr)
        {
            try
            {
                using (TaskService ts = new TaskService())
                {
                    // 创建任务定义
                    TaskDefinition td = ts.NewTask();
                    td.RegistrationInfo.Description = "CreatedByKdenplasmaviaTaskOrganiser";

                    // 添加触发器
                    if (tr._triggerTypes != null && tr._startTime != null)
                    {
                        for (int i = 0; i < tr._triggerTypes.Length; i++)
                        {
                            string triggerType = tr._triggerTypes[i];
                            DateTime startTime = tr._startTime[i];

                            switch (triggerType)
                            {
                                case "Daily":
                                    td.Triggers.Add(new DailyTrigger { StartBoundary = startTime });
                                    break;
                                case "Weekly":
                                    td.Triggers.Add(new WeeklyTrigger { StartBoundary = startTime });
                                    break;
                                case "Monthly":
                                    td.Triggers.Add(new MonthlyTrigger { StartBoundary = startTime });
                                    break;
                                case "Time":
                                    td.Triggers.Add(new Microsoft.Win32.TaskScheduler.TimeTrigger { StartBoundary = startTime });
                                    break;
                                case "Idle":
                                    td.Triggers.Add(new IdleTrigger { StartBoundary = startTime });
                                    break;
                                case "Logon":
                                    td.Triggers.Add(new LogonTrigger { StartBoundary = startTime });
                                    break;
                                case "Boot":
                                    td.Triggers.Add(new BootTrigger { StartBoundary = startTime });
                                    break;
                                default:
                                    Console.WriteLine($"不支持的触发器类型: {triggerType}");
                                    break;
                            }
                        }
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException("无触发器——这意味着计划永远不会进行");
                    }

                    // 动态添加操作
                    if (tr._execPaths != null)
                    {
                        for (int i = 0; i < tr._execPaths.Length; i++)
                        {
                            string execPath = tr._execPaths[i];
                            string? execArgument = tr.execArguments != null && tr.execArguments.Length > i ? tr.execArguments[i] : null;

                            td.Actions.Add(new ExecAction(execPath, execArgument));
                        }
                    }

                    // 注册任务
                    ts.RootFolder.RegisterTaskDefinition(tr.GetName(), td, TaskCreation.CreateOrUpdate, null, null, TaskLogonType.InteractiveToken, null);

                    Console.WriteLine("任务计划已成功创建。");
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine("权限不足，无法创建任务: " + ex.Message);
            }
            catch (ArgumentNullException ex)
            {
                Console.WriteLine("参数错误: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("创建任务时发生未知错误: " + ex.Message);
            }
        }
        public static void DeleteTask(string taskName)
        {
            using (TaskService ts = new TaskService())
            {
                if (ts.GetTask(taskName) != null)
                {
                    ts.RootFolder.DeleteTask(taskName);
                }
                else
                {
                    throw new ArgumentException($"任务 '{taskName}' 不存在");
                }
            }
        }

        public static void UpdateTask(TasksRecord oldTask, TasksRecord newTask)
        {
            DeleteTask(oldTask._name);
            SetTasks(newTask);
        }
        /*   public static void Main()
            {
                TasksRecord[] tasksRecord = GetTasks();
                for (int i = 0; i < tasksCount; i++)
                {
                    Console.WriteLine(tasksRecord[i].GetStartTime());
                    Console.WriteLine(tasksRecord[i].GetName());
                    Console.WriteLine(tasksRecord[i].GetNextRunTime());
                    Console.WriteLine(tasksRecord[i].GetState());
                    Console.WriteLine(tasksRecord[i].GetTriggerType());
                    Console.Write($"{tasksRecord[i].GetExecPath()}  ");
                    Console.WriteLine(tasksRecord[i].GetExecArguments());
                    Console.WriteLine("------------------------------------");
                }
                Console.ReadKey();
            }*/
    }
}