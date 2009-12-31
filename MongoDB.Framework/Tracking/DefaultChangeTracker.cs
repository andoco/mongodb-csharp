﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MongoDB.Driver;
using MongoDB.Framework.Mapping;

namespace MongoDB.Framework.Tracking
{
    public class DefaultChangeTracker : ChangeTracker
    {
        #region Private Fields

        private IMongoContext mongoContext;
        private Dictionary<object, TrackedObject> trackedObjects;
        private Dictionary<string, TrackedObject> trackedObjectsById;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultChangeTracker"/> class.
        /// </summary>
        /// <param name="mongoContext">The mongo context.</param>
        public DefaultChangeTracker()
        {
            this.trackedObjects = new Dictionary<object, TrackedObject>();
            this.trackedObjectsById = new Dictionary<string, TrackedObject>();
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

            //Preprocess all entities to gather and mark reference information in the change tracker...
            var trackedObjectsCopy = new List<TrackedObject>(this.trackedObjects.Values);
            foreach (var trackedObject in trackedObjectsCopy)
            {
                var referenceProcessor = new ReferenceProcessor(this.mongoContext.Configuration.MappingStore, this, trackedObject);
                referenceProcessor.Process();
            }
            
            //have each determine state...
            foreach (var trackedObject in this.trackedObjects.Values)
            {
                trackedObject.DetermineState();
                switch(trackedObject.State)
                {
                    case TrackedObjectState.Inserted:
                        added.Add(trackedObject.Current);
                        break;
                    case TrackedObjectState.Deleted:
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
        /// Initializes the specified mongo context.
        /// </summary>
        /// <param name="mongoContext">The mongo context.</param>
        public override void Initialize(IMongoContext mongoContext)
        {
            this.mongoContext = mongoContext;
        }

        /// <summary>
        /// Tracks the specified original.
        /// </summary>
        /// <param name="original">The original.</param>
        /// <param name="current">The current.</param>
        public override TrackedObject Track(Document original, object current)
        {
            var trackedObject = new TrackedObject(this.mongoContext, original, current);
            this.trackedObjects.Add(current, trackedObject);
            return trackedObject;
        }

        /// <summary>
        /// Tries to get a tracked object by id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public override bool TryGetTrackedObjectById(object id, out TrackedObject trackedObject)
        {
            trackedObject =  this.trackedObjects.Values.FirstOrDefault(to => Object.Equals(to.GetId(), id));
            return trackedObject != null;
        }

        #endregion
    }
}