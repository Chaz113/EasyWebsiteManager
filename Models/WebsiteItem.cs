namespace EasyWebsiteManager.Models;

public class WebsiteItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = "";

    public string Url { get; set; } = "";
}