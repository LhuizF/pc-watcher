using System;
using System.Diagnostics;

class Program
{
  static void Main(string[] args)
  {
    Stopwatch stopwatch = Stopwatch.StartNew();
    stopwatch.Start();
    Console.WriteLine("START\n");

    var processes = Process.GetProcesses().GroupBy(p => p.ProcessName).Select(processGroup =>
      new {
        Id = processGroup.First().Id,
        Name = processGroup.Key,
        Count = processGroup.Count(),
        TotalMemory = processGroup.Sum(p => p.WorkingSet64),
        TotalCpu = processGroup.Sum(p => GetTotalCpu(p)),
      }
    ).OrderByDescending(g => g.TotalMemory).ToList();

    string content = $"{"ID", -10} | {"App", -40} | {"RAM", 4} MB | CPU\n";

    foreach (var process in processes)
    {
      try
      {
          content += $"{process.Id, -10} | {process.Name, -40} | {process.TotalMemory / 1024 / 1024, 4} MB | {process.TotalCpu}\n";
      }
      catch (Exception ex)
      {
          Console.WriteLine($"Erro ao acessar o processo: {ex.Message}");
      }
    }

    CreateFile(content);

    stopwatch.Stop();

    TimeSpan elapsed = stopwatch.Elapsed;
    string formattedTime = string.Format("{0:D2}:{1:D2}", elapsed.Minutes, elapsed.Seconds);
    Console.WriteLine($"Tempo de execução: {formattedTime}");

    // Console.WriteLine("TOTAL " + processes.Count());
    Console.WriteLine("\nFINISH");
  }

  private static void CreateFile(string content)
  {
    string fileName = "result.txt";
    try
    {
      using (StreamWriter writer = new StreamWriter(fileName))
    {
      writer.WriteLine(content);
    }
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Erro ao criar o arquivo: {ex.Message}");
    }
  }

  static double GetTotalCpu(Process process)
  {
    double totalCpu = 0;
    try
    {
      Thread.Sleep(500);
      PerformanceCounter cpuCounter = new ("Process", "% Processor Time", process.ProcessName);
      totalCpu+= cpuCounter.NextValue();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro ao acessar o CPU do processo: {process.ProcessName}: {ex.Message}");
    }

    return totalCpu;
  }
}
