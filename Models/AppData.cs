namespace EasyWebsiteManager.Models;

public class AppData
{
    public List<WebsiteCategory> Categories { get; set; } = new();

    public AppSettings Settings { get; set; } = new();
}