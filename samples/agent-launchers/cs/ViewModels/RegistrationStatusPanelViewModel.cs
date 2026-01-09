// Copyright (C) Microsoft Corporation. All rights reserved.

using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace SampleAgentLauncher.ViewModels;

public partial class RegistrationStatusPanelViewModel : ObservableObject
{
    public ObservableCollection<RegistrationStatusItemViewModel> StatusItems { get; } = new();

    public RegistrationStatusPanelViewModel()
    {
    }

    public void AddStatusItem(RegistrationStatusItemViewModel item)
    {
        StatusItems.Add(item);
    }
}
