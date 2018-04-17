﻿using System.Web.Mvc;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Extensions;
using Roadkill.Core.Mvc.Controllers;
using Roadkill.Core.Services;
using Roadkill.Tests.Unit.StubsAndMocks;
using Roadkill.Tests.Unit.StubsAndMocks.Mvc;

namespace Roadkill.Tests.Unit.Extensions
{
	[TestFixture]
	[Category("Unit")]
	public class UrlHelperExtensionsTests
	{
		// Objects for the UrlHelper
		private MocksAndStubsContainer _container;
		private ApplicationSettings _applicationSettings;
		private IUserContext _context;
		private PageRepositoryMock _pageRepository;
		private UserServiceMock _userService;
		private PageService _pageService;
		private PageHistoryService _historyService;
		private SettingsService _settingsService;
		private SiteSettings _siteSettings;
		private PluginFactoryMock _pluginFactory;
		private WikiController _wikiController;
		private UrlHelper _urlHelper;

		[SetUp]
		public void Setup()
		{
			// WikiController setup (use WikiController as it's the one typically used by views)
			_container = new MocksAndStubsContainer();

			_applicationSettings = _container.ApplicationSettings;
			_context = _container.UserContext;
			_pageRepository = _container.PageRepository;
			_pluginFactory = _container.PluginFactory;
			_settingsService = _container.SettingsService;
			_siteSettings = _settingsService.GetSiteSettings();
			_siteSettings.Theme = "Mediawiki";

			_userService = _container.UserService;
			_historyService = _container.HistoryService;
			_pageService = _container.PageService;

			_wikiController = new WikiController(_applicationSettings, _userService, _pageService, _context, _settingsService);
			_wikiController.SetFakeControllerContext("~/wiki/index/1");
			_urlHelper = _wikiController.Url;
		}

		[Test]
		public void themecontent_should_return_expected_html()
		{
			// Arrange
			string expectedHtml = "/Themes/Mediawiki/mythemefile.png";

			// Act
			string actualHtml = _urlHelper.ThemeContent("mythemefile.png", _siteSettings);

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml), actualHtml);
		}

		[Test]
		public void csslink_should_return_expected_html()
		{
			// Arrange
			string expectedHtml = @"<link href=""/Assets/CSS/roadkill.css?version={AppVersion}"" rel=""stylesheet"" type=""text/css"" />";
			expectedHtml = expectedHtml.Replace("{AppVersion}", ApplicationSettings.ProductVersion);

			// Act
			string actualHtml = _urlHelper.CssLink("roadkill.css").ToHtmlString();

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml), actualHtml);
		}

		[Test]
		public void scriptlink_should_return_expected_html()
		{
			// Arrange
			string expectedHtml = @"<script type=""text/javascript"" language=""javascript"" src=""/Assets/Scripts/roadkill.js?version={AppVersion}""></script>";
			expectedHtml = expectedHtml.Replace("{AppVersion}", ApplicationSettings.ProductVersion);

			// Act
			string actualHtml = _urlHelper.ScriptLink("roadkill.js").ToHtmlString();

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml), actualHtml);
		}

		[Test]
		public void installerscriptlink_should_expected_html()
		{
			// Arrange
			string expectedHtml = @"<script type=""text/javascript"" language=""javascript"" src=""/Assets/Scripts/roadkill/installer/step1.js?version={AppVersion}""></script>";
			expectedHtml = expectedHtml.Replace("{AppVersion}", ApplicationSettings.ProductVersion);

			// Act
			string actualHtml = _urlHelper.InstallerScriptLink("step1.js").ToHtmlString();

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml), actualHtml);
		}

		[Test]
		public void bootstrapcss_should_return_expected_html()
		{
			// Arrange
			string expectedHtml = @"<link href=""/Assets/bootstrap/css/bootstrap.min.css?version={AppVersion}"" rel=""stylesheet"" type=""text/css"" />";
			expectedHtml = expectedHtml.Replace("{AppVersion}", ApplicationSettings.ProductVersion);

			// Act
			string actualHtml = _urlHelper.BootstrapCSS().ToHtmlString();

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml), actualHtml);
		}

		[Test]
		public void bootstrapjs_should_return_expected_html()
		{
			// Arrange
			string expectedHtml = @"<script type=""text/javascript"" language=""javascript"" src=""/Assets/bootstrap/js/bootstrap.min.js?version={AppVersion}""></script>";
			expectedHtml += "\n" +@"<script type=""text/javascript"" language=""javascript"" src=""/Assets/bootstrap/js/respond.min.js?version={AppVersion}""></script>";
			expectedHtml = expectedHtml.Replace("{AppVersion}", ApplicationSettings.ProductVersion);

			// Act
			string actualHtml = _urlHelper.BootstrapJS().ToHtmlString();

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml), actualHtml);
		}
	}
}
