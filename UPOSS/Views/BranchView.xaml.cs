﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UPOSS.Custom.ShortcutKey;
using UPOSS.Models;

namespace UPOSS.Views
{
    /// <summary>
    /// Interaction logic for BranchView.xaml
    /// </summary>
    public partial class BranchView : UserControl
    {
        public BranchView()
        {
            InitializeComponent();

            // set hotkey
            // key: ctrl enter
            // Search
            HotkeysManager.AddHotkey(ModifierKeys.Control, Key.Enter, () => {
                ButtonAutomationPeer peer = new ButtonAutomationPeer(btnSearch);
                IInvokeProvider invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
                invokeProv.Invoke();
            });
        }
    }
}
