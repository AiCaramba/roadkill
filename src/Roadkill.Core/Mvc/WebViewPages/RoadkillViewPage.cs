﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Roadkill.Core.Configuration;
using Roadkill.Core.Services;
using Roadkill.Core.Text;
using Roadkill.Core.Text.Parsers;
using Roadkill.Core.Text.TextMiddleware;
using StructureMap.Attributes;

namespace Roadkill.Core.Mvc.WebViewPages
{
	public abstract class RoadkillViewPage<T> : WebViewPage<T>
	{
		// Constructor injection isn't viable here, as this class is created by the ASP.NET runtime
		private SiteSettings _siteSettings;

		[SetterProperty]
		public ApplicationSettings ApplicationSettings { get; set; }
		
		[SetterProperty]
		public IUserContext RoadkillContext { get; set; }
		
		[SetterProperty]
		public TextMiddlewareBuilder TextMiddlewareBuilder { get; set; }

        [SetterProperty]
        public IMarkupConverter MarkupConverter { get; set; }

        [SetterProperty]
		public SettingsService SettingsService { get; set; }

		public SiteSettings SiteSettings
		{
			get
			{
				if (_siteSettings == null)
					_siteSettings = SettingsService.GetSiteSettings();

				return _siteSettings;
			}
		}
	}
}
