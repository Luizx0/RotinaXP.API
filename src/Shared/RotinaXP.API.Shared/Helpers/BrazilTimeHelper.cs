namespace QuoadAPI.Shared.Helpers;

/// <summary>
/// Helper para trabalhar com o timezone de Brasília.
/// </summary>
public static class BrazilTimeHelper
{
    private static readonly TimeZoneInfo BrazilTimeZone = GetBrazilTimeZone();

    /// <summary>
    /// Obtém o timezone de Brasília com fallback
    /// </summary>
    private static TimeZoneInfo GetBrazilTimeZone()
    {
        try
        {
            // Tentar primeiro o ID do Windows
            return TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
        }
        catch
        {
            try
            {
                // Tentar o ID do Linux
                return TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
            }
            catch
            {
                // Fallback: usar UTC-3 (horário padrão de Brasília)
                return TimeZoneInfo.CreateCustomTimeZone("Brazil", TimeSpan.FromHours(-3), "Brasília", "Brasília");
            }
        }
    }

    /// <summary>
    /// Obtém o horário atual de Brasília
    /// </summary>
    public static DateTime GetBrazilTime()
    {
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, BrazilTimeZone);
    }

    /// <summary>
    /// Obtém o horário atual de Brasília como UTC (para colunas timestamp with time zone)
    /// Retorna DateTime.UtcNow, pois o PostgreSQL timestamp with time zone armazena em UTC
    /// e converte automaticamente para o timezone do cliente quando necessário
    /// </summary>
    public static DateTime GetBrazilTimeAsUtc()
    {
        // Para timestamp with time zone, o PostgreSQL espera UTC
        // O horário será armazenado como UTC e convertido automaticamente quando lido
        return DateTime.UtcNow;
    }
}