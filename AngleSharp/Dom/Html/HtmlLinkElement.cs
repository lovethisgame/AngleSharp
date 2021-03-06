﻿namespace AngleSharp.Dom.Html
{
    using System;
    using System.Threading.Tasks;
    using AngleSharp.Dom.Collections;
    using AngleSharp.Extensions;
    using AngleSharp.Html;
    using AngleSharp.Network;
    using AngleSharp.Services.Styling;

    /// <summary>
    /// Represents the HTML link element.
    /// </summary>
    sealed class HtmlLinkElement : HtmlElement, IHtmlLinkElement
    {
        #region Fields

        IStyleSheet _sheet;
        TokenList _relList;
        SettableTokenList _sizes;
        Task<IResponse> _current;
        Url _cachedUrl;

        #endregion

        #region ctor

        /// <summary>
        /// Creates a new HTML link element.
        /// </summary>
        public HtmlLinkElement(Document owner, String prefix = null)
            : base(owner, Tags.Link, prefix, NodeFlags.Special | NodeFlags.SelfClosing)
        {
            RegisterAttributeObserver(AttributeNames.Media, UpdateMedia);
            RegisterAttributeObserver(AttributeNames.Disabled, UpdateDisabled);
            RegisterAttributeObserver(AttributeNames.Href, value => TargetChanged());
            RegisterAttributeObserver(AttributeNames.Type, value => TargetChanged());
        }

        #endregion

        #region Design properties

        /// <summary>
        /// Gets or sets if the link has been visited.
        /// </summary>
        internal Boolean IsVisited
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets if the link is currently active.
        /// </summary>
        internal Boolean IsActive
        {
            get;
            set;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the url of the link elements address.
        /// </summary>
        public Url Url
        {
            get { return this.HyperReference(GetOwnAttribute(AttributeNames.Href)); }
        }

        /// <summary>
        /// Gets or sets the URI for the target resource.
        /// </summary>
        public String Href
        {
            get { return Url.Href; }
            set { SetOwnAttribute(AttributeNames.Href, value); }
        }

        /// <summary>
        /// Gets or sets the language code for the linked resource.
        /// </summary>
        public String TargetLanguage
        {
            get { return GetOwnAttribute(AttributeNames.HrefLang); }
            set { SetOwnAttribute(AttributeNames.HrefLang, value); }
        }

        /// <summary>
        /// Gets or sets the character encoding for the target resource.
        /// </summary>
        public String Charset
        {
            get { return GetOwnAttribute(AttributeNames.Charset); }
            set { SetOwnAttribute(AttributeNames.Charset, value); }
        }

        /// <summary>
        /// Gets or sets the forward relationship of the linked resource from the document to the resource.
        /// </summary>
        public String Relation
        {
            get { return GetOwnAttribute(AttributeNames.Rel); }
            set { SetOwnAttribute(AttributeNames.Rel, value); }
        }

        /// <summary>
        /// Gets the list of relations contained in the rel attribute.
        /// </summary>
        public ITokenList RelationList
        {
            get
            {
                if (_relList == null)
                {
                    _relList = new TokenList(GetOwnAttribute(AttributeNames.Rel));
                    _relList.Changed += (s, ev) => UpdateAttribute(AttributeNames.Rel, _relList.ToString());
                }

                return _relList; 
            }
        }

        /// <summary>
        /// Gets the list of sizes defined in the sizes attribute.
        /// </summary>
        public ISettableTokenList Sizes
        {
            get
            {
                if (_sizes == null)
                {
                    _sizes = new SettableTokenList(GetOwnAttribute(AttributeNames.Sizes));
                    _sizes.Changed += (s, ev) => UpdateAttribute(AttributeNames.Sizes, _sizes.Value);
                }

                return _sizes; 
            }
        }

        /// <summary>
        /// Gets or sets the reverse relationship of the linked resource from the document to the resource.
        /// </summary>
        public String Rev
        {
            get { return GetOwnAttribute(AttributeNames.Rev); }
            set { SetOwnAttribute(AttributeNames.Rev, value); }
        }

        /// <summary>
        /// Gets or sets if the stylesheet is enabled or disabled.
        /// </summary>
        public Boolean IsDisabled
        {
            get { return GetOwnAttribute(AttributeNames.Disabled).ToBoolean(); }
            set { SetOwnAttribute(AttributeNames.Disabled, value ? String.Empty : null); }
        }

        /// <summary>
        /// Gets or sets the name of the target frame to which the resource applies.
        /// </summary>
        public String Target
        {
            get { return GetOwnAttribute(AttributeNames.Target); }
            set { SetOwnAttribute(AttributeNames.Target, value); }
        }

        /// <summary>
        /// Gets or sets the use with one or more target media.
        /// </summary>
        public String Media
        {
            get { return GetOwnAttribute(AttributeNames.Media); }
            set { SetOwnAttribute(AttributeNames.Media, value); }
        }

        /// <summary>
        /// Gets or sets the content type of the style sheet language.
        /// </summary>
        public String Type
        {
            get { return GetOwnAttribute(AttributeNames.Type); }
            set { SetOwnAttribute(AttributeNames.Type, value); }
        }

        /// <summary>
        /// Gets the associated stylesheet.
        /// </summary>
        public IStyleSheet Sheet
        {
            get { return _sheet; }
        }

        #endregion

        #region Internal methods

        void UpdateMedia(String value)
        {
            if (_sheet != null)
                _sheet.Media.MediaText = value;
        }

        void UpdateDisabled(String value)
        {
            if (_sheet != null)
                _sheet.IsDisabled = value != null;
        }

        #endregion

        #region Helpers

        void TargetChanged()
        {
            var owner = Owner;

            if (owner.Options.IsStyling())
            {
                var url = Url;

                if (url != null && (_current == null || !url.Equals(_cachedUrl)))
                {
                    var config = owner.Options;
                    var engine = config.GetStyleEngine(Type ?? MimeTypes.Css);

                    owner.Tasks.Cancel(_current);

                    if (engine != null)
                    {
                        var request = this.CreateRequestFor(url);
                        var options = new StyleOptions
                        {
                            Element = this,
                            Title = Title,
                            IsDisabled = IsDisabled,
                            IsAlternate = RelationList.Contains(Keywords.Alternate),
                            Configuration = config
                        };

                        _cachedUrl = url;
                        _current = owner.Tasks.Add(cancel => owner.Loader.FetchAsync(request, cancel));
                        _current.ContinueWith(m =>
                        {
                            var result = m.Result;

                            if (result != null)
                            {
                                _sheet = engine.Parse(result, options);
                                result.Dispose();
                            }
                        });
                    }
                }
            }
        }

        #endregion
    }
}
