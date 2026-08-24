using CommunityToolkit.Mvvm.ComponentModel;

namespace EasyWebsiteManager.Models;

public class AppSettings : ObservableObject
{
    private bool _confirmDelete = true;

    public bool ConfirmDelete
    {
        get => _confirmDelete;
        set => SetProperty(ref _confirmDelete, value);
    }
}