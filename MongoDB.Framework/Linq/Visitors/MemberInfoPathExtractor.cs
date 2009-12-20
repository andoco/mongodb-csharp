﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Remotion.Data.Linq.Parsing;

namespace MongoDB.Framework.Linq.Visitors
{
    public class MemberInfoPathExtractor : ExpressionTreeVisitor
    {
        #region Public Static Methods

        public static IList<MemberInfo> ExtractFrom(Expression expression)
        {
            MemberInfoPathExtractor visitor = new MemberInfoPathExtractor();
            visitor.VisitExpression(expression);
            return visitor.members.ToList();
        }

        #endregion

        #region Private Fields

        private Stack<MemberInfo> members = new Stack<MemberInfo>();

        #endregion

        #region Public Methods

        protected override Expression VisitMemberExpression(MemberExpression expression)
        {
            this.members.Push(expression.Member);
            return base.VisitMemberExpression(expression);
        }

        #endregion
    }
}