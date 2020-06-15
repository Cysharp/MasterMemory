using MasterMemory.GeneratorCore;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;

namespace MasterMemory.MSBuild.Tasks
{
    public class MasterMemoryGenerator : Task
    {
        [Required]
        public string UsingNamespace { get; set; }
        [Required]
        public string InputDirectory { get; set; }
        [Required]
        public string OutputDirectory { get; set; }

        public string PrefixClassName { get; set; }

        public bool AddImmutableConstructor { get; set; }
        public bool ReturnNullIfKeyNotFound { get; set; } = true;

        public bool ForceOverwrite { get; set; }

        public override bool Execute()
        {
            try
            {
                new CodeGenerator().GenerateFile(UsingNamespace, InputDirectory, OutputDirectory, PrefixClassName, AddImmutableConstructor, !ReturnNullIfKeyNotFound, ForceOverwrite, x => this.Log.LogMessage(x));
            }
            catch (Exception ex)
            {
                this.Log.LogErrorFromException(ex, true);
                return false;
            }
            return true;
        }
    }
}