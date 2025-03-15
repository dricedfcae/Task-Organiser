using Microsoft.Win32.TaskScheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using Task = Microsoft.Win32.TaskScheduler.Task;
//���BackWork�����ռ���Ķ�����ԭ�����ڿ���̨���򹹽��Ķ���
//��˹���úܸ������Ƶ�����winui3ǰ���Ժ�����ǰ�˴���������ô�����
//��������Կ���TasksRecord�ṹʮ�ָ��ӣ�����ǰ���ܵ�ľ���ô��
//��Ϊ��������ǲ��ַ����ĺܶ������ʵ�Ѿ���Ĭ���ṩ��
namespace BackWork

{
    public struct TasksRecord
    {
        public string? _description;
        public string? _name, _state;
        public DateTime? _nextRunTime;
        public DateTime[]? _startTime;
        public string[]? _triggerTypes, _execPaths, execArguments;
        public TasksRecord(string? name = "δ֪", string? state = null, DateTime? nextRunTime = null,//�������֣�״̬����һ������ʱ��
            //���Ǵ�����(����ʱ�䣬����)
            DateTime[]? startTime = null, string[]? triggerType = null,
            //����Actions��ִ��·������ѡ������������
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
                        return "δ֪";
                    }
                    else
                    {
                        return this._startTime.Max().ToString();
                    }
                }
                else
                {
                    return "δ֪";
                }
            }
            catch (System.InvalidOperationException ex)
            {
                //Console.WriteLine(ex);
                return "��������û�д�����";
            }
        }
        public readonly string? GetTriggerType()
        {
            if (this._triggerTypes is null)
                return "�޴�����";
            else
            {
                if (this._triggerTypes.Length == 1)
                    return this._triggerTypes[0];
                else if (this._triggerTypes.Length == 0)
                    return "�޴�����";
                else return "���������";
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
                "Disabled" => "����",
                "Queued" => "�����Ŷ�",
                "Ready" => "׼������",
                "Running" => "��������",
                _ => "δ֪"
            };
        }
        public readonly string? GetNextRunTime()
        {
            return this._nextRunTime switch
            {
                null => "δ֪",
                _ when this._nextRunTime == DateTime.MinValue => "δ֪",
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
                    return "��ִ��·��";
                else return "���ִ��·��";
            }
            else
            {
                return "��ִ��·��";
            }
        }
        public readonly string? GetExecArguments()
        {
            if (execArguments is not null)
            {
                if (execArguments.Length == 1)
                    return execArguments[0];
                else if (execArguments.Length == 0)
                    return "��ִ�в���";
                else return "���ִ�в���";
            }
            else
            {
                return "��ִ�в���";
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

                    //������������Ϣ
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
                                TaskTriggerType.Time => "��ʱ����",
                                TaskTriggerType.Daily => "ÿ�մ���",
                                TaskTriggerType.Weekly => "ÿ�ܴ���",
                                TaskTriggerType.Monthly => "ÿ�´���",
                                TaskTriggerType.Idle => "����ʱ����",
                                TaskTriggerType.Logon => "��¼ʱ����",
                                TaskTriggerType.Boot => "����ʱ����",
                                _ => "������������"
                            };
                        }
                        catch
                        {
                            startBoundary[i] = DateTime.MinValue;
                            triggerType[i] = "δ֪������";
                        }
                        i++;
                    }

                    //����������Ϣ
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
                                execActionPath[t] = "��Ч·��";
                                execActionArguments[t] = "��Ч����";
                            }
                            t++;
                        }
                    }
                    //���������¼
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

                    //��ӵ�����б�
                    filteredTasks.Add(taskRecord);
                }
            }
            return [.. filteredTasks];
        }
        public static void SetTasks()/*
                                      * 
                                      * �������ai������һ��ʾ������Ϊʾ�������ڵ���ʱ���ã�ʵ�ʳ��򲻻�ʹ��*/
        {
            // ����TaskServiceʵ��
            TaskService ts = new TaskService();

            // ����������
            TaskDefinition td = ts.NewTask();
            td.RegistrationInfo.Description = "My Sample Task";

            // ��Ӵ����� - ÿ������һ��
            td.Triggers.Add(new DailyTrigger { StartBoundary = DateTime.Today.AddHours(8) });

            // ��Ӳ��� - �������±�
            td.Actions.Add(new ExecAction("notepad.exe"));

            // ע����������ƻ�����
            ts.RootFolder.RegisterTaskDefinition(@"MySampleTask", td, TaskCreation.CreateOrUpdate, null, null, TaskLogonType.InteractiveToken, null);

            Console.WriteLine("����ƻ��ѳɹ�������");
        }
        public static void SetTasks(TasksRecord tr)
        {
            try
            {
                using (TaskService ts = new TaskService())
                {
                    // ����������
                    TaskDefinition td = ts.NewTask();
                    td.RegistrationInfo.Description = "CreatedByKdenplasmaviaTaskOrganiser";

                    // ��Ӵ�����
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
                                    Console.WriteLine($"��֧�ֵĴ���������: {triggerType}");
                                    break;
                            }
                        }
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException("�޴�������������ζ�żƻ���Զ�������");
                    }

                    // ��̬��Ӳ���
                    if (tr._execPaths != null)
                    {
                        for (int i = 0; i < tr._execPaths.Length; i++)
                        {
                            string execPath = tr._execPaths[i];
                            string? execArgument = tr.execArguments != null && tr.execArguments.Length > i ? tr.execArguments[i] : null;

                            td.Actions.Add(new ExecAction(execPath, execArgument));
                        }
                    }

                    // ע������
                    ts.RootFolder.RegisterTaskDefinition(tr.GetName(), td, TaskCreation.CreateOrUpdate, null, null, TaskLogonType.InteractiveToken, null);

                    Console.WriteLine("����ƻ��ѳɹ�������");
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine("Ȩ�޲��㣬�޷���������: " + ex.Message);
            }
            catch (ArgumentNullException ex)
            {
                Console.WriteLine("��������: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("��������ʱ����δ֪����: " + ex.Message);
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
                    throw new ArgumentException($"���� '{taskName}' ������");
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