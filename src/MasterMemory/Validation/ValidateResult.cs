using System;
using System.Collections.Generic;
using System.Text;

namespace MasterMemory.Validation
{
    public class ValidateResult
    {
        List<(Type type, string message)> result = new List<(Type type, string message)>();

        public bool IsValidationFailed => result.Count != 0;

        public IReadOnlyList<(Type type, string message)> FailedResults => result;

        public string FormatFailedResults()
        {
            var sb = new StringBuilder();
            foreach (var item in result)
            {
                sb.AppendLine(item.type.FullName + " - " + item.message);
            }
            return sb.ToString();
        }

        internal void AddFail(Type type, string message)
        {
            result.Add((type, message));
        }
    }
}
