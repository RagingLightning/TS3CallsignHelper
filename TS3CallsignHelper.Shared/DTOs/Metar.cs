namespace TS3CallsignHelper.Common.DTOs;
public class Metar
{
    public string? Airport { get; set; }
    public DateTime? ReportTime { get; set; }
    public bool Automatic { get; set; }
    public Wind Winds { get; set; }
    public int Temperature { get; set; }

    public int Dewpoint { get; set; }
    public Pressure Qnh { get; set; }
}

public class Wind
{
    public int Direction { get; set; }
    public int VariableFrom { get; set; }
    public int VariableTo { get; set; }
    public int Magnitude { get; set; }
    public int Gusts { get; set; }
}

public class Pressure
{
    public char Unit { get; set; }
    public double Value { get; set; }
}
