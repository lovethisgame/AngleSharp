﻿namespace AngleSharp.Dom.Css
{
    using System;
    using System.Collections.Generic;
    using AngleSharp.Css.Values;

    /// <summary>
    /// Represents the keyframe selector.
    /// </summary>
    sealed class KeyframeSelector : IKeyframeSelector
    {
        #region Fields

        readonly List<Percent> _stops;

        #endregion

        #region ctor

        public KeyframeSelector(IEnumerable<Percent> stops)
        {
            _stops = new List<Percent>(stops);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets an enumeration over all stops.
        /// </summary>
        public IEnumerable<Percent> Stops
        {
            get { return _stops; }
        }

        /// <summary>
        /// Gets the text representation of the keyframe selector.
        /// </summary>
        public String Text
        {
            get
            {
                var stops = new String[_stops.Count];

                for (int i = 0; i < stops.Length; i++)
                    stops[i] = _stops[i].ToString();

                return String.Join(", ", stops);
            }
        }

        #endregion
    }
}
