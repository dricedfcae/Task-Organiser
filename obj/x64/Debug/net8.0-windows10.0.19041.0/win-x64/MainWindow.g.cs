﻿#pragma checksum "C:\Users\drice\source\repos\App4\App4\MainWindow.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "38D530707C7A1266B8198BDF54A1CEA8D6E93B5147B78441D6A834B68AC30D7A"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace App3
{
    partial class MainWindow : 
        global::Microsoft.UI.Xaml.Window, 
        global::Microsoft.UI.Xaml.Markup.IComponentConnector
    {

        /// <summary>
        /// Connect()
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.UI.Xaml.Markup.Compiler"," 3.0.0.2502")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 2: // MainWindow.xaml line 12
                {
                    global::Microsoft.UI.Xaml.Controls.TextBlock element2 = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.TextBlock>(target);
                    ((global::Microsoft.UI.Xaml.Controls.TextBlock)element2).SelectionChanged += this.TextBlock_SelectionChanged;
                }
                break;
            case 3: // MainWindow.xaml line 13
                {
                    this.name = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.TextBox>(target);
                }
                break;
            case 4: // MainWindow.xaml line 15
                {
                    this.setTimeType = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.ComboBox>(target);
                    ((global::Microsoft.UI.Xaml.Controls.ComboBox)this.setTimeType).SelectionChanged += this.setTimeType_SelectionChanged;
                }
                break;
            case 5: // MainWindow.xaml line 19
                {
                    this.value = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.TextBox>(target);
                }
                break;
            case 6: // MainWindow.xaml line 25
                {
                    this.timeType = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.ComboBox>(target);
                }
                break;
            case 7: // MainWindow.xaml line 30
                {
                    this.datePicker = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.DatePicker>(target);
                }
                break;
            case 8: // MainWindow.xaml line 37
                {
                    this.timePicker = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.TimePicker>(target);
                }
                break;
            case 9: // MainWindow.xaml line 52
                {
                    this.addNewTask = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.Button>(target);
                    ((global::Microsoft.UI.Xaml.Controls.Button)this.addNewTask).Click += this.addTask;
                }
                break;
            case 10: // MainWindow.xaml line 53
                {
                    this.deleteTask = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.Button>(target);
                    ((global::Microsoft.UI.Xaml.Controls.Button)this.deleteTask).Click += this.deleteTask_Click;
                }
                break;
            case 11: // MainWindow.xaml line 55
                {
                    this.task_ = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.ComboBox>(target);
                }
                break;
            case 12: // MainWindow.xaml line 60
                {
                    this.cbFilterTasks = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.CheckBox>(target);
                    ((global::Microsoft.UI.Xaml.Controls.CheckBox)this.cbFilterTasks).Checked += this.OnFilterChecked;
                    ((global::Microsoft.UI.Xaml.Controls.CheckBox)this.cbFilterTasks).Unchecked += this.OnFilterChecked;
                }
                break;
            case 13: // MainWindow.xaml line 44
                {
                    this.filePathTextBox = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.TextBox>(target);
                }
                break;
            case 14: // MainWindow.xaml line 48
                {
                    global::Microsoft.UI.Xaml.Controls.Button element14 = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.Button>(target);
                    ((global::Microsoft.UI.Xaml.Controls.Button)element14).Click += this.BrowseButton_Click;
                }
                break;
            default:
                break;
            }
            this._contentLoaded = true;
        }


        /// <summary>
        /// GetBindingConnector(int connectionId, object target)
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.UI.Xaml.Markup.Compiler"," 3.0.0.2502")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::Microsoft.UI.Xaml.Markup.IComponentConnector GetBindingConnector(int connectionId, object target)
        {
            global::Microsoft.UI.Xaml.Markup.IComponentConnector returnValue = null;
            return returnValue;
        }
    }
}

