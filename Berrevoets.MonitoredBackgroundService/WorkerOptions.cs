namespace Berrevoets;

public class WorkerOptions
{
    public int NumberOfTasks { get; set; } = 1; // Default value
    public TimeSpan MaxTaskDuration { get; set; } = TimeSpan.FromSeconds(120); // Default to 120 seconds
}