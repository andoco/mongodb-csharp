﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using MongoDB.Driver;

namespace MongoDB.Framework.Configuration.Visitors
{
    public class MemberPathToDocumentKeyVisitor : IMapVisitor
    {
        private Stack<MemberInfo> memberPathParts;
        private List<string> documentKeyParts;

        private string documentKey;

        private MemberInfo CurrentMemberInfo
        {
            get { return this.memberPathParts.Peek(); }
        }

        private bool IsFinished
        {
            get { return this.memberPathParts.Count == 0; }
        }

        public string DocumentKey
        {
            get
            {
                return string.Join(".", this.documentKeyParts.ToArray());
            }
        }

        public MemberPathToDocumentKeyVisitor(IEnumerable<MemberInfo> memberPathParts)
        {
            if (memberPathParts == null)
                throw new ArgumentNullException("memberPathParts");

            this.documentKey = "";
            this.documentKeyParts = new List<string>();
            this.memberPathParts = new Stack<MemberInfo>(memberPathParts.Reverse());
        }

        public void VisitRootEntityMap(RootEntityMap rootEntityMap)
        {
            rootEntityMap.IdMap.Accept(this);
            this.VisitEntityMap(rootEntityMap);
        }

        public void VisitEntityMap(EntityMap entityMap)
        {
            if (this.IsFinished)
                return;

            var memberMap = entityMap.GetMemberMap(this.CurrentMemberInfo.DeclaringType, this.CurrentMemberInfo.Name);
            memberMap.Accept(this);
        }

        public void VisitDiscriminatedEntityMap(DiscriminatedEntityMap discriminatedEntityMap)
        {
            throw new NotSupportedException();
        }

        public void VisitPrimitiveMemberMap(PrimitiveMemberMap primitiveMemberMap)
        {
            this.memberPathParts.Pop();
            this.documentKeyParts.Add(primitiveMemberMap.DocumentKey);
        }

        public void VisitComponentMemberMap(ComponentMemberMap componentMemberMap)
        {
            this.memberPathParts.Pop();
            this.documentKeyParts.Add(componentMemberMap.DocumentKey);
            if(!this.IsFinished)
                componentMemberMap.EntityMap.Accept(this);
        }

        public void VisitIdMap(IdMap idMap)
        {
            if (this.CurrentMemberInfo.Name == idMap.MemberName)
            {
                this.documentKeyParts.Add("_id");
                this.memberPathParts.Pop();
            }
        }
    }
}