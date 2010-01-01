﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoDB.Framework.Mapping
{
    public interface IMapVisitor
    {
        void ProcessClass(ClassMap classMap);
        void ProcessSuperClass(SuperClassMap superClassMap);
        void ProcessRootClass(RootClassMap rootClassMap);
        void ProcessNestedClass(NestedClassMap nestedClassMap);
        void ProcessSubClass(SubClassMap subClassMap);
        void ProcessId(IdMap idMap);
        void ProcessMember(MemberMap memberMap);
        void ProcessExtendedProperties(ExtendedPropertiesMap extendedPropertiesMap);

        void Visit(ClassMap classMap);
        void Visit(SuperClassMap superClassMap);
        void Visit(RootClassMap rootClassMap);
        void Visit(NestedClassMap nestedClassMap);
        void Visit(SubClassMap subClassMap);
        void Visit(IdMap idMap);
        void Visit(MemberMap memberMap);
        void Visit(ExtendedPropertiesMap extendedPropertiesMap);
    }
}