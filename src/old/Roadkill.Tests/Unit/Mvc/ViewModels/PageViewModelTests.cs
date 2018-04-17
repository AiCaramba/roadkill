﻿using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Roadkill.Core.Cache;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Text;
using Roadkill.Core.Text.Menu;
using Roadkill.Core.Text.TextMiddleware;
using Roadkill.Tests.Unit.StubsAndMocks;

namespace Roadkill.Tests.Unit.Mvc.ViewModels
{
	[TestFixture]
	[Category("Unit")]
	public class PageViewModelTests
	{
		private MocksAndStubsContainer _container;

		private ApplicationSettings _appSettings;
		private SettingsRepositoryMock _settingsRepository;
		private TextMiddlewareBuilder _textMiddlewareBuilder;
		private PluginFactoryMock _pluginFactory;

		[SetUp]
		public void Setup()
		{
			_container = new MocksAndStubsContainer();

			_pluginFactory = _container.PluginFactory;
			_appSettings = _container.ApplicationSettings;
			_appSettings.Installed = true;
			_settingsRepository = _container.SettingsRepository;
			_textMiddlewareBuilder = _container.TextMiddlewareBuilder;
		}

		[Test]
		public void empty_constructor_should_fill_property_defaults()
		{
			// Arrange + act
			PageViewModel model = new PageViewModel();

			// Assert
			Assert.That(model.IsCacheable, Is.True);
			Assert.That(model.PluginHeadHtml, Is.EqualTo(""));
			Assert.That(model.PluginFooterHtml, Is.EqualTo(""));
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void PageContent_Constructor_Should_Throw_Exception_When_PageContent_IsNull()
		{
			// Arrange + Act + Assert
			PageViewModel model = new PageViewModel(null, new PageHtml());
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void PageContent_Constructor_Should_Throw_Exception_When_PageContent_Page_IsNull()
		{
			// Arrange
			PageContent content = new PageContent();
			content.Page = null;

			// Act + Assert
			PageViewModel model = new PageViewModel(content, new PageHtml());
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void PageContent_Constructor_Should_Throw_Exception_When_MarkupConverter_IsNull()
		{
			// Arrange
			PageContent content = new PageContent();
			content.Page = new Page();

			// Act + Assert
			PageViewModel model = new PageViewModel(content, null);
		}

		[Test]
		public void page_constructor_should_fill_properties()
		{
			// Arrange
			Page page = new Page();
			page.Id = 3;
			page.Title = "my title";
			page.CreatedBy = "me";
			page.CreatedOn = DateTime.Now;
			page.IsLocked = true;
			page.ModifiedBy = "me2";
			page.ModifiedOn = DateTime.Now.AddDays(1);
			page.Tags = "tag1,tag2,tag3";

			// Act
			PageViewModel model = new PageViewModel(page);

			// Assert
			Assert.That(model.Id, Is.EqualTo(page.Id));
			Assert.That(model.Title, Is.EqualTo(page.Title));
			Assert.That(model.CreatedBy, Is.EqualTo(page.CreatedBy));
			Assert.That(model.ModifiedBy, Is.EqualTo(page.ModifiedBy));
			Assert.That(model.CreatedOn, Is.EqualTo(page.CreatedOn));
			Assert.That(model.CreatedOn.Kind, Is.EqualTo(DateTimeKind.Utc));
			Assert.That(model.ModifiedOn, Is.EqualTo(page.ModifiedOn));
			Assert.That(model.ModifiedOn.Kind, Is.EqualTo(DateTimeKind.Utc));

			Assert.That(model.Tags.Count(), Is.EqualTo(3));
			Assert.That(model.Tags, Contains.Item("tag1"));
			Assert.That(model.Tags, Contains.Item("tag2"));
			Assert.That(model.Tags, Contains.Item("tag3"));
		}

		[Test]
		public void pagecontent_constructor_should_fill_properties_and_parse_markup()
		{
			// Arrange
			PageContent content = new PageContent();
			content.Page = new Page();
			content.Page.Id = 3;
			content.Page.Title = "my title";
			content.Page.CreatedBy = "me";
			content.Page.CreatedOn = DateTime.Now;
			content.Page.IsLocked = true;
			content.Page.ModifiedBy = "me2";
			content.Page.ModifiedOn = DateTime.Now.AddDays(1);
			content.Page.Tags = "tag1,tag2,tag3";
			content.Text = "some text **in bold**";
			content.VersionNumber = 5;

			var pageHtml = new PageHtml("my html");

			TextPluginStub plugin = new TextPluginStub();
			plugin.IsCacheable = false;
			plugin.HeadContent = "head content";
			plugin.FooterContent = "footer content";
			plugin.PreContainerHtml = "pre container";
			plugin.PostContainerHtml = "post container";
			plugin.PluginCache = new SiteCache(CacheMock.RoadkillCache);
			plugin.SettingsRepository = _settingsRepository;
			plugin.Settings.IsEnabled = true;
			_pluginFactory.RegisterTextPlugin(plugin);

			// Act
			PageViewModel model = new PageViewModel(content, pageHtml);

			// Assert
			Assert.That(model.Id, Is.EqualTo(content.Page.Id));
			Assert.That(model.Title, Is.EqualTo(content.Page.Title));
			Assert.That(model.CreatedBy, Is.EqualTo(content.Page.CreatedBy));
			Assert.That(model.ModifiedBy, Is.EqualTo(content.Page.ModifiedBy));
			Assert.That(model.VersionNumber, Is.EqualTo(content.VersionNumber));
			Assert.That(model.Content, Is.EqualTo(content.Text));

			Assert.That(model.CreatedOn, Is.EqualTo(content.Page.CreatedOn));
			Assert.That(model.CreatedOn.Kind, Is.EqualTo(DateTimeKind.Utc));
			Assert.That(model.ModifiedOn, Is.EqualTo(content.Page.ModifiedOn));
			Assert.That(model.ModifiedOn.Kind, Is.EqualTo(DateTimeKind.Utc));

			Assert.That(model.Tags.Count(), Is.EqualTo(3));
			Assert.That(model.Tags, Contains.Item("tag1"));
			Assert.That(model.Tags, Contains.Item("tag2"));
			Assert.That(model.Tags, Contains.Item("tag3"));

			// (this extra html is from the plugin)
			Assert.That(model.ContentAsHtml, Is.EqualTo("<p>some text <strong style='color:green'><iframe src='javascript:alert(test)'>in bold</strong></p>\n"), model.ContentAsHtml);
			
			Assert.That(model.IsCacheable, Is.EqualTo(plugin.IsCacheable));
			Assert.That(model.PluginHeadHtml, Is.EqualTo(plugin.HeadContent));
			Assert.That(model.PluginFooterHtml, Is.EqualTo(plugin.FooterContent));
			Assert.That(model.PluginPreContainer, Is.EqualTo(plugin.PreContainerHtml));
			Assert.That(model.PluginPostContainer, Is.EqualTo(plugin.PostContainerHtml));
		}

		[Test]
		public void content_should_be_empty_and_not_null_when_set_to_null()
		{
			// Arrange
			PageViewModel model = new PageViewModel();			

			// Act
			model.Content = null;

			// Assert
			Assert.That(model.Content, Is.EqualTo(string.Empty));
		}

		[Test]
		public void isnew_should_be_true_when_id_is_not_set()
		{
			// Arrange
			PageViewModel model = new PageViewModel();

			// Act
			model.Id = 0;
			
			// Assert
			Assert.That(model.IsNew, Is.True);
		}

		[Test]
		public void rawtags_should_be_csv_parsed_when_set()
		{
			// Arrange
			PageViewModel model = new PageViewModel();

			// Act
			model.RawTags = "tag1, tag2, tag3";

			// Assert
			Assert.That(model.Tags.Count(), Is.EqualTo(3));
			Assert.That(model.Tags, Contains.Item("tag1"));
			Assert.That(model.Tags, Contains.Item("tag2"));
			Assert.That(model.Tags, Contains.Item("tag3"));
		}

		[Test]
		public void commadelimitedtags_should_return_tags_in_csv_form()
		{
			// Arrange
			PageViewModel model = new PageViewModel();
			model.RawTags = "tag1, tag2, tag3";

			// Act
			string joinedTags = model.CommaDelimitedTags();

			// Assert
			Assert.That(joinedTags, Is.EqualTo("tag1,tag2,tag3"));
		}

		[Test]
		public void spacedelimitedtags_should_return_tags_space_separated()
		{
			// Arrange
			PageViewModel model = new PageViewModel();
			model.RawTags = "tag1, tag2, tag3";

			// Act
			string joinedTags = model.SpaceDelimitedTags();

			// Assert
			Assert.That(joinedTags, Is.EqualTo("tag1 tag2 tag3"));
		}

		[Test]
		public void parsetags_should_remove_trailing_whitespace_and_empty_elements()
		{
			// Arrange + Act
			IEnumerable<string> tags = PageViewModel.ParseTags("tag1, tag2, ,,    tag3      ,tag4");

			// Assert
			Assert.That(tags.Count(), Is.EqualTo(4));
			Assert.That(tags, Contains.Item("tag1"));
			Assert.That(tags, Contains.Item("tag2"));
			Assert.That(tags, Contains.Item("tag3"));
			Assert.That(tags, Contains.Item("tag4"));
		}

		[Test]
		public void javascriptarrayforalltags_should_return_valid_javascript_array()
		{
			// Arrange
			PageViewModel model = new PageViewModel();
			model.AllTags.Add(new TagViewModel("tag1"));
			model.AllTags.Add(new TagViewModel("tag2"));
			model.AllTags.Add(new TagViewModel("tag3"));

			string expectedJavascript = "\"tag1\", \"tag2\", \"tag3\"";

			// Act
			string actualJavascript = model.JavascriptArrayForAllTags();

			// Assert
			Assert.That(actualJavascript, Is.EqualTo(expectedJavascript));
		}

		[Test]
		public void EncodePageTitle_todo()
		{
			Assert.Fail("todo");
		}
	}
}
