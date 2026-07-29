namespace CouchDb.Domain.Enums
{
    public enum EWorkflowDataStatus
    {
        Pending, //0
        Running,//1
        Pause, //2
        Failed, //3
        Done, //4
        Reseting, //5
        Reseted, //6

        PendingCleaning, //7
        Cleaning, //8
        Cleaned, //9
    }
}
