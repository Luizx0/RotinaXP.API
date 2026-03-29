namespace RotinaXP.API.Workers.EmailLayouts;

public sealed class StreakReminderEmailLayout : IEmailLayout
{
    public string Subject => "Nao deixe sua streak morrer!";

    public string Render(IDictionary<string, string> data)
    {
        data.TryGetValue("userName", out var userName);
        data.TryGetValue("streakDays", out var streakDays);

        return $"""
            <html>
            <body>
              <h1>Ola, {userName ?? "usuario"}!</h1>
              <p>Voce tem uma streak de <strong>{streakDays ?? "0"} dias</strong> em risco.</p>
              <p>Complete pelo menos uma tarefa hoje para manter seu progresso.</p>
            </body>
            </html>
            """;
    }
}
