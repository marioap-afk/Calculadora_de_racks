using RackCad.Domain.RackFrames;

namespace RackCad.UI
{
    public sealed class FrameExceptionEditorRow
    {
        public FrameExceptionEditorRow(string targetId, string fieldName, ExceptionType exceptionType, string standardValue, string overrideValue)
        {
            TargetId = targetId;
            FieldName = fieldName;
            ExceptionType = exceptionType;
            StandardValue = standardValue;
            OverrideValue = overrideValue;
        }

        public string TargetId { get; set; }
        public string FieldName { get; set; }
        public ExceptionType ExceptionType { get; set; }
        public string StandardValue { get; set; }
        public string OverrideValue { get; set; }
        public string Summary => TargetId + ": " + StandardValue + " -> " + OverrideValue;
    }
}
