namespace RotinaXP.API.Workers.EmailLayouts;

public interface IEmailLayout
{
    string Subject { get; }
    string Render(IDictionary<string, string> data);
}
