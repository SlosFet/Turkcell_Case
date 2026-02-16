using System;

[Serializable]
public class PointsLedgerData
{
    public string LedgerId;
    public string UserId;
    public int PointsDelta;
    public string Source;
    public string SourceRef;
    public string CreatedAt;
}
