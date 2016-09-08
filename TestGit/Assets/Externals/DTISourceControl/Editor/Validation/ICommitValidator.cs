using System.Collections.Generic;

namespace DTI.SourceControl.Validation
{
    public interface ICommitValidator
    {
        bool IsValid(IEnumerable<string> files, out IEnumerable<string> errorMessages);
    }
}