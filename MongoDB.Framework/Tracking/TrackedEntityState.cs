﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoDB.Framework.Tracking
{
    public enum TrackedEntityState
    {
        PossiblyModified,
        Inserted,
        Modified,
        Deleted
    }
}