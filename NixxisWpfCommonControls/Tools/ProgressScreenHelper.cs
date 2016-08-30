using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows;

namespace Nixxis.Client.Controls
{
    public static class ProgressScreenHelper
    {
        private static ProgressScreen m_ProgressScreen;

        public static void StartTask(DoWorkEventHandler taskToRun, Window owner, string header)
        {
            if (m_ProgressScreen != null)
                throw new NotSupportedException();

            m_ProgressScreen = new ProgressScreen();

            if (owner == null)
            {
                if (Application.Current.MainWindow.IsLoaded)
                    m_ProgressScreen.Owner = Application.Current.MainWindow;
            }
            else
            {
                if (owner.IsLoaded)
                    m_ProgressScreen.Owner = owner;
            }
            m_ProgressScreen.LabelHeader = header;
            m_ProgressScreen.StartTask(taskToRun);
            m_ProgressScreen.ShowDialog();
            m_ProgressScreen = null;
        }
        
        public static void StartTask(DoWorkEventHandler taskToRun)
        {
            ProgressScreenHelper.StartTask(taskToRun, Application.Current.MainWindow, "Progress");
        }

        public static void StartTask(DoWorkEventHandler taskToRun, Window owner)
        {
            ProgressScreenHelper.StartTask(taskToRun, owner, "Progress");
        }

        public static void StartTask(DoWorkEventHandler taskToRun, string header)
        {
            ProgressScreenHelper.StartTask(taskToRun, Application.Current.MainWindow, header);
        }
        
    }
}
