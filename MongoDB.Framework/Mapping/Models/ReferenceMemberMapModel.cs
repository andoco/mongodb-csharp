﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoDB.Framework.Mapping.Models
{
    public class ReferenceMemberMapModel : KeyMemberMapModel
    {
        public Cascade Cascade { get; set; }
    }
}