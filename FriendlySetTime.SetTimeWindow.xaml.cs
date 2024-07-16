﻿using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace FriendlySetTime
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class SetTimeWindow : UserControl
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        public string Hours { get; set; }
        public string Minutes { get; set; }
        public string Seconds { get; set; }

        public List<string> statuses { get; set; } = new List<string>();
        private readonly FriendlySetPlugin plugin;
        private readonly Game game;

        public SetTimeWindow(FriendlySetPlugin plugin, Game game)
        {
            this.game = game;
            ulong curseconds = game.Playtime;
            Seconds = (curseconds % 60).ToString();
            ulong bigMinutes = curseconds / 60;
            Minutes = (bigMinutes % 60).ToString();
            Hours = (bigMinutes / 60).ToString();
            this.plugin = plugin;

            var completionStatusNone = "";
            statuses.Add(completionStatusNone);
            foreach (CompletionStatus completionStatus in plugin.PlayniteApi.Database.CompletionStatuses)
            {
                statuses.Add(completionStatus.Name);
            }
            InitializeComponent();
            newDate.SelectedDate = game.LastActivity;

            // Use completion status none if it's not set.
            var currentCompletionStatus = game.CompletionStatus?.Name;
            if (currentCompletionStatus != null)
            {
                newStatus.SelectedIndex = statuses.IndexOf(currentCompletionStatus);
            }
            else
            {
                newStatus.SelectedIndex = statuses.IndexOf(completionStatusNone);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                uint mins = UInt32.Parse(minutes.Text.Trim());
                uint hrs = UInt32.Parse(hours.Text.Trim());
                ulong scnds = UInt64.Parse(seconds.Text.Trim());
                scnds += mins * 60;
                scnds += hrs * 3600;
                game.Playtime = scnds;
                if ((bool)updateStatus.IsChecked)
                {
                    string status = newStatus.SelectedItem.ToString();
                    if (status != "")
                    {
                        game.CompletionStatusId = plugin.PlayniteApi.Database.CompletionStatuses.Where(x => x.Name == status).DefaultIfEmpty(game.CompletionStatus).First().Id;
                    }
                    else
                    {
                        game.CompletionStatusId = Guid.Empty;
                    }
                }
                if ((bool)setDate.IsChecked)
                {
                    game.LastActivity = newDate.SelectedDate;
                }
                plugin.PlayniteApi.Database.Games.Update(game);
                ((Window)this.Parent).Close();
            }
            catch (Exception E)
            {
                logger.Error(E, "Error when parsing time");
                plugin.PlayniteApi.Dialogs.ShowErrorMessage(E.Message, "Error when parsing time");
            }
        }

        private void StatusChanged(object sender, SelectionChangedEventArgs e)
        {
            // Detect if completion status wasn't set.
            var currentCompletionStatus = game.CompletionStatus?.Name;
            var completionStatusNone = "";
            if (currentCompletionStatus != null)
            {
                if (currentCompletionStatus != newStatus.SelectedItem.ToString())
                {
                    updateStatus.IsChecked = true;
                }
            }
            else
            {
                if (completionStatusNone != newStatus.SelectedItem.ToString())
                {
                    updateStatus.IsChecked = true;
                }
            }
        }

        private void DidDateChange()
        {
            if (!((game.LastActivity.HasValue && newDate.SelectedDate.HasValue) &&
                newDate.SelectedDate.Value.Date.Equals(game.LastActivity.Value.Date)))
            {
                setDate.IsChecked = true;
            }
        }

        private void SetToday_Checked(object sender, RoutedEventArgs e)
        {
            newDate.SelectedDate = DateTime.Today;
            DidDateChange();
        }

        private void NewDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            DidDateChange();
            if (!(newDate.SelectedDate.HasValue && newDate.SelectedDate.Value.Date.Equals(DateTime.Today.Date)))
            {
                setToday.IsChecked = false;
            }
        }

        private void hours_LostFocus(object sender, RoutedEventArgs e)
        {
            float hrs = float.Parse(hours.Text.Trim());

            if (hrs % 1 != 0)
            {
                int mins = (int)(hrs % 1 * 60);
                hours.Text = hrs.ToString("F0");
                minutes.Text = mins.ToString();
            }
        }

        private void minutes_LostFocus(object sender, RoutedEventArgs e)
        {
            float mins = float.Parse(minutes.Text.Trim());
            if (mins % 1 != 0)
            {
                int scnds = (int)(mins % 1 * 60);
                minutes.Text = mins.ToString("F0");
                seconds.Text = scnds.ToString();
            }
        }

        private void seconds_LostFocus(object sender, RoutedEventArgs e)
        {
            float scnds = float.Parse(seconds.Text.Trim());
            if (scnds % 1 != 0)
            {
                seconds.Text = scnds.ToString("F0");
            }
        } 
    }
}
