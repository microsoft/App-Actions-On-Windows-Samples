// Copyright (c) Microsoft Corporation. All rights reserved.

using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.Windows.ApplicationModel.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.AI.Actions;
using Windows.AI.Actions.Hosting;

namespace ActionConsumerSampleApp
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            string currentTime = DateTime.Now.ToString("hh:mm:ss tt", System.Globalization.DateTimeFormatInfo.InvariantInfo);

            if (String.IsNullOrEmpty(this.SubmittedText.Text))
            {
                this.SubmittedText.Text += (currentTime + " " + this.InputText.Text);
            }
            else
            {
                this.SubmittedText.Text += ("\n\n" + currentTime  + " " + this.InputText.Text);
            }
        }

        private void RefreshFeedButton_Click(object sender, RoutedEventArgs e)
        {
            this.SubmittedText.Text = string.Empty;
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            this.InputText.Text = string.Empty;
        }

        private void GetActionsForEntitiesButton_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyout menuFlyout = new MenuFlyout();
            menuFlyout.Placement = Microsoft.UI.Xaml.Controls.Primitives.FlyoutPlacementMode.BottomEdgeAlignedLeft;

            ResourceLoader resourceLoader = new();

            MenuFlyoutItem noActionsFoundItem = new MenuFlyoutItem
            {
                Text = resourceLoader.GetString("NoActionsFoundError"),
            };

            // Check if Action Runtime is created successfully
            if (ActionManager.Instance.ActionRuntime is null)
            {
                RuntimeCreationError.Foreground = new SolidColorBrush(Colors.Red);
                RuntimeCreationError.Visibility = Visibility.Visible;
                menuFlyout.Items.Add(noActionsFoundItem);
            }
            else
            {
                if (!String.IsNullOrEmpty(this.InputText.Text))
                {
                    List<ActionEntity> inputs = new List<ActionEntity>
                    {
                        ActionManager.Instance.EntityFactory.CreateTextEntity(this.InputText.Text),
                    };

                    // Return the the actions that are available for the specified file entity type
                    Windows.AI.Actions.Hosting.ActionInstance[] actionInstantiations = ActionManager.Instance.ActionRuntime.ActionCatalog.GetActionsForInputs(inputs.ToArray());

                    // Alphabetize the list of returned actions
                    List<Windows.AI.Actions.Hosting.ActionInstance> sortedList = actionInstantiations.OrderBy(obj => obj.DisplayInfo.Description).ToList();

                    // Add sorted actions to menu flyout
                    foreach (Windows.AI.Actions.Hosting.ActionInstance actionInstantiation in sortedList)
                    {
                        MenuFlyoutItem menuItem = new MenuFlyoutItem
                        {
                            DataContext = actionInstantiation,
                            Text = actionInstantiation.DisplayInfo.Description,
                        };
                        menuItem.Click += ActionMenuItem_Click;
                        menuFlyout.Items.Add(menuItem);
                    }

                    if (sortedList.Count == 0)
                    {
                        menuFlyout.Items.Add(noActionsFoundItem);
                    }
                }
                else
                {
                    menuFlyout.Items.Add(noActionsFoundItem);
                }
            }
            menuFlyout.ShowAt((FrameworkElement)sender);
        }

        private async void ActionMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem clickedItem = sender as MenuFlyoutItem;
            if (clickedItem != null)
            {
                ActionInstance? item = clickedItem.DataContext as ActionInstance;
                if (item != null)
                {
                    // Invoke the action
                    await item.InvokeAsync();

                    NamedActionEntity[] output = item.Context.GetOutputEntities();
                    if (output != null && output.Length > 0)
                    {
                        foreach (NamedActionEntity entity in output)
                        {
                            TextActionEntity textEntity = (TextActionEntity)entity.Entity;
                            if (String.IsNullOrEmpty(this.InputText.Text))
                            {

                                this.InputText.Text += textEntity.Text;
                            }
                            else
                            {
                                this.InputText.Text += ("\n" + textEntity.Text);
                            }
                        }
                    }
                }
            }
        }
    }
}
