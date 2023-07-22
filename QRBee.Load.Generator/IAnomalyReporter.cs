namespace QRBee.Load.Generator
{
    internal interface IAnomalyReporter
    {
        void Report(DateTime start, DateTime end, string description);
    }
}