﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MongoDB.Driver;

namespace MongoDB.Framework.Tracking
{
    public class DefaultChangeTracker : ChangeTracker
    {
        #region Private Fields

        private EntityMapper entityMapper;
        private Dictionary<object, TrackedObject> trackedObjects;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultChangeTracker"/> class.
        /// </summary>
        /// <param name="entityMapper">The entity mapper.</param>
        public DefaultChangeTracker(EntityMapper entityMapper)
        {
            if (entityMapper == null)
                throw new ArgumentNullException("entityMapper");

            this.entityMapper = entityMapper;
            this.trackedObjects = new Dictionary<object, TrackedObject>();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the change set.
        /// </summary>
        /// <returns></returns>
        public override ChangeSet GetChangeSet()
        {
            List<object> added = new List<object>();
            List<object> modified = new List<object>();
            List<object> removed = new List<object>();
            foreach (var trackedObject in this.trackedObjects.Values)
            {
                trackedObject.DetermineState();
                switch(trackedObject.State)
                {
                    case TrackedObjectState.Added:
                        added.Add(trackedObject.Current);
                        break;
                    case TrackedObjectState.Removed:
                        removed.Add(trackedObject.Current);
                        break;
                    case TrackedObjectState.Modified:
                        modified.Add(trackedObject.Current);
                        break;
                }
            }

            return new ChangeSet(added, modified, removed);
        }

        /// <summary>
        /// Gets the tracked object and begins tracking it if not already.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns></returns>
        public override TrackedObject GetTrackedObject(object obj)
        {
            TrackedObject trackedObject;
            if (!this.trackedObjects.TryGetValue(obj, out trackedObject))
                trackedObject = this.Track(null, obj);
            return trackedObject;
        }

        /// <summary>
        /// Tracks the specified original.
        /// </summary>
        /// <param name="original">The original.</param>
        /// <param name="current">The current.</param>
        public override TrackedObject Track(Document original, object current)
        {
            var trackedObject = new TrackedObject(this.entityMapper, original, current);
            this.trackedObjects.Add(current, trackedObject);
            return trackedObject;
        }

        #endregion
    }
}