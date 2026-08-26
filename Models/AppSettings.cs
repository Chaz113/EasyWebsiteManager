using CommunityToolkit.Mvvm.ComponentModel;

namespace EasyWebsiteManager.Models;

public class AppSettings : ObservableObject
{
    private bool _confirmDelete = true;
    private string _appearance = "System";
    private bool _textColorDefaultsMigrated;

    public bool ConfirmDelete
    {
        get => _confirmDelete;
        set => SetProperty(ref _confirmDelete, value);
    }

    public string Appearance
    {
        get => _appearance;
        set => SetProperty(ref _appearance, value);
    }

    public bool TextColorDefaultsMigrated
    {
        get => _textColorDefaultsMigrated;
        set => SetProperty(ref _textColorDefaultsMigrated, value);
    }
}