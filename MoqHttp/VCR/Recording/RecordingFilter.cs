using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace MoqHttp.VCR.Recording
{
    /// <summary>
    /// Filter configuration for selective recording
    /// </summary>
    public class RecordingFilter
    {
        /// <summary>
        /// Filter predicates - all must return true for request to be recorded
        /// </summary>
        public List<Func<HttpRequest, bool>> Predicates { get; }

        public RecordingFilter()
        {
            Predicates = new List<Func<HttpRequest, bool>>();
        }

        /// <summary>
        /// Add a filter predicate
        /// </summary>
        public void AddFilter(Func<HttpRequest, bool> predicate)
        {
            Predicates.Add(predicate);
        }

        /// <summary>
        /// Check if a request should be recorded
        /// </summary>
        public bool ShouldRecord(HttpRequest request)
        {
            // If no filters, record everything
            if (Predicates.Count == 0)
            {
                return true;
            }

            // All predicates must pass
            foreach (var predicate in Predicates)
            {
                if (!predicate(request))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
