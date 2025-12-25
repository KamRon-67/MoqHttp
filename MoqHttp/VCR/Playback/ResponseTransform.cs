using System;
using System.Collections.Generic;
using MoqHttp.VCR.Models;

namespace MoqHttp.VCR.Playback
{
    /// <summary>
    /// Transform configuration for modifying responses during playback
    /// </summary>
    public class ResponseTransform
    {
        /// <summary>
        /// List of transformation functions to apply
        /// </summary>
        public List<Func<RecordedResponse, RecordedResponse>> Transformers { get; }

        public ResponseTransform()
        {
            Transformers = new List<Func<RecordedResponse, RecordedResponse>>();
        }

        /// <summary>
        /// Add a transformation function
        /// </summary>
        public void AddTransformer(Func<RecordedResponse, RecordedResponse> transformer)
        {
            Transformers.Add(transformer);
        }

        /// <summary>
        /// Apply all transformations to a response
        /// </summary>
        public RecordedResponse Apply(RecordedResponse response)
        {
            if (response == null || Transformers.Count == 0)
            {
                return response;
            }

            var transformed = response;
            foreach (var transformer in Transformers)
            {
                transformed = transformer(transformed);
            }

            return transformed;
        }
    }
}
