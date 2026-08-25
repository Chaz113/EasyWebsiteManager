namespace EasyWebsiteManager.Models;

public class AppData
{
    public int DataVersion { get; set; } = 2;

    public List<WebsiteCategory> Categories { get; set; } = new();

    public AppSettings Settings { get; set; } = new();
}