namespace QRBee.Load.Generator;

internal class Anomaly
{
    public double Probability { get; set; }
    public Dictionary<string,string> Parameters  { get; set; } = new();
}

internal class GeneratorSettings
{
    public string QRBeeURL              { get; set; } = "http://localhost:5000/";
    public int DefaultConnectionLimit   { get; set; } = 100;
    public int NumberOfClients          { get; set; } = 100;
    public int NumberOfMerchants        { get; set; } = 10;
    public int NumberOfThreads          { get; set; } = 20;
    public int DelayBetweenMessagesMSec { get; set; } = 100;
    public int DelayJitterMSec          { get; set; } = 50;
    public double MinAmount             { get; set; } = 10;
    public double MaxAmount             { get; set; } = 100;

    public Anomaly LoadSpike   { get; set; } = new();
    public Anomaly LargeAmount { get; set; } = new();
    public Anomaly TransactionCorruption { get; set;} = new();
    public Anomaly UnconfirmedTransaction { get; set; } = new();
}
