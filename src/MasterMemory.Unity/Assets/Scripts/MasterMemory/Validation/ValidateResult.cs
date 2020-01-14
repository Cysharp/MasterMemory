using System;
using System.Collections.Generic;
using System.Text;

namespace MasterMemory.Validation
{
    public class ValidateResult
    {
        List<FaildItem> result = new List<FaildItem>();

        public bool IsValidationFailed => result.Count != 0;

        public IReadOnlyList<FaildItem> FailedResults => result;

        public string FormatFailedResults()
        {
            var sb = new StringBuilder();
            foreach (var item in result)
            {
                sb.AppendLine(item.Type.FullName + " - " + item.Message);
            }
            return sb.ToString();
        }

        internal void AddFail(Type type, string message, object data)
        {
            result.Add(new FaildItem(type, message, data));
        }
    }

    public readonly struct FaildItem
    {
        public FaildItem(Type type, string message, object data)
        {
            Type = type;
            Message = message;
            Data = data;
        }

        public Type Type { get; }
        public string Message { get; }
        public object Data { get; }
    }
}
