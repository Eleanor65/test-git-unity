﻿using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace DTI.SourceControl
{
    public enum Status
    {
        NotUnderVC,
        Missing,
        Deleted,
        Added,
        Modified,
        Conflicted,
        NotFound
    }
}