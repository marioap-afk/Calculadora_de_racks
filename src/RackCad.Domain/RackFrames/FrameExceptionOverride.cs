namespace RackCad.Domain.RackFrames
{
    public sealed class FrameExceptionOverride
    {
        public ExceptionType ExceptionType { get; set; }
        public string TargetId { get; set; }
        public string StandardValue { get; set; }
        public string OverrideValue { get; set; }
        public string Reason { get; set; }
    }
}
